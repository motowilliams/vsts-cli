using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Xunit;

namespace Vsts.Cli.Tests
{
    public class for_search_query_states
    {
        private IVsts vsts;

        public for_search_query_states()
        {
            vsts = A.Fake<IVsts>();
        }

        [Fact]
        void should_return_default_states_when_omitted()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(new List<string> { "new", "active" }, searchQuery.State);
        }

        [Fact]
        void should_return_states_passed_to_query()
        {
            var workItemStates = CommandSets.WorkItemState("closed", "resolved");
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(new List<string> { "closed", "resolved" }, searchQuery.State);
        }

        [Fact]
        void should_return_query_type_by_id_for_my_work_item_template()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType("123");
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(WorkItemQueryType.ById, searchQuery.QueryType);
        }

        [Fact]
        void should_return_query_type_by_type_for_my_work_item_template()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType("bug");
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(WorkItemQueryType.ByType, searchQuery.QueryType);
        }

        [Fact]
        void should_return_null_assigned_to_when_my_flag_is_missing()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Null(searchQuery.AssignedTo);
        }

        [Fact]
        void should_return_assigned_to_when_my_flag_is_set()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe("flag-set");
            var fullName = "7b629a58a898444e837e15827598874c";
            A.CallTo(() => vsts.FullName).Returns(fullName);

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(fullName, searchQuery.AssignedTo);
        }

        [Fact]
        void should_return_tags_passed_to_search_object()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags("tag01", "tag02");
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(new List<string> { "tag01", "tag02" }, searchQuery.Tags);
        }

        [Fact]
        void should_return_no_tags_passed_to_search_object()
        {
            var workItemStates = CommandSets.WorkItemState();
            var workItemTags = CommandSets.WorkItemTags();
            var workItemTypeId = CommandSets.WorkItemType();
            var myWorkItemOption = CommandSets.WorkItemForMe();

            SearchQuery searchQuery = new SearchQuery(vsts, workItemStates, workItemTags, workItemTypeId, myWorkItemOption);

            Assert.Equal(Enumerable.Empty<string>(), searchQuery.Tags);
        }
    }
}