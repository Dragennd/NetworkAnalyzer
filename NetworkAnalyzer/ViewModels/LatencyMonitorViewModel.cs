using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.ViewModels;

internal partial class LatencyMonitorViewModel : ObservableValidator
{
    // Corresponds to the LiveTargets DataGrid
    // - Updates each loop, regardless of an active selection
    public ObservableCollection<LatencyMonitorData> LiveTargets { get; set; }

    // Corresponds to the Traceroute DataGrid
    // - Updates each loop, based on the selection in the LiveTargets DataGrid
    public ObservableCollection<LatencyMonitorData> Traceroute { get; set; }

    // Contains a list of available target profiles from the database
    public ObservableCollection<LatencyMonitorPreset> Presets { get; set; }
    
    // Contains targets entered by the user to be stored in a preset
    public ObservableCollection<string> PresetTargets { get; set; }

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

    [ObservableProperty]
    public partial string TargetToAddToPreset { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PresetName { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial string SelectedPresetTarget { get; set; }

    [ObservableProperty]
    public partial bool IsPresetWindowVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsPresetDropdownEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsPresetSelected { get; set; } = false;

    [ObservableProperty]
    public partial bool IsInitializing { get; set; } = false;

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
    public partial LatencyMonitorPreset? SelectedPreset { get; set; }

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

    public LatencyMonitorData SelectedLiveTracerouteTarget
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();

                if (value != null)
                {
                    _latencyMonitorController.SendSetSelectedTargetGUIDRequest(value.TargetGUID);
                }
            }
        }
    }
    
    [ObservableProperty]
    public partial string SessionStatus { get; private set; }

    [ObservableProperty]
    public partial IBrush StatusBackgroundBrush { get; set; }
    
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly ILatencyMonitorService _latencyMonitorService;
    private readonly ILatencyMonitorController _latencyMonitorController;
    private readonly IDatabaseHandler _dbHandler;
    
    public LatencyMonitorViewModel(ILatencyMonitorService latencyMonitorService, ILatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
    {
        _latencyMonitorService = latencyMonitorService;
        _latencyMonitorController = latencyMonitorController;
        _dbHandler = dbHandler;
        _latencyMonitorController.SetTracerouteTargets += SetTracerouteTargets;
        _latencyMonitorController.SetSessionStatus += SetSessionStatus;

        LiveTargets = new();
        Traceroute = new();
        Presets = new();
        PresetTargets = new();
        UserDefinedTargets = new();

        _ = LoadPresetsFromDatabaseAsync();
        
        SetSessionStatus(LatencyMonitorSessionStatus.Idle);
    }

    [RelayCommand(CanExecute = nameof(CanStartBtnBeClicked))]
    public async Task StartButtonAsync()
    {
        if (SelectedPreset == null || !SelectedPreset.TargetCollection.ToList().Any())
        {
            return;
        }

        try
        {
            ResetSession();
            IsSessionActive = true;
            SetSessionStopwatchAsync();
            // To-Do: Add logic to instruct the user to remedy the ping issue and offer a one-click
            // resolution to be able to launch a script under sudo and request the sudo password
            // Popup should inform the user of the "why" and give the option to configure automatically
            // or skip, with a warning that skipping will result in disabling the Latency Monitor and the IP Scanner
            TargetList = SelectedPreset.TargetCollection.ToList();

            SetSubscriptions();

            await _latencyMonitorService.SetMonitoringSession();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error, ReportType.UserTargets);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopBtnBeClicked))]
    public async Task StopButtonAsync()
    {
        IsSessionActive = false;

        _latencyMonitorController.SendStopCodeRequest(true);
        _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.Idle);

        UnsetSubscriptions();

        await Task.Delay(4000); // Wait to ensure the current session ends completely
    }
    
    [RelayCommand]
    public void NewPresetButton()
    {
        TargetToAddToPreset = string.Empty;
        PresetName = string.Empty;
        PresetTargets.Clear();
        SelectedPreset = null;
        IsPresetWindowVisible = true;
        IsPresetDropdownEnabled = false;
    }

    [RelayCommand]
    public void EditPresetButton()
    {
        if (SelectedPreset == null)
        {
            return;
        }
        
        IsPresetWindowVisible = true;
        IsPresetDropdownEnabled = false;
        PresetName = SelectedPreset.PresetName;
        
        foreach (var target in SelectedPreset.TargetCollection)
        {
            PresetTargets.Add(target);
        }
    }

    [RelayCommand]
    public async Task SavePresetButtonAsync()
    {
        if (SelectedPreset == null)
        {
            LatencyMonitorPreset preset = new(true);
            
            if (string.IsNullOrEmpty(PresetName))
            {
                PresetName = DateTime.Now.ToString();
            }
            
            preset.PresetName = PresetName;
            preset.TargetCollection = PresetTargets;

            await _dbHandler.NewLatencyMonitorTargetProfileAsync(preset);
            Presets.Add(preset);
            SelectedPreset = Presets.Last();
        }
        else
        {
            SelectedPreset.PresetName = PresetName;
            SelectedPreset.TargetCollection = PresetTargets;
            await _dbHandler.UpdateLatencyMonitorTargetProfileAsync(SelectedPreset);
        }
        
        IsPresetWindowVisible = false;
        IsPresetDropdownEnabled = true;
    }

    [RelayCommand]
    public async Task DeletePresetButtonAsync()
    {
        if (SelectedPreset == null)
        {
            return;
        }
        
        await _dbHandler.DeleteSelectedProfileAsync(SelectedPreset);
        Presets.Remove(SelectedPreset);
        IsPresetWindowVisible = false;
        SelectedPreset = Presets.FirstOrDefault();
    }

    [RelayCommand]
    public void CancelPresetChangesButton()
    {
        IsPresetWindowVisible = false;
        IsPresetDropdownEnabled = true;
        TargetToAddToPreset = string.Empty;
        PresetName = string.Empty;
        PresetTargets.Clear();
    }

    [RelayCommand]
    public void AddItemButton()
    {
        if (TargetToAddToPreset == string.Empty)
        {
            return;
        }
        
        PresetTargets.Add(TargetToAddToPreset.Trim());
        TargetToAddToPreset = string.Empty;
    }

    [RelayCommand]
    public void RemoveItemButton()
    {
        PresetTargets.Remove(SelectedPresetTarget);
    }
    
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
        Presets.Clear();

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
                foreach (var target in JsonSerializer.Deserialize<ObservableCollection<string>>(preset.TargetCollection))
                {
                    newPreset.TargetCollection.Add(target);   
                }
            }

            Presets.Add(newPreset);
        }
    }

    private void SetLiveTargets(LatencyMonitorData data)
    {
        if (data.IsUserDefinedTarget)
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

            foreach (var t in _latencyMonitorService
                         .AllTargets
                         .Where(a => a.TracerouteGUID == data.TracerouteGUID).OrderBy(a => a.Hop))
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

        OnPropertyChanged(nameof(Presets));
    }

    // partial void OnPresetNameChanged(string value)
    // {
    //     if (value != string.Empty)
    //     {
    //         SelectedPreset.PresetName = value;
    //     }
    // }

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

    private void SetSessionStatus(LatencyMonitorSessionStatus status)
    {
        StatusBackgroundBrush = status switch
        {
            LatencyMonitorSessionStatus.Idle => Brushes.Gray,
            LatencyMonitorSessionStatus.GeneratingTraceroutes => Brushes.DarkOrange,
            LatencyMonitorSessionStatus.MonitoringTargets => Brushes.YellowGreen,
            LatencyMonitorSessionStatus.Error => Brushes.Red,
            _ => Brushes.Transparent
        };

        SessionStatus = status switch
        {
            LatencyMonitorSessionStatus.Idle => "Idle",
            LatencyMonitorSessionStatus.GeneratingTraceroutes => "Generating Traceroutes",
            LatencyMonitorSessionStatus.MonitoringTargets => "Monitoring Targets",
            LatencyMonitorSessionStatus.Error => "Error",
            _ => "Unknown"
        };
    }
}