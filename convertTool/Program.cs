using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{


    //PROFILE SYSTEM
    static string GetProfileFolder()
    {
        string path = Path.Combine(GetExeFolder(), "profile");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    static string GetProfilePath()
    {
        return Path.Combine(GetProfileFolder(), "profile.json");
    }

    static void SaveProfile(UserProfile profile)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(
            profile,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(GetProfilePath(), json);
    }

    static UserProfile LoadProfile()
    {
        if (!File.Exists(GetProfilePath()))
            return null;

        return System.Text.Json.JsonSerializer.Deserialize<UserProfile>(
            File.ReadAllText(GetProfilePath()));
    }

    static string EncryptString(string text)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);

        byte[] encrypted = System.Security.Cryptography.ProtectedData.Protect(
            data,
            null,
            System.Security.Cryptography.DataProtectionScope.CurrentUser
        );

        return Convert.ToBase64String(encrypted);
    }

    static string DecryptString(string encryptedText)
    {
        byte[] data = Convert.FromBase64String(encryptedText);

        byte[] decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
            data,
            null,
            System.Security.Cryptography.DataProtectionScope.CurrentUser
        );

        return System.Text.Encoding.UTF8.GetString(decrypted);
    }

    static void SetupProfile()
    {
        while (true)
        {
            string username, email, password, display;

            // Validate username
            while (true)
            {
                Console.Write("Enter username (required): ");
                username = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(username))
                    break;

                Console.WriteLine("Username cannot be empty!");
            }

            // Email optional, accept empty
            Console.Write("Enter email (optional): ");
            email = Console.ReadLine()?.Trim();

            // Validate password
            while (true)
            {
                Console.Write("Enter password (required): ");
                password = ReadPassword();

                if (!string.IsNullOrEmpty(password))
                    break;

                Console.WriteLine("Password cannot be empty!");
            }

            // Validate display name
            while (true)
            {
                Console.Write("Enter display name (required): ");
                display = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(display))
                    break;

                Console.WriteLine("Display name cannot be empty!");
            }

            Console.Write("Confirm setup? (Y/N): ");
            string confirm = Console.ReadLine().ToUpper();

            if (confirm == "Y")
            {
                SaveProfile(new UserProfile
                {
                    Username = username,
                    Email = email ?? "",  // Handle null
                    Password = EncryptString(password),
                    DisplayName = display
                });

                RestartApp();
                return;
            }

            Console.WriteLine("Restarting setup...");
        }
    }

    static void AdvancedCLIMode()
    {
        Console.Clear();
        ShowBanner();

        var profile = LoadProfile();
        if (profile != null)
        {
            Console.WriteLine($"Advanced Mode - User: {profile.DisplayName}");
        }
        else
        {
            Console.WriteLine("Advanced Mode - User: Normal");
        }

        Console.WriteLine("Type 'exit' to return to normal mode.");
        Console.WriteLine();

        while (true)
        {
            Console.Write(GetAdvancedPrompt());


            string cmd = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(cmd) || cmd == "exit")
            {
                // DÙNG HÀM CÓ SẴN RestartApp()!
                RestartApp();
                return;
            }

            Console.WriteLine("Unknown command.");
        }
    }

    static void RestartApp()
    {
        string exe = System.Diagnostics.Process
            .GetCurrentProcess()
            .MainModule.FileName;

        // Clear console trước
        Console.Clear();

        // Start process mới
        System.Diagnostics.Process.Start(exe);

        // Delay một chút để process mới kịp start
        System.Threading.Thread.Sleep(100);

        // Exit process cũ
        Environment.Exit(0);
    }
    //END PROFILE SYSTEM
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
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine("                    COMMAND LIST");
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine();

        WriteCmd("setupprofile", "Create profile (first time setup)");
        WriteCmd("setupprofile -r", "Reset and delete current profile");
        Console.WriteLine();
        WriteCmd("clearopen", "Remove Open With integration");
        WriteCmd("help", "Show this help menu");
        WriteCmd("info", "Show tool information");
        WriteCmd("apppath/path/where", "Show application paths and file info");  
        WriteCmd("change prompt/prompt", "Change CLI prompt style"); 
        WriteCmd("(ENTER)", "Continue to main tool");

        Console.WriteLine();
    }

    static void WriteCmd(string cmd, string desc)
    {
        Console.Write(" ");
        Console.Write(cmd.PadRight(18));
        Console.Write(" - ");
        Console.WriteLine(desc);
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

        var profile = LoadProfile();

        if (profile != null)
        {
            Console.WriteLine($"Welcome back! User: {profile.DisplayName}");

            while (true)
            {
                Console.Write("Enter password: ");
                string pass = ReadPassword();

                string realPass = DecryptString(profile.Password);

                if (pass == realPass)
                    break;

                Console.WriteLine("Wrong password.");
            }
        }
        else
        {
            Console.WriteLine("Welcome back! User: Normal");
        }

        ShowHelpMenu();

        while (true)
        {
            Console.Write(GetPrompt()); 

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

                case "setupprofile":

                    if (ProfileExists())
                    {
                        Console.WriteLine("Profile already exists.");
                        Console.WriteLine("Use 'setupprofile -r' to reset profile.");
                        break;
                    }

                    SetupProfile();
                    break;

                case "setupprofile -r":

                    ResetProfileFlow();
                    break;
                case "adv":
                    AdvancedCLIMode();
                    break;

                case "apppath":
                case "path":
                case "where":
                    ShowCurrentPaths();
                    break;

                case "change prompt":
                case "prompt":
                    ChangePromptMenu();
                    continue;


                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }


    }

    static string GetLinuxStylePrompt(string mode = "")
    {
        string username = Environment.UserName;
        string hostname = Environment.MachineName;

        // Lấy current directory, thay thế \ bằng / cho giống Unix
        string currentDir = Environment.CurrentDirectory.Replace('\\', '/');

        // Rút gọn home directory thành ~
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('\\', '/');
        if (currentDir.StartsWith(homePath))
        {
            currentDir = "~" + currentDir.Substring(homePath.Length);
        }

        // Thêm mode nếu có
        string fullMode = string.IsNullOrEmpty(mode) ? "ToolCLI" : $"ToolCLI/{mode}";

        return $"{username}@{hostname}:{currentDir}/{fullMode}$ ";
    }

    static string GetAdvancedPrompt()
    {
        string promptType = LoadCurrentPrompt();

        switch (promptType)
        {
            case "linux":
                return GetLinuxStylePrompt("advanced");
            case "simple":
                return "ToolCLI/advanced> ";
            case "classic":
                return $"{Environment.CurrentDirectory}> convertTool/advanced> ";
            case "default":
            default:
                string drive = Path.GetPathRoot(Environment.CurrentDirectory).TrimEnd('\\');
                return $"{drive}>ToolCLI/advanced> ";
        }
    }

    static void ChangePromptMenu()
    {
        string promptFile = Path.Combine(GetProfileFolder(), "setting_prompt.json");

        // Load prompt setting hiện tại
        string currentPrompt = LoadCurrentPrompt();

        Console.WriteLine();
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine("               PROMPT SELECTION MENU");
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.WriteLine($"Current prompt: {currentPrompt}");
        Console.WriteLine();

        Console.WriteLine("1. Default (J:>ToolCLI>)");
        Console.WriteLine("2. Linux Style (user@host:~/ToolCLI$)");
        Console.WriteLine("3. Simple (ToolCLI>)");
        Console.WriteLine("4. Classic (C:\\> convertTool)");
        Console.WriteLine();

        Console.Write("Choose (1-4): ");
        string choice = Console.ReadLine();

        string newPrompt = "";

        switch (choice)
        {
            case "1":
                newPrompt = "default";
                break;
            case "2":
                newPrompt = "linux";
                break;
            case "3":
                newPrompt = "simple";
                break;
            case "4":
                newPrompt = "classic";
                break;
            default:
                Console.WriteLine("Invalid choice.");
                return;
        }

        // Save to profile
        SavePromptSetting(newPrompt);

        Console.WriteLine();
        Console.WriteLine($"Prompt changed to: {newPrompt}");
        Console.WriteLine("Restarting app to apply changes...");

        // Restart để apply prompt mới
        RestartApp();
    }

    static string LoadCurrentPrompt()
    {
        string promptFile = Path.Combine(GetProfileFolder(), "setting_prompt.json");

        if (!File.Exists(promptFile))
            return "default"; // default prompt

        try
        {
            string json = File.ReadAllText(promptFile);
            return System.Text.Json.JsonSerializer.Deserialize<string>(json) ?? "default";
        }
        catch
        {
            return "default";
        }
    }

    static void SavePromptSetting(string promptType)
    {
        string promptFile = Path.Combine(GetProfileFolder(), "setting_prompt.json");

        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(promptType);
            File.WriteAllText(promptFile, json);
        }
        catch
        {
            // silent fail
        }
    }

    static string GetPrompt()
    {
        string promptType = LoadCurrentPrompt();

        switch (promptType)
        {
            case "linux":
                return GetLinuxStylePrompt();
            case "simple":
                return "ToolCLI> ";
            case "classic":
                return $"{Environment.CurrentDirectory}> convertTool> ";
            case "default":
            default:
                // default: J:>ToolCLI>
                string drive = Path.GetPathRoot(Environment.CurrentDirectory).TrimEnd('\\');
                return $"{drive}>ToolCLI> ";
        }
    }

    static void ShowCurrentPaths()
    {
        string exeFolder = GetExeFolder();
        string mainExe = Path.Combine(exeFolder, "convertTool.exe");
        string shellExe = Path.Combine(exeFolder, "convertTool_shell.exe");
        string historyFolder = GetHistoryFolder();
        string profileFolder = GetProfileFolder();

        Console.WriteLine();
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine("                    CURRENT PATHS");
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine();

        Console.WriteLine($" Main App:       {mainExe}");
        Console.WriteLine($" Shell Helper:   {(File.Exists(shellExe) ? shellExe : "Not extracted")}");
        Console.WriteLine($" History Folder: {historyFolder}");
        Console.WriteLine($" Profile Folder: {profileFolder}");
        Console.WriteLine($" Working Dir:    {Environment.CurrentDirectory}");
        Console.WriteLine();

        // Bonus: show file info
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine("                     FILE INFO");
        Console.WriteLine("════════════════════════════════════════════════════");
        Console.WriteLine();

        if (File.Exists(mainExe))
        {
            var mainInfo = new FileInfo(mainExe);
            Console.WriteLine($"convertTool.exe:     {mainInfo.Length / 1024} KB, {mainInfo.LastWriteTime}");
        }

        if (File.Exists(shellExe))
        {
            var shellInfo = new FileInfo(shellExe);
            Console.WriteLine($"convertTool_shell.exe: {shellInfo.Length / 1024} KB, {shellInfo.LastWriteTime}");
        }

        // Count files in history
        if (Directory.Exists(historyFolder))
        {
            int historyCount = Directory.GetFiles(historyFolder).Length;
            Console.WriteLine($"History files: {historyCount} files");
        }

        Console.WriteLine();
    }

    static void DeleteProfile()
    {
        try
        {
            string path = GetProfilePath();

            if (File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }

    static void ResetProfileFlow()
    {
        if (!ProfileExists())
        {
            Console.WriteLine("No profile found.");
            return;
        }

        // Load profile hiện tại
        var profile = LoadProfile();
        if (profile == null)
        {
            Console.WriteLine("Cannot load profile.");
            return;
        }

        Console.WriteLine("WARNING: This will delete current profile.");

        // Yêu cầu nhập password để xác nhận
        Console.Write("Enter current password to confirm deletion: ");
        string enteredPassword = ReadPassword();

        string realPassword = DecryptString(profile.Password);

        if (enteredPassword != realPassword)
        {
            Console.WriteLine("Wrong password. Deletion cancelled.");
            return;
        }

        Console.Write("Are you absolutely sure? (Type 'DELETE' to confirm): ");
        string confirm = Console.ReadLine().Trim().ToUpper();

        if (confirm != "DELETE")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        DeleteProfile();
        Console.WriteLine("Profile deleted successfully.");

        // Optional: restart app
        RestartApp();
    }

    static bool ProfileExists()
    {
        return File.Exists(GetProfilePath());
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
