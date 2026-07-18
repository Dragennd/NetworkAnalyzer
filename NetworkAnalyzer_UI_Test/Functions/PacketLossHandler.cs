using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.Functions
{
    internal static class PacketLossHandler
    {
        public static async Task<string> CalculateTotalPacketsLostAsync(IPStatus ipStatus, LatencyMonitorData data)
        {
            string response;

            if (ipStatus != IPStatus.Success)
            {
                if (int.TryParse(data.TotalPacketsLost, out int num))
                {
                    response = (num + 1).ToString();
                }
                else
                {
                    response = data.TotalPacketsLost;
                }
            }
            else
            {
                response = data.TotalPacketsLost;
            }

            return await Task.FromResult(response);
        }

        public static async Task<bool> CalculateFailedPingAsync(IPStatus ipStatus)
        {
            bool response;

            if (ipStatus == IPStatus.Success)
            {
                response = false;
            }
            else
            {
                response = true;
            }

            return await Task.FromResult(response);
        }
    }
}
