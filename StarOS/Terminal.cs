using System;
using System.Collections.Generic;
using Cosmos.System;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.Graphics;
using System.Drawing;
using Cosmos.System.Graphics.Fonts;
using System.IO;

namespace StarOS
{
    public class Terminal
    {
        private bool isOpen = false;
        public bool IsOpen => isOpen;

        public List<string> Lines { get; private set; } = new List<string>();
        public string CurrentInput { get; private set; } = "";

        private string currentDirectory = @"0:\";

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
                Lines.Add($"Current directory: {currentDirectory}");
            }
        }

        private void AddAsciiArt()
        {
            Lines.AddRange(asciiStarOS);
            Lines.Add("");
        }

        public void HandleInput(ConsoleKeyEx key, char keyChar)
        {
            if (!isOpen)
                return;

            if (key == ConsoleKeyEx.Enter)
            {
                Lines.Add("user@staros:" + currentDirectory + "$ " + CurrentInput);
                if (!string.IsNullOrWhiteSpace(CurrentInput))
                {
                    ProcessCommand(CurrentInput.Trim());
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

        private void ProcessCommand(string commandLine)
        {
            var parts = commandLine.Split(new[] { ' ' }, 2);
            var command = parts[0].ToLower();
            var argument = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "help":
                    Lines.Add("Available commands:");
                    Lines.Add("help            - Show this help message");
                    Lines.Add("clear           - Clear the terminal screen");
                    Lines.Add("ls              - List files and directories");
                    Lines.Add("cd [dir]        - Change directory");
                    Lines.Add("echo [text]     - Print text");
                    Lines.Add("cat [file]      - Show file content");
                    Lines.Add("write [file]    - Write text to a file");
                    break;

                case "clear":
                    Lines.Clear();
                    AddAsciiArt();
                    Lines.Add($"Current directory: {currentDirectory}");
                    break;

                case "ls":
                    ListDirectory();
                    break;

                case "cd":
                    ChangeDirectory(argument);
                    break;

                case "echo":
                    Lines.Add(argument);
                    break;

                case "cat":
                    CatFile(argument);
                    break;

                case "write":
                    WriteFile(argument);
                    break;

                default:
                    Lines.Add("Unknown command: " + command);
                    break;
            }
        }
        private void ListDirectory()
        {
            try
            {
                var entries = VFSManager.GetDirectoryListing(currentDirectory);
                foreach (var entry in entries)
                {
                    if (entry.mEntryType == Cosmos.System.FileSystem.Listing.DirectoryEntryTypeEnum.Directory)
                    {
                        Lines.Add(entry.mName + "/");
                    }
                    else
                    {
                        Lines.Add(entry.mName);
                    }
                }
            }
            catch (Exception ex)
            {
                Lines.Add("Error listing directory: " + ex.Message);
            }
        }



        private void ChangeDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Lines.Add("Usage: cd [directory]");
                return;
            }

            string newPath;

            if (path.StartsWith(@"0:\") || path.StartsWith(@"\"))
            {
                newPath = path;
            }
            else
            {
                newPath = currentDirectory.TrimEnd('\\') + @"\" + path;
            }

            if (!newPath.EndsWith(@"\"))
                newPath += @"\";

            try
            {
                var entries = VFSManager.GetDirectoryListing(newPath);
                currentDirectory = newPath;
                Lines.Add("Changed directory to: " + currentDirectory);
            }
            catch
            {
                Lines.Add("Directory not found: " + path);
            }
        }

        private void CatFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Lines.Add("Usage: cat [file]");
                return;
            }

            string fullPath;
            if (fileName.StartsWith(@"0:\"))
                fullPath = fileName;
            else
                fullPath = currentDirectory.TrimEnd('\\') + @"\" + fileName;

            try
            {
                var entry = VFSManager.GetFile(fullPath);
                using (var stream = entry.GetFileStream())
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    string content = System.Text.Encoding.UTF8.GetString(buffer);
                    Lines.Add(content);
                }
            }
            catch (Exception ex)
            {
                Lines.Add("Error reading file: " + ex.Message);
            }
        }

        private void WriteFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Lines.Add("Usage: write [file]");
                return;
            }

            Lines.Add($"Enter text to write to {fileName}. End input with a single '.' on a line.");

            string fullPath;
            if (fileName.StartsWith(@"0:\"))
                fullPath = fileName;
            else
                fullPath = currentDirectory.TrimEnd('\\') + @"\" + fileName;

            List<string> inputLines = new List<string>();

            while (true)
            {
                string line = ReadLineFromUser();
                if (line == ".")
                    break;

                inputLines.Add(line);
            }

            string content = string.Join("\n", inputLines);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);

            try
            {
                var entry = VFSManager.CreateFile(fullPath);
                if (entry == null)
                {
                    Lines.Add("Error creating file.");
                    return;
                }
                using (var stream = entry.GetFileStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                Lines.Add("File saved.");
            }
            catch (Exception ex)
            {
                Lines.Add("Error writing file: " + ex.Message);
            }
        }

        private string ReadLineFromUser()
        {
            string line = "";
            while (true)
            {
                if (KeyboardManager.TryReadKey(out var key))
                {
                    if (key.Key == ConsoleKeyEx.Enter)
                        break;
                    else if (key.Key == ConsoleKeyEx.Backspace && line.Length > 0)
                    {
                        line = line.Substring(0, line.Length - 1);
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        line += key.KeyChar;
                    }
                }
            }
            Lines.Add(line);
            return line;
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
                if (lineY > y + height - 20)
                    break;
            }

            canvas.DrawString("user@staros:" + currentDirectory + "$ " + CurrentInput, PCScreenFont.Default, textColor, x + 10, lineY);
        }
    }
}
