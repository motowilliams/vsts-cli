using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public IEnumerable<Repository> GetRepositories()
        {
            string uri = "DefaultCollection/_apis/git/repositories?api-version=1.0";
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);
            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Repository>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<RepositoryResource>(result);
            return resource.Value.AsEnumerable();
        }

        public IEnumerable<PullRequest> GetPullRequests(string repositoryId)
        {
            string uri = "DefaultCollection/_apis/git/repositories/{0}/pullRequests?api-version=3.0";
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(String.Format(uri, repositoryId)).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<PullRequest>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<PullRequesetResource>(result);
            return resource.Value.AsEnumerable();
        }

        public PullRequest CreatePullRequest(string repositoryId, string title, string description, string source,
            string target)
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

            var response = _httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                var error = response.Content.ReadAsStringAsync().Result;
                var pullRequestErrorResource = JsonConvert.DeserializeObject<PullRequestErrorResource>(error);
                Console.WriteLine(pullRequestErrorResource.message, ConsoleColor.Red);
                return null;
            }

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }

        public IEnumerable<WorkItem> SearchWorkItems(SearchQuery searchQuery)
        {
            string uri = $"DefaultCollection/{searchQuery.ProjectName}/_apis/wit/wiql?api-version=1.0";

            var workItemSearchResource = JsonConvert.SerializeObject(new WorkItemSearchResource { Query = searchQuery.Query });

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient
                .PostAsync(uri, new StringContent(workItemSearchResource, Encoding.ASCII, "application/json")).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<WorkItem>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<WorkItemResource>(result);
            return resource.WorkItems.AsEnumerable();
        }

        public IEnumerable<Fields> GetWorkItemDetail(int workItemId)
        {
            return GetWorkItemDetail(new[] { workItemId });
        }

        public IEnumerable<Fields> GetWorkItemDetail(IEnumerable<int> workItemIds)
        {
            string workItemIdString = String.Join(",", workItemIds.Select(x => x.ToString()));
            var uri = $"DefaultCollection/_apis/wit/WorkItems?ids={workItemIdString}&$expand=all&api-version=1.0";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Fields>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<WorkItemDetailResource>(result);
            return resource.Value.Select(x => x.Fields);
        }

        public NewWorkItemResource CreateWorkItem(string projectName, string workItemType, IEnumerable<object> document)
        {
            string uri = $"{projectName}/_apis/wit/workitems/${workItemType.Normalize()}?api-version=2.2";

            //serialize the fields array into a json string
            var patchValue = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8,
                "application/json-patch+json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, uri) { Content = patchValue };

            var response = _httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode) return null;

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<NewWorkItemResource>(result);
            return resource;
        }

        public BuildListItem QueueBuildDefinition(string projectName, BuildDefinitionQueueResource buildDefinitionQueueResource)
        {
            var uri = $"/DefaultCollection/{projectName}/_apis/build/builds?api-version=2.0";

            //serialize the fields array into a json string
            var serializeObject = JsonConvert.SerializeObject(buildDefinitionQueueResource);
            var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            var method = new HttpMethod("POST");
            var request = new HttpRequestMessage(method, uri) { Content = content };

            var response = _httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode) return null;

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<BuildListItem>(result);
            return resource;


        }

        public IEnumerable<BuildDefinition> GetBuildList(string projectName)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/definitions";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<BuildDefinition>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<BuildDefinitionResource>(result);
            return resource.value.AsEnumerable();
        }

        public IEnumerable<BuildListItem> GetBuildListDetails(string projectName)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<BuildListItem>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<BuildListResource>(result);
            return resource.value.AsEnumerable();
        }

        public BuildListItem GetBuildDetail(string projectName, int buildDefinitionId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds?definitions={buildDefinitionId}&statusFilter=completed&$top=1&api-version=2.0";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return null;

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<BuildListResource>(result);
            return resource.value.FirstOrDefault();
        }

        public IEnumerable<Record> GetBuildTimeline(string projectName, int buildId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds/{buildId}/Timeline";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Record>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<TimelineResource>(result);
            return resource.records.OrderBy(x => x.order);
        }

        public IEnumerable<string> GetBuildLogEntry(string projectName, int buildId, int logId)
        {
            var uri = $"DefaultCollection/{projectName}/_apis/build/builds/{buildId}/logs/{logId}";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<string>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<LogResource>(result);
            return resource.value;
        }
    }
}