using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Home.Functions;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;

namespace NetworkAnalyzer.Apps.Settings
{
    internal partial class SettingsViewModel : ObservableValidator
    {
        #region Control Properties
        public string BuildVersion
        {
            get => _settings.BuildVersion;
        }

        public string BuildDate
        {
            get => _settings.BuildDate;
        }

        public string LastCheckedForUpdates
        {
            get => _settings.LastCheckedForUpdates;
            set
            {
                if (_settings.LastCheckedForUpdates != value)
                {
                    _settings.LastCheckedForUpdates = value;
                }
            }
        }

        public string DatabaseSize
        {
            get => _settings.DatabaseSize;
            set
            {
                if (_settings.DatabaseSize != value)
                {
                    _settings.DatabaseSize = value;
                }
            }
        }

        public string SelectedTheme
        {
            get => _settings.DefaultTheme;
            set
            {
                if (_settings.DefaultTheme != value)
                {
                    _settings.DefaultTheme = value;
                    _settings.SavePropertyChanges();
                    _settings.UpdateActiveTheme();
                }
            }
        }

        public string SelectedAppCloseBehavior
        {
            get => _settings.DefaultAppCloseBehavior;
            set
            {
                if (_settings.DefaultAppCloseBehavior != value)
                {
                    _settings.DefaultAppCloseBehavior = value;
                    _settings.SavePropertyChanges();
                }
            }
        }

        public string SelectedScanMode
        {
            get => _settings.DefaultScanMode;
            set
            {
                if (_settings.DefaultScanMode != value)
                {
                    _settings.DefaultScanMode = value;
                    _settings.SavePropertyChanges();
                }
            }
        }

        public string SelectedReportType
        {
            get => _settings.DefaultReportType;
            set
            {
                if (_settings.DefaultReportType != value)
                {
                    _settings.DefaultReportType = value;
                    _settings.SavePropertyChanges();
                }
            }
        }

        public int MaxHops
        {
            get => _settings.MaxHops;
            set
            {
                if (_settings.MaxHops != value)
                {
                    _settings.MaxHops = value;
                    _settings.SavePropertyChanges();
                }
            }
        }

        public string[] Themes { get; } = { "Dark", "Light" };

        public string[] AppCloseBehaviors { get; } = { "Close", "Minimize" };

        public string[] ScanModes { get; } = { "Auto", "Manual" };

        public string[] ReportTypes { get; } = { "HTML", "CSV" };

        private readonly GlobalSettings _settings;

        private readonly IDatabaseHandler _dbHandler;

        private GitHubResponse? Response { get; set; }
        #endregion Control Properties

        public SettingsViewModel(IOptions<GlobalSettings> options, IDatabaseHandler dbHandler)
        {
            _settings = options.Value;
            _dbHandler = dbHandler;
            DatabaseSize = _dbHandler.GetDatabaseSize();
        }

        [RelayCommand]
        public async Task CheckForUpdatesButtonAsync()
        {
            GitHubRequestHandler handler = new();
            Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());

            if (Response.LatestVersion != BuildVersion)
            {
                MessageBox.Show(
                    "A new update is available!\nWould you like to download it now?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "The latest version of Network Analyzer is installed.",
                    "No Update Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        public void LaunchHelpWikiButton() =>
            Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/wiki") { UseShellExecute = true });

        [RelayCommand]
        public async Task ResetAllDatabasesButtonAsync()
        {
            // To-Do: Add logic to reset all database tables
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorDatabaseButtonAsync()
        {
            // To-Do: Add logic to reset the LatencyMonitorReportEntries database table
        }

        [RelayCommand]
        public async Task ResetIPScannerDatabaseButtonAsync()
        {
            // To-Do: Add logic to reset the IPScannerReportEntries database table
        }

        [RelayCommand]
        public async Task ResetLatencyMonitorPresetsButtonAsync()
        {
            // To-Do: Add logic to reset the LatencyMonitorPresets database table
        }

        [RelayCommand]
        public void UpdateDatabaseSizeButton()
        {
            _dbHandler.GetDatabaseSize();
        }

        #region Private Methods

        #endregion Private Methods
    }
}
