using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace StarOS
{
    public class Viewer3D
    {
        public bool IsOpen = false;
        private bool IsMaximized = false;
        private bool IsMinimized = false;
        private bool IsClosed = false;
        private int winX = 560, winY = 240, winW = 800, winH = 600;
        private float angleY = 0f, angleX = 0f;
        private Color shapeColor = Color.FromArgb(95, 158, 160);
        private Color backgroundColor = Color.FromArgb(0, 0, 0);
        private int frameCount = 0;
        private DateTime lastFrameTime = DateTime.Now;

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public void Minimize()
        {
            IsMinimized = true;
        }

        public void Maximize()
        {
            IsMaximized = true;
            winX = 0;
            winY = 0;
            winW = 1920; // Full screen width for example
            winH = 1080; // Full screen height for example
        }

        public void Restore()
        {
            IsMaximized = false;
            IsMinimized = false;
            winX = 560;
            winY = 240;
            winW = 800;
            winH = 600;
        }

        public void Close()
        {
            IsClosed = true;
            IsOpen = false;
        }

        public void ChangeShapeColor(Color color)
        {
            shapeColor = color;
        }

        public void ChangeBackgroundColor(Color color)
        {
            backgroundColor = color;
        }

        public void Draw(SVGAIICanvas canvas)
        {
            if (!IsOpen) return;

            if (IsMinimized || IsClosed)
            {
                return; // Do nothing if minimized or closed
            }

            // Clear the window with background color
            canvas.DrawFilledRectangle(backgroundColor, winX, winY, winW, winH);

            // Draw FPS
            DrawFPS(canvas);

            // Draw window buttons (close, minimize, maximize)
            DrawWindowButtons(canvas);

            int cx = winX + winW / 2;
            int cy = winY + winH / 2;

            float size = 150f;
            float fov = 400f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(-size, -size, -size),
                new Vector3(size, -size, -size),
                new Vector3(size, size, -size),
                new Vector3(-size, size, -size),
                new Vector3(-size, -size, size),
                new Vector3(size, -size, size),
                new Vector3(size, size, size),
                new Vector3(-size, size, size)
            };

            Matrix3 rotY = Matrix3.RotationY(angleY);
            Matrix3 rotX = Matrix3.RotationX(angleX);

            Point[] screen = new Point[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 rotated = rotY * verts[i];
                rotated = rotX * rotated;

                float z = rotated.Z + fov;
                if (z == 0) z = 0.01f;
                float scale = fov / z;

                int x = (int)(rotated.X * scale + cx);
                int y = (int)(-rotated.Y * scale + cy);
                screen[i] = new Point(x, y);
            }

            // Draw cube edges
            DrawLine(canvas, screen[0], screen[1]);
            DrawLine(canvas, screen[1], screen[2]);
            DrawLine(canvas, screen[2], screen[3]);
            DrawLine(canvas, screen[3], screen[0]);

            DrawLine(canvas, screen[4], screen[5]);
            DrawLine(canvas, screen[5], screen[6]);
            DrawLine(canvas, screen[6], screen[7]);
            DrawLine(canvas, screen[7], screen[4]);

            DrawLine(canvas, screen[0], screen[4]);
            DrawLine(canvas, screen[1], screen[5]);
            DrawLine(canvas, screen[2], screen[6]);
            DrawLine(canvas, screen[3], screen[7]);

            angleY += 0.03f;
            angleX += 0.02f;

            frameCount++;
        }

        // FPS Display
        private void DrawFPS(SVGAIICanvas canvas)
        {
            if ((DateTime.Now - lastFrameTime).TotalSeconds >= 1)
            {
                float fps = frameCount;
                frameCount = 0;
                lastFrameTime = DateTime.Now;

                string fpsText = $"FPS: {fps}";
                DrawText(canvas, 10, 10, fpsText, Color.White); // Draw FPS text manually
            }
        }

        // Manual text drawing (a basic example for letters, not scalable)
        private void DrawText(SVGAIICanvas canvas, int x, int y, string text, Color color)
        {
            foreach (char c in text)
            {
                // Simple example for letter 'F'
                if (c == 'F')
                {
                    canvas.DrawPoint(color, x, y);
                    canvas.DrawPoint(color, x + 1, y);
                    canvas.DrawPoint(color, x + 2, y);
                    canvas.DrawPoint(color, x, y + 1);
                    canvas.DrawPoint(color, x, y + 2);
                    x += 4;
                }
                // Handle more characters...
            }
        }

        // Draw window control buttons (Close, Minimize, Maximize)
        private void DrawWindowButtons(SVGAIICanvas canvas)
        {
            int buttonSize = 20;

            // Close button (red)
            DrawButton(canvas, winX + winW - buttonSize * 3, winY, buttonSize, Color.Red);
            // Minimize button (yellow)
            DrawButton(canvas, winX + winW - buttonSize * 2, winY, buttonSize, Color.Yellow);
            // Maximize button (green)
            DrawButton(canvas, winX + winW - buttonSize, winY, buttonSize, Color.Green);
        }

        private void DrawButton(SVGAIICanvas canvas, int x, int y, int size, Color color)
        {
            canvas.DrawFilledRectangle(color, x, y, size, size);
        }

        // Helper Structures
        private struct Vector3
        {
            public float X, Y, Z;
            public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        }

        private struct Matrix3
        {
            public float M11, M12, M13;
            public float M21, M22, M23;
            public float M31, M32, M33;

            public static Matrix3 RotationY(float angle)
            {
                float c = (float)Math.Cos(angle);
                float s = (float)Math.Sin(angle);
                return new Matrix3
                {
                    M11 = c,
                    M12 = 0,
                    M13 = s,
                    M21 = 0,
                    M22 = 1,
                    M23 = 0,
                    M31 = -s,
                    M32 = 0,
                    M33 = c
                };
            }

            public static Matrix3 RotationX(float angle)
            {
                float c = (float)Math.Cos(angle);
                float s = (float)Math.Sin(angle);
                return new Matrix3
                {
                    M11 = 1,
                    M12 = 0,
                    M13 = 0,
                    M21 = 0,
                    M22 = c,
                    M23 = -s,
                    M31 = 0,
                    M32 = s,
                    M33 = c
                };
            }

            public static Vector3 operator *(Matrix3 m, Vector3 v)
            {
                float x = m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z;
                float y = m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z;
                float z = m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z;
                return new Vector3(x, y, z);
            }
        }

        private struct Point
        {
            public int X, Y;
            public Point(int x, int y) { X = x; Y = y; }
        }

        // Drawing line using Bresenham's algorithm
        private void DrawLine(SVGAIICanvas canvas, Point p1, Point p2)
        {
            int dx = Math.Abs(p2.X - p1.X);
            int dy = Math.Abs(p2.Y - p1.Y);
            int sx = p1.X < p2.X ? 1 : -1;
            int sy = p1.Y < p2.Y ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                canvas.DrawPoint(shapeColor, p1.X, p1.Y);
                if (p1.X == p2.X && p1.Y == p2.Y) break;
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    p1.X += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    p1.Y += sy;
                }
            }
        }
    }
}
