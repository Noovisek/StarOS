using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;

namespace StarOS.Games
{
    public class SnakeGameWindow
    {
        private bool isOpen = true;
        private int x = 200, y = 200, width = 400, height = 300;
        private int headerHeight = 30;
        private SVGAIICanvas canvas;

        private List<Point> snake = new List<Point>();
        private Point food;
        private int dirX = 1, dirY = 0;
        private int blockSize = 10;
        private int speed = 150; // ms per update

        public bool IsOpen => isOpen;

        public SnakeGameWindow(SVGAIICanvas canvas)
        {
            this.canvas = canvas;
            snake.Add(new Point(5, 5));
            SpawnFood();
        }

        private void SpawnFood()
        {
            Random rnd = new Random();
            food = new Point(rnd.Next(0, width / blockSize), rnd.Next(0, (height - headerHeight) / blockSize));
        }

        public void HandleInput()
        {
            if (KeyboardManager.TryReadKey(out var key))
            {
                switch (key.Key)
                {
                    case ConsoleKeyEx.UpArrow: if (dirY == 0) { dirX = 0; dirY = -1; } break;
                    case ConsoleKeyEx.DownArrow: if (dirY == 0) { dirX = 0; dirY = 1; } break;
                    case ConsoleKeyEx.LeftArrow: if (dirX == 0) { dirX = -1; dirY = 0; } break;
                    case ConsoleKeyEx.RightArrow: if (dirX == 0) { dirX = 1; dirY = 0; } break;
                    case ConsoleKeyEx.Escape: isOpen = false; break;
                }
            }
        }

        public void Update()
        {
            Point head = snake[snake.Count - 1];
            Point newHead = new Point(head.X + dirX, head.Y + dirY);

            // Kolizje
            if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= width / blockSize || newHead.Y >= (height - headerHeight) / blockSize)
            {
                isOpen = false;
                return;
            }

            foreach (var p in snake)
                if (p == newHead) { isOpen = false; return; }

            if (newHead == food)
            {
                snake.Add(newHead);
                SpawnFood();
            }
            else
            {
                snake.RemoveAt(0);
                snake.Add(newHead);
            }
        }

        public void Draw()
        {
            if (!isOpen) return;

            // Okno
            Gui.DrawRoundedWindow(x, y, width, height, 10, Color.Black, Color.Gray);

            // Nagłówek
            canvas.DrawString("Snake Game - ESC to exit", PCScreenFont.Default, Color.White, x + 5, y + 5);

            // Wąż
            foreach (var p in snake)
                canvas.DrawFilledRectangle(Color.Green, x + p.X * blockSize, y + headerHeight + p.Y * blockSize, blockSize, blockSize);

            // Jedzenie
            canvas.DrawFilledRectangle(Color.Red, x + food.X * blockSize, y + headerHeight + food.Y * blockSize, blockSize, blockSize);
        }
    }
}
