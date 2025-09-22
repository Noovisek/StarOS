using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Drawing;
using System.IO;
using Sys = Cosmos.System;


namespace StarOS
{
    public static class Gui
    {
        public static int ScreenSizeX = 1920, ScreenSizeY = 1080;
        public static SVGAIICanvas MainCanvas;

        public static Bitmap Wallpaper, Cursor;
        public static Bitmap SettingsRaw;
        public static Bitmap NotepadRaw;
        public static Bitmap StartRaw;
        public static Bitmap TerminalRaw;
        public static Bitmap Viewer3DRaw;

        private static int dockHeight = 100;
        private static int iconBaseSize = 64;
        private static int iconMaxSize = 96;
        private static int iconSpacing = 20;

        private static bool showStartWindow = false;
        private static bool mouseLeftPressedLastFrame = false;

        private static bool isDraggingWindow = false;
        private static int dragOffsetX = 0, dragOffsetY = 0;

        private static Rectangle terminalWindow = new Rectangle(100, 100, 800, 500);

        public static Terminal Terminal = new Terminal();
        public static Viewer3D Viewer3DApp = new Viewer3D();
        public static Notepad NotepadApp = new Notepad();

        // Start Window
        private static Rectangle startWindowRect;
        private static string currentUsername = "User";

        public static void StartGUI()
        {
            MainCanvas = new SVGAIICanvas(new Mode((uint)ScreenSizeX, (uint)ScreenSizeY, ColorDepth.ColorDepth32));
            MouseManager.ScreenWidth = (uint)ScreenSizeX;
            MouseManager.ScreenHeight = (uint)ScreenSizeY;
            MouseManager.X = (uint)ScreenSizeX / 2;
            MouseManager.Y = (uint)ScreenSizeY / 2;

            // Odczytaj username z installed2.flag
            if (File.Exists(@"0:\installed2.flag"))
            {
                var lines = File.ReadAllLines(@"0:\installed2.flag");
                foreach (var line in lines)
                {
                    if (line.StartsWith("User:"))
                    {
                        currentUsername = line.Substring(5).Trim();
                        break;
                    }
                }
            }

            // Okno Start wyśrodkowane nad dockiem
            int winW = 300, winH = 200;
            int winX = (ScreenSizeX - winW) / 2;
            int winY = ScreenSizeY - dockHeight - winH - 10;
            startWindowRect = new Rectangle(winX, winY, winW, winH);
        }

        public static void Update()
        {
            if (Wallpaper != null)
                MainCanvas.DrawImage(Wallpaper, 0, 0);

            DrawDock();

            if (showStartWindow)
            {
                DrawStartWindow();
            }

            if (Settings.IsOpen)
                Settings.Draw(MainCanvas);

            if (Terminal.IsOpen)
                DrawTerminalWindow();

            if (Viewer3DApp.IsOpen)
                Viewer3DApp.Draw(MainCanvas);

            if (NotepadApp.IsOpen)
                NotepadApp.Draw(MainCanvas);

            if (Cursor != null)
                MainCanvas.DrawImageAlpha(Cursor, (int)MouseManager.X, (int)MouseManager.Y);

            MainCanvas.Display();

            HandleClick();
            HandleKeyboard();
        }

        private static void DrawDock()
        {
            int dockY = ScreenSizeY - dockHeight;
            Color dockColor = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
            MainCanvas.DrawFilledRectangle(dockColor, 0, dockY, ScreenSizeX, dockHeight);

            Bitmap[] icons = { StartRaw, SettingsRaw, NotepadRaw, TerminalRaw, Viewer3DRaw };
            int iconCount = icons.Length;

            int totalWidthBase = iconBaseSize * iconCount + iconSpacing * (iconCount - 1);
            int startX = (ScreenSizeX - totalWidthBase) / 2;

            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;
            int effectRadius = 150;

            for (int i = 0; i < iconCount; i++)
            {
                int baseX = startX + i * (iconBaseSize + iconSpacing);
                int iconCenterX = baseX + iconBaseSize / 2;

                int size = iconBaseSize;

                if (mouseY >= dockY)
                {
                    int dist = Math.Abs(mouseX - iconCenterX);
                    if (dist < effectRadius)
                    {
                        float factor = 1.0f + (effectRadius - dist) / (float)effectRadius * (iconMaxSize / (float)iconBaseSize - 1.0f);
                        int newSize = (int)(iconBaseSize * factor);
                        if (newSize > iconMaxSize) newSize = iconMaxSize;
                        size = newSize;
                    }
                }

                int x = baseX + (iconBaseSize - size) / 2;
                int y = dockY + dockHeight - size - 10;

                if (icons[i] != null)
                    MainCanvas.DrawImage(icons[i], x, y, size, size);
            }
        }

