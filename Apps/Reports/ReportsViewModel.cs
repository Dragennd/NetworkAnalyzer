using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Settings;
using System.Collections.ObjectModel;

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
            try
            {
                LatencyMonitorHTMLReportHandler newLMReport;
                IPScannerHTMLReportHandler newIPSReport;

                if (SelectedSessionData.Mode == ReportMode.LatencyMonitor)
                {
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

                    await newLMReport.GenerateReport();
                }
                else if (SelectedSessionData.Mode == ReportMode.IPScanner)
                {
                    newIPSReport = new IPScannerHTMLReportHandler(_dbHandler, _settings, SelectedSessionData.ReportGUID);

                    await newIPSReport.GenerateReport();
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
        #endregion Private Methods
    }
}
