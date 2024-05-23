using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    public partial class IPScannerViewModel : ObservableRecipient
    {
        public ObservableCollection<IPScanData> ScanData { get; set; }

        [ObservableProperty]
        public string? subnetsToScan;

        [ObservableProperty]
        public bool isEnabled = true;

        public IPScannerViewModel()
        {
            ScanData = new();
        }

        // Start the IPScanner scan and step through the individual components
        [RelayCommand]
        public async Task StartIPScannerAsync()
        {
            IsEnabled = false;
            List<Task> tasks = new();

            // Process the IP Addresses and MAC Addresses first since the rest of the scan is dependant upon them
            await IPScannerFunction.GetActiveIPAddressesAsync();
            await IPScannerFunction.GetActiveMACAddressesAsync();

            // Process everything else asynchronously since they are not dependant upon each other
            tasks.Add(IPScannerFunction.GetMACAddressInfoAsync());
            tasks.Add(IPScannerFunction.GetDNSHostNameAsync());
            tasks.Add(IPScannerFunction.GetRDPPortAvailabilityAsync());
            tasks.Add(IPScannerFunction.GetSMBPortAvailabilityAsync());
            tasks.Add(IPScannerFunction.GetSSHPortAvailabilityAsync());

            await Task.WhenAll(tasks);

            ScanData.Clear();
            
            foreach (var item in ScanResults)
            {
                // Assign each successfully scanned IP Address to the DataGrid
                ScanData.Add(item);
            }

            IsEnabled = true;
        }
    }
}