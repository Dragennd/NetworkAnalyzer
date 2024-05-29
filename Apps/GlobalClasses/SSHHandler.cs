using System.Net.Sockets;
using System.Diagnostics;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class SSHHandler
    {
        public async Task<bool> ScanSSHPortAsync(string ipAddress)
        {
            int sshPort = 22;
            bool sshPortAvailable;

            TcpClient tcpClient = new();

            try
            {
                // Attempt to connect to port 22 to check if the device is listening for SSH
                await tcpClient.ConnectAsync(ipAddress, sshPort);
                sshPortAvailable = true;
            }
            catch (SocketException)
            {
                sshPortAvailable = false;
            }

            return sshPortAvailable;
        }

        public async Task StartSSHSessionAsync(string ipAddress)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                // Specify the arguments for starting the PowerShell window
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "powershell.exe",
                Arguments = $"ssh {ipAddress}",
                UseShellExecute = true
            };

            // Open a PowerShell window for SSH and request credentials to connect
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}