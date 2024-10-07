using SQLite.Net2;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal class DatabaseHandler
    {
        private SQLiteConnection _db;
        private SemaphoreSlim _semaphore = new(1, 1);

        public DatabaseHandler()
        {
            SQLitePCL.Batteries.Init();
        }

        #region Latency Monitor Database Functions
        public async Task NewLatencyMonitorReportAsync()
        {
            LatencyMonitorReportID = GenerateReportNumberAsync();

            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new LatencyMonitorReports()
                    {
                        ReportID = LatencyMonitorReportID,
                        StartedWhen = StartTime,
                        CompletedWhen = StartTime,
                        TotalDuration = TotalDuration,
                        TotalPacketsSent = PacketsSent,
                        ReportType = LastLoggedType,
                        SuccessfullyCompleted = "false"
                    };

                    _db.Insert(report);
                }
            });

            _semaphore.Release();
        }

        public async Task NewLatencyMonitorReportSnapshotAsync(LatencyMonitorData data)
        {
            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new LatencyMonitorReportSnapshots()
                    {
                        ReportID = LatencyMonitorReportID,
                        TargetName = data.TargetName,
                        DNSHostName = data.DNSHostName,
                        Status = data.Status,
                        Hop = data.Hop,
                        FailedHopCounter = data.FailedHopCounter,
                        LowestLatency = data.LowestLatency,
                        HighestLatency = data.HighestLatency,
                        AverageLatency = data.AverageLatency,
                        TotalPacketsLost = data.TotalPacketsLost,
                        TimeStamp = data.TimeStamp.ToString(),
                        PacketsSent = PacketsSent,
                        Duration = TotalDuration
                    };

                    _db.Insert(report);
                }
            });

            _semaphore.Release();
        }

        public async Task NewLatencyMonitorReportEntryAsync(LatencyMonitorData data)
        {
            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new LatencyMonitorReportEntries()
                    {
                        ReportID = LatencyMonitorReportID,
                        TargetName = data.TargetName,
                        DNSHostName = data.DNSHostName,
                        Status = data.Status,
                        Hop = data.Hop,
                        LowestLatency = data.LowestLatency,
                        HighestLatency = data.HighestLatency,
                        AverageLatency = data.AverageLatency,
                        TotalPacketsLost = data.TotalPacketsLost,
                        TimeStamp = data.TimeStamp.ToString("MM/dd/yyyy HH:mm:ss")
                    };

                    _db.Insert(report);
                }
            });

            _semaphore.Release();
        }

        public async Task UpdateLatencyMonitorReportAsync()
        {
            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new LatencyMonitorReports()
                    {
                        ReportID = LatencyMonitorReportID,
                        StartedWhen = StartTime,
                        ReportType = LastLoggedType,
                        SuccessfullyCompleted = "true"
                    };

                    _db.Update(report);
                }
            });

            _semaphore.Release();
        }

        public async Task UpdateLatencyMonitorReportSnapshotAsync(LatencyMonitorData data, string duration)
        {
            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new LatencyMonitorReportSnapshots()
                    {
                        ReportID = LatencyMonitorReportID,
                        TargetName = data.TargetName,
                        DNSHostName = data.DNSHostName,
                        Status = data.Status,
                        Hop = data.Hop,
                        FailedHopCounter = data.FailedHopCounter,
                        LowestLatency = data.LowestLatency,
                        HighestLatency = data.HighestLatency,
                        AverageLatency = data.AverageLatency,
                        TotalPacketsLost = data.TotalPacketsLost,
                        TimeStamp = data.TimeStamp.ToString("MM/dd/yyyy HH:mm:ss"),
                        PacketsSent = PacketsSent,
                        Duration = duration
                    };

                    _db.Update(report);
                }
            });

            _semaphore.Release();
        }

        // Used in generating the list of reports to display in the Report Explorer
        public async Task<List<ReportExplorerData>> GetLatencyMonitorReportsAsync()
        {
            var queryResults = new List<ReportExplorerData>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                queryResults = _db.Query<ReportExplorerData>("SELECT ReportID as ReportNumber, CompletedWhen as Date, ReportType as Type FROM LatencyMonitorReports");
            }

            _semaphore.Release();

            return await Task.FromResult(queryResults);
        }

        // Used to get the report data from the LatencyMonitorReports table
        public async Task<List<LatencyMonitorReports>> GetLatencyMonitorReportAsync(string selectedReportID)
        {
            var query = new List<LatencyMonitorReports>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<LatencyMonitorReports>().Where(a => a.ReportID == selectedReportID).ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        // Used to get the individual entries for the various targets used in the Latency Monitor
        // for the various session types in the LatencyMonitorReportEntries table
        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntryAsync(string selectedReportID, string targetName)
        {
            var query = new List<LatencyMonitorReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportEntries>().Where(a => a.ReportID == selectedReportID && a.TargetName == targetName).ToList();
            }
            
            _semaphore.Release();

            return await Task.FromResult(query);
        }

        // Used to get the individual entries from the LatencyMonitorReportSnapshots table
        // to populate the Traceroute Overview table in the HTML report
        public async Task<List<LatencyMonitorReportSnapshots>> GetLatencyMonitorReportSnapshotAsync(string selectedReportID)
        {
            var query = new List<LatencyMonitorReportSnapshots>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportSnapshots>().Where(a => a.ReportID == selectedReportID).ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }
        #endregion

        #region IP Scanner Database Functions
        // Used to create a new entry in the IPScannerReports table
        public async Task NewIPScannerReportAsync()
        {
            IPScannerReportID = GenerateReportNumberAsync();

            await _semaphore.WaitAsync();

            await Task.Run(() =>
            {
                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var report = new IPScannerReports()
                    {
                        ReportID = IPScannerReportID,
                        TotalScannableIPs = TotalSizeOfSubnetToScan,
                        TotalActiveIPs = TotalActiveIPAddresses,
                        TotalInactiveIPs = CalculateInactiveIPAddressesAsync(),
                        TotalDuration = TotalScanDuration,
                        CreatedWhen = DateScanWasPerformed
                    };

                    _db.Insert(report);
                }
            });

            _semaphore.Release();
        }

        // Used to create a new entry in the IPScannerReportEntries table
        public async Task NewIPScannerReportEntryAsync()
        {

        }

        // Used in generating the list of reports to display in the Report Explorer
        public async Task<List<ReportExplorerData>> GetIPScannerReportsAsync()
        {
            var queryResults = new List<ReportExplorerData>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                queryResults = _db.Query<ReportExplorerData>("SELECT ReportID as ReportNumber, CreatedWhen as Date, ReportType as Type FROM IPScannerReports");
            }

            _semaphore.Release();

            return await Task.FromResult(queryResults);
        }

        // Used to pull the contents of the IPScannerReports table
        public async Task GetIPScannerReportAsync()
        {

        }

        // Used to get the individual entries for the various IP Addresses returned as successful
        // in the IP Scanner in the IPScannerReportEntries table
        public async Task GetIPScannerReportEntryAsync()
        {

        }
        #endregion

        private string GenerateReportNumberAsync() =>
            DateTime.Now.ToString("MMddyyyyHHmmss");

        private int CalculateInactiveIPAddressesAsync() =>
            TotalSizeOfSubnetToScan -= TotalActiveIPAddresses;
    }
}
