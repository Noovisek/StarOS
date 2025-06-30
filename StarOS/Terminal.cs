using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Collections.Generic;
using System.Drawing;

namespace StarOS
{
    public class Terminal
    {
        private bool isOpen = false;
        public bool IsOpen => isOpen;

        public List<string> Lines { get; private set; } = new List<string>();
        public string CurrentInput { get; private set; } = "";

        public bool IsRunning => isOpen;

        // Terminal position and size - zgodne z Gui
        private readonly int x = 100;
        private readonly int y = 100;
        private readonly int width = 800;
        private readonly int height = 500;

        private SVGAIICanvas canvas;

        public Terminal()
        {
            // Domyślnie pusta lista, pusty input
        }

        public void InitCanvas(SVGAIICanvas canvas)
        {
            this.canvas = canvas;
        }

        public void Toggle()
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                Lines.Clear();
                CurrentInput = "";
            }
        }

        public void Reset()
        {
            isOpen = false;
            Lines.Clear();
            CurrentInput = "";
        }

        public void HandleInput(ConsoleKeyEx key, char keyChar)
        {
            if (!isOpen)
                return;

            if (key == ConsoleKeyEx.Enter)
            {
                Lines.Add("user@staros:$ " + CurrentInput);

                // Tutaj możesz dodać obsługę prostych komend
                if (!string.IsNullOrWhiteSpace(CurrentInput))
                {
                    Lines.Add("Executed: " + CurrentInput);
                }

                CurrentInput = "";
            }
            else if (key == ConsoleKeyEx.Backspace)
            {
                if (CurrentInput.Length > 0)
                    CurrentInput = CurrentInput.Substring(0, CurrentInput.Length - 1);
            }
            else if (!char.IsControl(keyChar))
            {
                CurrentInput += keyChar;
            }
        }

        public void Run()
        {
            if (!isOpen || canvas == null)
                return;

            // Rysowanie okna terminala, tła itd. (Gui robi to za Ciebie, więc tu możesz pominąć albo rozbudować)

            // Możesz tu ewentualnie dodać odświeżanie terminala, obsługę klawiatury, jeśli nie chcesz, by Gui to robiło
        }
    }
}
