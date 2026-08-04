using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class LatencyMonitorService
{
    #region Properties
    public event PropertyChangedEventHandler? PropertyChanged;
    public ConcurrentBag<LatencyMonitorData> AllTargets { get; set; }
    public List<string> TargetList { get; set; }
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
    private readonly ITracerouteFactory _tracerouteFactory;
    private readonly ILatencyMonitorController _latencyMonitorController;
    private readonly IDatabaseHandler _dbHandler;
    #endregion Properties

    public LatencyMonitorService(ITracerouteFactory tracerouteFactory, ILatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
    {
        _tracerouteFactory = tracerouteFactory;
        _latencyMonitorController = latencyMonitorController;
        _dbHandler = dbHandler;
        AllTargets = new();
        TargetList = new();
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

    public async Task GetHistoryData(ObservableCollection<FilterData> data, string reportID)
    {
        StringBuilder sb = new();

        sb.Append($"SELECT * FROM LatencyMonitorReportEntries WHERE ReportID == \"{reportID}\" AND DisplayName != \"Request timed out\"");

        if (data.Count > 0)
        {
            sb.Append(" AND ");

            foreach (var item in data)
            {
                sb.Append(item.FilterQuery);

                if (item != data.Last())
                {
                    sb.Append(" AND ");
                }
            }
        }

        _latencyMonitorController.SendHistoryDataRequest(await _dbHandler.GetLatencyMonitorReportEntriesForHistoryAsync(sb.ToString()));
    }
    #endregion Public Methods

    #region Private Methods
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

    private string FormatElapsedTime(TimeSpan elapsedTime) => 
        $"{elapsedTime.Days:00}.{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}";

    private void GenerateReportID() => 
        ReportID = Guid.NewGuid().ToString();

    private void SetStartTime() => 
        StartTime = DateTime.Now.ToString("G");
    
    protected virtual void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion Private Methods
}