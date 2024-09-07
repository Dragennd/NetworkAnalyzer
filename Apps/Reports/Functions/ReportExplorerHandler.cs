using System.IO;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal static class ReportExplorerHandler
    {
        public static async Task GenerateReportsListAsync()
        {
            foreach (var file in Directory.GetFiles(ReportDirectory))
            {
                var fileName = file.Split("\\")[3];
                ReportsData.Add(await NewReportDataAsync(fileName));
            }
        }

        private static async Task<ReportExplorerData> NewReportDataAsync(string fileName)
        {
            var data = new ReportExplorerData();
            (string date, string mode) = await GetReportContentsAsync(fileName);

            data.ReportNumber = fileName;
            data.Date = date;

            switch (mode)
            {
                case "User Targets":
                    data.Mode = "Latency Monitor";
                    data.Type = "User Targets";
                    break;
                case "Traceroute":
                    data.Mode = "Latency Monitor";
                    data.Type = "Traceroute";
                    break;
                case "IP Scanner":
                    data.Mode = "IP Scanner";
                    data.Type = "None";
                    break;
                case "Network Survey":
                    data.Mode = "Network Survey";
                    data.Type = "None";
                    break;
                default:
                    data.Mode = "None";
                    data.Type = "None";
                    break;
            }

            return await Task.FromResult(data);
        }

        private static async Task<(string date, string mode)> GetReportContentsAsync(string fileName)
        {
            string filePath = $"{ReportDirectory}{fileName}";
            string reportContents = await File.ReadAllTextAsync(filePath);
            string date = string.Empty;
            string mode = string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(reportContents);

            var tableRow = doc.DocumentNode.SelectSingleNode("//tr[td]");

            if (tableRow != null)
            {
                date = tableRow.SelectSingleNode("td[3]").InnerText.ToString();
                mode = tableRow.SelectSingleNode("td[6]").InnerText.ToString();
            }

            return (date, mode);
        }
    }
}
