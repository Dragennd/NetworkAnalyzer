using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public partial class LatencyMonitorViewModel : ObservableValidator
    {
        #region Control Properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateHTMLReportCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopMonitoringSessionCommand))]
        [NotifyCanExecuteChangedFor(nameof(LatencyMonitorManagerCommand))]
        private bool isRunning = false;

        [ObservableProperty]
        public bool simpleMode = true;

        [ObservableProperty]
        public bool detailedMode = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateHTMLReportCommand))]
        public int packetsSentInThisSession = 0;

        [ObservableProperty]
        public string sessionStatus = "IDLE";

        [ObservableProperty]
        public Brush sessionStatusDisplayColor = Brushes.Red;

        [ObservableProperty]
        public string? dnsHostEntryResolution;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "The field cannot be empty.\nPlease enter a valid IP Address or DNS Name.")]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string target1;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target2;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target3;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target4;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target5;

        [ObservableProperty]
        public LatencyMonitorData dataKey1;

        [ObservableProperty]
        public LatencyMonitorData dataKey2;

        [ObservableProperty]
        public LatencyMonitorData dataKey3;

        [ObservableProperty]
        public LatencyMonitorData dataKey4;

        [ObservableProperty]
        public LatencyMonitorData dataKey5;

        #endregion

        // Command to execute when the Start button is clicked
        [RelayCommand]
        public async Task LatencyMonitorManagerAsync()
        {
            if (await ValidateUserInputAsync() == false)
            {
                return;
            }

            ClearPreviousSessionResults();
            SetSessionTargets();
            SetSessionStatus();
            await StartMonitoringSessionAsync();
        }

        // Command to execute when the Switch Modes button is clicked
        [RelayCommand]
        public void SwitchDisplayModes()
        {
            SimpleMode = !SimpleMode;
            DetailedMode = !DetailedMode;
        }

        // Command to execute when the Stop button is clicked
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForStopBtn))]
        public void StopMonitoringSession() => SetSessionStatus();

        // Command to execute when the Generate Report button is clicked
        [RelayCommand(CanExecute = nameof(GetReportDataStatus))]
        public void GenerateHTMLReport()
        {
            HTMLReportHandler.GenerateHTMLReport();
            MessageBox.Show("Report has been created at C:\\BWIT\\" + HTMLReportHandler.GenerateReportNumber() + ".html",
                            "Report Created",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.OK);

            try
            {
                Process.Start("explorer.exe", @"C:\BWIT");
            }
            catch (InvalidOperationException)
            {
                // Do nothing
                // If File Explorer fails to open, its not a big deal
                // The report should still be generated in the designated directory
            }
            catch (Win32Exception)
            {
                // Do nothing
                // If File Explorer fails to open, its not a big deal
                // The report should still be generated in the designated directory
            }
        }

        #region Private Methods
        // Update the UI with the latest info in the LiveData Dictionary
        private void UpdateUI()
        {
            if (!string.IsNullOrWhiteSpace(Target1))
            {
                DataKey1 = LiveData[Target1].LastOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(Target2))
            {
                DataKey2 = LiveData[Target2].LastOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(Target3))
            {
                DataKey3 = LiveData[Target3].LastOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(Target4))
            {
                DataKey4 = LiveData[Target4].LastOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(Target5))
            {
                DataKey5 = LiveData[Target5].LastOrDefault();
            }

            PacketsSentInThisSession = PacketsSent;
        }

        // Set the visual for the Status field depending on whether the session is active or not
        private void SetSessionStatus()
        {
            IsRunning = !IsRunning;

            if (IsRunning)
            {
                SessionStatus = "RUNNING";
                SessionStatusDisplayColor = Brushes.Green;
            }
            else
            {
                SessionStatus = "IDLE";
                SessionStatusDisplayColor = Brushes.Red;
            }
        }

        // Starts a session of the Latency Monitor
        private async Task StartMonitoringSessionAsync()
        {
            do
            {
                List<Task> task = new();
                PacketsSent++;

                foreach (string ipAddress in IPAddresses)
                {
                    // Use this segment if it is the first run and the dictionary hasn't been initiated yet
                    if (!LiveData.ContainsKey(ipAddress))
                    {
                        task.Add(LatencyMonitorFunction.InitializeDataAsync(ipAddress));
                        continue;
                    }

                    task.Add(LatencyMonitorFunction.ProcessDataAsync(ipAddress));
                }

                await Task.WhenAll(task);

                UpdateUI();

                await Task.Delay(1000);
            } while (IsRunning);

            foreach (string ipAddress in IPAddresses)
            {
                // When the session is completed, write the latest results of the session, stored in the LiveData Dictionary, to the ReportData Dictionary
                LatencyMonitorFunction.WriteToReportData(ipAddress);
                UpdateUI();
            }
        }

        // Sets the user-defined IP Addresses to monitor for the current session
        private void SetSessionTargets()
        {
            if (!string.IsNullOrWhiteSpace(Target1))
            {
                IPAddresses.Add(Target1);
            }

            if (!string.IsNullOrWhiteSpace(Target2))
            {
                IPAddresses.Add(Target2);
            }

            if (!string.IsNullOrWhiteSpace(Target3))
            {
                IPAddresses.Add(Target3);
            }

            if (!string.IsNullOrWhiteSpace(Target4))
            {
                IPAddresses.Add(Target4);
            }

            if (!string.IsNullOrWhiteSpace(Target5))
            {
                IPAddresses.Add(Target5);
            }
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

        // Clears out the test results from the UI and from the Latency Monitor Dictionary
        private void ClearPreviousSessionResults()
        {
            DataKey1 = null;
            DataKey2 = null;
            DataKey3 = null;
            DataKey4 = null;
            DataKey5 = null;
            ClearDataStorage();
        }

        // Used to determine when the Generate Report Button should be enabled
        private bool GetReportDataStatus()
        {
            if (PacketsSentInThisSession > 0 && !IsRunning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Used to determine when the Start Button should be enabled
        private bool GetMonitoringSessionStatusForStartBtn()
        {
            if (IsRunning)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // Used to determine when the Stop Button should be enabled
        private bool GetMonitoringSessionStatusForStopBtn()
        {
            if (IsRunning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
