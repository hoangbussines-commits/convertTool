public static class AppTemp
{
    private static string _tempFolder;

    public static string TempFolder
    {
        get
        {
            if (_tempFolder == null)
            {
                string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
                _tempFolder = Path.Combine(exeFolder, "Temp");

                if (!Directory.Exists(_tempFolder))
                {
                    Directory.CreateDirectory(_tempFolder);
                }
            }
            return _tempFolder;
        }
    }
}