// Program.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace convertTool_updater
{
    public class Program
    {
        public const string REPO_OWNER = "hoangbussines-commits";
        public const string REPO_NAME = "convertTool";

        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "convertTool Updater";
                Console.OutputEncoding = Encoding.UTF8;

                ShowBanner();

                // Get current version from main app
                Version currentVersion = GetCurrentAppVersion();
                // Get latest version from GitHub
                var latestInfo = GetLatestReleaseInfo();
                if (latestInfo == null)
                {
                    Console.WriteLine("❌ Cannot fetch latest release info.");
                    Pause();
                    return;
                }

                Version latestVersion = ParseVersion(latestInfo.TagName);
                Console.WriteLine($"Latest version: v{latestVersion}");
                Console.WriteLine();

                // Version comparison
                if (latestVersion <= currentVersion)
                {
                    Console.WriteLine("✅ You already have the latest version!");
                    Pause();
                    return;
                }

                if (latestVersion < currentVersion)
                {
                    Console.WriteLine("⚠  Warning: Latest release is OLDER than current.");
                    Console.WriteLine("   You might be using a beta/development build.");
                    Console.Write("   Force downgrade? (Type 'DOWNGRADE'): ");

                    if (Console.ReadLine().Trim().ToUpper() != "DOWNGRADE")
                    {
                        Console.WriteLine("❌ Update cancelled.");
                        Pause();
                        return;
                    }
                }

                // Ask for main app PID
                Console.Write("Enter main app Process ID (or 0 if already closed): ");
                if (int.TryParse(Console.ReadLine(), out int pid) && pid > 0)
                {
                    WaitForProcessExit(pid);
                }

                // Confirm update
                Console.WriteLine();
                Console.WriteLine($"📥 Ready to update: v{currentVersion} → v{latestVersion}");
                Console.Write("Continue? (Y/N): ");

                if (Console.ReadLine().Trim().ToUpper() != "Y")
                {
                    Console.WriteLine("❌ Update cancelled.");
                    Pause();
                    return;
                }

                // Download and update
                Console.WriteLine();
                Console.WriteLine("🔄 Downloading update...");
                bool success = DownloadAndUpdate(latestInfo);

                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ Update successful!");
                    Console.WriteLine("🚀 Launching updated app...");

                    // Start updated app
                    Thread.Sleep(1000);
                    Process.Start("convertTool.exe");
                }
                else
                {
                    Console.WriteLine("❌ Update failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Pause();
        }

        static void ShowBanner()
        {
            Console.WriteLine();
            Console.WriteLine(@"╔══════════════════════════════════════╗");
            Console.WriteLine(@"║       CONVERTTOOL UPDATER v1.0       ║");
            Console.WriteLine(@"╚══════════════════════════════════════╝");
            Console.WriteLine();
        }

        static Version GetCurrentAppVersion()
        {
            // CRITICAL CHECK: Main app must exist
            if (!File.Exists("convertTool.exe"))
            {
                Console.WriteLine();
                Console.WriteLine("❌ ERROR: convertTool.exe not found!");
                Console.WriteLine("   Updater must be in same folder as main app.");
                throw new FileNotFoundException("convertTool.exe not found in current directory.");
            }

            try
            {
                // Try to get version via -info command
                var psi = new ProcessStartInfo
                {
                    FileName = "convertTool.exe",
                    Arguments = "-info",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (var p = Process.Start(psi))
                {
                    if (p == null)
                        throw new Exception("Failed to start main app process.");

                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(10000); // 10 second timeout

                    if (p.ExitCode != 0)
                        throw new Exception($"Main app exited with code: {p.ExitCode}");

                    // Parse output for version
                    foreach (var line in output.Split('\n'))
                    {
                        if (line.TrimStart().StartsWith("version:", StringComparison.OrdinalIgnoreCase))
                        {
                            string versionStr = line.Split(':')[1].Trim();
                            Console.WriteLine($"✓ Current version: {versionStr}");
                            return ParseVersion(versionStr);
                        }
                    }

                    throw new Exception("Version not found in app output.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"❌ FAILED TO GET VERSION: {ex.Message}");
                Console.WriteLine("   Main app may be corrupted or incompatible.");
                throw; // Re-throw để Main() catch
            }
        }

        static GitHubRelease GetLatestReleaseInfo()
        {
            string url = $"https://api.github.com/repos/{REPO_OWNER}/{REPO_NAME}/releases/latest";

            Console.WriteLine($"🔗 Connecting to: {url}");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("convertTool-Updater");
                    client.Timeout = TimeSpan.FromSeconds(30);

                    Console.WriteLine("🌐 Sending request to GitHub API...");

                    var response = client.GetAsync(url).Result;

                    Console.WriteLine($"📡 Response: {(int)response.StatusCode} {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"❌ GitHub API error: {response.StatusCode}");
                        string errorContent = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine($"   Details: {errorContent}");
                        return null;
                    }

                    string json = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"✅ Received data ({json.Length} bytes)");
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    };

                    var release = JsonSerializer.Deserialize<GitHubRelease>(json, options);

                    if (release == null)
                    {
                        Console.WriteLine("❌ Failed to parse GitHub response.");
                        return null;
                    }

                    Console.WriteLine($"📦 Release object: {release != null}");
                    Console.WriteLine($"   TagName: '{release.TagName}'");
                    Console.WriteLine($"   Name: '{release.Name}'");
                    Console.WriteLine($"   HtmlUrl: '{release.HtmlUrl}'");
                    Console.WriteLine($"   Prerelease: {release.Prerelease}");
                    Console.WriteLine($"   PublishedAt: {release.PublishedAt}");

                    if (string.IsNullOrEmpty(release.TagName))
                    {
                        Console.WriteLine("❌ ERROR: Release has no TagName!");
                        Console.WriteLine("   Full JSON response:");
                        Console.WriteLine(json);
                        return null;
                    }

                    Console.WriteLine($"📦 Found release: {release.TagName}");
                    return release;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Network error: {ex.Message}");
                Console.WriteLine("   Check internet connection.");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("❌ Request timeout.");
                Console.WriteLine("   GitHub might be slow or blocked.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
            }

            return null;
        }

        static bool DownloadAndUpdate(GitHubRelease release)
        {
            try
            {
                // 1. Tìm asset cần download
                var asset = FindDownloadAsset(release);
                if (asset == null)
                {
                    Console.WriteLine("❌ No suitable download asset found.");
                    return false;
                }

                Console.WriteLine($"📥 Asset: {asset.Name} ({asset.Size / 1024 / 1024} MB)");

                // 2. Download file
                string tempFile = DownloadAsset(asset);
                if (tempFile == null)
                    return false;

                // 3. Backup current files (optional)
                BackupCurrentVersion();

                // 4. Delete old files
                Console.WriteLine("🗑️  Removing old files...");
                DeleteOldFiles();

                // 5. Extract/Copy new files
                Console.WriteLine("🔄 Installing new version...");
                bool extracted = ExtractUpdate(tempFile);

                // 6. Cleanup temp file
                File.Delete(tempFile);

                return extracted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Update failed: {ex.Message}");
                return false;
            }
        }

        static void DeleteOldFiles()
        {
            string[] filesToDelete =
            {
        "convertTool.exe",
        "convertTool_shell.exe",
        "ConsoleMG.dll",
        "convertTool.dll",
        "convertTool.pdb",
        "firstrun.flag"
    };

            string[] foldersToKeep =
            {
        "Temp",        // <-- NHƯNG SẼ XÓA SAU!
        "profile",
        "convertHistory"
    };

            // 1. Xóa files
            foreach (var file in filesToDelete)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        Console.WriteLine($"   Deleted: {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Cannot delete {file}: {ex.Message}");
                }
            }

            // 2. XÓA TEMP FOLDER LUÔN!
            try
            {
                if (Directory.Exists("Temp"))
                {
                    Directory.Delete("Temp", true);
                    Console.WriteLine("   🗑️  Deleted: Temp folder");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠  Cannot delete Temp: {ex.Message}");
            }

            Console.WriteLine("✅ Old files removed.");
        }

        static GitHubAsset FindDownloadAsset(GitHubRelease release)
        {
            if (release.Assets == null || release.Assets.Count == 0)
            {
                Console.WriteLine("❌ Release has no assets.");
                return null;
            }

            // Ưu tiên .exe > .zip > others
            var exeAsset = release.Assets.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

            if (exeAsset != null)
                return exeAsset;

            var zipAsset = release.Assets.FirstOrDefault(a =>
                a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            if (zipAsset != null)
                return zipAsset;

            // Fallback: bất kỳ asset nào
            return release.Assets[0];
        }

        static string DownloadAsset(GitHubAsset asset)
        {
            string tempFile = Path.GetTempFileName() + Path.GetExtension(asset.Name);

            Console.WriteLine($"🌐 Downloading from: {asset.BrowserDownloadUrl}");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5); // Timeout dài cho large file

                using (var stream = client.GetStreamAsync(asset.BrowserDownloadUrl).Result)
                using (var fileStream = File.Create(tempFile))
                {
                    // Progress bar
                    long totalBytes = asset.Size;
                    long bytesRead = 0;
                    byte[] buffer = new byte[8192];
                    int bytesReadThisTime;

                    Console.Write("   [");
                    int progressBarWidth = 30;

                    while ((bytesReadThisTime = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesReadThisTime);
                        bytesRead += bytesReadThisTime;

                        // Update progress
                        double percent = (double)bytesRead / totalBytes;
                        int filled = (int)(percent * progressBarWidth);

                        Console.Write("\r   [");
                        Console.Write(new string('█', filled));
                        Console.Write(new string('░', progressBarWidth - filled));
                        Console.Write($"] {(percent * 100):0}%");
                    }

                    Console.WriteLine();
                }
            }

            Console.WriteLine($"✅ Downloaded: {new FileInfo(tempFile).Length / 1024 / 1024} MB");
            return tempFile;
        }

        static bool ExtractUpdate(string archivePath)
        {
            string extension = Path.GetExtension(archivePath).ToLower();

            if (extension == ".exe")
            {
                // Nếu là .exe file đơn lẻ
                File.Copy(archivePath, "convertTool.exe", true);
                Console.WriteLine("✅ Copied convertTool.exe");
                return true;
            }
            else if (extension == ".zip")
            {
                // Extract zip
                System.IO.Compression.ZipFile.ExtractToDirectory(
                    archivePath,
                    ".",
                    true // overwrite
                );
                Console.WriteLine("✅ Extracted zip archive");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Unsupported file type: {extension}");
                return false;
            }
        }

        static void BackupCurrentVersion()
        {
            try
            {
                string backupFolder = Path.Combine("Temp", "Backup", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(backupFolder);

                string[] filesToBackup =
                {
            "convertTool.exe",
            "convertTool_shell.exe",
            "ConsoleMG.dll"
        };

                foreach (var file in filesToBackup)
                {
                    if (File.Exists(file))
                    {
                        File.Copy(file, Path.Combine(backupFolder, file));
                    }
                }

                Console.WriteLine($"💾 Backup created: {backupFolder}");
            }
            catch
            {
                Console.WriteLine("⚠  Backup skipped (non-critical)");
            }
        }




        static void WaitForProcessExit(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                Console.WriteLine($"⏳ Waiting for process {pid} to exit...");
                process.WaitForExit();
                Console.WriteLine("✅ Process exited.");
            }
            catch
            {
                // Process already exited
            }
        }

        static Version ParseVersion(string versionStr)
        {
            // Remove 'v' prefix
            string clean = versionStr.Trim().TrimStart('v', 'V');

            // Remove pre-release suffix (e.g., "-beta", "-alpha", "-rc1")
            int dashIndex = clean.IndexOf('-');
            if (dashIndex >= 0)
            {
                clean = clean.Substring(0, dashIndex);
            }

            // Parse numbers only
            var parts = clean.Split('.');

            int major = parts.Length > 0 ? ParseIntSafe(parts[0]) : 0;
            int minor = parts.Length > 1 ? ParseIntSafe(parts[1]) : 0;
            int build = parts.Length > 2 ? ParseIntSafe(parts[2]) : 0;
            int revision = parts.Length > 3 ? ParseIntSafe(parts[3]) : 0;

            if (revision > 0)
                return new Version(major, minor, build, revision);
            else
                return new Version(major, minor, build);
        }

        static int ParseIntSafe(string str)
        {
            // Extract only digits
            string digits = new string(str.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
                return 0;
            return int.Parse(digits);
        }

        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    // GitHub API response classes
    public class GitHubRelease
    {
        public string TagName { get; set; }
        public string Name { get; set; }
        public string HtmlUrl { get; set; }
        public string Body { get; set; }
        public bool Prerelease { get; set; }
        public DateTime PublishedAt { get; set; }
        public List<GitHubAsset> Assets { get; set; }
    }

    public class GitHubAsset
    {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
        public long Size { get; set; }
    }
}