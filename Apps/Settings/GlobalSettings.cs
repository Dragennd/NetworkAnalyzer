namespace NetworkAnalyzer.Apps.Settings
{
    internal class GlobalSettings
    {
        #region System Defaults
        public string AppDirectory { get; } = @"C:\Network Analyzer";

        public string ReportDirectory { get; } = @"C:\Network Analyzer\Reports";

        public string ConfigDirectory { get; } = @"C:\Network Analyzer\Config";

        public string LogDirectory { get; } = @"C:\Network Analyzer\Reports";

        public string DatabasePath { get; } = @"C:\Network Analyzer\Config\NetworkAnalyzerDB.db";

        public string LocalDatabasePath { get; } = "NetworkAnalyzer.Data.NetworkAnalyzerDB.db";

        public string ConfigPath { get; } = @"C:\Network Analyzer\Config\config.json";

        public string LocalConfigPath { get; } = "NetworkAnalyzer.Data.config.json";

        public string CurrentBuild { get; } = "1.6.1";

        public string ReleaseDate { get; } = "11/27/2024";
        #endregion System Defaults

        #region User Defaults
        public string DefaultTheme { get; set; } = "Dark";

        public string DefaultAppCloseBehavior { get; set; } = "Close";

        public int MaxHops { get; set; } = 30;

        public string DefaultScanMode { get; set; } = "Auto";

        public string DefaultReportType { get; set; } = "HTML";
        #endregion User Defaults

        public void SavePropertyChanges(string propertyName, object value)
        {
            // To-Do: Add logic to take in the property and the new value and overwrite the existing
            // line item in the config json file with the new value
        }
    }
}