        private static void DrawStartWindow()
        {
            DrawRoundedWindow(startWindowRect.X, startWindowRect.Y, startWindowRect.Width, startWindowRect.Height, 10, Color.LightGray, Color.Black);

            // Username
            MainCanvas.DrawString("User: " + currentUsername, PCScreenFont.Default, Color.Black, startWindowRect.X + 20, startWindowRect.Y + 20);

            // Clock
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            MainCanvas.DrawString("Time: " + timeStr, PCScreenFont.Default, Color.Black, startWindowRect.X + 20, startWindowRect.Y + 50);

            // Power Off button
            Rectangle powerButton = new Rectangle(startWindowRect.X + 20, startWindowRect.Y + 100, 100, 30);
            MainCanvas.DrawFilledRectangle(Color.Gray, powerButton.X, powerButton.Y, powerButton.Width, powerButton.Height);
            MainCanvas.DrawRectangle(Color.Black, powerButton.X, powerButton.Y, powerButton.Width, powerButton.Height);
            MainCanvas.DrawString("Power Off", PCScreenFont.Default, Color.Black, powerButton.X + 10, powerButton.Y + 8);

            // Obsługa kliknięcia Power Off
            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;
            bool isLeftPressed = MouseManager.MouseState == MouseState.Left;

            if (isLeftPressed && !mouseLeftPressedLastFrame && powerButton.Contains(mx, my))
            {
                Sys.Power.Shutdown();
            }
        }

        private static void DrawTerminalWindow()
        {
            DrawRoundedWindow(terminalWindow.X, terminalWindow.Y, terminalWindow.Width, terminalWindow.Height, 10, Color.Black, Color.Gray);

            MainCanvas.DrawFilledCircle(Color.Red, terminalWindow.X + 10, terminalWindow.Y + 10, 6);
            MainCanvas.DrawFilledCircle(Color.Yellow, terminalWindow.X + 30, terminalWindow.Y + 10, 6);
            MainCanvas.DrawFilledCircle(Color.Green, terminalWindow.X + 50, terminalWindow.Y + 10, 6);

            int lineY = terminalWindow.Y + 30;
            Color textColor = Color.LightGreen;

            foreach (var line in Terminal.Lines)
            {
                MainCanvas.DrawString(line, PCScreenFont.Default, textColor, terminalWindow.X + 10, lineY);
                lineY += 18;
            }

            MainCanvas.DrawString("user@staros:$ " + Terminal.CurrentInput, PCScreenFont.Default, textColor, terminalWindow.X + 10, lineY);
        }

        private static void HandleClick()
        {
            bool isLeftPressed = MouseManager.MouseState == MouseState.Left;
            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;

            if (isLeftPressed && !mouseLeftPressedLastFrame)
            {
                mouseLeftPressedLastFrame = true;

                // Start icon
                int dockY = ScreenSizeY - dockHeight;
                int iconX = (ScreenSizeX - ((iconBaseSize + iconSpacing) * 5 - iconSpacing)) / 2;

                if (mouseX >= iconX && mouseX <= iconX + iconBaseSize && mouseY >= dockY && mouseY <= dockY + iconBaseSize)
                {
                    showStartWindow = !showStartWindow;
                }

                // Pozostałe ikony
                for (int i = 1; i < 5; i++)
                {
                    int x = iconX + i * (iconBaseSize + iconSpacing);
                    int y = dockY + dockHeight - iconBaseSize - 10;

                    if (mouseX >= x && mouseX <= x + iconBaseSize &&
                        mouseY >= y && mouseY <= y + iconBaseSize)
                    {
                        switch (i)
                        {
                            case 1: Settings.Toggle(); break;
                            case 2: NotepadApp.Toggle(); break;
                            case 3: Terminal.Toggle(); break;
                            case 4: Viewer3DApp.Toggle(); break;
                        }
                    }
                }

                if (Terminal.IsOpen && new Rectangle(terminalWindow.X, terminalWindow.Y, terminalWindow.Width, 20).Contains(mouseX, mouseY))
                {
                    isDraggingWindow = true;
                    dragOffsetX = mouseX - terminalWindow.X;
                    dragOffsetY = mouseY - terminalWindow.Y;
                }

                if (new Rectangle(terminalWindow.X + 10, terminalWindow.Y + 10, 12, 12).Contains(mouseX, mouseY))
                {
                    Terminal.Toggle();
                }
            }
            else if (!isLeftPressed)
            {
                mouseLeftPressedLastFrame = false;
                isDraggingWindow = false;
            }

            if (isDraggingWindow)
            {
                terminalWindow.X = mouseX - dragOffsetX;
                terminalWindow.Y = mouseY - dragOffsetY;
            }
        }

        private static void HandleKeyboard()
        {
            if (!KeyboardManager.TryReadKey(out var key)) return;
            Terminal.HandleInput(key.Key, key.KeyChar);
            NotepadApp.HandleInput(key.Key, key.KeyChar);
        }

        public static void DrawRoundedWindow(int x, int y, int width, int height, int radius, Color fill, Color border)
        {
            MainCanvas.DrawFilledRectangle(fill, x + radius, y, width - 2 * radius, height);
            MainCanvas.DrawFilledRectangle(fill, x, y + radius, width, height - 2 * radius);

            MainCanvas.DrawFilledCircle(fill, x + radius, y + radius, radius);
            MainCanvas.DrawFilledCircle(fill, x + width - radius - 1, y + radius, radius);
            MainCanvas.DrawFilledCircle(fill, x + radius, y + height - radius - 1, radius);
            MainCanvas.DrawFilledCircle(fill, x + width - radius - 1, y + height - radius - 1, radius);

            MainCanvas.DrawRectangle(border, x, y, width, height);
        }
    }
}
