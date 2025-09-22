using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Drawing;
using Sys = Cosmos.System;
using System.IO;

namespace StarOS
{
    public class LoginWindow
    {
        private Canvas canvas;
        private Bitmap cursor;
        private int mouseX, mouseY;

        private string username = "";
        private string password = "";
        private bool hidePassword = true;

        private bool usernameActive = true; // focus na polu username
        private bool loginFailed = false;
        private int attempts = 0;
        private const int maxAttempts = 3;

        public bool Authenticated { get; private set; } = false;

        public LoginWindow(Canvas canvas, Bitmap cursor)
        {
            this.canvas = canvas;
            this.cursor = cursor;
            MouseManager.ScreenWidth = canvas.Mode.Width;
            MouseManager.ScreenHeight = canvas.Mode.Height;
            MouseManager.X = canvas.Mode.Width / 2;
            MouseManager.Y = canvas.Mode.Height / 2;
        }

        public void Update()
        {
            mouseX = (int)MouseManager.X;
            mouseY = (int)MouseManager.Y;

            DrawWindow();
            HandleInput();

            canvas.DrawImageAlpha(cursor, mouseX, mouseY);
            canvas.Display();
        }

        private void DrawWindow()
        {
            // tło
            canvas.DrawFilledRectangle(Color.DarkSlateGray, 0, 0, (int)canvas.Mode.Width, (int)canvas.Mode.Height);

            // okno logowania
            int winW = 400, winH = 200;
            int winX = ((int)canvas.Mode.Width - winW) / 2;
            int winY = ((int)canvas.Mode.Height - winH) / 2;
            canvas.DrawFilledRectangle(Color.LightGray, winX, winY, winW, winH);
            canvas.DrawRectangle(Color.Black, winX, winY, winW, winH);

            // napisy
            canvas.DrawString("Login to StarOS", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, winX + 100, winY + 10);
            canvas.DrawString("Username:", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, winX + 50, winY + 50);
            canvas.DrawString("Password:", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, winX + 50, winY + 90);

            // pola tekstowe
            string passDisplay = hidePassword ? new string('*', password.Length) : password;

            // podświetlenie aktywnego pola
            Color userBg = usernameActive ? Color.LightYellow : Color.White;
            Color passBg = usernameActive ? Color.White : Color.LightYellow;

            canvas.DrawFilledRectangle(userBg, winX + 150, winY + 45, 200, 20);
            canvas.DrawString(username, Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, winX + 152, winY + 46);

            canvas.DrawFilledRectangle(passBg, winX + 150, winY + 85, 200, 20);
            canvas.DrawString(passDisplay, Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, winX + 152, winY + 86);

            // przycisk login
            Rectangle loginRect = new Rectangle(winX + 150, winY + 130, 80, 30);
            canvas.DrawFilledRectangle(Color.Gray, loginRect.X, loginRect.Y, loginRect.Width, loginRect.Height);
            canvas.DrawRectangle(Color.Black, loginRect.X, loginRect.Y, loginRect.Width, loginRect.Height);
            canvas.DrawString("Login", Sys.Graphics.Fonts.PCScreenFont.Default, Color.Black, loginRect.X + 10, loginRect.Y + 5);

            // komunikat o błędzie
            if (loginFailed)
            {
                canvas.DrawString("Login failed! Attempts left: " + (maxAttempts - attempts),
                    Sys.Graphics.Fonts.PCScreenFont.Default, Color.Red, winX + 50, winY + 170);
            }
        }

        private void HandleInput()
        {
            HandleMouseClick();

            while (KeyboardManager.TryReadKey(out var key))
            {
                if (usernameActive)
                {
                    if (key.Key == ConsoleKeyEx.Tab)
                        usernameActive = false;
                    else if (key.Key == ConsoleKeyEx.Backspace && username.Length > 0)
                        username = username.Substring(0, username.Length - 1);
                    else if (!char.IsControl(key.KeyChar))
                        username += key.KeyChar;
                }
                else
                {
                    if (key.Key == ConsoleKeyEx.Tab)
                        usernameActive = true;
                    else if (key.Key == ConsoleKeyEx.Backspace && password.Length > 0)
                        password = password.Substring(0, password.Length - 1);
                    else if (!char.IsControl(key.KeyChar))
                        password += key.KeyChar;
                }

                // Enter = próbuj zalogować
                if (key.Key == ConsoleKeyEx.Enter)
                {
                    AttemptLogin();
                }
            }
        }

        private void HandleMouseClick()
        {
            if (MouseManager.MouseState != MouseState.Left) return;

            int winW = 400, winH = 200;
            int winX = ((int)canvas.Mode.Width - winW) / 2;
            int winY = ((int)canvas.Mode.Height - winH) / 2;

            Rectangle userRect = new Rectangle(winX + 150, winY + 45, 200, 20);
            Rectangle passRect = new Rectangle(winX + 150, winY + 85, 200, 20);
            Rectangle loginRect = new Rectangle(winX + 150, winY + 130, 80, 30);

            int mx = (int)MouseManager.X;
            int my = (int)MouseManager.Y;

            if (userRect.Contains(mx, my))
                usernameActive = true;
            else if (passRect.Contains(mx, my))
                usernameActive = false;

            if (loginRect.Contains(mx, my))
            {
                AttemptLogin();
            }
        }

        private void AttemptLogin()
        {
            if (TryAuthenticate())
                Authenticated = true;
            else
            {
                loginFailed = true;
                attempts++;
                if (attempts >= maxAttempts)
                    Sys.Power.Reboot();
            }
        }

        private bool TryAuthenticate()
        {
            try
            {
                if (!File.Exists(@"0:\installed2.flag")) return false;

                var content = File.ReadAllLines(@"0:\installed2.flag");
                string fileUser = null;
                string filePass = null;

                foreach (var line in content)
                {
                    if (line.StartsWith("User:")) fileUser = line.Substring(5).Trim();
                    if (line.StartsWith("Password:")) filePass = line.Substring(9).Trim();
                }

                return username == fileUser && password == filePass;
            }
            catch
            {
                return false;
            }
        }
    }
}
