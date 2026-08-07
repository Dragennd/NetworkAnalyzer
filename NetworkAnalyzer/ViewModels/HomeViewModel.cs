using System.Collections.ObjectModel;
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
    public ObservableCollection<NetworkStatusInfo> IPv4StatusInfo { get; set; }
    public ObservableCollection<NetworkStatusInfo> IPv6StatusInfo { get; set; }
    public ObservableCollection<NetworkStatusInfo> DNSStatusInfo { get; set; }
    public ObservableCollection<ISeries> IPv4Series { get; set; }
    public ObservableCollection<ISeries> IPv6Series { get; set; }
    public ObservableCollection<ISeries> DNSSeries { get; set; }
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestIPv4 { get; set; }
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestIPv6 { get; set; }
    
    [ObservableProperty]
    public partial NetworkStatusInfo LatestDNS { get; set; }
    
    private IHomeController _homeController;
    private readonly HomeService _homeService = App.AppHost.Services.GetRequiredService<HomeService>();

    public HomeViewModel(IHomeController homeController)
    {
        _homeController = homeController;

        IPv4StatusInfo = new();
        IPv6StatusInfo = new();
        DNSStatusInfo = new();

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
        
        SetSubscriptions();

        _ = _homeService.StartNetworkStatusMonitor();
    }

    private void SetSubscriptions()
    {
        _homeController.UpdateIPv4 += SetIPv4Status;
        _homeController.UpdateIPv6 += SetIPv6Status;
        _homeController.UpdateDNS += SetDNSStatus;
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