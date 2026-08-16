using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.ExtensionMethods;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class HomeService
{
    private readonly HomeController _homeController;
    private GitHubResponse Response { get; set; }
    private GitHubRequestHandler GitHubRequestHandler { get; set; } = new();

    public HomeService(HomeController homeController)
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

    public async Task<(string, string, string)> GetChangelogAsync()
    {
        string generalNotes = string.Empty;
        string newFeatures = string.Empty;
        string bugFixes = string.Empty;
        
        try
        {
            Response = await GitHubRequestHandler.ProcessEncodedResponse(await GitHubRequestHandler.GetRepositoryManifest());

            if (Response.VersionInfo.Find(a => a.Build == GlobalSettings.BuildVersion) != null)
            {
                generalNotes = await GetGeneralNotesAsync();
                newFeatures = await GetNewFeaturesAsync();
                bugFixes = await GetBugFixesAsync();
            }
            else
            {
                generalNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                newFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                bugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }
        }
        catch (InvalidOperationException)
        {
            generalNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            newFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            bugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
        }
        catch (HttpRequestException)
        {
            generalNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            newFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            bugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
        }
        catch (TaskCanceledException)
        {
            generalNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            newFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            bugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
        }

        return await Task.FromResult((generalNotes,newFeatures,bugFixes));
    }
    
    private async Task<string> GetGeneralNotesAsync()
    {
        var info = Response.VersionInfo.Find(a => a.Build == GlobalSettings.BuildVersion);
        return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.GeneralNotes)));
    }

    private async Task<string> GetNewFeaturesAsync()
    {
        var info = Response.VersionInfo.Find(a => a.Build == GlobalSettings.BuildVersion);
        return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.NewFeatures)));
    }

    private async Task<string> GetBugFixesAsync()
    {

        var info = Response.VersionInfo.Find(a => a.Build == GlobalSettings.BuildVersion);
        return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.BugFixes)));
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