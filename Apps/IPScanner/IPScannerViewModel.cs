using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
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

        [ObservableProperty]
        public bool isScanning = false;

        [ObservableProperty]
        public string? scanDuration = "N/A";

        public IPScannerViewModel()
        {
            ScanData = new();
        }

        // Start the IPScanner scan and step through the individual components
        [RelayCommand]
        public async Task StartIPScannerAsync()
        {
            List<Task> tasks = new();
            Stopwatch watch = new();

            watch.Start();
            IsScanning = true;
            IsEnabled = false;

            ScanData.Clear();
            ScanResults.Clear();

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

            watch.Stop();
            IsScanning = false;
            
            foreach (var item in ScanResults)
            {
                // Assign each successfully scanned IP Address to the DataGrid
                ScanData.Add(item);
            }

            ScanDuration = watch.Elapsed.ToString(@"mm\:ss\.fff");
            IsEnabled = true;
        }

        [RelayCommand]
        public static async Task ConnectRDPAsync(string ipAddress) => await RDPHandler.StartRDPSessionAsync(ipAddress);

        [RelayCommand]
        public static async Task ConnectSMBAsync(string ipAddress) => await SMBHandler.StartSMBSessionAsync(ipAddress);

        [RelayCommand]
        public static async Task ConnectSSHAsync(string ipAddress) => await SSHHandler.StartSSHSessionAsync(ipAddress);
    }
}