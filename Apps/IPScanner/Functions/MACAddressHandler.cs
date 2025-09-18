using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class MACAddressHandler : IMACAddressHandler
    {
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

            // Send the ARP request to the destination IP Address
            if (await Task.Run(() => SendARP(dIPInt, 0, mac, ref hwLength) != 0))
            {
                return string.Empty;
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

        // Request Manufacturer info from api.maclookup.app
        public async Task<string> GetManufacturerAsync(string macAddress)
        {
            string apiResponse = null;
            HttpClient client = new();
            HttpResponseMessage response;

            // Send API call to request info from the API
            response = await client.GetAsync($"https://api.maclookup.app/v2/macs/{macAddress}/company/name");

            // If the Response is good, assign the response to apiResponse
            if (response.IsSuccessStatusCode)
            {
                apiResponse = await response.Content.ReadAsStringAsync();

                // If the API response doesn't provide the manufacturer name
                // provide an empty string instead of the generic responses below
                if (apiResponse == "*NO COMPANY*" || apiResponse == "*PRIVATE*")
                {
                    apiResponse = string.Empty;
                }
            }

            if (apiResponse != null)
            {
                apiResponse = apiResponse.Replace(",", "");
            }

            return apiResponse;
        }
    }
}