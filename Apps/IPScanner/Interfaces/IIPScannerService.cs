using System.ComponentModel;

namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IIPScannerService: INotifyPropertyChanged
    {
        string SubnetsToScan { get; set; }
        string ScanDuration { get; set; }

        Task StartScanAsync(bool isAutoChecked);
    }
}
