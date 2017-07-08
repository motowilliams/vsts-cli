using System;

namespace Vsts.Cli.Tests
{
    public static class HelpCommandResponseFor
    {

        public static string Root => @"

Usage: vsts [options] [command]

Options:
  -? | -h | --help  Show help information

Commands:
  browse        launches the default browser to the VSTS project root
  builds        commands for working with VSTS build definitions
  code          launches the default browser to the current repos code dashboard
  pullrequests  commands for working with VSTS pull requests
  workitems     commands for working with VSTS work items

Use ""vsts [command] --help"" for more information about a command.

";

        public static string Browse => @"

Usage: vsts browse [arguments] [options]

Arguments:
  dashboard  General dashboard to view. Opionts are builds, releases, workitems, pullrequests or testmanagement

Options:
  -? | -h | --help  Show help information

";

        public static string PullRequest => @"

Usage: vsts pullrequests [arguments] [options] [command]

Arguments:
  pull request identifier  pull request id to browse to

Options:
  -? | -h | --help  Show help information

Commands:
  create  commands for creating a pull request

Use ""pullrequests [command] --help"" for more information about a command.

";

        public static string PullRequestCreate => @"

Usage: vsts pullrequests create [options]

Options:
  -? | -h | --help  Show help information
  --title           pull request title [required] 
  --description     pull request description
  --source          source branch
  --target          target branch

";

        public static string Builds => @"

Usage: vsts builds [options] [command]

Options:
  -? | -h | --help  Show help information

Commands:
  logs  view latest build log for build definition

Use ""builds [command] --help"" for more information about a command.

";

        public static string BuildsLog => @"

Usage: vsts builds logs [options]

Options:
  --id              build definition id
  --detail          show the log file for the build
  -? | -h | --help  Show help information

";

        public static string Code => @"

Usage: vsts code [options]

Options:
  -? | -h | --help  Show help information

";

        public static string WorkItems => @"

Usage: vsts workitems [arguments] [options] [command]

Arguments:
  work item identifier  work item id or type, such as epic, user story, task or bug

Options:
  --states          filter by states such as new, active, resolved, closed or removed
  --tags            filter by any tag that assigned to work items
  --description     include description
  --my              only return open work items assigned to me
  --browse          browse specific work item in VSTS
  -? | -h | --help  Show help information

Commands:
  add  command for adding new work items to the current project

Use ""workitems [command] --help"" for more information about a command.

";

        public static string WorkItemsAdd => @"

Usage: vsts workitems add [options]

Options:
  --workitemtype    work item type [required]
  --title           work item title [required] 
  --description     work item description
  --priority        work item priority
  --tags            work item tags
  -? | -h | --help  Show help information

";

    }
}