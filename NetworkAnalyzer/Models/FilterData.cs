using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Models;

internal class FilterData
{
    public FilterType FilterType { get; }
    public FilterOperator FilterOperator { get; }
    public BinaryFilterOperator BinaryFilterOperator { get; }
    public string FilterValue { get; }
    public string? DisplayValue { get; }
    public string DisplayType { get; }
    public string DisplayOperator { get; }
    public string FilterGUID { get; }
    public bool IsUseTracerouteTargetChecked { get; }
    public Expression FilterQuery { get; }
    public string? Date { get; }
    public string? Time { get; }
    public FilterTargetData TargetData { get; }
    public static ParameterExpression Parameter = Expression.Parameter(typeof(LatencyMonitorReportEntries), "a");
    private readonly LatencyMonitorController _latencyMonitorController = App.AppHost.Services.GetRequiredService<LatencyMonitorController>();

    // Contstructor for use with latency values
    public FilterData(FilterType filterType, FilterOperator filterOperator, string filterValue)
    {
        FilterType = filterType;
        FilterOperator = filterOperator;
        FilterValue = filterValue;
        DisplayValue = FilterValue;
        DisplayType = FilterType.ToString();
        FilterGUID = Guid.NewGuid().ToString();
        DisplayOperator = FilterOperator.ToString();
        FilterQuery = GenerateIntegerFilterQuery();
    }

    // Constructor for use with target values
    public FilterData(FilterType filterType, FilterOperator filterOperator, FilterTargetData targetData, bool isUseTracerouteTargetChecked)
    {
        FilterOperator = filterOperator;
        TargetData = targetData;
        IsUseTracerouteTargetChecked = isUseTracerouteTargetChecked;
        FilterGUID = Guid.NewGuid().ToString();
        DisplayOperator = FilterOperator.ToString();
        
        if (IsUseTracerouteTargetChecked)
        {
            FilterType = FilterType.TargetGUID;
            FilterValue = TargetData.TracerouteTargetGUID;
            DisplayType = "Traceroute Target";
            DisplayValue = TargetData.TracerouteTargetAddress;
        }
        else
        {
            FilterType = filterType;
            FilterValue = TargetData.UserDefinedTargetGUID;
            DisplayType = "User Defined Target";
            DisplayValue = TargetData.UserDefinedTargetAddress;
        }

        FilterQuery = GenerateStringFilterQuery();
    }

    // Constructor for use with failed ping values
    public FilterData(FilterType filterType, BinaryFilterOperator binaryFilterOperator)
    {
        FilterType = filterType;
        BinaryFilterOperator = binaryFilterOperator;
        DisplayType = FilterType.ToString();
        FilterGUID = Guid.NewGuid().ToString();
        DisplayValue = BinaryFilterOperator.ToString();
        FilterQuery = GenerateBoolFilterQuery();
    }

    // Constructor for use with DateTime values
    public FilterData(FilterType filterType, FilterOperator filterOperator, DateTimeOffset date, TimeSpan time)
    {
        FilterType = filterType;
        FilterOperator = filterOperator;
        DisplayType = "TimeStamp";
        Date = date.ToString("MM/dd/yyyy");
        Time = time.ToString();
        FilterValue = $"{Date} {Time}";
        DisplayValue = FilterValue;
        FilterGUID = Guid.NewGuid().ToString();
        DisplayOperator = FilterOperator.ToString();
        FilterQuery = GenerateDateTimeFilterQuery();
    }

    public void ClearFilter()
    {
        _latencyMonitorController.SendRemoveFilterRequest(FilterGUID);
    }

    private Expression GenerateIntegerFilterQuery()
    {
        MemberExpression property = Expression.Property(Parameter, FilterType.ToString());
        ConstantExpression value = Expression.Constant(int.Parse(FilterValue));
        MethodCallExpression type = Expression.Call(typeof(int), nameof(int.Parse), null, property);
        BinaryExpression query = Expression.NotEqual(Expression.Property(Parameter, FilterType.ToString()), Expression.Constant("-"));
        
        query = FilterOperator switch
        {
            FilterOperator.EqualTo => Expression.AndAlso(query, Expression.Equal(type, value)),
            FilterOperator.NotEqualTo => Expression.AndAlso(query, Expression.NotEqual(type, value)),
            FilterOperator.GreaterThan => Expression.AndAlso(query, Expression.GreaterThan(type, value)),
            FilterOperator.GreaterThanOrEqualTo => Expression.AndAlso(query, Expression.GreaterThanOrEqual(type, value)),
            FilterOperator.LessThan => Expression.AndAlso(query, Expression.LessThan(type, value)),
            FilterOperator.LessThanOrEqualTo => Expression.AndAlso(query, Expression.LessThanOrEqual(type, value)),
            _ => throw new ArgumentOutOfRangeException(nameof(FilterOperator), FilterOperator, null) // To-Do: Send a notification instead
        };

        return query;
    }

    private Expression GenerateDateTimeFilterQuery()
    {
        MemberExpression property = Expression.Property(Parameter, FilterType.ToString());
        ConstantExpression value = Expression.Constant(DateTime.Parse(FilterValue));
        MethodCallExpression type = Expression.Call(typeof(DateTime), nameof(DateTime.Parse), null, property);
            
        BinaryExpression query = FilterOperator switch
        {
            FilterOperator.EqualTo => Expression.Equal(type, value),
            FilterOperator.NotEqualTo => Expression.NotEqual(type, value),
            FilterOperator.GreaterThan => Expression.GreaterThan(type, value),
            FilterOperator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(type, value),
            FilterOperator.LessThan => Expression.LessThan(type, value),
            FilterOperator.LessThanOrEqualTo => Expression.LessThanOrEqual(type, value),
            _ => throw new ArgumentOutOfRangeException(nameof(FilterOperator), FilterOperator, null) // To-Do: Send a notification instead
        };

        return query;
    }

    private Expression GenerateStringFilterQuery()
    {
        MemberExpression type = Expression.Property(Parameter, FilterType.ToString());
        ConstantExpression value = Expression.Constant(FilterValue);
            
        BinaryExpression query = FilterOperator switch
        {
            FilterOperator.EqualTo => Expression.Equal(type, value),
            FilterOperator.NotEqualTo => Expression.NotEqual(type, value),
            _ => throw new ArgumentOutOfRangeException(nameof(FilterOperator), FilterOperator, null) // To-Do: Send a notification instead
        };

        return query;
    }

    private Expression GenerateBoolFilterQuery()
    {
        MemberExpression property = Expression.Property(Parameter, FilterType.ToString());
        ConstantExpression value = Expression.Constant(bool.Parse(FilterValue));
        MethodCallExpression type = Expression.Call(typeof(bool), nameof(bool.Parse), null, property);
            
        BinaryExpression query = FilterOperator switch
        {
            FilterOperator.EqualTo => Expression.Equal(type, value),
            FilterOperator.NotEqualTo => Expression.NotEqual(type, value),
            _ => throw new ArgumentOutOfRangeException(nameof(FilterOperator), FilterOperator, null) // To-Do: Send a notification instead
        };

        return query;
    }
}