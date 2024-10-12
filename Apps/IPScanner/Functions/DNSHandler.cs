using System.Net;
using System.Net.Sockets;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal static class DNSHandler
    {
        public static async Task<string> GetDeviceNameAsync(string ipAddress)
        {
            string? deviceName = string.Empty;

            try
            {
                // Attempt to resolve the hostname of the device
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                deviceName = hostEntry.HostName.ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                // If the hostname is too long,
                // return an empty string rather than throw an exception
                deviceName = string.Empty;
            }
            catch (SocketException)
            {
                // If the hostname couldn't be resolved,
                // return an empty string rather than throw an exception
                deviceName = string.Empty;
            }

            // Return the hostname as a string
            return deviceName;
        }

        public static async Task<string> ResolveIPAddressFromDNSAsync(string target)
        {
            IPHostEntry temp = await Dns.GetHostEntryAsync(target);
            return temp.AddressList.First(addr => addr.AddressFamily == AddressFamily.InterNetwork).ToString();
        }
    }
}