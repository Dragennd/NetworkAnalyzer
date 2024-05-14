using System.Net;
using System.Net.Sockets;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class DNSHandler
    {
        public static async Task<string> GetDeviceNameAsync(string ipAddress)
        {
            string? deviceName = null;

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