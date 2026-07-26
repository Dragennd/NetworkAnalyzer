using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.Functions
{
    internal class SMBHandler : ISMBHandler
    {
        public async Task<bool> ScanSMBPortAsync(string ipAddress)
        {
            int smbPort = 445;
            using var tcpClient = new TcpClient();

            try
            {
                // Attempt to connect to port 445 to check if the device is listening for SMB
                await tcpClient.ConnectAsync(ipAddress, smbPort).WaitAsync(TimeSpan.FromSeconds(5));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task StartSMBSessionAsync(string ipAddress)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                // Specify arguments for launching File Explorer
                FileName = "explorer.exe",
                Arguments = @$"\\{ipAddress}"
            };

            // Launch File Explorer and attempt to connect to the root shares directory
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}