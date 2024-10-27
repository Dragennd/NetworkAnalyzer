using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Collections.ObjectModel;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Controls;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal partial class LatencyMonitorViewModel : ObservableValidator
    {
        #region Control Properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StopMonitoringSessionCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartMonitoringSessionCommand))]
        [NotifyCanExecuteChangedFor(nameof(SwitchDisplayModesCommand))]
        [NotifyCanExecuteChangedFor(nameof(LoadSelectedProfileCommand))]
        private bool isRunning = false;

        [ObservableProperty]
        public bool performingInitialTraceroute = false;

        [ObservableProperty]
        public bool tracerouteFailedToComplete = false;

        [ObservableProperty]
        public bool tracerouteMode = false;

        [ObservableProperty]
        public bool userTargetsMode = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateReportCommand))]
        [NotifyCanExecuteChangedFor(nameof(SwitchDisplayModesCommand))]
        public bool readyToGenerateReport = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateReportCommand))]
        public int packetsSentInThisSession = 0;

        [ObservableProperty]
        public string sessionStatus = "IDLE";

        [ObservableProperty]
        public string? dnsHostEntryResolution;

        [ObservableProperty]
        public string sessionDuration = "00.00:00:00";

        [ObservableProperty]
        public ReportType sessionMode = ReportType.UserTargets;

        // Target for Traceroute scan
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ConditionalRequired(
            nameof(TracerouteMode),
            ErrorMessage = "The field cannot be empty.\nPlease enter a valid IP Address or DNS Name.")]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? targetAddress;

        // TTL for Traceroute Scan
        [ObservableProperty]
        [ConditionalRequired(
            nameof(TracerouteMode),
            ErrorMessage = "The field cannot be empty.\nPlease enter a number from 1 to 255.")]
        [RegularExpression(
            @"^(?:[1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$",
            ErrorMessage = "The number entered is not valid.\nPlease enter a number from 1 to 255.")]
        public int timeToLive = 30;

        // Targets 1 - 5 for User Targets scan
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ConditionalRequired(
            nameof(UserTargetsMode),
            ErrorMessage = "The field cannot be empty.\nPlease enter a valid IP Address or DNS Name.")]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target1;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target2;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target3;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target4;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])|(?:[a-zA-Z0-9-]{1,63}\.)?[a-zA-Z0-9-]{1,63}(?:\.[a-zA-Z0-9]{1,63})$",
            ErrorMessage = "Please enter a valid IP Address or DNS Name.")]
        [PingTarget]
        public string? target5;

        [ObservableProperty]
        public Brush sessionStatusDisplayColor = Brushes.Red;

        [ObservableProperty]
        public LatencyMonitorData? dataKey1;

        [ObservableProperty]
        public LatencyMonitorData? dataKey2;

        [ObservableProperty]
        public LatencyMonitorData? dataKey3;

        [ObservableProperty]
        public LatencyMonitorData? dataKey4;

        [ObservableProperty]
        public LatencyMonitorData? dataKey5;

        [ObservableProperty]
        public ProfileSelector profileInstance;

        [ObservableProperty]
        public LatencyMonitorTargetProfiles selectedTargetProfile = null;

        public ObservableCollection<LatencyMonitorData> TracerouteModeData { get; set; }

        public ObservableCollection<LatencyMonitorTargetProfiles> TargetProfiles { get; set; }

        private static ProfileSelector _profileSelector = new();
        #endregion

        public LatencyMonitorViewModel()
        {
            TracerouteModeData = new();
            TargetProfiles = new();
        }

        // Command to execute when the Start button is clicked
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForStartBtn))]
        public async Task StartMonitoringSessionAsync()
        {
            ClearPreviousSessionResults();

            if (SelectedTargetProfile == null && await ValidateUserInputAsync() == false)
            {
                return;
            }
            else if ((SelectedTargetProfile.Target1 != Target1 ||
                      SelectedTargetProfile.Target2 != Target2 ||
                      SelectedTargetProfile.Target3 != Target3 ||
                      SelectedTargetProfile.Target4 != Target4 ||
                      SelectedTargetProfile.Target5 != Target5) && await ValidateUserInputAsync() == false)
            {
                return;
            }

            ReadyToGenerateReport = false;

            SetSessionTargets();
            SetSessionStatus();

            StartTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            LastLoggedType = SessionMode;

            var dbHandler = new DatabaseHandler();
            await dbHandler.NewLatencyMonitorReportAsync();

            if (TracerouteMode)
            {
                var tracerouteHandler = new TracerouteHandler(TargetAddress, true, TimeToLive);

                PerformingInitialTraceroute = true;
                SetSessionStopwatchAsync();
                TracerouteStatus status = await tracerouteHandler.PerformInitialTraceroute();
                PerformingInitialTraceroute = false;

                if (status == TracerouteStatus.Completed)
                {
                    TracerouteFailedToComplete = false;
                    UpdateTracerouteDataForDataGrid();
                    await ManageMonitoringSessionAsync();
                }
                else
                {
                    TracerouteFailedToComplete = true;
                    ReadyToGenerateReport = true;
                    SetSessionStatus();
                }
            }
            else
            {
                SetSessionStopwatchAsync();
                await ManageMonitoringSessionAsync();
            }

            EndTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }

        // Command to execute when the Switch Modes button is clicked
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForSwitchModeBtn))]
        public async Task SwitchDisplayModes()
        {
            TracerouteMode = !TracerouteMode;
            UserTargetsMode = !UserTargetsMode;

            SetSessionMode();
            await GetTargetProfilesAsync();
        }

        // Command to execute when the Stop button is clicked
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForStopBtn))]
        public void StopMonitoringSession()
        {
            SetSessionStatus();
            SelectedTargetProfile = null;
        }

        // Command to execute when the Generate Report button is clicked
        [RelayCommand(CanExecute = nameof(GetReportDataStatus))]
        public void GenerateReport()
        {
            MenuController.SendActiveAppRequest("Reports");
        }

        // Command to display the Profile Selection window
        [RelayCommand]
        public async Task ShowProfileSelectorAsync()
        {
            if (ProfileInstance == null)
            {
                ProfileInstance = _profileSelector;
            }
            else
            {
                ProfileInstance = null;
                await GetTargetProfilesAsync();
            }
        }

        // Command to load the selected profile from the combobox
        [RelayCommand(CanExecute = nameof(GetMonitoringSessionStatusForStartBtn))]
        public void LoadSelectedProfile()
        {
            if (SelectedTargetProfile != null)
            {
                if (SelectedTargetProfile.ReportType == ReportType.UserTargets)
                {
                    Target1 = SelectedTargetProfile.Target1;
                    Target2 = SelectedTargetProfile.Target2;
                    Target3 = SelectedTargetProfile.Target3;
                    Target4 = SelectedTargetProfile.Target4;
                    Target5 = SelectedTargetProfile.Target5;
                }
                else
                {
                    TargetAddress = SelectedTargetProfile.Target1;
                    TimeToLive = SelectedTargetProfile.Hops;
                }
            }
        }

        public async Task GetTargetProfilesAsync()
        {
            var dbHandler = new DatabaseHandler();

            TargetProfiles.Clear();

            if (UserTargetsMode)
            {
                var profiles = (await dbHandler.GetLatencyMonitorTargetProfilesAsync()).Where(a => a.ReportType == ReportType.UserTargets);
                foreach (var profile in profiles)
                {
                    TargetProfiles.Add(profile);
                }
            }
            else
            {
                var profiles = (await dbHandler.GetLatencyMonitorTargetProfilesAsync()).Where(a => a.ReportType == ReportType.Traceroute);
                foreach (var profile in profiles)
                {
                    TargetProfiles.Add(profile);
                }
            }
        }

        #region Private Methods
        // Starts a session of the Latency Monitor
        private async Task ManageMonitoringSessionAsync()
        {
            var dbHandler = new DatabaseHandler();
            ReadyToGenerateReport = false;

            do
            {
                var task = new List<Task>();
                Stopwatch iteration = Stopwatch.StartNew();
                PacketsSent++;

                foreach (string ipAddress in IPAddresses)
                {
                    var response = await new Ping().SendPingAsync(ipAddress, 1000);

                    if (!TracerouteMode)
                    {
                        // Set to true if the LiveData dictionary has not been initialized
                        var isLiveSessionDataEmpty = false;

                        if (!LiveSessionData.ContainsKey(ipAddress))
                        {
                            isLiveSessionDataEmpty = true;
                        }

                        var userTargetsHandler = new UserTargetsHandler(ipAddress, response.Status, response.RoundtripTime, isLiveSessionDataEmpty);

                        if (isLiveSessionDataEmpty)
                        {
                            var targetData = await userTargetsHandler.NewUserTargetsDataAsync();

                            task.Add(CreateSessionAsync(ipAddress, targetData));
                            task.Add(dbHandler.NewLatencyMonitorReportSnapshotAsync(targetData));
                            task.Add(dbHandler.NewLatencyMonitorReportEntryAsync(targetData));
                        }
                        else
                        {
                            var targetData = await userTargetsHandler.NewUserTargetsDataAsync();

                            task.Add(AddSessionDataAsync(ipAddress, true, false, targetData));
                            task.Add(dbHandler.UpdateLatencyMonitorReportSnapshotAsync(targetData, SessionDuration));
                        }
                    }
                    else
                    {
                        var tracerouteHandler = new TracerouteHandler(ipAddress, false, TimeToLive, response.Status, response.RoundtripTime);

                        var targetData = await tracerouteHandler.NewTracerouteSuccessDataAsync();

                        task.Add(AddSessionDataAsync(ipAddress, true, false, targetData));
                        task.Add(dbHandler.UpdateLatencyMonitorReportSnapshotAsync(targetData, SessionDuration));
                    }
                }

                await Task.WhenAll(task);
                task.Clear();

                foreach (string ipAddress in IPAddresses)
                {
                    task.Add(ProcessLastMajorChange(ipAddress));
                }

                await Task.WhenAll(task);

                UpdateUI();
                iteration.Stop();

                if (iteration.ElapsedMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)iteration.ElapsedMilliseconds);
                }
            } while (IsRunning);

            var finalTask = new List<Task>();
            PacketsSent++;

            // Write the final entry to Live and to Report
            foreach (string ipAddress in IPAddresses)
            {
                PingReply response = await new Ping().SendPingAsync(ipAddress, 1000);
                
                if (!TracerouteMode)
                {
                    var userTargetsHandler = new UserTargetsHandler(ipAddress, response.Status, response.RoundtripTime, false);
                    var targetData = await userTargetsHandler.NewUserTargetsDataAsync();

                    finalTask.Add(AddSessionDataAsync(ipAddress, true, true, targetData));
                    finalTask.Add(dbHandler.UpdateLatencyMonitorReportAsync());
                    finalTask.Add(dbHandler.UpdateLatencyMonitorReportSnapshotAsync(targetData, SessionDuration));
                }
                else
                {
                    var tracerouteHandler = new TracerouteHandler(ipAddress, false, TimeToLive, response.Status, response.RoundtripTime);
                    var targetData = await tracerouteHandler.NewTracerouteSuccessDataAsync();

                    finalTask.Add(AddSessionDataAsync(ipAddress, true, true, targetData));
                    finalTask.Add(dbHandler.UpdateLatencyMonitorReportAsync());
                    finalTask.Add(dbHandler.UpdateLatencyMonitorReportSnapshotAsync(targetData, SessionDuration));
                }

                UpdateUI();
            }

            await Task.WhenAll(finalTask);
            TotalDuration = SessionDuration;
            ReadyToGenerateReport = true;
            SetSessionStatus();
        }

        private void SetSessionMode()
        {
            if (TracerouteMode)
            {
                SessionMode = ReportType.Traceroute;
            }
            else
            {
                SessionMode = ReportType.UserTargets;
            }
        }

        private void UpdateTracerouteDataForDataGrid()
        {
            List<LatencyMonitorData> tempData = new();

            foreach (var keypair in LiveSessionData)
            {
                tempData.Add(keypair.Value.Last());
            }

            tempData = tempData.OrderBy(a => a.Hop).ToList();

            TracerouteModeData.Clear();
            
            foreach (var data in tempData)
            {
                TracerouteModeData.Add(data);
            }
        }

        // Update the UI with the latest info in the LiveData Dictionary
        private void UpdateUI()
        {
            if (UserTargetsMode)
            {
                if (!string.IsNullOrWhiteSpace(Target1))
                {
                    DataKey1 = LiveSessionData[Target1].Last();
                }

                if (!string.IsNullOrWhiteSpace(Target2))
                {
                    DataKey2 = LiveSessionData[Target2].Last();
                }

                if (!string.IsNullOrWhiteSpace(Target3))
                {
                    DataKey3 = LiveSessionData[Target3].Last();
                }

                if (!string.IsNullOrWhiteSpace(Target4))
                {
                    DataKey4 = LiveSessionData[Target4].Last();
                }

                if (!string.IsNullOrWhiteSpace(Target5))
                {
                    DataKey5 = LiveSessionData[Target5].Last();
                }
            }
            else
            {
                UpdateTracerouteDataForDataGrid();
            }

            PacketsSentInThisSession = PacketsSent;
        }

        // Set the visual for the Status field depending on whether the session is active or not
        private void SetSessionStatus()
        {
            IsRunning = !IsRunning;

            if (IsRunning && LiveSessionData.IsEmpty)
            {
                SessionStatus = "RUNNING";
                SessionStatusDisplayColor = Brushes.Green;
            }
            else if (!IsRunning && !ReadyToGenerateReport)
            {
                SessionStatus = "FINALIZING";
                SessionStatusDisplayColor = Brushes.Yellow;
            }
            else if (IsRunning && ReadyToGenerateReport)
            {
                SessionStatus = "IDLE";
                SessionStatusDisplayColor = Brushes.Red;
                IsRunning = !IsRunning;
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

        private async void SetSessionStopwatchAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (!ReadyToGenerateReport)
            {
                SessionDuration = FormatElapsedTime(sw.Elapsed);
                await Task.Delay(1000);
            }
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
            SessionDuration = "00.00:00:00";
            PacketsSentInThisSession = 0;
            TracerouteFailedToComplete = false;

            TracerouteModeData.Clear();
            DataKey1 = null;
            DataKey2 = null;
            DataKey3 = null;
            DataKey4 = null;
            DataKey5 = null;
            ClearDataStorage();

            if (TracerouteMode)
            {
                Target1 = string.Empty;
                Target2 = string.Empty;
                Target3 = string.Empty;
                Target4 = string.Empty;
                Target5 = string.Empty;   
            }
            else
            {
                TargetAddress = string.Empty;
            }
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

        // Used to determine when the Start Button should be enabled
        private bool GetMonitoringSessionStatusForSwitchModeBtn()
        {
            bool canClick = false;

            if (!ReadyToGenerateReport && IsRunning)
            {
                canClick = false;
            }
            else if (!ReadyToGenerateReport && !IsRunning && SessionDuration == "00.00:00:00")
            {
                canClick = true;
            }
            else if (ReadyToGenerateReport)
            {
                canClick = true;
            }

            return canClick;
        }
        #endregion
    }
}
