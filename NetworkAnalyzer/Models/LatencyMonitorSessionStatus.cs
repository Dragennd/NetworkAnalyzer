namespace NetworkAnalyzer.Models;

public enum LatencyMonitorSessionStatus
{
    Idle,
    GeneratingTraceroutes,
    MonitoringTargets,
    Error
}