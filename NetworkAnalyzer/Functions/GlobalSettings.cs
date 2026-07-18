using System;
using System.IO;
using System.Text.Json;

namespace NetworkAnalyzer.Functions;

internal class GlobalSettings
{
    #region System Defaults
    private static readonly JsonSerializerOptions JsonOptions = new (){ WriteIndented = true };
    private static readonly string HomeFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string AppDirectory { get; } = Path.Combine(HomeFolderPath, "Network Analyzer");
    public static string ReportDirectory { get; } = Path.Combine(AppDirectory, "Reports");
    public static string ConfigDirectory { get; } = Path.Combine(AppDirectory, "Config");
    public static string LogDirectory { get; } = Path.Combine(AppDirectory, "Logs");
    public static string DatabasePath { get; } = Path.Combine(ConfigDirectory, "NetworkAnalyzerDB.db");
    public static string ConfigPath { get; } = Path.Combine(ConfigDirectory, "config.json");
    public static string LocalDatabasePath { get; } = "NetworkAnalyzer_UI_Test.Data.NetworkAnalyzerDB.db";
    public static string LocalConfigPath { get; } = "NetworkAnalyzer_UI_Test.Data.config.json";
    public static string BuildVersion { get; } = "3.0.0"; // This is used for both the app version and the db version
    public static string BuildDate { get; } = "12/22/2025";
    public static string LastCheckedForUpdates { get; set; } = "Never";
    public static string DatabaseSize { get; set; } = string.Empty;
    #endregion System Defaults

    #region User Defaults
    public static string DefaultTheme { get; set; } = "Dark";
    public static string DefaultAppCloseBehavior { get; set; } = "Close";
    public static int DefaultMaxAllowableJitter { get; set; } = 150;
    public static int MaxHops { get; set; } = 30;
    public static string DefaultScanMode { get; set; } = "Auto";
    #endregion User Defaults

    public static void SavePropertyChanges()
    {
        var snapshot = new
        {
            LastCheckedForUpdates,
            DefaultTheme,
            DefaultAppCloseBehavior,
            DefaultMaxAllowableJitter,
            MaxHops,
            DefaultScanMode
        };
        
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        File.WriteAllText(ConfigPath, json);
    }
}