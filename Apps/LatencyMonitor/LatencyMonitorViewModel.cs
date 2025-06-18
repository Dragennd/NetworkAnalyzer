using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        //public List<string> TargetList { get; set; }

        // Stores all data generated from the TargetList, including targets obtained through the initial Traceroute
        // - Updates each loop and is used to update the Traceroute DataGrid
        //public ConcurrentBag<LatencyMonitorData> AllTargets { get; set; }

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
        public LatencyMonitorPreset selectedPreset;

        [ObservableProperty]
        public int packetsSent;

        public LatencyMonitorData SelectedTarget
        {
            get => _latencyMonitorService.SelectedTarget;
            set
            {
                if (_latencyMonitorService.SelectedTarget != value)
                {
                    _latencyMonitorService.SelectedTarget = value;
                    OnPropertyChanged();
                    OnSelectedTargetChanged(value);
                }
            }
        }

        private LogHandler LogHandler { get; set; }

        private DatabaseHandler DB { get; set; }

        private readonly ILatencyMonitorService _latencyMonitorService;

        private readonly ILatencyMonitorController _latencyMonitorController;
        #endregion Control Properties

        public LatencyMonitorViewModel(ILatencyMonitorService latencyMonitorService, ILatencyMonitorController latencyMonitorController)
        {
            _latencyMonitorService = latencyMonitorService;
            _latencyMonitorController = latencyMonitorController;
            LiveTargets = new();
            Traceroute = new();
            History = new();
            //TargetList = new();
            //AllTargets = new();
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
                ResetPreSession();
                SetSessionStopwatchAsync();
                _latencyMonitorService.TargetList = SelectedPreset.TargetCollection.ToList();

                SetSubscriptions();

                await _latencyMonitorService.SetMonitoringSession(true);
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
            _latencyMonitorService.IsSessionActive = false;

            UnsetSubscriptions();

            await Task.Delay(4000); // Wait to ensure the current session ends completely
            ResetPostSession();
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
        private void SetSubscriptions()
        {
            _latencyMonitorController.SetLiveTargetData += SetLiveTargets;
            _latencyMonitorController.SetTracerouteData += SetTraceroute;
            _latencyMonitorController.SetSelectedTargetData += SetSelectedLiveTarget;
            _latencyMonitorController.UpdateLiveTargetData += UpdateLiveTargets;
            _latencyMonitorController.UpdateTracerouteData += UpdateTraceroute;
            _latencyMonitorController.UpdatePacketsSent += UpdatePacketsSent;
        }

        private void UnsetSubscriptions()
        {
            _latencyMonitorController.SetLiveTargetData -= SetLiveTargets;
            _latencyMonitorController.SetTracerouteData -= SetTraceroute;
            _latencyMonitorController.SetSelectedTargetData -= SetSelectedLiveTarget;
            _latencyMonitorController.UpdateLiveTargetData -= UpdateLiveTargets;
            _latencyMonitorController.UpdateTracerouteData -= UpdateTraceroute;
            _latencyMonitorController.UpdatePacketsSent -= UpdatePacketsSent;
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

            _latencyMonitorService.AllTargets.Add(data);
        }

        private void ChangeTraceroute(LatencyMonitorData data)
        {
            if (Traceroute.Count > 0 && data != null)
            {
                Traceroute.Clear();

                foreach (var t in _latencyMonitorService.AllTargets.Where(a => a.TracerouteGUID == data.TracerouteGUID).OrderBy(a => a.Hop))
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

        private void OnSelectedTargetChanged(LatencyMonitorData value) =>
            ChangeTraceroute(value);

        private void UpdatePacketsSent(int newPacket)
        {
            PacketsSent = newPacket;
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

        private void ResetPostSession()
        {
            _latencyMonitorService.TargetList.Clear();
        }

        private void ResetPreSession()
        {
            LiveTargets.Clear();
            Traceroute.Clear();
            History.Clear();
            _latencyMonitorService.AllTargets.Clear();

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
