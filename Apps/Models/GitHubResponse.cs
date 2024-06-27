using System.Text.Json.Serialization;

namespace NetworkAnalyzer.Apps.Models
{
    internal class GitHubResponse
    {
        [JsonPropertyName("LatestVersion")]
        public string LatestVersion { get; set; }

        [JsonPropertyName("LatestReleaseDate")]
        public string LatestReleaseDate { get; set; }

        [JsonPropertyName("VersionInfo")]
        public List<VersionInfo> VersionInfo { get; set; }
    }

    internal class VersionInfo
    {
        [JsonPropertyName("Build")]
        public string Build { get; set; }

        [JsonPropertyName("ReleaseDate")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("ChangeLog")]
        public List<ChangeLog> ChangeLog { get; set; }
    }

    internal class ChangeLog
    {
        [JsonPropertyName("GeneralNotes")]
        public string GeneralNotes { get; set; }

        [JsonPropertyName("NewFeatures")]
        public string NewFeatures { get; set; }

        [JsonPropertyName("BugFixes")]
        public string BugFixes { get; set; }
    }
}
