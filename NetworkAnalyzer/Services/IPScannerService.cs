using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class IPScannerService
{
    #region Properties
    // Contains all active subnets as calculated by the available NICs on the device
    private List<IPv4Info> ActiveSubnets { get; set; } = new();

    // Contains the processed subnet data defined by the user in SubnetsToScan
    private IPv4Info UserDefinedSubnet { get; set; }

    // Contains ReportID for this session
    private string ReportID { get; set; } = "N/A";

    // Contains the datetime the scan was started
    private string StartTime { get; set; } = "N/A";

    // Contains user input for the subnets to scan by the network scan
    public string SubnetsToScan
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
            }
        }
    }

    // Contains the amount of time the scan was active
    public string ScanDuration
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
            }
        }
    } = "00:00.000";
    
    public string CustomPortCollection
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
            }
        }
    }
    
    public ScanType ScanType
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
            }
        }
    }

    // Contains the overall amount of addresses to be scanned
    private int _totalAddressCount = 0;

    // Contains the amount of addresses which returned data and are considered active
    private int _totalActiveAddresses = 0;

    // Contains the amount of addresses which failed to return data and are considered inactive
    private int _totalInactiveAddresses = 0;

    private bool _isScanning = false;

    private readonly CancellationTokenSource _scanCTS = new();
    private readonly SemaphoreSlim _manufacturerSemaphore = new(1);
    private readonly ISubnetHandler _subnetHandler;
    private readonly IMACAddressHandler _macAddressHandler;
    private readonly DatabaseHandler _dbHandler = App.AppHost.Services.GetRequiredService<DatabaseHandler>();
    private readonly IDNSHandler _dnsHandler;
    private readonly IPScannerController _ipScannerController = App.AppHost.Services.GetRequiredService<IPScannerController>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    #endregion Properties

    public IPScannerService(ISubnetHandler subnetHandler, IMACAddressHandler macAddressHandler, IDNSHandler dnsHandler)
    {
        _subnetHandler = subnetHandler;
        _macAddressHandler = macAddressHandler;
        _dnsHandler = dnsHandler;

        _ipScannerController.SetEmergencyStopStatus += CancelScan;
    }

    public async Task StartScanAsync(bool isAutoChecked)
    {
        ResetStatistics();
        _isScanning = true;
        _ = UpdateUICounts();
        StartTime = DateTime.Now.ToString("G");
        ReportID = GenerateNewGUID();
        
        await _dbHandler.NewIPScannerReportAsync(new IPScannerReports()
        {
            ReportID = ReportID,
            TotalScannableIPs = _totalAddressCount,
            TotalActiveIPs = _totalActiveAddresses,
            TotalInactiveIPs = _totalInactiveAddresses,
            TotalDuration = ScanDuration,
            CreatedWhen = StartTime,
            ReportMode = ReportMode.IPScanner,
            ScanType = ScanType
        }).ConfigureAwait(false);

        if (!isAutoChecked) // If Manual Mode is set, perform these checks
        {
            UserDefinedSubnet = new IPv4Info(SubnetsToScan, isAutoChecked);

            if (UserDefinedSubnet.IsError == true)
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        "Scan failed to start", 
                        "The provided subnet failed to be scanned. Please check the formatting and try again.", 
                        NotificationType.Error));
                _isScanning = false;
                return;
            }

            var info = 
                await _subnetHandler.CalculateNetworkAndBroadcastAddressesAsync(UserDefinedSubnet.IPv4Address, UserDefinedSubnet.SubnetMask).ConfigureAwait(false);

            UserDefinedSubnet.NetworkAddress = info._networkAddress;
            UserDefinedSubnet.BroadcastAddress = info._broadcastAddress;
        }
        else // If Auto Mode is set, perform these checks
        {
            ActiveSubnets = await _subnetHandler.GenerateListOfActiveSubnetsAsync().ConfigureAwait(false);
        }

        _ipScannerController.SendUpdateScanStatusRequest("SCANNING");
        await ProcessActiveSubnetsAsync(isAutoChecked).ConfigureAwait(false);
        await _dbHandler.UpdateIPScannerReportAsync(new IPScannerReports()
        {
            ReportID = ReportID,
            TotalScannableIPs = _totalAddressCount,
            TotalActiveIPs = _totalActiveAddresses,
            TotalInactiveIPs = _totalInactiveAddresses,
            TotalDuration = ScanDuration,
            CreatedWhen = StartTime,
            ReportMode = ReportMode.IPScanner
        }).ConfigureAwait(false);
        await Task.Delay(1000);
        _ipScannerController.SendUpdateScanStatusRequest("IDLE");
        _isScanning = false;
    }

    #region Private Methods
    private async Task ProcessActiveSubnetsAsync(bool isAutoChecked)
    {
        int[] lowerBound = new int[4];
        int[] upperBound = new int[4];

        if (isAutoChecked)
        {
            foreach (var subnet in ActiveSubnets)
            {
                lowerBound = subnet.NetworkAddress.Split('.').Select(int.Parse).ToArray();
                upperBound = subnet.BroadcastAddress.Split('.').Select(int.Parse).ToArray();

                try
                {
                    await Parallel.ForEachAsync(
                        GenerateIPs(lowerBound, upperBound),
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 50,
                            CancellationToken = _scanCTS.Token
                        },
                        async (ip, token) =>
                        {
                            token.ThrowIfCancellationRequested();
                            await ProcessIPAddressAsync(ip).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Do nothing, the cancellation should bubble up and processed out in the View Model
                }
            }

            lowerBound = new int[4];
            upperBound = new int[4];
        }
        else
        {
            if (UserDefinedSubnet.IsIPv4Range)
            {
                lowerBound = UserDefinedSubnet.LowerRange.Split('.').Select(int.Parse).ToArray();
                upperBound = UserDefinedSubnet.UpperRange.Split('.').Select(int.Parse).ToArray();
            }
            else if (UserDefinedSubnet.IsIPv4WithCIDR || UserDefinedSubnet.IsIPv4WithMask)
            {
                lowerBound = UserDefinedSubnet.NetworkAddress.Split('.').Select(int.Parse).ToArray();
                upperBound = UserDefinedSubnet.BroadcastAddress.Split('.').Select(int.Parse).ToArray();
            }

            try
            {
                await Parallel.ForEachAsync(
                    GenerateIPs(lowerBound, upperBound),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 50,
                        CancellationToken = _scanCTS.Token
                    },
                    async (ip, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        await ProcessIPAddressAsync(ip).ConfigureAwait(false);
                    }).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the cancellation should bubble up and processed out in the View Model
            }

            lowerBound = new int[4];
            upperBound = new int[4];
        }
    }

    private IEnumerable<string> GenerateIPs(int[] lowerBound, int[] upperBound)
    {
        // Loops through the provided bounds for the first octet of the IP Address to be generated
        for (int h = lowerBound[0]; h <= upperBound[0]; h++)
        {
            // Loops through the provided bounds for the second octet of the IP Address to be generated
            for (int i = lowerBound[1]; i <= upperBound[1]; i++)
            {
                // Loops through the provided bounds for the third octet of the IP Address to be generated
                for (int j = lowerBound[2]; j <= upperBound[2]; j++)
                {
                    // Loops through the provided bounds for the fourth octet of the IP Address to be generated
                    for (int k = lowerBound[3]; k <= upperBound[3]; k++)
                    {
                        yield return $"{h}.{i}.{j}.{k}";
                    }
                }
            }
        }
    }

    private async Task ProcessIPAddressAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var pingResult = await ping.SendPingAsync(ipAddress, 2000).ConfigureAwait(false);

            if (pingResult.Status == IPStatus.Success)
            {
                var mac = await _macAddressHandler.GetMACAddressAsync(pingResult.Address.ToString()).ConfigureAwait(false);
                var results = await NewIPScannerDataAsync(pingResult.Address.ToString(), mac, pingResult.RoundtripTime).ConfigureAwait(false);
                
                _ipScannerController.SendAddScanResultsRequest(results);
                await _dbHandler.NewIPScannerReportEntryAsync(results).ConfigureAwait(false);
                
                Interlocked.Increment(ref _totalActiveAddresses);
            }
            else
            {
                Interlocked.Increment(ref _totalInactiveAddresses);
            }
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _totalInactiveAddresses);
        }
        finally
        {
            Interlocked.Increment(ref _totalAddressCount);
        }
    }

    private async Task<IPScannerData> NewIPScannerDataAsync(string ipAddress, string macAddress, long rtt)
    {
        var activeIP = new IPScannerData();

        activeIP.ReportID = ReportID;
        activeIP.IPAddress = ipAddress;
        activeIP.Latency = rtt;
        activeIP.MACAddress = macAddress.ToUpper();
        activeIP.Name = await _dnsHandler.GetDeviceNameAsync(ipAddress).ConfigureAwait(false);

        await _manufacturerSemaphore.WaitAsync().ConfigureAwait(false);
        activeIP.Manufacturer = await _macAddressHandler.GetManufacturerAsync(macAddress).ConfigureAwait(false);
        await Task.Delay(600).ConfigureAwait(false);
        _manufacturerSemaphore.Release();

        var portHandler = new PortHandler(ipAddress, ScanType, activeIP, CustomPortCollection);
        await portHandler.StartPortScanAsync().ConfigureAwait(false);
        
        if (activeIP.ActivePorts.Count == 0)
        {
            activeIP.ActivePorts.Add("None");
        }
        
        return activeIP;
    }
    
    private void CancelScan(bool cancel)
    {
        if (cancel)
        {
            _scanCTS.Cancel();   
        }
    }
    
    private void ResetStatistics()
    {
        ActiveSubnets.Clear();
        StartTime = string.Empty;
        UserDefinedSubnet = null;
        _totalAddressCount = 0;
        _totalActiveAddresses = 0;
        _totalInactiveAddresses = 0;
    }

    private async Task UpdateUICounts()
    {
        while (_isScanning)
        {
            _ipScannerController.SendUpdateTotalActiveAddressesRequest(_totalActiveAddresses);
            _ipScannerController.SendUpdateTotalInactiveAddressesRequest(_totalInactiveAddresses);
            _ipScannerController.SendUpdateTotalAddressCountRequest(_totalAddressCount);
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    private string GenerateNewGUID() => Guid.NewGuid().ToString();
    #endregion Private Methods
}