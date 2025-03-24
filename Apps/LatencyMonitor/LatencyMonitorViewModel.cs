using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Controls;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using System.Collections.Concurrent;

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
        public LatencyMonitorData selectedTarget;

        public LogHandler LogHandler { get; set; }
        #endregion Control Properties

        public LatencyMonitorViewModel()
        {
            LiveTargets = new();
            Traceroute = new();
            History = new();
            TargetList = new();
            AllTargets = new();
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
        public async Task ManageProfilesButtonAsync()
        {

        }

        [RelayCommand]
        public async Task FilterButtonAsync()
        {

        }

        [RelayCommand]
        public async Task RefreshButtonAsync()
        {

        }

        #region Private Methods
        private async Task StartMonitoringSessionAsync()
        {
            var db = new DatabaseHandler();

            foreach (var a in TargetList)
            {
                foreach (var b in await ExecuteInitialSessionAsync(a))
                {
                    AllTargets.Enqueue(b);
                }
            }

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
                        if (t != null)
                        {
                            AllTargets.Enqueue(await ExecuteSessionUpdateAsync(t));
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

                    // Add database update method to add the current item
                }

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
