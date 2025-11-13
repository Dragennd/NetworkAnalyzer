using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using System.IO;
using System.Text;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    internal class LatencyMonitorHTMLReportHandler
    {
        private List<LatencyMonitorReportEntries> TracerouteData;
        
        private string ReportGUID { get; set; }

        private string TracerouteGUID { get; set; }

        private string SessionStartTime { get; set; }

        private string SessionEndTime { get; set; }

        private string TotalDuration { get; set; }

        private int TotalPacketsSent { get; set; }

        private readonly IDatabaseHandler _dbHandler;

        private readonly GlobalSettings _settings;

        private StringBuilder SB { get; set; }

        private StreamWriter SW { get; set; }

        public LatencyMonitorHTMLReportHandler(IDatabaseHandler dbHandler, GlobalSettings settings, string reportGUID, string tracerouteGUID)
        {
            _dbHandler = dbHandler;
            _settings = settings;

            ReportGUID = reportGUID;
            TracerouteGUID = tracerouteGUID;

            SB = new();
            TracerouteData = new();
        }

        public async Task GenerateReport()
        {
            await GetLatencyMonitorReportDataAsync();

            GenerateReportHeaderSection();

            await GenerateReportBodySection();

            SW = new($@"{_settings.ReportDirectory}\LM-{ReportGUID}.html");
            SW.Write(SB);
            SW.Flush();
            SW.Close();
            SW.Dispose();
        }

        private async Task GetLatencyMonitorReportDataAsync()
        {
            var reportData = await _dbHandler.GetLatencyMonitorReportAsync(ReportGUID);
            TracerouteData = await _dbHandler.GetDistinctFinalLatencyMonitorTracerouteTargetsAsync(TracerouteGUID);

            SessionStartTime = reportData.First().StartedWhen;
            SessionEndTime = reportData.First().CompletedWhen;
            TotalPacketsSent = reportData.First().TotalPacketsSent;
            TotalDuration = reportData.First().TotalDuration;
        }

        private void GenerateReportHeaderSection()
        {
            SB.AppendLine(
@"
<html>
<head>
    <title>Latency Monitor Report</title>
    <meta charset='UTF-8' />
    <meta http-equiv='X-UA-Compatible' content='IT=edge' />
    <style>
        * {
            margin: 0;
            padding: 0;
            font-size: 16px;
            color: #000000;
        }

        body {
            font-family: 'Segoe UI', 'Trebuchet MS', Tahoma, sans-serif;
            background-color: #F9FAFB;
        }

        /* Start Main Form Wrapper */
        .report-header-bar {
            width: 100%;
            height: 90px;
            background-color: #4373BC;
            left: 0px;
            top: 0px;
            position: absolute;
            z-index: 1;
        }

        .main-form-container {
            width: 1000px;
            margin-left: auto;
            margin-right: auto;
            margin-top: 10px;
            position: relative;
            z-index: 2;
        }

        .main-title-container {
            width: 1000px;
            height: 75px;
            left: 0px;
            top: 0px;
            position: absolute;
            z-index: 3;
        }

        .main-title {
            font-size: 50px;
            text-align: center;
            color: #F9FAFB;
        }
        /* End Main Form Wrapper */

        /* Start Statistics Wrapper */
        .session-statistics-container {
            width: 1000;
            height: 120px;
            left: 0px;
            top: 95px;
            position: absolute;
            z-index: 3;
        }

        .session-statistics-data-container {
            background-color: #EBEBEB;
            box-shadow: 5px 5px 8px #D2D2D2;
            width: 1000px;
            margin-left: auto;
            margin-right: auto;
            margin-bottom: 30px;
            display: grid;
            grid-column: 1 / span 2;
        }

        .session-statistics-title {
            font-size: 24px;
            text-align: left;
            margin-left: 7px;
        }
        /* End Statistics Wrapper */

        /* Start Session Data Wrapper - Includes Session Traceroute Summary and Session Target Data */
        .session-data-container {
            width: 1000px;
            left: 0px;
            top: 235px;
            display: flex;
            flex-direction: column;
            align-items: start;
            position: absolute;
            z-index: 3;
        }

        .session-traceroute-summary-container {
            background-color: #EBEBEB;
            box-shadow: 5px 5px 8px #D2D2D2;
            min-width: 700px;
            max-width: 1000px;
            margin-left: auto;
            margin-right: auto;
            margin-bottom: 30px;
            display: grid;
            grid-column: 1 / span 2;
        }

        .session-traceroute-summary-title {
            font-size: 24px;
            text-align: left;
            margin-left: 7px;
        }

        .session-target-data-title {
            font-size: 24px;
            text-align: left;
            margin-left: 7px;
        }

        .session-target-data-container {
            background-color: #EBEBEB;
            box-shadow: 5px 5px 8px #D2D2D2;
            min-width: 700px;
            max-width: 1000px;
            margin-bottom: 30px;
            display: grid;
            grid-template-columns: 700px 250px;
            grid-template-rows: auto auto;
            row-gap: 3px;
            column-gap: 50px;
        }

        .session-target-data-top {
            grid-column: 1 / span 2;
            margin-bottom: 5px;
        }

        .session-target-data-left {
            grid-column: 1;
        }

        .session-target-data-right {
            grid-column: 2;
        }

        .session-target-data-hop {
            width: 5%;
            text-align: center;
        }

        .session-target-data-name {
            width: 95%;
            text-align: left;
            padding-left: 15px;
        }
        /* End Session Data Wrapper - Includes Session Traceroute Summary and Session Target Data */

        /* Start Object-specific */
        table,th,tr,td {
            border: 4px solid #EBEBEB;
            text-align: center;
            padding: 3px 6px 3px 6px;
            border-collapse: collapse;
        }

        th {
            background-color: #BC8C43;
        }

        td {
            background-color: #D2D2D2;
        }

        hr {
            width: 99%;
            margin-left: auto;
            margin-right: auto;
            margin-bottom: 10px;
            background-color: #000000;
            border-color: #000000;
            border-width: 1.5px;
        }
        /* End Object-specific */
    </style>
</head>
");
        }

        private async Task GenerateReportBodySection()
        {
            SB.AppendLine(
@"
<body>
    <div class='report-header-bar'></div>
    <div class='main-form-container'>
        <div class='main-title-container'>
            <p class='main-title'>Latency Monitor Report</p>
        </div>
");

            GenerateSessionStatisticsSection();

            SB.AppendLine(
@"
        <div class='session-data-container'>
");

            GenerateSessionTracerouteSummarySection();

            SB.AppendLine(
@"
            <p class='session-target-data-title'>Session Target Data</p>
            <hr>
            <div class='session-target-data-container'>
");

            await GenerateSessionTargetDataSection();

            SB.AppendLine(
@"
            </div>
        </div>
    </div>
</body>
</html>
");
        }

        private void GenerateSessionStatisticsSection()
        {
            SB.AppendLine(
$@"
        <div class='session-statistics-container'>
            <p class='session-statistics-title'>Session Statistics</p>
            <hr>
            <div class='session-statistics-data-container'>
                <table width='100%'>
                    <tr>
                        <th>Report GUID</th>
                        <th>Session Start Time</th>
                        <th>Session End Time</th>
                        <th>Total Packets Sent</th>
                        <th>Total Duration</th>
                    </tr>
                    <tr>
                        <td>{ReportGUID}</td>
                        <td>{SessionStartTime}</td>
                        <td>{SessionEndTime}</td>
                        <td>{TotalPacketsSent}</td>
                        <td>{TotalDuration}</td>
                    </tr>
                </table>
            </div>
        </div>
");
        }

        private void GenerateSessionTracerouteSummarySection()
        {
            SB.AppendLine(
@"
            <p class='session-traceroute-summary-title'>Session Traceroute Summary</p>
            <hr>
            <div class='session-traceroute-summary-container'>
                <table style='width: 100%;'>
                    <tr>
                        <th>Hop</th>
                        <th>Target Address</th>
                        <th>Target Name</th>
                        <th>Lowest Latency</th>
                        <th>Highest Latency</th>
                        <th>Average Latency</th>
                        <th>Total Packets Lost</th>
                    </tr>
");

            GenerateTracerouteSummaryEntries();

            SB.AppendLine(
@"
                </table>
            </div>
");
        }

        private void GenerateTracerouteSummaryEntries()
        {
            foreach (var entry in TracerouteData)
            {
                SB.AppendLine(
$@"
                    <tr>
                        <td>{entry.Hop}</td>
                        <td>{entry.TargetAddress}</td>
                        <td>{entry.TargetName}</td>
                        <td>{entry.LowestLatency}</td>
                        <td>{entry.HighestLatency}</td>
                        <td>{entry.AverageLatency}</td>
                        <td>{entry.TotalPacketsLost}</td>
                    </tr>
");
            }
        }

        private async Task GenerateSessionTargetDataSection()
        {
            foreach (var entry in TracerouteData)
            {
                GenerateSessionTargetDataTop(entry);
                await GenerateSessionTargetDataLeft(entry);
                await GenerateSessionTargetDataRight(entry);
            }
        }

        private void GenerateSessionTargetDataTop(LatencyMonitorReportEntries data)
        {
            SB.AppendLine(
$@"
                <div class='session-target-data-top'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <th class='session-target-data-hop'>{data.Hop}</th>
                            <th class='session-target-data-name'>{data.TargetAddress} - {data.TargetName}</th>
                        </tr>
                    </table>
                </div>
");
        }

        private async Task GenerateSessionTargetDataLeft(LatencyMonitorReportEntries data)
        {
            SB.AppendLine(
$@"
                <div class='session-target-data-left'>
                    <table style='width: 100%;'>
                        <tr>
                            <th>Packets Sent with High Jitter</th>
                        </tr>
                    </table>
                    <table style='width: 100%;'>
                        <tr>
                            <th>Timestamp</th>
                            <th>Latency</th>
                            <th>Low</th>
                            <th>High</th>
                            <th>Average</th>
                        </tr>
");

            await GenerateSessionTargetDataLeftEntries(data);

            SB.AppendLine(
@"
                    </table>
                </div>
");
        }

        private async Task GenerateSessionTargetDataLeftEntries(LatencyMonitorReportEntries data)
        {
            foreach (var entry in await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, data.TargetGUID))
            {
                if (entry.CurrentLatency != "-" && (int.Parse(entry.CurrentLatency) >= (int.Parse(entry.AverageLatency) * 1.2)))
                {
                    SB.AppendLine(
$@"
                        <tr>
                            <td>{entry.TimeStamp}</td>
                            <td>{entry.CurrentLatency}</td>
                            <td>{entry.LowestLatency}</td>
                            <td>{entry.HighestLatency}</td>
                            <td>{entry.AverageLatency}</td>
                        </tr>
");
                }
            }
        }

        private async Task GenerateSessionTargetDataRight(LatencyMonitorReportEntries data)
        {
            var reportData = await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, data.TargetGUID);
            int totalPacketsLost = 0;
            double percentPacketsLost = totalPacketsLost / 100;

            for (int i = 0; i < reportData.Count; i++)
            {
                if (reportData[i].FailedPing == true)
                {
                    totalPacketsLost++;
                }
            }

            percentPacketsLost = Math.Round(percentPacketsLost, 2);

            SB.AppendLine(
$@"
                <div class='session-target-data-right'>
                    <table style='width: 100%;'>
                        <tr>
                            <th width='50%'>Total Packets Lost</th>
                            <th width='50%'>Percentage Packets Lost</th>
                        </tr>
                        <tr>
                            <td>{totalPacketsLost}</td>
                            <td>{percentPacketsLost}%</td>
                        </tr>
                    </table>
                </div>
                <div class='session-target-data-right'>
                    <table style='width: 100%;'>
                        <tr>
                            <th>Timestamps of Lost Packets</th>
                        </tr>
");

            await GenerateSessionTargetDataRightEntries(data);

            SB.AppendLine(
@"
                    </table>
                </div>
");
        }

        private async Task GenerateSessionTargetDataRightEntries(LatencyMonitorReportEntries data)
        {
            foreach (var entry in await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, data.TargetGUID))
            {
                if (entry.FailedPing == true)
                {
                    SB.AppendLine(
$@"
                        <tr>
                            <td>{entry.TimeStamp}</td>
                        </tr>
");
                }
            }
        }
    }
}
