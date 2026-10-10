using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    [NotifyCanExecuteChangedFor(nameof(GenerateReportButtonCommand))]
    public partial LatencyMonitorReportEntries? SelectedUserDefinedTarget { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? StartDate { get; set; } = new DateTimeOffset(new DateTime(2000, 1, 1));

    [ObservableProperty]
    public partial DateTimeOffset? EndDate { get; set; } = new DateTimeOffset(new DateTime(2000, 1, 1));
    
    [ObservableProperty]
    public partial TimeSpan? StartTime { get; set; } = new TimeSpan(9, 15, 0);

    [ObservableProperty]
    public partial TimeSpan? EndTime { get; set; } = new TimeSpan(9, 15, 0);

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

    [ObservableProperty]
    public partial bool IsNewReportGenerationCardVisible { get; set; } = false;
    
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
    public void NewReportButton()
    {
        IsNewReportGenerationCardVisible = true;
    }

    [RelayCommand]
    public void CancelNewReportButton()
    {
        IsNewReportGenerationCardVisible = false;
    }
    
    [RelayCommand(CanExecute = nameof(CanGenerateNewReportButtonBeClicked))]
    public async Task GenerateReportButtonAsync()
    {
        if (SelectedSessionData == null)
        {
            return;
        }

        var start = $"{StartDate?.ToString("MM/dd/yyyy")} {StartTime.ToString()}";
        var end = $"{EndDate?.ToString("MM/dd/yyyy")} {EndTime.ToString()}";
        // To-Do: Hide the New Report Generation card and show a loading bar while the report is being generated
        await _reportService.NewReport(SelectedSessionData, SelectedUserDefinedTarget, IsDateRangeChecked, start, end);
        IsNewReportGenerationCardVisible = false;
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
        ConcurrentBag<ReportExplorerData> temp = new();
        foreach (var entry in await _dbHandler.GetLatencyMonitorReportsAsync())
        {
            await Task.Run(async () =>
            {
                temp.Add(entry);
            });
        }

        foreach (var entry in await _dbHandler.GetIPScannerReportsAsync())
        {
            await Task.Run(async () =>
            {
                temp.Add(entry);
            });
        }

        foreach (var item in temp)
        {
            AvailableSessionData.Add(item);
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

    private bool CanGenerateNewReportButtonBeClicked()
    {
        var canClick = true;

        if (SelectedUserDefinedTarget == null)
            canClick = false;

        return canClick;
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