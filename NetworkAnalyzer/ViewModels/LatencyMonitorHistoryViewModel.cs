using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

internal partial class LatencyMonitorHistoryViewModel : ObservableValidator
{
    public ObservableCollection<FilterData> ActiveFilters { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries> AllData { get; set; } = new();
    public ObservableCollection<FilterTargetData>? DistinctTargets { get; set; } = new();
    public ObservableCollection<LatencyMonitorReport> AvailableSessions { get; set; } = new();
    public List<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().Where(a => a != FilterType.TracerouteTarget).ToList();
    public ObservableCollection<FilterOperator>? FilterOperators { get; set; } = new();
    public ObservableCollection<BinaryFilterOperator>? BinaryFilterOperators { get; set; } = new();
    
    public FilterType SelectedFilterType
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;

                if (field is FilterType.UserDefinedTarget or FilterType.TracerouteTarget)
                {
                    FilterOperators.Clear();
                    
                    FilterOperators.Add(FilterOperator.EqualTo);
                    FilterOperators.Add(FilterOperator.NotEqualTo);
                    
                    IsBinaryFilterOperatorComboBoxVisible = false;
                    IsFilterOperatorComboBoxVisible = true;
                    IsTextFilterValueTextBoxVisible = false;
                    IsDateTimePickerVisible = false;
                    IsDistinctTargetsControlVisible = true;
                }
                else if (field is FilterType.FailedPing)
                {
                    BinaryFilterOperators.Clear();
                    
                    foreach (var filterOperator in Enum.GetValues<BinaryFilterOperator>())
                    {
                        BinaryFilterOperators.Add(filterOperator);
                    }
                    
                    IsBinaryFilterOperatorComboBoxVisible = true;
                    IsFilterOperatorComboBoxVisible = false;
                    IsTextFilterValueTextBoxVisible = false;
                    IsDistinctTargetsControlVisible = false;
                }
                else
                {
                    FilterOperators.Clear();
                    
                    foreach (var filterOperator in Enum.GetValues<FilterOperator>())
                    {
                        FilterOperators.Add(filterOperator);
                    }
                    
                    IsBinaryFilterOperatorComboBoxVisible = false;
                    IsFilterOperatorComboBoxVisible = true;
                    IsDistinctTargetsControlVisible = false;

                    if (field is FilterType.TimeStamp)
                    {
                        IsDateTimePickerVisible = true;
                        IsTextFilterValueTextBoxVisible = false;
                    }
                    else
                    {
                        IsTextFilterValueTextBoxVisible = true;
                        IsDateTimePickerVisible = false;
                    }
                }
            }
        }
    }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial FilterOperator? SelectedFilterOperator { get; set; }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial BinaryFilterOperator? SelectedBinaryFilterOperator { get; set; }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial FilterTargetData? SelectedDistinctTarget { get; set; }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial string? TextFilterValue { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial DateTimeOffset? SelectedDate { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial TimeSpan? SelectedTime { get; set; } = new TimeSpan(9, 15, 0);

    [ObservableProperty]
    public partial LatencyMonitorReport SelectedSession { get; set; }
    
    [ObservableProperty]
    public partial bool IsBinaryFilterOperatorComboBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsFilterOperatorComboBoxVisible { get; set; } = false;
    
    [ObservableProperty]
    public partial bool IsDistinctTargetsControlVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsTextFilterValueTextBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsDateTimePickerVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsUseTracerouteTargetChecked { get; set; } = false;

    [ObservableProperty]
    public partial bool IsLoadReportsWindowVisible { get; set; }
    
    [ObservableProperty]
    public partial string ReportGUID { get; set; }
    
    private readonly LatencyMonitorService _latencyMonitorService = 
        App.AppHost.Services.GetRequiredService<LatencyMonitorService>();

    private readonly LatencyMonitorController _latencyMonitorController =
        App.AppHost.Services.GetRequiredService<LatencyMonitorController>();

    public LatencyMonitorHistoryViewModel(IDatabaseHandler dbHandler)
    {
        _latencyMonitorController.SetHistoryReport += AddReport;
        _latencyMonitorController.SetHistoryReportEntry += AddReportEntry;
        _latencyMonitorController.SetHistoryDistinctTarget += AddDistinctTarget;
    }

    [RelayCommand]
    public async Task ShowLoadReportsWindowAsync()
    {
        IsLoadReportsWindowVisible = !IsLoadReportsWindowVisible;
        await _latencyMonitorService.GetReportsAsync();
    }

    [RelayCommand]
    public void HideLoadReportsWindow()
    {
        IsLoadReportsWindowVisible = false;
    }

    [RelayCommand]
    public async Task LoadSessionAsync()
    {
        IsLoadReportsWindowVisible = false;
        AllData.Clear();
        await _latencyMonitorService.GetReportEntriesAsync(SelectedSession.ReportGUID);
        await _latencyMonitorService.GetDistinctHistoryTargetsAsync(SelectedSession.ReportGUID);
    }

    [RelayCommand(CanExecute = nameof(CanApplyFiltersButtonBeClicked))]
    public void ApplyFilters()
    {
        switch (SelectedFilterType)
        {
            case FilterType.UserDefinedTarget:
                ActiveFilters.Add(new FilterData(
                    SelectedFilterType,
                    (FilterOperator)SelectedFilterOperator,
                    SelectedDistinctTarget,
                    IsUseTracerouteTargetChecked));
                break;
            
            case FilterType.LowestLatency:
            case FilterType.AverageLatency:
            case FilterType.CurrentLatency:
            case FilterType.HighestLatency:
                ActiveFilters.Add(new FilterData(
                    SelectedFilterType,
                    (FilterOperator)SelectedFilterOperator,
                    TextFilterValue));
                break;
            
            case FilterType.FailedPing:
                ActiveFilters.Add(new FilterData(
                    SelectedFilterType,
                    (BinaryFilterOperator)SelectedBinaryFilterOperator));
                break;
            
            case FilterType.TimeStamp:
                ActiveFilters.Add(new FilterData(
                    SelectedFilterType,
                    (FilterOperator)SelectedFilterOperator,
                    (DateTimeOffset)SelectedDate,
                    (TimeSpan)SelectedTime));
                break;
        }
    }
    
    [RelayCommand]
    public void RemoveNotification(string guid)
    {
        ActiveFilters.Remove(ActiveFilters.First(a => a.FilterGUID == guid));
    }

    private void AddDistinctTarget(FilterTargetData data)
    {
        DistinctTargets?.Add(data);
    }

    private void AddReportEntry(LatencyMonitorReportEntries reportEntry)
    {
        AllData.Add(reportEntry);
    }

    private void AddReport(LatencyMonitorReport report)
    {
        AvailableSessions.Add(report);
    }
    
    private bool CanApplyFiltersButtonBeClicked()
    {
        var canClick = false;

        switch (SelectedFilterType)
        {
            case FilterType.UserDefinedTarget 
                when SelectedDistinctTarget is not null 
                     && SelectedFilterOperator is not null:
                
            case FilterType.FailedPing 
                when SelectedBinaryFilterOperator is not null:
                
            case FilterType.TimeStamp 
                when SelectedDate is not null 
                     && SelectedTime is not null 
                     && SelectedFilterOperator is not null:
                
            case FilterType.CurrentLatency 
                or FilterType.LowestLatency 
                or FilterType.AverageLatency 
                or FilterType.HighestLatency 
                when !string.IsNullOrWhiteSpace(TextFilterValue) 
                     && SelectedFilterOperator is not null:
                
                canClick = true;
                break;
        }

        return canClick;
    }
}