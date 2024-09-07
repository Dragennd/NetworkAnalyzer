using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.Reports.Functions.ReportExplorerHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<ReportExplorerData> ReportExplorerData { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenReportCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowRenameReportGridCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteReportCommand))]
        public ReportExplorerData? selectedReport;

        [ObservableProperty]
        public int reportExplorerGridRow = 3;

        [ObservableProperty]
        public int reportExplorerGridRowSpan = 12;

        [ObservableProperty]
        public string controlRowHeight = "*";

        [ObservableProperty]
        public string newReportName = string.Empty;

        [ObservableProperty]
        public bool isRenameFieldEnabled = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateNewReportCommand))]
        public bool isRBLatencyMonitorChecked = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateNewReportCommand))]
        public bool isRBIPScannerChecked = false;

        [ObservableProperty]
        public bool isRBNetworkSurveyChecked;

        [ObservableProperty]
        public bool isRBHTMLChecked = true;

        [ObservableProperty]
        public bool isRBCSVChecked = false;

        private string filePath;
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

        [RelayCommand(CanExecute = nameof(GetActionStatusForControlBtns))]
        public void OpenReport()
        {
            filePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        [RelayCommand(CanExecute = nameof(GetActionStatusForControlBtns))]
        public void ShowRenameReportGrid()
        {
            if (SelectedReport != null)
            {
                ReportExplorerGridRow = 6;
                ReportExplorerGridRowSpan = 9;
                ControlRowHeight = "15";
                IsRenameFieldEnabled = true;
            }
        }

        [RelayCommand]
        public async Task RenameReportAsync()
        {
            filePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
            string fileExtension = Path.GetExtension(filePath);
            string adjustedNewReportName = $"{ReportDirectory}{NewReportName}{fileExtension}";
            File.Move(filePath, adjustedNewReportName);
            File.Delete(filePath);

            ReportExplorerGridRow = 3;
            ReportExplorerGridRowSpan = 12;
            ControlRowHeight = "*";
            IsRenameFieldEnabled = false;

            await GetReportDirectoryContentsAsync();
        }

        [RelayCommand]
        public void CancelRenameReport()
        {
            ReportExplorerGridRow = 3;
            ReportExplorerGridRowSpan = 12;
            ControlRowHeight = "*";
            IsRenameFieldEnabled = false;
        }

        [RelayCommand(CanExecute = nameof(GetActionStatusForControlBtns))]
        public async Task DeleteReportAsync()
        {
            filePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
            File.Delete(filePath);

            await GetReportDirectoryContentsAsync();
        }

        [RelayCommand]
        public void OpenReportDirectory()
        {
            Process.Start("explorer.exe", ReportDirectory);
        }

        [RelayCommand(CanExecute = nameof(GetReportStatusForGenerateBtn))]
        public async Task GenerateNewReportAsync()
        {
            if (IsRBLatencyMonitorChecked && IsRBHTMLChecked)
            {
                var reportNumber = await GenerateReportNumber();
                var handler = new HTMLReportHandler(reportNumber);

                await handler.GenerateHTMLReportAsync();
            }
            
            await GetReportDirectoryContentsAsync();
        }

        #region Private Methods
        // Generate a report number for the HTML Report following the "LM{0:MMddyyyy.HHmm}" format (e.g. LM08272024.1345)
        private async Task<string> GenerateReportNumber() => await Task.FromResult(string.Format("LM{0:MMddyyyy.HHmm}", DateTime.Now));

        private bool GetReportStatusForGenerateBtn()
        {
            bool canGenerateReport = false;

            if (IsRBLatencyMonitorChecked && !ReportSessionData.IsEmpty)
            {
                canGenerateReport = true;
            }
            else if (IsRBIPScannerChecked && !ScanResults.IsEmpty)
            {
                canGenerateReport = true;
            }

            return canGenerateReport;
        }

        private bool GetActionStatusForControlBtns()
        {
            bool canUseControlButtons = false;

            if (SelectedReport != null)
            {
                canUseControlButtons = true;
            }

            return canUseControlButtons;
        }
        #endregion
    }
}
