using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

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

        public ConcurrentBag<LatencyMonitorData> AllTargets
        {
            get => _latencyMonitorService.AllTargets;
            set
            {
                if (_latencyMonitorService.AllTargets != value)
                {
                    _latencyMonitorService.AllTargets = value;
                }
            }
        }

        public List<string> TargetList
        {
            get => _latencyMonitorService.TargetList;
            set
            {
                if (_latencyMonitorService.TargetList != value)
                {
                    _latencyMonitorService.TargetList = value;
                }
            }
        }

        public string ReportNumber
        {
            get => _latencyMonitorService.ReportID;
            set
            {
                if (_latencyMonitorService.ReportID != value)
                {
                    _latencyMonitorService.ReportID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SessionDuration
        {
            get => _latencyMonitorService.SessionDuration;
            set
            {
                if (_latencyMonitorService.SessionDuration != value)
                {
                    _latencyMonitorService.SessionDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StartTime
        {
            get => _latencyMonitorService.StartTime;
            set
            {
                if (_latencyMonitorService.StartTime != value)
                {
                    _latencyMonitorService.StartTime = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public string targetToAddToPreset = string.Empty;

        [ObservableProperty]
        public string presetName = string.Empty;

        [ObservableProperty]
        public bool isPresetWindowVisible = false;

        [ObservableProperty]
        public bool isFilterWindowVisible = false;

        [ObservableProperty]
        public bool isNonDefaultPresetSelected = false;

        [ObservableProperty]
        public bool isInitializing = false;

        [ObservableProperty]
        public bool isManageProfilesButtonChecked = false;

        [ObservableProperty]
        public bool isFilterButtonChecked = false;

        public bool IsSessionActive
        {
            get => _latencyMonitorService.IsSessionActive;
            set
            {
                if (_latencyMonitorService.IsSessionActive != value)
                {
                    _latencyMonitorService.IsSessionActive = value;
                    OnPropertyChanged();
                    StartButtonCommand.NotifyCanExecuteChanged();
                    StopButtonCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartButtonCommand))]
        public LatencyMonitorPreset selectedPreset;

        public int PacketsSent
        {
            get => _latencyMonitorService.PacketsSent;
            set
            {
                if (_latencyMonitorService.PacketsSent != value)
                {
                    _latencyMonitorService.PacketsSent = value;
                    OnPropertyChanged();
                }
            }
        }

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

        [ObservableProperty]
        public Color selectedButtonForegroundColor;

        private LogHandler LogHandler { get; set; }

        private readonly ILatencyMonitorService _latencyMonitorService;

        private readonly ILatencyMonitorController _latencyMonitorController;
        #endregion Control Properties

        public LatencyMonitorViewModel(ILatencyMonitorService latencyMonitorService, ILatencyMonitorController latencyMonitorController)
        {
            _latencyMonitorService = latencyMonitorService;
            _latencyMonitorController = latencyMonitorController;
            _latencyMonitorService.PropertyChanged += LatencyMonitorService_PropertyChanged;
            _latencyMonitorController.SetErrorMessage += DisplayErrorMessage;

            LiveTargets = new();
            Traceroute = new();
            History = new();
            LogHandler = new();
            TargetPresets = new();
        }

        [RelayCommand(CanExecute = nameof(CanStartBtnBeClicked))]
        public async Task StartButtonAsync()
        {
            if (!SelectedPreset.TargetCollection.ToList().Any())
            {
                return;
            }

            if (IsPresetWindowVisible)
            {
                IsPresetWindowVisible = false;
                IsManageProfilesButtonChecked = false;
            }

            try
            {
                ResetPreSession();
                IsSessionActive = true;
                SetSessionStopwatchAsync();
                TargetList = SelectedPreset.TargetCollection.ToList();
                SetSubscriptions();

                await _latencyMonitorService.SetMonitoringSession();
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error);
                throw;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStopBtnBeClicked))]
        public async Task StopButtonAsync()
        {
            IsSessionActive = false;

            _latencyMonitorController.SendStopCodeRequest(true);

            UnsetSubscriptions();

            await Task.Delay(4000); // Wait to ensure the current session ends completely
            ResetPostSession();
        }

        [RelayCommand]
        public void ManageProfilesButton()
        {
            IsPresetWindowVisible = !IsPresetWindowVisible;

            if (IsPresetWindowVisible)
            {
                IsManageProfilesButtonChecked = true;
            }
            else
            {
                IsManageProfilesButtonChecked = false;
            }
        }

        [RelayCommand]
        public void FilterButton()
        {
            IsFilterWindowVisible = !IsFilterWindowVisible;

            if (IsFilterWindowVisible)
            {
                IsFilterButtonChecked = true;
            }
            else
            {
                IsFilterButtonChecked = false;
            }
        }

        [RelayCommand]
        public async Task RefreshFiltersButtonAsync()
        {

        }

        [RelayCommand]
        public void NewPresetButton()
        {
            if (TargetPresets.Count == 0)
            {
                TargetPresets.Add(new LatencyMonitorPreset("Default"));
            }

            if (SelectedPreset == null)
            {
                SelectedPreset = new();
                IsNonDefaultPresetSelected = false;
            }

            SelectedPreset = new LatencyMonitorPreset();
            TargetToAddToPreset = string.Empty;
            PresetName = string.Empty;

            TargetPresets.Add(SelectedPreset);
        }

        [RelayCommand]
        public void DeletePresetButton()
        {
            if (SelectedPreset != null)
            {
                TargetPresets.Remove(SelectedPreset);
                SelectedPreset = TargetPresets[0];
                IsNonDefaultPresetSelected = false;
            }
        }

        [RelayCommand]
        public void AddItemButton()
        {
            if (TargetToAddToPreset != string.Empty)
            {
                SelectedPreset.TargetCollection.Add(TargetToAddToPreset);
                TargetToAddToPreset = string.Empty;
            }
        }

        [RelayCommand]
        public void RemoveItemButton(string item)
        {
            if (SelectedPreset.TargetCollection.Contains(item))
            {
                SelectedPreset.TargetCollection.Remove(item);
            }
        }

        #region Private Methods
        private void SetSubscriptions()
        {
            _latencyMonitorController.SetLiveTargetData += SetLiveTargets;
            _latencyMonitorController.SetTracerouteData += SetTraceroute;
            _latencyMonitorController.SetSelectedTargetData += SetSelectedLiveTarget;
            _latencyMonitorController.UpdateLiveTargetData += UpdateLiveTargets;
            _latencyMonitorController.UpdateTracerouteData += UpdateTraceroute;
        }

        private void UnsetSubscriptions()
        {
            _latencyMonitorController.SetLiveTargetData -= SetLiveTargets;
            _latencyMonitorController.SetTracerouteData -= SetTraceroute;
            _latencyMonitorController.SetSelectedTargetData -= SetSelectedLiveTarget;
            _latencyMonitorController.UpdateLiveTargetData -= UpdateLiveTargets;
            _latencyMonitorController.UpdateTracerouteData -= UpdateTraceroute;
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

            AllTargets.Add(data);
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
            TargetList.Clear();
        }

        private void ResetPreSession()
        {
            LiveTargets.Clear();
            Traceroute.Clear();
            History.Clear();
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

        partial void OnPresetNameChanged(string value)
        {
            if (value != string.Empty)
            {
                SelectedPreset.PresetName = value;
            }
        }

        private bool CanStartBtnBeClicked()
        {
            bool statusCheck = false;

            if (IsSessionActive || SelectedPreset == null || SelectedPreset.UUID == "Default")
            {
                statusCheck = false;
            }
            else
            {
                statusCheck = true;
            }

            return statusCheck;
        }

        private bool CanStopBtnBeClicked()
        {
            bool statusCheck = false;

            if (IsSessionActive)
            {
                statusCheck = true;
            }
            else
            {
                statusCheck = false;
            }

            return statusCheck;
        }

        private void LatencyMonitorService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LatencyMonitorService.PacketsSent))
            {
                OnPropertyChanged(nameof(PacketsSent));
            }
            else if (e.PropertyName == nameof(LatencyMonitorService.SessionDuration))
            {
                OnPropertyChanged(nameof(SessionDuration));
            }
            else if (e.PropertyName == nameof(LatencyMonitorService.ReportID))
            {
                OnPropertyChanged(nameof(ReportNumber));
            }
            else if (e.PropertyName == nameof(LatencyMonitorService.StartTime))
            {
                OnPropertyChanged(nameof(StartTime));
            }
        }

        private void DisplayErrorMessage(LogType logType, string message)
        {
            MessageBoxImage iconType;
            string title;

            switch (logType)
            {
                case LogType.Error:
                    iconType = MessageBoxImage.Error;
                    title = "Latency Monitor Error";
                    break;

                case LogType.Warning:
                    iconType = MessageBoxImage.Warning;
                    title = "Latency Monitor Warning";
                    break;

                default:
                    iconType = MessageBoxImage.Information;
                    title = "Latency Monitor Information";
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, iconType);

            IsSessionActive = false;

            _latencyMonitorController.SendStopCodeRequest(true);

            UnsetSubscriptions();
            ResetPostSession();
        }
        #endregion Private Methods
    }
}
