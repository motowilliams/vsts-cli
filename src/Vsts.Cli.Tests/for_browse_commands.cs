using System;
using System.Collections.Generic;
using Xunit;
using FakeItEasy;

namespace Vsts.Cli.Tests
{
    public class for_browse_commands : Context
    {
        [Fact]
        public void no_args_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseCodeUri()).MustHaveHappened();
        }

        [Fact]
        public void code_arg_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.Code);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseCodeUri()).MustHaveHappened();
        }

        [Fact]
        public void build_arg_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.Builds);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseBuildsUri()).MustHaveHappened();
        }

        [Fact]
        public void release_arg_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.Releases);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseReleasesUri()).MustHaveHappened();
        }

        [Fact]
        public void work_items_arg_should_launch_code_uri()
        {
            var cli = new Cli(vsts, adapter);

            var execute = cli.Execute(CommandName.Browse, CommandName.WorkItems);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseWorkItemsUri()).MustHaveHappened();
        }

        [Fact]
        public void test_management_arg_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.TestManagement);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseTestManagementUri()).MustHaveHappened();
        }

        [Fact]
        public void dashboard_arg_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Browse, CommandName.Dashboard);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.BrowseProjectUri()).MustHaveHappened();
        }
    }
}
