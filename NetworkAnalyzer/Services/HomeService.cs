using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetworkAnalyzer.ExtensionMethods;
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

    public async Task StartNetworkStatusMonitorAsync()
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

    public async Task<string> GetDeviceNameAsync() => 
        await Task.FromResult(Environment.MachineName);

    public async Task<string> GetCurrentUserAsync() =>
        await Task.FromResult(Environment.UserName);

    public async Task<string> GetOSAsync() =>
        await Task.FromResult(RuntimeInformation.OSDescription);

    public async Task<string> GetBiosVersionAsync()
    {
        string biosManufacturer = string.Empty;
        
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using (ManagementObjectSearcher osDetails = new("SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementObject item in osDetails.Get().Cast<ManagementObject>())
                    {
                        biosManufacturer = $"{item["Manufacturer"]} v{item["SMBIOSMajorVersion"]}.{item["SMBIOSMinorVersion"]}";
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, string will return empty if the bios info is not available
            }  
        }

        if (OperatingSystem.IsLinux())
        {
            string biosVersionPath = "/sys/class/dmi/id/bios_version";
            string biosVendorPath = "/sys/class/dmi/id/bios_vendor";
            
            biosManufacturer = $"{(await File.ReadAllTextAsync(biosVendorPath)).Trim()} v{(await File.ReadAllTextAsync(biosVersionPath)).Trim()}";
        }

        return await Task.FromResult(biosManufacturer);
    }

    public async Task<string> GetBIOSReleaseDateAsync()
    {
        string biosReleaseDate = string.Empty;
        
        if (OperatingSystem.IsWindows())
        {
            // To-Do: Look into the WMI options for getting the bios release date
        }
        
        if (OperatingSystem.IsLinux())
        {
            string biosReleasePath = "/sys/class/dmi/id/bios_date";

            biosReleaseDate = (await File.ReadAllTextAsync(biosReleasePath)).Trim();
        }

        return await Task.FromResult(biosReleaseDate);
    }

    public async Task<string> GetSystemUptimeAsync()
    {
        TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        return await Task.FromResult($"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
    }

    public async Task<string> GetIPv4GatewaysAsync()
    {
        var gatewayaddresses = 
            NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().GatewayAddresses);

        var filteredGatewayAddresses = gatewayaddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address);

        return await Task.FromResult(string.Join("\n", filteredGatewayAddresses));
    }

    public async Task<string> GetIPv4AddressesAsync()
    {
        var interfaceAddresses = 
            NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);

        var filteredIPAddresses = interfaceAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address);

        return await Task.FromResult(string.Join("\n", filteredIPAddresses));
    }

    public async Task<string> GetIPv6AddressesAsync()
    {
        var interfaceAddresses = 
            NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);
        
        var filteredIPAddresses = interfaceAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetworkV6)
            .Select(a => a.Address);
        
        return await Task.FromResult(string.Join("\n", filteredIPAddresses));
    }

    public async Task<string> GetMACAddresses()
    {
        var interfaceAddresses = 
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(a => a.OperationalStatus == OperationalStatus.Up)
                .Select(a => a.GetPhysicalAddress().ToString().FormatAsMacAddress());

        return await Task.FromResult(string.Join("\n", interfaceAddresses));
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