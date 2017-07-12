using System;
using System.Linq;

namespace Vsts.Cli
{
    public class VstsProjectHelper
    {
        private readonly IVsts vsts;

        public VstsProjectHelper(IVsts vsts)
        {
            this.vsts = vsts;
        }

        public void CheckAccessToken()
        {
            if (!vsts.NeedsAccessToken) return;

            Console.Write("Please enter your personal access token: ", ConsoleColor.Green);
            var personalAccessToken = Console.ReadLine();

            var vstsApiHelper = new VstsApiHelper(vsts.AccountUri, personalAccessToken);
            var vstsRepos = vstsApiHelper.GetRepositories();
            if (!vstsRepos.Any())
            {
                Console.WriteLine($"Could not find an existing VSTS git repo that matches with the current git repo {vsts.RepositoryName}", ConsoleColor.Yellow);
                Environment.Exit(0);
            }

            vsts.SetAccessToken(personalAccessToken);
        }

        public void CheckRemoteProjectLink(VstsApiHelper vstsApiHelper)
        {
            if (vsts.IsInProject) return;

            var repositories = vstsApiHelper.GetRepositories();
            var vstsRepos = repositories.Where(x => x.Name.Equals(vsts.RepositoryName)).ToList();
            if (!vstsRepos.Any())
            {
                Console.WriteLine($"Could not find an existing VSTS git repo that matches with the current git repo {vsts.RepositoryName}", ConsoleColor.Yellow);
                Environment.Exit(0);
            }

            Repository selectedRepo;
            if (vstsRepos.Count > 1)
            {
                Console.WriteLine($"Multiple projects contain repositories that match your current git repository {vsts.RepositoryName}", ConsoleColor.DarkYellow);

                int index = 1;
                foreach (var repository in vstsRepos)
                {
                    Console.WriteLine($" - {index}) the {repository.Name} repository exists in the {repository.Project.Name} project");
                    index++;
                }

                var selectedRange = Enumerable.Range(1, vstsRepos.Count);
                var count = String.Join(",", selectedRange);
                Console.Write($"Select the project you want to link to this local repository [{count}]: ", ConsoleColor.Yellow);

                // validate we recieved both a numeric and in-range selection
                var input = Console.ReadLine();
                var validInput = Int32.TryParse(input, out int selected);
                if (!validInput || !selectedRange.Contains(selected))
                {
                    Console.WriteLine("Invalid selection", ConsoleColor.Red);
                }

                selectedRepo = vstsRepos[selected - 1];
            }
            else
            {
                selectedRepo = vstsRepos.First();
            }
            vsts.CreateNewConfiguration(selectedRepo.Name, selectedRepo.Id, selectedRepo.Project.Name, selectedRepo.Project.Id, vsts.PersonalAccessToken);
        }

        public void CheckLocalProjectLink(VstsApiHelper vstsApiHelper)
        {
            //Check to see if the current directories repo should be added to the vsts-cli configuration
            // this is for single vsts projects that have multiple repos
            if (vsts.LocalDirectoryLinked) return;

            Console.WriteLine($"This is the first time seeing the {vsts.RepositoryName} repository.", ConsoleColor.DarkYellow);
            Console.Write($"Add this to the configuration? [Yes]/No ", ConsoleColor.Yellow);
            var add = Console.ReadLine();
            if (add.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                add.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }
            
            var repositories = vstsApiHelper.GetRepositories();
            var vstsRepo = repositories.FirstOrDefault(x => x.Name.Equals(vsts.RepositoryName, StringComparison.OrdinalIgnoreCase));
            vsts.AddLocalDirectoryLink(vsts.RepositoryName, vstsRepo.Id);
        }
    }
}