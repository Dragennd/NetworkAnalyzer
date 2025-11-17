using System.Net;
using System.Net.Sockets;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class DNSHandler : IDNSHandler
    {
        public async Task<string> GetDeviceNameAsync(string ipAddress)
        {
            string? deviceName;

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

        public async Task<string> ResolveIPAddressFromDNSAsync(string target)
        {
            string resolvedIPAddress = string.Empty;

            try
            {
                IPHostEntry temp = await Dns.GetHostEntryAsync(target);
                var address = temp.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);

                if (address != null && IPAddress.TryParse(address.ToString(), out _))
                {
                    resolvedIPAddress = address.ToString();
                }
                else
                {
                    if (IPAddress.TryParse(target, out _))
                    {
                        resolvedIPAddress = target;
                    }
                    else
                    {
                        resolvedIPAddress = "N/A";
                    }
                }
            }
            catch (SocketException)
            {
                // If the target couldn't be resolved
                // attempt to parse it as an IP Address
                // return "N/A" rather than throw an exception
                if (IPAddress.TryParse(target, out _))
                {
                    resolvedIPAddress = target;
                }
                else
                {
                    resolvedIPAddress = "N/A";
                }
            }
            catch (ArgumentException)
            {
                // If the IP Address couldn't be resolved
                // return "N/A" rather than throw an exception
                resolvedIPAddress = "N/A";
            }

            return resolvedIPAddress;
        }
    }
}