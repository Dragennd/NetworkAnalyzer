using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Apps.Models;
using System.IO;
using static NetworkAnalyzer.Apps.Settings.GlobalSettings;

namespace NetworkAnalyzer.Apps.Settings
{
    internal class LogHandler
    {
        private string? LogName { get; set; }

        private string LogPath { get; set; }

        private readonly GlobalSettings _globalSettings = App.AppHost.Services.GetRequiredService<GlobalSettings>();

        public LogHandler()
        {
            LogName = GenerateLogName();
            LogPath = $"{_globalSettings.LogDirectory}{LogName}";

            GenerateLogFile();
        }

        // Add an entry to the log file
        public async Task CreateLogEntry(string exceptionDetails, LogType logType, ReportType reportType = ReportType.None)
        {
            await File.AppendAllTextAsync(LogPath, $"----------------------------------- LOG ENTRY START -----------------------------------{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Timestamp: {DateTime.Now:G}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Feature: {await ParseFeatureType(reportType)}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Log Type: {logType}{Environment.NewLine}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Exception Details:{Environment.NewLine}{exceptionDetails}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"------------------------------------ LOG ENTRY END ------------------------------------{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
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

        private async Task<string> ParseFeatureType(ReportType reportType)
        {
            string fullFeatureType = string.Empty;

            switch (reportType)
            {
                case ReportType.UserTargets:
                    fullFeatureType = $"Latency Monitor [{reportType}]";
                    break;
                case ReportType.Traceroute:
                    fullFeatureType = $"Latency Monitor [{reportType}]";
                    break;
                case ReportType.ICMP:
                    fullFeatureType = $"IP Scanner [{reportType}]";
                    break;
                case ReportType.None:
                    fullFeatureType = "Report Explorer";
                    break;
            }

            return await Task.FromResult(fullFeatureType);
        }
        #endregion
    }
}
