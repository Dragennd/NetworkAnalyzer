using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.Configuration;

namespace NetworkAnalyzer.Functions;

internal class GlobalSettings
{
    #region System Defaults
    private static readonly JsonSerializerOptions JsonOptions = new (){ WriteIndented = true };
    private static readonly string HomeFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents");
    public static string AppDirectory { get; } = Path.Combine(HomeFolderPath, "Network Analyzer");
    public static string ReportDirectory { get; } = Path.Combine(AppDirectory, "Reports");
    public static string ConfigDirectory { get; } = Path.Combine(AppDirectory, "Config");
    public static string LogDirectory { get; } = Path.Combine(AppDirectory, "Logs");
    public static string DatabasePath { get; } = Path.Combine(ConfigDirectory, "NetworkAnalyzerDB.db");
    public static string ConfigPath { get; } = Path.Combine(ConfigDirectory, "config.json");
    public static string LocalDatabasePath { get; } = "NetworkAnalyzer.Data.NetworkAnalyzerDB.db";
    public static string LocalConfigPath { get; } = "NetworkAnalyzer.Data.config.json";
    public static string BuildVersion { get; } = "3.0.0"; // This is used for both the app version and the db version
    public static string BuildDate { get; } = "10/1/2026";
    public static string LastCheckedForUpdates { get; set; } = "Never";
    public static string DatabaseSize { get; set; } = string.Empty;
    #endregion System Defaults

    #region User Defaults
    public static string DefaultTheme { get; set; } = "Dark";
    public static int DefaultMaxAllowableJitter { get; set; } = 150;
    public static int MaxHops { get; set; } = 30;
    public static string DefaultScanMode { get; set; } = "Auto";
    public static string DefaultPortScanMode { get; set; } = "Common";
    public static string DefaultReportGenerationMode { get; set; } = "CSV";
    #endregion User Defaults

    public static void SavePropertyChanges()
    {
        var snapshot = new
        {
            LastCheckedForUpdates,
            DefaultTheme,
            DefaultMaxAllowableJitter,
            MaxHops,
            DefaultScanMode,
            DefaultPortScanMode,
            DefaultReportGenerationMode
        };
        
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        File.WriteAllText(ConfigPath, json);
    }

    public static void LoadSettings(IConfiguration config)
    {
        DefaultTheme = config.GetValue<string>("GlobalSettings:DefaultTheme")!;
        DefaultMaxAllowableJitter = config.GetValue<int>("GlobalSettings:DefaultMaxAllowableJitter")!;
        MaxHops = config.GetValue<int>("GlobalSettings:MaxHops");
        DefaultScanMode = config.GetValue<string>("GlobalSettings:DefaultScanMode")!;
        DefaultPortScanMode = config.GetValue<string>("GlobalSettings:DefaultPortScanMode")!;
        LastCheckedForUpdates = config.GetValue<string>("GlobalSettings:LastCheckedForUpdates")!;
        DefaultReportGenerationMode = config.GetValue<string>("GlobalSettings:DefaultReportGenerationMode")!;
        
        UpdateTheme();
    }

    public static void UpdateTheme()
    {
        Application.Current.RequestedThemeVariant = DefaultTheme switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Dark
        };
    }
}