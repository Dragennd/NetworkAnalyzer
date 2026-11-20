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
    private static readonly string HomeFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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
    public static string DefaultTheme
    {
        get;
        set
        {
            field = value;
            UpdateTheme();
        }
    } = "Dark";
    public static int DefaultMaxAllowableJitter { get; set; } = 150;
    public static int MaxHops { get; set; } = 30;
    public static string DefaultScanMode { get; set; } = "Auto";
    public static string DefaultPortScanMode { get; set; } = "Common";
    #endregion User Defaults

    public static void SavePropertyChanges()
    {
        // To-Do: Investigate why LastCheckedForUpdates isn't being updated from the config.json file
        var snapshot = new
        {
            LastCheckedForUpdates,
            DefaultTheme,
            DefaultMaxAllowableJitter,
            MaxHops,
            DefaultScanMode,
            DefaultPortScanMode
        };
        
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);

        File.WriteAllText(ConfigPath, json);
    }

    public static void LoadSettings(IConfiguration config)
    {
        DefaultTheme = config.GetValue<string>(nameof(DefaultTheme))!;
        DefaultMaxAllowableJitter = config.GetValue<int>(nameof(DefaultMaxAllowableJitter))!;
        MaxHops = config.GetValue<int>(nameof(MaxHops));
        DefaultScanMode = config.GetValue<string>(nameof(DefaultScanMode))!;
        DefaultPortScanMode = config.GetValue<string>(nameof(DefaultPortScanMode))!;
        LastCheckedForUpdates = config.GetValue<string>(nameof(LastCheckedForUpdates))!;
    }

    private static void UpdateTheme()
    {
        Application.Current.RequestedThemeVariant = DefaultTheme switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Dark
        };
    }
}