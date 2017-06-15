using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Vsts.Cli
{
    public class VstsApiHelper
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

        public IEnumerable<WorkItem> SearchWorkItems(string projectName, string workItemType, IEnumerable<string> state)
        {
            string uri = $"DefaultCollection/{projectName}/_apis/wit/wiql?api-version=1.0";
            var stateList = string.Join(",", state.Select(x => $"\"{x}\""));

            string workItemQuery = string.IsNullOrWhiteSpace(workItemType) 
                ? $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{projectName}\" AND [System.State] IN ({stateList}) ORDER BY [System.ChangedDate] DESC" 
                : $"SELECT [System.Id] FROM workitems WHERE [System.TeamProject] = \"{projectName}\" AND [System.State] IN ({stateList}) AND [System.WorkItemType] = \"{workItemType.NormalizeWorkItemType()}\" ORDER BY [System.ChangedDate] DESC";

            var workItemSearchResource = JsonConvert.SerializeObject(new WorkItemSearchResource { Query = workItemQuery });

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.PostAsync(uri, new StringContent(workItemSearchResource, Encoding.ASCII, "application/json")).Result;

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
            var uri = $"DefaultCollection/_apis/wit/WorkItems?ids={workItemIdString}&fields=System.Id,System.WorkItemType,System.Title,System.Description,System.AssignedTo,System.State,System.CreatedDate,System.Tags&api-version=1.0";

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _pat);

            HttpResponseMessage response = _httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Fields>();

            var result = response.Content.ReadAsStringAsync().Result;
            var resource = JsonConvert.DeserializeObject<WorkItemDetailResource>(result);
            return resource.Value.Select(x => x.Fields);
        }

        public NewWorkItemResource CreateWorkItem(string projectName, string workItemType, IEnumerable<Object> document)
        {
            string uri = $"{projectName}/_apis/wit/workitems/${workItemType.NormalizeWorkItemType()}?api-version=2.2";

            //serialize the fields array into a json string
            var patchValue = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8, "application/json-patch+json");

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
    }
}