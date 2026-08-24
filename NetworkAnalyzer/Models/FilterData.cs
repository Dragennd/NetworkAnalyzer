using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Models;

internal class FilterData
{
    public FilterType FilterType { get; set; }
    public FilterOperator FilterOperator { get; set; }
    public BinaryFilterOperator BinaryFilterOperator { get; set; }
    public string FilterValue { get; set; } = string.Empty;
    public string DisplayType { get; set; } = string.Empty;
    public string DisplayOperator { get; set; } = string.Empty;
    public string FilterGUID { get; set; }
    public string FilterQuery { get; set; } = string.Empty;
    public bool IsUseTracerouteTargetChecked { get; set; } = false;
    public DateTimeOffset? Date { get; set; }
    public TimeSpan? Time { get; set; }
    public FilterTargetData TargetData { get; set; }
    private readonly LatencyMonitorController _latencyMonitorController = App.AppHost.Services.GetRequiredService<LatencyMonitorController>();

    // Contstructor for use with latency values
    public FilterData(FilterType filterType, FilterOperator filterOperator, string filterValue)
    {
        FilterType = filterType;
        FilterOperator = filterOperator;
        FilterValue = filterValue;
        DisplayType = FilterType.ToString();
        FilterGUID = Guid.NewGuid().ToString();
        
        FilterQuery = SetLatencyFilterQuery();
    }

    // Constructor for use with target values
    public FilterData(FilterType filterType, FilterOperator filterOperator, FilterTargetData targetData, bool isUseTracerouteTargetChecked)
    {
        FilterOperator = filterOperator;
        TargetData = targetData;
        IsUseTracerouteTargetChecked = isUseTracerouteTargetChecked;
        FilterGUID = Guid.NewGuid().ToString();
        
        if (IsUseTracerouteTargetChecked)
        {
            FilterType = FilterType.TracerouteTarget;   
        }
        else
        {
            FilterType = filterType;
        }

        FilterQuery = SetTargetFilterQuery();
    }

    // Constructor for use with failed ping values
    public FilterData(FilterType filterType, BinaryFilterOperator binaryFilterOperator)
    {
        FilterType = filterType;
        BinaryFilterOperator = binaryFilterOperator;
        DisplayType = FilterType.ToString();
        FilterGUID = Guid.NewGuid().ToString();
        DisplayOperator = BinaryFilterOperator.ToString();

        FilterQuery = SetBinaryFilterQuery();
    }

    // Constructor for use with DateTime values
    public FilterData(FilterType filterType, FilterOperator filterOperator, DateTimeOffset date, TimeSpan time)
    {
        FilterType = filterType;
        FilterOperator = filterOperator;
        DisplayType = "TimeStamp";
        Date = date;
        Time = time;
        FilterGUID = Guid.NewGuid().ToString();

        FilterQuery = SetDateTimeFilterQuery();
    }

    public void ClearFilter()
    {
        _latencyMonitorController.SendRemoveFilterRequest(FilterGUID);
    }

    private string SetBinaryFilterQuery() => 
        $"{DisplayType} == {BinaryFilterOperator}";

    private string SetTargetFilterQuery()
    {
        switch (FilterType)
        {
            case FilterType.UserDefinedTarget:
                DisplayType = "TargetGUID";
                return $"{DisplayType} {ConvertFilterOperators()} \"{TargetData.UserDefinedTargetGUID}\"";
            case FilterType.TracerouteTarget:
                DisplayType = "TracerouteGUID";
                return $"{DisplayType} {ConvertFilterOperators()} \"{TargetData.TracerouteTargetGUID}\"";
            default:
                return "error";
        }
    }

    private string SetDateTimeFilterQuery() =>
        $"{DisplayType} {ConvertFilterOperators()} {Date} {Time}";

    private string SetLatencyFilterQuery() =>
        $"CAST({DisplayType} as INTEGER) {ConvertFilterOperators()} \"{FilterValue}\"";
    
    private string ConvertFilterOperators() =>
        FilterOperator switch
        {
            FilterOperator.EqualTo => "==",
            FilterOperator.NotEqualTo => "!=",
            FilterOperator.GreaterThan => ">",
            FilterOperator.GreaterThanOrEqualTo => ">=",
            FilterOperator.LessThan => "<",
            FilterOperator.LessThanOrEqualTo => "<=",
            _ => string.Empty
        };
}