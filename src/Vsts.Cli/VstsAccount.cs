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

        private List<VstsAccount> Accounts { get; set; }

        public Vsts(GitConfiguration gitConfiguration, string currentDirectory, string closestGitDirectory)
        {
            _gitConfiguration = gitConfiguration;
            _currentDirectory = currentDirectory;
            _closestGitDirectory = closestGitDirectory;

            Accounts = File.Exists(VstsCliConfigPath)
                ? JsonConvert.DeserializeObject<List<VstsAccount>>(File.ReadAllText(VstsCliConfigPath))
                : new List<VstsAccount>();
        }

        private VstsAccount ActiveAccount => Accounts.FirstOrDefault(x => x.HasRegisteredGitDirectory(_closestGitDirectory));

        /// <summary>
        /// Indicates if the currently directory can be linked to a VSTS project
        /// </summary>
        public bool IsInProject => ActiveAccount != null;

        public bool NeedsAccessToken => !Accounts.Any(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase));

        public bool LocalDirectoryLinked => ActiveAccount != null;
        public string GitHost => _gitConfiguration.Host;
        public string AccountName => ActiveAccount.AccountName;
        public string ProjectName => ActiveAccount.CurrentProject(_closestGitDirectory).Name;
        public string PersonalAccessToken => Accounts.FirstOrDefault(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase))?.PersonalAccessToken;
        public string RepositoryName => _gitConfiguration.Name;
        public string RepositoryBranchName => _gitConfiguration.CurrentBranch;
        public string RepositoryId => ActiveAccount.CurrentProject(_closestGitDirectory).GetCurrentRepositoryId(_closestGitDirectory);

        //Common VSTS Project Uris
        public Uri PullRequestIdUri(int pullRequestId) => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_git/{RepositoryName}/pullrequest/{pullRequestId}");
        public Uri PullRequestUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_git/{RepositoryName}/pullrequests?_a=active");
        public Uri AccountUri => new Uri($"https://{GitHost}.visualstudio.com");
        public Uri WorkItemUri(int id) => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_workitems?id={id}");
        public Uri WorkItemsUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_backlogs");
        public Uri ProjectUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}");
        public Uri CodeUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_git/{RepositoryName}");
        public Uri CodeBranchUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_git/{RepositoryName}?version=GB{RepositoryBranchName}&_a=contents");
        public Uri BuildsUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_build?_a=allDefinitions");
        public Uri ReleasesUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_release");
        public Uri TestManagementUri => new Uri($"https://{GitHost}.visualstudio.com/{ProjectName}/_testManagement");

        public void CreateNewConfiguration(string repositoryName, string repositoryId, string projectName, string projectId, string personalAccessToken)
        {
            var vstsProject = new VstsProject
            {
                Name = projectName,
                Id = projectId,
                Repositories = new List<RepositoryRegistration>
                {
                    new RepositoryRegistration
                    {
                        Name = repositoryName,
                        Id = repositoryId,
                        Directory = _closestGitDirectory
                    }
                }
            };

            //check to see if this account is in the configuration
            var configuration = Accounts.FirstOrDefault(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase));
            if (configuration == null)
            {
                configuration = new VstsAccount
                {
                    AccountName = _gitConfiguration.Host,
                    PersonalAccessToken = personalAccessToken,
                    Projects = new List<VstsProject>
                    {
                        vstsProject
                    }
                };
                Accounts.Add(configuration);
            }
            else
            {
                var existingProject = configuration.Projects.FirstOrDefault(x => x.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
                if (existingProject == null)
                    configuration.Projects.Add(vstsProject);
                else
                    existingProject.Repositories.Add(new RepositoryRegistration { Directory = _closestGitDirectory, Id = repositoryId, Name = repositoryName });
            }

            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public void AddLocalDirectoryLink(string repositoryName, string repositoryId)
        {
            VstsProject currentProject = ActiveAccount.CurrentProject(_closestGitDirectory);
            currentProject.Repositories.Add(new RepositoryRegistration
            {
                Name = repositoryName,
                Id = repositoryId,
                Directory = _closestGitDirectory
            });

            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public void SetAccessToken(string personalAccessToken)
        {
            //check to see if this account is in the configuration
            var configuration = Accounts.FirstOrDefault(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase));

            // Don't stomp on the existing account
            if (configuration != null) return;

            configuration = new VstsAccount
            {
                AccountName = _gitConfiguration.Host,
                PersonalAccessToken = personalAccessToken,
                Projects = new List<VstsProject>()
            };
            Accounts.Add(configuration);
            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }
    }

    public class VstsAccount
    {
        public string AccountName { get; set; }
        public string PersonalAccessToken { get; set; }
        public List<VstsProject> Projects { get; set; }

        public VstsProject CurrentProject(string gitDirectory) => Projects.SingleOrDefault(x => x.Name == x.GetProjectNameForDirectory(gitDirectory));

        public bool HasRegisteredGitDirectory(string gitDirectory) => Projects.SelectMany(x => x.Repositories).Any(x => x.Matches(gitDirectory));
    }

    public class VstsProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<RepositoryRegistration> Repositories { get; set; }

        public string GetProjectNameForDirectory(string gitDirectory)
        {
            return Repositories.Any(x => x.Directory.Equals(gitDirectory, StringComparison.OrdinalIgnoreCase)) ? Name : null;
        }

        public string GetCurrentRepositoryId(string gitDirectory) => Repositories
            .FirstOrDefault(x => x.Directory.Equals(gitDirectory, StringComparison.OrdinalIgnoreCase)).Id;
    }

    public class RepositoryRegistration
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Directory { get; set; }

        public bool Matches(string directory) => Directory.Equals(directory, StringComparison.OrdinalIgnoreCase);
    }
}
