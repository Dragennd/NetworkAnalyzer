using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NetworkAnalyzer.ExtensionMethods;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class GitHubRequestHandler
{
    private readonly HttpClient _client = new();
        
    public async Task<string> GetRepositoryManifest()
    {
        const string owner = "Dragennd";
        const string repo = "NetworkAnalyzer";
        const string path = "manifest.json";

        _client.DefaultRequestHeaders.UserAgent.ParseAdd("NetworkAnalyzer");

        // Send API request to GitHub and pull the manifest from the NetworkAnalyzer Repository
        HttpResponseMessage response = await _client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/contents/{path}");

        // Parse the string response, decode the Base64 encoded content response and convert it into a usable string
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.TryGetProperty("content", out JsonElement contentElement);

        return contentElement.ToString();
    }

    public async Task<GitHubResponse> ProcessEncodedResponse(string encodedData)
    {
        GitHubResponse? response = JsonSerializer.Deserialize<GitHubResponse>(encodedData.DecodeBase64());

        return await Task.FromResult(response);
    }
}