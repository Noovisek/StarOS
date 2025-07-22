using Cosmos.System.Graphics;
using System;
using System.Drawing;

namespace StarOS
{
    public class Viewer3D
    {
        public bool IsOpen = false;
        private float angleY = 0f;

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public void Draw(SVGAIICanvas canvas)
        {
            if (!IsOpen) return;

            int winX = 560, winY = 240;
            int winW = 800, winH = 600;
            canvas.DrawFilledRectangle(Color.FromArgb(0, 0, 0), winX, winY, winW, winH);

            int cx = winX + winW / 2;
            int cy = winY + winH / 2;

            float size = 150f;
            float fov = 400f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(0, size, 0),
                new Vector3(-size, -size, size),
                new Vector3(size, -size, size)
            };

            Matrix3 rotY = Matrix3.RotationY(angleY);

            Point[] screen = new Point[3];
            for (int i = 0; i < 3; i++)
            {
                Vector3 rotated = rotY * verts[i];
                float z = rotated.Z + fov;
                if (z == 0) z = 0.01f;
                float scale = fov / z;

                int x = (int)(rotated.X * scale + cx);
                int y = (int)(-rotated.Y * scale + cy);
                screen[i] = new Point(x, y);
            }

            DrawFilledTriangle(canvas, screen[0], screen[1], screen[2], Color.FromArgb(95, 158, 160));

            angleY += 0.03f;
        }

        // ----------------- Struktury pomocnicze ------------------

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

        // ----------------- Rysowanie trójkąta ------------------

        private void DrawFilledTriangle(SVGAIICanvas canvas, Point p1, Point p2, Point p3, Color color)
        {
            if (p2.Y < p1.Y) (p1, p2) = (p2, p1);
            if (p3.Y < p1.Y) (p1, p3) = (p3, p1);
            if (p3.Y < p2.Y) (p2, p3) = (p3, p2);

            void DrawScanline(int y, int x1, int x2)
            {
                if (x1 > x2) (x1, x2) = (x2, x1);
                for (int x = x1; x <= x2; x++)
                    canvas.DrawPoint(color, x, y);
            }

            int Interpolate(int y0, int x0, int y1, int x1, int y)
            {
                if (y1 == y0) return x0;
                return x0 + (x1 - x0) * (y - y0) / (y1 - y0);
            }

            for (int y = p1.Y; y <= p2.Y; y++)
            {
                int xa = Interpolate(p1.Y, p1.X, p2.Y, p2.X, y);
                int xb = Interpolate(p1.Y, p1.X, p3.Y, p3.X, y);
                DrawScanline(y, xa, xb);
            }

            for (int y = p2.Y; y <= p3.Y; y++)
            {
                int xa = Interpolate(p2.Y, p2.X, p3.Y, p3.X, y);
                int xb = Interpolate(p1.Y, p1.X, p3.Y, p3.X, y);
                DrawScanline(y, xa, xb);
            }
        }
    }
}
    