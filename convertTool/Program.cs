using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;


class Program
{
    public static class AppInfo
    {
        public const string tool_version = "v1.1.4.1";
        public const string ownered_by = "hycoredragon";
        public const string runtime_info = ".NET 8 Self-Contained";
        public const string copyright_info = "Copyright (c) 2026 hycoredragon. All rights reserved.";
        public const string discord_info = "https://discord.gg/DuH6c7hhsK";
        public const string repository_info = "https://github.com/hoangbussines-commits/convertTool";
        public const string license_info = "MIT License";
        public const string legal_info = "This tool is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.";
        public const string license_url = "https://opensource.org/licenses/MIT";
        public const string support_info = "For support, visit the Discord server linked above.";
        public const string additional_info = "This tool is developed and maintained by hycoredragon.";
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

    static string ShowHistoryMenu()
    {
        string history = GetHistoryFolder();
        var files = Directory.GetFiles(history);

        if (files.Length == 0)
        {
            ConsoleMG.WriteLine("History empty.");
            return null;
        }

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("=== CONVERT HISTORY ===");

        for (int i = 0; i < files.Length; i++)
        {
            ConsoleMG.WriteLine($"{i + 1} - {Path.GetFileName(files[i])}");
        }

        ConsoleMG.WriteLine();
        ConsoleMG.Write("Choose file number: ");

        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            if (choice >= 1 && choice <= files.Length)
                return files[choice - 1];
        }

        return null;
    }
    static void DrawProgressBar(int progress, int total)
    {
        int barSize = 30;

        double percent = (double)progress / total;
        int filled = (int)(percent * barSize);

        string bar =
            new string('█', filled) +
            new string('░', barSize - filled);

        ConsoleMG.Write($"\r[{bar}] {(percent * 100):0}%");
    }
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

    static void ViewCurrentProfile()
    {
        if (!ProfileExists())
        {
            ConsoleMG.WriteLine("❌ No profile found.");
            ConsoleMG.WriteLine("   Use 'setupprofile' to create one.");
            return;
        }

        var profile = LoadProfile();
        if (profile == null)
        {
            ConsoleMG.WriteLine("❌ Cannot load profile.");
            return;
        }

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("═══════════════════════════════════════════");
        ConsoleMG.WriteLine("           CURRENT PROFILE");
        ConsoleMG.WriteLine("═══════════════════════════════════════════");
        ConsoleMG.WriteLine();

        ConsoleMG.WriteLine($" Display Name: {profile.DisplayName}");
        ConsoleMG.WriteLine($" Username:     {profile.Username}");

        if (string.IsNullOrEmpty(profile.Email))
        {
            ConsoleMG.WriteLine($" Email:        (empty)");
        }
        else
        {
            bool emailValid = profile.Email.Contains('@');
            ConsoleMG.WriteLine($" Email:        {profile.Email} {(emailValid ? "" : "⚠")}");
        }

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("═══════════════════════════════════════════");
    }

    static void SetupProfile()
    {
        while (true)
        {
            string username, email, password, display;

                        while (true)
            {
                ConsoleMG.Write("Enter username (required): ");
                username = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(username))
                    break;

                ConsoleMG.WriteLine("Username cannot be empty!");
            }
            ConsoleMG.Write("Enter email (optional): ");
            email = Console.ReadLine()?.Trim();

            // Nếu email không rỗng VÀ không có @
            if (!string.IsNullOrEmpty(email) && !email.Contains('@'))
            {
                ConsoleMG.WriteLine();
                ConsoleMG.WriteLine("⚠  INVALID EMAIL FORMAT");
                ConsoleMG.WriteLine("   Email should contain '@'");
                ConsoleMG.WriteLine("   Example: user@gmail.com");
                ConsoleMG.WriteLine();

                // Cho user chọn
                while (true)
                {
                    ConsoleMG.WriteLine("Options:");
                    ConsoleMG.WriteLine("R - Re-enter email");
                    ConsoleMG.WriteLine("E - Leave email empty");
                    ConsoleMG.Write("Choose (R/E): ");

                    string choice = Console.ReadLine().Trim().ToUpper();

                    if (choice == "R")
                    {
                        ConsoleMG.Write("Enter email: ");
                        email = Console.ReadLine()?.Trim();

                        // Check lại format mới
                        if (string.IsNullOrEmpty(email) || email.Contains('@'))
                        {
                            break; // OK hoặc empty
                        }

                        // Vẫn sai → loop lại
                        ConsoleMG.WriteLine("Still invalid. Email must contain '@'");
                        continue;
                    }
                    else if (choice == "E")
                    {
                        email = ""; // Set empty
                        break;
                    }
                    else
                    {
                        ConsoleMG.WriteLine("Invalid choice.");
                    }
                }
            }

            while (true)
            {
                ConsoleMG.Write("Enter password (required): ");
                password = ReadPassword();

                if (!string.IsNullOrEmpty(password))
                    break;

                ConsoleMG.WriteLine("Password cannot be empty!");
            }

                        while (true)
            {
                ConsoleMG.Write("Enter display name (required): ");
                display = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(display))
                    break;

                ConsoleMG.WriteLine("Display name cannot be empty!");
            }

