using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class HomeService
{
    private readonly IHomeController _homeController;

    public HomeService(IHomeController homeController)
    {
        _homeController = homeController;
    }

    public async Task StartNetworkStatusMonitor()
    {
        while (true)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            await GetIPv4NetworkStatusAsync();
            await GetIPv6NetworkStatusAsync();
            await GetDNSNetworkStatusAsync();
            
            sw.Stop();

            if (sw.ElapsedMilliseconds < 5000)
            {
                await Task.Delay(5000 - (int)sw.ElapsedMilliseconds);   
            }
        }
    }
    
    private async Task GetIPv4NetworkStatusAsync()
    {
        using Ping ping = new();
        string target = "8.8.8.8";
        int latency;
        IPStatus status;

        try
        {
            // Check DNS against www.google.com and return true if successful
            PingReply response = await ping.SendPingAsync(target, 1000);
            status = response.Status;
            latency = (int)response.RoundtripTime;
        }
        catch (Win32Exception)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        catch (PingException)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        
        _homeController.SendUpdateIPv4Request(new NetworkStatusInfo(target, latency, status));
    }

    private async Task GetIPv6NetworkStatusAsync()
    {
        using Ping ping = new();
        string target = "2001:4860:4860::8888";
        int latency;
        IPStatus status;

        try
        {
            // Check DNS against www.google.com and return true if successful
            PingReply response = await ping.SendPingAsync(target, 1000);
            status = response.Status;
            latency = (int)response.RoundtripTime;
        }
        catch (Win32Exception)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        catch (PingException)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        
        _homeController.SendUpdateIPv6Request(new NetworkStatusInfo(target, latency, status));
    }

    private async Task GetDNSNetworkStatusAsync()
    {
        using Ping ping = new();
        string target = "www.google.com";
        int latency;
        IPStatus status;

        try
        {
            // Check DNS against www.google.com and return true if successful
            PingReply response = await ping.SendPingAsync(target, 1000);
            status = response.Status;
            latency = (int)response.RoundtripTime;
        }
        catch (Win32Exception)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        catch (PingException)
        {
            status = IPStatus.Unknown;
            latency = 0;
        }
        
        _homeController.SendUpdateDNSRequest(new NetworkStatusInfo(target, latency, status));
    }
}