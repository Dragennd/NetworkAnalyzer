using System.Net;
using System.Net.Sockets;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class DNSHandler
    {
        public async Task<string> GetDeviceNameAsync(string ipAddress)
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
    }
}