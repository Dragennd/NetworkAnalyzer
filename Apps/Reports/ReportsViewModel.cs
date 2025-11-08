using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using System.Collections.ObjectModel;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<ReportExplorerData> AvailableSessionData { get; set; }

        [ObservableProperty]
        public ReportExplorerData selectedSessionData;

        [ObservableProperty]
        public bool isAllDataChecked = true;

        [ObservableProperty]
        public bool isDateRangeChecked = false;

        public Task InitializeAvailableSessions { get; private set; }

        private readonly IDatabaseHandler _dbHandler;

        private readonly IReportsController _reportsController;
        #endregion Control Properties

        public ReportsViewModel(IDatabaseHandler dbHandler, IReportsController reportController)
        {
            AvailableSessionData = new();
            
            _dbHandler = dbHandler;
            _reportsController = reportController;

            reportController.UpdateAvailableSessionData += LoadAvailableSessionDataAsync;
            _reportsController.SendUpdateAvailableSessionDataRequest();
        }

        [RelayCommand]
        public void FetchAvailableSessionDataButton()
        {
            _reportsController.SendUpdateAvailableSessionDataRequest();
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
        #endregion Private Methods
    }
}
