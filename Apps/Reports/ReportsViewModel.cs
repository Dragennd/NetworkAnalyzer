using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.Reports.Functions.ReportExplorerHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        // Start properties for the Report Explorer
        public ObservableCollection<ReportExplorerData> ReportExplorerData { get; set; }

        [ObservableProperty]
        public ReportExplorerData? selectedReport;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?![ ])(?!\b(?:CON|con|PRN|prn|AUX|aux|NUL|nul|COM[1-9]|com[1-9]|LPT[1-9]|lpt[1-9])\b)(?!.*[<>:""\/|?*\x00-\x1F]).+$",
            ErrorMessage = "Filename contains characters or words that are not allowed.\nPlease enter a valid filename.")]
        public string newReportName = string.Empty;
        // End properties for the Report Explorer

        // Start properties for the Report Generator
        [ObservableProperty]
        public bool isRBLatencyMonitorChecked = true;

        [ObservableProperty]
        public bool isRBIPScannerChecked = false;

        [ObservableProperty]
        public bool isRBNetworkSurveyChecked = false;

        [ObservableProperty]
        public bool isRBHTMLChecked = true;

        [ObservableProperty]
        public bool isRBCSVChecked = false;
        // End properties for the Report Generator
        #endregion

        public ReportsViewModel()
        {
            ReportExplorerData = new();
        }

        public async Task GetReportDirectoryContentsAsync()
        {
            ReportExplorerData.Clear();
            ReportsData.Clear();

            await GenerateReportsListAsync();

            var sortedList = ReportsData.OrderByDescending(p => p.Date).ToList();

            foreach (var report in sortedList)
            {
                ReportExplorerData.Add(report);
            }
        }

        [RelayCommand]
        public async Task DeleteReportAsync()
        {
            var dbHandler = new DatabaseHandler();

            await dbHandler.DeleteSelectedReportAsync(SelectedReport.ReportNumber, SelectedReport.Type);
            await GetReportDirectoryContentsAsync();
        }

        [RelayCommand]
        public async Task ResetDatabaseAsync()
        {
            var dbHandler = new DatabaseHandler();

            await dbHandler.DeleteAllReportDataAsync();

            ReportExplorerData.Clear();
        }

        [RelayCommand]
        public void OpenReportDirectory()
        {
            Process.Start("explorer.exe", ReportDirectory);
        }

        [RelayCommand]
        public async Task GenerateNewReportAsync()
        {
            if (IsRBHTMLChecked)
            {
                switch (SelectedReport.Mode)
                {
                    case ReportMode.LatencyMonitor:
                        var latencyMonitorReport = new LatencyMonitorHTMLReportHandler(SelectedReport.ReportNumber);
                        await latencyMonitorReport.GenerateLatencyMonitorHTMLReportAsync();
                        break;
                    case ReportMode.IPScanner:
                        var ipScannerReport = new IPScannerHTMLReportHandler(SelectedReport.ReportNumber);
                        await ipScannerReport.GenerateIPScannerHTMLReportAsync();
                        break;
                }
            }
        }

        #region Private Methods

        #endregion
    }
}
