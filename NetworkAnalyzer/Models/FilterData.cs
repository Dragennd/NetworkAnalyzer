using System.Runtime.InteropServices;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Models;

internal class FilterData
{
    public FilterType FilterType { get; set; }
    public FilterOperator FilterOperator { get; set; }
    public BinaryFilterOperator BinaryFilterOperator { get; set; } = BinaryFilterOperator.All;
    public string FilterValue { get; set; } = string.Empty;
    public string DisplayType { get; set; } = string.Empty;
    public string DisplayOperator { get; set; } = string.Empty;
    public string GUID { get; set; } = string.Empty;
    public string FilterQuery { get; set; } = string.Empty;
    private readonly LatencyMonitorController _latencyMonitorController;

    public FilterData(
        [Optional]FilterType filterType, 
        [Optional]BinaryFilterOperator binaryFilterOperator,
        [Optional]string guid, 
        FilterOperator filterOperator, 
        string filterValue,
        LatencyMonitorController latencyMonitorController)
    {
        FilterType = filterType;
        BinaryFilterOperator = binaryFilterOperator;
        GUID = guid;
        FilterOperator = filterOperator;
        FilterValue = filterValue;
        _latencyMonitorController = latencyMonitorController;
        DisplayType = FilterType.ToString();
        
        if (FilterType == FilterType.TargetAddress)
        {
            DisplayType = "TracerouteGUID";
        }
        else if (FilterType == FilterType.TracerouteTarget)
        {
            DisplayType = "TargetGUID";
        }

        if (FilterType == FilterType.LostPacket)
        {
            DisplayOperator = BinaryFilterOperator.ToString();
            FilterValue = "-";
        }
        else
        {
            DisplayOperator = FilterOperator.ToString();
        }

        FilterQuery = SetFilterQuery();
    }

    public void ClearFilter()
    {
        _latencyMonitorController.SendRemoveFilterRequest(GUID);
    }

    private string SetFilterQuery()
    {
        string convertedFilterOperator = string.Empty;

        switch (FilterOperator)
        {
            case FilterOperator.EqualTo:
                convertedFilterOperator = "==";
                break;
            case FilterOperator.NotEqualTo:
                convertedFilterOperator = "!=";
                break;
            case FilterOperator.GreaterThan:
                convertedFilterOperator = ">";
                break;
            case FilterOperator.GreaterThanOrEqualTo:
                convertedFilterOperator = ">=";
                break;
            case FilterOperator.LessThan:
                convertedFilterOperator = "<";
                break;
            case FilterOperator.LessThanOrEqualTo:
                convertedFilterOperator = "<=";
                break;
        }

        if (BinaryFilterOperator == BinaryFilterOperator.True || BinaryFilterOperator == BinaryFilterOperator.False)
        {
            return $"{DisplayType} == {BinaryFilterOperator}";
        }
        else if (GUID != null)
        {
            return $"{DisplayType} {convertedFilterOperator} \"{GUID}\"";
        }
        else if (DisplayType == "CurrentLatency" || DisplayType == "LowestLatency" || DisplayType == "HighestLatency" || DisplayType == "AverageLatency")
        {
            return $"CAST({DisplayType} as INTEGER) {convertedFilterOperator} \"{FilterValue}\"";
        }
        else
        {
            return $"{DisplayType} {convertedFilterOperator} \"{FilterValue}\"";
        }
    }
}