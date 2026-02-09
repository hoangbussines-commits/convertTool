using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void ShowBanner()
    {
        Console.WriteLine(@"
 ██████╗ ██████╗ ███╗   ██╗██╗   ██╗███████╗██████╗ ████████╗
██╔════╝██╔═══██╗████╗  ██║██║   ██║██╔════╝██╔══██╗╚══██╔══╝
██║     ██║   ██║██╔██╗ ██║██║   ██║█████╗  ██████╔╝   ██║   
██║     ██║   ██║██║╚██╗██║╚██╗ ██╔╝██╔══╝  ██╔══██╗   ██║   
╚██████╗╚██████╔╝██║ ╚████║ ╚████╔╝ ███████╗██║  ██║   ██║   
 ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝   ╚═╝   

                 CONVERT TOOL
------------------------------------------------------------
Powered by hycoredragon 
");
    }

    static void ShowHelpMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Available Commands:");
        Console.WriteLine();
        Console.WriteLine(" clearopen   - Remove Open With integration");
        Console.WriteLine(" help        - Show this help menu");
        Console.WriteLine(" info        - Show tool information");
        Console.WriteLine(" (ENTER)     - Continue to main tool");
        Console.WriteLine();
    }

    static void ShowInfo()
    {
        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("                 CONVERT TOOL INFO                ");
        Console.WriteLine("==================================================");
        Console.WriteLine();
        Console.WriteLine(" Developer        : hycoredragon");
        Console.WriteLine(" Support Server   : === hycord server ===");
        Console.WriteLine(" Server URL       : https://discord.gg/DuH6c7hhsK");
        Console.WriteLine();
        Console.WriteLine(" Tool Type        : PNG/JPG -> ICO Converter");
        Console.WriteLine(" Runtime          : .NET 8 Self-Contained");
        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine();
    }

    static void ToolCLI()
    {
        ShowBanner();
        ShowHelpMenu();   

        while (true)
        {

            Console.Write("ToolCLI> ");

            string cmd = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(cmd))
            {
                // ENTER → exit CLI → vào Main
                break;
            }

            switch (cmd)
            {
                case "clearopen":
                    ClearOpenWith();
                    DeleteFirstRunFlag();
                    Console.WriteLine("Open With removed.");
                    break;
                case "help":
                    ShowHelpMenu();
                    break;

                case "info":
                    ShowInfo();
                    break;
                case "dev-debug":
                    DevDebugMode();
                    break;


                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }


    }

    static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(true);

            // ENTER → kết thúc nhập
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            // BACKSPACE → xoá ký tự
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                continue;
            }

            // Thêm ký tự → in *
            password.Append(key.KeyChar);
            Console.Write("*");
        }

        return password.ToString();
    }


    static void DevDebugMode()
    {
        const string pass = "hycoredragondebug";

        Console.Write("Enter password: ");
        string p1 = ReadPassword();

        Console.Write("Re-enter password: ");
        string p2 = ReadPassword();

        if (p1 != pass || p2 != pass)
        {
            Console.WriteLine("Access denied.");
            return;
        }

        Console.WriteLine("Access granted.");

        DumpEmbeddedDll("converttool.dll", "convertTool.dll");
        DumpEmbeddedDll("converttool_shell.dll", "convertTool_shell.dll");

        Console.WriteLine("Dump done.");
    }

    static void DumpEmbeddedDll(string keyword, string outputFile)
    {
        try
        {
            var asm = typeof(Program).Assembly;
            string exeFolder = GetExeFolder();

            foreach (var res in asm.GetManifestResourceNames())
            {
                if (res.ToLower().Contains(keyword))
                {
                    using var rs = asm.GetManifestResourceStream(res);
                    using var fs = File.Create(Path.Combine(exeFolder, outputFile));

                    rs.CopyTo(fs);

                    Console.WriteLine($"{outputFile} dumped.");
                    return;
                }
            }

            Console.WriteLine($"{outputFile} resource not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dump failed: {ex.Message}");
        }
    }



    static void ClearOpenWith()
    {
        try
        {
            string[] exts = { ".png", ".jpg", ".jpeg" };

            foreach (var ext in exts)
            {
                string keyPath =
                    $@"Software\Classes\{ext}\OpenWithList";

                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath, true))
                {
                    key?.DeleteSubKeyTree("convertTool.exe", false);
                }
            }
        }
        catch { }
    }

    static void DeleteFirstRunFlag()
    {
        try
        {
            string flag = Path.Combine(GetExeFolder(), "firstrun.flag");

            if (File.Exists(flag))
                File.Delete(flag);
        }
        catch { }
    }
    static string GetHistoryFolder()
    {
        string path = Path.Combine(GetExeFolder(), "convertHistory");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }
    static void SaveToHistory(string filePath)
    {
        try
        {
            string history = GetHistoryFolder();
            string name = Path.GetFileName(filePath);
            string dest = Path.Combine(history, name);

            File.Copy(filePath, dest, true);
        }
        catch { }
    }
    static bool IsFirstRun()
    {
        string flag = Path.Combine(GetExeFolder(), "firstrun.flag");
        return !File.Exists(flag);
    }

    static void SetFirstRunDone()
    {
        string flag = Path.Combine(GetExeFolder(), "firstrun.flag");
        File.WriteAllText(flag, "done");
    }

    static void AddToOpenWith()
    {
        try
        {
            string exePath = Path.Combine(GetExeFolder(), "convertTool.exe");

            string[] exts = { ".png", ".jpg", ".jpeg" };

            foreach (var ext in exts)
            {
                string keyPath =
                    $@"Software\Classes\{ext}\OpenWithList\convertTool.exe";

                using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                }
            }

            Console.WriteLine("Added to Open With list.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to add Open With: " + ex.Message);
        }
    }



    static List<FileStream> historyLocks = new List<FileStream>();
    static void LockAllHistoryFiles()
    {
        try
        {
            string historyFolder = GetHistoryFolder();

            var files = Directory.GetFiles(historyFolder);

            foreach (var file in files)
            {
                try
                {
                    var fs = new FileStream(
                        file,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read

                    );

                    historyLocks.Add(fs);
                }
                catch
                {
                    // Nếu file đang bị lock bởi ai đó → skip
                }
            }
        }
        catch { }
    }


    static string GetExeFolder()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }
    static void ExtractShellTool()
    {
        try
        {
            string exeFolder = GetExeFolder();
            string outputPath = Path.Combine(exeFolder, "convertTool_shell.exe");

            if (File.Exists(outputPath))
                return;

            var asm = typeof(Program).Assembly;

            using (Stream res = asm.GetManifestResourceStream("convertTool.convertTool_shell.exe"))
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                res.CopyTo(fs);
            }
        }
        catch
        {
            // silent fail OK
        }
    }
    static void Main(string[] args)
    {
        ExtractShellTool();
        LockAllHistoryFiles();

        ToolCLI();

        if (IsFirstRun())
        {
            while (true)
            {
                Console.WriteLine("Add convertTool to Open With list? (Y/N)");

                var k = Console.ReadKey(true);
                char c = char.ToUpper(k.KeyChar);

                if (c == 'Y')
                {
                    AddToOpenWith();
                    SetFirstRunDone();
                    break;
                }
                else if (c == 'N')
                {
                    SetFirstRunDone();
                    break;
                }
                else
                {
                    Console.WriteLine("Please press Y or N.");
                }
            }
        }

        try
        {
           
            string inputPath = null;

            if (args.Length > 0 && args[0].ToLower() == "-info")
            {
                Console.WriteLine("convertTool OK");
                return;
            }
            if (args.Length == 0)
            {
                Console.WriteLine("No file selected.");


                string historyChoice = ShowHistoryMenu();

                if (historyChoice == null)
                {
                    Pause();
                    return;
                }

                inputPath = historyChoice;
            }
            else
            {
                inputPath = args[0];
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("File not found.");
                Pause();
                return;
            }



            Console.WriteLine("=== ICON CONVERT TOOL ===");
            Console.WriteLine();
            Console.WriteLine("File: " + Path.GetFileName(inputPath));
            Console.WriteLine();

            // ===== SIZE MENU =====
            Console.WriteLine("Select mode:");
            Console.WriteLine("1 - Single size");
            Console.WriteLine("2 - Multi size (16,32,48,64,128,256)");
            Console.WriteLine("3 - Custom size");
            Console.WriteLine();

            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            List<int> sizes = new List<int>();

            if (choice == "1")
            {
                Console.Write("Enter size: ");
                sizes.Add(int.Parse(Console.ReadLine()));
            }
            else if (choice == "2")
            {
                sizes.AddRange(new int[] { 16, 32, 48, 64, 128, 256 });
            }
            else if (choice == "3")
            {
                Console.Write("Enter sizes (32,64,128): ");
                string[] input = Console.ReadLine().Split(',');
                foreach (var s in input)
                    sizes.Add(int.Parse(s.Trim()));
            }
            else
            {
                Console.WriteLine("Invalid choice.");
                Pause();
                return;
            }



            static void DrawProgressBar(int progress, int total)
            {
                int barSize = 30;

                double percent = (double)progress / total;
                int filled = (int)(percent * barSize);

                string bar =
                    new string('█', filled) +
                    new string('░', barSize - filled);

                Console.Write($"\r[{bar}] {(percent * 100):0}%");
            }
            static string ShowHistoryMenu()
            {
                string history = GetHistoryFolder();
                var files = Directory.GetFiles(history);

                if (files.Length == 0)
                {
                    Console.WriteLine("History empty.");
                    return null;
                }

                Console.WriteLine();
                Console.WriteLine("=== CONVERT HISTORY ===");

                for (int i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"{i + 1} - {Path.GetFileName(files[i])}");
                }

                Console.WriteLine();
                Console.Write("Choose file number: ");

                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= files.Length)
                        return files[choice - 1];
                }

                return null;
            }


            // ===== COUNT MENU =====
            int count = 1;

            while (true)
            {
                Console.WriteLine();
                Console.Write("How many ICO files to create (1-10): ");

                if (!int.TryParse(Console.ReadLine(), out count))
                    count = 1;

                if (count < 1) count = 1;
                if (count > 10) count = 10;

                Console.WriteLine();
                Console.Write("Are you sure? (Y/N): ");
                string confirm = Console.ReadLine().Trim().ToUpper();

                if (confirm == "Y")
                    break;

                Console.WriteLine("Re-selecting count...");
            }


            string historyFolder = GetHistoryFolder();
            string exeFolder = GetExeFolder();

            string folder;

            if (inputPath.StartsWith(historyFolder, StringComparison.OrdinalIgnoreCase))
            {
                // Nếu file nằm trong history → xuất ico ra ngoài exe
                folder = exeFolder;
            }
            else
            {
                // Nếu file gốc → xuất ico cùng folder gốc
                folder = Path.GetDirectoryName(inputPath);
            }
            string name = Path.GetFileNameWithoutExtension(inputPath);

            using (Bitmap original = new Bitmap(inputPath))
            {
                Console.WriteLine();
                Console.WriteLine("Creating icons...");
                Console.WriteLine();

                DrawProgressBar(0, count);

                for (int i = 1; i <= count; i++)
                {
                    string outputName = i == 1
                        ? name + ".ico"
                        : name + "_" + i + ".ico";

                    string outputPath = Path.Combine(folder, outputName);

                    SaveMultiIcon(original, sizes, outputPath);

                    DrawProgressBar(i, count);
                }

                Console.WriteLine();
                Console.WriteLine("DONE ");
                SaveToHistory(inputPath);


            }

            Console.WriteLine();
            Console.WriteLine("DONE  Created " + count + " icon file(s).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        Pause();
    }

    static void SaveMultiIcon(Bitmap original, List<int> sizes, string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)sizes.Count);

            long dirStart = fs.Position;
            bw.Write(new byte[sizes.Count * 16]);

            List<byte[]> imageDatas = new List<byte[]>();

            foreach (int size in sizes)
            {
                using (Bitmap resized = new Bitmap(original, new Size(size, size)))
                using (MemoryStream ms = new MemoryStream())
                {
                    resized.Save(ms, ImageFormat.Png);
                    imageDatas.Add(ms.ToArray());
                }
            }

            long dataOffset = 6 + sizes.Count * 16;
            fs.Position = dirStart;

            for (int i = 0; i < sizes.Count; i++)
            {
                int size = sizes[i];
                byte[] img = imageDatas[i];

                bw.Write((byte)(size >= 256 ? 0 : size));
                bw.Write((byte)(size >= 256 ? 0 : size));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(img.Length);
                bw.Write((int)dataOffset);

                dataOffset += img.Length;
            }

            fs.Position = fs.Length;

            foreach (var img in imageDatas)
                bw.Write(img);
        }
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
