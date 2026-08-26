namespace NetworkAnalyzer.Models;

internal class LatencyMonitorReport
{
    public string ReportGUID { get; private set; }
    public string FriendlyName { get; private set; }
    public string DisplayName { get; private set; }
    public string TotalDuration { get; private set; }
    public string StartTime { get; private set; }

    public LatencyMonitorReport(string reportGUID, string friendlyName, string totalDuration, string startTime)
    {
        ReportGUID = reportGUID;
        DisplayName = reportGUID;
        FriendlyName = friendlyName;
        TotalDuration = totalDuration;
        StartTime = startTime;

        if (!string.IsNullOrWhiteSpace(friendlyName))
        {
            DisplayName = friendlyName;
        }
    }
}