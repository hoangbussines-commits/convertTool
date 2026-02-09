using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        
        try
        {
            if (!CheckMainApp())
                return;

            if (!CheckMainAppInfo())
                return;

            if (args.Length == 0)
            {
                Console.WriteLine("No command.");
                Pause();
                return;
            }

            string cmd = args[0].ToLower();

            if (cmd == "-clearhistory")
            {
                ClearHistoryCommand();
            }
            else
            {
                Console.WriteLine("Unknown command.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        Pause();
    }

    // ===== CHECK MAIN APP EXISTS =====

    static bool CheckMainApp()
    {
        string exeFolder = GetExeFolder();
        string mainExe = Path.Combine(exeFolder, "convertTool.exe");

        if (!File.Exists(mainExe))
        {
            Console.WriteLine("Main app not found. Abort.");
            return false;
        }

        return true;
    }

    // ===== HANDSHAKE VIA -info =====

    static bool CheckMainAppInfo()
    {
        try
        {
            string exeFolder = GetExeFolder();
            string mainExe = Path.Combine(exeFolder, "convertTool.exe");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = mainExe,
                Arguments = "-info",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(psi))
            {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                if (output.Contains("convertTool OK"))
                    return true;
            }

            Console.WriteLine("Main app handshake failed.");
            return false;
        }
        catch
        {
            Console.WriteLine("Main app handshake failed.");
            return false;
        }
    }

    // ===== CLEAR HISTORY =====

    static void ClearHistoryCommand()
    {
        string historyFolder = GetHistoryFolder();

        var files = Directory.GetFiles(historyFolder);

        Console.WriteLine("CLEAR HISTORY REQUESTED");
        Console.WriteLine();

        if (files.Length == 0)
        {
            Console.WriteLine("History empty.");
            return;
        }

        Console.WriteLine($"History contains {files.Length} file(s).");
        Console.WriteLine();

        Console.Write("Are you sure? (Y/N): ");
        string confirm = Console.ReadLine().Trim().ToUpper();

        if (confirm != "Y")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        int deleted = 0;

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
                deleted++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot delete {Path.GetFileName(file)}");
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine($"DONE ☠️ Cleared {deleted} file(s).");
    }

    // ===== PATH =====

    static string GetExeFolder()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    static string GetHistoryFolder()
    {
        string path = Path.Combine(GetExeFolder(), "convertHistory");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
