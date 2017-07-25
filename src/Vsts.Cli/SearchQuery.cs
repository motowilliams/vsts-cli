using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public class SearchQuery
    {
        public SearchQuery(IVsts vsts, CommandOption workItemStates, CommandOption tags, CommandArgument workItemTypeId, CommandOption myWorkItemOption)
        {
            ProjectName = vsts.ProjectName;
            State = workItemStates.AsStateDefault();
            Tags = tags.HasValue() ? tags.Values : Enumerable.Empty<string>();

            if (Int32.TryParse(workItemTypeId.Value, out int workItemId))
            {
                QueryType = WorkItemQueryType.ById;
                WorkItemId = workItemId;
            }
            else
            {
                QueryType = WorkItemQueryType.ByType;
                WorkItemType = workItemTypeId.Value;
            }

            MyWorkItems = myWorkItemOption.HasValue();
            AssignedTo = myWorkItemOption.HasValue() ? vsts.FullName : null;
        }

        public int? WorkItemId { get; private set; }
        public WorkItemQueryType QueryType { get; private set; }
        public bool MyWorkItems { get; private set; }
        public string ProjectName { get; private set; }
        public string WorkItemType { get; private set; }
        public IEnumerable<string> State { get; private set; }
        public IEnumerable<string> Tags { get; private set; }
        public string AssignedTo { get; private set; }

        public string Query => GetQuery();

        private string GetQuery()
        {
            var filterBuilder = new StringBuilder();
            if (State.Any())
            {
                filterBuilder.Append("(");
                var itemQuery = string.Join(",", State.Select(x => $"\"{x}\""));
                filterBuilder.Append($"[System.State] IN ({itemQuery})");
                filterBuilder.Append(")");
            }

            if (Tags.Any())
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append(string.Join(" AND ", Tags.Select(tag => $"[System.Tags] Contains \"{tag}\"")));
            }

            if (!string.IsNullOrWhiteSpace(WorkItemType))
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append($"[System.WorkItemType] = \"{WorkItemType.Normalize()}\"");
            } else if (WorkItemId.HasValue)
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append($"[System.Id] = \"{WorkItemId.Value}\"");
            }

            if (!string.IsNullOrWhiteSpace(AssignedTo))
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append($"[System.AssignedTo] CONTAINS \"{AssignedTo}\"");
            }

            if (filterBuilder.Length > 0)
                filterBuilder.Insert(0, "AND ");

            string workItemQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{ProjectName}\" {filterBuilder} ORDER BY [System.ChangedDate] DESC";
            return workItemQuery;
        }

        public override string ToString()
        {
            return $"{nameof(WorkItemId)}: {WorkItemId}, {nameof(QueryType)}: {QueryType}, {nameof(MyWorkItems)}: {MyWorkItems}, {nameof(ProjectName)}: {ProjectName}, {nameof(WorkItemType)}: {WorkItemType}, {nameof(State)}: {State}, {nameof(Tags)}: {Tags}, {nameof(AssignedTo)}: {AssignedTo}";
        }

        protected bool Equals(SearchQuery other)
        {
            return WorkItemId == other.WorkItemId && QueryType == other.QueryType && MyWorkItems == other.MyWorkItems && string.Equals(ProjectName, other.ProjectName) && string.Equals(WorkItemType, other.WorkItemType) && Equals(State, other.State) && Equals(Tags, other.Tags) && string.Equals(AssignedTo, other.AssignedTo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SearchQuery) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WorkItemId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) QueryType;
                hashCode = (hashCode * 397) ^ MyWorkItems.GetHashCode();
                hashCode = (hashCode * 397) ^ (ProjectName != null ? ProjectName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WorkItemType != null ? WorkItemType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (State != null ? State.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tags != null ? Tags.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssignedTo != null ? AssignedTo.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}