            ConsoleMG.Write("Confirm setup? (Y/N): ");
            string confirm = Console.ReadLine().ToUpper();

            if (confirm == "Y")
            {
                SaveProfile(new UserProfile
                {
                    Username = username,
                    Email = email ?? "",                      Password = EncryptString(password),
                    DisplayName = display
                });

                RestartApp();
                return;
            }

            ConsoleMG.WriteLine("Restarting setup...");
        }
    }

    static void AdvancedCLIMode()
    {
        Console.Clear();
        ShowBanner();

        var profile = LoadProfile();
        if (profile != null)
        {
            ConsoleMG.WriteLine($"Advanced Mode - User: {profile.DisplayName}");
        }
        else
        {
            ConsoleMG.WriteLine("Advanced Mode - User: Normal");
        }

        ConsoleMG.WriteLine("Type 'exit' to return to normal mode.");
        ConsoleMG.WriteLine();

        while (true)
        {
            ConsoleMG.Write(GetAdvancedPrompt());


            string cmd = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(cmd) || cmd == "exit")
            {
                                RestartApp();
                return;
            }

            ConsoleMG.WriteLine("Unknown command.");
        }
    }

    static void RestartApp()
    {
        string exe = System.Diagnostics.Process
            .GetCurrentProcess()
            .MainModule.FileName;

        // Tạo process mới TRƯỚC
        var newProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true
            }
        };

        // Start process mới
        newProcess.Start();

        // Đợi một chút để process mới khởi động
        System.Threading.Thread.Sleep(500);

        // Rồi mới thoát process hiện tại
        Environment.Exit(0);
    }
    static void ShowBanner()
    {
        ConsoleMG.WriteLine($@"
 ██████╗ ██████╗ ███╗   ██╗██╗   ██╗███████╗██████╗ ████████╗
██╔════╝██╔═══██╗████╗  ██║██║   ██║██╔════╝██╔══██╗╚══██╔══╝
██║     ██║   ██║██╔██╗ ██║██║   ██║█████╗  ██████╔╝   ██║   
██║     ██║   ██║██║╚██╗██║╚██╗ ██╔╝██╔══╝  ██╔══██╗   ██║   
╚██████╗╚██████╔╝██║ ╚████║ ╚████╔╝ ███████╗██║  ██║   ██║   
 ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝  ╚═══╝  ╚══════╝╚═╝  ╚═╝   ╚═╝   

                 CONVERT TOOL
------------------------------------------------------------
{AppInfo.copyright_info}

");
    }

    static void ShowHelpMenu()
    {
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine("                    COMMAND LIST");
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine();

        WriteCmd("setupprofile", "Create profile (first time setup)");
        WriteCmd("setupprofile -r", "Reset and delete current profile");
        WriteCmd("setupprofile -rollback", "Restore deleted profile from backup");
        WriteCmd("setupprofile -view", "View current profile information");
        WriteCmd("setupprofile -change password", "Change profile password");
        ConsoleMG.WriteLine();
        WriteCmd("clearopen", "Remove Open With integration");
        WriteCmd("help", "Show this help menu");
        WriteCmd("info", "Show tool information");
        WriteCmd("apppath/path/where", "Show application paths and file info");
        WriteCmd("change prompt/prompt", "Change CLI prompt style");
        WriteCmd("(ENTER)", "Continue to main tool");
        ConsoleMG.WriteLine();
        WriteCmd("adv", "Enter advanced CLI mode");
        WriteCmd("info-adv", "Show advanced detailed command info");
        WriteCmd("adv-convert", "Advanced image format conversion PNG/JPG -> JPG/PNG");
        ConsoleMG.WriteLine();
    }

    static void WriteCmd(string cmd, string desc)
    {
        ConsoleMG.Write(" ");
        ConsoleMG.Write(cmd.PadRight(18));
        ConsoleMG.Write(" - ");
        ConsoleMG.WriteLine(desc);
    }


    static void ShowInfo()
    {
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("==================================================");
        ConsoleMG.WriteLine("                 CONVERT TOOL INFO                ");
        ConsoleMG.WriteLine("==================================================");
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine(" Developer        : hycoredragon");
        ConsoleMG.WriteLine(" Support Server   : === hycord server ===");
        ConsoleMG.WriteLine(" Server URL       : https://discord.gg/DuH6c7hhsK");
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine(" Tool Type        : PNG/JPG -> ICO Converter");
        ConsoleMG.WriteLine(" Runtime          : .NET 8 Self-Contained");
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("==================================================");
        ConsoleMG.WriteLine();
    }

    static void ToolCLI(string[] args = null)
    {
        ShowBanner();


        var profile = LoadProfile();

        if (profile != null)
        {
            ConsoleMG.WriteLine($"Welcome back! User: {profile.DisplayName}");

            while (true)
            {
                ConsoleMG.Write("Enter password: ");
                string pass = ReadPassword();

                string realPass = DecryptString(profile.Password);

                if (pass == realPass)
                    break;

                ConsoleMG.WriteLine("Wrong password.");
            }
        }
        else
        {
            ConsoleMG.WriteLine("Welcome back! User: Normal");
        }

        ShowHelpMenu();

        while (true)
        {
            ConsoleMG.Write(GetPrompt());

            string cmd = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(cmd))
            {
                                break;
            }

            switch (cmd)
            {
                case "clearopen":
                    ClearOpenWith();
                    DeleteFirstRunFlag();
                    ConsoleMG.WriteLine("Open With removed.");
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
                        ConsoleMG.WriteLine("Profile already exists.");
                        ConsoleMG.WriteLine("Use 'setupprofile -r' to reset profile.");
                        break;
                    }

                    SetupProfile();
                    break;

                    case "setupprofile -rollback":
                        RollbackProfile();
                        break;
                    case "setupprofile -view":
                        ViewCurrentProfile();
                        break;
                    case "setupprofile -change password":
                        ChangePassword();
                        break;
                case "setupprofile -r":

                    ResetProfileFlow();
                    break;
                case "adv":
                    AdvancedCLIMode();
                    break;
                case "info-adv":
                    ShowCommandInfoWithCLI();
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

                case "adv-convert":
                    ProcessImageConversion(args);

                    break;
                case "testdll":
                    ConsoleMG.DebugTest();
                    break;




                default:
                    ConsoleMG.WriteLine("Unknown command.");
                    break;
            }
        }


    }

    static void ChangePassword()
    {
        if (!ProfileExists())
        {
            ConsoleMG.WriteLine("ERROR: No profile found.");
            ConsoleMG.WriteLine("       Use 'setupprofile' to create one first.");
            return;
        }

        var profile = LoadProfile();
        if (profile == null)
        {
            ConsoleMG.WriteLine("ERROR: Cannot load profile.");
            return;
        }

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("===========================================");
        ConsoleMG.WriteLine("           CHANGE PASSWORD");
        ConsoleMG.WriteLine("===========================================");
        ConsoleMG.WriteLine();

        // Current password
        ConsoleMG.Write("Enter current password: ");
        string currentPassword = ReadPassword();

        string realPassword = DecryptString(profile.Password);

        if (currentPassword != realPassword)
        {
            ConsoleMG.WriteLine("ERROR: Wrong password. Aborted.");
            return;
        }

        // New password
        string newPassword, confirmPassword;

        while (true)
        {
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("Enter NEW password:");

            ConsoleMG.Write("New password: ");
            newPassword = ReadPassword();

            if (string.IsNullOrEmpty(newPassword))
            {
                ConsoleMG.WriteLine("ERROR: Password cannot be empty.");
                continue;
            }

            if (newPassword.Length < 6)
            {
                ConsoleMG.WriteLine("WARNING: Password should be at least 6 characters.");
                ConsoleMG.Write("Continue anyway? (Y/N): ");
                string continueChoice = Console.ReadLine().Trim().ToUpper();
                ConsoleMG.WriteLine();

                if (continueChoice != "Y")
                    continue;
            }

            ConsoleMG.Write("Confirm new password: ");
            confirmPassword = ReadPassword();

            if (newPassword != confirmPassword)
            {
                ConsoleMG.WriteLine("ERROR: Passwords do not match.");
                continue;
            }

            if (newPassword == realPassword)
            {
                ConsoleMG.WriteLine("ERROR: New password is identical to current password.");
                ConsoleMG.WriteLine("       If you want to keep current password, just cancel.");
                continue;
            }

            break;
        }

        // Final confirmation
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("WARNING: You are about to change your password.");
        ConsoleMG.Write("Type 'CHANGE' to confirm: ");

        string confirm = Console.ReadLine().Trim().ToUpper();
        if (confirm != "CHANGE")
        {
            ConsoleMG.WriteLine("CANCELLED: Password not changed.");
            return;
        }

        // Update
        profile.Password = EncryptString(newPassword);
        SaveProfile(profile);

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("SUCCESS: Password changed!");
        ConsoleMG.WriteLine("Restarting application...");

        RestartApp();
    }

    static string GetLinuxStylePrompt(string mode = "")
    {
        string username = Environment.UserName;
        string hostname = Environment.MachineName;

                string currentDir = Environment.CurrentDirectory.Replace('\\', '/');

                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('\\', '/');
        if (currentDir.StartsWith(homePath))
        {
            currentDir = "~" + currentDir.Substring(homePath.Length);
        }

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

                string currentPrompt = LoadCurrentPrompt();

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine("               PROMPT SELECTION MENU");
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine();

        ConsoleMG.WriteLine($"Current prompt: {currentPrompt}");
        ConsoleMG.WriteLine();

        ConsoleMG.WriteLine("1. Default (J:>ToolCLI>)");
        ConsoleMG.WriteLine("2. Linux Style (user@host:~/ToolCLI$)");
        ConsoleMG.WriteLine("3. Simple (ToolCLI>)");
        ConsoleMG.WriteLine("4. Classic (C:\\> convertTool)");
        ConsoleMG.WriteLine();

        ConsoleMG.Write("Choose (1-4): ");
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
                ConsoleMG.WriteLine("Invalid choice.");
                return;
        }

                SavePromptSetting(newPrompt);

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine($"Prompt changed to: {newPrompt}");
        ConsoleMG.WriteLine("Restarting app to apply changes...");

                RestartApp();
    }

    static string LoadCurrentPrompt()
    {
        string promptFile = Path.Combine(GetProfileFolder(), "setting_prompt.json");

        if (!File.Exists(promptFile))
            return "default"; 
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

        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine("                    CURRENT PATHS");
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine();

        ConsoleMG.WriteLine($" Main App:       {mainExe}");
        ConsoleMG.WriteLine($" Shell Helper:   {(File.Exists(shellExe) ? shellExe : "Not extracted")}");
        ConsoleMG.WriteLine($" History Folder: {historyFolder}");
        ConsoleMG.WriteLine($" Profile Folder: {profileFolder}");
        ConsoleMG.WriteLine($" Working Dir:    {Environment.CurrentDirectory}");
        ConsoleMG.WriteLine();

                ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine("                     FILE INFO");
        ConsoleMG.WriteLine("════════════════════════════════════════════════════");
        ConsoleMG.WriteLine();

        if (File.Exists(mainExe))
        {
            var mainInfo = new FileInfo(mainExe);
            ConsoleMG.WriteLine($"convertTool.exe:     {mainInfo.Length / 1024} KB, {mainInfo.LastWriteTime}");
        }

        if (File.Exists(shellExe))
        {
            var shellInfo = new FileInfo(shellExe);
            ConsoleMG.WriteLine($"convertTool_shell.exe: {shellInfo.Length / 1024} KB, {shellInfo.LastWriteTime}");
        }

                if (Directory.Exists(historyFolder))
        {
            int historyCount = Directory.GetFiles(historyFolder).Length;
            ConsoleMG.WriteLine($"History files: {historyCount} files");
        }

        ConsoleMG.WriteLine();
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
            ConsoleMG.WriteLine("No profile found.");
            return;
        }

        var profile = LoadProfile();
        if (profile == null)
        {
            ConsoleMG.WriteLine("Cannot load profile.");
            return;
        }

        ConsoleMG.WriteLine("WARNING: This will delete current profile.");

        ConsoleMG.Write("Enter current password to confirm deletion: ");
        string enteredPassword = ReadPassword();

        string realPassword = DecryptString(profile.Password);

        if (enteredPassword != realPassword)
        {
            ConsoleMG.WriteLine("Wrong password. Deletion cancelled.");
            return;
        }

        ConsoleMG.Write("Are you absolutely sure? (Type 'DELETE' to confirm): ");
        string confirm = Console.ReadLine().Trim().ToUpper();

        if (confirm != "DELETE")
        {
            ConsoleMG.WriteLine("Cancelled.");
            return;
        }

        // ===== BACKUP PROFILE TRƯỚC KHI XÓA =====
        string backupPath = BackupProfile(profile);

        // Xóa profile chính
        DeleteProfile();
        ConsoleMG.WriteLine("Profile deleted successfully.");

        // Thông báo backup nếu thành công
        if (!string.IsNullOrEmpty(backupPath))
        {
            ConsoleMG.WriteLine("Use 'setupprofile -rollback' to restore");
        }

        RestartApp();
    }

    static string BackupProfile(UserProfile profile)
    {
        try
        {
            // Tạo folder Temp/ProfileTemp/
            string profileTempFolder = Path.Combine(AppTemp.TempFolder, "ProfileTemp");
            if (!Directory.Exists(profileTempFolder))
            {
                Directory.CreateDirectory(profileTempFolder);
            }

            // Tên file với timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFile = Path.Combine(profileTempFolder, $"profile.{timestamp}.json");

            // Serialize profile (giữ nguyên encryption password)
            string json = System.Text.Json.JsonSerializer.Serialize(
                profile,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(backupFile, json);

            // Cleanup backups cũ (giữ 5 backup mới nhất)
            CleanupOldBackups(profileTempFolder);

            return backupFile;
        }
        catch
        {
            return null; // Backup fail nhưng vẫn xóa profile
        }
    }

    static void RollbackProfile()
    {
        if (ProfileExists())
        {
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine(" RESTORE BLOCKED!");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("You currently have an active profile.");
            ConsoleMG.WriteLine("Restoring an old profile would overwrite it.");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("To restore an old profile:");
            ConsoleMG.WriteLine("1. Delete current profile: 'setupprofile -r'");
            ConsoleMG.WriteLine("2. Then use 'setupprofile -rollback'");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("This prevents accidental data loss.");
            return;
        }

        // Check folder backup
        string profileTempFolder = Path.Combine(AppTemp.TempFolder, "ProfileTemp");

        if (!Directory.Exists(profileTempFolder))
        {
            ConsoleMG.WriteLine(" No backup folder found.");
            return;
        }

        // Get all backup files
        var backupFiles = Directory.GetFiles(profileTempFolder, "profile.*.json")
                                   .OrderByDescending(f => f)
                                   .ToArray();

        if (backupFiles.Length == 0)
        {
            ConsoleMG.WriteLine(" No backup files available.");
            return;
        }

        // Show backup list
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("═══════════════════════════════════════════");
        ConsoleMG.WriteLine("           AVAILABLE BACKUPS");
        ConsoleMG.WriteLine("═══════════════════════════════════════════");
        ConsoleMG.WriteLine();

        for (int i = 0; i < backupFiles.Length; i++)
        {
            try
            {
                string json = File.ReadAllText(backupFiles[i]);
                var profile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(json);

                if (profile != null)
                {
                    string fileName = Path.GetFileName(backupFiles[i]);
                    string dateStr = fileName.Replace("profile.", "").Replace(".json", "");

                    ConsoleMG.WriteLine($"[{i + 1}] {profile.DisplayName}");
                    ConsoleMG.WriteLine($"     Username: {profile.Username}");
                    ConsoleMG.WriteLine($"     Email: {(string.IsNullOrEmpty(profile.Email) ? "(empty)" : profile.Email)}");
                    ConsoleMG.WriteLine($"     Date: {dateStr.Insert(4, "-").Insert(7, "-").Replace("_", " ")}");
                    ConsoleMG.WriteLine();
                }
            }
            catch { /* Skip corrupted backups */ }
        }

        // Select backup
        ConsoleMG.Write($"Select backup to restore (1-{backupFiles.Length}, or 0 to cancel): ");

        if (!int.TryParse(Console.ReadLine(), out int choice) || choice == 0)
        {
            ConsoleMG.WriteLine(" Cancelled.");
            return;
        }

        if (choice < 1 || choice > backupFiles.Length)
        {
            ConsoleMG.WriteLine(" Invalid selection.");
            return;
        }

        string selectedBackup = backupFiles[choice - 1];

        try
        {
            // Load backup
            string json = File.ReadAllText(selectedBackup);
            var profile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(json);

            if (profile == null)
            {
                ConsoleMG.WriteLine(" Backup file is corrupted.");
                return;
            }

            // Final confirmation
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine($"  YOU ARE ABOUT TO RESTORE:");
            ConsoleMG.WriteLine($"   • Display Name: {profile.DisplayName}");
            ConsoleMG.WriteLine($"   • Username: {profile.Username}");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("  This will OVERWRITE current profile if exists!");
            ConsoleMG.Write("Type 'RESTORE' to confirm: ");

            string confirm = Console.ReadLine().Trim().ToUpper();

            if (confirm != "RESTORE")
            {
                ConsoleMG.WriteLine("❌ Cancelled.");
                return;
            }

            // Save restored profile
            SaveProfile(profile);
            ConsoleMG.WriteLine(" Profile restored successfully!");

            // Delete the restored backup (optional)
            try
            {
                File.Delete(selectedBackup);
                ConsoleMG.WriteLine(" Backup file removed.");
            }
            catch { /* Ignore delete errors */ }

            // Restart app
            ConsoleMG.WriteLine("🔄 Restarting application...");
            RestartApp();
        }
        catch (Exception ex)
        {
            ConsoleMG.WriteLine($"❌ Restore failed: {ex.Message}");
        }
    }

    static void CleanupOldBackups(string backupFolder, int keepCount = 5)
    {
        try
        {
            var backupFiles = Directory.GetFiles(backupFolder, "profile.*.json")
                                       .OrderByDescending(f => f)
                                       .ToList();

            if (backupFiles.Count > keepCount)
            {
                for (int i = keepCount; i < backupFiles.Count; i++)
                {
                    File.Delete(backupFiles[i]);
                }
            }
        }
        catch { /* Ignore cleanup errors */ }
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

                        if (key.Key == ConsoleKey.Enter)
            {
                ConsoleMG.WriteLine();
                break;
            }

                        if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    ConsoleMG.Write("\b \b");
                }
                continue;
            }

                        password.Append(key.KeyChar);
            ConsoleMG.Write("*");
        }

        return password.ToString();
    }


    static void DevDebugMode()
    {
        const string pass = "hycoredragondebug";

        ConsoleMG.Write("Enter password: ");
        string p1 = ReadPassword();

        ConsoleMG.Write("Re-enter password: ");
        string p2 = ReadPassword();

        if (p1 != pass || p2 != pass)
        {
            ConsoleMG.WriteLine("Access denied.");
            return;
        }

        ConsoleMG.WriteLine("Access granted.");

        DumpEmbeddedDll("converttool.dll", "convertTool.dll");
        DumpEmbeddedDll("converttool_shell.dll", "convertTool_shell.dll");

        ConsoleMG.WriteLine("Dump done.");
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

                    ConsoleMG.WriteLine($"{outputFile} dumped.");
                    return;
                }
            }

            ConsoleMG.WriteLine($"{outputFile} resource not found.");
        }
        catch (Exception ex)
        {
            ConsoleMG.WriteLine($"Dump failed: {ex.Message}");
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

            ConsoleMG.WriteLine("Added to Open With list.");
        }
        catch (Exception ex)
        {
            ConsoleMG.WriteLine("Failed to add Open With: " + ex.Message);
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
        }
    }

    static bool ExtractConsoleMGDll()
    {
        try
        {
            string installerTempFolder = Path.Combine(AppTemp.TempFolder, "installerTemp");

            if (!Directory.Exists(installerTempFolder))
            {
                Directory.CreateDirectory(installerTempFolder);
            }

            string flagFile = Path.Combine(installerTempFolder, "extracted.flag");

            if (File.Exists(flagFile))
            {
                return true;
            }

            string exeFolder = GetExeFolder();
            string outputPath = Path.Combine(exeFolder, "ConsoleMG.dll");

            var asm = typeof(Program).Assembly;
            using (Stream res = asm.GetManifestResourceStream("convertTool.ConsoleMG.dll"))
            {
                if (res == null)
                {
                    Console.WriteLine("❌ ERROR: ConsoleMG.dll not found in embedded resources!");
                    return false;
                }

                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                {
                    res.CopyTo(fs);
                }
            }

            File.WriteAllText(flagFile, $"Extracted at: {DateTime.Now}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR extracting ConsoleMG.dll: {ex.Message}");
            return false;
        }
    }

    static bool ExtractUpdate()
    {
        try
        {
            string installerTempFolder = Path.Combine(AppTemp.TempFolder, "installerTemp");

            if (!Directory.Exists(installerTempFolder))
            {
                Directory.CreateDirectory(installerTempFolder);
            }

            string flagFile = Path.Combine(installerTempFolder, "extracted_updater.flag");

            if (File.Exists(flagFile))
            {
                return true;
            }

            string exeFolder = GetExeFolder();
            string outputPath = Path.Combine(exeFolder, "convertTool_updater.exe");

            var asm = typeof(Program).Assembly;
            using (Stream res = asm.GetManifestResourceStream("convertTool.convertTool_updater.exe"))
            {
                if (res == null)
                {
                    Console.WriteLine("❌ ERROR: convertTool_updater.exe not found in embedded resources!");
                    return false;
                }

                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                {
                    res.CopyTo(fs);
                }
            }

            File.WriteAllText(flagFile, $"Extracted at: {DateTime.Now}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR extracting convertTool_updater.exe: {ex.Message}");
            return false;
        }
    }
    static void ProcessConversion(string[] args)
    {
        try
        {
            string inputPath = null;
            if (args.Length == 0)
            {
                ConsoleMG.WriteLine("No file selected.");

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
                ConsoleMG.WriteLine("File not found.");
                Pause();
                return;
            }

            ConsoleMG.WriteLine("=== ICON CONVERT TOOL ===");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("File: " + Path.GetFileName(inputPath));
            ConsoleMG.WriteLine();

                        ConsoleMG.WriteLine("Select mode:");
            ConsoleMG.WriteLine("1 - Single size");
            ConsoleMG.WriteLine("2 - Multi size (16,32,48,64,128,256)");
            ConsoleMG.WriteLine("3 - Custom size");
            ConsoleMG.WriteLine();

            ConsoleMG.Write("Choose: ");
            string choice = Console.ReadLine();

            List<int> sizes = new List<int>();

            if (choice == "1")
            {
                ConsoleMG.Write("Enter size: ");
                sizes.Add(int.Parse(Console.ReadLine()));
            }
            else if (choice == "2")
            {
                sizes.AddRange(new int[] { 16, 32, 48, 64, 128, 256 });
            }
            else if (choice == "3")
            {
                ConsoleMG.Write("Enter sizes (32,64,128): ");
                string[] input = Console.ReadLine().Split(',');
                foreach (var s in input)
                    sizes.Add(int.Parse(s.Trim()));
            }
            else
            {
                ConsoleMG.WriteLine("Invalid choice.");
                Pause();
                return;
            }

                        int count = 1;

            while (true)
            {
                ConsoleMG.WriteLine();
                ConsoleMG.Write("How many ICO files to create (1-20): ");

                if (!int.TryParse(Console.ReadLine(), out count))
                    count = 1;

                if (count < 1) count = 1;
                if (count > 20) count = 20;

                ConsoleMG.WriteLine();
                ConsoleMG.Write("Are you sure? (Y/N): ");
                string confirm = Console.ReadLine().Trim().ToUpper();

                if (confirm == "Y")
                    break;

                ConsoleMG.WriteLine("Re-selecting count...");
            }

            string historyFolder = GetHistoryFolder();
            string exeFolder = GetExeFolder();

            string folder;

            if (inputPath.StartsWith(historyFolder, StringComparison.OrdinalIgnoreCase))
            {
                                folder = exeFolder;
            }
            else
            {
                                folder = Path.GetDirectoryName(inputPath);
            }
            string name = Path.GetFileNameWithoutExtension(inputPath);

            using (Bitmap original = new Bitmap(inputPath))
            {
                ConsoleMG.WriteLine();
                ConsoleMG.WriteLine("Creating icons...");
                ConsoleMG.WriteLine();

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

                ConsoleMG.WriteLine();
                ConsoleMG.WriteLine("DONE ");
                SaveToHistory(inputPath);
            }

            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("DONE  Created " + count + " icon file(s).");
        }
        catch (Exception ex)
        {
            ConsoleMG.WriteLine("Error: " + ex.Message);
        }

        Pause();
    }

    static void ProcessImageConversion(string[] args)
    {
        try
        {
            string inputPath = null;

                        if (args.Length == 0)
            {
                ConsoleMG.WriteLine("No file selected.");
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
                ConsoleMG.WriteLine("File not found.");
                Pause();
                return;
            }

                        string ext = Path.GetExtension(inputPath).ToLower();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                ConsoleMG.WriteLine($"Unsupported file type: {ext}");
                ConsoleMG.WriteLine("Supported: .png, .jpg, .jpeg");
                Pause();
                return;
            }

                        ConsoleMG.WriteLine("=== IMAGE FORMAT CONVERTER ===");
            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("File: " + Path.GetFileName(inputPath));
            ConsoleMG.WriteLine($"Current format: {ext.ToUpper()}");
            ConsoleMG.WriteLine();

            ConsoleMG.WriteLine("Convert to:");

            if (ext == ".png")
            {
                ConsoleMG.WriteLine("1 - JPG (Compressed, smaller file size)");
            }
            else             {
                ConsoleMG.WriteLine("1 - PNG (Lossless, preserves quality)");
            }

            ConsoleMG.WriteLine("2 - Cancel");
            ConsoleMG.WriteLine();

            ConsoleMG.Write("Choose: ");
            string choice = Console.ReadLine();

            if (choice != "1")
            {
                ConsoleMG.WriteLine("Cancelled.");
                Pause();
                return;
            }

                        string outputExt = (ext == ".png") ? ".jpg" : ".png";
            string outputPath = Path.ChangeExtension(inputPath, outputExt);

            ConsoleMG.Write($"Output file [{Path.GetFileName(outputPath)}]: ");
            string customOutput = Console.ReadLine()?.Trim('"').Trim();

            if (!string.IsNullOrEmpty(customOutput))
            {
                                if (!customOutput.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !customOutput.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !customOutput.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    customOutput += outputExt;                 }
                outputPath = customOutput;
            }

                        int quality = 90;
            if (outputExt == ".jpg")
            {
                ConsoleMG.Write("Quality (1-100) [90]: ");
                string qualityStr = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(qualityStr) && int.TryParse(qualityStr, out int q))
                    quality = Math.Clamp(q, 1, 100);
            }

                        ConsoleMG.WriteLine();
            ConsoleMG.Write($"Convert {Path.GetFileName(inputPath)} → {Path.GetFileName(outputPath)}? (Y/N): ");
            string confirm = Console.ReadLine()?.Trim().ToUpper();

            if (confirm != "Y")
            {
                ConsoleMG.WriteLine("Cancelled.");
                Pause();
                return;
            }

                        ConsoleMG.WriteLine();
            ConsoleMG.WriteLine("Converting...");
            DrawProgressBar(0, 100);

            using (Bitmap original = new Bitmap(inputPath))
            {
                                string historyFolder = GetHistoryFolder();
                string exeFolder = GetExeFolder();
                string outputFolder;

                if (inputPath.StartsWith(historyFolder, StringComparison.OrdinalIgnoreCase))
                {
                                        outputFolder = exeFolder;
                }
                else
                {
                                        outputFolder = Path.GetDirectoryName(inputPath);
                }

                                string outputFileName = Path.GetFileName(outputPath);
                string finalOutputPath = Path.Combine(outputFolder, outputFileName);

                if (outputExt == ".jpg")
                {
                                        var encoder = ImageCodecInfo.GetImageEncoders()
                        .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

                    var encParams = new EncoderParameters(1);
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                    original.Save(finalOutputPath, encoder, encParams);
                }
                else
                {
                                        original.Save(finalOutputPath, ImageFormat.Png);
                }

                DrawProgressBar(100, 100);
                ConsoleMG.WriteLine();
                ConsoleMG.WriteLine("✅ Conversion complete!");
                outputPath = finalOutputPath;

                if (!inputPath.StartsWith(historyFolder, StringComparison.OrdinalIgnoreCase))
                {
                    SaveToHistory(inputPath);
                }
            }

            ConsoleMG.WriteLine();
            ConsoleMG.WriteLine($"Saved: {outputPath}");
            if (outputExt == ".jpg")
                ConsoleMG.WriteLine($"Quality: {quality}%");
        }
        catch (Exception ex)
        {
            ConsoleMG.WriteLine("Error: " + ex.Message);
        }

        Pause();
        RestartApp();
    }
    static void Main(string[] args)
    {
        ExtractConsoleMGDll();
        ExtractUpdate();

        string dllPath = Path.Combine(GetExeFolder(), "ConsoleMG.dll");
        if (!File.Exists(dllPath))
        {
            Console.WriteLine(" CRITICAL: ConsoleMG.dll missing!");
            Console.WriteLine("Please reinstall the application.");
            Pause();
            return;
        }
        ExtractShellTool();
        LockAllHistoryFiles();

        if (args.Length > 0 && args[0].ToLower() == "-info")
        {
            ShowCommandInfo();
            return;
        }

        ToolCLI(args);  

        if (IsFirstRun())
        {
            while (true)
            {
                ConsoleMG.WriteLine("Add convertTool to Open With list? (Y/N)");

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
                    ConsoleMG.WriteLine("Please press Y or N.");
                }
            }
        }

        ProcessConversion(args);
    }
    

    static void ShowCommandInfo()
    {
        ShowBanner();
        ConsoleMG.WriteLine("convertTool OK");
        ConsoleMG.WriteLine($"version: {AppInfo.tool_version}");
        ConsoleMG.WriteLine($"owner: {AppInfo.ownered_by}");
        ConsoleMG.WriteLine($"runtime: {AppInfo.runtime_info}");
        ConsoleMG.WriteLine($"copyright: {AppInfo.copyright_info}");
        ConsoleMG.WriteLine($"support: {AppInfo.discord_info}");
        ConsoleMG.WriteLine($"repository: {AppInfo.repository_info}");
        ConsoleMG.WriteLine($"license: {AppInfo.license_info}");
        ConsoleMG.WriteLine("legal: " + AppInfo.legal_info);
        ConsoleMG.WriteLine($"license url: {AppInfo.license_url}");
        ConsoleMG.WriteLine($"support info: {AppInfo.support_info}");
        ConsoleMG.WriteLine($"additional info: {AppInfo.additional_info}");
    }

    static void ShowCommandInfoWithCLI()
    {
        ConsoleMG.WriteLine("===========================================");
        ConsoleMG.WriteLine("              CONVERT TOOL INFO           ");
        ConsoleMG.WriteLine("===========================================");
        ConsoleMG.WriteLine($"version: {AppInfo.tool_version}");
        ConsoleMG.WriteLine($"owner: {AppInfo.ownered_by}");
        ConsoleMG.WriteLine($"runtime: {AppInfo.runtime_info}");
        ConsoleMG.WriteLine($"copyright: {AppInfo.copyright_info}");
        ConsoleMG.WriteLine($"support: {AppInfo.discord_info}");
        ConsoleMG.WriteLine($"repository: {AppInfo.repository_info}");
        ConsoleMG.WriteLine($"license: {AppInfo.license_info}");
        ConsoleMG.WriteLine("legal: " + AppInfo.legal_info);
        ConsoleMG.WriteLine($"license url: {AppInfo.license_url}");
        ConsoleMG.WriteLine($"support info: {AppInfo.support_info}");
        ConsoleMG.WriteLine($"additional info: {AppInfo.additional_info}");
        ConsoleMG.WriteLine("===========================================");
        ConsoleMG.WriteLine("End of Info");
        ConsoleMG.WriteLine("===========================================");
    }
    static void Pause()
    {
        ConsoleMG.WriteLine();
        ConsoleMG.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
