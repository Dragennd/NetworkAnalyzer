using System.Runtime.InteropServices;
using System.Net;
using System.Net.Http;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal static class MACAddressHandler
    {
        // Request MAC Address via ARP
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int destIP, int srcIP, byte[] macAddr, ref uint hwAddrLength);
        public static async Task<string> GetMACAddress(string ipAddress)
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
        public static async Task<string> SendAPIRequestAsync(string macAddress)
        {
            string apiResponse = null;
            HttpClient client = new();
            HttpResponseMessage response;

            do
            {
                // Send API call to request info from the API
                response = await client.GetAsync($"https://api.maclookup.app/v2/macs/{macAddress}/company/name");

                // If too many requests are sent at once, a short cooldown period is required
                // Wait for 1 second before attempting again
                // Max requests: 2 requests/sec
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(1000);
                }
            } while (response.StatusCode == HttpStatusCode.TooManyRequests);

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