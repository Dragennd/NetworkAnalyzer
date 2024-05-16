using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    public partial class IPScannerViewModel : ObservableObject
    {
        public ObservableCollection<IPScanData> ScanData { get; set; }

        [ObservableProperty]
        public string? subnetsToScan;

        public ICommand StartScanCommand { get; }

        public IPScannerViewModel()
        {
            StartScanCommand = new RelayCommand(async () => await StartIPScannerAsync());
            ScanData = new ObservableCollection<IPScanData>(ScanResults);
        }

        public static async Task StartIPScannerAsync()
        {
            List<Task> tasks = new();

            await IPScannerFunction.GetActiveIPAddressesAsync();
            await IPScannerFunction.GetActiveMACAddressesAsync();
            
            tasks.Add(IPScannerFunction.GetMACAddressInfoAsync());
            tasks.Add(IPScannerFunction.GetDNSHostNameAsync());
            tasks.Add(IPScannerFunction.GetRDPPortAvailabilityAsync());
            tasks.Add(IPScannerFunction.GetSMBPortAvailabilityAsync());
            tasks.Add(IPScannerFunction.GetSSHPortAvailabilityAsync());

            await Task.WhenAll(tasks);
        }
    }
}