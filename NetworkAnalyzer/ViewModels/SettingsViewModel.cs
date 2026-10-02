using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer.EventControllers;
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
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();

    public SettingsViewModel(IDatabaseHandler dbHandler)
    {
        _dbHandler = dbHandler;
        DatabaseSize = _dbHandler.GetDatabaseSize();
    }
    
    [RelayCommand]
        public async Task CheckForUpdatesButtonAsync()
        {
            GitHubRequestHandler handler = new();
            Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());
            SetLastCheckedForUpdatesDate();

            if (Response.LatestVersion != BuildVersion)
            {
                var response =
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Update Available",
                        "A new update is available!\nWould you like to download it now?",
                            ButtonEnum.YesNo).ShowAsync();
                
                if (response == ButtonResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/releases") { UseShellExecute = true });
                }
            }
            else
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        $"No Update Available", 
                        "The latest version of Network Analyzer is installed.", 
                        NotificationType.Info));
            }
        }

        [RelayCommand]
        public void LaunchHelpWikiButton() =>
            Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/wiki") { UseShellExecute = true });

        [RelayCommand]
        public async Task ResetAllDatabasesButtonAsync()
        {
            try
            {
                await _dbHandler.DeleteAllReportDataAsync();
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset All Databases",
                       "All databases have been successfully reset.",
                        NotificationType.Info));
            }
            catch
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset All Databases", 
                       $"An error has occurred while resetting the databases. See the log file under {GlobalSettings.LogDirectory} for more info.",
                        NotificationType.Error));
            }
            
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorDatabaseButtonAsync()
        {
            try
            {
                await _dbHandler.ResetLatencyMonitorReportTablesAsync();
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset Latency Monitor Database",
                       "The Latency Monitor database has been successfully reset.",
                        NotificationType.Info));
            }
            catch
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset Latency Monitor Database", 
                       $"An error has occurred while resetting the Latency Monitor database. See the log file under {GlobalSettings.LogDirectory} for more info.",
                        NotificationType.Error));
            }
            
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetIPScannerDatabaseButtonAsync()
        {
            try
            {
                await _dbHandler.ResetIPScannerReportTablesAsync();
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset IP Scanner Database",
                        "The IP Scanner database has been successfully reset.",
                        NotificationType.Info));
            }
            catch
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Reset IP Scanner Database", 
                        $"An error has occurred while resetting the IP Scanner database. See the log file under {GlobalSettings.LogDirectory} for more info.",
                        NotificationType.Error));
            }
            
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorPresetsButtonAsync()
        {
            try
            {
                await _dbHandler.ResetLatencyMonitorPresetsTableAsync();
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Clear Latency Monitor Presets",
                        "The Latency Monitor Presets have been successfully cleared.",
                        NotificationType.Info));
            }
            catch
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Clear Latency Monitor Presets", 
                        $"An error has occurred while clearing the Latency Monitor Presets. See the log file under {GlobalSettings.LogDirectory} for more info.",
                        NotificationType.Error));
            }
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