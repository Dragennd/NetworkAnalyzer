using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal class IPScannerService : IIPScannerService
    {
        // Contains all results from the network scan
        public ConcurrentBag<IPScannerData> AllScanResults { get; set; }

        // Contains user input for the subnets to scan by the network scan
        public string SubnetsToScan { get; set; } = string.Empty;

        // Contains the current status of the network scan
        public string ScanStatus { get; set; } = string.Empty;

        // Contains the amount of time the scan was active
        public string ScanDuration { get; set; } = string.Empty;

        // Contains the overall amount of addresses to be scanned
        public int TotalAddressCount { get; set; }

        // Contains the amount of addresses which returned data and are considered active
        public int TotalActiveAddresses { get; set; }

        // Contains the amount of addresses which failed to return data and are considered inactive
        public int TotalInactiveAddresses { get; set; }

        public IPScannerService()
        {
            AllScanResults = new();
        }
    }
}
