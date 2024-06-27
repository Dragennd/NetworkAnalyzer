using System.Net.Sockets;
using System.Diagnostics;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class SMBHandler
    {
        public async Task<bool> ScanSMBPortAsync(string ipAddress)
        {
            int smbPort = 445;
            bool smbPortAvailable;

            TcpClient tcpClient = new();

            try
            {
                // Attempt to connect to port 445 to check if the device is listening for SMB
                await tcpClient.ConnectAsync(ipAddress, smbPort);
                smbPortAvailable = true;
            }
            catch (SocketException)
            {
                smbPortAvailable = false;
            }

            return smbPortAvailable;
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