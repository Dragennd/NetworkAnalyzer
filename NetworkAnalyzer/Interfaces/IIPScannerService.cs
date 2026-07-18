using System.ComponentModel;
using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IIPScannerService: INotifyPropertyChanged
    {
        string SubnetsToScan { get; set; }
        string ScanDuration { get; set; }

        Task StartScanAsync(bool isAutoChecked);
    }
}
