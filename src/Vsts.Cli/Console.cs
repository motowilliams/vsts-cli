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

        public static void Logo()
        {
            Console.WriteLine(@"    _  _  ____  ____  ____     ___  __    __ ", ConsoleColor.Blue);
            Console.WriteLine(@"   / )( \/ ___)(_  _)/ ___)   / __)(  )  (  )", ConsoleColor.Blue);
            Console.WriteLine(@"   \ \/ /\___ \  )(  \___ \  ( (__ / (_/\ )( ", ConsoleColor.Blue);
            Console.WriteLine(@"    \__/ (____/ (__) (____/   \___)\____/(__)", ConsoleColor.Blue);
            Console.Write(@"Visual Studio Team Service Command Line Interface", ConsoleColor.Blue);
        }
    }
}