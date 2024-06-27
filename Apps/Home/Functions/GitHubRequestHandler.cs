using System.Net.Http;
using System.Text.Json;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.ExtensionsHandler;

namespace NetworkAnalyzer.Apps.Home.Functions
{
    internal class GitHubRequestHandler
    {
        public async Task<string> GetRepositoryManifest()
        {
            string encodedResponse = string.Empty;

            using (HttpClient client = new())
            {
                string owner = "Dragennd";
                string repo = "NetworkAnalyzer";
                string path = "manifest.json";

                client.DefaultRequestHeaders.UserAgent.ParseAdd("NetworkAnalyzer");

                // Send API request to GitHub and pull the manifest from the NetworkAnalyzer Repository
                HttpResponseMessage response = await client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/contents/{path}");

                // Parse the string response, decode the Base64 encoded content response and convert it into a usable string
                JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.TryGetProperty("content", out JsonElement contentElement);

                encodedResponse = contentElement.ToString();
            }

            return encodedResponse;
        }

        public async Task<GitHubResponse> ProcessEncodedResponse(string encodedData)
        {
            GitHubResponse? response = JsonSerializer.Deserialize<GitHubResponse>(encodedData.DecodeBase64());

            return await Task.FromResult(response);
        }
    }
}
