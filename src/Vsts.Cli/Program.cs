using System;
using System.IO;

namespace Vsts.Cli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            GitConfiguration gitConfiguration = GitRepoHelpers.Create(currentDirectory);

            if (gitConfiguration.GitDirectory != null && gitConfiguration.NonVstsHost)
            {
                Console.WriteLine($"Found a non-VSTS git repo at {gitConfiguration.GitDirectory} pointing to {gitConfiguration.Origin}", ConsoleColor.Yellow);
                Environment.Exit(0);
            }

            if (string.IsNullOrWhiteSpace(gitConfiguration.Name) || string.IsNullOrWhiteSpace(gitConfiguration.Host))
            {
                Console.WriteLine($"Could not find an existing VSTS git repo in the current {gitConfiguration.CurrentDirectory} directory or parent directories", ConsoleColor.Yellow);
                Environment.Exit(0);
            }

            Vsts vsts = new Vsts(gitConfiguration, gitConfiguration.CurrentDirectory, gitConfiguration.GitDirectory);

            VstsApiHelper vstsApiHelper = VstsService.CheckStatus(gitConfiguration, vsts);

            if (vstsApiHelper == null) return;

            VstsService.ProcessArgs(args, vsts, vstsApiHelper);
        }
    }
}
