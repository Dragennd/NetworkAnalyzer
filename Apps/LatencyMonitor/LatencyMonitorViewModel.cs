using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using System.Collections.Concurrent;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.IPScanner.Functions;

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

        // Contains a list of available target profiles from the database
        public ObservableCollection<LatencyMonitorPreset> TargetPresets { get; set; }

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
        public string targetToAddToPreset = string.Empty;

        [ObservableProperty]
        public string presetName = string.Empty;

        [ObservableProperty]
        public int packetsSent = 0;

        [ObservableProperty]
        public bool isSessionActive = false;

        [ObservableProperty]
        public bool isPresetWindowVisible = false;

        [ObservableProperty]
        public bool isFilterWindowVisible = false;

        [ObservableProperty]
        public bool isNonDefaultPresetSelected = false;

        [ObservableProperty]
        public bool isInitializing = false;

        [ObservableProperty]
        public LatencyMonitorData selectedTarget;

        [ObservableProperty]
        public LatencyMonitorPreset selectedPreset;

        private LogHandler LogHandler { get; set; }

        private DatabaseHandler DB { get; set; }
        #endregion Control Properties

        public LatencyMonitorViewModel()
        {
            LiveTargets = new();
            Traceroute = new();
            History = new();
            TargetList = new();
            AllTargets = new();
            LogHandler = new();
            DB = new();
            TargetPresets = new()
            {
                new LatencyMonitorPreset("Default")
            };
        }

        [RelayCommand]
        public async Task StartButtonAsync()
        {
            try
            {
                TargetList = SelectedPreset.TargetCollection.ToList();
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

            if (SelectedPreset == null)
            {
                SelectedPreset = new();
            }

            IsNonDefaultPresetSelected = false;
        }

        [RelayCommand]
        public void FilterButton()
        {
            
        }

        [RelayCommand]
        public async Task RefreshFiltersButtonAsync()
        {

        }

        [RelayCommand]
        public void NewPresetButton()
        {
            SelectedPreset = new LatencyMonitorPreset();
            TargetToAddToPreset = string.Empty;
            PresetName = string.Empty;            
        }

        [RelayCommand]
        public void SavePresetButton()
        {
            if (TargetPresets.Any(a => a.UUID == SelectedPreset.UUID))
            {
                var existingPreset = TargetPresets.First(a => a.UUID == SelectedPreset.UUID);
                SelectedPreset.PresetName = PresetName;
                existingPreset = SelectedPreset;
                SelectedPreset = TargetPresets[0];
            }
            else
            {
                SelectedPreset.PresetName = PresetName;
                TargetPresets.Add(SelectedPreset);
                SelectedPreset = TargetPresets[0];
            }
        }

        [RelayCommand]
        public void DeletePresetButton()
        {
            if (SelectedPreset != null)
            {
                TargetPresets.Remove(SelectedPreset);
            }
        }

        [RelayCommand]
        public void AddItemButton()
        {
            SelectedPreset.TargetCollection.Add(TargetToAddToPreset);
            TargetToAddToPreset = string.Empty;
        }

        [RelayCommand]
        public void RemoveItemButton(string item)
        {
            if (SelectedPreset.TargetCollection.Contains(item))
            {
                SelectedPreset.TargetCollection.Remove(item);
            }
        }

        [RelayCommand]
        public void ClosePresetWindowButton()
        {
            IsPresetWindowVisible = false;
        }

        #region Private Methods
        private async Task StartMonitoringSessionAsync()
        {
            SetSessionStopwatchAsync();

            LatencyMonitorController.LiveTargetsSet += SetLiveTargets;
            LatencyMonitorController.LiveTargetsSet += SetSelectedLiveTarget;
            LatencyMonitorController.TracerouteSet += SetTraceroute;
            await ExecuteInitialSessionAsync(TargetList);
            LatencyMonitorController.LiveTargetsSet -= SetLiveTargets;
            LatencyMonitorController.LiveTargetsSet -= SetSelectedLiveTarget;
            LatencyMonitorController.TracerouteSet -= SetTraceroute;

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
                    if (TargetList.Any(a => a == item.DisplayName))
                    {
                        UpdateLiveTargets(item);
                    }

                    if (SelectedTarget != null && SelectedTarget.TracerouteGUID == item.TracerouteGUID)
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

        private void SetLiveTargets(LatencyMonitorData data)
        {
            if (data.IsUserDefinedTarget == true)
            {
                LiveTargets.Add(data);
            }
        }

        private void UpdateLiveTargets(LatencyMonitorData data)
        {
            if (LiveTargets.Any(a => a.TargetGUID == data.TargetGUID))
            {
                LatencyMonitorData obj = LiveTargets.First(a => a.TargetGUID == data.TargetGUID);

                obj.Latency = data.Latency;
                obj.LowestLatency = data.LowestLatency;
                obj.HighestLatency = data.HighestLatency;
                obj.AverageLatency = data.AverageLatency;
                obj.TotalPacketsLost = data.TotalPacketsLost;
            }
        }

        private void SetTraceroute(LatencyMonitorData data)
        {
            if (SelectedTarget.TracerouteGUID == data.TracerouteGUID && !Traceroute.Any(a => a.TargetGUID == data.TargetGUID))
            {
                Traceroute.Add(data);
            }

            AllTargets.Enqueue(data);
        }

        private void ChangeTraceroute(LatencyMonitorData data)
        {
            if (Traceroute.Count > 0 && data != null)
            {
                Traceroute.Clear();

                foreach (var t in AllTargets.Where(a => a.TracerouteGUID == data.TracerouteGUID).OrderBy(a => a.Hop))
                {
                    Traceroute.Add(t);
                }
            }
        }

        private void UpdateTraceroute(LatencyMonitorData data)
        {
            if (Traceroute.Any(a => a.TargetGUID == data.TargetGUID))
            {
                LatencyMonitorData obj = Traceroute.First(a => a.TargetGUID == data.TargetGUID);

                obj.Latency = data.Latency;
                obj.TotalPacketsLost = data.TotalPacketsLost;
            }
        }

        private void SetSelectedLiveTarget(LatencyMonitorData data)
        {
            if (SelectedTarget == null)
            {
                SelectedTarget = data;
            }
        }

        partial void OnSelectedTargetChanged(LatencyMonitorData value) =>
            ChangeTraceroute(value);

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

        partial void OnSelectedPresetChanged(LatencyMonitorPreset value)
        {
            if (value != null)
            {
                PresetName = value.PresetName;
            }
            else
            {
                PresetName = string.Empty;
            }

            if (value != null && value.UUID == "Default")
            {
                IsNonDefaultPresetSelected = false;
            }
            else
            {
                IsNonDefaultPresetSelected = true;
            }

            OnPropertyChanged(nameof(TargetPresets));
        }
        #endregion Private Methods
    }
}
