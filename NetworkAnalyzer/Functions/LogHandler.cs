using System;
using System.IO;
using System.Threading.Tasks;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class LogHandler
{
    private string? LogName { get; set; }
    private static string LogPath { get; set; }

    public LogHandler()
    {
        LogName = GenerateLogName();
        LogPath = Path.Combine(GlobalSettings.LogDirectory, LogName);

        GenerateLogFile();
    }

    // Add an entry to the log file
    public async Task CreateLogEntry(string data, LogType logType) =>
        await File.AppendAllTextAsync(LogPath, $"{DateTime.Now:G} {logType} {data} {Environment.NewLine}");
    
    // Specifically for Latency Monitor debugging
    public async Task CreateDebugLogEntry(LatencyMonitorData data)
    {
        await File.AppendAllTextAsync(LogPath, $"Target Timestamp Added: {data.TimeStamp}{Environment.NewLine}Target Address {data.TargetAddress}{Environment.NewLine}Target GUID: {data.TargetGUID}{Environment.NewLine}{Environment.NewLine}");
    }

    #region Private Methods
    // Generate log name based on the current month and day
    private string GenerateLogName() =>
        $"Log-{DateTime.Now:MMdd}.log";

    // Create the log file if it doesn't exist
    private void GenerateLogFile()
    {
        if (!File.Exists(LogPath))
        {
            File.Create(LogPath);
        }
    }
    #endregion
}