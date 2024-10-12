using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal static class SSHHandler
    {
        private static string ScriptFilePath { get; } = $"{ConfigDirectory}start-ssh.ps1";

        public static async Task<bool> ScanSSHPortAsync(string ipAddress)
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

        public static async Task StartSSHSessionAsync(string ipAddress)
        {
            await GenerateSSHScriptAsync(ipAddress);

            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                // Specify the arguments for starting the PowerShell window
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                Arguments = $"-ExecutionPolicy bypass -File \"{ScriptFilePath}\" -NoExit",
                UseShellExecute = false
            };

            // Open a PowerShell window for SSH and execute the SSH script
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
         }

        private static async Task GenerateSSHScriptAsync(string target)
        {
            await ConfirmConfigDirectoryExistsAsync();

            File.Delete(ScriptFilePath);

            StringBuilder sb = new();
            StreamWriter sw = new(ScriptFilePath, false, Encoding.Unicode);

            sb.AppendFormat("$target = \"{0}\"", target);
            sb.AppendLine();
            sb.AppendLine("$username = Read-Host \"Username\"");
            sb.AppendLine("Write-Host \"Connecting to $target with $username.\"");
            sb.AppendLine("Start-Sleep 1");
            sb.AppendLine("ssh $username@$target");

            sw.Write(sb);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        private static async Task ConfirmConfigDirectoryExistsAsync()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }
            });
        }
    }
}