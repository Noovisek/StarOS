using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Drawing;

namespace StarOS
{
    public static class Settings
    {
        public static bool IsOpen = false;
        private static string CurrentTab = "About"; // domyślna zakładka

        // Klikalne obszary zakładek
        private static Rectangle AboutRect = new Rectangle(10, 60, 150, 20);
        private static Rectangle ThemesRect = new Rectangle(10, 10, 150, 20);
        private static Rectangle EthernetRect = new Rectangle(10, 30, 150, 20);

        public static void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public static void Draw(SVGAIICanvas canvas)
        {
            if (!IsOpen) return;

            int sidebarWidth = 200;
            int screenHeight = (int)canvas.Mode.Height;
            int screenWidth = (int)canvas.Mode.Width;


            // Boczny panel
            canvas.DrawFilledRectangle(Color.White, 0, 0, sidebarWidth, screenHeight);
            canvas.DrawRectangle(Color.Black, 0, 0, sidebarWidth, screenHeight);

            // Zakładki
            DrawTab(canvas, "Themes", ThemesRect, CurrentTab == "Themes");
            DrawTab(canvas, "Ethernet (SOON)", EthernetRect, CurrentTab == "Ethernet");
            DrawTab(canvas, "About me", AboutRect, CurrentTab == "About");

            // Wybrana treść
            int contentX = sidebarWidth + 20;
            int contentWidth = screenWidth - sidebarWidth - 40;

            switch (CurrentTab)
            {
                case "Themes":
                    DrawThemesTab(canvas, contentX);
                    break;

                case "Ethernet":
                    DrawEthernetTab(canvas, contentX);
                    break;

                case "About":
                    DrawAboutTab(canvas, contentX, screenWidth, screenHeight);
                    break;
            }
        }

        private static void DrawTab(SVGAIICanvas canvas, string title, Rectangle rect, bool isActive)
        {
            var color = isActive ? Color.Blue : Color.Black;
            canvas.DrawString(title, PCScreenFont.Default, color, rect.X, rect.Y);
        }

        private static void DrawAboutTab(SVGAIICanvas canvas, int contentX, int screenWidth, int screenHeight)
        {
            int logoY = 50;

            // Logo StarOS z Gui.StartRaw
            if (Gui.StartRaw != null)
                canvas.DrawImage(Gui.StartRaw, contentX, logoY, 64, 64);

            int textX = contentX + 80;
            int textY = logoY + 10;

            canvas.DrawString("StarOS 1.2.0", PCScreenFont.Default, Color.DeepSkyBlue, textX, textY);
            canvas.DrawString("BBOT Version: 1.3.3", PCScreenFont.Default, Color.DeepSkyBlue, contentX, 130);
            canvas.DrawString(@"NAME NAZWA Z O:/INSTALLED2.FLAG", PCScreenFont.Default, Color.DeepSkyBlue, contentX, 150);
            canvas.DrawString(@"$""user-{USERNAME}\password-{PASSWORD}""", PCScreenFont.Default, Color.DeepSkyBlue, contentX, 170);
            canvas.DrawString("BY NOBUS", PCScreenFont.Default, Color.DeepSkyBlue, screenWidth - 150, screenHeight - 30);
        }

        private static void DrawThemesTab(SVGAIICanvas canvas, int contentX)
        {
            canvas.DrawString("Themes (coming soon...)", PCScreenFont.Default, Color.Gray, contentX, 50);
        }

        private static void DrawEthernetTab(SVGAIICanvas canvas, int contentX)
        {
            canvas.DrawString("Ethernet settings will be here.", PCScreenFont.Default, Color.Gray, contentX, 50);
        }

        public static void HandleClick()
        {
            if (!IsOpen) return;

            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;

            if (AboutRect.Contains(mx, my))
                CurrentTab = "About";
            else if (ThemesRect.Contains(mx, my))
                CurrentTab = "Themes";
            else if (EthernetRect.Contains(mx, my))
                CurrentTab = "Ethernet";
        }
    }
}
