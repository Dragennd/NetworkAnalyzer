using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Models;
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
        public async Task GenerateHTMLReport(string reportNumber, string mode)
        {
            string logFilePath = $"{DataDirectory}{reportNumber}.html";

            await ConfirmBWITFolderExists();

            StringBuilder sb = new();
            StreamWriter sw = new(logFilePath);

            // Start generating report
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>Latency Monitor Report</title>");
            sb.AppendLine("<meta charset=\"UTF-8\"/>");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IT=edge\"/>");

            await GenerateReportStyle(sb);

            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"header-bar\"></div>");
            sb.AppendLine("<div class=\"main-form-container\">");

            await GenerateReportHeader(sb, reportNumber, mode);

            sb.AppendLine("<div class=\"session-data\">");

            if (mode == "Traceroute")
            {
                await GenerateTracerouteSummary(sb);
            }

            await GenerateTargetData(sb, mode);

            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            sw.Write(sb);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        private async Task GenerateReportStyle(StringBuilder builder)
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
                builder.AppendLine(".session-data-container {background-color: #EBEBEB; box-shadow: 5px 5px 8px #D2D2D2; width: 1000px; margin-bottom: 30px; display: grid; grid-template-columns: 815px 180px; grid-template-rows: auto auto; row-gap: 3px; column-gap: 3px;}");
                builder.AppendLine(".session-data-container-minimal {background-color: #EBEBEB; box-shadow: 5px 5px 8px #D2D2D2; width: 650px; margin-left: auto; margin-right: auto; margin-bottom: 30px; display: grid; grid-column: 1 / span 2;}");
                builder.AppendLine(".session-data-top {grid-column: 1 / span 2;}");
                builder.AppendLine(".session-data-left {grid-column: 1;}");
                builder.AppendLine(".session-data-right {grid-column: 2;}");
                builder.AppendLine("table, th, tr, td {border: 4px solid #EBEBEB; text-align: center; padding: 3px 6px 3px 6px; border-collapse: collapse;}");
                builder.AppendLine("th {background-color: #BC8C43;}");
                builder.AppendLine("td {background-color: #D2D2D2;}");
                builder.AppendLine("hr {width: 99%; margin-left: auto; margin-right: auto; margin-bottom: 10px; background-color: #000000; border-color: #000000; border-width: 1.5px;}");
                builder.AppendLine("</style>");
            });
        }

        private async Task GenerateReportHeader(StringBuilder builder, string reportNumber, string mode)
        {
            await Task.Run(() =>
            {
                builder.AppendLine("<div class=\"main-title-container\">");
                builder.AppendLine("<p class=\"main-title\">Latency Monitor Report</p>");
                builder.AppendLine("</div>");

                builder.AppendLine("<div class=\"secondary-container\">");
                builder.AppendLine("<p class=\"secondary-title\">Session Statistics</p>");
                builder.AppendLine("<hr>");

                builder.AppendLine("<div class=\"session-data-container-minimal\" style=\"width: 1000px; grid-column: 1 / span 2;\">");
                builder.AppendLine("<table width=\"100%\">");
                builder.AppendLine("<tr><th>Report Number</th><th>Session Start Time</th><th>Session End Time</th><th>Session Duration</th><th>Total Packets Sent</th><th>Session Mode</th></tr>");
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>", reportNumber, StartTime, EndTime, TotalDuration, PacketsSent, mode);
                builder.AppendLine("</table>");
                builder.AppendLine("</div>");
                builder.AppendLine("</div>");
            });
        }

        private async Task GenerateTracerouteSummary(StringBuilder builder)
        {
            await Task.Run(() =>
            {
                List<LatencyMonitorData> tempData = new();

                foreach (var keypair in ReportSessionData)
                {
                    tempData.Add(keypair.Value.Last());
                }

                tempData = tempData.OrderBy(a => a.Hop).ToList();

                builder.AppendLine("<p class=\"secondary-title\">Session Traceroute Summary</p>");
                builder.AppendLine("<hr>");

                builder.AppendLine("<div class=\"session-data-container-minimal\">");
                builder.AppendLine("<table style=\"width: 100%;\">");
                builder.AppendLine("<tr><th>Hop</th><th>Target Name</th><th>Lowest Latency</th><th>Highest Latency</th><th>Average Latency</th></tr>");

                foreach (var item in tempData)
                {
                    builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                        item.Hop,
                        item.TargetName,
                        item.LowestLatency,
                        item.HighestLatency,
                        item.AverageLatency);
                }
                
                builder.AppendLine("</table>");
                builder.AppendLine("</div>");
            });
        }

        private async Task GenerateTargetData(StringBuilder builder, string mode)
        {
            await Task.Run(() =>
            {
                builder.AppendLine("<p class=\"secondary-title\">Session Target Data</p>");
                builder.AppendLine("<hr>");

                foreach (var target in IPAddresses)
                {
                    string fontColor = "black";

                    if (CalculatePercentagePacketsLost(ReportSessionData[target].Last().TotalPacketsLost) > 0)
                    {
                        fontColor = "red";
                    }

                    builder.AppendLine("<div class=\"session-data-container\">");
                    builder.AppendLine("<div class=\"session-data-top\">");

                    if (mode == "Traceroute")
                    {
                        builder.AppendLine("<table style=\"width: 100%; text-align: left;\">");
                        builder.AppendLine("<tr>");
                        builder.AppendFormat("<th style=\"width: 5%; text-align: center;\">{0}</th>", ReportSessionData[target].Last().Hop);
                        builder.AppendFormat("<th style=\"width: 95%; text-align: left; padding-left: 15px;\">{0}</th>", ReportSessionData[target].Last().TargetName);
                        builder.AppendLine("</tr>");
                        builder.AppendLine("</table>");
                    }
                    else
                    {
                        builder.AppendLine("<table style=\"width: 100%; text-align: left;\">");
                        builder.AppendLine("<tr>");
                        builder.AppendFormat("<th style=\"width: 100%; text-align: left; padding-left: 15px;\">{0}</th>", ReportSessionData[target].Last().TargetName);
                        builder.AppendLine("</tr>");
                        builder.AppendLine("</table>");
                    }
                    
                    builder.AppendLine("</div>");

                    builder.AppendLine("<div class=\"session-data-left\">");
                    builder.AppendLine("<table style=\"width: 100%;\">");
                    builder.AppendLine("<tr><th>Time of Last Event</th><th>Connection Status</th><th>Lowest Latency</th><th>Highest Latency</th><th>Average Latency</th></tr>");

                    foreach (var item in ReportSessionData[target])
                    {
                        builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                            item.TimeStamp,
                            item.Status,
                            item.LowestLatency,
                            item.HighestLatency,
                            item.AverageLatency);
                    }
                    
                    builder.AppendLine("</table>");
                    builder.AppendLine("</div>");

                    builder.AppendLine("<div class=\"session-data-right\">");
                    builder.AppendLine("<table style=\"width: 100%;\">");
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<th>Total Packets Lost</th>");
                    builder.AppendLine("</tr>");
                    builder.AppendLine("<tr>");
                    builder.AppendFormat("<td style=\"color: {0}\">{1}</td>", fontColor, ReportSessionData[target].Last().TotalPacketsLost);
                    builder.AppendLine("</tr>");
                    builder.AppendLine("</table>");

                    builder.AppendLine("<table style=\"width: 100%;\">");
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<th>Percent Packets Lost</th>");
                    builder.AppendLine("</tr>");
                    builder.AppendLine("<tr>");
                    builder.AppendFormat("<td style=\"color: {0}\">{1}</td>", fontColor, CalculatePercentagePacketsLost(ReportSessionData[target].Last().TotalPacketsLost).ToString("P"));
                    builder.AppendLine("</tr>");
                    builder.AppendLine("</table>");
                    builder.AppendLine("</div>");
                    builder.AppendLine("</div>");
                }
            });
        }

        private double CalculatePercentagePacketsLost(int packetsLost)
        {
            double p = (double)packetsLost / (double)PacketsSent;

            return p;
        }
    }
}
