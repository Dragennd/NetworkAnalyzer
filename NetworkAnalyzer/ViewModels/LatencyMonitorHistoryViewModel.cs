using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.ViewModels;

internal partial class LatencyMonitorHistoryViewModel : ObservableValidator
{
    public ObservableCollection<FilterData> ActiveFilters { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries> UserDefinedTargets { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries> TracerouteTargets { get; set; } = new();
    public List<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().ToList();
    public ObservableCollection<FilterOperator> FilterOperators { get; set; } = new();
    public ObservableCollection<BinaryFilterOperator> BinaryFilterOperators { get; set; } = new();
    
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
                }
            }
        }
    }
    
    [ObservableProperty]
    public partial FilterOperator SelectedFilterOperator { get; set; }
    
    [ObservableProperty]
    public partial BinaryFilterOperator SelectedBinaryFilterOperator { get; set; }
    
    [ObservableProperty]
    public partial bool IsBinaryFilterOperatorComboBoxVisible { get; set; }
    
    [ObservableProperty]
    public partial bool IsFilterOperatorComboBoxVisible { get; set; }

    public LatencyMonitorHistoryViewModel()
    {
        
    }

    [RelayCommand]
    public void NewFilterOption()
    {
        
    }
    
    [RelayCommand]
    public void RemoveNotification(string guid)
    {
        ActiveFilters.Remove(ActiveFilters.First(a => a.GUID == guid));
    }
}