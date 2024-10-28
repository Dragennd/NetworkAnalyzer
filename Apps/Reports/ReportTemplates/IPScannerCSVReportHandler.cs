using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    class IPScannerCSVReportHandler
    {
        private string ReportNumber { get; set; }
        private string LogFilePath { get; set; }

        public IPScannerCSVReportHandler(string selectedReportID)
        {
            ReportNumber = selectedReportID;
            LogFilePath = $"{ReportDirectory}{selectedReportID}.csv";
        }

        // Generate a CSV Report using the data in LatencyMonitorReports table
        public async Task GenerateIPScannerCSVReportAsync()
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

        // Get the global info for the IP Scanner report
        private async Task GenerateReportHeaderAsync(StringBuilder sb)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetIPScannerReportAsync(ReportNumber)).First();

            sb.AppendLine("Report Number,Total Scannable IPs,Total Active IPs,Total Inactive IPs,Duration of Scan,Date of Scan");
            sb.AppendLine($"{report.ReportID},{report.TotalScannableIPs},{report.TotalActiveIPs},{report.TotalInactiveIPs},{report.TotalDuration},{report.CreatedWhen}");
        }

        // Get the individual entries for the IP Scanner reports
        private async Task GenerateReportDataAsync(StringBuilder sb)
        {
            var dbHandler = new DatabaseHandler();
            var reportEntries = await dbHandler.GetIPScannerReportEntriesAsync(ReportNumber);

            sb.AppendLine("IP Address,Name,MAC Address,Manufacturer,RDP Enabled,SMB Enabled,SSH Enabled");

            foreach (var entry in reportEntries)
            {
                sb.AppendLine($"{entry.IPAddress},{entry.Name},{entry.MACAddress},{entry.Manufacturer},{entry.RDPEnabled},{entry.SMBEnabled},{entry.SSHEnabled}");
            }
        }
    }
}
