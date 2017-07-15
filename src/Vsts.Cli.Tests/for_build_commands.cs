using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Vsts.Cli.Tests
{
    public class for_build_commands : Context
    {
        [Fact]
        public void should_return_non_zero_return_code_when_missing_project_name()
        {
            A.CallTo(() => adapter.GetBuildListDetails(null)).Returns(Enumerable.Empty<BuildListItem>());

            var execute = cli.Execute(CommandName.Builds);
            Assert.Equal(1, execute);
        }

        [Fact]
        public void should_return_zero_return_code_when_listing_builds()
        {
            GenFu.A.Configure<BuildListItem>().Fill(x => x.definition, () => GenFu.A.New<Definition>());
            var buildListItems = GenFu.A.ListOf<BuildListItem>();
            A.CallTo(() => adapter.GetBuildListDetails("")).Returns(buildListItems);

            var execute = cli.Execute(CommandName.Builds);

            Assert.Equal(0, execute);
        }

        [Fact]
        public void should_return_build_queue_help_for_help_arg()
        {
            var execute = cli.Execute(CommandName.Builds, CommandName.Queue, CommandName.DashH);

            Assert.Equal(HelpCommandResponseFor.BuildsQueue, cli.Response);
        }

        [Fact]
        public void should_return_build_queue_help_for_no_id()
        {
            var execute = cli.Execute(CommandName.Builds, CommandName.Queue);

            Assert.Equal(HelpCommandResponseFor.BuildsQueue, cli.Response);
        }
    }

    public class for_build_log_commands : Context
    {
        int buildId = 42;

        [Fact]
        public void should_return_help_when_with_no_build_id()
        {
            var execute = cli.Execute(CommandName.Builds, CommandName.Logs);

            Assert.Equal(HelpCommandResponseFor.BuildsLog, (string)cli.Response);
            Assert.Equal(1, execute);
        }

        [Fact]
        public void should_return_non_zero_return_code_for_unknown_build_id()
        {
            GenFu.A.Configure<BuildListItem>().Fill(x => x.definition, () => GenFu.A.New<Definition>());

            var execute = cli.Execute(CommandName.Builds, CommandName.Logs, CommandOptionTemplates.IdTemplate, buildId.ToString());

            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).MustHaveHappened();
            Assert.Equal(1, execute);
        }

        [Fact]
        public void should_return_non_zero_return_code_for_missing_timeline_logs()
        {
            GenFu.A.Configure<BuildListItem>().Fill(x => x.definition, () => GenFu.A.New<Definition>());
            var buildListItems = GenFu.A.New<BuildListItem>();
            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).Returns(buildListItems);
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).Returns(Enumerable.Empty<Record>());

            var execute = cli.Execute(CommandName.Builds, CommandName.Logs, CommandOptionTemplates.IdTemplate, buildId.ToString());

            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).MustHaveHappened();
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).MustHaveHappened();
            Assert.Equal(1, execute);
        }

        [Fact]
        public void should_return_zero_return_code_for_timeline_logs()
        {
            GenFu.A.Configure<BuildListItem>().Fill(x => x.definition, () => GenFu.A.New<Definition>());
            var buildListItems = GenFu.A.New<BuildListItem>();
            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).Returns(buildListItems);
            var records = GenFu.A.ListOf<Record>();
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).Returns(records);

            var execute = cli.Execute(CommandName.Builds, CommandName.Logs, CommandOptionTemplates.IdTemplate, buildId.ToString());

            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).MustHaveHappened();
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).MustHaveHappened();
            Assert.Equal(0, execute);
        }

        [Fact]
        public void should_return_zero_return_code_for_detail_log_flag()
        {
            GenFu.A.Configure<BuildListItem>().Fill(x => x.definition, () => GenFu.A.New<Definition>());
            var buildListItems = GenFu.A.New<BuildListItem>();
            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).Returns(buildListItems);
            var records = GenFu.A.ListOf<Record>();
            records.ForEach(x => x.log = GenFu.A.New<Log>());
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).Returns(records);

            var execute = cli.Execute(CommandName.Builds, CommandName.Logs, CommandOptionTemplates.IdTemplate, buildId.ToString(), CommandOptionTemplates.DetailTemplate);

            A.CallTo(() => adapter.GetBuildDetail(A<string>._, buildId)).MustHaveHappened();
            A.CallTo(() => adapter.GetBuildTimeline(A<string>._, buildListItems.id)).MustHaveHappened();
            A.CallTo(() => adapter.GetBuildLogEntry(A<string>._, A<int>._, A<int>._)).MustHaveHappened();
            Assert.Equal(0, execute);
        }
    }

}