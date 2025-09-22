    using System;
    using System.Collections.Generic;
    using Cosmos.System;
    using Cosmos.System.FileSystem.VFS;
    using Cosmos.System.FileSystem.Listing;
    using Cosmos.System.Graphics;
    using System.Drawing;
    using Cosmos.System.Graphics.Fonts;
    using System.IO;
    using Sys = Cosmos.System;
    using StarOS.Games;

    namespace StarOS
    {
        public class Terminal
        {
            private bool isOpen = false;
            public bool IsOpen => isOpen;
            private string username = "user"; // domyślnie


            private readonly int x = 100;
            private readonly int y = 100;
            private readonly int width = 800;
            private readonly int height = 500;

            private readonly int maxVisibleLines = 25; // ile linii mieści terminal
            public List<string> Lines { get; private set; } = new List<string>();
            public string CurrentInput { get; private set; } = "";

            private string currentDirectory = @"0:\";

            private readonly string[] asciiStarOS = new string[]
            {
                @"   _____ __             ____  _____",
                @"  / ___// /_____ ______/ __ \/ ___/",
                @"  \__ \/ __/ __ `/ ___/ / / /\__ \ ",
                @" ___/ / /_/ /_/ / /  / /_/ /___/ / ",
                @"/____/\__/\__,_/_/   \____/A/____/  ",
                @"                                    " 
            };

            // --- zmienne do resetu ---
            private bool waitingForResetPassword = false;
            private bool waitingForResetConfirm = false;
            private string resetPasswordInput = "";

            public Terminal() { }

            public void Toggle()
            {
                isOpen = !isOpen;
                if (isOpen)
                {
                    Lines.Clear();
                    CurrentInput = "";
                    username = GetInstalledUsername(); // <-- pobiera nazwę usera
                    AddAsciiArt();
                    AddLine("Type 'help' to see available commands.");
                    AddLine($"Current directory: {currentDirectory}");
                }
            }


            private void AddAsciiArt()
            {
                foreach (var line in asciiStarOS)
                    AddLine(line);
                AddLine("");
            }

            private void AddLine(string text)
            {
                Lines.Add(text);
                if (Lines.Count > maxVisibleLines)
                    Lines.RemoveAt(0);
            }

            private string GetInstalledUsername()
            {
                if (!File.Exists(@"0:\installed2.flag")) return "user";
                var content = File.ReadAllLines(@"0:\installed2.flag");
                foreach (var line in content)
                {
                    if (line.StartsWith("User:"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                            return parts[1].Trim(); // zabiera spacje jeśli są
                    }
                }
                return "user";
            }


            public void HandleInput(ConsoleKeyEx key, char keyChar)
            {
                if (!isOpen) return;

                // --- obsługa resetu ---
                if (waitingForResetPassword)
                {
                    if (key == ConsoleKeyEx.Enter)
                    {
                        string correctPass = GetInstalledPassword();
                        if (resetPasswordInput == correctPass)
                        {
                            AddLine("Password correct. Type 'y' to confirm reset or 'n' to cancel:");
                            waitingForResetPassword = false;
                            waitingForResetConfirm = true;
                        }
                        else
                        {
                            AddLine("Incorrect password. Reset aborted.");
                            waitingForResetPassword = false;
                        }
                        resetPasswordInput = "";
                    }
                    else if (key == ConsoleKeyEx.Backspace && resetPasswordInput.Length > 0)
                    {
                        resetPasswordInput = resetPasswordInput.Substring(0, resetPasswordInput.Length - 1);
                    }
                    else if (!char.IsControl(keyChar))
                    {
                        resetPasswordInput += keyChar;
                    }
                    return;
                }

                if (waitingForResetConfirm)
                {
                    if (key == ConsoleKeyEx.Enter)
                    {
                        if (resetPasswordInput.ToLower() == "y")
                            PerformReset();
                        else
                            AddLine("Reset cancelled.");
                        waitingForResetConfirm = false;
                        resetPasswordInput = "";
                    }
                    else if (key == ConsoleKeyEx.Backspace && resetPasswordInput.Length > 0)
                    {
                        resetPasswordInput = resetPasswordInput.Substring(0, resetPasswordInput.Length - 1);
                    }
                    else if (!char.IsControl(keyChar))
                    {
                        resetPasswordInput += keyChar;
                    }
                    return;
                }

                // --- normalne wpisywanie komend ---
                if (key == ConsoleKeyEx.Enter)
                {
                    AddLine(username + "@staros:" + currentDirectory + "$ " + CurrentInput);
                    if (!string.IsNullOrWhiteSpace(CurrentInput))
                        ProcessCommand(CurrentInput.Trim());
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

            private string StripHtmlTags(string html)
            {
                var result = new System.Text.StringBuilder();
                bool insideTag = false;
                foreach (char c in html)
                {
                    if (c == '<') insideTag = true;
                    else if (c == '>') insideTag = false;
                    else if (!insideTag) result.Append(c);
                }
                return result.ToString();
            }


            private void SaveHtmlToFile(string content)
            {
                string fullPath = currentDirectory.TrimEnd('\\') + @"\html.txt";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                try
                {
                    var entry = VFSManager.CreateFile(fullPath);
                    if (entry != null)
                    {
                        using (var stream = entry.GetFileStream())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Flush();
                        }
                        AddLine($"Saved HTML to {fullPath}");
                    }
                    else AddLine("Error creating html.txt");
                }
                catch (Exception ex)
                {
                    AddLine("Error saving html.txt: " + ex.Message);
                }
            }

            private void WriteFileDirect(string fileName, string content)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        try
        {
            var entry = VFSManager.CreateFile(fileName);
            using (var stream = entry.GetFileStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }
        catch (Exception ex)
        {
            AddLine("Error writing file: " + ex.Message);
        }
    }


            private void ProcessCommand(string commandLine)
            {
                var parts = commandLine.Split(new[] { ' ' }, 2);
                var command = parts[0].ToLower();
                var argument = parts.Length > 1 ? parts[1] : "";

                switch (command)
                {
                    case "web":
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            AddLine("Usage: web [url]");
                            break;
                        }
                        try
                        {
                            AddLine($"Downloading {argument} ...");
                            string content = StarOS.Network.Http.DownloadFile(argument);

                            SaveHtmlToFile(content); // zapisujemy surowy HTML

                            // pokazujemy tylko tekst
                            string textOnly = DecodeHtmlEntities(StripHtmlTags(content));
                            foreach (var line in textOnly.Split('\n'))
                                AddLine(line);
                        }
                        catch (Exception ex)
                        {
                            AddLine("Error: " + ex.Message);
                        }
                        break;



                    case "ip":
                        AddLine("Current IP: " + Cosmos.System.Network.Config.NetworkConfiguration.CurrentAddress);
                        break;

                case "spm":
                    HandleSpm(argument);
                    break;



                case "http":
                        if (!string.IsNullOrWhiteSpace(argument))
                        {
                            try
                            {
                                string content = StarOS.Network.Http.DownloadFile(argument);
                                WriteFileDirect("html.txt", content);
                                AddLine("HTML saved to html.txt");
                            }
                            catch (Exception ex)
                            {
                                AddLine("HTTP Error: " + ex.Message);
                            }
                        }
                        else
                        {
                            AddLine("Usage: http [url]");
                        }
                        break;
    


                    case "dhcp":
                        if (StarOS.Network.Dhcp.Ask())
                            AddLine("DHCP success. IP: " + Cosmos.System.Network.Config.NetworkConfiguration.CurrentAddress);
                        else
                            AddLine("DHCP failed.");
                        break;


                    case "help":
                        AddLine("Available commands:");
                        AddLine("help            - Show this help message");
                        AddLine("clear           - Clear the terminal screen");
                        AddLine("ls              - List files and directories");
                        AddLine("cd [dir]        - Change directory");
                        AddLine("echo [text]     - Print text");
                        AddLine("cat [file]      - Show file content");
                        AddLine("write [file]    - Write text to a file");
                        AddLine("resetos         - Reset the entire OS (requires password confirmation)");
                        break;


                    case "ping":
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            AddLine("Usage: ping [host]");
                            break;
                        }
                        try
                        {
                            string host = argument;
                            string url = host.StartsWith("http") ? host : "http://" + host;

                            int sent = 4;
                            int received = 0;
                            List<long> times = new List<long>();

                            AddLine($"Pinging {host} with HTTP requests:");

                            for (int i = 0; i < sent; i++)
                            {
                                try
                                {
                                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                                    StarOS.Network.Http.DownloadFile(url); // próbujemy pobrać
                                    stopwatch.Stop();

                                    long ms = stopwatch.ElapsedMilliseconds;
                                    times.Add(ms);
                                    received++;

                                    AddLine($"Reply from {host}: time={ms}ms");
                                }
                                catch
                                {
                                    AddLine($"Request timed out.");
                                }

                                // mała przerwa między pingami (symulacja, 1 sekunda)
                                var start = DateTime.Now;
                                while ((DateTime.Now - start).TotalMilliseconds < 1000) { }
                            }

                            // podsumowanie
                            int lost = sent - received;
                            AddLine("");
                            AddLine($"Ping statistics for {host}:");
                            AddLine($"    Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({(lost * 100 / sent)}% loss)");

                            if (times.Count > 0)
                            {
                                long min = long.MaxValue;
                                long max = long.MinValue;
                                long sum = 0;
                                foreach (var t in times)
                                {
                                    if (t < min) min = t;
                                    if (t > max) max = t;
                                    sum += t;
                                }
                                long avg = sum / times.Count;

                                AddLine($"Approximate round trip times in milli-seconds:");
                                AddLine($"    Minimum = {min}ms, Maximum = {max}ms, Average = {avg}ms");
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLine($"Ping error: {ex.Message}");
                        }
                        break;


                    case "clear":
                        Lines.Clear();
                        AddAsciiArt();
                        AddLine($"Current directory: {currentDirectory}");
                        break;

                    case "ls": ListDirectory(); break;
                    case "cd": ChangeDirectory(argument); break;
                    case "echo": AddLine(argument); break;
                    case "cat": CatFile(argument); break;
                    case "write": WriteFile(argument); break;
                    case "snake":
                        var error = new StarOS.MsgBoxes.ErrorWindow(Gui.MainCanvas, "Terminal Error", "[x01] Not Found");
                        error.Draw();
                        break;

               

                    case "resetos": StartResetOS(); break;
                    default: AddLine("Unknown command: " + command); break;
                }
            }

            private void StartResetOS()
            {
                AddLine("WARNING: This will erase all system files!");
                AddLine("Enter admin password to continue:");
                waitingForResetPassword = true;
                resetPasswordInput = "";
            }

            private string GetInstalledPassword()
            {
                if (!File.Exists(@"0:\installed2.flag")) return "";
                var content = File.ReadAllLines(@"0:\installed2.flag");
                foreach (var line in content)
                {
                    if (line.StartsWith("Password:"))
                        return line.Substring(9).Trim();
                }
                return "";
            }

            private void PerformReset()
            {
                try
                {
                    AddLine("Resetting system...");
                    var entries = VFSManager.GetDirectoryListing(@"0:\");
                    foreach (var entry in entries)
                    {
                        try
                        {
                            if (entry.mEntryType == DirectoryEntryTypeEnum.File)
                                VFSManager.DeleteFile(@"0:\" + entry.mName);
                            else if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                                VFSManager.DeleteDirectory(@"0:\" + entry.mName, true);
                        }
                        catch { }
                    }
                    AddLine("System reset complete. Rebooting...");
                    Sys.Power.Reboot();
                }
                catch (Exception ex)
                {
                    AddLine("Error during reset: " + ex.Message);
                }
            }

            // --- reszta funkcji: ListDirectory, ChangeDirectory, CatFile, WriteFile, ReadLineFromUser ---
            private void ListDirectory()
            {
                try
                {
                    var entries = VFSManager.GetDirectoryListing(currentDirectory);
                    foreach (var entry in entries)
                        AddLine(entry.mEntryType == DirectoryEntryTypeEnum.Directory ? entry.mName + "/" : entry.mName);
                }
                catch (Exception ex)
                {
                    AddLine("Error listing directory: " + ex.Message);
                }
            }

            private void ChangeDirectory(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    AddLine("Usage: cd [directory]");
                    return;
                }

                string newPath = path.StartsWith(@"0:\") || path.StartsWith(@"\")
                    ? path
                    : currentDirectory.TrimEnd('\\') + @"\" + path;

                if (!newPath.EndsWith(@"\")) newPath += @"\";

                try
                {
                    var entries = VFSManager.GetDirectoryListing(newPath);
                    currentDirectory = newPath;
                    AddLine("Changed directory to: " + currentDirectory);
                }
                catch
                {
                    AddLine("Directory not found: " + path);
                }
            }

            private void CatFile(string fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    AddLine("Usage: cat [file]");
                    return;
                }

                string fullPath = fileName.StartsWith(@"0:\") ? fileName : currentDirectory.TrimEnd('\\') + @"\" + fileName;

                try
                {
                    var entry = VFSManager.GetFile(fullPath);
                    using (var stream = entry.GetFileStream())
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        string content = System.Text.Encoding.UTF8.GetString(buffer);
                        AddLine(content);
                    }
                }
                catch (Exception ex)
                {
                    AddLine("Error reading file: " + ex.Message);
                }
            }

            private void WriteFile(string fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    AddLine("Usage: write [file]");
                    return;
                }

                AddLine($"Enter text to write to {fileName}. End input with a single '.' on a line.");
                string fullPath = fileName.StartsWith(@"0:\") ? fileName : currentDirectory.TrimEnd('\\') + @"\" + fileName;
                List<string> inputLines = new List<string>();

                while (true)
                {
                    string line = ReadLineFromUser();
                    if (line == ".") break;
                    inputLines.Add(line);
                }

                string content = string.Join("\n", inputLines);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);

                try
                {
                    var entry = VFSManager.CreateFile(fullPath);
                    if (entry == null) { AddLine("Error creating file."); return; }
                    using (var stream = entry.GetFileStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                    AddLine("File saved.");
                }
                catch (Exception ex)
                {
                    AddLine("Error writing file: " + ex.Message);
                }
            }

            private string ReadLineFromUser()
            {
                string line = "";
                while (true)
                {
                    if (KeyboardManager.TryReadKey(out var key))
                    {
                        if (key.Key == ConsoleKeyEx.Enter) break;
                        else if (key.Key == ConsoleKeyEx.Backspace && line.Length > 0)
                            line = line.Substring(0, line.Length - 1);
                        else if (!char.IsControl(key.KeyChar))
                            line += key.KeyChar;
                    }
                }
                AddLine(line);
                return line;
            }

        private void HandleSpm(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                AddLine("Usage: spm [list|install <pkg>|remove <pkg>]");
                return;
            }

            var parts = argument.Split(' ', 2);
            var cmd = parts[0].ToLower();
            var pkg = parts.Length > 1 ? parts[1] : "";

            switch (cmd)
            {
                case "list":
                    try
                    {
                        string index = StarOS.Network.Http.DownloadFile("http://staros.ct8.pl/pkg/v1/index.txt");
                        AddLine("Available packages:");

                        var seen = new HashSet<string>();

                        foreach (var line in index.Split('\n'))
                        {
                            string packageName = line.Trim(); // <-- zmieniamy nazwę
                            if (string.IsNullOrWhiteSpace(packageName)) continue;
                            if (seen.Contains(packageName)) continue;
                            seen.Add(packageName);

                            AddLine("  " + packageName);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLine("Error fetching package list: " + ex.Message);
                    }
                    break;



                case "install":
                    if (string.IsNullOrWhiteSpace(pkg))
                    {
                        AddLine("Usage: spm install <package>");
                        return;
                    }
                    try
                    {
                        AddLine($"Downloading {pkg} ...");
                        byte[] fileData = StarOS.Network.Http.DownloadRawFile("http://staros.ct8.pl/pkg/v1/" + pkg + ".zip");

                        string savePath = @"0:\Programs\" + pkg + ".zip";
                        var entry = VFSManager.CreateFile(savePath);
                        using (var stream = entry.GetFileStream())
                        {
                            stream.Write(fileData, 0, fileData.Length);
                        }

                        AddLine($"Package {pkg} downloaded to {savePath}");
                        // TODO: rozpakować ZIP
                    }
                    catch (Exception ex)
                    {
                        AddLine("Install failed: " + ex.Message);
                    }
                    break;

                case "remove":
                    if (string.IsNullOrWhiteSpace(pkg))
                    {
                        AddLine("Usage: spm remove <package>");
                        return;
                    }
                    try
                    {
                        string filePath = @"0:\Programs\" + pkg + ".zip";
                        VFSManager.DeleteFile(filePath);
                        AddLine($"Removed package {pkg}");
                    }
                    catch (Exception ex)
                    {
                        AddLine("Remove failed: " + ex.Message);
                    }
                    break;

                default:
                    AddLine("Unknown spm command. Use: list, install, remove");
                    break;
            }
        }



        private string DecodeHtmlEntities(string text)
            {
                return text.Replace("&nbsp;", " ")
                           .Replace("&lt;", "<")
                           .Replace("&gt;", ">")
                           .Replace("&amp;", "&")
                           .Replace("&quot;", "\"")
                           .Replace("&apos;", "'")
                           .Replace("&copy;", "(c)")
                           .Replace("&reg;", "(R)");
            }


            public void Draw(SVGAIICanvas canvas)

            {
                if (!isOpen) return;

                Gui.DrawRoundedWindow(x, y, width, height, 10, Color.Black, Color.Gray);

                int lineY = y + 20;
                Color textColor = Color.LightGreen;

                int start = Lines.Count > maxVisibleLines ? Lines.Count - maxVisibleLines : 0;
                for (int i = start; i < Lines.Count; i++)
                {
                    canvas.DrawString(Lines[i], PCScreenFont.Default, textColor, x + 10, lineY);
                    lineY += 18;
                }
                username = GetInstalledUsername(); // <-- pobiera nazwę usera
                canvas.DrawString(username + "@staros:" + currentDirectory + "$ " + CurrentInput, PCScreenFont.Default, textColor, x + 10, lineY);

            }
        }
    }
