using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.Reports.Interfaces
{
    internal interface IDatabaseHandler
    {
        string GetDatabaseSize();
        Task NewLatencyMonitorReportAsync(string reportID, string startTime);
        Task NewLatencyMonitorReportEntryAsync(List<LatencyMonitorData> data);
        Task<List<LatencyMonitorReports>> GetLatencyMonitorReportAsync(string selectedReportID);
        Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntryAsync(string selectedReportID, string targetGUID);
        Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesAsync(string selectedReportID);
        Task<List<LatencyMonitorReportEntries>> GetDistinctLatencyMonitorTracerouteTargetsAsync(string tracerouteGUID);
        Task<List<LatencyMonitorReportEntries>> GetDistinctLatencyMonitorUserDefinedTargetsAsync(string reportGUID);
        Task<List<LatencyMonitorReportEntries>> GetDistinctFinalLatencyMonitorTracerouteTargetsAsync(string tracerouteGUID);
        Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesForHistoryAsync(string filterQuery);
        Task UpdateLatencyMonitorFinalDataAsync(string reportGUID, string finalDuration, int totalPacketsSent);
        Task NewLatencyMonitorTargetProfileAsync(LatencyMonitorPreset data);
        Task UpdateLatencyMonitorTargetProfileAsync(LatencyMonitorPreset data);
        Task<List<LatencyMonitorTargetProfiles>> GetLatencyMonitorTargetProfilesAsync();
        Task DeleteSelectedProfileAsync(LatencyMonitorPreset selectedProfile);
        Task ResetLatencyMonitorReportTablesAsync();
        Task ResetLatencyMonitorPresetsTableAsync();
        Task NewIPScannerReportAsync(IPScannerReports report);
        Task NewIPScannerReportEntryAsync(IPScannerData data);
        Task UpdateIPScannerReportAsync(IPScannerReports report);
        Task<List<IPScannerReports>> GetIPScannerReportAsync(string selectedReportID);
        Task<List<IPScannerReportEntries>> GetIPScannerReportEntriesAsync(string selectedReportID);
        Task<List<ReportExplorerData>> GetIPScannerReportsAsync();
        Task<List<ReportExplorerData>> GetLatencyMonitorReportsAsync();
        Task ResetIPScannerReportTablesAsync();
        Task DeleteAllReportDataAsync();
    }
}
