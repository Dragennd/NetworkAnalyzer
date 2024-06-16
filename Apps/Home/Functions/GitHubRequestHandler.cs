﻿using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.ExtensionsHandler;

namespace NetworkAnalyzer.Apps.Home.Functions
{
    public class GitHubRequestHandler
    {
        public async Task<string> GetRepositoryManifest()
        {
            string encodedResponse = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                string owner = "Dragennd";
                string repo = "NetworkAnalyzer";
                string path = "manifest.json";

                client.DefaultRequestHeaders.UserAgent.ParseAdd("NetworkAnalyzer");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");

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
            GitHubResponse response = JsonSerializer.Deserialize<GitHubResponse>(encodedData.DecodeBase64());

            return await Task.FromResult(response);
        }
    }
}
