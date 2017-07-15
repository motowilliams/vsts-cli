using System;

namespace Vsts.Cli
{
    public static class CommandName
    {
        public const string Code = "code";
        public const string PullRequests = "pullrequests";
        public const string WorkItems = "workitems";
        public const string Builds = "builds";
        public const string Logs = "logs";
        public const string Queue = "queue";
        public const string Releases = "releases";
        public const string TestManagement = "testmanagement";
        public const string Dashboard = "dashboard";
        public const string Browse = "browse";
        public const string Create = "create";
        public const string Add = "add";
        public const string Unassigned = "unassigned";


        public const string DashQuestion = "-?";
        public const string DashH = "-h";
        public const string DashDashHelp = "--help";
        public static string HelpTemplate => $"{DashQuestion} | {DashH} | {DashDashHelp}";
    }
}