using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAnalyzer.Functions;

internal class PortHandler
{
    private static string _scriptFilePath = $@"{GlobalSettings.ConfigDirectory}\start-ssh.ps1";
    private string _ipAddress;
    private int _port;
    
    public PortHandler(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }
    
    public async Task<bool> ScanPortAsync()
    {
        // To-Do: Create a "Standard" scan portfolio with the listed ports below
        // SSH: 22
        // FTP: 21 and 20
        // Telnet: 23
        // SMTP: 25
        // TFTP: 69
        // HTTP: 80
        // SFTP: 115
        // SQL Server: 156
        // HTTPS: 443
        // SMB: 445
        // DHCP Server: 547
        // RDP: 3389
        
        using var tcpClient = new TcpClient();

        try
        {
            await tcpClient.ConnectAsync(_ipAddress, _port).WaitAsync(TimeSpan.FromSeconds(5));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task StartRDPSessionAsync(string ipAddress)
    {
        // To-Do: Implement option for Mac and Linux to RDP alongside Windows
        
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            FileName = "mstsc.exe",
            Arguments = $"/v: {ipAddress}"
        };
        
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();
    }
    
    public static async Task StartSMBSessionAsync(string ipAddress)
    {
        // To-Do: Copy logic from App.xaml.cs for launching File Explorer or Dolphin depending on OS
        
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            FileName = "explorer.exe",
            Arguments = @$"\\{ipAddress}"
        };
        
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();
    }
    
    public static async Task StartSSHSessionAsync(string ipAddress)
    {
        // To-Do: Look for better alternative to activate an SSH session using the OS specific terminal

        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Normal,
            FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
            Arguments = $"-ExecutionPolicy bypass -File \"{_scriptFilePath}\" -NoExit",
            UseShellExecute = false
        };
        
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();
    }
}