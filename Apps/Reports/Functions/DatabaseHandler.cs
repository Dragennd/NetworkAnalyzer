using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using SQLite.Net2;
using SQLitePCL;
using System.IO;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal class DatabaseHandler : IDatabaseHandler
    {
        private SQLiteConnection _db;

        private SemaphoreSlim _semaphore = new(1, 1);

        private readonly GlobalSettings _settings = App.AppHost.Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

        public DatabaseHandler()
        {
            Batteries.Init();
        }

        #region Latency Monitor Database Functions
        // Used to create a new entry in the LatencyMonitorReports table
        public async Task NewLatencyMonitorReportAsync(string reportID, string startTime)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                var report = new LatencyMonitorReports()
                {
                    ReportID = reportID,
                    StartedWhen = startTime,
                    CompletedWhen = startTime,
                    TotalDuration = "00:00:00",
                    TotalPacketsSent = 0,
                    SuccessfullyCompleted = "false"
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to create a new entry in the LatencyMonitorReportEntries table
        public async Task NewLatencyMonitorReportEntryAsync(List<LatencyMonitorData> data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                foreach (var item in data)
                {
                    var report = new LatencyMonitorReportEntries()
                    {
                        ReportID = item.ReportID,
                        DisplayName = item.DisplayName,
                        TargetName = item.TargetName,
                        TargetAddress = item.TargetAddress,
                        TargetStatus = item.TargetStatus,
                        TargetGUID = item.TargetGUID,
                        TracerouteGUID = item.TracerouteGUID,
                        Hop = item.Hop,
                        FailedHopCounter = item.FailedHopCounter,
                        AverageLatencyCounter = item.AverageLatencyCounter,
                        CurrentLatency = item.Latency,
                        LowestLatency = item.LowestLatency.ToString(),
                        HighestLatency = item.HighestLatency,
                        AverageLatency = item.AverageLatency,
                        TotalPacketsLost = item.TotalPacketsLost,
                        TotalLatency = item.TotalLatency,
                        FailedPing = item.FailedPing,
                        IsUserDefinedTarget = item.IsUserDefinedTarget,
                        TimeStamp = item.TimeStamp.ToString("MM/dd/yyyy HH:mm:ss")
                    };

                    _db.Insert(report);
                }
            }

            _semaphore.Release();
        }

        // Used to get the report data from the LatencyMonitorReports table
        public async Task<List<LatencyMonitorReports>> GetLatencyMonitorReportAsync(string selectedReportID)
        {
            var query = new List<LatencyMonitorReports>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                query = _db.Table<LatencyMonitorReports>()
                           .Where(a => a.ReportID == selectedReportID)
                           .ToList();
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportEntries>()
                           .Where(a => a.ReportID == selectedReportID && a.TargetName == targetName)
                           .ToList();
            }
            
            _semaphore.Release();

            return await Task.FromResult(query);
        }

        // Used to get all of the entries from the LatencyMonitorReportEntries table
        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesAsync(string selectedReportID)
        {
            var query = new List<LatencyMonitorReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportEntries>()
                           .Where(a => a.ReportID == selectedReportID)
                           .ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesForHistoryAsync(string filterQuery)
        {
            var queryResults = new List<LatencyMonitorReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                queryResults = _db.Query<LatencyMonitorReportEntries>(filterQuery).ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(queryResults);
        }

        public async Task<List<LatencyMonitorReportEntries>> GetDistinctLatencyMonitorTracerouteTargetsAsync(string tracerouteGUID)
        {
            var query = new List<LatencyMonitorReportEntries>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                query = _db.Table<LatencyMonitorReportEntries>()
                           .Where(a => a.TracerouteGUID == tracerouteGUID)
                           .GroupBy(b => b.TargetAddress)
                           .Select(c => c.First())
                           .ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        // Used to create a new entry in the LatencyMonitorTargetProfiles table
        public async Task NewLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ProfileName = data.ProfileName,
                    TargetCollection = data.TargetCollection,
                    UUID = data.UUID
                };

                _db.Insert(report);
            }

            _semaphore.Release();
        }

        // Used to update an existing entry in the LatencyMonitorTargetProfiles table
        public async Task UpdateLatencyMonitorTargetProfile(LatencyMonitorTargetProfiles data)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ProfileName = data.ProfileName,
                    TargetCollection = data.TargetCollection,
                    UUID = data.UUID
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                query = _db.Table<LatencyMonitorTargetProfiles>().ToList();
            }

            _semaphore.Release();

            return await Task.FromResult(query);
        }

        public async Task DeleteSelectedProfilesAsync(LatencyMonitorTargetProfiles selectedProfile)
        {
            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
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
            //_globalSettings.IPScannerReportID = GenerateReportNumber();

            //await _semaphore.WaitAsync();

            //using (_db = new SQLiteConnection(_globalSettings.DatabasePath))
            //{
            //    var report = new IPScannerReports()
            //    {
            //        ReportID = _globalSettings.IPScannerReportID,
            //        TotalScannableIPs = _globalSettings.TotalSizeOfSubnetToScan,
            //        TotalActiveIPs = _globalSettings.TotalActiveIPAddresses,
            //        TotalInactiveIPs = _globalSettings.TotalInactiveIPAddresses,
            //        TotalDuration = _globalSettings.TotalScanDuration,
            //        CreatedWhen = _globalSettings.DateScanWasPerformed,
            //        ReportType = _globalSettings.IPScannerReportType
            //    };

            //    _db.Insert(report);
            //}

            //_semaphore.Release();
        }

        // Used to create a new entry in the IPScannerReportEntries table
        public async Task NewIPScannerReportEntryAsync(IPScannerData data)
        {
            //await _semaphore.WaitAsync();

            //using (_db = new SQLiteConnection(_globalSettings.DatabasePath))
            //{
            //    var report = new IPScannerReportEntries()
            //    {
            //        ReportID = _globalSettings.IPScannerReportID,
            //        Name = data.Name,
            //        IPAddress = data.IPAddress,
            //        MACAddress = data.MACAddress,
            //        Manufacturer = data.Manufacturer,
            //        RDPEnabled = data.RDPEnabled,
            //        SMBEnabled = data.SMBEnabled,
            //        SSHEnabled = data.SSHEnabled
            //    };

            //    _db.Insert(report);
            //}

            //_semaphore.Release();
        }

        public async Task UpdateIPScannerReportsAsync()
        {
            //await _semaphore.WaitAsync();

            //using (_db = new SQLiteConnection(_globalSettings.DatabasePath))
            //{
            //    var report = new IPScannerReports()
            //    {
            //        ReportID = _globalSettings.IPScannerReportID,
            //        TotalScannableIPs = _globalSettings.TotalSizeOfSubnetToScan,
            //        TotalActiveIPs = _globalSettings.TotalActiveIPAddresses,
            //        TotalInactiveIPs = _globalSettings.TotalInactiveIPAddresses,
            //        TotalDuration = _globalSettings.TotalScanDuration,
            //        CreatedWhen = _globalSettings.DateScanWasPerformed,
            //        ReportType = _globalSettings.IPScannerReportType
            //    };

            //    _db.Update(report);
            //}

            //_semaphore.Release();
        }

        // Used to pull the contents of the IPScannerReports table
        public async Task<List<IPScannerReports>> GetIPScannerReportAsync(string selectedReportID)
        {
            var query = new List<IPScannerReports>();

            await _semaphore.WaitAsync();

            using (_db = new SQLiteConnection(_settings.DatabasePath))
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
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

                using (_db = new SQLiteConnection(_settings.DatabasePath))
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

                using (_db = new SQLiteConnection(_settings.DatabasePath))
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

            using (_db = new SQLiteConnection(_settings.DatabasePath))
            {
                _db.DeleteAll<LatencyMonitorReportEntries>();
                _db.DeleteAll<LatencyMonitorReports>();

                _db.DeleteAll<IPScannerReportEntries>();
                _db.DeleteAll<IPScannerReports>();
            }

            _semaphore.Release();
        }

        public string GetDatabaseSize()
        {
            var fileInfo = new FileInfo(_settings.DatabasePath);
            long fileSizeInBytes = fileInfo.Length;
            string readableFileSize = string.Empty;

            if (fileSizeInBytes <= 1024)
            {
                readableFileSize = $"{fileSizeInBytes.ToString("N0")} B";
            }
            else if (fileSizeInBytes > 1024 && fileSizeInBytes <= 1048576)
            {
                double fileSizeInKB = fileSizeInBytes / 1024;
                readableFileSize = $"{fileSizeInKB.ToString("N0")} KB";
            }
            else if (fileSizeInBytes > 1048576 && fileSizeInBytes <= 1073741824)
            {
                double fileSizeInMB = fileSizeInBytes / Math.Pow(1024, 2);
                readableFileSize = $"{fileSizeInMB.ToString("N0")} MB";
            }
            else if (fileSizeInBytes > 1073741824 && fileSizeInBytes <= 1099511627776)
            {
                double fileSizeInGB = fileSizeInBytes / Math.Pow(1024, 3);
                readableFileSize = $"{fileSizeInGB.ToString("N0")} GB";
            }

            return readableFileSize;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
