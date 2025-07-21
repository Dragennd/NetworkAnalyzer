using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;

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

        private int _packetsSent;
        public int PacketsSent
        {
            get => _packetsSent;
            set
            {
                if (_packetsSent != value)
                {
                    _packetsSent = value;
                    OnPropertyChanged(nameof(PacketsSent));
                }
            }
        }

        private readonly ITracerouteFactory _tracerouteFactory;

        private readonly ILatencyMonitorController _latencyMonitorController;
        #endregion Properties

        public LatencyMonitorService(ITracerouteFactory tracerouteFactory, ILatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
        {
            _tracerouteFactory = tracerouteFactory;
            _latencyMonitorController = latencyMonitorController;
            AllTargets = new();
            TargetList = new();
        }

        #region Public Methods
        public async Task SetMonitoringSession()
        {
            await ExecuteInitialSessionAsync(TargetList);

            while (IsSessionActive)
            {
                var task = new List<Task>();
                Stopwatch sw = Stopwatch.StartNew();
                PacketsSent++;
                
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

                var dataToAddToDB = new List<LatencyMonitorData>();

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

                    dataToAddToDB.Add(item);
                }

                // To-Do: Add database update method to add the batch of items from the dataToAddToTheDB list
                // To-Do: Add a table to the database which contains the current database version to check if the current database file is outdated and/or incompatible

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
            var tasks = new List<Task>();

            foreach (var a in targetList)
            {
                var tr = _tracerouteFactory.Create(a);
                tasks.Add(tr.NewTracerouteDataAsync());

                if (!IsSessionActive)
                {
                    break;
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
        {
            var u = new TargetWorker(data: data);

            return await u.UpdateTargetDataAsync();
        }
        #endregion Private Methods
    }
}
