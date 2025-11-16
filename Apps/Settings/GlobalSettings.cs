using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

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
        public string LogDirectory { get; } = @"C:\Network Analyzer\Logs";

        [JsonIgnore]
        public string DatabasePath { get; } = @"C:\Network Analyzer\Config\NetworkAnalyzerDB.db";

        [JsonIgnore]
        public string LocalDatabasePath { get; } = "NetworkAnalyzer.Data.NetworkAnalyzerDB.db";

        [JsonIgnore]
        public string ConfigPath { get; } = @"C:\Network Analyzer\Config\config.json";

        [JsonIgnore]
        public string LocalConfigPath { get; } = "NetworkAnalyzer.Data.config.json";

        [JsonIgnore] // This is used for both the app version and the db version
        public string BuildVersion { get; } = "2.0.0";

        [JsonIgnore]
        public string BuildDate { get; } = "11/27/2024";

        public string LastCheckedForUpdates { get; set; } = "Never";

        [JsonIgnore]
        public string DatabaseSize { get; set; } = string.Empty;
        #endregion System Defaults

        #region User Defaults
        public string DefaultTheme { get; set; } = "Dark";

        public string DefaultAppCloseBehavior { get; set; } = "Close";

        public int DefaultMaxAllowableJitter { get; set; } = 150;

        public int MaxHops { get; set; } = 30;

        public string DefaultScanMode { get; set; } = "Auto";
        #endregion User Defaults

        public void SavePropertyChanges()
        {
            var json = JsonSerializer.Serialize(
                new { GlobalSettings = this },
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(ConfigPath, json);
        }

        public void UpdateActiveTheme()
        {
            var darkModeDictionary = new ResourceDictionary();
            var lightModeDictionary = new ResourceDictionary();

            darkModeDictionary.Source = new Uri("Styles/DarkModeTheme.xaml", UriKind.RelativeOrAbsolute);
            lightModeDictionary.Source = new Uri("Styles/LightModeTheme.xaml", UriKind.RelativeOrAbsolute);

            if (DefaultTheme == "Dark")
            {
                Application.Current.Resources.MergedDictionaries.Remove(lightModeDictionary);
                Application.Current.Resources.MergedDictionaries.Add(darkModeDictionary);
            }
            else if (DefaultTheme == "Light")
            {
                Application.Current.Resources.MergedDictionaries.Remove(darkModeDictionary);
                Application.Current.Resources.MergedDictionaries.Add(lightModeDictionary);
            }
        }
    }
}