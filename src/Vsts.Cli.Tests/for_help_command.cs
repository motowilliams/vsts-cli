using System;
using FakeItEasy.Configuration;
using Xunit;

namespace Vsts.Cli.Tests
{
    public class for_help_command : Context
    {
        [Theory]
        [InlineData("")]
        [InlineData(CommandName.DashQuestion)]
        [InlineData(CommandName.DashH)]
        [InlineData(CommandName.DashDashHelp)]
        public void should_return_primary_root_help(string helpTemplate)
        {
            var execute = cli.Execute(helpTemplate);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.Root, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_browse_root_help()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.Browse, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_builds_root_help()
        {
            var execute = cli.Execute(CommandName.Builds, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.Builds, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_pull_request_root_help()
        {
            var execute = cli.Execute(CommandName.PullRequests, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.PullRequest, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_pull_request_create_help()
        {
            var execute = cli.Execute(CommandName.PullRequests, CommandName.Create, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.PullRequestCreate, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_code_root_help()
        {
            var execute = cli.Execute(CommandName.Code, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.Code, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_work_items_root_help()
        {
            var execute = cli.Execute(CommandName.WorkItems, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.WorkItems, cli.Response, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void should_return_work_items_add_help()
        {
            var execute = cli.Execute(CommandName.WorkItems, CommandName.Add, CommandName.DashH);

            Assert.Equal(0, execute);
            Assert.Equal(HelpCommandResponseFor.WorkItemsAdd, cli.Response, ignoreLineEndingDifferences: true);
        }
    }
}