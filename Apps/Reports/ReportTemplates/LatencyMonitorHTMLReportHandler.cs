using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Markup;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    internal class LatencyMonitorHTMLReportHandler
    {
        private List<LatencyMonitorReportEntries> TracerouteData;
        
        private string ReportGUID { get; set; }

        private string TracerouteGUID { get; set; }

        private string SessionStartTime { get; set; }

        private string SessionEndTime { get; set; }

        private string ManualStartTime { get; set; }

        private string ManualEndTime { get; set; }

        private string TotalDuration { get; set; }

        private bool IsDateRangeChecked { get; set; }

        private bool IsTracerouteDataEmpty { get; set; } = false;

        private int TotalPacketsSent { get; set; }

        private int MaxJitter
        {
            get => _settings.DefaultMaxAllowableJitter;
        }

        private readonly IDatabaseHandler _dbHandler;

        private readonly GlobalSettings _settings;

        private StringBuilder SB { get; set; }

        private StreamWriter SW { get; set; }

        public LatencyMonitorHTMLReportHandler(IDatabaseHandler dbHandler, GlobalSettings settings, string reportGUID, string tracerouteGUID, [Optional]bool isDateRangeChecked, [Optional]string manualStartTime, [Optional]string manualEndTime)
        {
            _dbHandler = dbHandler;
            _settings = settings;

            ReportGUID = reportGUID;
            TracerouteGUID = tracerouteGUID;
            IsDateRangeChecked = isDateRangeChecked;
            ManualStartTime = manualStartTime;
            ManualEndTime = manualEndTime;

            SB = new();
            TracerouteData = new();
        }

        public async Task GenerateReport()
        {
            await GetLatencyMonitorReportDataAsync();

            GenerateReportHeaderSection();

            await GenerateReportBodySection();

            SW = new($@"{_settings.ReportDirectory}\LM-{SessionStartTime.Replace("/","-").Replace(":","-")}.html");
            SW.Write(SB);
            SW.Flush();
            SW.Close();
            SW.Dispose();
        }

        private async Task GetLatencyMonitorReportDataAsync()
        {
            var reportData = await _dbHandler.GetLatencyMonitorReportAsync(ReportGUID);

            if (IsDateRangeChecked)
            {
                TracerouteData = await _dbHandler.GetDistinctFinalLatencyMonitorTracerouteTargetsAsync(TracerouteGUID, ManualStartTime, ManualEndTime);
                
                if (TracerouteData.Count == 0)
                {
                    IsTracerouteDataEmpty = true;
                }
                else
                {
                    TotalPacketsSent = (await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, TracerouteData.First().TargetGUID, ManualStartTime, ManualEndTime)).Count;
                }
            }
            else
            {
                TracerouteData = await _dbHandler.GetDistinctFinalLatencyMonitorTracerouteTargetsAsync(TracerouteGUID);

                if (TracerouteData.Count == 0)
                {
                    IsTracerouteDataEmpty = true;
                }
                else
                {
                    TotalPacketsSent = reportData.First().TotalPacketsSent;
                }
            }

            SessionStartTime = reportData.First().StartedWhen;
            SessionEndTime = reportData.First().CompletedWhen;
            TotalDuration = reportData.First().TotalDuration;

            if (string.IsNullOrEmpty(ManualStartTime) || DateTime.Parse(ManualStartTime) <= DateTime.Parse(SessionStartTime))
            {
                ManualStartTime = SessionStartTime;
            }

            if (string.IsNullOrEmpty(ManualEndTime) || DateTime.Parse(ManualEndTime) >= DateTime.Parse(SessionEndTime))
            {
                ManualEndTime = SessionEndTime;
            }
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
    <script>
        document.addEventListener(""DOMContentLoaded"", () => {
            document.querySelectorAll('.inner-table-wrapper').forEach(wrapper => {
                wrapper.classList.add(""collapsed""); // collapse inner table on load
            });

            // set button text
            ['HighJitterbtn', 'Lostbtn'].forEach(btnId => {
                const btn = document.getElementById(btnId);
                if (btn) btn.textContent = ""+"";
            });
        });

        function toggleCollapseTable(wrapperId, buttonId) {
            const wrapper = document.getElementById(wrapperId);
            if (!wrapper) return;

            const isCollapsed = wrapper.classList.contains(""collapsed"");

            if (isCollapsed) {
                // EXPAND
                wrapper.classList.remove(""collapsed"");
                wrapper.style.height = wrapper.scrollHeight + ""px"";

                // remove height after transition
                setTimeout(() => wrapper.style.height = """", 410);

                document.getElementById(buttonId).textContent = ""-"";
            } else {
                // COLLAPSE
                wrapper.style.height = wrapper.scrollHeight + ""px""; // current height
                wrapper.offsetHeight; // force reflow
                wrapper.classList.add(""collapsed"");

                document.getElementById(buttonId).textContent = ""+"";
            }
        }
    </script>
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
            width: 1000px;
            margin-bottom: 30px;
            display: grid;
            grid-template-rows: auto auto;
            row-gap: 3px;
        }

        .session-target-data-top {
            margin-bottom: 5px;
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

        button {
            background-color: transparent;
            width: auto;
            font-size: 20px;
            border: 0;
            padding: 0;
            margin: 0;
        }

        .inner-table-wrapper {
            overflow: hidden;
            height: auto; /* natural height */
            transition: height 0.4s ease, opacity 0.4s ease;
            opacity: 1;
        }

        .inner-table-wrapper.collapsed {
            height: 0 !important;
            opacity: 0;
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

            if (!IsTracerouteDataEmpty)
            {
                await GenerateSessionTargetDataSection();
            }

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
                    </tr>
                    <tr>
                        <td>{ReportGUID}</td>
                        <td>{ManualStartTime}</td>
                        <td>{ManualEndTime}</td>
                        <td>{TotalPacketsSent}</td>
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

            if (!IsTracerouteDataEmpty)
            {
                GenerateTracerouteSummaryEntries();
            }

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
                await GenerateSessionTargetDataBottom(entry);
            }
        }

        private void GenerateSessionTargetDataTop(LatencyMonitorReportEntries data)
        {
            if (string.IsNullOrEmpty(data.TargetName) || string.IsNullOrWhiteSpace(data.TargetName))
            {
                data.TargetName = "(N/A)";
            }

            SB.AppendLine(
$@"
                <div class='session-target-data-top' style='margin-top: 10px;'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <th class='session-target-data-hop'>{data.Hop}</th>
                            <th class='session-target-data-name'>{data.TargetAddress} - {data.TargetName}</th>
                        </tr>
                    </table>
                </div>
");
        }

        private async Task GenerateSessionTargetDataBottom(LatencyMonitorReportEntries data)
        {
            List<LatencyMonitorReportEntries> packetsExceedingMaxJitter = new();
            List<LatencyMonitorReportEntries> packetsLost = new();
            List<LatencyMonitorReportEntries> reportData;
            int totalPacketsLost = 0;
            int totalPacketsExceedingMaxJitter = 0;
            double percentPacketsLost = totalPacketsLost / 100;

            if (IsDateRangeChecked)
            {
                reportData = await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, data.TargetGUID, ManualStartTime, ManualEndTime);
            }
            else
            {
                reportData = await _dbHandler.GetLatencyMonitorReportEntryAsync(ReportGUID, data.TargetGUID);
            }

                foreach (var entry in reportData)
                {
                    if (entry.FailedPing == false && int.Parse(entry.CurrentLatency) >= MaxJitter && (int.Parse(entry.CurrentLatency) >= (int.Parse(entry.AverageLatency) * 1.2)))
                    {
                        totalPacketsExceedingMaxJitter++;
                        packetsExceedingMaxJitter.Add(entry);
                    }

                    if (entry.FailedPing == true)
                    {
                        totalPacketsLost++;
                        packetsLost.Add(entry);
                    }
                }

            SB.AppendLine(
$@"
                <div>
                    <table style='width: 100%;' data-collapsed='true'>
                        <tr>
                            <th style='width: 35px; padding: 0px;'><button id='HighJitterbtn{data.Hop}' onclick=""toggleCollapseTable('HighJitterWrapper{data.Hop}', 'HighJitterbtn{data.Hop}')"">+</button></th>
                            <th style='text-align: left;'>Packets with High Jitter - Total Count: {totalPacketsExceedingMaxJitter}</th>
                        </tr>
                        <tr>
                            <td colspan='2'>
                                <div id='HighJitterWrapper{data.Hop}' class='inner-table-wrapper'>
                                    <table style='width: 100%;'>
                                        <tr>
                                            <th>ID</th>
                                            <th>Timestamp</th>
                                            <th>Latency</th>
                                            <th>Low</th>
                                            <th>High</th>
                                            <th>Average</th>
                                        </tr>
");

            foreach (var entry in packetsExceedingMaxJitter)
            {
                if (packetsExceedingMaxJitter.Count == 0)
                {
                    SB.AppendLine(
@"
                                        <tr>
                                            <td colspan='6'>No Packets Exceed Max Jitter</td>
                                        </tr>
");
                    break;
                }
                GenerateSessionTargetDataBottomHighJitterEntries(entry);
            }

            SB.AppendLine(
$@"
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
                <div>
                    <table style='width: 100%;' data-collapsed='true'>
                        <tr>
                            <th style='width: 35px; padding: 0px;'><button id='Lostbtn{data.Hop}' onclick=""toggleCollapseTable('LostWrapper{data.Hop}', 'Lostbtn{data.Hop}')"">+</button></th>
                            <th style='text-align: left;'>Packets Lost - Total Lost: {totalPacketsLost} - Percent Lost: {percentPacketsLost.ToString("F2")}%</th>
                        </tr>
                        <tr>
                            <td colspan='2'>
                                <div id='LostWrapper{data.Hop}' class='inner-table-wrapper'>
                                    <table style='width: 100%;'>
                                        <tr>
                                            <th>ID</th>
                                            <th>Timestamp</th>
                                            <th>Latency</th>
                                            <th>Low</th>
                                            <th>High</th>
                                            <th>Average</th>
                                        </tr>
");
            foreach (var entry in packetsLost)
            {
                if (packetsLost.Count == 0)
                {
                    SB.AppendLine(
@"
                                        <tr>
                                            <td colspan='6'>No Packets Lost</td>
                                        </tr>
");
                    break;
                }
                GenerateSessionTargetDataBottomLostPacketEntries(entry);
            }

            SB.AppendLine(
@"
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
");
        }

        private void GenerateSessionTargetDataBottomHighJitterEntries(LatencyMonitorReportEntries data)
        {

            SB.AppendLine(
$@"
                                        <tr>
                                            <td>{data.ID}</td>
                                            <td>{data.TimeStamp}</td>
                                            <td>{data.CurrentLatency}</td>
                                            <td>{data.LowestLatency}</td>
                                            <td>{data.HighestLatency}</td>
                                            <td>{data.AverageLatency}</td>
                                        </tr>
");

        }

        private void GenerateSessionTargetDataBottomLostPacketEntries(LatencyMonitorReportEntries data)
        {
            SB.AppendLine(
$@"
                                        <tr>
                                            <td>{data.ID}</td>
                                            <td>{data.TimeStamp}</td>
                                            <td>-</td>
                                            <td>{data.LowestLatency}</td>
                                            <td>{data.HighestLatency}</td>
                                            <td>{data.AverageLatency}</td>
                                        </tr>
");
        }
    }
}
