using System.Diagnostics;
using System.Threading.Tasks;
using NetworkAnalyzer.Functions;

namespace NetworkAnalyzer.Models;

internal class ActiveService
{
    public string ServiceName { get; set; }
    public string Port { get; }
    private static readonly string _scriptFilePath = $@"{GlobalSettings.ConfigDirectory}\start-ssh.ps1";

    public ActiveService(string port)
    {
        Port = port;
        
        SetServiceName();
    }
    
    public async Task StartRDPSessionAsync(string ipAddress)
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
    
    public async Task StartSMBSessionAsync(string ipAddress)
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
    
    public async Task StartSSHSessionAsync(string ipAddress)
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

    private void SetServiceName()
    {
        ServiceName = Port switch
        {
            "21" => "FTP",
            "22" => "SSH",
            "23" => "Telnet",
            "69" => "TFTP",
            "80" => "HTTP",
            "443" => "HTTPS",
            "3389" => "RDP",
            "Ping" => "Ping",
            "Traceroute" => "Traceroute"
        };
    }
}