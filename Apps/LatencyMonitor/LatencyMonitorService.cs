using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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

        private string _reportID = "N/A";
        public string ReportID
        {
            get => _reportID;
            set
            {
                if (_reportID != value)
                {
                    _reportID = value;
                    OnPropertyChanged(nameof(ReportID));
                }
            }
        }

        private string _startTime = "N/A";
        public string StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        private string _sessionDuration = "N/A";
        public string SessionDuration
        {
            get => _sessionDuration;
            set
            {
                if (_sessionDuration != value)
                {
                    _sessionDuration = value;
                    OnPropertyChanged(nameof(SessionDuration));
                }
            }
        }

        private string _quickStartAddress = string.Empty;
        public string QuickStartAddress
        {
            get => _quickStartAddress;
            set
            {
                if (_quickStartAddress != value)
                {
                    _quickStartAddress = value;
                    OnPropertyChanged(nameof(QuickStartAddress));
                }
            }
        }

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

        private readonly IDatabaseHandler _dbHandler;
        #endregion Properties

        public LatencyMonitorService(ITracerouteFactory tracerouteFactory, ILatencyMonitorController latencyMonitorController, IDatabaseHandler dbHandler)
        {
            _tracerouteFactory = tracerouteFactory;
            _latencyMonitorController = latencyMonitorController;
            _dbHandler = dbHandler;
            AllTargets = new();
            TargetList = new();
        }

        #region Public Methods
        public async Task SetMonitoringSession()
        {
            SetStartTime();
            GenerateReportID();
            await _dbHandler.NewLatencyMonitorReportAsync(ReportID, StartTime);

            if (!string.IsNullOrEmpty(QuickStartAddress))
            {
                TargetList.Add(QuickStartAddress);
            }

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

                await _dbHandler.NewLatencyMonitorReportEntryAsync(dataToAddToDB);
                // To-Do: Add a table to the database which contains the current database version to check if the current database file is outdated and/or incompatible

                if (sw.ElapsedMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)sw.ElapsedMilliseconds);
                }
            }

            try
            {
                await _dbHandler.UpdateLatencyMonitorFinalDataAsync(ReportID, SessionDuration, PacketsSent);
            }
            catch (InvalidOperationException)
            {
                // Do nothing, this appears to only occur when the session fails to run successfully
            }
        }

        public async Task GetHistoryData(ObservableCollection<FilterData> data, string reportID)
        {
            StringBuilder sb = new();

            sb.Append($"SELECT * FROM LatencyMonitorReportEntries WHERE ReportID == \"{reportID}\"");

            if (data.Count > 0)
            {
                sb.Append(" AND ");

                foreach (var item in data)
                {
                    sb.Append(item.FilterQuery);

                    if (item != data.Last())
                    {
                        sb.Append(" AND ");
                    }
                }
            }

            _latencyMonitorController.SendHistoryDataRequest(await _dbHandler.GetLatencyMonitorReportEntriesForHistoryAsync(sb.ToString()));
        }
        #endregion Public Methods

        #region Private Methods
        private async Task ExecuteInitialSessionAsync(List<string> targetList)
        {
            var tasks = new List<Task>();

            foreach (var a in targetList)
            {
                var tr = _tracerouteFactory.Create(a, ReportID);
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
            var u = new TargetWorker(reportID: ReportID, data: data);

            return await u.UpdateTargetDataAsync();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GenerateReportID() => ReportID = Guid.NewGuid().ToString();

        private void SetStartTime() => StartTime = DateTime.Now.ToString("G");
        #endregion Private Methods
    }
}
