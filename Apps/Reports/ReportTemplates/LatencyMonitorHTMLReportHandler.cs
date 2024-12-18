﻿using System.IO;
using System.Text;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    internal class LatencyMonitorHTMLReportHandler
    {
        private string ReportNumber { get; set; }
        private string LogFilePath { get; set; }

        public LatencyMonitorHTMLReportHandler(string selectedReportID)
        {
            ReportNumber = selectedReportID;
            LogFilePath = $"{ReportDirectory}{selectedReportID}.html";
        }

        // Generate a HTML Report using the data in the LatencyMonitorReports table
        public async Task GenerateLatencyMonitorHTMLReportAsync()
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetLatencyMonitorReportAsync(ReportNumber)).First();

            await ConfirmReportDirectoryExistsAsync();

            var sb = new StringBuilder();
            var sw = new StreamWriter(LogFilePath);

            // Start generating report
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>Latency Monitor Report</title>");
            sb.AppendLine("<meta charset=\"UTF-8\"/>");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IT=edge\"/>");

            await GenerateReportStyleAsync(sb);

            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"header-bar\"></div>");
            sb.AppendLine("<div class=\"main-form-container\">");

            await GenerateReportHeaderAsync(sb);

            sb.AppendLine("<div class=\"session-data\">");

            if (report.ReportType == ReportType.Traceroute)
            {
                await GenerateTracerouteSummaryAsync(sb);
            }

            await GenerateTargetDataAsync(sb);

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

        private async Task GenerateReportHeaderAsync(StringBuilder builder)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetLatencyMonitorReportAsync(ReportNumber)).First();

            builder.AppendLine("<div class=\"main-title-container\">");
            builder.AppendLine("<p class=\"main-title\">Latency Monitor Report</p>");
            builder.AppendLine("</div>");

            builder.AppendLine("<div class=\"secondary-container\">");
            builder.AppendLine("<p class=\"secondary-title\">Session Statistics</p>");
            builder.AppendLine("<hr>");

            builder.AppendLine("<div class=\"session-data-container-minimal\" style=\"width: 1000px; grid-column: 1 / span 2;\">");
            builder.AppendLine("<table width=\"100%\">");
            builder.AppendLine("<tr><th>Report Number</th><th>Session Start Time</th><th>Session End Time</th><th>Session Duration</th><th>Total Packets Sent</th><th>Session Mode</th></tr>");
            builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>",
                report.ReportID, report.StartedWhen, report.CompletedWhen, report.TotalDuration, report.TotalPacketsSent, report.ReportType);
            builder.AppendLine("</table>");
            builder.AppendLine("</div>");
            builder.AppendLine("</div>");
        }

        private async Task GenerateTracerouteSummaryAsync(StringBuilder builder)
        {
            var dbHandler = new DatabaseHandler();
            var reportSnapshots = await dbHandler.GetLatencyMonitorReportSnapshotAsync(ReportNumber);

            reportSnapshots = reportSnapshots.OrderBy(a => a.Hop).ToList();

            builder.AppendLine("<p class=\"secondary-title\">Session Traceroute Summary</p>");
            builder.AppendLine("<hr>");

            builder.AppendLine("<div class=\"session-data-container-minimal\">");
            builder.AppendLine("<table style=\"width: 100%;\">");
            builder.AppendLine("<tr><th>Hop</th><th>Target Name</th><th>Lowest Latency</th><th>Highest Latency</th><th>Average Latency</th></tr>");

            foreach (var item in reportSnapshots)
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
        }

        private async Task GenerateTargetDataAsync(StringBuilder builder)
        {
            var dbHandler = new DatabaseHandler();
            var report = (await dbHandler.GetLatencyMonitorReportAsync(ReportNumber)).First();
            var reportSnapshots = await dbHandler.GetLatencyMonitorReportSnapshotAsync(ReportNumber);

            reportSnapshots = reportSnapshots.OrderBy(a => a.Hop).ToList();

            builder.AppendLine("<p class=\"secondary-title\">Session Target Data</p>");
            builder.AppendLine("<hr>");

            foreach (var target in reportSnapshots.Where(a =>
                                    a.Status != LatencyMonitorSessionStatus.NoResponse &&
                                    a.Status != LatencyMonitorSessionStatus.None))
            {
                var fontColor = "black";

                if (CalculatePercentagePacketsLost(target.TotalPacketsLost, report.TotalPacketsSent) > 0)
                {
                    fontColor = "red";
                }

                builder.AppendLine("<div class=\"session-data-container\">");
                builder.AppendLine("<div class=\"session-data-top\">");

                if (report.ReportType == ReportType.Traceroute)
                {
                    builder.AppendLine("<table style=\"width: 100%; text-align: left;\">");
                    builder.AppendLine("<tr>");
                    builder.AppendFormat("<th style=\"width: 5%; text-align: center;\">{0}</th>", target.Hop);
                    builder.AppendFormat("<th style=\"width: 95%; text-align: left; padding-left: 15px;\">{0}{1}</th>", target.TargetName, GetFormattedDNSName(target.DNSHostName));
                    builder.AppendLine("</tr>");
                    builder.AppendLine("</table>");
                }
                else
                {
                    builder.AppendLine("<table style=\"width: 100%; text-align: left;\">");
                    builder.AppendLine("<tr>");
                    builder.AppendFormat("<th style=\"width: 100%; text-align: left; padding-left: 15px;\">{0}</th>", target.TargetName);
                    builder.AppendLine("</tr>");
                    builder.AppendLine("</table>");
                }

                builder.AppendLine("</div>");

                builder.AppendLine("<div class=\"session-data-left\">");
                builder.AppendLine("<table style=\"width: 100%;\">");
                builder.AppendLine("<tr><th>Time of Last Event</th><th>Connection Status</th><th>Lowest Latency</th><th>Highest Latency</th><th>Average Latency</th></tr>");

                foreach (var item in await dbHandler.GetLatencyMonitorReportEntryAsync(ReportNumber, target.TargetName))
                {
                    builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                        item.TimeStamp,
                        item.Status,
                        item.LowestLatency,
                        item.HighestLatency,
                        item.AverageLatency);
                }

                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                        target.TimeStamp,
                        target.Status,
                        target.LowestLatency,
                        target.HighestLatency,
                        target.AverageLatency);

                builder.AppendLine("</table>");
                builder.AppendLine("</div>");

                builder.AppendLine("<div class=\"session-data-right\">");
                builder.AppendLine("<table style=\"width: 100%;\">");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Total Packets Lost</th>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr>");
                builder.AppendFormat("<td style=\"color: {0}\">{1}</td>", fontColor, target.TotalPacketsLost);
                builder.AppendLine("</tr>");
                builder.AppendLine("</table>");

                builder.AppendLine("<table style=\"width: 100%;\">");
                builder.AppendLine("<tr>");
                builder.AppendLine("<th>Percent Packets Lost</th>");
                builder.AppendLine("</tr>");
                builder.AppendLine("<tr>");
                builder.AppendFormat("<td style=\"color: {0}\">{1}</td>", fontColor, CalculatePercentagePacketsLost(target.TotalPacketsLost, report.TotalPacketsSent).ToString("P"));
                builder.AppendLine("</tr>");
                builder.AppendLine("</table>");
                builder.AppendLine("</div>");
                builder.AppendLine("</div>");
            }
        }

        private double CalculatePercentagePacketsLost(int packetsLost, int totalPacketsSent) =>
            packetsLost / (double)totalPacketsSent;

        private string GetFormattedDNSName(string dnsHostName)
        {
            string formattedName = string.Empty;

            if (!(dnsHostName == string.Empty))
            {
                formattedName = $" - ({dnsHostName})";
            }
            else
            {
                formattedName = $" - (N/A)";
            }

            return formattedName;
        }
    }
}
