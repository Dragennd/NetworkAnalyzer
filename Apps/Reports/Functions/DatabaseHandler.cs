using SQLite.Net2;
using SQLitePCL;
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
            Batteries.Init();
        }

        #region Latency Monitor Database Functions
        // Used to create a new entry in the LatencyMonitorReports table
        public async Task NewLatencyMonitorReportAsync()
        {
            //LatencyMonitorReportID = GenerateReportNumber();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorReports()
                {
                    //ReportID = LatencyMonitorReportID,
                    //StartedWhen = StartTime,
                    //CompletedWhen = StartTime,
                    //TotalDuration = TotalDuration,
                    //TotalPacketsSent = PacketsSent,
                    //ReportType = LatencyMonitorReportType,
                    //SuccessfullyCompleted = "false"
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to create a new entry in the LatencyMonitorReportSnapshots table
        public async Task NewLatencyMonitorReportSnapshotAsync(LatencyMonitorData data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorReportSnapshots()
                {
                    //ReportID = LatencyMonitorReportID,
                    //TargetName = data.TargetName,
                    //DNSHostName = data.DNSHostName,
                    //Status = data.Status,
                    //Hop = data.Hop,
                    //FailedHopCounter = data.FailedHopCounter,
                    //LowestLatency = data.LowestLatency,
                    //HighestLatency = data.HighestLatency,
                    //AverageLatency = data.AverageLatency,
                    //TotalPacketsLost = data.TotalPacketsLost,
                    //TimeStamp = data.TimeStamp.ToString(),
                    //PacketsSent = PacketsSent,
                    //Duration = TotalDuration
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to create a new entry in the LatencyMonitorReportEntries table
        public async Task NewLatencyMonitorReportEntryAsync(LatencyMonitorData data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorReportEntries()
                {
                    //ReportID = LatencyMonitorReportID,
                    //TargetName = data.TargetName,
                    //DNSHostName = data.DNSHostName,
                    //Status = data.Status,
                    //Hop = data.Hop,
                    //LowestLatency = data.LowestLatency,
                    //HighestLatency = data.HighestLatency,
                    //AverageLatency = data.AverageLatency,
                    //TotalPacketsLost = data.TotalPacketsLost,
                    //TimeStamp = data.TimeStamp.ToString("MM/dd/yyyy HH:mm:ss")
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to update the entry in the LatencyMonitorReports table for the current session
        public async Task UpdateLatencyMonitorReportAsync()
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorReports()
                {
                    //ReportID = LatencyMonitorReportID,
                    //StartedWhen = StartTime,
                    //ReportType = LatencyMonitorReportType,
                    //SuccessfullyCompleted = "true"
                };

                _db.Update(report);
            }

            _semaphore.Release();
        }

        // Used to update the entries in the LatencyMonitorReportSnapshots table
        public async Task UpdateLatencyMonitorReportSnapshotAsync(LatencyMonitorData data, string duration)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                //var query = _db.Table<LatencyMonitorReportSnapshots>().Where(a => a.ReportID == LatencyMonitorReportID &&
                //                                                                  a.TargetName == data.TargetName &&
                //                                                                  a.Hop == data.Hop &&
                //                                                                  a.FailedHopCounter == data.FailedHopCounter)
                //                                                      .Select(a => a.ID);

                var report = new LatencyMonitorReportSnapshots()
                {
                    //ID = query.First(),
                    //ReportID = LatencyMonitorReportID,
                    //TargetName = data.TargetName,
                    //DNSHostName = data.DNSHostName,
                    //Status = data.Status,
                    //Hop = data.Hop,
                    //FailedHopCounter = data.FailedHopCounter,
                    //LowestLatency = data.LowestLatency,
                    //HighestLatency = data.HighestLatency,
                    //AverageLatency = data.AverageLatency,
                    //TotalPacketsLost = data.TotalPacketsLost,
                    //TimeStamp = data.TimeStamp.ToString("MM/dd/yyyy HH:mm:ss"),
                    //PacketsSent = PacketsSent,
                    //Duration = duration
                };

                _db.Update(report);
            }

            _semaphore.Release();
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

        // Used to get all of the entries from the LatencyMonitorReportEntries table
        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesAsync(string selectedReportID)
        {
            var query = new List<LatencyMonitorReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportEntries>().Where(a => a.ReportID == selectedReportID).ToList();
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

        // Used to create a new entry in the LatencyMonitorTargetProfiles table
        public async Task NewLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ProfileName = data.ProfileName,
                    ReportType = data.ReportType,
                    Hops = data.Hops,
                    Target1 = data.Target1,
                    Target2 = data.Target2,
                    Target3 = data.Target3,
                    Target4 = data.Target4,
                    Target5 = data.Target5
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to update an existing entry in the LatencyMonitorTargetProfiles table
        public async Task UpdateLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ID = data.ID,
                    ProfileName = data.ProfileName,
                    ReportType = data.ReportType,
                    Hops = data.Hops,
                    Target1 = data.Target1,
                    Target2 = data.Target2,
                    Target3 = data.Target3,
                    Target4 = data.Target4,
                    Target5 = data.Target5
                };

                _db.Update(report);
            }

            _semaphore.Release();
        }

        // Used to pull the list of target profiles which the user has created in the LatencyMonitorTargetProfiles table
        public async Task<List<LatencyMonitorTargetProfiles>> GetLatencyMonitorTargetProfilesAsync()
        {
            var query = new List<LatencyMonitorTargetProfiles>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<LatencyMonitorTargetProfiles>().ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        public async Task DeleteSelectedProfilesAsync(LatencyMonitorTargetProfiles selectedProfile)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var ids = new List<int>
                {
                    selectedProfile.ID
                };

                _db.DeleteIn<LatencyMonitorTargetProfiles>(ids);
            }

            _semaphore.Release();
        }
        #endregion

        #region IP Scanner Database Functions
        // Used to create a new entry in the IPScannerReports table
        public async Task NewIPScannerReportAsync()
        {
            IPScannerReportID = GenerateReportNumber();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new IPScannerReports()
                {
                    ReportID = IPScannerReportID,
                    TotalScannableIPs = TotalSizeOfSubnetToScan,
                    TotalActiveIPs = TotalActiveIPAddresses,
                    TotalInactiveIPs = TotalInactiveIPAddresses,
                    TotalDuration = TotalScanDuration,
                    CreatedWhen = DateScanWasPerformed,
                    ReportType = IPScannerReportType
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to create a new entry in the IPScannerReportEntries table
        public async Task NewIPScannerReportEntryAsync(IPScannerData data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new IPScannerReportEntries()
                {
                    ReportID = IPScannerReportID,
                    Name = data.Name,
                    IPAddress = data.IPAddress,
                    MACAddress = data.MACAddress,
                    Manufacturer = data.Manufacturer,
                    RDPEnabled = data.RDPEnabled,
                    SMBEnabled = data.SMBEnabled,
                    SSHEnabled = data.SSHEnabled
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        public async Task UpdateIPScannerReportsAsync()
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                var report = new IPScannerReports()
                {
                    ReportID = IPScannerReportID,
                    TotalScannableIPs = TotalSizeOfSubnetToScan,
                    TotalActiveIPs = TotalActiveIPAddresses,
                    TotalInactiveIPs = TotalInactiveIPAddresses,
                    TotalDuration = TotalScanDuration,
                    CreatedWhen = DateScanWasPerformed,
                    ReportType = IPScannerReportType
                };

                _db.Update(report);
            }

            _semaphore.Release();
        }

        // Used to pull the contents of the IPScannerReports table
        public async Task<List<IPScannerReports>> GetIPScannerReportAsync(string selectedReportID)
        {
            var query = new List<IPScannerReports>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<IPScannerReports>().Where(a => a.ReportID == selectedReportID).ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        // Used to get the individual entries for the various IP Addresses returned as successful
        // in the IP Scanner in the IPScannerReportEntries table
        public async Task<List<IPScannerReportEntries>> GetIPScannerReportEntriesAsync(string selectedReportID)
        {
            var query = new List<IPScannerReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                query = _db.Table<IPScannerReportEntries>().Where(a => a.ReportID == selectedReportID).ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }
        #endregion

        #region Report Explorer
        // Used in generating the list of IP Scanner reports to display in the Report Explorer
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

        // Used in generating the list of Latency Monitor reports to display in the Report Explorer
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

        public async Task DeleteSelectedReportAsync(string selectedReportID, ReportType selectedReportType)
        {
            if (selectedReportType == ReportType.UserTargets || selectedReportType == ReportType.Traceroute)
            {
                await _semaphore.WaitAsync();

                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var reportQuery = new LatencyMonitorReports() { ReportID = selectedReportID };
                    var reportEntryQuery = _db.Table<LatencyMonitorReportEntries>().Where(a => a.ReportID == selectedReportID).ToList();
                    var ids = new List<int>();

                    foreach (var entry in reportEntryQuery)
                    {
                        ids.Add(entry.ID);
                    }

                    _db.DeleteIn<LatencyMonitorReportEntries>(ids);
                    _db.Delete(reportQuery);
                }

                _semaphore.Release();
            }
            else
            {
                await _semaphore.WaitAsync();

                using (_db = new SQLiteConnection(DatabasePath))
                {
                    var reportQuery = new IPScannerReports() { ReportID = selectedReportID };
                    var reportEntryQuery = _db.Table<IPScannerReportEntries>().Where(a => a.ReportID == selectedReportID).ToList();
                    var ids = new List<int>();

                    foreach (var entry in reportEntryQuery)
                    {
                        ids.Add(entry.ID);
                    }

                    _db.DeleteIn<IPScannerReportEntries>(ids);
                    _db.Delete(reportQuery);
                }

                _semaphore.Release();
            }
        }

        public async Task DeleteAllReportDataAsync()
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(DatabasePath))
            {
                _db.DeleteAll<LatencyMonitorReportEntries>();
                _db.DeleteAll<LatencyMonitorReportSnapshots>();
                _db.DeleteAll<LatencyMonitorReports>();

                _db.DeleteAll<IPScannerReportEntries>();
                _db.DeleteAll<IPScannerReports>();
            }

            _semaphore.Release();
        }
        #endregion

        #region Private Methods
        private string GenerateReportNumber() =>
            DateTime.Now.ToString("MMddyyyyHHmmss");
        #endregion
    }
}
