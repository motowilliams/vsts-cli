using System;
using System.Linq;

namespace Vsts.Cli
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Project Project { get; set; }
        public string DefaultBranch { get; set; }
        public string RemoteUrl { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public int Revision { get; set; }
        public string Visibility { get; set; }
    }

    public class ProjectResource
    {
        public int Count { get; set; }
        public Project[] value { get; set; }
    }

    public class RepositoryResource
    {
        public Repository[] Value { get; set; }
    }

    public class PullRequesetResource
    {
        public PullRequest[] Value { get; set; }
    }

    public class PullRequest
    {
        public Repository Repository { get; set; }
        public int PullRequestId { get; set; }
        public int CodeReviewId { get; set; }
        public string Status { get; set; }
        public CreatedBy CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceRefName { get; set; }
        public string TargetRefName { get; set; }
        public string MergeStatus { get; set; }
        public string MergeId { get; set; }
        public Lastmergesourcecommit LastMergeSourceCommit { get; set; }
        public Lastmergetargetcommit LastMergeTargetCommit { get; set; }
        public Lastmergecommit LastMergeCommit { get; set; }
        public Reviewer[] Reviewers { get; set; }
        public string Url { get; set; }
        public bool SupportsIterations { get; set; }
    }

    public class CreatedBy
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
    }

    public class Commit
    {
        public string CommitId { get; set; }
        public string Url { get; set; }
    }

    public class Lastmergesourcecommit : Commit
    {
    }

    public class Lastmergetargetcommit : Commit
    {
    }

    public class Lastmergecommit : Commit
    {
    }

    public class Reviewer
    {
        public string ReviewerUrl { get; set; }
        public int Vote { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UniqueName { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public bool IsContainer { get; set; }
    }


    public class WorkItemResource
    {
        public string QueryType { get; set; }
        public string QueryResultType { get; set; }
        public DateTime AsOf { get; set; }
        public Column[] Columns { get; set; }
        public Sortcolumn[] SortColumns { get; set; }
        public WorkItem[] WorkItems { get; set; }
    }

    public class Column
    {
        public string ReferenceName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Sortcolumn
    {
        public Field Field { get; set; }
        public bool Descending { get; set; }
    }

    public class Field
    {
        public string ReferenceName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class WorkItemSearchResource
    {
        public string Query { get; set; }
    }

    public class WorkItemDetailResource
    {
        public int Count { get; set; }
        public Value[] Value { get; set; }
    }

    public class Value
    {
        public int Id { get; set; }
        public int Rev { get; set; }
        public Fields Fields { get; set; }
        public string Url { get; set; }
    }

    public class Fields
    {
        [Newtonsoft.Json.JsonProperty("System.Id")]
        public int Id { get; set; }
        [Newtonsoft.Json.JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [Newtonsoft.Json.JsonProperty("System.State")]
        public string State { get; set; }
        [Newtonsoft.Json.JsonProperty("System.AssignedTo")]
        public string AssignedTo { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string AssignedToName => AssignedTo?.Split('<').First().Trim();

        [Newtonsoft.Json.JsonProperty("System.Title")]
        public string Title { get; set; }
        [Newtonsoft.Json.JsonProperty("System.Description")]
        public string Description { get; set; }
        [Newtonsoft.Json.JsonProperty("System.CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [Newtonsoft.Json.JsonProperty("System.Tags")]
        public string Tags { get; set; }
    }

    //New work item
    public class NewWorkItemResource
    {
        public int Id { get; set; }
        public int Rev { get; set; }
        public WorkItemFields Fields { get; set; }
        [Newtonsoft.Json.JsonProperty("_links")]
        public Links Links { get; set; }
        public string Url { get; set; }
    }
    public class WorkItemFields
    {
        [Newtonsoft.Json.JsonProperty("System.AreaPath")]
        public string AreaPath { get; set; }
        [Newtonsoft.Json.JsonProperty("System.TeamProject")]
        public string TeamProject { get; set; }
        [Newtonsoft.Json.JsonProperty("System.IterationPath")]
        public string IterationPath { get; set; }
        [Newtonsoft.Json.JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [Newtonsoft.Json.JsonProperty("System.State")]
        public string State { get; set; }
        [Newtonsoft.Json.JsonProperty("System.Reason")]
        public string Reason { get; set; }
        [Newtonsoft.Json.JsonProperty("System.CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [Newtonsoft.Json.JsonProperty("System.CreatedBy")]
        public string CreatedBy { get; set; }
        [Newtonsoft.Json.JsonProperty("System.ChangedDate")]
        public DateTime ChangedDate { get; set; }
        [Newtonsoft.Json.JsonProperty("System.ChangedBy")]
        public string ChangedBy { get; set; }
        [Newtonsoft.Json.JsonProperty("System.Title")]
        public string Title { get; set; }
        [Newtonsoft.Json.JsonProperty("Microsoft.VSTS.Common.StateChangeDate")]
        public DateTime StateChangeDate { get; set; }
        [Newtonsoft.Json.JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int Priority { get; set; }
        [Newtonsoft.Json.JsonProperty("Microsoft.VSTS.Common.Severity")]
        public string Severity { get; set; }
        [Newtonsoft.Json.JsonProperty("Microsoft.VSTS.Common.ValueArea")]
        public string ValueArea { get; set; }
        [Newtonsoft.Json.JsonProperty("System.Description")]
        public string Description { get; set; }
    }
    public class Links
    {
        public Self Self { get; set; }
        public Workitemupdates WorkItemUpdates { get; set; }
        public Workitemrevisions WorkItemRevisions { get; set; }
        public Workitemhistory WorkItemHistory { get; set; }
        public Html Html { get; set; }
        public Workitemtype WorkItemType { get; set; }
        public Fields1 Fields { get; set; }
    }
    public class Self
    {
        public string Href { get; set; }
    }
    public class Workitemupdates
    {
        public string Href { get; set; }
    }
    public class Workitemrevisions
    {
        public string Href { get; set; }
    }
    public class Workitemhistory
    {
        public string Href { get; set; }
    }
    public class Html
    {
        public string Href { get; set; }
    }
    public class Workitemtype
    {
        public string Href { get; set; }
    }
    public class Fields1
    {
        public string Href { get; set; }
    }
}
