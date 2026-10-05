using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.Services;

namespace NetworkAnalyzer.ViewModels;

internal partial class ReportsViewModel : ObservableValidator
{
    public ObservableCollection<ReportExplorerData> AvailableSessionData { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries> AvailableUserDefinedTargets { get; set; } = new();

    [ObservableProperty]
    public partial ReportExplorerData? SelectedSessionData { get; set; }

    [ObservableProperty]
    public partial LatencyMonitorReportEntries SelectedUserDefinedTarget { get; set; }

    [ObservableProperty]
    public partial string StartTime { get; set; }

    [ObservableProperty]
    public partial string EndTime { get; set; }

    [ObservableProperty]
    public partial bool IsAllDataChecked { get; set; } = true;

    [ObservableProperty]
    public partial bool IsDateRangeChecked { get; set; } = false;

    [ObservableProperty]
    public partial bool IsStartTimePickerVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsEndTimePickerVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsLatencyMonitorReportOptionsGridVisible { get; set; } = false;

    public Task InitializeAvailableSessions { get; private set; }
    private readonly IDatabaseHandler _dbHandler;
    private readonly ReportService _reportService = App.AppHost.Services.GetRequiredService<ReportService>();
    private readonly ReportsController _reportsController = App.AppHost.Services.GetRequiredService<ReportsController>();

    public ReportsViewModel(IDatabaseHandler dbHandler)
    {
        _dbHandler = dbHandler;
        
        _reportsController.UpdateAvailableSessionData += LoadAvailableSessionDataAsync;
        _reportsController.SetUserDefinedTargetData += FetchUserDefinedTargetsForSelectedSession;
        
        _reportsController.SendUpdateAvailableSessionDataRequest();
    }
    
    [RelayCommand]
    public async Task GenerateReportButtonAsync()
    {
        if (SelectedSessionData == null)
        {
            return;
        }

        await _reportService.NewReport(SelectedSessionData, SelectedUserDefinedTarget, IsDateRangeChecked, StartTime, EndTime);
    }

    [RelayCommand]
    public void OpenReportsDirectoryButton()
    {
        _reportService.OpenReportsDirectory();
    }

    [RelayCommand]
    public void FetchAvailableSessionDataButton()
    {
        IsLatencyMonitorReportOptionsGridVisible = false;
        _reportsController.SendUpdateAvailableSessionDataRequest();
    }

    [RelayCommand]
    public void ShowStartTimePickerButton()
    {
        IsStartTimePickerVisible = !IsStartTimePickerVisible;
    }

    [RelayCommand]
    public void ShowEndTimePickerButton()
    {
        IsEndTimePickerVisible = !IsEndTimePickerVisible;
    }

    #region Private Methods
    private async void LoadAvailableSessionDataAsync()
    {
        AvailableSessionData.Clear();
        // To-Do: Move to the ReportService class and send data back with events
        foreach (var entry in await _dbHandler.GetLatencyMonitorReportsAsync())
        {
            AvailableSessionData.Add(entry);
        }

        foreach (var entry in await _dbHandler.GetIPScannerReportsAsync())
        {
            AvailableSessionData.Add(entry);
        }
    }
    
    private async void FetchUserDefinedTargetsForSelectedSession(string data)
    {
        // To-Do: Move to the ReportService class and send data back with events
        foreach (var address in await _dbHandler.GetDistinctLatencyMonitorUserDefinedTargetsAsync(data))
        {
            AvailableUserDefinedTargets.Add(address);
        }
    }

    partial void OnSelectedSessionDataChanged(ReportExplorerData value)
    {
        AvailableUserDefinedTargets.Clear();

        if (SelectedSessionData == null)
        {
            return;
        }

        if (SelectedSessionData.Mode == ReportMode.LatencyMonitor)
        {
            _reportsController.SendSetUserDefinedTargetDataRequest(value.ReportGUID);
            IsLatencyMonitorReportOptionsGridVisible = true;
        }
        else
        {
            IsLatencyMonitorReportOptionsGridVisible = false;
        }
        
        IsAllDataChecked = true;
        IsDateRangeChecked = false;
    }
    #endregion Private Methods
}