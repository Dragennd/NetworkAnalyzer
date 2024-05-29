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

        [ObservableProperty]
        public bool autoChecked = true;

        [ObservableProperty]
        public bool manualChecked = false;

        [ObservableProperty]
        public bool isErrored = false;

        [ObservableProperty]
        public string? errorMsg;

        public IPScannerViewModel()
        {
            ScanData = new();
        }

        // Start the IPScanner scan and step through the individual components
        [RelayCommand]
        public async Task StartIPScannerAsync()
        {
            IPScannerFunction ipScannerFunction = new();

            try
            {
                List<Task> tasks = new();
                Stopwatch watch = new();

                watch.Start();
                IsScanning = true;
                IsEnabled = false;
                IsErrored = false;

                ScanData.Clear();
                ScanResults.Clear();

                // Process the IP Addresses and MAC Addresses first since the rest of the scan is dependant upon them
                if (AutoChecked)
                {
                    await ipScannerFunction.GetActiveIPAddressesAsync();
                }
                else
                {
                    await ipScannerFunction.GetActiveIPAddressesAsync(SubnetsToScan);
                }

                await ipScannerFunction.GetActiveMACAddressesAsync();

                // Process everything else asynchronously since they are not dependant upon each other
                tasks.Add(ipScannerFunction.GetMACAddressInfoAsync());
                tasks.Add(ipScannerFunction.GetDNSHostNameAsync());
                tasks.Add(ipScannerFunction.GetRDPPortAvailabilityAsync());
                tasks.Add(ipScannerFunction.GetSMBPortAvailabilityAsync());
                tasks.Add(ipScannerFunction.GetSSHPortAvailabilityAsync());

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
            catch (ArgumentNullException ex)
            {
                IsScanning = false;
                IsEnabled = true;
                ErrorMsg = ex.Message;
                IsErrored = true;
            }
            catch (ExceptionHandler ex)
            {
                IsScanning = false;
                IsEnabled = true;
                ErrorMsg = ex.Message;
                IsErrored = true;
            }
        }

        [RelayCommand]
        public static async Task ConnectRDPAsync(string ipAddress) => await new RDPHandler().StartRDPSessionAsync(ipAddress);

        [RelayCommand]
        public static async Task ConnectSMBAsync(string ipAddress) => await new SMBHandler().StartSMBSessionAsync(ipAddress);

        [RelayCommand]
        public static async Task ConnectSSHAsync(string ipAddress) => await new SSHHandler().StartSSHSessionAsync(ipAddress);
    }
}