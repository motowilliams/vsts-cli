using System.Collections.Generic;

namespace Vsts.Cli
{
    public interface IVstsApiHelper
    {
        PullRequest CreatePullRequest(string repositoryId, string title, string description, string source, string target);
        NewWorkItemResource CreateWorkItem(string projectName, string workItemType, IEnumerable<object> document);
        BuildListItem GetBuildDetail(string projectName, int buildDefinitionId);
        IEnumerable<BuildDefinition> GetBuildList(string projectName);
        IEnumerable<BuildListItem> GetBuildListDetails(string projectName);
        IEnumerable<string> GetBuildLogEntry(string projectName, int buildId, int logId);
        IEnumerable<Record> GetBuildTimeline(string projectName, int buildId);
        IEnumerable<PullRequest> GetPullRequests(string repositoryId);
        IEnumerable<Repository> GetRepositories();
        IEnumerable<Fields> GetWorkItemDetail(IEnumerable<int> workItemIds);
        IEnumerable<Fields> GetWorkItemDetail(int workItemId);
        IEnumerable<WorkItem> SearchWorkItems(string projectName, string workItemType, IEnumerable<string> state, IEnumerable<string> tags, string assignedTo = null);
        BuildListItem QueueBuildDefinition(string projectName, BuildDefinitionQueueResource buildDefinitionQueueResource);
    }
}