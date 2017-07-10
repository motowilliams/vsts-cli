using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Vsts.Cli.Tests
{
    public class for_cli_arguments
    {
        [Fact]
        public void should_correct_null_args_to_help_template()
        {
            string[] args = null;

            args = args.AsUpdatedArray();

            Assert.Equal("-h", args[0]);
        }

        [Fact]
        public void should_correct_single_empty_string_item_args_to_help_template()
        {
            string[] args = new[] { "" };

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.DashH, args[0]);
        }

        [Fact]
        public void should_correct_single_null_array_args_to_help_template()
        {
            string[] args = Enumerable.Empty<string>().ToArray();

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.DashH, args[0]);
        }

        [Fact]
        public void should_leave_foo_command()
        {
            string[] args = new[] { "foo" };

            args = args.AsUpdatedArray();

            Assert.Equal("foo", args[0]);
        }

        public static IEnumerable<object[]> PullRequestCommands()
        {
            foreach (var s in ReservedNames.PullRequestCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(PullRequestCommands))]
        public void should_correct_pull_request_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.PullRequests, args[0]);
        }

        public static IEnumerable<object[]> BuildCommands()
        {
            foreach (var s in ReservedNames.BuildCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(BuildCommands))]
        public void should_correct_build_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.Builds, args[0]);
        }

        public static IEnumerable<object[]> LogCommands()
        {
            foreach (var s in ReservedNames.LogCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(LogCommands))]
        public void should_correct_log_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.Logs, args[0]);
        }

        public static IEnumerable<object[]> ReleaseCommands()
        {
            foreach (var s in ReservedNames.ReleaseCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(ReleaseCommands))]
        public void should_correct_release_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.Releases, args[0]);
        }

        public static IEnumerable<object[]> TestManagmentCommands()
        {
            foreach (var s in ReservedNames.TestManagmentCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(TestManagmentCommands))]
        public void should_correct_test_management_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.TestManagement, args[0]);
        }

        public static IEnumerable<object[]> WorkItemCommands()
        {
            foreach (var s in ReservedNames.WorkItemCommands)
                yield return new object[] { new string[] { s } };
        }

        [Theory]
        [MemberData(nameof(WorkItemCommands))]
        public void should_correct_work_item_commands(string[] values)
        {
            string[] args = values;

            args = args.AsUpdatedArray();

            Assert.Equal(CommandName.WorkItems, args[0]);
        }
    }
}