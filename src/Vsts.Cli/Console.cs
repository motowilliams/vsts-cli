using System;

namespace Vsts.Cli
{
    public static class Console
    {
        public static void WriteLine(string source)
        {
            System.Console.WriteLine(source);
        }

        public static void Write(string source)
        {
            System.Console.WriteLine(source);
        }

        public static void WriteLine(string source, ConsoleColor color)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(source);
            System.Console.ResetColor();
        }

        public static void Write(string source, ConsoleColor color)
        {
            System.Console.ForegroundColor = color;
            System.Console.Write(source);
            System.Console.ResetColor();
        }

        public static string ReadLine()
        {
            return System.Console.ReadLine();
        }
    }
}