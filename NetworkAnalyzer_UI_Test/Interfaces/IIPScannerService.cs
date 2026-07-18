using System.ComponentModel;
using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IIPScannerService: INotifyPropertyChanged
    {
        string SubnetsToScan { get; set; }
        string ScanDuration { get; set; }

        Task StartScanAsync(bool isAutoChecked);
    }
}
