using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Interfaces
{
    internal interface ILatencyMonitorService : INotifyPropertyChanged
    {
        ConcurrentBag<LatencyMonitorData> AllTargets { get; set; }
        List<string> TargetList { get; set; }
        bool IsSessionActive { get; set; }
        int PacketsSent { get; set; }
        LatencyMonitorData SelectedTarget { get; set; }

        Task SetMonitoringSession();
    }
}
