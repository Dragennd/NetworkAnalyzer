using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.GlobalClasses;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SubnetMaskHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.RDPHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SMBHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SSHHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal partial class IPScannerViewModel : ObservableValidator
    {
        #region Control Properties
        const string ipWithCIDR = @"^(?!.*[<>:""|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\/(?:3[0-2]|[1-2]?[0-9])$";
        const string ipWithSubnetMask = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)$";
        const string ipRange = @"^(?!.*[<>:""\/|?*\x00-\x1F])(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*-\s*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])$";

        public ObservableCollection<IPScannerData> ScanData { get; set; }

        private SemaphoreSlim _semaphore = new(1, 1);

        private LogHandler LogHandler { get; set; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [IPScannerInput(nameof(ManualChecked))]
        public string? subnetsToScan;

        [ObservableProperty]
        public string? errorMsg;

        [ObservableProperty]
        public string scanDuration = "00:00.000";

        [ObservableProperty]
        public string status = "IDLE";

        [ObservableProperty]
        public int totalInactiveAddresses = 0;

        [ObservableProperty]
        public int totalActiveAddresses = 0;

        [ObservableProperty]
        public int totalSizeOfSubnets = 0;

        [ObservableProperty]
        public bool isStartButtonEnabled = true;

        [ObservableProperty]
        public bool isStopButtonEnabled = false;

        [ObservableProperty]
        public bool isScanning = false;

        [ObservableProperty]
        public bool autoChecked = true;

        [ObservableProperty]
        public bool manualChecked = false;

        [ObservableProperty]
        public bool isErrored = false;

        [ObservableProperty]
        public bool emptyScanResults = false;
        #endregion

        public IPScannerViewModel()
        {
            ScanData = new();
            LogHandler = new();
            PropertyChanged += OnStaticPropertyChanged;
        }

        // Manage the flow of the IP Scanner and user validation
        [RelayCommand]
        public async Task StartIPScanAsync()
        {
            try
            {
                if (AutoChecked)
                {
                    await StartIPScannerAsync();
                }
                else if (ManualChecked)
                {
                    if (await ValidateUserInputAsync() == false)
                    {
                        return;
                    }

                    if (Regex.IsMatch(SubnetsToScan, ipWithCIDR))
                    {
                        await StartIPScannerAsync(await ParseIPWithCIDRAsync(SubnetsToScan));
                    }
                    // If user input is an IP Address followed by the full subnet mask (e.g. 172.30.1.1 255.255.255.0)
                    else if (Regex.IsMatch(SubnetsToScan, ipWithSubnetMask))
                    {
                        await StartIPScannerAsync(await ParseIPWithSubnetMaskAsync(SubnetsToScan));
                    }
                    // If user input is two IP Addresses separated by a hyphen (e.g. 172.30.1.1 - 172.30.1.50)
                    else if (Regex.IsMatch(SubnetsToScan, ipRange))
                    {
                        await StartIPScannerAsync(await ParseIPRangeAsync(SubnetsToScan));
                    }
                }
            }
            catch (Exception ex)
            {
                await LogHandler.CreateLogEntry(ex.ToString(), LogType.Error, IPScannerReportType);
                throw;
            }
        }

        // Receive command from DataGrid and initiate a RDP session
        [RelayCommand]
        public static async Task ConnectRDPAsync(string ipAddress) => await StartRDPSessionAsync(ipAddress);

        // Receive command from DataGrid and launch File Explorer to the specified destination
        [RelayCommand]
        public static async Task ConnectSMBAsync(string ipAddress) => await StartSMBSessionAsync(ipAddress);

        // Receive command from DataGrid and initiate a SSH session
        [RelayCommand]
        public static async Task ConnectSSHAsync(string ipAddress) => await StartSSHSessionAsync(ipAddress);

        #region Private Methods
        // Start the IP Scanner scan and step through the individual components
        private async Task StartIPScannerAsync([Optional]IPv4Info ipv4Info)
        {
            var manager = new IPScannerManager();
            var dbHandler = new DatabaseHandler();
            manager.IPScannerResults += ReceiveIPScannerResults;
            IPScannerReportType = ReportType.ICMP;

            try
            {
                // Initialize the main report entry in the database for this scan
                await dbHandler.NewIPScannerReportAsync();

                // Clear out previous test results
                ClearPreviousResults();

                IsScanning = true;
                IsStartButtonEnabled = false;
                IsStopButtonEnabled = true;

                SetStatus();
                SetSessionStopwatchAsync();

                // Perform scan and return results
                if (AutoChecked)
                {
                    await GenerateListOfActiveSubnetsAsync(ManualChecked);
                    await manager.ProcessIPScannerDataAsync();
                }
                else
                {
                    await GenerateListOfActiveSubnetsAsync(ManualChecked, ipv4Info);
                    await manager.ProcessIPScannerDataAsync();
                }
            }
            catch (OperationCanceledException)
            {
                IsScanning = false;
            }
            finally
            {
                IsScanning = false;

                TotalScanDuration = ScanDuration;
                DateScanWasPerformed = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                IsStartButtonEnabled = true;
                IsStopButtonEnabled = false;
                SetStatus();
                await dbHandler.UpdateIPScannerReportsAsync();
            }

            manager.IPScannerResults -= ReceiveIPScannerResults;
            manager.Dispose();
        }

        private async Task<bool> ValidateUserInputAsync()
        {
            bool status = true;

            // Validate all of the user input fields against regex expressions
            ValidateAllProperties();

            // If the user input fields have errors based on their attributes, return false
            if (HasErrors)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }

        // Clear previous test results
        private void ClearPreviousResults()
        {
            if (AutoChecked)
            {
                SubnetsToScan = string.Empty;
            }

            EmptyScanResults = false;
            ScanData.Clear();
            ActiveSubnets.Clear();
            TotalSizeOfSubnetToScan = 0;
            TotalActiveIPAddresses = 0;
            TotalActiveAddresses = 0;
            TotalInactiveIPAddresses = 0;
            TotalInactiveAddresses = 0;
            ScanDuration = "00:00.000";
            TotalScanDuration = string.Empty;
        }

        // Start the stopwatch which is used to track and display the duration of the scan
        private async void SetSessionStopwatchAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (IsScanning)
            {
                ScanDuration = FormatElapsedTime(sw.Elapsed);
                await Task.Delay(10);
            }
        }

        // Format the scan duration time to minutes:seconds.milliseconds
        private string FormatElapsedTime(TimeSpan elapsedTime)
        {
            return $"{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}";
        }

        // Set the status text for the scan
        private void SetStatus()
        {
            if (IsStopButtonEnabled)
            {
                Status = "SCANNING";
            }
            else
            {
                Status = "IDLE";
            }
        }

        private async Task<IPv4Info> ParseIPWithCIDRAsync(string userInput)
        {
            IPv4Info info = new();

            int hostBits = int.Parse(userInput.Split("/")[1]);
            int[] maskParts = new int[4];

            // Loop through the octets of the Subnet Mask
            // and assign a mask to that position based upon the provided CIDR Notation
            for (int i = 0; i < maskParts.Length; i++)
            {
                if (hostBits >= 8)
                {
                    maskParts[i] = 255;
                    hostBits -= 8;
                }
                else if (hostBits > 0)
                {
                    maskParts[i] = 255 - ((int)Math.Pow(2, 8 - hostBits) - 1);
                    hostBits = 0;
                }
                else
                {
                    maskParts[i] = 0;
                }
            }

            info.IPv4Address = userInput.Split("/")[0];
            info.SubnetMask = string.Join(".", maskParts);

            return await Task.FromResult(info);
        }

        private async Task<IPv4Info> ParseIPWithSubnetMaskAsync(string userInput)
        {
            IPv4Info info = new()
            {
                IPv4Address = userInput.Split(' ')[0],
                SubnetMask = userInput.Split(' ')[1]
            };

            return await Task.FromResult(info);
        }

        private async Task<IPv4Info> ParseIPRangeAsync(string userInput)
        {
            IPv4Info info = new()
            {
                IPv4Address = userInput.Split("-")[0].Trim(),
                SubnetMask = userInput.Split("-")[1].Trim(),
                IsIPv4Range = true
            };

            return await Task.FromResult(info);
        }

        // Refresh the numbers displayed on the UI whenever new data is available
        private void OnStaticPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            TotalSizeOfSubnets = TotalSizeOfSubnetToScan;
            TotalInactiveAddresses = TotalInactiveIPAddresses;
            TotalActiveAddresses = TotalActiveIPAddresses;
        }

        // Refresh the scan list every time a new device is discovered
        private async void ReceiveIPScannerResults(IPScannerData data)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ScanData.Add(data);
            });
        }
        #endregion
    }
}