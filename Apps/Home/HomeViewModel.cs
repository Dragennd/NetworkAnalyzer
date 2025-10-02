using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Management;
using CommunityToolkit.Mvvm.ComponentModel;
using static NetworkAnalyzer.Apps.GlobalClasses.ExtensionsHandler;
using NetworkAnalyzer.Apps.Home.Functions;
using Material.Icons;
using System.Windows.Media;
using System.Net.Http;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Linq.Expressions;

namespace NetworkAnalyzer.Apps.Home
{
    internal partial class HomeViewModel : ObservableRecipient
    {
        #region Control Properties
        [ObservableProperty]
        public string buildID;

        [ObservableProperty]
        public string latestRelease;

        [ObservableProperty]
        public string deviceName = string.Empty;

        [ObservableProperty]
        public string currentUser = string.Empty;

        [ObservableProperty]
        public string windowsVersion = string.Empty;

        [ObservableProperty]
        public string biosInfo = string.Empty;

        [ObservableProperty]
        public string ipAddress = string.Empty;

        [ObservableProperty]
        public string gatewayAddress = string.Empty;

        [ObservableProperty]
        public string macAddress = string.Empty;

        [ObservableProperty]
        public string generalNotes = string.Empty;

        [ObservableProperty]
        public string newFeatures = string.Empty;

        [ObservableProperty]
        public string bugFixes = string.Empty;

        [ObservableProperty]
        public MaterialIconKind ipv4Kind = MaterialIconKind.CheckCircleOutline;

        [ObservableProperty]
        public MaterialIconKind ipv6Kind = MaterialIconKind.CheckCircleOutline;

        [ObservableProperty]
        public MaterialIconKind dnsKind = MaterialIconKind.CheckCircleOutline;

        [ObservableProperty]
        public SolidColorBrush ipv4KindColor = Brushes.Green;

        [ObservableProperty]
        public SolidColorBrush ipv6KindColor = Brushes.Green;

        [ObservableProperty]
        public SolidColorBrush dnsKindColor = Brushes.Green;

        [ObservableProperty]
        public GitHubResponse? response;

        [ObservableProperty]
        private bool hasUpdatesBeenChecked = false;

        private readonly GlobalSettings _globalSettings = App.AppHost.Services.GetRequiredService<GlobalSettings>();
        #endregion

        public HomeViewModel()
        {
            DeviceName = Environment.MachineName;
            CurrentUser = Environment.UserName;
            WindowsVersion = GetWindowsVersion();
            BiosInfo = GetBIOSInfo();
            IpAddress = GetIPAddress();
            GatewayAddress = GetGatewayAddress();
            MacAddress = GetMACAddress();

            BuildID = _globalSettings.CurrentBuild;
            LatestRelease = _globalSettings.ReleaseDate;

            HomeConnectionUtility.UpdateChangelogRequest += LoadChangelog;
        }

        #region Private Methods
        private string GetWindowsVersion()
        {
            var displayVersion = (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")).GetValue("DisplayVersion");
            string windowsVersion = string.Empty;

            try
            {
                using (ManagementObjectSearcher osDetails = new("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject item in osDetails.Get().Cast<ManagementObject>())
                    {
                        windowsVersion = $"{item["Caption"].ToString().Replace("Microsoft ", "")} {displayVersion}";
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing, string will return empty if the windows info is not available
            }

            return windowsVersion;
        }
        private string GetBIOSInfo()
        {
            string biosManufacturer = string.Empty;

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

            return biosManufacturer;
        }

        private string GetIPAddress()
        {
            var interfaceAddresses = NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);

            var filteredIPAddresses = interfaceAddresses
                    .Where(a =>
                           a.Address.ToString().Split(".").Length == 4 &&
                         !(a.Address.ToString().Split(".")[0] == "127" ||
                          a.Address.ToString().Split(".")[0] == "169" && a.Address.ToString().Split(".")[1] == "254" ||
                           a.Address.ToString().Contains(':')))
                    .First();

            return filteredIPAddresses.Address.ToString();
        }

        private string GetGatewayAddress()
        {
            var gatewayaddresses = NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().GatewayAddresses);

            var filteredGatewayAddresses = gatewayaddresses.Where(a =>
                           a.Address.ToString().Split(".").Length == 4 &&
                         !(a.Address.ToString().Split(".")[0] == "127" ||
                          a.Address.ToString().Split(".")[0] == "169" && a.Address.ToString().Split(".")[1] == "254" ||
                           a.Address.ToString().Contains(':')))
                    .First();

            return filteredGatewayAddresses.Address.ToString();
        }

        private string GetMACAddress()
        {
            var macAddress = NetworkInterface.GetAllNetworkInterfaces().First(a => a.OperationalStatus == OperationalStatus.Up);

            return macAddress.GetPhysicalAddress().ToString().FormatAsMacAddress();
        }

        private async Task GetNetworkStatusAsync()
        {
            NetworkStatusHandler networkStatusHandler = new();

            while (true)
            {
                bool ipv4 = await networkStatusHandler.GetIPv4NetworkStatusAsync();
                bool ipv6 = await networkStatusHandler.GetIPv6NetworkStatusAsync();
                bool dns = await networkStatusHandler.GetDNSNetworkStatusAsync();

                if (ipv4)
                {
                    Ipv4Kind = MaterialIconKind.CheckCircleOutline;
                    Ipv4KindColor = Brushes.Green;
                }
                else
                {
                    Ipv4Kind = MaterialIconKind.AlphaXCircleOutline;
                    Ipv4KindColor = Brushes.Red;
                }

                if (ipv6)
                {
                    Ipv6Kind = MaterialIconKind.CheckCircleOutline;
                    Ipv6KindColor = Brushes.Green;
                }
                else
                {
                    Ipv6Kind = MaterialIconKind.AlphaXCircleOutline;
                    Ipv6KindColor = Brushes.Red;
                }

                if (dns)
                {
                    DnsKind = MaterialIconKind.CheckCircleOutline;
                    DnsKindColor = Brushes.Green;
                }
                else
                {
                    DnsKind = MaterialIconKind.AlphaXCircleOutline;
                    DnsKindColor = Brushes.Red;
                }

                await Task.Delay(5000);
            }
        }

        private async void LoadChangelog()
        {
            try
            {
                if (!HasUpdatesBeenChecked)
                {
                    GitHubRequestHandler handler = new();
                    Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());

                    if (Response.VersionInfo.Find(a => a.Build == _globalSettings.CurrentBuild) != null)
                    {
                        GeneralNotes = await GetGeneralNotes();
                        NewFeatures = await GetNewFeatures();
                        BugFixes = await GetBugFixes();
                    }
                    else
                    {
                        GeneralNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                        NewFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                        BugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                    }
                }
            }
            catch (InvalidOperationException)
            {
                GeneralNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                NewFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                BugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }
            catch (HttpRequestException)
            {
                GeneralNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                NewFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                BugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }
            catch (TaskCanceledException)
            {
                GeneralNotes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                NewFeatures = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
                BugFixes = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }

            HasUpdatesBeenChecked = true;

            await GetNetworkStatusAsync();
        }

        private async Task<string> GetGeneralNotes()
        {
            var info = Response.VersionInfo.Find(a => a.Build == _globalSettings.CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.GeneralNotes)));
        }

        private async Task<string> GetNewFeatures()
        {
            var info = Response.VersionInfo.Find(a => a.Build == _globalSettings.CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.NewFeatures)));
        }

        private async Task<string> GetBugFixes()
        {

            var info = Response.VersionInfo.Find(a => a.Build == _globalSettings.CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.BugFixes)));
        }
        #endregion
    }
}
