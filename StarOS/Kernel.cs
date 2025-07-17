using System;
using Cosmos.Core.Memory;
using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;
using StarOS.Resources;
using Sys = Cosmos.System;

namespace StarOS
{
    public class Kernel : Sys.Kernel
    {
        private bool isBooted = false;
        private bool RunGUI = false;
        private int lastHeapCollect = 0;

        private Canvas canvas;

        private Bitmap cursor;
        private int mouseX;
        private int mouseY;

        private Button[] buttons;

        protected override void BeforeRun()
        {
            canvas = FullScreenCanvas.GetFullScreenCanvas();
            cursor = new Bitmap(Files.StarOSCursorRaw);

            // Ustawienia myszy (ważne, by mysz działała poprawnie)
            MouseManager.ScreenWidth = canvas.Mode.Width;
            MouseManager.ScreenHeight = canvas.Mode.Height;
            MouseManager.X = canvas.Mode.Width / 2;
            MouseManager.Y = canvas.Mode.Height / 2;

            mouseX = (int)MouseManager.X;
            mouseY = (int)MouseManager.Y;

            buttons = new Button[]
            {
                new Button("bBoot Version", 100, 100),
                new Button("Start GUI", 100, 160),
                new Button("Shutdown", 100, 340)
            };

            ShowBootloaderCanvas();
        }

        private void ShowBootloaderCanvas()
        {
            Bitmap wallpaper = new Bitmap(Files.StarOSBackgroundRaw);
            bool selected = false;

            while (!selected)
            {
                // Pozycja myszy jest automatycznie aktualizowana przez Cosmos

                mouseX = (int)MouseManager.X;
                mouseY = (int)MouseManager.Y;

                // Ogranicz pozycję kursora do ekranu
                if (mouseX < 0) mouseX = 0;
                if (mouseY < 0) mouseY = 0;
                if (mouseX > (int)canvas.Mode.Width - 1) mouseX = (int)canvas.Mode.Width - 1;
                if (mouseY > (int)canvas.Mode.Height - 1) mouseY = (int)canvas.Mode.Height - 1;

                // Rysuj tło i przyciski
                canvas.DrawImage(wallpaper, 0, 0);

                foreach (var button in buttons)
                {
                    button.Draw(canvas, mouseX, mouseY);
                    if (button.IsHovered(mouseX, mouseY) && MouseManager.MouseState == MouseState.Left)
                    {
                        switch (button.Text)
                        {
                            case "bBoot Version":
                                DrawMessage("bBoot Version 1.3.3 ");
                                break;
                            case "Start GUI":
                                Boot.OnBoot();
                                isBooted = true;
                                RunGUI = true;
                                selected = true;
                                break;
                            case "Shutdown":
                                Sys.Power.Shutdown();
                                break;
                        }
                    }
                }

                // Rysuj kursor myszy
                canvas.DrawImageAlpha(cursor, mouseX, mouseY);

                // Wyświetl wszystko
                canvas.Display();

                // Odczyt klawiatury - czyść bufor aby nie zawiesić systemu
                while (KeyboardManager.TryReadKey(out var key))
                {
                    // Nic nie rób, po prostu czytaj żeby odświeżyć stan
                }
            }
        }

        private void DrawMessage(string msg)
        {
            canvas.DrawFilledRectangle(Color.Black, 0, 400, (int)canvas.Mode.Width, 20);
            canvas.DrawString(msg, Sys.Graphics.Fonts.PCScreenFont.Default, Color.White, 100, 400);
        }

        protected override void Run()
        {
            if (!isBooted)
                return;

            if (!RunGUI)
            {
                // Usunięto obsługę terminala
                isBooted = false;
                ShowBootloaderCanvas();
            }
            else
            {
                Gui.Update();
            }

            if (lastHeapCollect >= 20)
            {
                Heap.Collect();
                lastHeapCollect = 0;
            }
            else
            {
                lastHeapCollect++;
            }
        }

        private class Button
        {
            public string Text;
            public int X, Y, Width = 200, Height = 40;

            public Button(string text, int x, int y)
            {
                Text = text;
                X = x;
                Y = y;
            }

            public void Draw(Canvas canvas, int mouseX, int mouseY)
            {
                var bg = IsHovered(mouseX, mouseY) ? Color.Gray : Color.DarkSlateGray;

                canvas.DrawFilledRectangle(bg, X, Y, Width, Height);
                canvas.DrawRectangle(Color.White, X, Y, Width, Height);
                canvas.DrawString(Text, Sys.Graphics.Fonts.PCScreenFont.Default, Color.White, X + 10, Y + 12);
            }

            public bool IsHovered(int mx, int my)
            {
                return mx >= X && mx <= X + Width && my >= Y && my <= Y + Height;
            }
        }
    }
}
