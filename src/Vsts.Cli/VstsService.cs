using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public static class VstsService
    {
        private const string Help = "-? | -h | --help";
        private const string Code = "code";
        private const string PullRequests = "pullrequests";
        private const string WorkItems = "workitems";
        private const string Builds = "builds";
        private const string Releases = "releases";
        private const string TestManagement = "testmanagement";
        private const string Dashboard = "dashboard";
        private const string New = "new";
        private const string Active = "active";
        private const string Unassigned = "unassigned";
        private const string Add = "add";
        private const string Browse = "browse";

        public static VstsApiHelper CheckStatus(GitConfiguration gitConfiguration, Vsts vsts)
        {
            VstsApiHelper vstsApiHelper;

            var accountUri = vsts.AccountUri;

            if (vsts.NeedsAccessToken)
            {
                Console.Write("Please enter your personal access token: ", ConsoleColor.Green);
                var personalAccessToken = Console.ReadLine();

                vstsApiHelper = new VstsApiHelper(accountUri, personalAccessToken);
                var vstsRepos = vstsApiHelper.GetRepositories();
                if (!vstsRepos.Any())
                {
                    Console.WriteLine($"Could not find an existing VSTS git repo that matches with the current git repo {vsts.RepositoryName}", ConsoleColor.Yellow);
                    Environment.Exit(0);
                }

                vsts.SetAccessToken(personalAccessToken);
            }

            if (vsts.IsInProject == false)
            {
                vstsApiHelper = new VstsApiHelper(accountUri, vsts.PersonalAccessToken);

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
            else
            {
                vstsApiHelper = new VstsApiHelper(accountUri, vsts.PersonalAccessToken);
            }

            //Check to see if the current directories repo should be added to the vsts-cli configuration
            // this is for single vsts projects that have multiple repos
            if (vsts.LocalDirectoryLinked == false)
            {
                Console.WriteLine($"This is the first time seeing the {vsts.RepositoryName} repository.", ConsoleColor.DarkYellow);
                Console.Write($"Add this to the configuration? [Yes]/No ", ConsoleColor.Yellow);
                var add = Console.ReadLine();
                if (add.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                    add.Equals("n", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.Exit(0);
                }

                var repositories = vstsApiHelper.GetRepositories();
                var vstsRepo = repositories.FirstOrDefault(x => x.Name.Equals(gitConfiguration.Name, StringComparison.OrdinalIgnoreCase));
                vsts.AddLocalDirectoryLink(vsts.RepositoryName, vstsRepo.Id);
            }

            return vstsApiHelper;
        }

        public static void ProcessArgs(string[] args, Vsts vsts, VstsApiHelper vstsApiHelper)
        {
            if (args == null || args.Length == 0)
                args = new[] { "-h" };

            //Now we can start to inspect the command-line arguments
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "vsts"
            };

            app.Command(Browse, config =>
            {
                config.Description = "Launches the default browser to the VSTS project root";
                var dashboardArgument = config.Argument("dashboard", "General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement");
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    switch (dashboardArgument.Value.NormalizeCommand())
                    {
                        case Code:
                            vsts.CodeUri.Browse();
                            break;
                        case Builds:
                            vsts.BuildsUri.Browse();
                            break;
                        case Releases:
                            vsts.ReleasesUri.Browse();
                            break;
                        case WorkItems:
                            vsts.WorkItemsUri.Browse();
                            break;
                        case PullRequests:
                            vsts.PullRequestUri.Browse();
                            break;
                        case TestManagement:
                            vsts.TestManagementUri.Browse();
                            break;
                        case Dashboard:
                            vsts.ProjectUri.Browse();
                            break;
                        default:
                            vsts.CodeUri.Browse();
                            break;
                    }
                    return 0;
                });
            });

            var buildCommand = app.Command(Builds, config =>
            {
                config.Description = "commands for working with VSTS build definitions";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    var details = vstsApiHelper.GetBuildListDetails(vsts.ProjectName);

                    if (!details.Any())
                        return 0;

                    var detailIdWidth = details.Max(x => x.definition.id.ToString().Length);
                    var buildNameWidth = details.Select(x => x.definition.name).Distinct().Max(x => x.Length);

                    foreach (var buildDefinitionId in details.Select(x => x.definition.name).Distinct().OrderBy(x => x))
                    {
                        var buildListItems = details.Where(x => x.definition.name.Equals(buildDefinitionId));
                        var detail = buildListItems.OrderByDescending(x => x.finishTime).First();

                        System.Console.ForegroundColor = detail.result.Equals("failed") ? ConsoleColor.Red : ConsoleColor.Green;
                        Console.WriteLine($"{detail.definition.id.ToString().PadRight(detailIdWidth, ' ')} {detail.definition.name.PadRight(buildNameWidth)} {detail.finishTime.ToLocalTime():yyyy/MM/dd hh:mm} {detail.buildNumber}", System.Console.ForegroundColor);
                    }

                    return 0;

                });
            });

            var codeCommand = app.Command(Code, config =>
            {
                config.Description = "launches the default browser to the current repos code dashboard";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    vsts.CodeBranchUri.Browse();
                    return 0;
                });
            });

            var workItemsCommand = app.Command(WorkItems, config =>
            {
                config.Description = "commands for working with VSTS work items";
                var id = config.Argument("work item identifier", "work item id or type, such as epic, user story, task or bug");
                id.ShowInHelpText = true;
                var stateOption = config.Option("--states", "work item states", CommandOptionType.MultipleValue);
                stateOption.ShortName = "s";
                var descriptionOption = config.Option("--description", "include description", CommandOptionType.NoValue);
                descriptionOption.ShortName = "d";
                var browseOption = config.Option("--browse", "browse specific work item in VSTS", CommandOptionType.NoValue);
                browseOption.ShortName = "b";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    IEnumerable<Fields> details = null;
                    bool singleWorkItem = false;

                    if (Int32.TryParse(id.Value, out int workItemId))
                    {
                        if (browseOption.HasValue())
                        {
                            vsts.WorkItemUri(workItemId).Browse();
                            return 0;
                        }
                        details = vstsApiHelper.GetWorkItemDetail(workItemId);
                        singleWorkItem = true;
                    }
                    else
                    {
                        List<string> stateArgumentValues = stateOption.Values;
                        if (!stateArgumentValues.Any())
                        {
                            stateArgumentValues.Add(New);
                            stateArgumentValues.Add(Active);
                        }

                        IEnumerable<WorkItem> searchWorkItems = vstsApiHelper.SearchWorkItems(vsts.ProjectName, id.Value, stateArgumentValues);
                        details = vstsApiHelper.GetWorkItemDetail(searchWorkItems.Select(x => x.id));
                    }

                    var detailIdWidth = details.Max(x => x.Id.ToString().Length);
                    var stateWidth = details.Select(x => x.State).Distinct().Max(x => x.Length);
                    var assignments = details.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToName)).Distinct();
                    var assignedToNameWidth = assignments.Any() ? assignments.Select(x => x.AssignedToName).Max(x => x.Length) : 0;

                    foreach (Fields detail in details)
                    {
                        var assignedTo = $"{detail.AssignedToName ?? Unassigned}";
                        var color = string.IsNullOrWhiteSpace(detail.AssignedToName) ? ConsoleColor.DarkYellow : ConsoleColor.Green;
                        Console.WriteLine($"#{detail.Id.ToString().PadRight(detailIdWidth)} {detail.State.PadRight(stateWidth)} {detail.CreatedDate.ToLocalTime():yyyy/MM/dd} {assignedTo.PadRight(assignedToNameWidth)} {detail.Title.Trim()} : {detail.Tags ?? "no tags"}", color);
                        if (singleWorkItem || descriptionOption.HasValue())
                            Console.WriteLine($"{" ".PadRight(detailIdWidth + 1)} {detail.Description ?? "no description provided"}", color);
                    }

                    return 0;
                });
            });

            workItemsCommand.Command(Add, config =>
            {
                config.Description = "command for adding new work items to the current project";
                var typeOption = config.Option("--workitemtype", "work item type [required]", CommandOptionType.SingleValue);
                typeOption.ShortName = "w";
                var titleOption = config.Option("--title", "work item title [required] ", CommandOptionType.SingleValue);
                typeOption.ShortName = "t";
                var descriptionOption = config.Option("--description", "work item description", CommandOptionType.SingleValue);
                descriptionOption.ShortName = "d";
                var priorityOption = config.Option("--priority", "work item priority", CommandOptionType.SingleValue);
                priorityOption.ShortName = "p";
                var tagsOption = config.Option("--tag", "work item tags", CommandOptionType.MultipleValue);

                config.HelpOption(Help);

                config.OnExecute(() =>
                {
                    if (!typeOption.HasValue() || !titleOption.HasValue())
                {
                        config.ShowHelp(Add);
                        return 0;
                    }

                    var document = new List<Object>();
                    document.Add(new { op = "add", path = "/fields/System.Title", value = titleOption.Value() });

                    if (descriptionOption.HasValue())
                        document.Add(new { op = "add", path = "/fields/System.Description", value = descriptionOption.Value() });

                    if (priorityOption.HasValue())
                        document.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = priorityOption.Value() });

                    if (tagsOption.HasValue())
                        document.Add(new { op = "add", path = "/fields/System.Tags", value = String.Join(";", tagsOption.Values) });

                    var detail = vstsApiHelper.CreateWorkItem(vsts.ProjectName, typeOption.Value(), document);
                    Console.WriteLine($"#{detail.Id} {detail.Fields.State} {detail.Fields.CreatedDate.ToLocalTime():yyyy/MM/dd} - {detail.Fields.Title.Trim()}");

                    return 0;
                });
            });

            var pullRequestsCommand = app.Command(PullRequests, config =>
            {
                config.Description = "commands for working with VSTS pull requests";
                var id = config.Argument("pull request identifier", "pull request id to browse to");
                id.ShowInHelpText = true;
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    if (Int32.TryParse(id.Value, out int pullRequestId))
                    {
                        vsts.PullRequestIdUri(pullRequestId).Browse();
                        return 0;
                    }

                    IEnumerable<PullRequest> detail = vstsApiHelper.GetPullRequests(vsts.RepositoryId);
                    foreach (var pullRequest in detail.OrderBy(x => x.CreationDate))
                        Console.WriteLine($"#{pullRequest.PullRequestId} {pullRequest.Title} by {pullRequest.CreatedBy.DisplayName}");

                    return 0;
                });
            });

            if (args.Any())
                args[0] = args[0].NormalizeCommand();
            app.HelpOption(Help);
            app.Execute(args);
        }
    }
}