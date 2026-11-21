using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class IPScannerCSVReportHandler
{
    private List<IPScannerReportEntries> ReportData { get; set; } = new();
    private string ReportFilePath { get; }
    private string ReportFileName { get; }
    private string ReportID { get; }
    private readonly IDatabaseHandler _dbHandler;
    private StringBuilder SB { get; } = new();
    private StreamWriter SW { get; set; }

    public IPScannerCSVReportHandler(IDatabaseHandler dbHandler, string reportGUID)
    {
        _dbHandler = dbHandler;

        ReportFileName = $"IPS-{DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-")}.csv";
        ReportFilePath = Path.Combine(GlobalSettings.ReportDirectory, ReportFileName);
        ReportID = reportGUID;
    }

    public async Task<string> GenerateReport()
    {
        await GetIPScannerReportDataAsync();
        await GenerateReportDataAsync();

        SW = new(ReportFilePath);
        await SW.WriteAsync(SB);
        await SW.FlushAsync();
        SW.Close();
        await SW.DisposeAsync();

        return await Task.FromResult(ReportFileName);
    }

    private async Task GetIPScannerReportDataAsync()
    {
        ReportData = await _dbHandler.GetIPScannerReportEntriesAsync(ReportID);
    }
    
    private async Task GenerateReportDataAsync()
    {
        foreach (var entry in ReportData)
        {
            SB.AppendLine($"{entry.Name},{entry.IPAddress},{entry.MACAddress},{entry.Manufacturer},{entry.Latency},{entry.ActivePorts}");
        }
    }
}