using System;
using System.IO;
using System.Text;
using Cosmos.System.FileSystem.VFS;

namespace StarOS.Installer
{
    internal class Installer
    {
        private static bool exitInstaller = false;
        private static bool isInstalled = false;

        private static string username = "";
        private static string password = "";

        private static int currentStep = 0;
        private static string inputBuffer = "";

        public static void Start()
        {
            exitInstaller = false;
            isInstalled = false;
            currentStep = 0;
            inputBuffer = "";

            while (!exitInstaller)
            {
                // Zamiast Console.Clear, czyścimy ekran pustymi liniami
                for (int i = 0; i < 25; i++) Console.WriteLine();

                DrawUI();
                HandleInput();
            }
        }

        private static void DrawUI()
        {
            Console.WriteLine("=== Instalator StarOS ===\n");

            if (!isInstalled)
            {
                if (currentStep == 0)
                {
                    Console.WriteLine("Wybierz opcję:");
                    Console.WriteLine($"1. Ustaw nazwę użytkownika (aktualnie: {(string.IsNullOrEmpty(username) ? "<nie ustawiono>" : username)})");
                    Console.WriteLine($"2. Ustaw hasło (aktualnie: {(string.IsNullOrEmpty(password) ? "<nie ustawiono>" : new string('*', password.Length))})");
                    Console.WriteLine("3. Rozpocznij instalację\n");
                    Console.Write("Wpisz numer opcji i naciśnij Enter: ");
                }
                else if (currentStep == 1)
                {
                    Console.Write("Podaj nazwę użytkownika: ");
                }
                else if (currentStep == 2)
                {
                    Console.Write("Podaj hasło: ");
                }
                else if (currentStep == 3)
                {
                    Console.WriteLine("Instalacja w toku...");
                    RunInstall();
                }
                else if (currentStep == 4)
                {
                    if (isInstalled)
                    {
                        Console.WriteLine("\n[+] Pomyślnie zainstalowano StarOS.");
                        Console.WriteLine("Naciśnij dowolny klawisz, aby zakończyć...");
                    }
                    else
                    {
                        Console.WriteLine("\n[-] Błąd instalacji: Nie podano nazwy użytkownika lub hasła!");
                        Console.WriteLine("Naciśnij dowolny klawisz, aby powrócić do menu...");
                    }
                }
            }
            else
            {
                Console.WriteLine("[+] Instalacja zakończona. Naciśnij dowolny klawisz, aby wyjść.");
            }
        }

        private static void HandleInput()
        {
            if (currentStep == 0)
            {
                var input = Console.ReadLine();
                if (input == "1")
                {
                    currentStep = 1;
                }
                else if (input == "2")
                {
                    currentStep = 2;
                }
                else if (input == "3")
                {
                    currentStep = 3;
                }
            }
            else if (currentStep == 1)
            {
                inputBuffer = Console.ReadLine();
                username = inputBuffer.Trim();
                inputBuffer = "";
                currentStep = 0;
            }
            else if (currentStep == 2)
            {
                inputBuffer = ReadPassword();
                password = inputBuffer.Trim();
                inputBuffer = "";
                currentStep = 0;
            }
            else if (currentStep == 4)
            {
                Console.ReadKey(true);
                currentStep = 0;
                if (isInstalled)
                    exitInstaller = true;
            }
        }

        private static void RunInstall()
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                isInstalled = false;
                currentStep = 4;
                return;
            }

            string path = @"0:\installed2.flag";
            string content = $"User:{username}\nPassword:{password}";
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            try
            {
                if (!Directory.Exists(@"0:\"))
                {
                    Console.WriteLine("Dysk 0:\\ nie jest dostępny!");
                    isInstalled = false;
                    currentStep = 4;
                    return;
                }

                var entry = VFSManager.CreateFile(path);
                if (entry == null)
                {
                    Console.WriteLine("Nie udało się utworzyć pliku: " + path);
                    isInstalled = false;
                    currentStep = 4;
                    return;
                }

                using (var stream = entry.GetFileStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }

                isInstalled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd zapisu pliku: " + ex.Message);
                isInstalled = false;
            }

            currentStep = 4;
        }



        private static string ReadPassword()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write("*");
                }
            }

            Console.WriteLine();
            return sb.ToString();
        }
    }
}
