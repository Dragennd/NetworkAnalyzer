using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;
using System.Collections.ObjectModel;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal partial class LatencyMonitorViewModel : ObservableValidator
    {
        #region Control Properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopMonitoringSessionCommand))]
        [NotifyCanExecuteChangedFor(nameof(LatencyMonitorManagerCommand))]
        private bool isRunning = false;

        [ObservableProperty]
        public bool traceRouteMode = true;

        [ObservableProperty]
        public bool userTargetsMode = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateReportCommand))]
        public bool readyToGenerateReport = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateReportCommand))]
        public int packetsSentInThisSession = 0;

        [ObservableProperty]
        public int timeToLive = 30;

        [ObservableProperty]
        public string sessionStatus = "IDLE";

        [ObservableProperty]
        public string? dnsHostEntryResolution;

        [ObservableProperty]
        public string sessionDuration = "00.00:00:00";

        [ObservableProperty]
        public string sessionMode = "Traceroute Mode";

        // Target for TraceRoute scan
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string targetAddress;

        // Targets 1 - 5 for User Targets scan
        [ObservableProperty]
        [NotifyDataErrorInfo]
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
        public Brush sessionStatusDisplayColor = Brushes.Red;

        [ObservableProperty]
        public LatencyMonitorData dataKey1;

        [ObservableProperty]
        public LatencyMonitorData? dataKey2;

        [ObservableProperty]
        public LatencyMonitorData? dataKey3;

        [ObservableProperty]
        public LatencyMonitorData? dataKey4;

        [ObservableProperty]
        public LatencyMonitorData? dataKey5;

        public ObservableCollection<LatencyMonitorData> TraceRouteModeData { get; set; }
        #endregion

        public LatencyMonitorViewModel()
        {
            TraceRouteModeData = new();
        }

        // Command to execute when the Start button is clicked
        [RelayCommand]
        public async Task LatencyMonitorManagerAsync()
        {
            TraceRouteHandler traceRouteHandler = new();

            if (await ValidateUserInputAsync() == false)
            {
                return;
            }

            ClearPreviousSessionResults();
            SetSessionTargets();
            SetSessionStatus();

            if (TraceRouteMode)
            {
                await traceRouteHandler.ProcessHopsAsync(targetAddress, TimeToLive);
                ProcessTraceRouteDataForDataGrid();
            }

            await StartMonitoringSessionAsync();
        }

        // Command to execute when the Switch Modes button is clicked
        [RelayCommand]
        public void SwitchDisplayModes()
        {
            TraceRouteMode = !TraceRouteMode;
            UserTargetsMode = !UserTargetsMode;

            SetSessionMode();
        }

        // Command to execute when the Stop button is clicked
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForStopBtn))]
        public void StopMonitoringSession() => SetSessionStatus();

        // Command to execute when the Generate Report button is clicked
        [RelayCommand(CanExecute = nameof(GetReportDataStatus))]
        public async Task GenerateReportAsync()
        {
            HTMLReportHandler handler = new();
            var reportNumber = await GenerateReportNumber();

            await handler.GenerateHTMLReport(reportNumber);
            MessageBox.Show($"Report has been created in {DataDirectory}\nFile Name: {reportNumber}.html",
                            "Report Created",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.OK);

            try
            {
                Process.Start("explorer.exe", DataDirectory);
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
        // Starts a session of the Latency Monitor
        private async Task StartMonitoringSessionAsync()
        {
            LatencyMonitorManager manager = new();
            LatencyHandler latencyHandler = new();
            StatusHandler statusHandler = new();
            PacketLossHandler packetLossHandler = new();
            TimeStampHandler timeStampHandler = new();
            TraceRouteHandler traceRouteHandler = new();

            SetSessionStopwatch();
            ReadyToGenerateReport = false;

            do
            {
                List<Task> task = new();
                PacketsSent++;

                foreach (string ipAddress in IPAddresses)
                {
                    PingReply response = await new Ping().SendPingAsync(ipAddress, 1000);

                    if (!TraceRouteMode)
                    {
                        // Use this segment if it is the first run and the dictionary hasn't been initiated yet
                        if (!LiveSessionData.ContainsKey(ipAddress))
                        {
                            var initialStatus = await statusHandler.CalculateCurrentStatusAsync(response.Status, ipAddress, true);

                            task.Add(manager.CreateSessionAsync(ipAddress,
                                             await manager.NewSessionDataAsync(ipAddress, (int)response.RoundtripTime, initialStatus,
                                                           await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, true),
                                                           await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, true),
                                                           await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, true),
                                                           await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, ipAddress, true),
                                                           await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, ipAddress, true),
                                                           await packetLossHandler.CalculateFailedPingAsync(response.Status, ipAddress, true),
                                                           await timeStampHandler.CalculateTimeStampAsync(ipAddress, initialStatus, LatencyMonitorSessionType.Live, true))));
                            continue;
                        }

                        var standardStatus = await statusHandler.CalculateCurrentStatusAsync(response.Status, ipAddress, false);
                        await manager.RemoveSessionDataAsync(ipAddress);

                        task.Add(manager.AddSessionDataAsync(ipAddress, true, false,
                                             await manager.NewSessionDataAsync(ipAddress, (int)response.RoundtripTime, standardStatus,
                                                           await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, ipAddress, false),
                                                           await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, ipAddress, false),
                                                           await packetLossHandler.CalculateFailedPingAsync(response.Status, ipAddress, false),
                                                           await timeStampHandler.CalculateTimeStampAsync(ipAddress, standardStatus, LatencyMonitorSessionType.Live, false))));
                    }
                    else
                    {
                        var standardStatus = await statusHandler.CalculateCurrentStatusAsync(response.Status, ipAddress, false);
                        await manager.RemoveSessionDataAsync(ipAddress);

                        task.Add(manager.AddSessionDataAsync(ipAddress, true, false,
                                             await manager.NewSessionDataAsync(ipAddress, (int)response.RoundtripTime, standardStatus,
                                                           await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                           await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, ipAddress, false),
                                                           await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, ipAddress, false),
                                                           await packetLossHandler.CalculateFailedPingAsync(response.Status, ipAddress, false),
                                                           await timeStampHandler.CalculateTimeStampAsync(ipAddress, standardStatus, LatencyMonitorSessionType.Live, false),
                                                           await traceRouteHandler.MaintainAssignedHop(ipAddress))));
                    }
                }
                    

                await Task.WhenAll(task);

                UpdateUI();

                await Task.Delay(1000);
            } while (IsRunning);

            List<Task> finalTask = new();
            PacketsSent++;

            // Write the final entry to Live and to Report
            foreach (string ipAddress in IPAddresses)
            {
                PingReply response = await new Ping().SendPingAsync(ipAddress, 1000);
                var finalStatus = await statusHandler.CalculateCurrentStatusAsync(response.Status, ipAddress, false);

                finalTask.Add(manager.AddSessionDataAsync(ipAddress, true, true, await manager.NewSessionDataAsync(ipAddress, (int)response.RoundtripTime, finalStatus,
                                                       await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                       await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                       await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, ipAddress, false),
                                                       await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, ipAddress, false),
                                                       await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, ipAddress, false),
                                                       await packetLossHandler.CalculateFailedPingAsync(response.Status, ipAddress, false),
                                                       await timeStampHandler.CalculateTimeStampAsync(ipAddress, finalStatus, LatencyMonitorSessionType.Report, false))));

                UpdateUI();
            }

            await Task.WhenAll(finalTask);
            TotalDuration = SessionDuration;
            ReadyToGenerateReport = true;
        }

        private void SetSessionMode()
        {
            if (TraceRouteMode)
            {
                SessionMode = "Traceroute Mode";
            }
            else
            {
                SessionMode = "User Targets Mode";
            }
        }

        // Generate a report number for the HTML Report following the "LM{0:MMddyyyy.HHmm}" format (e.g. LM08272024.1345)
        private async Task<string> GenerateReportNumber() => await Task.FromResult(string.Format("LM{0:MMddyyyy.HHmm}", DateTime.Now));

        private void ProcessTraceRouteDataForDataGrid()
        {
            List<LatencyMonitorData> tempData = new();

            foreach (var keypair in LiveSessionData)
            {
                tempData.Add(keypair.Value.LastOrDefault());
            }

            tempData = tempData.OrderBy(a => a.Hop).ToList();

            TraceRouteModeData.Clear();
            
            foreach (var data in tempData)
            {
                TraceRouteModeData.Add(data);
            }
        }

        // Update the UI with the latest info in the LiveData Dictionary
        private void UpdateUI()
        {
            if (UserTargetsMode)
            {
                if (!string.IsNullOrWhiteSpace(Target1))
                {
                    DataKey1 = LiveSessionData[Target1].LastOrDefault();
                }

                if (!string.IsNullOrWhiteSpace(Target2))
                {
                    DataKey2 = LiveSessionData[Target2].LastOrDefault();
                }

                if (!string.IsNullOrWhiteSpace(Target3))
                {
                    DataKey3 = LiveSessionData[Target3].LastOrDefault();
                }

                if (!string.IsNullOrWhiteSpace(Target4))
                {
                    DataKey4 = LiveSessionData[Target4].LastOrDefault();
                }

                if (!string.IsNullOrWhiteSpace(Target5))
                {
                    DataKey5 = LiveSessionData[Target5].LastOrDefault();
                }
            }
            else
            {
                ProcessTraceRouteDataForDataGrid();
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

        private void SetSessionStopwatch()
        {
            Stopwatch sw = Stopwatch.StartNew();

            Task.Run(async () =>
            {
                while (!ReadyToGenerateReport)
                {
                    SessionDuration = FormatElapsedTime(sw.Elapsed);
                    await Task.Delay(1000);
                }
            });
        }

        private string FormatElapsedTime(TimeSpan elapsedTime)
        {
            return $"{elapsedTime.Days:00}.{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
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
            TraceRouteModeData.Clear();
            ClearDataStorage();
        }

        // Used to determine when the Generate Report Button should be enabled
        private bool GetReportDataStatus()
        {
            if (PacketsSentInThisSession > 0 && !IsRunning && ReadyToGenerateReport)
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
