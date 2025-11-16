using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Models;
using System.IO;

namespace NetworkAnalyzer.Apps.Settings
{
    internal class LogHandler
    {
        private string? LogName { get; set; }

        private string LogPath { get; set; }

        private readonly GlobalSettings _settings = App.AppHost.Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

        public LogHandler()
        {
            LogName = GenerateLogName();
            LogPath = $@"{_settings.LogDirectory}\{LogName}";

            GenerateLogFile();
        }

        // Add an entry to the log file
        public async Task CreateLogEntry(string exceptionDetails, LogType logType, ReportType reportType = ReportType.None)
        {
            await File.AppendAllTextAsync(LogPath, $"{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Timestamp: {DateTime.Now:G}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Feature: {await ParseFeatureType(reportType)}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Log Type: {logType}{Environment.NewLine}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"Exception Details:{Environment.NewLine}{exceptionDetails}{Environment.NewLine}");
            await File.AppendAllTextAsync(LogPath, $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
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
                    fullFeatureType = $"Latency Monitor";
                    break;
                case ReportType.Traceroute:
                    fullFeatureType = $"Latency Monitor";
                    break;
                case ReportType.ICMP:
                    fullFeatureType = $"IP Scanner";
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
