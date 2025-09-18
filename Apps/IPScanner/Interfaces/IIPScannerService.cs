using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IIPScannerService: INotifyPropertyChanged
    {
        public string SubnetsToScan { get; set; }
        public string ScanStatus { get; set; }
        public string ScanDuration { get; set; }
        public int TotalAddressCount { get; set; }
        public int TotalActiveAddresses { get; set; }
        public int TotalInactiveAddresses { get; set; }

        Task StartScanAsync(bool isAutoChecked);
    }
}
