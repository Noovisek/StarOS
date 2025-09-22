using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace StarOS
{
    public class Notepad
    {
        public bool IsOpen { get; private set; } = false;

        private List<string> Lines = new List<string>();
        private string CurrentInput = "";
        private int cursorLine = 0;

        private int x = 150, y = 150, width = 600, height = 400;
        private int lineHeight = 18;
        private int maxVisibleLines => height / lineHeight - 4;

        private string currentFilePath = "";

        // Przyciski GUI
        private Rectangle btnNew = new Rectangle(10, 10, 60, 20);
        private Rectangle btnOpen = new Rectangle(80, 10, 60, 20);
        private Rectangle btnSave = new Rectangle(150, 10, 60, 20);

        public void Toggle()
        {
            IsOpen = !IsOpen;
            if (!IsOpen)
            {
                Lines.Clear();
                CurrentInput = "";
                cursorLine = 0;
                currentFilePath = "";
            }
        }

        public void HandleInput(ConsoleKeyEx key, char keyChar)
        {
            if (!IsOpen) return;

            if (key == ConsoleKeyEx.Enter)
            {
                Lines.Add(CurrentInput);
                CurrentInput = "";
                cursorLine++;
            }
            else if (key == ConsoleKeyEx.Backspace)
            {
                if (CurrentInput.Length > 0)
                    CurrentInput = CurrentInput.Substring(0, CurrentInput.Length - 1);
                else if (Lines.Count > 0)
                {
                    cursorLine--;
                    CurrentInput = Lines[cursorLine];
                    Lines.RemoveAt(cursorLine);
                }
            }
            else if (!char.IsControl(keyChar))
            {
                CurrentInput += keyChar;
            }
        }

        public void Draw(SVGAIICanvas canvas)
        {
            if (!IsOpen) return;

            // Rysowanie okna
            Gui.DrawRoundedWindow(x, y, width, height, 10, Color.White, Color.Gray);

            // Rysowanie przycisków
            DrawButton(canvas, btnNew, "Nowy");
            DrawButton(canvas, btnOpen, "Otwórz");
            DrawButton(canvas, btnSave, "Zapisz");

            int startLine = cursorLine - maxVisibleLines + 1;
            if (startLine < 0) startLine = 0;

            int drawY = y + 40;
            Color textColor = Color.Black;

            for (int i = startLine; i < Lines.Count; i++)
            {
                canvas.DrawString(Lines[i], PCScreenFont.Default, textColor, x + 10, drawY);
                drawY += lineHeight;
            }

            canvas.DrawString(CurrentInput, PCScreenFont.Default, textColor, x + 10, drawY);

            HandleMouseClick();
        }

        private void DrawButton(SVGAIICanvas canvas, Rectangle rect, string text)
        {
            canvas.DrawFilledRectangle(Color.LightGray, x + rect.X, y + rect.Y, rect.Width, rect.Height);
            canvas.DrawRectangle(Color.Black, x + rect.X, y + rect.Y, rect.Width, rect.Height);
            canvas.DrawString(text, PCScreenFont.Default, Color.Black, x + rect.X + 5, y + rect.Y + 4);
        }

        private void HandleMouseClick()
        {
            int mouseX = (int)MouseManager.X;
            int mouseY = (int)MouseManager.Y;
            bool leftPressed = MouseManager.MouseState == MouseState.Left;

            if (!leftPressed) return;

            if (IsInside(mouseX, mouseY, btnNew)) { NewFile(); }
            else if (IsInside(mouseX, mouseY, btnOpen)) { OpenFile(); }
            else if (IsInside(mouseX, mouseY, btnSave)) { SaveFile(); }
        }

        private bool IsInside(int mx, int my, Rectangle rect)
        {
            return mx >= x + rect.X && mx <= x + rect.X + rect.Width &&
                   my >= y + rect.Y && my <= y + rect.Y + rect.Height;
        }

        private void NewFile()
        {
            Lines.Clear();
            CurrentInput = "";
            cursorLine = 0;
            currentFilePath = "";
        }

        private void OpenFile()
        {
            string path = @"0:\myfile.txt"; // Możesz zmienić na dowolną ścieżkę
            if (File.Exists(path))
            {
                Lines.Clear();
                Lines.AddRange(File.ReadAllLines(path));
                cursorLine = Lines.Count;
                CurrentInput = "";
                currentFilePath = path;
            }
            else
            {
                Lines.Add("Nie znaleziono pliku: " + path);
            }
        }

        public void SaveFile()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(CurrentInput))
                    Lines.Add(CurrentInput);

                string path = string.IsNullOrWhiteSpace(currentFilePath) ? @"0:\myfile.txt" : currentFilePath;
                File.WriteAllLines(path, Lines);
                currentFilePath = path;
            }
            catch (Exception ex)
            {
                Lines.Add("Błąd zapisu: " + ex.Message);
            }
        }
    }
}
