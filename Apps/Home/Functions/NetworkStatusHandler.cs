using System.ComponentModel;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.Home.Functions
{
    internal class NetworkStatusHandler
    {
        public async Task<bool> GetIPv4NetworkStatusAsync()
        {
            using Ping ping = new();
            bool status = true;

            try
            {
                // Check IPv4 against 8.8.8.8 (Google DNS Servers) and return true if successful
                PingReply response = await ping.SendPingAsync("8.8.8.8", 1000);

                if (response.Status == IPStatus.Success)
                {

                }
                else
                {
                    // If no IPv4 response, return false
                    status = false;
                }
            }
            catch (Win32Exception)
            {
                status = false;
            }
            catch (PingException)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }

        public async Task<bool> GetIPv6NetworkStatusAsync()
        {
            using Ping ping = new();
            bool status = true;

            try
            {
                // Check IPv6 against 2001:4860:4860::8888 (Google DNS Servers) and return true if successful
                PingReply response = await ping.SendPingAsync("2001:4860:4860:0:0:0:0:8888", 1000);

                if (response.Status == IPStatus.Success)
                {

                }
                else
                {
                    // If no IPv6 response, return false
                    status = false;
                }
            }
            catch (Win32Exception)
            {
                status = false;
            }
            catch (PingException)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }

        public async Task<bool> GetDNSNetworkStatusAsync()
        {
            using Ping ping = new();
            bool status = true;

            try
            {
                // Check DNS against www.google.com and return true if successful
                PingReply response = await ping.SendPingAsync("www.google.com", 1000);

                if (response.Status == IPStatus.Success)
                {
                    status = true;
                }
                else
                {
                    // If no DNS response, return false
                    status = false;
                }
            }
            catch (Win32Exception)
            {
                status = false;
            }
            catch (PingException)
            {
                status = false;
            }

            return await Task.FromResult(status);
        }
    }
}
