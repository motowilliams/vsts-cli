using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public class Cli : CommandLineApplication
    {
        private readonly IVsts vsts;
        private readonly IVstsApiHelper vstsApiHelper;
        private readonly StringWriter _writer;

        public Cli(IVsts vsts, IVstsApiHelper vstsApiHelper, StringWriter writer = null) : base(throwOnUnexpectedArg: true)
        {
            this.vsts = vsts;
            this.vstsApiHelper = vstsApiHelper;
            _writer = writer;

            base.Name = "vsts";
            base.Description = "Visual Studio Team Services Command Line Interface";
            base.HelpOption(CommandName.HelpTemplate);

            if (_writer != null) base.Out = _writer;

            Setup();
        }

        private void Setup()
        {
            Command(CommandName.Browse, config =>
            {
                config.Out = base.Out;
                config.Description = "launches the default browser to the VSTS project root";
                var dashboardArgument = config.Argument("dashboard", "General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement");
                config.HelpOption(CommandName.HelpTemplate);
                config.OnExecute(() =>
                {
                    switch (dashboardArgument.Value.Normalize())
                    {
                        case CommandName.Code:
                            vsts.BrowseCodeUri();
                            break;
                        case CommandName.Builds:
                            vsts.BrowseBuildsUri();
                            break;
                        case CommandName.Releases:
                            vsts.BrowseReleasesUri();
                            break;
                        case CommandName.WorkItems:
                            vsts.BrowseWorkItemsUri();
                            break;
                        case CommandName.PullRequests:
                            vsts.BrowsePullRequestUri();
                            break;
                        case CommandName.TestManagement:
                            vsts.BrowseTestManagementUri();
                            break;
                        case CommandName.Dashboard:
                            vsts.BrowseProjectUri();
                            break;
                        default:
                            vsts.BrowseCodeUri();
                            break;
                    }
                    return 0;
                });
            });

            var buildCommand = Command(CommandName.Builds, config =>
            {
                config.Out = base.Out;
                config.Description = "commands for working with VSTS build definitions";
                config.HelpOption(CommandName.HelpTemplate);
                config.OnExecute(() =>
                {
                    var details = vstsApiHelper.GetBuildListDetails(vsts.ProjectName);

                    if (!details.Any())
                        return 1;

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

            buildCommand.Command(CommandName.Logs, config =>
            {
                config.Out = base.Out;
                config.Description = "view latest build log for build definition";
                var buildIdOption = config.Option(CommandOptionTemplates.IdTemplate, "build definition id", CommandOptionType.SingleValue);
                buildIdOption.ShortName = CommandOptionTemplates.IdTemplateShort;
                var buildLogDetailOption = config.Option(CommandOptionTemplates.DetailTemplate, "show the log file for the build", CommandOptionType.NoValue);
                buildLogDetailOption.ShortName = CommandOptionTemplates.DetailTemplateShort;
                config.HelpOption(CommandName.HelpTemplate);
                config.OnExecute(() =>
                {
                    if (!buildIdOption.HasValue() || !int.TryParse(buildIdOption.Value(), out int buildDefinitionId))
                    {
                        buildCommand.ShowHelp(CommandName.Logs);
                        return 1;
                    }

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

            buildCommand.Command(CommandName.Queue, config =>
            {
                config.Out = base.Out;
                config.Description = "commands for queueing a new build definition";
                var buildIdOption = config.Option(CommandOptionTemplates.IdTemplate, "build definition id", CommandOptionType.SingleValue);
                buildIdOption.ShortName = CommandOptionTemplates.IdTemplateShort;

                config.HelpOption(CommandName.HelpTemplate);

                config.OnExecute(() =>
                {
                    if (!int.TryParse(buildIdOption.Value(), out int buildId))
                    {
                        config.ShowHelp(CommandName.Queue);
                        return 1;
                    }

                    BuildListItem detail = vstsApiHelper.QueueBuildDefinition(vsts.ProjectName, new BuildDefinitionQueueResource { definition = new DefinitionId { id = buildId } });
                    Console.WriteLine($"{detail.definition.id} {detail.status} {detail.result} {detail.definition.name} {detail.TimeReport} {detail.buildNumber}", detail.ConsoleColor);

                    return 0;
                });
            });

            var codeCommand = Command(CommandName.Code, config =>
            {
                config.Out = base.Out;
                config.Description = "launches the default browser to the current repos code dashboard";
                config.HelpOption(CommandName.HelpTemplate);
                config.OnExecute(() =>
                {
                    vsts.BrowseCodeBranchUri();
                    return 0;
                });
            });

            var workItemsCommand = Command(CommandName.WorkItems, config =>
            {
                config.Out = base.Out;
                config.Description = "commands for working with VSTS work items";
                var id = config.Argument("work item identifier", "work item id or type, such as epic, user story, task or bug");
                id.ShowInHelpText = true;

                var stateOption = config.Option(CommandOptionTemplates.StatesTemplate, "filter by states such as new, active, resolved, closed or removed", CommandOptionType.MultipleValue);
                stateOption.ShortName = CommandOptionTemplates.StatesTemplateShort;
                var tagOption = config.Option(CommandOptionTemplates.TagTemplate, "filter by any tag that assigned to work items", CommandOptionType.MultipleValue);
                tagOption.ShortName = CommandOptionTemplates.TagTemplateShort;
                var descriptionOption = config.Option(CommandOptionTemplates.DescriptionTemplate, "include description", CommandOptionType.NoValue);
                descriptionOption.ShortName = CommandOptionTemplates.DescriptionTemplateShort;
                var myWorkItemOption = config.Option(CommandOptionTemplates.MyTemplate, "only return open work items assigned to me", CommandOptionType.NoValue);
                myWorkItemOption.ShortName = CommandOptionTemplates.MyTemplateShort;
                var browseOption = config.Option(CommandOptionTemplates.BrowseTemplate, "browse specific work item in VSTS", CommandOptionType.NoValue);
                browseOption.ShortName = CommandOptionTemplates.BrowseTemplateShort;

                config.HelpOption(CommandName.HelpTemplate);
                config.OnExecute(() =>
                {
                    IEnumerable<Fields> details = null;
                    bool singleWorkItem = false;
                    var stateArgumentValues = stateOption.AsStateDefault();

                    if (myWorkItemOption.HasValue())
                    {
                        IEnumerable<WorkItem> searchWorkItems =
                            vstsApiHelper.SearchWorkItems(vsts.ProjectName, id.Value, stateArgumentValues,
                                tagOption.Values, vsts.FullName);
                        details = vstsApiHelper.GetWorkItemDetail(searchWorkItems.Select(x => x.id));
                    }
                    else if (Int32.TryParse(id.Value, out int workItemId))
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
                        IEnumerable<WorkItem> searchWorkItems = vstsApiHelper.SearchWorkItems(vsts.ProjectName, id.Value, stateArgumentValues,
                                tagOption.Values);
                        details = vstsApiHelper.GetWorkItemDetail(searchWorkItems.Select(x => x.id));
                    }

                    if (!details.Any())
                        return 0;

                    var detailIdWidth = details.Max(x => x.WorkItemIdLength);
                    var stateWidth = details.Max(x => x.StateLength);
                    var assignedToNameWidth = details.Max(x => x.AssignedToNameLength);
                    var workItemTypeWidth = details.Max(x => x.WorkItemTypeLength);

                    // this sort just happens to work out for the Epic/Feature/Story level but it may not work for other project types
                    foreach (Fields detail in details.OrderBy(x => x.WorkItemType).ThenBy(x => x.CreatedDate))
                    {
                        var assignedTo = $"{detail.AssignedToName ?? CommandName.Unassigned}";
                        var color = string.IsNullOrWhiteSpace(detail.AssignedToName) ? ConsoleColor.DarkYellow : ConsoleColor.Green;
                        Console.WriteLine($"#{detail.Id.ToString().PadRight(detailIdWidth)} {detail.State.PadRight(stateWidth)} {detail.WorkItemType.PadRight(workItemTypeWidth)} {detail.CreatedDate.ToLocalTime():yyyy/MM/dd} {assignedTo.PadRight(assignedToNameWidth)} {detail.Title.Trim()} : {detail.Tags ?? "no tags"}", color);
                        if (singleWorkItem || descriptionOption.HasValue())
                            Console.WriteLine($"{" ".PadRight(detailIdWidth + 1)} {detail.Description ?? "no description provided"}", color);
                    }

                    return 0;
                });
            });

            workItemsCommand.Command(CommandName.Add, config =>
            {
                config.Out = base.Out;
                config.Description = "command for adding new work items to the current project";
                var typeOption = config.Option("--workitemtype", "work item type [required]", CommandOptionType.SingleValue);
                typeOption.ShortName = "w";
                var titleOption = config.Option(CommandOptionTemplates.TitleTemplate, "work item title [required] ", CommandOptionType.SingleValue);
                titleOption.ShortName = CommandOptionTemplates.TitleTemplateShort;
                var descriptionOption = config.Option(CommandOptionTemplates.DescriptionTemplate, "work item description", CommandOptionType.SingleValue);
                descriptionOption.ShortName = CommandOptionTemplates.DescriptionTemplateShort;
                var priorityOption = config.Option(CommandOptionTemplates.PriorityTemplate, "work item priority", CommandOptionType.SingleValue);
                priorityOption.ShortName = CommandOptionTemplates.PriorityTemplateShort;
                var tagsOption = config.Option(CommandOptionTemplates.TagTemplate, "work item tags", CommandOptionType.MultipleValue);
                tagsOption.ShortName = CommandOptionTemplates.TagTemplateShort;

                config.HelpOption(CommandName.HelpTemplate);

                config.OnExecute(() =>
                {
                    if (!typeOption.HasValue() || !titleOption.HasValue())
                    {
                        config.ShowHelp(CommandName.Add);
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

            var pullRequestsCommand = Command(CommandName.PullRequests, config =>
            {
                config.Out = base.Out;
                config.Description = "commands for working with VSTS pull requests";
                var id = config.Argument("pull request identifier", "pull request id to browse to");
                id.ShowInHelpText = true;
                config.HelpOption(CommandName.HelpTemplate);
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

            var pullRequestCreateCommand = pullRequestsCommand.Command(CommandName.Create, config =>
            {
                config.Out = base.Out;
                config.Description = "commands for creating a pull request";
                config.HelpOption(CommandName.HelpTemplate);
                var titleOption = config.Option(CommandOptionTemplates.TitleTemplate, "pull request title [required] ", CommandOptionType.SingleValue);
                titleOption.ShortName = CommandOptionTemplates.TitleTemplateShort;
                titleOption.ShowInHelpText = true;
                var descriptionOption = config.Option(CommandOptionTemplates.DescriptionTemplate, "pull request description", CommandOptionType.SingleValue);
                descriptionOption.ShortName = CommandOptionTemplates.DescriptionTemplateShort;
                descriptionOption.ShowInHelpText = true;
                var sourceRefNameOption = config.Option(CommandOptionTemplates.SourceReferenceNameTemplate, "source branch", CommandOptionType.SingleValue);
                sourceRefNameOption.ShortName = CommandOptionTemplates.SourceReferenceNameTemplateShort;
                sourceRefNameOption.ShowInHelpText = true;
                var targetRefNameOption = config.Option(CommandOptionTemplates.TargetReferenceNameTemplate, "target branch", CommandOptionType.SingleValue);
                targetRefNameOption.ShortName = CommandOptionTemplates.TargetReferenceNameTemplateShort;
                targetRefNameOption.ShowInHelpText = true;

                config.OnExecute(() =>
                {

                    var title = titleOption.HasValue() ? titleOption.Value() : vsts.LastCommit.Split(Environment.NewLine.ToCharArray()).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        Console.Write("Pull request title missing", ConsoleColor.Yellow);
                        return 1;
                    }

                    var description = descriptionOption.HasValue() ? descriptionOption.Value() : vsts.LastCommit;
                    var source = sourceRefNameOption.HasValue() ? sourceRefNameOption.Value() : vsts.RepositoryBranchName;
                    var target = targetRefNameOption.HasValue() ? targetRefNameOption.Value() : "master";

                    Console.WriteLine($"Create New Pull Request", ConsoleColor.Gray);
                    Console.WriteLine($"-----------------------", ConsoleColor.Gray);
                    Console.WriteLine($"Title: {title}", ConsoleColor.Gray);
                    Console.WriteLine($"From: {source}", ConsoleColor.Gray);
                    Console.WriteLine($"To: {target}", ConsoleColor.Gray);
                    Console.WriteLine($"Description: {description}", ConsoleColor.Gray);
                    Console.Write($"Submit this pull request y/[n] ", ConsoleColor.Yellow);
                    var result = System.Console.ReadLine();

                    if (result.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                        result.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        var detail = vstsApiHelper.CreatePullRequest(vsts.RepositoryId, title, description, source, target);
                        Console.WriteLine($"#{detail.PullRequestId} {detail.Title} by {detail.CreatedBy.DisplayName}");
                    }

                    return 0;
                });
            });
        }

        public string Response { get; private set; }

        public new int Execute(params string[] args)
        {
            args = args.AsUpdatedArray();

            int result = 1;
            try
            {
                result = base.Execute(args);
            }
            catch (CommandParsingException e)
            {
                // Try to show the user the help system items for what they were trying to call
                // Any valid args should have been rewriten by this point
                if (args.Any())
                {
                    var command =
                        Commands.FirstOrDefault(x => x.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
                    if (command == null)
                        ShowHelp();
                    else
                        command.ShowHelp();
                }
                Console.WriteLine(e.Message, ConsoleColor.Yellow);
            }
            finally
            {
                if (_writer != null)
                {
                    var sb = _writer.GetStringBuilder();
                    Response = sb.ToString();
                }
            }
            return result;
        }
    }

}