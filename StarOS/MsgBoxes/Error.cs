using System;
using System.Drawing;
using Cosmos.System;
using Cosmos.System.Graphics;
using Sys = Cosmos.System;

namespace StarOS.MsgBoxes
{
    public class ErrorWindow
    {
        private SVGAIICanvas canvas;
        private string title;
        private string message;

        private int x, y, width, height;
        private Rectangle closeButton;
        private bool isOpen = true;

        public bool IsOpen => isOpen;

        public ErrorWindow(SVGAIICanvas canvas, string title, string message)
        {
            this.canvas = canvas;
            this.title = title;
            this.message = message;

            width = 400;
            height = 200;
            x = (int)(canvas.Mode.Width / 2 - width / 2);
            y = (int)(canvas.Mode.Height / 2 - height / 2);

            closeButton = new Rectangle(x + width - 30, y + 5, 25, 25);
        }

        public void Draw()
        {
            if (!isOpen) return;

            // Okno
            Gui.DrawRoundedWindow(x, y, width, height, 10, Color.DarkRed, Color.Black);

            // Tytuł
            canvas.DrawString(title, Sys.Graphics.Fonts.PCScreenFont.Default, Color.White, x + 10, y + 10);

            // Treść
            canvas.DrawString(message, Sys.Graphics.Fonts.PCScreenFont.Default, Color.White, x + 10, y + 50);

            // Przycisk zamknięcia (czerwony X)
            canvas.DrawFilledRectangle(Color.Red, closeButton.X, closeButton.Y, closeButton.Width, closeButton.Height);
            canvas.DrawString("X", Sys.Graphics.Fonts.PCScreenFont.Default, Color.White, closeButton.X + 7, closeButton.Y + 5);

            HandleClick();
        }

        private void HandleClick()
        {
            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;
            bool isLeftPressed = MouseManager.MouseState == MouseState.Left;

            if (isLeftPressed && closeButton.Contains(mouseX, mouseY))
            {
                isOpen = false;
            }
        }
    }
}
