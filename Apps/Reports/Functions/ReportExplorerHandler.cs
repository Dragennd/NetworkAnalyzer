using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal static class ReportExplorerHandler
    {
        public static async Task GenerateReportsListAsync()
        {
            var dbHandler = new DatabaseHandler();
            
            ReportsData.Clear();

            foreach (var report in await dbHandler.GetLatencyMonitorReportsAsync())
            {
                report.Mode = await SetReportMode(report.Type);
                ReportsData.Add(report);
            }

            foreach (var report in await dbHandler.GetIPScannerReportsAsync())
            {
                ReportsData.Add(report);
            }
        }

        private static async Task<ReportMode> SetReportMode(ReportType type)
        {
            ReportMode reportMode = ReportMode.None;

            switch (type)
            {
                case ReportType.UserTargets:
                    reportMode = ReportMode.LatencyMonitor;
                    break;
                case ReportType.Traceroute:
                    reportMode = ReportMode.LatencyMonitor;
                    break;
                case ReportType.ICMP:
                    reportMode = ReportMode.IPScanner;
                    break;
                default:
                    break;
            }

            return await Task.FromResult(reportMode);
        }
    }
}
