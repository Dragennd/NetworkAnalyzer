using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class LatencyMonitorService
{
    #region Properties
    public event PropertyChangedEventHandler? PropertyChanged;
    public ConcurrentBag<LatencyMonitorData> AllTargets { get; set; } = new();
    public ObservableCollection<LatencyMonitorReportEntries> AllData { get; set; } = new();
    public ObservableCollection<FilterData> ActiveFilters { get; set; } = new();
    public List<string> TargetList { get; set; } = new();
    public LatencyMonitorData SelectedTarget { get; set; }
    public bool IsSessionActive { get; set; } = false;
    public string ReportID
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(ReportID));
            }
        }
    } = "N/A";
    public string StartTime
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
    } = "N/A";
    public string SessionDuration
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(SessionDuration));
            }
        }
    } = "00:00.00";
    public int PacketsSent
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(PacketsSent));
            }
        }
    } = 0;

    private Expression<Func<LatencyMonitorReportEntries, bool>>? FilterQuery { get; set; }
    private bool IsLatencyMonitorInError { get; set; }
    private readonly ITracerouteFactory _tracerouteFactory;
    private readonly IDatabaseHandler _dbHandler;
    private readonly LatencyMonitorController _latencyMonitorController = App.AppHost.Services.GetRequiredService<LatencyMonitorController>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    #endregion Properties

    public LatencyMonitorService(ITracerouteFactory tracerouteFactory, IDatabaseHandler dbHandler)
    {
        _tracerouteFactory = tracerouteFactory;
        _dbHandler = dbHandler;

        _latencyMonitorController.SetSessionStatus += EndSessionIfInError;
        _latencyMonitorController.RemoveFilter += RemoveFilter;
    }

    #region Public Methods
    public async Task SetMonitoringSession()
    {
        SetStartTime();
        _ = SetSessionStopwatchAsync();
        GenerateReportID();
        await _dbHandler.NewLatencyMonitorReportAsync(ReportID, StartTime);

        _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.GeneratingTraceroutes);
            
        await ExecuteInitialSessionAsync(TargetList);

        if (IsLatencyMonitorInError)
        {
            IsSessionActive = false;
            return;    
        }
        

        _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.MonitoringTargets);
            
        while (IsSessionActive)
        {
            var task = new List<Task>();
            Stopwatch sw = Stopwatch.StartNew();
            PacketsSent++;
                
            foreach (var t in AllTargets)
            {
                Func<Task> item = async () =>
                {
                    if (t != null && t.TargetStatus == LatencyMonitorTargetStatus.Active)
                    {
                        LatencyMonitorData obj = await ExecuteSessionUpdateAsync(t);

                        t.Latency = obj.Latency;
                        t.LowestLatency = obj.LowestLatency;
                        t.HighestLatency = obj.HighestLatency;
                        t.AverageLatency = obj.AverageLatency;
                        t.TotalPacketsLost = obj.TotalPacketsLost;
                        t.TotalLatency = obj.TotalLatency;
                        t.AverageLatencyCounter = obj.AverageLatencyCounter;
                        t.FailedPing = obj.FailedPing;
                        t.TimeStamp = obj.TimeStamp;
                    }
                };

                task.Add(item());
            }

            await Task.WhenAll(task);

            var dataToAddToDB = new List<LatencyMonitorData>();

            foreach (var item in AllTargets)
            {
                if (TargetList.Any(a => a == item.DisplayName))
                {
                    _latencyMonitorController.SendUpdateLiveTargetRequest(item);
                }

                if (SelectedTarget != null && SelectedTarget.TracerouteGUID == item.TracerouteGUID)
                {
                    _latencyMonitorController.SendUpdateTracerouteRequest(item);
                }

                dataToAddToDB.Add(item);
            }

            await _dbHandler.NewLatencyMonitorReportEntryAsync(dataToAddToDB);
            
            sw.Stop();

            if (sw.ElapsedMilliseconds < 1000)
            {
                await Task.Delay(1000 - (int)sw.ElapsedMilliseconds);
            }
        }

        try
        {
            _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.EndingSession);
            await _dbHandler.UpdateLatencyMonitorFinalDataAsync(ReportID, SessionDuration, PacketsSent);
        }
        catch (InvalidOperationException)
        {
            // Do nothing, this appears to only occur when the session fails to run successfully
        }
    }

    public async Task GetDistinctHistoryTargetsAsync(string reportGUID)
    {
        foreach (var userDefinedTarget in await _dbHandler.GetDistinctLatencyMonitorUserDefinedTargetsAsync(reportGUID))
        {
            foreach (var tracerouteTarget in await _dbHandler.GetDistinctLatencyMonitorTracerouteTargetsAsync(userDefinedTarget.TracerouteGUID))
            {
                if (tracerouteTarget.TargetAddress == "Request timed out")
                    continue;
                
                _latencyMonitorController.SendSetHistoryDistinctTargetRequest(new FilterTargetData(
                    userDefinedTarget.TargetAddress, 
                    userDefinedTarget.TargetName, 
                    tracerouteTarget.TargetAddress, 
                    tracerouteTarget.TargetName,
                    tracerouteTarget.TracerouteGUID, 
                    tracerouteTarget.TargetGUID));
            }
        }
    }

    public async Task GetReportsAsync()
    {
        foreach (var item in await _dbHandler.GetLatencyMonitorHistoryReportsAsync())
        {
            _latencyMonitorController.SendSetHistoryReportRequest(item);
        }
    }

    public async Task GetReportEntriesAsync(string selectedReportGUID)
    {
        List<LatencyMonitorReportEntries> reportEntries = await _dbHandler.GetLatencyMonitorReportEntriesAsync(selectedReportGUID);

        _latencyMonitorController.SendHistoryDataRequest(reportEntries);
        
        AllData = new ObservableCollection<LatencyMonitorReportEntries>(reportEntries);
    }
    
    public async Task GenerateFilteredDataAsync()
    {
        FilterQuery = null;
        ProcessActiveFilters();
        
        List<LatencyMonitorReportEntries> reportEntries = await Task.Run(() => AllData.Where(FilterQuery!.Compile()).ToList());

        _latencyMonitorController.SendHistoryDataRequest(reportEntries);
    }
    #endregion Public Methods

    #region Private Methods
    private void ProcessActiveFilters()
    {
        Expression? query = null;
        
        foreach (var filter in ActiveFilters)
        {
            if (query == null)
            {
                query = filter.FilterQuery;
            }
            else
            {
                query = Expression.AndAlso(query, filter.FilterQuery);
            }
        }
        
        FilterQuery = Expression.Lambda<Func<LatencyMonitorReportEntries, bool>>(query ?? Expression.Constant(true), FilterData.Parameter);
    }
    
    private async Task ExecuteInitialSessionAsync(List<string> targetList)
    {
        var tasks = new List<Task>();

        foreach (var a in targetList)
        {
            var tr = _tracerouteFactory.Create(a, ReportID);
            tasks.Add(tr.NewTracerouteDataAsync());

            if (!IsSessionActive)
            {
                break;
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
    {
        var u = new TargetWorker(reportID: ReportID, data: data);

        return await u.UpdateTargetDataAsync();
    }
    
    private async Task SetSessionStopwatchAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();

        while (IsSessionActive)
        {
            SessionDuration = FormatElapsedTime(sw.Elapsed);
            
            await Task.Delay(1000);
        }
    }

    private void EndSessionIfInError(LatencyMonitorSessionStatus status)
    {
        if (status == LatencyMonitorSessionStatus.Error)
        {
            IsLatencyMonitorInError = true;
        }
    }

    private string FormatElapsedTime(TimeSpan elapsedTime) => 
        $"{elapsedTime.Days:00}.{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";

    private void GenerateReportID() => 
        ReportID = Guid.NewGuid().ToString();

    private void SetStartTime() => 
        StartTime = DateTime.Now.ToString("G");
    
    private void RemoveFilter(string guid)
    {
        ActiveFilters.Remove(ActiveFilters.First(a => a.FilterGUID == guid));
    }
    
    protected virtual void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion Private Methods
}