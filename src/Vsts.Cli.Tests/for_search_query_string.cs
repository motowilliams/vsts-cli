using System;
using FakeItEasy;
using Xunit;

namespace Vsts.Cli.Tests
{
    /// <summary>
    /// Naive tests to at at least hit all the query builder legs
    /// </summary>
    public class for_search_query_string
    {
        private string defaultQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"new\",\"active\")) ORDER BY [System.ChangedDate] DESC";
        private string statesQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"closed\",\"resolved\")) ORDER BY [System.ChangedDate] DESC";
        private string workItemIdQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"new\",\"active\")) AND [System.Id] = \"123\" ORDER BY [System.ChangedDate] DESC";
        private string workItemTypeQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"new\",\"active\")) AND [System.WorkItemType] = \"bug\" ORDER BY [System.ChangedDate] DESC";
        private string myWorkItemQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"new\",\"active\")) AND [System.AssignedTo] CONTAINS \"{FullName}\" ORDER BY [System.ChangedDate] DESC";
        private string tagsQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" AND ([System.State] IN (\"new\",\"active\")) AND [System.Tags] Contains \"tag01\" AND [System.Tags] Contains \"tag02\" ORDER BY [System.ChangedDate] DESC";
        
        private IVsts vsts;
        private const string ProjectName = "TestProject3f4c049e87054e8590d56147b4073042";
        private const string FullName = "7b629a58a898444e837e15827598874c";

        public for_search_query_string()
        {
            vsts = A.Fake<IVsts>();
            A.CallTo(() => vsts.ProjectName).Returns(ProjectName);
            A.CallTo(() => vsts.FullName).Returns(FullName);
        }

        [Fact]
        void should_return_default_states_when_omitted()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(defaultQuery, searchQuery.Query);
        }

        [Fact]
        void should_return_states_passed_to_query()
        {
            var workItemStates = CommandSets.WorkItemState("closed", "resolved");
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(statesQuery, searchQuery.Query);
        }

        [Fact]
        void should_return_query_type_by_id_for_my_work_item_template()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType("123");
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(workItemIdQuery, searchQuery.Query);
        }

        [Fact]
        void should_return_query_type_by_type_for_my_work_item_template()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType("bug");
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(workItemTypeQuery, searchQuery.Query);
        }

        [Fact]
        void should_return_assigned_to_when_my_flag_is_set()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe("flag-set");

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(myWorkItemQuery, searchQuery.Query);
        }

        [Fact]
        void should_return_tags_passed_to_search_object()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags("tag01", "tag02");
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(tagsQuery, searchQuery.Query);
        }
    }
}