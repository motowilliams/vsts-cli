using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public static class VstsService
    {
        private const string Help = "-? | -h | --help";

        private const string New = "new";
        private const string Active = "active";
        private const string Unassigned = "unassigned";
        private const string Add = "add";

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
            {
                Console.Logo();
                args = new[] { "-h" };
            }

            //Now we can start to inspect the command-line arguments
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "vsts",
                Description = "Visual Studio Team Services Command Line Interface",
                ShowInHelpText = true
            };

            app.Command(Command.Browse, config =>
            {
                config.Description = "launches the default browser to the VSTS project root";
                var dashboardArgument = config.Argument("dashboard", "General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement");
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    switch (dashboardArgument.Value.NormalizeCommand())
                    {
                        case Command.Code:
                            vsts.CodeUri.Browse();
                            break;
                        case Command.Builds:
                            vsts.BuildsUri.Browse();
                            break;
                        case Command.Releases:
                            vsts.ReleasesUri.Browse();
                            break;
                        case Command.WorkItems:
                            vsts.WorkItemsUri.Browse();
                            break;
                        case Command.PullRequests:
                            vsts.PullRequestUri.Browse();
                            break;
                        case Command.TestManagement:
                            vsts.TestManagementUri.Browse();
                            break;
                        case Command.Dashboard:
                            vsts.ProjectUri.Browse();
                            break;
                        default:
                            vsts.CodeUri.Browse();
                            break;
                    }
                    return 0;
                });
            });

            var buildCommand = app.Command(Command.Builds, config =>
            {
                config.Description = "commands for working with VSTS build definitions";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    var details = vstsApiHelper.GetBuildListDetails(vsts.ProjectName);

                    if (!details.Any())
                        return 0;

                    var detailIdWidth = details.Max(x => x.DefinitionIdLength);
                    var buildNameWidth = details.Max(x => x.DefinitionNameLength);
                    var statusNameWidth = details.Max(x => x.StatusNameLength);
                    var resultNameWidth = details.Max(x => x.ResultNameLength);

                    foreach (var definitionGroup in details.GroupBy(x => x.definition.name).OrderBy(x => x.Key))
                    {
                        BuildListItem detail = definitionGroup.OrderByDescending(x => x.id).First();
                        Console.WriteLine($"{detail.definition.id.ToString().PadRight(detailIdWidth, ' ')} {detail.status.PadRight(statusNameWidth)} {detail.ResultName(resultNameWidth)} {detail.definition.name.PadRight(buildNameWidth)} {detail.TimeReport} {detail.buildNumber}", detail.ConsoleColor);
                    }

                    return 0;

                });
            });

            buildCommand.Command("log", config =>
            {
                config.Description = "view latest build log for build definition";
                var buildIdOption = config.Option("--id", "build definition", CommandOptionType.SingleValue);
                buildIdOption.ShortName = "i";
                var buildLogDetailOption = config.Option("--detail", "how the log file for the build", CommandOptionType.NoValue);
                buildLogDetailOption.ShortName = "d";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    if (!buildIdOption.HasValue() || !int.TryParse(buildIdOption.Value(), out int buildDefinitionId))
                        return 1;

                    var detail = vstsApiHelper.GetBuildDetail(vsts.ProjectName, buildDefinitionId);

                    if (detail == null)
                        return 1;

                    var records = vstsApiHelper.GetBuildTimeline(vsts.ProjectName, detail.id);

                    if (!records.Any())
                        return 1;

                    Console.WriteLine($"{records.First().resultSymbol} {records.First().name}", records.First().ConsoleColor);
                    foreach (var record in records.Skip(1).OrderBy(x => x.order))
                        Console.WriteLine($"-{record.resultSymbol} {record.name}", record.ConsoleColor);

                    if (buildLogDetailOption.HasValue())
                    {
                        var buildLogEntry = vstsApiHelper.GetBuildLogEntry(vsts.ProjectName, detail.id, records.OrderBy(x => x.order).First().log.id);
                        buildLogEntry.ToList().ForEach(Console.WriteLine);
                    }

                    return 0;
                });
            });

            var codeCommand = app.Command(Command.Code, config =>
            {
                config.Description = "launches the default browser to the current repos code dashboard";
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    vsts.CodeBranchUri.Browse();
                    return 0;
                });
            });

            var workItemsCommand = app.Command(Command.WorkItems, config =>
            {
                config.Description = "commands for working with VSTS work items";
                var id = config.Argument("work item identifier", "work item id or type, such as epic, user story, task or bug");
                id.ShowInHelpText = true;
                var stateOption = config.Option("--states", "filter by states such as new, active, resolved, closed or removed", CommandOptionType.MultipleValue);
                stateOption.ShortName = "s";
                var tagOption = config.Option("--tags", "filter by any tag that assigned to work items", CommandOptionType.MultipleValue);
                tagOption.ShortName = "t";
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

                        IEnumerable<WorkItem> searchWorkItems = vstsApiHelper.SearchWorkItems(vsts.ProjectName, id.Value, stateArgumentValues, tagOption.Values);
                        details = vstsApiHelper.GetWorkItemDetail(searchWorkItems.Select(x => x.id));
                    }

                    var detailIdWidth = details.Max(x => x.Id.ToString().Length);
                    var stateWidth = details.Select(x => x.State).Distinct().Max(x => x.Length);
                    var assignments = details.Where(x => !string.IsNullOrWhiteSpace(x.AssignedToName)).Distinct();
                    var assignedToNameWidth = assignments.Any() ? assignments.Select(x => x.AssignedToName).Max(x => x.Length) : 0;
                    var workItemTypeWidth = details.Select(x => x.WorkItemType).Distinct().Max(x => x.Length);

                    // this sort just happens to work out for the Epic/Feature/Story level but it may not work for other project types
                    foreach (Fields detail in details.OrderBy(x => x.WorkItemType).ThenBy(x => x.CreatedDate))
                    {
                        var assignedTo = $"{detail.AssignedToName ?? Unassigned}";
                        var color = string.IsNullOrWhiteSpace(detail.AssignedToName) ? ConsoleColor.DarkYellow : ConsoleColor.Green;
                        Console.WriteLine($"#{detail.Id.ToString().PadRight(detailIdWidth)} {detail.State.PadRight(stateWidth)} {detail.WorkItemType.PadRight(workItemTypeWidth)} {detail.CreatedDate.ToLocalTime():yyyy/MM/dd} {assignedTo.PadRight(assignedToNameWidth)} {detail.Title.Trim()} : {detail.Tags ?? "no tags"}", color);
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

            var pullRequestsCommand = app.Command(Command.PullRequests, config =>
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
            {
                var strings = Help.Split('|').Select(x => x.Trim());
                if (!strings.Any(x => x.Equals(args[0], StringComparison.OrdinalIgnoreCase)))
                    args[0] = args[0].NormalizeCommand();
            }

            app.HelpOption(Help);
            app.Execute(args);
        }
    }
}