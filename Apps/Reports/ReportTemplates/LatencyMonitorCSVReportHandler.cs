using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    class LatencyMonitorCSVReportHandler
    {
        private string ReportNumber { get; set; }
        private string LogFilePath { get; set; }

        public LatencyMonitorCSVReportHandler(string selectedReportID)
        {
            ReportNumber = selectedReportID;
            LogFilePath = $"{ReportDirectory}{selectedReportID}.csv";
        }

        // Generate a CSV Report using the data in LatencyMonitorReports table
        public async Task GenerateLatencyMonitorCSVReportAsync()
        {
            await ConfirmReportDirectoryExistsAsync();

            var sb = new StringBuilder();
            var sw = new StreamWriter(LogFilePath);

            await GenerateReportHeaderAsync(sb);
            
            sb.AppendLine(",,,,,");

            await GenerateReportDataAsync(sb);

            sw.Write(sb);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        // Verify the data folder exists
        private async Task ConfirmReportDirectoryExistsAsync()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(ReportDirectory))
                {
                    Directory.CreateDirectory(ReportDirectory);
                }
            });
        }

        // Get the global info for the Latency Monitor report
        private async Task GenerateReportHeaderAsync(StringBuilder sb)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetLatencyMonitorReportAsync(ReportNumber)).First();

            sb.AppendLine("Report Number,Session Start Time,Session End Time,Session Duration,Total Packets Sent,Session Mode");
            sb.AppendLine($"{report.ReportID},{report.StartedWhen},{report.CompletedWhen},{report.TotalDuration},{report.TotalPacketsSent}");
        }

        // Get the individual entries for the Latency Monitor reports
        private async Task GenerateReportDataAsync(StringBuilder sb)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetLatencyMonitorReportAsync(ReportNumber)).First();

            sb.AppendLine("Timestamp,Target Name,Status,Lowest Latency,Highest Latency,Average Latency,Total Packets Lost");

            foreach (var target in await dbHandler.GetLatencyMonitorReportEntriesAsync(ReportNumber))
            {
                //sb.AppendLine($"{target.TimeStamp},{target.TargetName},{target.Status},{target.LowestLatency},{target.HighestLatency},{target.AverageLatency},{target.TotalPacketsLost}");
            }

            //foreach (var target in reportSnapshots.Where(a =>
            //                    a.Status != LatencyMonitorSessionStatus.NoResponse &&
            //                    a.Status != LatencyMonitorSessionStatus.None))
            //{
            //    sb.AppendLine($"{target.TimeStamp},{target.TargetName},{target.Status},{target.LowestLatency},{target.HighestLatency},{target.AverageLatency},{target.TotalPacketsLost}");
            //}
        }
    }
}
