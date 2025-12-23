using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Settings;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<ReportExplorerData> AvailableSessionData { get; set; }

        public ObservableCollection<LatencyMonitorReportEntries> AvailableUserDefinedTargets { get; set; }

        [ObservableProperty]
        public ReportExplorerData selectedSessionData;

        [ObservableProperty]
        public LatencyMonitorReportEntries selectedUserDefinedTarget;

        [ObservableProperty]
        public string startTime;

        [ObservableProperty]
        public string endTime;

        [ObservableProperty]
        public bool isAllDataChecked = true;

        [ObservableProperty]
        public bool isDateRangeChecked = false;

        [ObservableProperty]
        public bool isStartTimePickerVisible = false;

        [ObservableProperty]
        public bool isEndTimePickerVisible = false;

        [ObservableProperty]
        public bool isLatencyMonitorReportOptionsGridVisible = false;

        public Task InitializeAvailableSessions { get; private set; }

        private readonly IDatabaseHandler _dbHandler;

        private readonly IReportsController _reportsController;

        private readonly GlobalSettings _settings;

        private readonly LogHandler _logHandler;
        #endregion Control Properties

        public ReportsViewModel(IDatabaseHandler dbHandler, IReportsController reportsController, GlobalSettings settings, LogHandler logHandler)
        {
            AvailableSessionData = new();
            AvailableUserDefinedTargets = new();
            
            _dbHandler = dbHandler;
            _reportsController = reportsController;
            _settings = settings;
            _logHandler = logHandler;

            _reportsController.UpdateAvailableSessionData += LoadAvailableSessionDataAsync;
            _reportsController.SetUserDefinedTargetData += FetchUserDefinedTargetsForSelectedSession;

            _reportsController.SendUpdateAvailableSessionDataRequest();
        }

        [RelayCommand]
        public async Task GenerateReportButtonAsync()
        {
            string filename = string.Empty;
            if (SelectedSessionData == null)
            {
                return;
            }

            try
            {
                if (SelectedSessionData.Mode == ReportMode.LatencyMonitor)
                {
                    LatencyMonitorHTMLReportHandler newLMReport;

                    if (SelectedUserDefinedTarget == null)
                    {
                        return;
                    }

                    if (IsDateRangeChecked)
                    {
                        newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, _settings, SelectedSessionData.ReportGUID, SelectedUserDefinedTarget.TracerouteGUID, IsDateRangeChecked, StartTime, EndTime);
                    }
                    else
                    {
                        newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, _settings, SelectedSessionData.ReportGUID, SelectedUserDefinedTarget.TracerouteGUID);
                    }

                    filename = await newLMReport.GenerateReport();
                }
                else if (SelectedSessionData.Mode == ReportMode.IPScanner)
                {
                    IPScannerHTMLReportHandler newIPSReport = new(_dbHandler, _settings, SelectedSessionData.ReportGUID);

                    filename = await newIPSReport.GenerateReport();
                }

                if (VerifyReportGenerated(filename))
                {
                    DisplayMessage(
                        LogType.Info,
                        "Report Generated Successfully",
                        $"Report {filename.Split('\\')[3]} was generated successfully.",
                        MessageBoxImage.Information
                    );

                    OpenReportsDirectory();
                }
                else
                {
                    DisplayMessage(
                        LogType.Error,
                        "Report Generation Failed",
                        $"Report {filename.Split('\\')[3]} failed to generate. See logs for details.",
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error);
            }
        } 

        [RelayCommand]
        public void FetchAvailableSessionDataButton()
        {
            IsLatencyMonitorReportOptionsGridVisible = false;
            _reportsController.SendUpdateAvailableSessionDataRequest();
        }

        [RelayCommand]
        public void ShowStartTimePickerButton()
        {
            IsStartTimePickerVisible = !IsStartTimePickerVisible;
        }

        [RelayCommand]
        public void ShowEndTimePickerButton()
        {
            IsEndTimePickerVisible = !IsEndTimePickerVisible;
        }

        #region Private Methods
        private async void LoadAvailableSessionDataAsync()
        {
            AvailableSessionData.Clear();

            foreach (var entry in await _dbHandler.GetLatencyMonitorReportsAsync())
            {
                AvailableSessionData.Add(entry);
            }

            foreach (var entry in await _dbHandler.GetIPScannerReportsAsync())
            {
                AvailableSessionData.Add(entry);
            }
        }

        private async void FetchUserDefinedTargetsForSelectedSession(string data)
        {
            foreach (var address in await _dbHandler.GetDistinctLatencyMonitorUserDefinedTargetsAsync(data))
            {
                AvailableUserDefinedTargets.Add(address);
            }
        }

        partial void OnSelectedSessionDataChanged(ReportExplorerData value)
        {
            AvailableUserDefinedTargets.Clear();

            if (SelectedSessionData == null)
            {
                return;
            }

            if (SelectedSessionData.Mode == ReportMode.LatencyMonitor)
            {
                _reportsController.SendSetUserDefinedTargetDataRequest(value.ReportGUID);
                IsLatencyMonitorReportOptionsGridVisible = true;
                IsDateRangeChecked = false;
                IsAllDataChecked = true;
            }
            else
            {
                IsLatencyMonitorReportOptionsGridVisible = false;
                IsDateRangeChecked = false;
                IsAllDataChecked = true;
            }
        }

        private void OpenReportsDirectory()
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                FileName = "explorer.exe",
                Arguments = $"{_settings.ReportDirectory}"
            };

            process.StartInfo = startInfo;
            process.Start();
        }

        private bool VerifyReportGenerated(string reportFilename) =>
            File.Exists(reportFilename);

        private void DisplayMessage(LogType logType, string title, string message, MessageBoxImage messageBoxImage) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, messageBoxImage);
        #endregion Private Methods
    }
}
