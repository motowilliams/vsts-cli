using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Vsts.Cli
{
    public class Vsts
    {
        private readonly GitConfiguration _gitConfiguration;
        private readonly string _currentDirectory;
        private readonly string _closestGitDirectory;

        private static string UserprofileDirectory => Environment.GetEnvironmentVariable("USERPROFILE");
        private static string ConfigDirectory => Path.Combine(UserprofileDirectory, ".config");
        private static string VstsCliConfigPath => Path.Combine(ConfigDirectory, "vsts-cli");

        private List<VstsConfiguration> _configuration { get; set; }

        public Vsts(GitConfiguration gitConfiguration, string currentDirectory, string closestGitDirectory)
        {
            _gitConfiguration = gitConfiguration;
            _currentDirectory = currentDirectory;
            _closestGitDirectory = closestGitDirectory;

            if (File.Exists(VstsCliConfigPath))
            {
                var readAllText = File.ReadAllText(VstsCliConfigPath);
                _configuration = JsonConvert.DeserializeObject<List<VstsConfiguration>>(readAllText);
            }
            else
            {
                _configuration = new List<VstsConfiguration>();
            }
        }

        private VstsConfiguration ActiveConfiguration => _configuration.FirstOrDefault(c =>
        {
            RepositoryRegistration repositoryRegistration = c.RepositoryRegistrations.FirstOrDefault(repo => repo.Directory.Equals(_closestGitDirectory, StringComparison.OrdinalIgnoreCase));
            return repositoryRegistration != null;
        });

        public bool IsEmpty => !_configuration.Any();
        public bool IsInProject => _configuration.Any(x => x.AccountName.Equals(_gitConfiguration.Host, StringComparison.OrdinalIgnoreCase));
        public bool NeedsAccessToken => _configuration.FirstOrDefault(x => x.AccountName.Equals(_gitConfiguration.Host, StringComparison.OrdinalIgnoreCase))?.PersonalAccessToken == null;
        public bool LocalDirectoryLinked => ActiveConfiguration != null;
        public string Host => _gitConfiguration.Host;
        public string AccountName => _gitConfiguration.Host;
        public string ProjectName => _configuration.Single(x => x.AccountName.Equals(_gitConfiguration.Host, StringComparison.OrdinalIgnoreCase)).ProjectName;
        public string PersonalAccessToken => _configuration.Single(x => x.AccountName.Equals(_gitConfiguration.Host, StringComparison.OrdinalIgnoreCase)).PersonalAccessToken;
        public string RepositoryName => _gitConfiguration.Name;
        public string RepositoryBranchName => _gitConfiguration.CurrentBranch;
        public string RepositoryId => ActiveConfiguration.RepositoryRegistrations.Single(x => x.Directory.Equals(_closestGitDirectory)).RepositoryId;

        public Uri PullRequestIdUri(int pullRequestId)
            => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_git/{RepositoryName}/pullrequest/{pullRequestId}");
        public Uri PullRequestUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_git/{RepositoryName}/pullrequests?_a=active");
        public Uri AccountUri => new Uri($"https://{Host}.visualstudio.com");
        public Uri WorkItemsUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_backlogs");
        public Uri ProjectUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}");
        public Uri CodeUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_git/{RepositoryName}");
        public Uri CodeBranchUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_git/{RepositoryName}?version=GB{RepositoryBranchName}&_a=contents");
        public Uri BuildsUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_build?_a=allDefinitions");
        public Uri ReleasesUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_release");
        public Uri TestManagementUri => new Uri($"https://{Host}.visualstudio.com/{ProjectName}/_testManagement");

        public void AddLocalDirectoryLink(string repositoryName, string repositoryId)
        {
            _configuration.Single(x => x.AccountName.Equals(_gitConfiguration.Host, StringComparison.OrdinalIgnoreCase))
                .RepositoryRegistrations.Add(new RepositoryRegistration(repositoryName, repositoryId, _closestGitDirectory));

            var json = JsonConvert.SerializeObject(_configuration, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public string Credentials => Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format($"{String.Empty}:{PersonalAccessToken}")));

        private void Save()
        {

        }
    }

    public class VstsConfiguration
    {
        public string AccountName { get; set; }
        public string PersonalAccessToken { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUri { get; set; }
        public List<RepositoryRegistration> RepositoryRegistrations { get; set; }

        public static string Credentials(string token) => Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format($"{String.Empty}:{token}")));
    }

    public class RepositoryRegistration
    {
        public RepositoryRegistration(string repositoryName, string repositoryId, string directory)
        {
            RepositoryName = repositoryName;
            RepositoryId = repositoryId;
            Directory = directory;
        }

        public string RepositoryName { get; }
        public string RepositoryId { get; }
        public string Directory { get; }
    }

}