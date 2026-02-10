using System;

namespace Hycore.ConsoleMG
{
    public static class ConsoleManager
    {
        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void Write(string value)
        {
            Console.Write(value);
        }

        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }
    }
}