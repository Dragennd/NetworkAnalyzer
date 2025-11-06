using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using NetworkAnalyzer.Apps.Utilities;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
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
        public ObservableCollection<LatencyMonitorReportEntries> History { get; set; }

        // Contains a list of available target profiles from the database
        public ObservableCollection<LatencyMonitorPreset> TargetPresets { get; set; }

        // Contains all filters currently applied to the Latency Monitor History section
        public ObservableCollection<FilterData> ActiveFilters { get; set; }

        // Contains all of the targets defined by the user for filtering history results
        public ObservableCollection<LatencyMonitorData> UserDefinedTargets { get; set; }

        // Contains all of the targets gathered by the traceroute for filtering history results
        public ObservableCollection<LatencyMonitorReportEntries> TracerouteTargets { get; set; }

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

        public string QuickStartAddress
        {
            get => _latencyMonitorService.QuickStartAddress;
            set
            {
                if (_latencyMonitorService.QuickStartAddress != value)
                {
                    _latencyMonitorService.QuickStartAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public int historyGridRow = 3;

        [ObservableProperty]
        public int historyGridRowSpan = 1;

        [ObservableProperty]
        public int historyGridColumn = 0;

        [ObservableProperty]
        public int historyGridColumnSpan = 1;

        [ObservableProperty]
        public string historySectionButtonKind = "ArrowExpand";

        [ObservableProperty]
        public string targetToAddToPreset = string.Empty;

        [ObservableProperty]
        public string presetName = string.Empty;

        [ObservableProperty]
        public string filterValue = string.Empty;

        [ObservableProperty]
        public bool isQuickStartCardVisible = true;

        [ObservableProperty]
        public bool isOverviewCardVisible = false;

        [ObservableProperty]
        public bool isPresetWindowVisible = false;

        [ObservableProperty]
        public bool isFilterWindowVisible = false;

        [ObservableProperty]
        public bool isColumnSelectorWindowVisible = false;

        [ObservableProperty]
        public bool isColumnSelectorButtonChecked = false;

        [ObservableProperty]
        public bool isNonDefaultPresetSelected = false;

        [ObservableProperty]
        public bool isPresetSelected = false;

        [ObservableProperty]
        public bool isInitializing = false;

        [ObservableProperty]
        public bool isManageProfilesButtonChecked = false;

        [ObservableProperty]
        public bool isFilterButtonChecked = false;

        [ObservableProperty]
        public bool isTargetAddressColumnVisible = true;

        [ObservableProperty]
        public bool isTargetNameColumnVisible = false;

        [ObservableProperty]
        public bool isHopColumnVisible = false;

        [ObservableProperty]
        public bool isLowColumnVisible = true;

        [ObservableProperty]
        public bool isHighColumnVisible = true;

        [ObservableProperty]
        public bool isAvgColumnVisible = true;

        [ObservableProperty]
        public bool isLostColumnVisible = true;

        [ObservableProperty]
        public bool isTotalLostColumnVisible = false;

        [ObservableProperty]
        public bool isTimeStampColumnVisible = true;

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
                    ClearResultsButtonCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [ObservableProperty]
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

                    if (value != null)
                    {
                        _latencyMonitorController.SendSetSelectedTargetGUIDRequest(value.TargetGUID);

                    }
                }
            }
        }

        private LatencyMonitorData _selectedLiveTracerouteTarget;
        public LatencyMonitorData SelectedLiveTracerouteTarget
        {
            get => _selectedLiveTracerouteTarget;
            set
            {
                if (_selectedLiveTracerouteTarget != value)
                {
                    _selectedLiveTracerouteTarget = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        _latencyMonitorController.SendSetSelectedTargetGUIDRequest(value.TargetGUID);

                    }
                }
            }
        }

        [ObservableProperty]
        public LatencyMonitorData selectedUserDefinedTarget;

        [ObservableProperty]
        public LatencyMonitorReportEntries selectedTracerouteTarget;

        [ObservableProperty]
        public FilterData selectedActiveFilter;

        [ObservableProperty]
        public FilterType selectedFilterType;

        [ObservableProperty]
        public FilterOperator selectedFilterOperator;

        [ObservableProperty]
        public BinaryFilterOperator selectedBinaryFilterOperator;

        [ObservableProperty]
        public Color selectedButtonForegroundColor;

        public Task InitializePresets { get; private set; }

        private bool IsHistorySectionFullSize { get; set; } = false;

        private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();

        private readonly LatencyMonitorDetailsWindow _detailsWindow = App.AppHost.Services.GetRequiredService<LatencyMonitorDetailsWindow>();

        private readonly ILatencyMonitorService _latencyMonitorService;

        private readonly ILatencyMonitorController _latencyMonitorController;

        private readonly IDatabaseHandler _dbHandler;
        #endregion Control Properties

        public LatencyMonitorViewModel(ILatencyMonitorService latencyMonitorService, ILatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
        {
            _latencyMonitorService = latencyMonitorService;
            _latencyMonitorController = latencyMonitorController;
            _dbHandler = dbHandler;
            _latencyMonitorService.PropertyChanged += LatencyMonitorService_PropertyChanged;
            _latencyMonitorController.SetErrorMessage += DisplayErrorMessage;
            _latencyMonitorController.SetTracerouteTargets += SetTracerouteTargets;
            _latencyMonitorController.SetHistoryData += SetHistoryData;

            LiveTargets = new();
            Traceroute = new();
            History = new();
            TargetPresets = new();
            ActiveFilters = new();
            UserDefinedTargets = new();
            TracerouteTargets = new();

            InitializePresets = LoadPresetsFromDatabaseAsync();
        }

        [RelayCommand(CanExecute = nameof(CanStartBtnBeClicked))]
        public async Task StartButtonAsync()
        {
            if ((SelectedPreset == null || !SelectedPreset.TargetCollection.ToList().Any()) && string.IsNullOrEmpty(QuickStartAddress))
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
                IsQuickStartCardVisible = false;
                IsOverviewCardVisible = true;
                ResetSession();
                IsSessionActive = true;
                SetSessionStopwatchAsync();

                if (string.IsNullOrEmpty(QuickStartAddress))
                {
                    TargetList = SelectedPreset.TargetCollection.ToList();
                }

                SetSubscriptions();

                await _latencyMonitorService.SetMonitoringSession();
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error);
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
        }

        [RelayCommand]
        public void ShowDetailsWindowButton()
        {
            if (_detailsWindow.IsVisible)
            {
                _detailsWindow.Hide();
            }
            else
            {
                _detailsWindow.Show();
            }
        }

        [RelayCommand(CanExecute = nameof(CanClearResultsBtnBeClicked))]
        public void ClearResultsButton()
        {
            ResetSession();
            IsQuickStartCardVisible = true;
            IsOverviewCardVisible = false;
            QuickStartAddress = string.Empty;
        }

        [RelayCommand]
        public async Task ManageProfilesButtonAsync()
        {
            IsPresetWindowVisible = !IsPresetWindowVisible;

            if (IsPresetWindowVisible)
            {
                await LoadPresetsFromDatabaseAsync();
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

            if (UserDefinedTargets.Count == 0)
            {
                foreach (var item in LiveTargets)
                {
                    UserDefinedTargets.Add(item);
                }
            }

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
        public void ApplyFilterButton()
        {
            if (SelectedFilterType != FilterType.LostPacket &&
               (FilterValue == string.Empty || SelectedFilterOperator == FilterOperator.None))
            {
                return;
            }

            ActiveFilters.Add(new FilterData(
                filterType: SelectedFilterType,
                filterOperator: SelectedFilterOperator,
                binaryFilterOperator: SelectedBinaryFilterOperator,
                filterValue: FilterValue
                ));
        }

        [RelayCommand]
        public void ManageColumnsButton()
        {
            IsColumnSelectorWindowVisible = !IsColumnSelectorWindowVisible;

            if (IsColumnSelectorWindowVisible)
            {
                IsColumnSelectorButtonChecked = true;
            }
            else
            {
                IsColumnSelectorButtonChecked = false;
            }
        }

        [RelayCommand]
        public void RemoveFilterButton(FilterData data)
        {
            ActiveFilters.Remove(data);
        }

        [RelayCommand]
        public void SetAddressFilterButton()
        {
            if (SelectedUserDefinedTarget != null && !ActiveFilters.Select(a => a.GUID).Contains(SelectedUserDefinedTarget.TracerouteGUID))
            {
                var itemToRemove = ActiveFilters.FirstOrDefault(a => a.DisplayType == "TracerouteGUID");

                if (itemToRemove != null)
                {
                    ActiveFilters.Remove(itemToRemove);
                }

                ActiveFilters.Add(new FilterData(
                    addressFilterType: AddressFilterType.UserDefinedTarget,
                    filterOperator: FilterOperator.EqualTo,
                    filterValue: SelectedUserDefinedTarget.TargetAddress,
                    guid: SelectedUserDefinedTarget.TracerouteGUID
                    ));
            }

            if (SelectedTracerouteTarget != null && !ActiveFilters.Select(a => a.GUID).Contains(SelectedTracerouteTarget.TargetGUID))
            {
                var itemToRemove = ActiveFilters.FirstOrDefault(a => a.DisplayType == "TargetGUID");

                if (itemToRemove != null)
                {
                    ActiveFilters.Remove(itemToRemove);
                }

                ActiveFilters.Add(new FilterData(
                    addressFilterType: AddressFilterType.TracerouteTarget,
                    filterOperator: FilterOperator.EqualTo,
                    filterValue: SelectedTracerouteTarget.TargetAddress,
                    guid: SelectedTracerouteTarget.TargetGUID
                    ));
            }
        }

        [RelayCommand]
        public void FetchHistoryDataButton()
        {
            History.Clear();
            _latencyMonitorService.GetHistoryData(ActiveFilters, ReportNumber);
            FilterButton();
        }

        [RelayCommand]
        public void SetHistorySectionSizeButton()
        {
            if (IsHistorySectionFullSize)
            {
                IsHistorySectionFullSize = false;
                HistoryGridRow = 3;
                HistoryGridRowSpan = 1;
                HistoryGridColumn = 0;
                HistoryGridColumnSpan = 1;
                HistorySectionButtonKind = "ArrowExpand";
            }
            else
            {
                IsHistorySectionFullSize = true;
                HistoryGridRow = 1;
                HistoryGridRowSpan = 3;
                HistoryGridColumn = 0;
                HistoryGridColumnSpan = 2;
                HistorySectionButtonKind = "ArrowCollapse";
            }
        }

        [RelayCommand]
        public async Task NewPresetButtonAsync()
        {
            SelectedPreset = new();
            TargetToAddToPreset = string.Empty;
            PresetName = string.Empty;

            await _dbHandler.NewLatencyMonitorTargetProfileAsync(SelectedPreset);
            TargetPresets.Add(SelectedPreset);
            await LoadPresetsFromDatabaseAsync();
            SelectedPreset = TargetPresets.Last();
        }

        [RelayCommand]
        public async Task SavePresetButtonAsync()
        {
            if (SelectedPreset != null)
            {
                SelectedPreset.PresetName = PresetName;
                await _dbHandler.UpdateLatencyMonitorTargetProfileAsync(SelectedPreset);
            }
        }

        [RelayCommand]
        public async Task DeletePresetButtonAsync()
        {
            if (SelectedPreset != null)
            {
                await _dbHandler.DeleteSelectedProfileAsync(SelectedPreset);
                TargetPresets.Remove(SelectedPreset);
                SelectedPreset = TargetPresets.FirstOrDefault();
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

        private async Task LoadPresetsFromDatabaseAsync()
        {
            TargetPresets.Clear();

            foreach (var preset in await _dbHandler.GetLatencyMonitorTargetProfilesAsync())
            {
                var newPreset = new LatencyMonitorPreset()
                {
                    ID = preset.ID,
                    PresetName = preset.ProfileName,
                    UUID = preset.UUID
                };

                if (preset.TargetCollection != null)
                {
                    newPreset.TargetCollection = JsonSerializer.Deserialize<ObservableCollection<string>>(preset.TargetCollection);
                }

                TargetPresets.Add(newPreset);
            }
        }

        private void SetHistoryData(List<LatencyMonitorReportEntries> data)
        {
            foreach (var item in data)
            {
                History.Add(item);
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

        private void ResetSession()
        {
            TargetList.Clear();
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
                IsPresetSelected = true;
            }
            else
            {
                PresetName = string.Empty;
                IsPresetSelected = false;
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

            if (IsSessionActive)
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

        private bool CanClearResultsBtnBeClicked()
        {
            bool statusCheck = false;

            if (IsSessionActive)
            {
                statusCheck = false;
            }
            else
            {
                statusCheck = true;
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

        partial void OnSelectedFilterTypeChanged(FilterType value)
        {
            SelectedFilterOperator = FilterOperator.None;
            FilterValue = string.Empty;
        }

        partial void OnSelectedUserDefinedTargetChanged(LatencyMonitorData value)
        {
            _latencyMonitorController.SendSetTracerouteTargetsRequest(value);
        }

        private async void SetTracerouteTargets(LatencyMonitorData data)
        {
            TracerouteTargets.Clear();

            foreach (var item in await _dbHandler.GetDistinctLatencyMonitorTracerouteTargetsAsync(data.TracerouteGUID))
            {
                if (item.TargetAddress != "Request timed out" && item.CurrentLatency != "-")
                {
                    TracerouteTargets.Add(item);
                }
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

            if (logType == LogType.Error)
            {
                IsSessionActive = false;

                _latencyMonitorController.SendStopCodeRequest(true);

                UnsetSubscriptions();
                ResetSession();
            }
        }
        #endregion Private Methods
    }
}
