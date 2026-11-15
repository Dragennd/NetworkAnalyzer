using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class PortHandler
{
    private IPScannerData Data { get; set; }
    private string IPAddress { get; }
    private ScanType ScanType { get; }
    private string CustomPortCollection { get; }
    private static readonly string _scriptFilePath = $@"{GlobalSettings.ConfigDirectory}\start-ssh.ps1";
    
    public PortHandler(string ipAddress, ScanType scanType, IPScannerData data, [Optional]string portCollection)
    {
        Data = data;
        IPAddress = ipAddress;
        ScanType = scanType;
        CustomPortCollection = portCollection;
    }

    public async Task StartPortScanAsync()
    {
        // To-Do: Investigate what is causing the UI thread to be blocked, issue appears to be during the port scan
        if (ScanType == ScanType.Custom)
        {
            string[] ports = CustomPortCollection.Split(",");
            var tasks = ports.Select(port => ScanPortAsync(port.Trim()));
            var scanResults = await Task.WhenAll(tasks);
            
            foreach (var port in scanResults)
            {
                if (port != "N/A")
                {
                    Data.ActivePorts.Add(port);
                }
            }
        }
        else if (ScanType == ScanType.Common)
        {
            string[] ports = { "20", "21", "22", "23", "25", "69", "80", "115", "156", "443", "445", "547", "3389" };
            var tasks = ports.Select(port => ScanPortAsync(port));
            var scanResults = await Task.WhenAll(tasks);
            
            foreach (var port in scanResults)
            {
                if (port != "N/A")
                {
                    Data.ActivePorts.Add(port);
                }
            }
        }
        else if (ScanType == ScanType.Full)
        {
            var tasks = Enumerable.Range(1, 65535).Select(port => ScanPortAsync(port.ToString()));
            var scanResults = await Task.WhenAll(tasks);
            
            foreach (var port in scanResults)
            {
                if (port != "N/A")
                {
                    Data.ActivePorts.Add(port);
                }
            }
        }
    }
    
    private async Task<string> ScanPortAsync(string port)
    {
        using var tcpClient = new TcpClient();

        try
        {
            await tcpClient.ConnectAsync(IPAddress, int.Parse(port)).WaitAsync(TimeSpan.FromSeconds(5));
            return port;
        }
        catch
        {
            return "N/A";
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