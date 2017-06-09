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

            source = source == "pullrequest" ? "pullrequests" : source;
            source = source == "pr" ? "pullrequests" : source;
            source = source == "pullrequets" ? "pullrequests" : source;

            source = source == "workitem" ? "workitems" : source;
            source = source == "wi" ? "workitems" : source;
            source = source == "workitme" ? "workitems" : source;
            source = source == "workitmes" ? "workitems" : source;

            source = source == "build" ? "builds" : source;

            source = source == "release" ? "releases" : source;
            source = source == "rel" ? "releases" : source;
            source = source == "releaes" ? "releases" : source;

            source = source == "test" ? "testmanagement" : source;
            source = source == "tests" ? "testmanagement" : source;
            source = source == "tets" ? "testmanagement" : source;
            source = source == "tetss" ? "testmanagement" : source;

            return source;
        }
    }
}