using System;
using System.Linq;

namespace Vsts.Cli
{
    public static class ReservedNames
    {
        public static readonly string[] UserStory = new[] { "userstory", "user story", "user-story" };
        public static readonly string[] TestCase = new[] { "testcase", "test case", "test-case" };
        public static readonly string[] TestSuite = new[] { "testsuite", "test suite", "test-suite" };
        public static readonly string[] Epic = new[] { "epic", "epics", "epci", "epcis" };
        public static readonly string[] Bug = new[] { "bug", "bugs", "bgu", "bgus" };
        public static readonly string[] Feature = new[] { "feature", "features", "featuer", "featuers" };
        public static readonly string[] Issue = new[] { "issue", "issues", "isseu", "isseus" };

        public static readonly string[] PullRequestCommands = new[] { "pullrequest", "pr", "pullrequets" };
        public static readonly string[] WorkItemCommands = new[] { "workitem", "wi", "workitme", "workitmes" };
        public static readonly string[] BuildCommands = new[] { "build", "biuld" };
        public static readonly string[] LogCommands = new[] { "log", "logs", "lgo", "lgos" };
        public static readonly string[] ReleaseCommands = new[] { "release", "releases", "rel", "releaes" };
        public static readonly string[] TestManagmentCommands = new[] { "test", "tests", "testmanagemnet", "tets", "tetss" };

        public static string Normalize(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            source = source.Replace(" ", string.Empty).Replace("-", string.Empty);

            if (UserStory.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.UserStory;

            if (TestCase.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.TestCase;

            if (TestSuite.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.TestSuite;

            if (Epic.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.Epic;

            if (Bug.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.Bug;

            if (Feature.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.Feature;

            if (Issue.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return WorkItemType.Issue;

            if (PullRequestCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.PullRequests;

            if (WorkItemCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.WorkItems;

            if (BuildCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.Builds;

            if (LogCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.Logs;

            if (ReleaseCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.Releases;

            if (TestManagmentCommands.Any(x => x.Equals(source, StringComparison.OrdinalIgnoreCase)))
                return CommandName.TestManagement;

            return source;
        }
    }
}