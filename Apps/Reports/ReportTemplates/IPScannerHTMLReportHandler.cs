using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    internal class IPScannerHTMLReportHandler
    {
        private string ReportNumber { get; set; }
        private string LogFilePath { get; set; }

        public IPScannerHTMLReportHandler(string selectedReportID)
        {
            ReportNumber = selectedReportID;
            LogFilePath = $"{ReportDirectory}{selectedReportID}.html";
        }

        public async Task GenerateIPScannerHTMLReportAsync()
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetIPScannerReportAsync(ReportNumber)).First();

            await ConfirmReportDirectoryExistsAsync();

            var sb = new StringBuilder();
            var sw = new StreamWriter(LogFilePath);

            // Start generating report
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>IP Scanner Report</title>");
            sb.AppendLine("<meta charset=\"UTF-8\"/>");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IT=edge\"/>");

            await GenerateReportStyleAsync(sb);

            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"header-bar\"></div>");
            sb.AppendLine("<div class=\"main-form-container\">");

            await GenerateReportHeaderAsync(sb);

            sb.AppendLine("<div class=\"session-data\">");

            await GenerateIPScanSummaryAsync(sb);

            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

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

        private async Task GenerateReportStyleAsync(StringBuilder builder)
        {
            await Task.Run(() =>
            {
                builder.AppendLine("<style>");
                builder.AppendLine("* {margin: 0; padding: 0; font-size: 16px; color: #000000;}");
                builder.AppendLine("body {font-family: 'Segoe UI', 'Trebuchet MS', Tahoma, sans-serif; background-color: #F9FAFB;}");
                builder.AppendLine(".header-bar {width: 100%; height: 90px; background-color: #4373BC; left: 0px; top: 0px; position: absolute; z-index: 1;}");
                builder.AppendLine(".main-form-container {width: 1000px; margin-left: auto; margin-right: auto; margin-top: 10px; position: relative; z-index: 2;}");
                builder.AppendLine(".main-title-container {width: 1000px; height: 75px; left: 0px; top: 0px; position: absolute; z-index: 3;}");
                builder.AppendLine(".main-title {font-size: 50px; text-align: center; color: #F9FAFB;}");
                builder.AppendLine(".secondary-container {width: 1000; height: 120px; left: 0px; top: 95px; position: absolute; z-index: 3;}");
                builder.AppendLine(".secondary-title {font-size: 24px; text-align: left; margin-left: 7px;}");
                builder.AppendLine(".session-data {width: 1000px; left: 0px; top: 235px; display: flex; flex-direction: column; align-items: start; position: absolute; z-index: 3;}");
                builder.AppendLine(".session-data-container-minimal {background-color: #EBEBEB; box-shadow: 5px 5px 8px #D2D2D2; width: 650px; margin-left: auto; margin-right: auto; margin-bottom: 30px; display: grid; grid-column: 1 / span 2;}");
                builder.AppendLine("table, th, tr, td {border: 4px solid #EBEBEB; text-align: center; padding: 3px 6px 3px 6px; border-collapse: collapse;}");
                builder.AppendLine("th {background-color: #BC8C43;}");
                builder.AppendLine("td {background-color: #D2D2D2;}");
                builder.AppendLine("hr {width: 99%; margin-left: auto; margin-right: auto; margin-bottom: 10px; background-color: #000000; border-color: #000000; border-width: 1.5px;}");
                builder.AppendLine("</style>");
            });
        }

        private async Task GenerateReportHeaderAsync(StringBuilder builder)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetIPScannerReportAsync(ReportNumber)).First();

            builder.AppendLine("<div class=\"main-title-container\">");
            builder.AppendLine("<p class=\"main-title\">IP Scanner Report</p>");
            builder.AppendLine("</div>");

            builder.AppendLine("<div class=\"secondary-container\">");
            builder.AppendLine("<p class=\"secondary-title\">Session Statistics</p>");
            builder.AppendLine("<hr>");

            builder.AppendLine("<div class=\"session-data-container-minimal\" style=\"width: 1000px; grid-column: 1 / span 2;\">");
            builder.AppendLine("<table width=\"100%\">");
            builder.AppendLine("<tr><th>Report Number</th><th>Total Scannable IPs</th><th>Total Active IPs</th><th>Total Inactive IPs</th><th>Duration of Scan</th><th>Date of Scan</th></tr>");
            builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>",
                report.ReportID, report.TotalScannableIPs, report.TotalActiveIPs, report.TotalInactiveIPs, report.TotalDuration, report.CreatedWhen);
            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
            builder.AppendLine("</div>");
        }

        private async Task GenerateIPScanSummaryAsync(StringBuilder builder)
        {
            var dbHandler = new DatabaseHandler();
            var reportEntries = await dbHandler.GetIPScannerReportEntriesAsync(ReportNumber);

            builder.AppendLine("<p class=\"secondary-title\">Session Summary</p>");
            builder.AppendLine("<hr>");

            builder.AppendLine("<div class=\"session-data-container-minimal\" style=\"width: 1000px; grid-column: 1 / span 2;\">");
            builder.AppendLine("<table style=\"width: 100%;\">");
            builder.AppendLine("<tr><th>IP Address</th><th>Name</th><th>MAC Address</th><th>Manufacturer</th><th>RDP Available</th><th>SMB Available</th><th>SSH Available</th></tr>");

            foreach (var item in reportEntries)
            {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>",
                    item.IPAddress,
                    item.Name,
                    item.MACAddress,
                    item.Manufacturer,
                    item.RDPEnabled,
                    item.SMBEnabled,
                    item.SSHEnabled);
            }

            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
        }

        private int CalculateInactiveIPAddresses() => TotalSizeOfSubnetToScan -= TotalActiveIPAddresses;
    }
}
