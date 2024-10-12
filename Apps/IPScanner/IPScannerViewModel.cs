using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SubnetMaskHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.RDPHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SMBHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SSHHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal partial class IPScannerViewModel : ObservableRecipient
    {
        #region Control Properties
        const string ipWithCIDR = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\/(?:3[0-2]|[1-2]?[0-9])\b";
        const string ipWithSubnetMask = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\b";
        const string ipRange = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*-\s*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\b";

        public ObservableCollection<IPScannerData> ScanData { get; set; }

        [ObservableProperty]
        public string? subnetsToScan;

        [ObservableProperty]
        public string? errorMsg;

        [ObservableProperty]
        public string? scanDuration = "00:00.000";

        [ObservableProperty]
        public bool isEnabled = true;

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
        }

        // Manage the flow of the IP Scanner and user validation
        [RelayCommand]
        public async Task IPScannerManagerAsync()
        {
            (IPScannerStatusCode status, IPv4Info? info, bool errorBool, string? errorString) = await ValidateFormInputAsync();

            if (status == IPScannerStatusCode.Success && AutoChecked)
            {
                IsErrored = false;
                await StartIPScannerAsync(status);
            }
            else if ((status == IPScannerStatusCode.Success || status == IPScannerStatusCode.GoodRange) && ManualChecked)
            {
                IsErrored = false;
                await StartIPScannerAsync(status, info);
            }
            else
            {
                IsErrored = errorBool;
                ErrorMsg = errorString;
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
        // Validate user input and ensure it follows the correct formats
        private async Task<(IPScannerStatusCode status, IPv4Info? info, bool errorBool, string? errorstring)> ValidateFormInputAsync()
        {
            IPScannerStatusCode vCode;
            IPv4Info? vInfo = null;
            bool vError = false;
            string? vMessage = string.Empty;

            if (AutoChecked)
            {
                vCode = IPScannerStatusCode.Success;
                vInfo = null;
                vError = false;
                vMessage = string.Empty;
            }
            // If user input is an IP Address followed by cidr notation (e.g. 172.30.1.1/24)
            else if (!string.IsNullOrWhiteSpace(SubnetsToScan) && Regex.IsMatch(SubnetsToScan, ipWithCIDR))
            {
                vInfo = await ParseIPWithCIDRAsync(SubnetsToScan);
                vCode = IPScannerStatusCode.Success;
            }
            // If user input is an IP Address followed by the full subnet mask (e.g. 172.30.1.1 255.255.255.0)
            else if (!string.IsNullOrWhiteSpace(SubnetsToScan) && Regex.IsMatch(SubnetsToScan, ipWithSubnetMask))
            {
                vInfo = await ParseIPWithSubnetMaskAsync(SubnetsToScan);
                vCode = IPScannerStatusCode.Success;
            }
            // If user input is two IP Addresses separated by a hyphen (e.g. 172.30.1.1 - 172.30.1.50)
            else if (!string.IsNullOrWhiteSpace(SubnetsToScan) && Regex.IsMatch(SubnetsToScan, ipRange))
            {
                (IPv4Info info, IPScannerStatusCode status) = await ParseIPRangeAsync(SubnetsToScan);

                if (status == IPScannerStatusCode.GoodRange)
                {
                    vInfo = info;
                    vCode = IPScannerStatusCode.GoodRange;
                }
                else
                {
                    (bool errorBool, string errorString, IPScannerStatusCode IPScannerStatusCode) = ProcessIPScannerStatusCode(status);
                    vError = errorBool;
                    vMessage = errorString;
                    vCode = IPScannerStatusCode;
                }
            }
            // If user input doesn't match the proper formatting, throw an error
            else
            {
                (bool errorBool, string errorString, IPScannerStatusCode IPScannerStatusCode) = ProcessIPScannerStatusCode(IPScannerStatusCode.Invalid);
                vError = errorBool;
                vMessage = errorString;
                vCode = IPScannerStatusCode;
            }

            return (vCode, vInfo, vError, vMessage);
        }

        // Start the IP Scanner scan and step through the individual components
        private async Task StartIPScannerAsync(IPScannerStatusCode status, [Optional]IPv4Info ipv4Info)
        {
            var ipScannerManager = new IPScannerManager();
            var tasks = new List<Task>();
            var watch = new Stopwatch();

            watch.Start();

            // Clear out previous test results
            IsScanning = true;
            IsEnabled = false;
            EmptyScanResults = false;
            ScanData.Clear();
            ScanResults.Clear();
            ActiveSubnets.Clear();
            TotalSizeOfSubnetToScan = 0;
            TotalActiveIPAddresses = 0;
            ScanDuration = "00:00.000";
            TotalScanDuration = string.Empty;

            if (AutoChecked)
            {
                await GenerateListOfActiveSubnetsAsync(ManualChecked, status);
                await ipScannerManager.AddIPScannerDataAsync();
            }
            else
            {
                await GenerateListOfActiveSubnetsAsync(ManualChecked, status, ipv4Info);
                await ipScannerManager.AddIPScannerDataAsync();
            }

            watch.Stop();
            IsScanning = false;

            // Check to see if the scan located any devices
            if (ScanResults.IsEmpty)
            {
                // Display the empty banner if no results were found
                EmptyScanResults = true;
            }
            else
            {
                // Add the results of the scan to ScanData to be displayed in the DataGrid
                foreach (var item in ScanResults)
                {
                    ScanData.Add(item);
                }
            }

            ScanDuration = watch.Elapsed.ToString(@"mm\:ss\.fff");
            TotalScanDuration = ScanDuration;
            DateScanWasPerformed = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            IsEnabled = true;
        }

        // Used with user input validation - check if the input matches an IP Address with CIDR notation (e.g. 172.30.1.13 /24)
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

        // Used with user input validation - check if the input matches an IP Address with a Subnet Mask (e.g. 172.30.1.13 255.255.255.0)
        private async Task<IPv4Info> ParseIPWithSubnetMaskAsync(string userInput)
        {
            IPv4Info info = new()
            {
                IPv4Address = userInput.Split(' ')[0],
                SubnetMask = userInput.Split(' ')[1]
            };

            return await Task.FromResult(info);
        }

        // Used with user input validation - check if the input matches an IP Address range (e.g. 172.30.1.13 - 172.30.1.128)
        private async Task<(IPv4Info info, IPScannerStatusCode status)> ParseIPRangeAsync(string userInput)
        {
            IPv4Info info = new();
            string ip1 = userInput.Split("-")[0].Trim();
            string ip2 = userInput.Split("-")[1].Trim();
            bool validInput = true;
            IPScannerStatusCode status;

            // Check each octet from the IP Range to see if the left IP is greater than the right IP
            for (int i = 3; i >= 1; i--)
            {
                if (int.Parse(ip1.Split(".")[i]) > int.Parse(ip2.Split(".")[i]) &&
                  ((int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1])) ||
                   (int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]))))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]) &&
                         validInput == false)
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) < int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else
                {
                    validInput = true;
                }
            }

            if (validInput)
            {
                info.IPv4Address = ip1;
                info.SubnetMask = ip2;
                status = IPScannerStatusCode.GoodRange;
            }
            else
            {
                status = IPScannerStatusCode.BadRange;
            }

            return (await Task.FromResult(info), status);
        }

        // Process status codes and set error messages as needed
        private (bool errorBool, string errorString, IPScannerStatusCode status) ProcessIPScannerStatusCode(IPScannerStatusCode status)
        {
            string errorMsg = string.Empty;
            bool errorStatus = false;

            if (status == IPScannerStatusCode.BadRange)
            {
                errorStatus = true;
                errorMsg = "IP Address Range provided is invalid.\nPlease check your input and try again.";
            }
            
            if (status == IPScannerStatusCode.Invalid)
            {
                errorStatus = true;
                errorMsg = "IP Addressing information provided does not match the required formats.\nPlease check your input and try again.";
            }

            return (errorStatus, errorMsg, status);
        }
        #endregion
    }
}