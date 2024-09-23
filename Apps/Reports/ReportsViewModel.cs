using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.ComponentModel.DataAnnotations;
using NetworkAnalyzer.Apps.Reports.ReportTemplates;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.Reports.Functions.ReportExplorerHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;
using NetworkAnalyzer.Apps.GlobalClasses;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        #region Control Properties
        // Start properties for the Report Explorer
        public ObservableCollection<ReportExplorerData> ReportExplorerData { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenReportCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowRenameReportGridCommand))]
        [NotifyCanExecuteChangedFor(nameof(ShowConfirmDeleteReportGridCommand))]
        public ReportExplorerData? selectedReport;

        [ObservableProperty]
        public int reportExplorerGridRow = 3;

        [ObservableProperty]
        public int reportExplorerGridRowSpan = 12;

        [ObservableProperty]
        public string controlRowHeight = "*";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?![ ])(?!\b(?:CON|con|PRN|prn|AUX|aux|NUL|nul|COM[1-9]|com[1-9]|LPT[1-9]|lpt[1-9])\b)(?!.*[<>:""\/|?*\x00-\x1F]).+$",
            ErrorMessage = "Filename contains characters or words that are not allowed.\nPlease enter a valid filename.")]
        public string newReportName = string.Empty;

        [ObservableProperty]
        public bool isConfirmDeleteGridVisible = false;

        [ObservableProperty]
        public bool isRenameGridVisible = false;
        // End properties for the Report Explorer

        // Start properties for the Report Generator
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

        private string FilePath { get; set; }
        // End properties for the Report Generator
        #endregion

        public ReportsViewModel()
        {
            ReportExplorerData = new();
            MenuController.optionalControlsVisibility += HideOptionalControlsOnMenuChange;
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
            if (SelectedReport != null)
            {
                FilePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
                Process.Start(new ProcessStartInfo(FilePath) { UseShellExecute = true });
            }
        }

        [RelayCommand(CanExecute = nameof(GetActionStatusForControlBtns))]
        public void ShowRenameReportGrid()
        {
            if (SelectedReport != null && IsConfirmDeleteGridVisible)
            {
                ReportExplorerGridRow = 6;
                ReportExplorerGridRowSpan = 9;
                ControlRowHeight = "15";

                IsRenameGridVisible = true;

                IsConfirmDeleteGridVisible = false;
            }
            else if (SelectedReport != null && !IsRenameGridVisible)
            {
                ReportExplorerGridRow = 6;
                ReportExplorerGridRowSpan = 9;
                ControlRowHeight = "15";

                IsRenameGridVisible = true;
            }
            else
            {
                ReportExplorerGridRow = 3;
                ReportExplorerGridRowSpan = 12;
                ControlRowHeight = "*";

                IsRenameGridVisible = false;
                NewReportName = string.Empty;
            }
        }

        [RelayCommand]
        public async Task RenameReportAsync()
        {
            if (await ValidateUserInputAsync() == false)
            {
                return;
            }

            if (SelectedReport != null)
            {
                FilePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
                string fileExtension = Path.GetExtension(FilePath);
                string adjustedNewReportName = $"{ReportDirectory}{NewReportName}{fileExtension}";
                
                if (FilePath != adjustedNewReportName)
                {
                    File.Move(FilePath, adjustedNewReportName);
                    File.Delete(FilePath);
                }

                ReportExplorerGridRow = 3;
                ReportExplorerGridRowSpan = 12;
                ControlRowHeight = "*";

                IsConfirmDeleteGridVisible = false;
                IsRenameGridVisible = false;
                SelectedReport = null;
                NewReportName = string.Empty;

                await GetReportDirectoryContentsAsync();
            }
        }

        [RelayCommand]
        public void CancelModifyReport()
        {
            ReportExplorerGridRow = 3;
            ReportExplorerGridRowSpan = 12;
            ControlRowHeight = "*";

            IsRenameGridVisible = false;
            IsConfirmDeleteGridVisible = false;
            NewReportName = string.Empty;
        }

        [RelayCommand(CanExecute = nameof(GetActionStatusForControlBtns))]
        public void ShowConfirmDeleteReportGrid()
        {
            if (SelectedReport != null && IsRenameGridVisible)
            {
                ReportExplorerGridRow = 6;
                ReportExplorerGridRowSpan = 9;
                ControlRowHeight = "15";

                IsConfirmDeleteGridVisible = true;

                IsRenameGridVisible = false;
                NewReportName = string.Empty;
            }
            else if (SelectedReport != null && !IsConfirmDeleteGridVisible)
            {
                ReportExplorerGridRow = 6;
                ReportExplorerGridRowSpan = 9;
                ControlRowHeight = "15";

                IsConfirmDeleteGridVisible = true;
            }
            else
            {
                ReportExplorerGridRow = 3;
                ReportExplorerGridRowSpan = 12;
                ControlRowHeight = "*";

                IsConfirmDeleteGridVisible = false;
            }
        }

        [RelayCommand]
        public async Task DeleteReportAsync()
        {
            if (SelectedReport != null)
            {
                FilePath = $"{ReportDirectory}{SelectedReport.ReportNumber}";
                File.Delete(FilePath);

                ReportExplorerGridRow = 3;
                ReportExplorerGridRowSpan = 12;
                ControlRowHeight = "*";

                IsConfirmDeleteGridVisible = false;
                IsRenameGridVisible = false;
                SelectedReport = null;

                await GetReportDirectoryContentsAsync();
            }
        }

        [RelayCommand]
        public void OpenReportDirectory()
        {
            Process.Start("explorer.exe", ReportDirectory);
        }

        [RelayCommand(CanExecute = nameof(GetReportStatusForGenerateBtn))]
        public async Task GenerateNewReportAsync()
        {
            string reportNumber = string.Empty;
            if (IsRBLatencyMonitorChecked && IsRBHTMLChecked)
            {
                reportNumber = await GenerateLatencyMonitorReportNumber();
                var handler = new LatencyMonitorHTMLReportHandler(reportNumber);

                await handler.GenerateLatencyMonitorHTMLReportAsync();
            }
            else if (IsRBIPScannerChecked && IsRBHTMLChecked)
            {
                reportNumber = await GenerateIPScannerReportNumber();
                var handler = new IPScannerHTMLReportHandler(reportNumber);

                await handler.GenerateIPScannerHTMLReport();
            }
            
            await GetReportDirectoryContentsAsync();
        }

        #region Private Methods
        // Generate a report number for the HTML Report following the "LM{0:MMddyyyy.HHmm}" format (e.g. LM08272024.1345)
        private async Task<string> GenerateLatencyMonitorReportNumber() => await Task.FromResult(string.Format("LM{0:MMddyyyy.HHmm}", DateTime.Now));

        private async Task<string> GenerateIPScannerReportNumber() => await Task.FromResult(string.Format("IPS{0:MMddyyyy.HHmm}", DateTime.Now));

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

        private async Task<bool> ValidateUserInputAsync()
        {
            bool status = true;

            // Validate all of the user input fields against regex expressions
            ValidateAllProperties();

            // If the user input fields have errors based on their attributes, return false
            if (HasErrors)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }

        private void HideOptionalControlsOnMenuChange(bool status)
        {
            ReportExplorerGridRow = 3;
            ReportExplorerGridRowSpan = 12;
            ControlRowHeight = "*";

            IsConfirmDeleteGridVisible = false;
            IsRenameGridVisible = false;
            SelectedReport = null;
        }
        #endregion
    }
}
