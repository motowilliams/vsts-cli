using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vsts.Cli
{
    public class VstsApiHelper : IVstsApiHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _pat;

        public VstsApiHelper(Uri baseAddress, string token)
        {
            _httpClient = new HttpClient { BaseAddress = baseAddress };
            _pat = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format($"{String.Empty}:{token}")));
        }

        public async Task<IEnumerable<Repository>> GetRepositories()
        {
            string uri = "DefaultCollection/_apis/git/repositories?api-version=1.0";
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);
            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Repository>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<RepositoryResource>(result);
            return resource.Value.AsEnumerable();
        }

        public async Task<IEnumerable<PullRequest>> GetPullRequests(string repositoryId)
        {
            string uri = "DefaultCollection/_apis/git/repositories/{0}/pullRequests?api-version=3.0";
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(String.Format(uri, repositoryId));

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<PullRequest>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<PullRequesetResource>(result);
            return resource.Value.AsEnumerable();
        }

        public async Task<PullRequest> CreatePullRequest(string repositoryId, string title, string description, string source, string target)
        {
            string uri = $"DefaultCollection/_apis/git/repositories/{repositoryId}/pullRequests?api-version=3.0";

            var document = new
            {
                title = title,
                description = description,
                sourceRefName = $"refs/heads/{source}",
                targetRefName = $"refs/heads/{target}"
            };

            var json = JsonConvert.SerializeObject(document);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            var method = new HttpMethod("POST");
            var request = new HttpRequestMessage(method, uri) { Content = jsonContent };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var pullRequestErrorResource = JsonConvert.DeserializeObject<PullRequestErrorResource>(error);
                Console.WriteLine(pullRequestErrorResource.message, ConsoleColor.Red);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }

        public async Task<IEnumerable<WorkItem>> SearchWorkItems(string projectName, string workItemType, IEnumerable<string> state, IEnumerable<string> tags, string assignedTo = null)
        {
            string uri = $"DefaultCollection/{projectName}/_apis/wit/wiql?api-version=1.0";

            var stateList = string.Join(",", state.Select(x => $"\"{x}\""));

            var filterBuilder = new StringBuilder();
            if (state.Any())
            {
                filterBuilder.Append("(");
                var itemQuery = string.Join(",", state.Select(x => $"\"{x}\""));
                filterBuilder.Append($"[System.State] IN ({itemQuery})");
                filterBuilder.Append(")");
            }

            if (tags.Any())
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append(string.Join(" AND ", tags.Select(tag => $"[System.Tags] Contains \"{tag}\"")));
            }

            if (!string.IsNullOrWhiteSpace(workItemType))
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append($"[System.WorkItemType] = \"{workItemType.NormalizeWorkItemType()}\"");
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                if (filterBuilder.Length > 0)
                    filterBuilder.Append(" AND ");

                filterBuilder.Append($"[System.AssignedTo] CONTAINS \"{assignedTo}\"");
            }

            if (filterBuilder.Length > 0)
                filterBuilder.Insert(0, "AND ");

            string workItemQuery = $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{projectName}\" {filterBuilder} ORDER BY [System.ChangedDate] DESC";

            //string workItemQuery = string.IsNullOrWhiteSpace(workItemType)
            //    ? $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{projectName}\" AND [System.State] IN ({stateList}) ORDER BY [System.ChangedDate] DESC"
            //    : $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{projectName}\" AND [System.State] IN ({stateList}) AND [System.WorkItemType] = \"{workItemType.NormalizeWorkItemType()}\" ORDER BY [System.ChangedDate] DESC";

            var workItemSearchResource = JsonConvert.SerializeObject(new WorkItemSearchResource { Query = workItemQuery });

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.PostAsync(uri, new StringContent(workItemSearchResource, Encoding.ASCII, "application/json"));

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<WorkItem>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<WorkItemResource>(result);
            return resource.WorkItems.AsEnumerable();
        }

        public async Task<IEnumerable<Fields>> GetWorkItemDetail(int workItemId)
        {
            return await GetWorkItemDetail(new[] { workItemId });
        }

        public async Task<IEnumerable<Fields>> GetWorkItemDetail(IEnumerable<int> workItemIds)
        {
            string workItemIdString = String.Join(",", workItemIds.Select(x => x.ToString()));
            var uri = $"DefaultCollection/_apis/wit/WorkItems?ids={workItemIdString}&$expand=all&api-version=1.0";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Fields>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<WorkItemDetailResource>(result);
            return resource.Value.Select(x => x.Fields);
        }

        public async Task<NewWorkItemResource> CreateWorkItem(string projectName, string workItemType, IEnumerable<object> document)
        {
            string uri = $"{projectName}/_apis/wit/workitems/${workItemType.NormalizeWorkItemType()}?api-version=2.2";

            //serialize the fields array into a json string
            var patchValue = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8, "application/json-patch+json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, uri) { Content = patchValue };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<NewWorkItemResource>(result);
            return resource;
        }

        public async Task<IEnumerable<BuildDefinition>> GetBuildList(string projectName)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/definitions";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<BuildDefinition>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<BuildDefinitionResource>(result);
            return resource.value.AsEnumerable();
        }

        public async Task<IEnumerable<BuildListItem>> GetBuildListDetails(string projectName)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<BuildListItem>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<BuildListResource>(result);
            return resource.value.AsEnumerable();
        }

        public async Task<BuildListItem> GetBuildDetail(string projectName, int buildDefinitionId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds?definitions={buildDefinitionId}&statusFilter=completed&$top=1&api-version=2.0";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<BuildListResource>(result);
            return resource.value.FirstOrDefault();
        }

        public async Task<IEnumerable<Record>> GetBuildTimeline(string projectName, int buildId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds/{buildId}/Timeline";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Record>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<TimelineResource>(result);
            return resource.records.OrderBy(x => x.order);
        }

        public async Task<IEnumerable<string>> GetBuildLogEntry(string projectName, int buildId, int logId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds/{buildId}/logs/{logId}";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<string>();

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<LogResource>(result);
            return resource.value;
        }
    }
}