using System;
using System.IO;
using System.Reflection;

public static class ConsoleMG
{
        private static Action<string> _writeLine;
    private static Action<string> _write;
    private static Action<string, object[]> _writeLineFormat;
    private static Action _writeLineEmpty;

        private static bool _dllLoaded = false;
    private static Assembly _consoleMGAssembly;

    static ConsoleMG()
    {
        Initialize();
    }

    private static void Initialize()
    {
        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleMG.dll");

        if (File.Exists(dllPath))
        {
            try
            {
                                _consoleMGAssembly = Assembly.LoadFrom(dllPath);
                var consoleMGType = _consoleMGAssembly.GetType("Hycore.ConsoleMG.ConsoleManager");

                if (consoleMGType != null)
                {
                                        var writeLineMethod = consoleMGType.GetMethod("WriteLine", new[] { typeof(string) });
                    var writeMethod = consoleMGType.GetMethod("Write", new[] { typeof(string) });
                    var writeLineFormatMethod = consoleMGType.GetMethod("WriteLine", new[] { typeof(string), typeof(object[]) });

                    if (writeLineMethod != null && writeMethod != null)
                    {
                        _writeLine = (text) => writeLineMethod.Invoke(null, new object[] { text });
                        _write = (text) => writeMethod.Invoke(null, new object[] { text });

                        if (writeLineFormatMethod != null)
                        {
                            _writeLineFormat = (format, args) => writeLineFormatMethod.Invoke(null, new object[] { format, args });
                        }

                        _dllLoaded = true;
                        Console.WriteLine("[ConsoleMG] DLL loaded successfully.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsoleMG] DLL load failed: {ex.Message}");
            }
        }

                SetupFallback();
        Console.WriteLine("[ConsoleMG] Using built-in fallback.");
    }

    private static void SetupFallback()
    {
        _writeLine = Console.WriteLine;
        _write = Console.Write;
        _writeLineFormat = (format, args) => Console.WriteLine(format, args);
        _writeLineEmpty = Console.WriteLine;
        _dllLoaded = false;
    }

        public static void WriteLine()
    {
        if (_writeLineEmpty != null)
            _writeLineEmpty();
        else
            Console.WriteLine();
    }

    public static void WriteLine(string value)
    {
        _writeLine?.Invoke(value);
    }

    public static void WriteLine(string format, params object[] args)
    {
        if (_writeLineFormat != null)
            _writeLineFormat(format, args);
        else
            Console.WriteLine(format, args);
    }

    public static void Write(string value)
    {
        _write?.Invoke(value);
    }

    public static void Write(string format, params object[] args)
    {
        Console.Write(format, args);
    }

        public static bool IsDllLoaded => _dllLoaded;
    public static string GetDllVersion()
    {
        if (!_dllLoaded || _consoleMGAssembly == null)
            return "Built-in fallback";

        var version = _consoleMGAssembly.GetName().Version;
        return version?.ToString() ?? "Unknown";
    }
    public static void DebugTest()
    {
        ConsoleMG.WriteLine($"[ConsoleMG Debug] DLL Loaded: {_dllLoaded}");
        ConsoleMG.WriteLine($"[ConsoleMG Debug] _writeLine is null: {_writeLine == null}");
        ConsoleMG.WriteLine($"[ConsoleMG Debug] Type: {_consoleMGAssembly?.FullName}");
    }
}