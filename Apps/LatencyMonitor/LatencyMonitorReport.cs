using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public class LatencyMonitorReport
    {
        public static void ProcessLastMajorChangeAsync(string ipAddress, string responseCode)
        {
            var lastDataSet = ReportData[ipAddress].LastOrDefault();

            if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Down"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently down, was previously down and its been half an hour
                // Updating the dictionary if the internet is still down and its been half an hour
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Unstable"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently unstable, was previously unstable and its been half an hour
                // Updating the dictionary if the internet is still unstable and its been half an hour
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the connection was down but is slowly becoming better
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the connection was unstable and is now down completely
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet just went down and has been good
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet started being bad and has been good
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the internet was down but is now good
                WriteToReportDataAsync(ipAddress);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the internet was unstable but is now good
                WriteToReportDataAsync(ipAddress);
            }
        }

        public static void WriteToReportDataAsync(string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();

            ReportData[ipAddress]
            .Add(new LatencyMonitorData()
            {
                IPAddress = ipAddress,
                Status = lastDataSet.Status,
                Latency = lastDataSet.Latency,
                LowestLatency = lastDataSet.LowestLatency,
                HighestLatency = lastDataSet.HighestLatency,
                AverageLatency = lastDataSet.AverageLatency,
                PacketsLostTotal = lastDataSet.PacketsLostTotal,
                FailedPings = lastDataSet.FailedPings,
                ConnectionStatus = lastDataSet.ConnectionStatus,
                TimeStampOfLastMajorChange = DateTime.Now
            });
        }

        public static string CalculateTestDuration()
        {
            var startTime = ReportData.ElementAt(0).Value.FirstOrDefault().TimeStampOfLastMajorChange;
            var endTime = ReportData.ElementAt(0).Value.LastOrDefault().TimeStampOfLastMajorChange;
            TimeSpan duration = endTime.Subtract(startTime);

            return duration.ToString(@"dd\.hh\:mm\:ss");

        }

        public static string GenerateReportNumber() => string.Format("LM{0:MMddyyyy.HHmm}", DateTime.Now);

        public static void ProcessFinalEntryAsync(string ipAddress) => WriteToReportDataAsync(ipAddress);

        public static void ConfirmBWITFolderExists()
        {
            if (!Directory.Exists("C:\\BWIT\\"))
            {
                Directory.CreateDirectory("C:\\BWIT\\");
            }
        }

        public static void GenerateHTMLReport()
        {
            var dataSet = ReportData;
            string logFilePath = (@"C:\\BWIT\\") + GenerateReportNumber() + ".html";

            ConfirmBWITFolderExists();

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
            sb.AppendFormat("</style><title>MITS Latency Monitor Report - {0}</title></head>", GenerateReportNumber());
            // End of CSS

            // Start of Body
            sb.AppendLine("<body><header><div class='title'><h1>MITS Latency Monitor Report</h1></div></header>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<div class='el side-panel'>");

            // DIV to present statistics for the test
            sb.AppendLine("<h1 class='panel-header'>Statistics</h1><div class='panel-left-content'>");
            sb.AppendFormat("<p><strong>Report Number</strong></p><p>{0}</p><br>", GenerateReportNumber());
            sb.AppendFormat("<p><strong>Test Duration</strong></p><p>{0}</p><br>", CalculateTestDuration());
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
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td>", dataSetElement.TimeStampOfLastMajorChange, dataSetElement.ConnectionStatus);
                    sb.AppendFormat("<td>{0}</td><td>{1}</td>", dataSetElement.LowestLatency, dataSetElement.HighestLatency);
                    sb.AppendFormat("<td>{0}</td></tr>", dataSetElement.AverageLatency);
                }

                sb.AppendLine("</table></div>");
                sb.AppendFormat("<div><p><strong>Total Packets Lost</strong></p><p>{0}</p></div>", dataSet[ipAddress].LastOrDefault().PacketsLostTotal);
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
