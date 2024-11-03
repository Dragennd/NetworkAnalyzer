using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.GlobalClasses;
using static NetworkAnalyzer.Apps.Reports.Functions.ReportExplorerHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        private LogHandler LogHandler { get; set; }

        // Start properties for the Report Explorer
        public ObservableCollection<ReportExplorerData> ReportExplorerData { get; set; }

        [ObservableProperty]
        public ReportExplorerData? selectedReport;
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

        [ObservableProperty]
        public bool isReportMessageVisible = false;

        [ObservableProperty]
        public string message = string.Empty;

        [ObservableProperty]
        public string messageSymbol = string.Empty;
        // End properties for the Report Generator
        #endregion

        public ReportsViewModel()
        {
            ReportExplorerData = new();
            LogHandler = new();
        }

        public async Task GetReportDirectoryContentsAsync()
        {
            try
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
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        [RelayCommand]
        public async Task DeleteReportAsync()
        {
            try
            {
                var dbHandler = new DatabaseHandler();

                await dbHandler.DeleteSelectedReportAsync(SelectedReport.ReportNumber, SelectedReport.Type);
                await GetReportDirectoryContentsAsync();
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        [RelayCommand]
        public async Task ResetDatabaseAsync()
        {
            try
            {
                var dbHandler = new DatabaseHandler();

                await dbHandler.DeleteAllReportDataAsync();

                ReportExplorerData.Clear();
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        [RelayCommand]
        public void OpenReportDirectory()
        {
            Process.Start("explorer.exe", ReportDirectory);
        }

        [RelayCommand]
        public async Task GenerateNewReportAsync()
        {
            try
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

                    await AlertOnReportCreation($"{SelectedReport.ReportNumber}.html");
                }
                else if (IsRBCSVChecked)
                {
                    switch (SelectedReport.Mode)
                    {
                        case ReportMode.LatencyMonitor:
                            var latencyMonitorReport = new LatencyMonitorCSVReportHandler(SelectedReport.ReportNumber);
                            await latencyMonitorReport.GenerateLatencyMonitorCSVReportAsync();
                            break;
                        case ReportMode.IPScanner:
                            var ipScannerReport = new IPScannerCSVReportHandler(SelectedReport.ReportNumber);
                            await ipScannerReport.GenerateIPScannerCSVReportAsync();
                            break;
                    }

                    await AlertOnReportCreation($"{SelectedReport.ReportNumber}.csv");
                }
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        #region Private Methods
        private async Task AlertOnReportCreation(string reportName)
        {
            var sw = new Stopwatch();

            IsReportMessageVisible = true;

            if (File.Exists($"{ReportDirectory}{reportName}"))
            {
                MessageSymbol = "\uE001";
                Message = "Report was successfully created.";
            }
            else
            {
                MessageSymbol = "\uE8BB";
                Message = "An error occurred generating the report.";
            }

            sw.Start();
            await Task.Delay(5000);
            sw.Stop();

            IsReportMessageVisible = false;
        }
        #endregion
    }
}
