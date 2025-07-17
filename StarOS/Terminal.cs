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

        private readonly int x = 100;
        private readonly int y = 100;
        private readonly int width = 800;
        private readonly int height = 500;

        private readonly string[] asciiStarOS = new string[]
        {
    @"   _____ __             ____  _____",
    @"  / ___// /_____ ______/ __ \/ ___/",
    @"  \__ \/ __/ __ `/ ___/ / / /\__ \ ",
    @" ___/ / /_/ /_/ / /  / /_/ /___/ / ",
    @"/____/\__/\__,_/_/   \____/A/____/  ",
    @"                                    "
        };

        public Terminal()
        {
            // Domyślnie pusta lista, pusty input
        }

        public void Toggle()
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                Lines.Clear();
                CurrentInput = "";
                AddAsciiArt();
                Lines.Add("Type 'help' to see available commands.");
            }
        }

        private void AddAsciiArt()
        {
            Lines.AddRange(asciiStarOS);
            Lines.Add(""); // Pusta linia po ASCII
        }

        public void HandleInput(ConsoleKeyEx key, char keyChar)
        {
            if (!isOpen)
                return;

            if (key == ConsoleKeyEx.Enter)
            {
                Lines.Add("user@staros:$ " + CurrentInput);
                if (!string.IsNullOrWhiteSpace(CurrentInput))
                {
                    ProcessCommand(CurrentInput.Trim().ToLower());
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

        private void ProcessCommand(string command)
        {
            switch (command)
            {
                case "help":
                    Lines.Add("Available commands:");
                    Lines.Add("help   - Show this help message");
                    Lines.Add("clear  - Clear the terminal screen");
                    Lines.Add("date   - Show current date and time");
                    Lines.Add("echo   - Repeat what you type (usage: echo your_text)");
                    break;

                case "clear":
                    Lines.Clear();
                    AddAsciiArt();
                    break;

                case "date":
                    Lines.Add("Current date and time: " + System.DateTime.Now.ToString());
                    break;

                default:
                    if (command.StartsWith("echo "))
                    {
                        Lines.Add(command.Substring(5)); // echo zwraca tekst po 'echo '
                    }
                    else
                    {
                        Lines.Add("Unknown command: " + command);
                    }
                    break;
            }
        }

        public void Draw(SVGAIICanvas canvas)
        {
            if (!isOpen)
                return;

            Color fill = Color.Black;
            Color border = Color.Gray;
            int radius = 10;

            Gui.DrawRoundedWindow(x, y, width, height, radius, fill, border);

            int lineY = y + 20;
            Color textColor = Color.LightGreen;

            foreach (var line in Lines)
            {
                canvas.DrawString(line, PCScreenFont.Default, textColor, x + 10, lineY);
                lineY += 18;
            }

            canvas.DrawString("user@staros:$ " + CurrentInput, PCScreenFont.Default, textColor, x + 10, lineY);
        }
    }
}
