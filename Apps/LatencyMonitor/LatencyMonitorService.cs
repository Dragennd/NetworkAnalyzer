using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal class LatencyMonitorService : ILatencyMonitorService, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler? PropertyChanged;

        public ConcurrentBag<LatencyMonitorData> AllTargets { get; set; }

        public List<string> TargetList { get; set; }

        public LatencyMonitorData SelectedTarget { get; set; }

        public bool IsSessionActive { get; set; } = false;

        public int PacketsSent { get; set; }

        private readonly ITracerouteFactory _tracerouteFactory;

        private readonly ILatencyMonitorController _latencyMonitorController;
        #endregion Properties

        public LatencyMonitorService(ITracerouteFactory tracerouteFactory, ILatencyMonitorController latencyMonitorController)
        {
            _tracerouteFactory = tracerouteFactory;
            _latencyMonitorController = latencyMonitorController;
            AllTargets = new();
            TargetList = new();
        }

        #region Public Methods
        public async Task SetMonitoringSession(bool sessionStatus)
        {
            IsSessionActive = sessionStatus;

            await ExecuteInitialSessionAsync(TargetList);

            while (IsSessionActive)
            {
                var task = new List<Task>();
                Stopwatch sw = Stopwatch.StartNew();
                int allTargetsCount = AllTargets.Count;
                PacketsSent++;
                _latencyMonitorController.SendUpdatePacketsSentRequest(PacketsSent);

                foreach (var t in AllTargets)
                {
                    Func<Task> item = async () =>
                    {
                        if (t != null && t.TargetStatus == LatencyMonitorTargetStatus.Active)
                        {
                            LatencyMonitorData obj = await ExecuteSessionUpdateAsync(t);

                            t.Latency = obj.Latency;
                            t.LowestLatency = obj.LowestLatency;
                            t.HighestLatency = obj.HighestLatency;
                            t.AverageLatency = obj.AverageLatency;
                            t.TotalPacketsLost = obj.TotalPacketsLost;
                            t.TotalLatency = obj.TotalLatency;
                            t.AverageLatencyCounter = obj.AverageLatencyCounter;
                            t.FailedHopCounter = obj.FailedHopCounter;
                            t.TimeStamp = obj.TimeStamp;
                        }
                    };

                    task.Add(item());
                }

                await Task.WhenAll(task);

                foreach (var item in AllTargets)
                {
                    if (TargetList.Any(a => a == item.DisplayName))
                    {
                        _latencyMonitorController.SendUpdateLiveTargetRequest(item);
                    }

                    if (SelectedTarget != null && SelectedTarget.TracerouteGUID == item.TracerouteGUID)
                    {
                        _latencyMonitorController.SendUpdateTracerouteRequest(item);
                    }

                    // To-Do: Add the current item to a list to hold until all items have been processed for this round
                }

                // To-Do: Add database update method to add the batch of items

                if (sw.ElapsedMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)sw.ElapsedMilliseconds);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Public Methods

        #region Private Methods
        private async Task ExecuteInitialSessionAsync(List<string> targetList)
        {
            //List<LatencyMonitorData> TracerouteResults = new();

            foreach (var a in targetList)
            {
                var tr = _tracerouteFactory.Create(a);
                await tr.NewTracerouteDataAsync();
                //TracerouteResults.AddRange(await tr.NewTracerouteDataAsync());
            }
        }

        private async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
        {
            var u = new TargetWorker(data: data);

            return await u.UpdateTargetDataAsync();
        }
        #endregion Private Methods
    }
}
