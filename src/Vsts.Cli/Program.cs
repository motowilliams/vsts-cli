using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public class Program
    {
        const string Help = "-? | -h | --help";

        const string Code = "code";
        const string PullRequests = "pullrequests";
        const string WorkItems = "workitems";
        const string Builds = "builds";
        const string Releases = "releases";
        const string TestManagement = "testmanagement";
        const string Dashboard = "dashboard";

        const string New = "new";
        const string Active = "active";
        const string Unassigned = "unassigned";

        const string Add = "add";
        const string Create = "create";
        const string Browse = "browse";

        public static void Main(string[] args)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var gitRepoDirectory = GitRepoHelpers.FindDirectory(currentDirectory, true);
            GitConfiguration gitConfiguration = GitRepoHelpers.Create(gitRepoDirectory);

            if (gitRepoDirectory != null && gitConfiguration.NonVstsHost)
            {
                Console.WriteLine($"Found a non-VSTS git repo at {gitRepoDirectory} pointing to {gitConfiguration.Origin}");
                Environment.Exit(0);
                return;
            }

            if (string.IsNullOrWhiteSpace(gitConfiguration.Name) || string.IsNullOrWhiteSpace(gitConfiguration.Host))
            {
                Console.WriteLine($"Could not find an existing VSTS git repo in the current {currentDirectory} directory or parent directories");
                Environment.Exit(0);
                return;
            }

            var vsts = new Vsts(gitConfiguration, currentDirectory, gitRepoDirectory);
            var accountUri = vsts.AccountUri;

            VstsApiHelper vstsApiHelper;
            if (vsts.IsInProject && vsts.NeedsAccessToken)
            {
                Console.Write("Please enter your personal access token: ");
                var personalAccessToken = Console.ReadLine();

                vstsApiHelper = new VstsApiHelper(accountUri, personalAccessToken);

                var repositories = vstsApiHelper.GetRepositories();
                var vstsRepo = repositories.FirstOrDefault(x => x.Name.Equals(vsts.RepositoryName));
                if (vstsRepo == null)
                {
                    Console.WriteLine("Could not find an existing VSTS git repo that matches with the current git repo");
                    Environment.Exit(0);
                    return;
                }
            }
            else
            {
                vstsApiHelper = new VstsApiHelper(accountUri, vsts.PersonalAccessToken);
            }

            //Check to see if the current directories repo shoudl be added to the vsts-cli configuration
            if (vsts.LocalDirectoryLinked == false)
            {
                Console.Write($"This is the first time seeing this projects repo. Add this to the {vsts.ProjectName} configuration? [Yes]/No ");
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

            if (args == null || args.Length == 0)
                args = new[] { "-h" };

            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "vsts"
            };

            app.Command(Browse, config =>
            {
                config.Description = "Launches the default browser to the VSTS project root";
                var dashboardArgument = config.Argument("dashboard", "General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement", false);
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
                config.HelpOption(Help);
                config.OnExecute(() =>
                {
                    IEnumerable<Fields> details = null;

                    if (int.TryParse(id.Value, out int workItemId))
                    {
                        details = vstsApiHelper.GetWorkItemDetail(workItemId);
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

                    foreach (Fields detail in details)
                        Console.WriteLine($"#{detail.Id} {detail.State} {detail.CreatedDate.ToLocalTime():yyyy/MM/dd} {detail.AssignedToName ?? Unassigned} - {detail.Title.Trim()} : {detail.Tags ?? "no tags"}");

                    return 0;
                });
            });

            workItemsCommand.Command(Add, config =>
            {
                config.Description = "command for adding new work items to the current project";
                var typeOption = config.Option("--workitemtype", "work item type", CommandOptionType.SingleValue);
                typeOption.ShortName = "w";
                var titleOption = config.Option("--title", "work item title", CommandOptionType.SingleValue);
                typeOption.ShortName = "t";
                var descriptionOption = config.Option("--description", "work item description", CommandOptionType.SingleValue);
                descriptionOption.ShortName = "d";
                var priorityOption = config.Option("--priority", "work item priority", CommandOptionType.SingleValue);
                priorityOption.ShortName = "p";
                var tagsOption = config.Option("--tag", "work item tags", CommandOptionType.MultipleValue);

                config.HelpOption(Help);

                config.OnExecute(() =>
                {
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
                    if (int.TryParse(id.Value, out int pullRequestId))
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

            app.HelpOption(Help);
            app.Execute(args);
        }
    }
}
