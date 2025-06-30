using System;
using System.IO;
using Cosmos.System.FileSystem.VFS;

public static class Mfiles
{
    public static bool Copy(string source, string destination)
    {
        try
        {
            if (!File.Exists(source))
            {
                Console.WriteLine($"Source file '{source}' not found.");
                return false;
            }

            if (File.Exists(destination))
            {
                VFSManager.DeleteFile(destination);
            }

            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (var destStream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int read;
                while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, read);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Copy error: {ex.Message}");
            return false;
        }
    }

    public static bool Move(string source, string destination)
    {
        if (!Copy(source, destination))
            return false;

        try
        {
            VFSManager.DeleteFile(source);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move error: {ex.Message}");
            return false;
        }
    }

    public static bool CreateFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Console.WriteLine("File already exists.");
                return false;
            }

            using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                // Tworzymy pusty plik
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateFile error: {ex.Message}");
            return false;
        }
    }

    public static bool DeleteFile(string path)
    {
        try
        {
            VFSManager.DeleteFile(path); // void metoda

            if (File.Exists(path))
                return false;
            else
                return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteFile error: {ex.Message}");
            return false;
        }
    }


    public static void Echo(string text)
    {
        Console.WriteLine(text);
    }
}
