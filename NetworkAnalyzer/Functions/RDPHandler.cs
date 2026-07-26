using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.Functions
{
    internal class RDPHandler : IRDPHandler
    {
        public async Task<bool> ScanRDPPortAsync(string ipAddress)
        {
            int rdpPort = 3389;
            using var tcpClient = new TcpClient();

            try
            {
                // Attempt to connect to port 3389 to check if the device is listening for RDP
                await tcpClient.ConnectAsync(ipAddress, rdpPort).WaitAsync(TimeSpan.FromSeconds(5));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task StartRDPSessionAsync(string ipAddress)
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