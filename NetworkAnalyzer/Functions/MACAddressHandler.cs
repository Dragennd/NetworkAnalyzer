using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetworkAnalyzer.ExtensionMethods;
using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.Functions;

internal class MACAddressHandler : IMACAddressHandler
{
    private static readonly HttpClient _client = new();
        
    // Request MAC Address via ARP
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    public static extern int SendARP(int destIP, int srcIP, byte[] macAddr, ref uint hwAddrLength);

    public async Task<string> GetMACAddressAsync(string ipAddress)
    {
        // Prep all the data that goes into the ARP request
        IPAddress dIP = IPAddress.Parse(ipAddress);
        int dIPInt = BitConverter.ToInt32(dIP.GetAddressBytes(), 0);
        uint hwLength = 6;
        byte[] mac = new byte[hwLength];
        string[] macSegments = new string[hwLength];

        if (OperatingSystem.IsWindows())
        {
            // Send the ARP request to the destination IP Address
            if (await Task.Run(() => SendARP(dIPInt, 0, mac, ref hwLength) != 0))
            {
                return "-";
            }
            else
            {
                // Format the byte array into a string array containing segments of a MAC Address
                for (int i = 0; i < hwLength; i++)
                {
                    macSegments[i] = mac[i].ToString("x2");
                }

                return string.Join(":", macSegments);
            }   
        }

        if (OperatingSystem.IsLinux())
        {
            string macaddr;
            if ((macaddr = await GetLocalMACAddress(ipAddress)) != string.Empty)
            {
                return macaddr;
            }
            
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "ip",
                Arguments = $"neigh show {ipAddress}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            try
            {
                using var process = Process.Start(processStartInfo);

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var match = Regex.Match(output, @"lladdr\s+([0-9a-fA-F:]{17})");

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch (Exception)
            {
                return "-";
            }
        }

        return "-";
    }

    // Request Manufacturer info from api.maclookup.app
    public async Task<string> GetManufacturerAsync(string macAddress)
    {
        string apiResponse = "-";
        HttpResponseMessage response;

        // Send API call to request info from the API
        response = await _client.GetAsync($"https://api.maclookup.app/v2/macs/{macAddress}/company/name");

        // If the Response is good, assign the response to apiResponse
        if (response.IsSuccessStatusCode)
        {
            apiResponse = await response.Content.ReadAsStringAsync();

            // If the API response doesn't provide the manufacturer name
            // provide an empty string instead of the generic responses below
            if (apiResponse == "*NO COMPANY*" || apiResponse == "*PRIVATE*")
            {
                apiResponse = "-";
            }
        }

        if (apiResponse != null)
        {
            apiResponse = apiResponse.Replace(",", "");
        }

        return apiResponse;
    }

    private async Task<string> GetLocalMACAddress(string ipAddress)
    {
        // Get all of the network interfaces on the device
        var interfaceAddresses = 
            NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);

        // Parse the network interfaces for IPv4 addresses
        var filteredIPAddresses = interfaceAddresses
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address.ToString());

        // Check if the available IPv4 addresses on the device contain the provided IP Address
        if (filteredIPAddresses.Contains(ipAddress))
        {
            // If the device contains the provided IP Address, grab the
            // MAC Address from the network interface containing the IP Address
            // and return it
            return await Task.FromResult(NetworkInterface.GetAllNetworkInterfaces()
                    .Where(a => a.OperationalStatus == OperationalStatus.Up 
                                && a.GetIPProperties().UnicastAddresses.Where(b => 
                                    b.Address.AddressFamily == AddressFamily.InterNetwork).Select(c => 
                                     c.Address.ToString()).Contains(ipAddress))
                    .First().GetPhysicalAddress().ToString().FormatAsMacAddress());
        }
        
        return await Task.FromResult(string.Empty);
    }
}