using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vsts.Cli
{
    public interface IVstsApiHelper
    {
        Task<PullRequest> CreatePullRequest(string repositoryId, string title, string description, string source, string target);
        Task<NewWorkItemResource> CreateWorkItem(string projectName, string workItemType, IEnumerable<object> document);
        Task<BuildListItem> GetBuildDetail(string projectName, int buildDefinitionId);
        Task<IEnumerable<BuildDefinition>> GetBuildList(string projectName);
        Task<IEnumerable<BuildListItem>> GetBuildListDetails(string projectName);
        Task<IEnumerable<string>> GetBuildLogEntry(string projectName, int buildId, int logId);
        Task<IEnumerable<Record>> GetBuildTimeline(string projectName, int buildId);
        Task<IEnumerable<PullRequest>> GetPullRequests(string repositoryId);
        Task<IEnumerable<Repository>> GetRepositories();
        Task<IEnumerable<Fields>> GetWorkItemDetail(IEnumerable<int> workItemIds);
        Task<IEnumerable<Fields>> GetWorkItemDetail(int workItemId);
        Task<IEnumerable<WorkItem>> SearchWorkItems(string projectName, string workItemType, IEnumerable<string> state, IEnumerable<string> tags, string assignedTo = null);
    }
}