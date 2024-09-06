using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.Reports.Functions.ReportExplorerHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;
using System.Diagnostics;
using System.IO;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        public ObservableCollection<ReportExplorerData> ReportExplorerData { get; set; }

        [ObservableProperty]
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

            foreach (var report in ReportsData)
            {
                ReportExplorerData.Add(report);
            }
        }

        [RelayCommand]
        public void OpenReport()
        {
            filePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        [RelayCommand]
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

        [RelayCommand]
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

        #region Private Methods

        #endregion
    }
}
