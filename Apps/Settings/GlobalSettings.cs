using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetworkAnalyzer.Apps.Settings
{
    internal class GlobalSettings
    {
        #region System Defaults
        [JsonIgnore]
        public string AppDirectory { get; } = @"C:\Network Analyzer";

        [JsonIgnore]
        public string ReportDirectory { get; } = @"C:\Network Analyzer\Reports";

        [JsonIgnore]
        public string ConfigDirectory { get; } = @"C:\Network Analyzer\Config";

        [JsonIgnore]
        public string LogDirectory { get; } = @"C:\Network Analyzer\Reports";

        [JsonIgnore]
        public string DatabasePath { get; } = @"C:\Network Analyzer\Config\NetworkAnalyzerDB.db";

        [JsonIgnore]
        public string LocalDatabasePath { get; } = "NetworkAnalyzer.Data.NetworkAnalyzerDB.db";

        [JsonIgnore]
        public string ConfigPath { get; } = @"C:\Network Analyzer\Config\config.json";

        [JsonIgnore]
        public string LocalConfigPath { get; } = "NetworkAnalyzer.Data.config.json";

        [JsonIgnore]
        public string CurrentBuild { get; } = "1.6.1";

        [JsonIgnore]
        public string ReleaseDate { get; } = "11/27/2024";

        [JsonIgnore]
        public string LastCheckedForUpdates { get; set; } = string.Empty;
        #endregion System Defaults

        #region User Defaults
        public string DefaultTheme { get; set; } = "Dark";

        public string DefaultAppCloseBehavior { get; set; } = "Close";

        public int MaxHops { get; set; } = 30;

        public string DefaultScanMode { get; set; } = "Auto";

        public string DefaultReportType { get; set; } = "HTML";
        #endregion User Defaults

        public void SavePropertyChanges()
        {
            var json = JsonSerializer.Serialize(
                new {GlobalSettings = this },
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(ConfigPath, json);
        }
    }
}