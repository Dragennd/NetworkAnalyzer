using CommunityToolkit.Mvvm.ComponentModel;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.Utilities
{
    internal partial class LatencyMonitorDetailsWindowViewModel : ObservableValidator
    {
        [ObservableProperty]
        public string targetAddress;

        [ObservableProperty]
        public string targetName;

        [ObservableProperty]
        public string targetGUID;

        [ObservableProperty]
        public string currentLatency;

        [ObservableProperty]
        public string lowestLatency;

        [ObservableProperty]
        public string highestLatency;

        [ObservableProperty]
        public string averageLatency;

        [ObservableProperty]
        public bool isUserDefinedTarget;

        [ObservableProperty]
        public int hop;

        private readonly ILatencyMonitorController _latencyMonitorController;

        public LatencyMonitorDetailsWindowViewModel(ILatencyMonitorController latencyMonitorController)
        {
            _latencyMonitorController = latencyMonitorController;

            _latencyMonitorController.SetSelectedTargetGuid += SetSelectedTargetGUID;
            _latencyMonitorController.UpdateTracerouteData += SetSelectedTargetDetails;
            _latencyMonitorController.UpdateLiveTargetData += SetSelectedTargetDetails;
        }

        private void SetSelectedTargetDetails(LatencyMonitorData data)
        {
            if (TargetGUID == data.TargetGUID)
            {
                TargetAddress = data.TargetAddress;
                TargetName = data.TargetName;
                CurrentLatency = data.Latency;
                LowestLatency = data.LowestLatency;
                HighestLatency = data.HighestLatency;
                AverageLatency = data.AverageLatency;
                IsUserDefinedTarget = data.IsUserDefinedTarget;
                Hop = data.Hop;
            }
        }

        private void SetSelectedTargetGUID(string targetGUID)
        {
            TargetGUID = targetGUID;
        }
    }
}
