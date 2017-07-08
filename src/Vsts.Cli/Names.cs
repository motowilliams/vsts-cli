using System;

namespace Vsts.Cli
{
    public static class Names
    {
        public static string NormalizeWorkItemType(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;

            source = source.ToLower().Replace("-", string.Empty) == "userstory" ? "user story" : source;
            source = source.ToLower().Replace("-", string.Empty) == "testcase" ? "test case" : source;
            source = source.ToLower().Replace("-", string.Empty) == "testsuite" ? "test suite" : source;

            if (source.Equals("bugs", StringComparison.OrdinalIgnoreCase))
                source = "bug";
            if (source.Equals("epics", StringComparison.OrdinalIgnoreCase))
                source = "epic";
            if (source.Equals("epics", StringComparison.OrdinalIgnoreCase))
                source = "epic";
            if (source.Equals("features", StringComparison.OrdinalIgnoreCase))
                source = "feature";
            if (source.Equals("issues", StringComparison.OrdinalIgnoreCase))
                source = "issue";

            return source;
        }

        public static string NormalizeCommand(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            source = source.ToLower().Replace("-", string.Empty);

            source = source == "pullrequest" ? CommandName.PullRequests : source;
            source = source == "pr" ? CommandName.PullRequests : source;
            source = source == "pullrequets" ? CommandName.PullRequests : source;

            source = source == "workitem" ? CommandName.WorkItems : source;
            source = source == "wi" ? CommandName.WorkItems : source;
            source = source == "workitme" ? CommandName.WorkItems : source;
            source = source == "workitmes" ? CommandName.WorkItems : source;

            source = source == "build" ? CommandName.Builds : source;
            source = source == "log" ? CommandName.Logs : source;
            source = source == "lgo" ? CommandName.Logs : source;
            source = source == "lgos" ? CommandName.Logs : source;

            source = source == "release" ? CommandName.Releases : source;
            source = source == "rel" ? CommandName.Releases : source;
            source = source == "releaes" ? CommandName.Releases : source;

            source = source == "test" ? CommandName.TestManagement : source;
            source = source == "testmanagemnet" ? CommandName.TestManagement : source;
            source = source == "tests" ? CommandName.TestManagement : source;
            source = source == "tets" ? CommandName.TestManagement : source;
            source = source == "tetss" ? CommandName.TestManagement : source;

            return source;
        }
    }
}