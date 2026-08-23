using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Services;
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

    [ObservableProperty]
    public partial string ReportNumber { get; set; } = "N/A";

    [ObservableProperty]
    public partial string SessionDuration { get; set; } = "00.00:00:00";

    [ObservableProperty]
    public partial string StartTime { get; set; } = "N/A";

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

    public bool IsSessionActive
    {
        get => _latencyMonitorService.IsSessionActive;
        set
        {
            if (_latencyMonitorService.IsSessionActive != value)
            {
                _latencyMonitorService.IsSessionActive = value;
                StartButtonCommand.NotifyCanExecuteChanged();
                StopButtonCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [ObservableProperty]
    public partial LatencyMonitorPreset? SelectedPreset { get; set; }

    [ObservableProperty]
    public partial int PacketsSent { get; set; }

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
    public partial string SessionStatus { get; private set; } = "Idle";

    [ObservableProperty]
    public partial IBrush StatusBackgroundBrush { get; set; } = Brushes.Gray;

    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly LatencyMonitorService _latencyMonitorService = App.AppHost.Services.GetRequiredService<LatencyMonitorService>();
    private readonly LatencyMonitorController _latencyMonitorController;
    private readonly IDatabaseHandler _dbHandler;
    
    public LatencyMonitorViewModel(LatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
    {
        _latencyMonitorController = latencyMonitorController;
        _dbHandler = dbHandler;
        _latencyMonitorController.SetTracerouteTargets += SetTracerouteTargets;
        _latencyMonitorController.SetSessionStatus += SetSessionStatus;
        _latencyMonitorService.PropertyChanged += LatencyMonitorService_PropertyChanged;

        LiveTargets = new();
        Traceroute = new();
        Presets = new();
        PresetTargets = new();
        UserDefinedTargets = new();
        
        _ = LoadPresetsFromDatabaseAsync();
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

        UnsetSubscriptions();

        await Task.Delay(4000); // Wait to ensure the current session ends completely
        _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.Idle);
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
        
        PresetTargets.Clear();
        
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
            Presets.Clear();
            await LoadPresetsFromDatabaseAsync();
            SelectedPreset = Presets.Last();
        }
        else
        {
            SelectedPreset.PresetName = PresetName;
            SelectedPreset.TargetCollection.Clear();
            foreach (var target in PresetTargets)
            {
                SelectedPreset.TargetCollection.Add(target);
            }
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

        if (!IsPresetDropdownEnabled)
        {
            IsPresetDropdownEnabled = true;
        }
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
        ConcurrentBag<LatencyMonitorPreset> temp = new();

        foreach (var preset in await _dbHandler.GetLatencyMonitorTargetProfilesAsync())
        {
            await Task.Run(() =>
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

                temp.Add(newPreset);
            });
        }

        foreach (var preset in temp)
        {
            Presets.Add(preset);
        }
    }

    private void SetLiveTargets(LatencyMonitorData data)
    {
        if (data.IsUserDefinedTarget)
        {
            data.Latency = data.Latency;
            data.LowestLatency = data.LowestLatency;
            data.HighestLatency = data.HighestLatency;
            data.AverageLatency = data.AverageLatency;
            
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

    private void LatencyMonitorService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(LatencyMonitorService.ReportID):
                ReportNumber = _latencyMonitorService.ReportID;
                break;
            
            case nameof(LatencyMonitorService.StartTime):
                StartTime = _latencyMonitorService.StartTime;
                break;
            
            case nameof(LatencyMonitorService.PacketsSent):
                PacketsSent = _latencyMonitorService.PacketsSent;
                break;
            
            case nameof(LatencyMonitorService.IsSessionActive):
                IsSessionActive = _latencyMonitorService.IsSessionActive;
                break;
            
            case nameof(LatencyMonitorService.SessionDuration):
                SessionDuration = _latencyMonitorService.SessionDuration;
                break;
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
            LatencyMonitorSessionStatus.EndingSession => Brushes.Orange,
            LatencyMonitorSessionStatus.Error => Brushes.Red,
            _ => Brushes.Transparent
        };

        SessionStatus = status switch
        {
            LatencyMonitorSessionStatus.Idle => "Idle",
            LatencyMonitorSessionStatus.GeneratingTraceroutes => "Generating Traceroutes",
            LatencyMonitorSessionStatus.MonitoringTargets => "Monitoring Targets",
            LatencyMonitorSessionStatus.EndingSession => "Ending Session",
            LatencyMonitorSessionStatus.Error => "Error",
            _ => "Unknown"
        };
    }
}