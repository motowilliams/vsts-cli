using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Vsts.Cli
{
    public class GitConfiguration
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public bool NonVstsHost => Origin == null || !Origin.Contains("visualstudio.com");
        public string CurrentBranch { get; set; }
    }

    public static class GitRepoHelpers
    {
        public static string FindDirectory(string directory, bool searchParent = false)
        {
            var directoryInfo = new DirectoryInfo(directory);
            var gitPath = Path.Combine(directoryInfo.FullName, ".git");
            if (Directory.Exists(gitPath))
            {
                return gitPath;
            }

            if (!searchParent) return null;

            bool checkparent = directoryInfo.Parent != null;
            if (checkparent)
                directory = directoryInfo.Parent.FullName;

            return FindDirectory(directory, checkparent);
        }

        public static GitConfiguration Create(string directory)
        {
            GitConfiguration configuration = new GitConfiguration();

            if (!Directory.Exists(directory)) return configuration;

            using (var repo = new LibGit2Sharp.Repository(directory))
            {
                //For VSTS the repository 'name' is the last segment without a .git extension
                Remote origin = repo.Network.Remotes.Single(x => x.Name.Equals("origin"));
                configuration.Origin = origin.Url.ToLower();

                if (configuration.NonVstsHost)
                    return configuration;

                configuration.Name = origin.Url.Split('/').Last();
                configuration.Host = new Uri(origin.Url).Host.Split('.').First();
                configuration.CurrentBranch = repo.Head.FriendlyName;
            }

            return configuration;
        }

        public static string Name(string directory)
        {
            var path = Path.Combine(directory, ".git");
            if (Directory.Exists(path))
            {
                using (var repo = new LibGit2Sharp.Repository(path))
                {
                    //For VSTS the repository 'name' is the last segment without a .git extension
                    var origin = repo.Network.Remotes.Single(x => x.Name.Equals("origin"));
                    return origin.Url.Split('/').Last();
                }
            }
            return null;
        }

        public static Uri Origin(string directory)
        {
            var path = Path.Combine(directory, ".git");
            if (Directory.Exists(path))
            {
                using (var repo = new LibGit2Sharp.Repository(path))
                {
                    //For VSTS the repository 'name' is the last segment without a .git extension
                    var origin = repo.Network.Remotes.Single(x => x.Name.Equals("origin"));
                    return new Uri(origin.Url);
                }
            }
            return null;
        }

        public static string ServerHost(string directory)
        {
            var origin = Origin(directory);
            return origin.Host.Split('.').First();
        }
    }
}