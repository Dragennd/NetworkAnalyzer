using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using System.Collections.Concurrent;
using NetworkAnalyzer.Utilities;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal partial class LatencyMonitorViewModel : ObservableValidator
    {
        #region Control Properties
        // Corresponds to the LiveTargets DataGrid
        // - Updates each loop, regardless of an active selection
        public ObservableCollection<LatencyMonitorData> LiveTargets { get; set; }

        // Corresponds to the Traceroute DataGrid
        // - Updates each loop, based on the selection in the LiveTargets DataGrid
        public ObservableCollection<LatencyMonitorData> Traceroute { get; set; }

        // Corresponds to the History DataGrid
        // - Updates once, based on the selection in the LiveTargets DataGrid
        // - Pulls data from the SQLite Database and not from memory
        public ObservableCollection<LatencyMonitorData> History { get; set; }

        // Stores the list of targets provided by the user in the presets
        public List<string> TargetList { get; set; }

        // Stores all data generated from the TargetList, including targets obtained through the initial Traceroute
        // - Updates each loop and is used to update the Traceroute DataGrid
        public ConcurrentQueue<LatencyMonitorData> AllTargets { get; set; }

        [ObservableProperty]
        public string reportNumber = "N/A";

        [ObservableProperty]
        public string sessionDuration = "N/A";

        [ObservableProperty]
        public string startTime = "N/A";

        [ObservableProperty]
        public int packetsSent = 0;

        [ObservableProperty]
        public bool isSessionActive = false;

        [ObservableProperty]
        public bool isPresetWindowVisible = false;

        [ObservableProperty]
        public LatencyMonitorData selectedTarget;

        [ObservableProperty]
        public ManagePresets? presetManagerInstance;

        [ObservableProperty]
        public Filter? filterInstance;

        private LogHandler LogHandler { get; set; }
        #endregion Control Properties

        public LatencyMonitorViewModel()
        {
            LiveTargets = new();
            Traceroute = new();
            History = new();
            TargetList = new();
            LogHandler = new();
        }

        [RelayCommand]
        public async Task StartButtonAsync()
        {
            try
            {
                IsSessionActive = true;
                await StartMonitoringSessionAsync();
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        [RelayCommand]
        public async Task StopButtonAsync()
        {
            IsSessionActive = false;
            await Task.Delay(4000); // Wait to ensure the current session ends completely
            ResetMonitoringSession();
        }

        [RelayCommand]
        public void ManageProfilesButton()
        {
            IsPresetWindowVisible = !IsPresetWindowVisible;
        }

        [RelayCommand]
        public void FilterButton()
        {
            
        }

        [RelayCommand]
        public async Task RefreshButtonAsync()
        {

        }

        [RelayCommand]
        public async Task SavePresetButtonAsync()
        {

        }

        [RelayCommand]
        public async Task DeletePresetButtonAsync()
        {

        }

        [RelayCommand]
        public async Task AddItemButtonAsync()
        {

        }

        [RelayCommand]
        public async Task RemoveItemButtonAsync()
        {

        }

        [RelayCommand]
        public void ClosePresetWindowButton()
        {
            IsPresetWindowVisible = false;
        }

        #region Private Methods
        private async Task StartMonitoringSessionAsync()
        {
            var db = new DatabaseHandler();

            AllTargets = new(await ExecuteInitialSessionAsync(TargetList));

            SetSessionStopwatchAsync();

            while (IsSessionActive)
            {
                var task = new List<Task>();
                Stopwatch sw = Stopwatch.StartNew();
                int allTargetsCount = AllTargets.Count;
                PacketsSent++;

                for (int i = 0; i < allTargetsCount; i++)
                {
                    Func<Task> item = async () =>
                    {
                        AllTargets.TryDequeue(out var t);
                        if (t != null && t.TargetStatus == LatencyMonitorTargetStatus.Active)
                        {
                            AllTargets.Enqueue(await ExecuteSessionUpdateAsync(t));
                        }
                        else if (t != null)
                        {
                            AllTargets.Enqueue(t);
                        }
                    };

                    task.Add(item());
                }

                await Task.WhenAll(task);

                foreach (var item in AllTargets)
                {
                    if (TargetList.Any(a => a == item.TargetName))
                    {
                        UpdateLiveTargets(item);
                    }

                    if (SelectedTarget.TargetName == item.UserDefinedTarget)
                    {
                        UpdateTraceroute(item);
                    }

                    // To-Do: Add the current item to a list to hold until all items have been processed for this round
                }

                // To-Do: Add database update method to add the batch of items

                if (sw.ElapsedMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)sw.ElapsedMilliseconds);
                }
            }
        }

        private void UpdateLiveTargets(LatencyMonitorData data)
        {
            LatencyMonitorData obj = LiveTargets.First(a => a.TargetName == data.TargetName);
            
            obj.Latency = data.Latency;
            obj.LowestLatency = data.LowestLatency;
            obj.HighestLatency = data.HighestLatency;
            obj.AverageLatency = data.AverageLatency;
            obj.TotalPacketsLost = data.TotalPacketsLost;
        }

        private void UpdateTraceroute(LatencyMonitorData data)
        {
            LatencyMonitorData obj = Traceroute.First(a => a.TargetName == data.TargetName);

            obj.Latency = data.Latency;
            obj.TotalPacketsLost = data.TotalPacketsLost;
        }

        private void UpdateHistory()
        {

        }

        private async void SetSessionStopwatchAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (IsSessionActive)
            {
                SessionDuration = FormatElapsedTime(sw.Elapsed);
                await Task.Delay(1000);
            }
        }

        private string FormatElapsedTime(TimeSpan elapsedTime)
        {
            return $"{elapsedTime.Days:00}.{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";
        }

        private void ResetMonitoringSession()
        {
            LiveTargets.Clear();
            Traceroute.Clear();
            History.Clear();
            TargetList.Clear();
            AllTargets.Clear();

            ReportNumber = "N/A";
            SessionDuration = "N/A";
            StartTime = "N/A";
            PacketsSent = 0;
            SelectedTarget = null;
        }
        #endregion Private Methods
    }
}
