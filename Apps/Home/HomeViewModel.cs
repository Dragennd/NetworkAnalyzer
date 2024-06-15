using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Management;
using CommunityToolkit.Mvvm.ComponentModel;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;
using static NetworkAnalyzer.Apps.GlobalClasses.ExtensionsHandler;

namespace NetworkAnalyzer.Apps.Home
{
    public partial class HomeViewModel : ObservableRecipient
    {
        #region Control Properties
        [ObservableProperty]
        public string buildID = CurrentBuild;

        [ObservableProperty]
        public string latestRelease = ReleaseDate;

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
        #endregion

        public HomeViewModel()
        {
            DeviceName = Environment.MachineName;
            CurrentUser = Environment.UserName;
            WindowsVersion = RuntimeInformation.OSDescription;
            BiosInfo = GetBIOSInfo();
            IpAddress = GetIPAddress();
            GatewayAddress = GetGatewayAddress();
            MacAddress = GetMACAddress();
        }

        private string GetBIOSInfo()
        {
            string biosManufacturer = string.Empty;

            using (ManagementObjectSearcher osDetails = new("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject item in osDetails.Get())
                {
                    biosManufacturer = $"{item["Manufacturer"].ToString()} v{item["SMBIOSMajorVersion"].ToString()}.{item["SMBIOSMinorVersion"].ToString()}";
                }
            }

            return biosManufacturer;
        }

        private string GetIPAddress()
        {
            var interfaceAddresses = NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);

            // Filter out the IPv6, APIPA and Link Local network interfaces
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
    }
}
