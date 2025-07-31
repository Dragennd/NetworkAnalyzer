using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.Reports.Interfaces
{
    internal interface IDatabaseHandler
    {
        Task NewLatencyMonitorReportAsync(string reportID, string startTime);
        Task NewLatencyMonitorReportEntryAsync(List<LatencyMonitorData> data);
        Task<List<LatencyMonitorReports>> GetLatencyMonitorReportAsync(string selectedReportID);
        Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntryAsync(string selectedReportID, string targetName);
        Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesAsync(string selectedReportID);
        Task NewLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data);
        Task UpdateLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data);
        Task<List<LatencyMonitorTargetProfiles>> GetLatencyMonitorTargetProfilesAsync();
        Task DeleteSelectedProfilesAsync(LatencyMonitorTargetProfiles selectedProfile);
        Task NewIPScannerReportAsync();
        Task NewIPScannerReportEntryAsync(IPScannerData data);
        Task UpdateIPScannerReportsAsync();
        Task<List<IPScannerReports>> GetIPScannerReportAsync(string selectedReportID);
        Task<List<IPScannerReportEntries>> GetIPScannerReportEntriesAsync(string selectedReportID);
        Task<List<ReportExplorerData>> GetIPScannerReportsAsync();
        Task<List<ReportExplorerData>> GetLatencyMonitorReportsAsync();
        Task DeleteSelectedReportAsync(string selectedReportID, ReportType selectedReportType);
        Task DeleteAllReportDataAsync();
    }
}
