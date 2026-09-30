using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.ViewModels;

internal partial class SettingsViewModel : ObservableValidator
{
    public string BuildVersion
    {
        get => GlobalSettings.BuildVersion;
    }

    public string BuildDate
    {
        get => GlobalSettings.BuildDate;
    }

    public string LastCheckedForUpdates
    {
        get => GlobalSettings.LastCheckedForUpdates;
        set
        {
            if (GlobalSettings.LastCheckedForUpdates != value)
            {
                GlobalSettings.LastCheckedForUpdates = value;
                OnPropertyChanged();
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public string DatabaseSize
    {
        get => GlobalSettings.DatabaseSize;
        set
        {
            if (GlobalSettings.DatabaseSize != value)
            {
                GlobalSettings.DatabaseSize = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedTheme
    {
        get => GlobalSettings.DefaultTheme;
        set
        {
            if (GlobalSettings.DefaultTheme != value)
            {
                GlobalSettings.DefaultTheme = value;
                GlobalSettings.SavePropertyChanges();
                //GlobalSettings.UpdateActiveTheme();
            }
        }
    }

    public string SelectedAppCloseBehavior
    {
        get => GlobalSettings.DefaultAppCloseBehavior;
        set
        {
            if (GlobalSettings.DefaultAppCloseBehavior != value)
            {
                GlobalSettings.DefaultAppCloseBehavior = value;
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public string SelectedScanMode
    {
        get => GlobalSettings.DefaultScanMode;
        set
        {
            if (GlobalSettings.DefaultScanMode != value)
            {
                GlobalSettings.DefaultScanMode = value;
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public string SelectedPortScanMode
    {
        get => GlobalSettings.DefaultPortScanMode;
        set
        {
            if (GlobalSettings.DefaultPortScanMode != value)
            {
                GlobalSettings.DefaultPortScanMode = value;
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public int MaxHops
    {
        get => GlobalSettings.MaxHops;
        set
        {
            if (GlobalSettings.MaxHops != value)
            {
                GlobalSettings.MaxHops = value;
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public int MaxJitter
    {
        get => GlobalSettings.DefaultMaxAllowableJitter;
        set
        {
            if (GlobalSettings.DefaultMaxAllowableJitter != value)
            {
                GlobalSettings.DefaultMaxAllowableJitter = value;
                GlobalSettings.SavePropertyChanges();
            }
        }
    }

    public string[] Themes { get; } = { "Dark", "Light" };
    public string[] AppCloseBehaviors { get; } = { "Close", "Minimize" };
    public string[] ScanModes { get; } = { "Auto", "Manual" };
    public string[] PortScanModes { get; } = { "Common", "Full", "Custom" };
    private readonly IDatabaseHandler _dbHandler;
    private GitHubResponse? Response { get; set; }

    public SettingsViewModel(IDatabaseHandler dbHandler)
    {
        _dbHandler = dbHandler;
    }
    
    [RelayCommand]
        public async Task CheckForUpdatesButtonAsync()
        {
            GitHubRequestHandler handler = new();
            Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());
            SetLastCheckedForUpdatesDate();

            if (Response.LatestVersion != BuildVersion)
            {
                // To-Do: Add a notification for this instead of the message box
                // var response = MessageBox.Show(
                //     "A new update is available!\nWould you like to download it now?",
                //     "Update Available",
                //     MessageBoxButton.YesNo,
                //     MessageBoxImage.Information);
                
                // if (response == MessageBoxResult.Yes)
                // {
                //     Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/releases") { UseShellExecute = true });
                // }
            }
            else
            {
                // To-Do: Add a notification for this instead of the message box
                // MessageBox.Show(
                //     "The latest version of Network Analyzer is installed.",
                //     "No Update Available",
                //     MessageBoxButton.OK,
                //     MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        public void LaunchHelpWikiButton() =>
            Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/wiki") { UseShellExecute = true });

        [RelayCommand]
        public async Task ResetAllDatabasesButtonAsync()
        {
            await _dbHandler.DeleteAllReportDataAsync();
            // To-Do: Add a notification for this when the process has completed
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorDatabaseButtonAsync()
        {
            await _dbHandler.ResetLatencyMonitorReportTablesAsync();
            // To-Do: Add a notification for this when the process has completed
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetIPScannerDatabaseButtonAsync()
        {
            await _dbHandler.ResetIPScannerReportTablesAsync();
            // To-Do: Add a notification for this when the process has completed
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorPresetsButtonAsync()
        {
            await _dbHandler.ResetLatencyMonitorPresetsTableAsync();
            // To-Do: Add a notification for this when the process has completed
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public void UpdateDatabaseSizeButton()
        {
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        #region Private Methods
        private void SetLastCheckedForUpdatesDate()
        {
            LastCheckedForUpdates = DateTime.Now.ToString();
        }
        #endregion Private Methods
}