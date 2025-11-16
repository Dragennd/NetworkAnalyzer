using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using SQLite;
using SQLitePCL;
using System.IO;
using System.Text.Json;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal class DatabaseHandler : IDatabaseHandler
    {
        private SQLiteAsyncConnection _db;

        private SemaphoreSlim _semaphore = new(1, 1);

        private readonly GlobalSettings _settings = App.AppHost.Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

        public DatabaseHandler()
        {
            Batteries.Init();
            _db = new SQLiteAsyncConnection(_settings.DatabasePath);
        }

        #region Database Global Functions
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
        #endregion Database Global Functions

        #region Latency Monitor Database Functions
        public async Task NewLatencyMonitorReportAsync(string reportID, string startTime)
        {
            await _semaphore.WaitAsync();

            try
            {
                var report = new LatencyMonitorReports()
                {
                    ReportID = reportID,
                    StartedWhen = startTime,
                    CompletedWhen = startTime,
                    TotalDuration = "00.00:00:00",
                    TotalPacketsSent = 0,
                    SuccessfullyCompleted = "false",
                    ReportMode = ReportMode.LatencyMonitor
                };

                await _db.InsertAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task NewLatencyMonitorReportEntryAsync(List<LatencyMonitorData> data)
        {
            await _semaphore.WaitAsync();

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

                await _db.InsertAsync(report);
            }

            _semaphore.Release();
        }

        public async Task<List<LatencyMonitorReports>> GetLatencyMonitorReportAsync(string selectedReportID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<LatencyMonitorReports>()
                                       .Where(a => a.ReportID == selectedReportID)
                                       .ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntryAsync(string selectedReportID, string targetGUID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<LatencyMonitorReportEntries>()
                           .Where(a => a.ReportID == selectedReportID && a.TargetGUID == targetGUID)
                           .ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesAsync(string selectedReportID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<LatencyMonitorReportEntries>()
                           .Where(a => a.ReportID == selectedReportID)
                           .ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetLatencyMonitorReportEntriesForHistoryAsync(string filterQuery)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.QueryAsync<LatencyMonitorReportEntries>(filterQuery);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetDistinctLatencyMonitorTracerouteTargetsAsync(string tracerouteGUID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return (await _db.QueryAsync<LatencyMonitorReportEntries>($"SELECT * FROM LatencyMonitorReportEntries WHERE TracerouteGUID = \"{tracerouteGUID}\" ORDER BY ID DESC LIMIT 200"))
                    .GroupBy(b => b.TargetAddress)
                    .Select(c => c.First())
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetDistinctFinalLatencyMonitorTracerouteTargetsAsync(string tracerouteGUID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return (await _db.QueryAsync<LatencyMonitorReportEntries>($"SELECT * FROM LatencyMonitorReportEntries WHERE TracerouteGUID = \"{tracerouteGUID}\" ORDER BY ID DESC LIMIT 200"))
                    .GroupBy(b => b.TargetAddress)
                    .Select(c => c.First())
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorReportEntries>> GetDistinctLatencyMonitorUserDefinedTargetsAsync(string reportGUID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.QueryAsync<LatencyMonitorReportEntries>($"SELECT * FROM LatencyMonitorReportEntries WHERE ReportID = \"{reportGUID}\" GROUP BY TracerouteGUID LIMIT 300");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateLatencyMonitorFinalDataAsync(string reportGUID, string finalDuration, int totalPacketsSent)
        {
            await _semaphore.WaitAsync();

            try
            {
                var report = await _db.Table<LatencyMonitorReports>()
                   .Where(a => a.ReportID == reportGUID)
                   .ToListAsync();

                report.First().TotalDuration = finalDuration;
                report.First().TotalPacketsSent = totalPacketsSent;

                await _db.UpdateAsync(report.First());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task NewLatencyMonitorTargetProfileAsync(LatencyMonitorPreset data)
        {
            await _semaphore.WaitAsync();

            try
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ProfileName = data.PresetName,
                    UUID = data.UUID
                };

                await _db.InsertAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateLatencyMonitorTargetProfileAsync(LatencyMonitorPreset data)
        {
            await _semaphore.WaitAsync();

            try
            {
                var report = new LatencyMonitorTargetProfiles()
                {
                    ID = data.ID,
                    ProfileName = data.PresetName,
                    TargetCollection = JsonSerializer.Serialize(data.TargetCollection),
                    UUID = data.UUID
                };

                await _db.UpdateAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LatencyMonitorTargetProfiles>> GetLatencyMonitorTargetProfilesAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<LatencyMonitorTargetProfiles>().ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteSelectedProfileAsync(LatencyMonitorPreset selectedProfile)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.DeleteAsync(new LatencyMonitorTargetProfiles()
                {
                    ID = selectedProfile.ID,
                    UUID = selectedProfile.UUID
                });
            }
            finally
            {
                _semaphore.Release();
            }            
        }

        public async Task ResetLatencyMonitorReportTablesAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.DeleteAllAsync<LatencyMonitorReports>();
                await _db.DeleteAllAsync<LatencyMonitorReportEntries>();
                await _db.ExecuteAsync("VACUUM");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ResetLatencyMonitorPresetsTableAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.DeleteAllAsync<LatencyMonitorTargetProfiles>();
                await _db.ExecuteAsync("VACUUM");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region IP Scanner Database Functions
        public async Task NewIPScannerReportAsync(IPScannerReports report)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.InsertAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task NewIPScannerReportEntryAsync(IPScannerData data)
        {
            await _semaphore.WaitAsync();

            try
            {
                var report = new IPScannerReportEntries()
                {
                    ReportID = data.ReportID,
                    Name = data.Name,
                    IPAddress = data.IPAddress,
                    MACAddress = data.MACAddress,
                    Manufacturer = data.Manufacturer,
                    RDPEnabled = data.RDPEnabled,
                    SMBEnabled = data.SMBEnabled,
                    SSHEnabled = data.SSHEnabled
                };

                await _db.InsertAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateIPScannerReportAsync(IPScannerReports report)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.UpdateAsync(report);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<IPScannerReports>> GetIPScannerReportAsync(string selectedReportID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<IPScannerReports>().Where(a => a.ReportID == selectedReportID).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<IPScannerReportEntries>> GetIPScannerReportEntriesAsync(string selectedReportID)
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.Table<IPScannerReportEntries>().Where(a => a.ReportID == selectedReportID).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region Report Explorer
        public async Task<List<ReportExplorerData>> GetIPScannerReportsAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.QueryAsync<ReportExplorerData>("SELECT ReportID as ReportGUID, CreatedWhen as StartDateTime, ReportMode as Mode FROM IPScannerReports");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ReportExplorerData>> GetLatencyMonitorReportsAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                return await _db.QueryAsync<ReportExplorerData>("SELECT ReportID as ReportGUID, ReportFriendlyName as FriendlyName, StartedWhen as StartDateTime, CompletedWhen as EndDateTime, ReportMode as Mode FROM LatencyMonitorReports");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ResetIPScannerReportTablesAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.DeleteAllAsync<IPScannerReportEntries>();
                await _db.DeleteAllAsync<IPScannerReports>();
                await _db.ExecuteAsync("VACUUM");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAllReportDataAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await _db.DeleteAllAsync<LatencyMonitorReportEntries>();
                await _db.DeleteAllAsync<LatencyMonitorReports>();
                await _db.DeleteAllAsync<LatencyMonitorTargetProfiles>();

                await _db.DeleteAllAsync<IPScannerReportEntries>();
                await _db.DeleteAllAsync<IPScannerReports>();

                await _db.ExecuteAsync("VACUUM");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
