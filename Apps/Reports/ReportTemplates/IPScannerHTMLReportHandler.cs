using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NetworkAnalyzer.Apps.Reports.ReportTemplates
{
    internal class IPScannerHTMLReportHandler
    {
        List<IPScannerReportEntries> reportData;

        List<IPScannerReports> report;

        private string ReportFilename { get; set; }

        private string ReportID { get; set; }

        private string SessionDuration { get; set; }

        private string SessionStartTime { get; set; }

        private int TotalActiveDevices { get; set; }

        private int TotalInactiveDevices { get; set; }

        private int TotalIPs { get; set; }

        private readonly IDatabaseHandler _dbHandler;

        private readonly GlobalSettings _settings;

        private StringBuilder SB { get; set; }

        private StreamWriter SW { get; set; }

        public IPScannerHTMLReportHandler(IDatabaseHandler dbHandler, GlobalSettings settings, string reportGUID)
        {
            _dbHandler = dbHandler;
            _settings = settings;

            ReportFilename = $@"{_settings.ReportDirectory}\IPS-{DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-")}.html";

            ReportID = reportGUID;

            reportData = new();
            SB = new();
        }

        public async Task<string> GenerateReport()
        {
            await GetIPScannerReportDataAsync();

            GenerateReportHeaderSection();

            GenerateReportBodySection();

            SW = new(ReportFilename);
            SW.Write(SB);
            SW.Flush();
            SW.Close();
            SW.Dispose();

            return await Task.FromResult(ReportFilename);
        }

        private async Task GetIPScannerReportDataAsync()
        {
            report = await _dbHandler.GetIPScannerReportAsync(ReportID);

            reportData = await _dbHandler.GetIPScannerReportEntriesAsync(ReportID);

            SessionDuration = report.First().TotalDuration;
            SessionStartTime = report.First().CreatedWhen;
            TotalActiveDevices = report.First().TotalActiveIPs;
            TotalInactiveDevices = report.First().TotalInactiveIPs;
            TotalIPs = report.First().TotalScannableIPs;
        }

        private void GenerateReportHeaderSection()
        {
            SB.AppendLine(
@"
<html>

<head>
    <title>IP Scanner Report</title>
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

        /* Start Session Data Wrapper - Includes Scan Summary */
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

        .session-scan-summary-container {
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

        .session-scan-summary-title {
            font-size: 24px;
            text-align: left;
            margin-left: 7px;
        }
        /* End Session Data Wrapper - Includes Scan Summary */

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

        private void GenerateReportBodySection()
        {
            SB.AppendLine(
@"
<body>
    <div class='report-header-bar'></div>
    <div class='main-form-container'>
        <div class='main-title-container'>
            <p class='main-title'>IP Scanner Report</p>
        </div>
        <div class='session-statistics-container'>
            <p class='session-statistics-title'>Scan Statistics</p>
            <hr>
            <div class='session-statistics-data-container'>
                <table width='100%'>
                    <tr>
                        <th>Report GUID</th>
                        <th>Start Time</th>
                        <th>Duration</th>
                        <th>Total IPs</th>
                        <th>Active Devices</th>
                        <th>Inactive Devices</th>
                    </tr>
");

            GenerateReportStatistics();

            SB.AppendLine(
@"
                </table>
            </div>
        </div>
        <div class='session-data-container'>
            <p class='session-scan-summary-title'>Scan Summary</p>
            <hr>
            <div class='session-scan-summary-container'>
                <table style='width: 100%;'>
                    <tr>
                        <th>IP Address</th>
                        <th>Name</th>
                        <th>MAC Address</th>
                        <th>Manufacturer</th>
                        <th>RDP Available</th>
                        <th>SMB Available</th>
                        <th>SSH Available</th>
                    </tr>
");

            GenerateReportSummary();

            SB.AppendLine(
@"
                </table>
            </div>
        </div>
    </div>
</body>

</html>
");
        }

        private void GenerateReportStatistics()
        {
            SB.AppendLine(
$@"
                    <tr>
                        <td>{ReportID}</td>
                        <td>{SessionStartTime}</td>
                        <td>{SessionDuration}</td>
                        <td>{TotalIPs}</td>
                        <td>{TotalActiveDevices}</td>
                        <td>{TotalInactiveDevices}</td>
                    </tr>
");
        }

        private void GenerateReportSummary()
        {
            foreach (var entry in reportData)
            {
                SB.AppendLine(
$@"
                    <tr>
                        <td>{entry.IPAddress}</td>
                        <td>{entry.Name}</td>
                        <td>{entry.MACAddress}</td>
                        <td>{entry.Manufacturer}</td>
                        <td>{entry.RDPEnabled}</td>
                        <td>{entry.SMBEnabled}</td>
                        <td>{entry.SSHEnabled}</td>
                    </tr>
");
            }
        }
    }
}
