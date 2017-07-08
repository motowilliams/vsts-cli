using System;

namespace Vsts.Cli
{
    public interface IVsts
    {
        string AccountName { get; }

        Uri AccountUri { get; }
        Uri BuildsUri { get; }
        Uri CodeBranchUri { get; }
        Uri CodeUri { get; }
        Uri ProjectUri { get; }
        Uri PullRequestUri { get; }
        Uri ReleasesUri { get; }
        Uri TestManagementUri { get; }
        Uri WorkItemsUri { get; }
        Uri PullRequestIdUri(int pullRequestId);
        Uri WorkItemUri(int id);

        string FullName { get; }
        string GitHost { get; }
        bool IsInProject { get; }
        string LastCommit { get; }
        bool LocalDirectoryLinked { get; }
        bool NeedsAccessToken { get; }
        string PersonalAccessToken { get; }
        string ProjectName { get; }
        string RepositoryBranchName { get; }
        string RepositoryId { get; }
        string RepositoryName { get; }
        
        void AddLocalDirectoryLink(string repositoryName, string repositoryId);
        void CreateNewConfiguration(string repositoryName, string repositoryId, string projectName, string projectId, string personalAccessToken);
        void SetAccessToken(string personalAccessToken);

        void BrowseCodeUri();
        void BrowseCodeBranchUri();
        void BrowseBuildsUri();
        void BrowseReleasesUri();
        void BrowseWorkItemsUri();
        void BrowsePullRequestUri();
        void BrowseTestManagementUri();
        void BrowseProjectUri();
    }
}