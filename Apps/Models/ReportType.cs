namespace NetworkAnalyzer.Apps.Models
{
    public enum ReportType
    {
        None = 0,
        UserTargets = 1,
        Traceroute = 2,
        ICMP = 3
    }

    public enum ReportMode
    {
        None = 0,
        LatencyMonitor = 1,
        IPScanner = 2,
    }
}
