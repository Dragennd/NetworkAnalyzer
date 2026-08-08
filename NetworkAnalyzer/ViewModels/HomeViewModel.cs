using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.Services;

namespace NetworkAnalyzer.ViewModels;

internal partial class HomeViewModel : ObservableValidator
{
    public ObservableCollection<NetworkStatusInfo> IPv4StatusInfo { get; set; } = new();
    public ObservableCollection<NetworkStatusInfo> IPv6StatusInfo { get; set; } = new();
    public ObservableCollection<NetworkStatusInfo> DNSStatusInfo { get; set; } = new();
    public ObservableCollection<ISeries> IPv4Series { get; set; } = new();
    public ObservableCollection<ISeries> IPv6Series { get; set; } = new();
    public ObservableCollection<ISeries> DNSSeries { get; set; } = new();
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestIPv4 { get; set; }
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestIPv6 { get; set; }
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestDNS { get; set; }
    
    [ObservableProperty]
    public partial string DeviceName { get; private set; }
    
    [ObservableProperty]
    public partial string CurrentUser { get; private set; }
    
    [ObservableProperty]
    public partial string OperatingSystem { get; private set; }
    
    [ObservableProperty]
    public partial string BIOSVersion { get; private set; }
    
    [ObservableProperty]
    public partial string BIOSReleaseDate { get; private set; }
    
    [ObservableProperty]
    public partial string IPv4Gateways { get; private set; }
    
    [ObservableProperty]
    public partial string IPv4Addresses { get; private set; }
    
    [ObservableProperty]
    public partial string IPv6Addresses { get; private set; }
    
    [ObservableProperty]
    public partial string MACAddresses { get; private set; }
    
    [ObservableProperty]
    public partial string SystemUptime { get; private set; }
    
    private IHomeController _homeController;
    private readonly HomeService _homeService = App.AppHost.Services.GetRequiredService<HomeService>();

    public HomeViewModel(IHomeController homeController)
    {
        _homeController = homeController;
        
        SetChartDataSets();
        SetSubscriptions();
        _ = SetDeviceInfoAsync();
        _ = SetNetworkInfoAsync();
        _ = _homeService.StartNetworkStatusMonitorAsync();
    }

    private void SetSubscriptions()
    {
        _homeController.UpdateIPv4 += SetIPv4Status;
        _homeController.UpdateIPv6 += SetIPv6Status;
        _homeController.UpdateDNS += SetDNSStatus;
    }

    private async Task SetDeviceInfoAsync()
    {
        DeviceName = await _homeService.GetDeviceNameAsync();
        CurrentUser = await _homeService.GetCurrentUserAsync();
        OperatingSystem = await _homeService.GetOSAsync();
        BIOSVersion = await _homeService.GetBiosVersionAsync();
        BIOSReleaseDate = await _homeService.GetBIOSReleaseDateAsync();
        SystemUptime = await _homeService.GetSystemUptimeAsync();
    }

    private async Task SetNetworkInfoAsync()
    {
        IPv4Gateways = await _homeService.GetIPv4GatewaysAsync();
        IPv4Addresses = await _homeService.GetIPv4AddressesAsync();
        IPv6Addresses = await _homeService.GetIPv6AddressesAsync();
        MACAddresses = await _homeService.GetMACAddresses();
    }

    private void SetChartDataSets()
    {
        IPv4Series = new()
        {
            new LineSeries<NetworkStatusInfo>
            {
                Values = IPv4StatusInfo,
                GeometrySize = 8,
                
            }
        };
        
        IPv6Series = new()
        {
            new LineSeries<NetworkStatusInfo>
            {
                Values = IPv6StatusInfo,
                GeometrySize = 8
            }
        };
        
        DNSSeries = new()
        {
            new LineSeries<NetworkStatusInfo>
            {
                Values = DNSStatusInfo,
                GeometrySize = 8
            }
        };
    }

    private void SetIPv4Status(NetworkStatusInfo networkStatusInfo)
    {
        LatestIPv4 = networkStatusInfo;
        IPv4StatusInfo.Add(networkStatusInfo);

        if (IPv4StatusInfo.Count >= 18)
        {
            IPv4StatusInfo.RemoveAt(0);
        }
    }

    private void SetIPv6Status(NetworkStatusInfo networkStatusInfo)
    {
        LatestIPv6 = networkStatusInfo;
        IPv6StatusInfo.Add(networkStatusInfo);

        if (IPv6StatusInfo.Count >= 18)
        {
            IPv6StatusInfo.RemoveAt(0);
        }
    }

    private void SetDNSStatus(NetworkStatusInfo networkStatusInfo)
    {
        LatestDNS = networkStatusInfo;
        DNSStatusInfo.Add(networkStatusInfo);

        if (DNSStatusInfo.Count >= 18)
        {
            DNSStatusInfo.RemoveAt(0);
        }
    }
}