using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Vsts.Cli
{
    public class Vsts : IVsts
    {
        private readonly GitConfiguration _gitConfiguration;

        private static string UserprofileDirectory => Environment.GetEnvironmentVariable("USERPROFILE");
        private static string ConfigDirectory => Path.Combine(UserprofileDirectory, ".config");
        private static string VstsCliConfigPath => Path.Combine(ConfigDirectory, "vsts-cli");

        private List<VstsAccount> Accounts { get; set; }

        public Vsts(GitConfiguration gitConfiguration)
        {
            _gitConfiguration = gitConfiguration;

            if (File.Exists(VstsCliConfigPath))
            {
                var readAllText = File.ReadAllText(VstsCliConfigPath);
                Accounts = readAllText.Any() ? JsonConvert.DeserializeObject<List<VstsAccount>>(readAllText) : new List<VstsAccount>();
            }
            else
            {
                Accounts = new List<VstsAccount>();
            }
        }

        private VstsAccount ActiveAccount => Accounts.FirstOrDefault(x => x.HasRegisteredGitDirectory(_gitConfiguration.GitDirectory));

        /// <summary>
        /// Indicates if the currently directory can be linked to a VSTS project
        /// </summary>
        public bool IsInProject => ActiveAccount != null;

        public bool NeedsAccessToken => !Accounts.Any(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase));

        public bool LocalDirectoryLinked => ActiveAccount != null;
        public string GitHost => _gitConfiguration.Host;
        public string AccountName => ActiveAccount.AccountName;
        public string FullName => ActiveAccount.FullName;
        public string ProjectName => ActiveAccount.CurrentProject(_gitConfiguration.GitDirectory).Name;
        public string PersonalAccessToken => Accounts.FirstOrDefault(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase))?.PersonalAccessToken;
        public string RepositoryName => _gitConfiguration.Name;
        public string RepositoryBranchName => _gitConfiguration.CurrentBranch;
        public string LastCommit => GitRepoHelpers.LastCommitMessage(_gitConfiguration.GitDirectory, 1);
        public string RepositoryId => ActiveAccount.CurrentProject(_gitConfiguration.GitDirectory).GetCurrentRepositoryId(_gitConfiguration.GitDirectory);

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
                        Directory = _gitConfiguration.GitDirectory
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
                    existingProject.Repositories.Add(new RepositoryRegistration { Directory = _gitConfiguration.GitDirectory, Id = repositoryId, Name = repositoryName });
            }

            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public void AddLocalDirectoryLink(string repositoryName, string repositoryId)
        {
            VstsProject currentProject = ActiveAccount.CurrentProject(_gitConfiguration.GitDirectory);
            currentProject.Repositories.Add(new RepositoryRegistration
            {
                Name = repositoryName,
                Id = repositoryId,
                Directory = _gitConfiguration.GitDirectory
            });

            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public void SetAccountInfo(string personalAccessToken, string fullName)
        {
            //check to see if this account is in the configuration
            var configuration = Accounts.FirstOrDefault(x => x.AccountName.Equals(GitHost, StringComparison.OrdinalIgnoreCase));

            // Don't stomp on the existing account
            if (configuration != null) return;

            configuration = new VstsAccount
            {
                AccountName = _gitConfiguration.Host,
                PersonalAccessToken = personalAccessToken,
                FullName = fullName,
                Projects = new List<VstsProject>()
            };
            Accounts.Add(configuration);
            var json = JsonConvert.SerializeObject(Accounts, Formatting.Indented);
            File.WriteAllText(VstsCliConfigPath, json);
        }

        public void BrowseCodeUri()
        {
            CodeUri.Browse();
        }
        public void BrowseCodeBranchUri()
        {
            CodeBranchUri.Browse();
        }

        public void BrowseBuildsUri()
        {
            BuildsUri.Browse();
        }

        public void BrowseReleasesUri()
        {
            ReleasesUri.Browse();
        }

        public void BrowseWorkItemsUri()
        {
            WorkItemsUri.Browse();
        }

        public void BrowsePullRequestUri()
        {
            PullRequestUri.Browse();
        }

        public void BrowseTestManagementUri()
        {
            TestManagementUri.Browse();
        }

        public void BrowseProjectUri()
        {
            ProjectUri.Browse();
        }

        public void BrowseAccountUri()
        {
            AccountUri.Browse();
        }
    }

    public class VstsAccount
    {
        public string FullName { get; set; }
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
