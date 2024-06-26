﻿using System.IO;
using System.Text;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class HTMLReportHandler
    {
        // Verify the data folder exists
        private async Task ConfirmBWITFolderExists()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(DataDirectory))
                {
                    Directory.CreateDirectory(DataDirectory);
                }
            });
        }

        // Generate a HTML Report using the data in the ReportData dictionary
        public async Task GenerateHTMLReport(string reportNumber)
        {
            var dataSet = ReportSessionData;
            string logFilePath = $"{DataDirectory}{reportNumber}.html";

            await ConfirmBWITFolderExists();

            StringBuilder sb = new();
            StreamWriter sw = new(logFilePath);

            // Start of Page

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head><meta charset='UTF-8'/><meta http-equiv='X-UA-Compatible' content='IT=edge'/><style>");

            // Start of CSS

            // Global Defaults
            sb.AppendLine("* {margin: 0; padding: 0; font-size: 16px;}");

            // Body
            sb.AppendLine("body {font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #535353}");

            // Header
            sb.AppendLine("header {background-color: #128BC8; padding: 8px 30px; border: 10px; height: 30px; display: flex; justify-content: flex-end;}");
            sb.AppendLine(".title {position: absolute; top: 15px; left: 20px; height: 46px; width: 500px; background-color: #b6b6b6; box-shadow: 3px 5px 10px #3b3b3b;}");
            sb.AppendLine(".title h1 {font-size: 32px; text-align: center;}");

            // Page Content
            sb.AppendLine(".container {margin: 50px auto; padding: 0px 10px; font-size: 16px; width: 825px; display: grid; grid-template-columns: 1fr 3fr; gap: 30px;}");
            sb.AppendLine(".panel-header {border-top-left-radius: 6px; border-top-right-radius: 6px; background-color: #128BC8; text-align: left; font-size: 26px; padding: 0 0 0 8px;}");
            sb.AppendLine(".panel-left-content {padding: 5px 8px; text-align: center;}");
            sb.AppendLine(".panel-right-content {padding: 5px 10px 5px 10px; text-align: center; display: grid; grid-template-columns: 4fr 1fr;}");
            sb.AppendLine(".el {border-radius: 6px; background-color: #efefef; box-shadow: 3px 5px 10px #3b3b3b;}");
            sb.AppendLine(".side-panel {text-align: center; grid-row: span 5;}");
            sb.AppendLine(".side-panel h1 {text-align: center;}");
            sb.AppendLine("li {list-style: none; margin: 10px;}");
            sb.AppendLine("td {text-align: center; padding-left: 6px; padding-right: 6px;}");
            sb.AppendLine("th {text-align: center; padding-left: 2px; padding-right: 2px;}");
            sb.AppendFormat("</style><title>Latency Monitor Report - {0}</title></head>", reportNumber);
            // End of CSS

            // Start of Body
            sb.AppendLine("<body><header><div class='title'><h1>Latency Monitor Report</h1></div></header>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='el side-panel'>");

            // DIV to present statistics for the test
            sb.AppendLine("<h1 class='panel-header'>Statistics</h1><div class='panel-left-content'>");
            sb.AppendFormat("<p><strong>Report Number</strong></p><p>{0}</p><br>", reportNumber);
            sb.AppendFormat("<p><strong>Test Duration</strong></p><p>{0}</p><br>", TotalDuration.ToString());
            sb.AppendFormat("<p><strong>Packets Sent</strong></p><p>{0}</p><br>", PacketsSent);
            sb.AppendLine("</div></div>");
            // End of statistics DIV

            // DIV to be duplicated for each monitored target
            foreach (string ipAddress in IPAddresses)
            {
                sb.AppendLine("<div class='el panel'>");
                sb.AppendFormat("<h1 class='panel-header'>{0}</h1>", ipAddress);
                sb.AppendLine("<div class='panel-right-content'>");
                sb.AppendLine("<div><table>");
                sb.AppendLine("<tr><th>Timestamp of<br>Last Major Event</th><th>Connection<br>Status</th><th>Lowest<br>Latency</th><th>Highest<br>Latency</th><th>Average<br>Latency</th></tr>");

                foreach (var dataSetElement in dataSet[ipAddress])
                {
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td>", dataSetElement.TimeStamp, dataSetElement.Status);
                    sb.AppendFormat("<td>{0}</td><td>{1}</td>", dataSetElement.LowestLatency, dataSetElement.HighestLatency);
                    sb.AppendFormat("<td>{0}</td></tr>", dataSetElement.AverageLatency);
                }

                sb.AppendLine("</table></div>");
                sb.AppendFormat("<div><p><strong>Total Packets Lost</strong></p><p>{0}</p></div>", dataSet[ipAddress].LastOrDefault().TotalPacketsLost);
                sb.AppendLine("</div></div>");
            }
            // End of target DIV

            sb.AppendLine("</div></body></html>");
            // End of Page

            sw.Write(sb);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
    }
}
