namespace NetworkAnalyzer.Apps.Models
{
    internal enum LatencyMonitorSessionStatus
    {
        None = 0,
        Up = 1,
        Down = 2,
        Unstable = 3,
        NoResponse = 4, // Used for Traceroutes when the target responds to the traceroute but not the follow up pings
    }
}
