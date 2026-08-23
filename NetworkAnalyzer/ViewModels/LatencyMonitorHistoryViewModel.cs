using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.ViewModels;

internal partial class LatencyMonitorHistoryViewModel : ObservableValidator
{
    public ObservableCollection<FilterData> ActiveFilters { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries>? UserDefinedTargets { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries>? TracerouteTargets { get; set; } = new();
    public List<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().ToList();
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

                if (field is FilterType.TargetAddress or FilterType.TracerouteTarget)
                {
                    FilterOperators.Clear();
                    
                    FilterOperators.Add(FilterOperator.EqualTo);
                    FilterOperators.Add(FilterOperator.NotEqualTo);
                    
                    IsBinaryFilterOperatorComboBoxVisible = false;
                    IsFilterOperatorComboBoxVisible = true;
                    IsTextFilterValueTextBoxVisible = false;
                    IsDateTimePickerVisible = false;

                    if (field is FilterType.TargetAddress)
                    {
                        IsUserDefinedTargetsComboBoxVisible = true;
                        IsTracerouteTargetsComboBoxVisible = false;
                    }
                    else
                    {
                        IsTracerouteTargetsComboBoxVisible = true;
                        IsUserDefinedTargetsComboBoxVisible = false;
                    }
                }
                else if (field is FilterType.LostPacket)
                {
                    BinaryFilterOperators.Clear();
                    
                    foreach (var filterOperator in Enum.GetValues<BinaryFilterOperator>())
                    {
                        BinaryFilterOperators.Add(filterOperator);
                    }
                    
                    IsBinaryFilterOperatorComboBoxVisible = true;
                    IsFilterOperatorComboBoxVisible = false;
                    IsTextFilterValueTextBoxVisible = false;
                    IsUserDefinedTargetsComboBoxVisible = false;
                    IsTracerouteTargetsComboBoxVisible = false;
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
                    IsUserDefinedTargetsComboBoxVisible = false;
                    IsTracerouteTargetsComboBoxVisible = false;

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
    public partial LatencyMonitorReportEntries? SelectedUserDefinedTarget { get; set; }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    public partial LatencyMonitorReportEntries? SelectedTracerouteTarget { get; set; }
    
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
    public partial bool IsBinaryFilterOperatorComboBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsFilterOperatorComboBoxVisible { get; set; } = false;
    
    [ObservableProperty]
    public partial bool IsUserDefinedTargetsComboBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsTracerouteTargetsComboBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsTextFilterValueTextBoxVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsDateTimePickerVisible { get; set; } = false;

    public LatencyMonitorHistoryViewModel()
    {
        
    }

    [RelayCommand(CanExecute = nameof(CanApplyFiltersButtonBeClicked))]
    public void ApplyFilters()
    {
        
    }
    
    [RelayCommand]
    public void RemoveNotification(string guid)
    {
        ActiveFilters.Remove(ActiveFilters.First(a => a.GUID == guid));
    }

    private bool CanApplyFiltersButtonBeClicked()
    {
        var canClick = false;

        switch (SelectedFilterType)
        {
            case FilterType.TargetAddress 
                when SelectedUserDefinedTarget is not null 
                     && SelectedFilterOperator is not null:
                
            case FilterType.TracerouteTarget 
                when SelectedTracerouteTarget is not null 
                     && SelectedFilterOperator is not null:
                
            case FilterType.LostPacket 
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