using System.Net.Sockets;
using System.Diagnostics;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class RDPHandler
    {
        public async Task<bool> ScanRDPPortAsync(string ipAddress)
        {
            int rdpPort = 3389;
            bool rdpPortAvailable;

            TcpClient tcpClient = new();

            try
            {
                // Attempt to connect to port 3389 to check if the device is listening for RDP
                await tcpClient.ConnectAsync(ipAddress, rdpPort);
                rdpPortAvailable = true;
            }
            catch (SocketException)
            {
                rdpPortAvailable = false;
            }

            return rdpPortAvailable;
        }

        public static async Task StartRDPSessionAsync(string ipAddress)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                // Specify the arguments for launching RDP
                FileName = "mstsc.exe",
                Arguments = $"/v: {ipAddress}"
            };

            // Launch a RDP session and attempt to connect to the provided IP Address
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}