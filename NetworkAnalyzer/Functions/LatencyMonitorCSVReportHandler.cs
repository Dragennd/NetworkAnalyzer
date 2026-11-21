using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class LatencyMonitorCSVReportHandler : ILatencyMonitorReportHandler
{
    private List<LatencyMonitorReportEntries> TracerouteData { get; set; } = new();
    private string ReportFilePath { get; }
    private string ReportFileName { get; }
    private string ReportGUID { get; }
    private string TracerouteGUID { get; }
    private string ManualStartTime { get; }
    private string ManualEndTime { get; }
    private bool IsDateRangeChecked { get; }
    private readonly DatabaseHandler _dbHandler;
    private StringBuilder SB { get; } = new();
    private StreamWriter SW { get; set; }

    public LatencyMonitorCSVReportHandler(DatabaseHandler dbHandler, string reportGUID, string tracerouteGUID, [Optional]bool isDateRangeChecked, [Optional]string manualStartTime, [Optional]string manualEndTime)
    {
        _dbHandler = dbHandler;
        ReportGUID = reportGUID;
        
        ReportFileName = $"LM-{DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-")}.csv";
        ReportFilePath = Path.Combine(GlobalSettings.ReportDirectory, ReportFileName);
        TracerouteGUID = tracerouteGUID;
        IsDateRangeChecked = isDateRangeChecked;
        ManualStartTime = manualStartTime;
        ManualEndTime = manualEndTime;
    }

    public async Task<string> GenerateReport()
    {
        await GetLatencyMonitorReportDataAsync();
        await GenerateReportDataAsync();

        SW = new(ReportFilePath);
        
        await SW.WriteAsync(SB);
        await SW.FlushAsync();
        SW.Close();
        await SW.DisposeAsync();

        return await Task.FromResult(ReportFileName);
    }

    private async Task GetLatencyMonitorReportDataAsync()
    {
        if (IsDateRangeChecked)
        {
            TracerouteData = await _dbHandler.GetLatencyMonitorReportEntriesAsync(ReportGUID, TracerouteGUID, ManualStartTime, ManualEndTime);
        }
        else
        {
            TracerouteData = await _dbHandler.GetLatencyMonitorReportEntriesAsync(ReportGUID, TracerouteGUID);
        }
    }

    private async Task GenerateReportDataAsync()
    {
        SB.AppendLine("Target Name, Target Address, Hop, Current Latency, Lowest Latency, Highest Latency, Average Latency, Jitter, Standard Deviation, Failed, Timestamp");
        
        foreach (var entry in TracerouteData)
        {
            SB.AppendLine($"{entry.TargetName},{entry.TargetAddress},{entry.Hop},{entry.CurrentLatency},{entry.LowestLatency},{entry.HighestLatency},{entry.AverageLatency},{entry.Jitter},{entry.StandardDeviation},{entry.FailedPing},{entry.TimeStamp}");
        }
    }
}