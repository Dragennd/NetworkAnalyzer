using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
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
    private CancellationToken _timeoutToken = CancellationToken.None;
    private static readonly string _scriptFilePath = $@"{GlobalSettings.ConfigDirectory}\start-ssh.ps1";
    
    public PortHandler(string ipAddress, ScanType scanType, IPScannerData data, string portCollection)
    {
        Data = data;
        IPAddress = ipAddress;
        ScanType = scanType;
        CustomPortCollection = portCollection;
    }

    public async Task StartPortScanAsync()
    {
        if (ScanType == ScanType.Custom)
        {
            ConcurrentBag<string> temp = new();
            string[] ports = CustomPortCollection.Split(",");
            await Parallel.ForEachAsync(
                ports,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1
                },
                async (port, cancellationtoken) =>
                {
                    var scanResult = await ScanPortAsync(port.Trim()).ConfigureAwait(false);

                    if (scanResult != "N/A")
                    {
                        temp.Add(scanResult);
                    }
                });
            
            foreach (var item in temp)
            {
                Data.ActivePorts.Add(item);   
            }
        }
        else if (ScanType == ScanType.Common)
        {
            ConcurrentBag<string> temp = new();
            string[] ports = { "20", "21", "22", "23", "25", "69", "80", "115", "156", "443", "445", "547", "3389" };
            await Parallel.ForEachAsync(
                ports,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1
                },
                async (port, cancellationtoken) =>
                {
                    var scanResult = await ScanPortAsync(port.Trim()).ConfigureAwait(false);

                    if (scanResult != "N/A")
                    {
                        temp.Add(scanResult);
                    }
                });
            
            foreach (var item in temp)
            {
                Data.ActivePorts.Add(item);   
            }
        }
        else if (ScanType == ScanType.Full)
        {
            ConcurrentBag<string> temp = new();
            await Parallel.ForEachAsync(
                AsyncEnumerable.Range(1, 65535),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1
                },
                async (port, cancellationtoken) =>
                {
                    var scanResult = await ScanPortAsync(port.ToString()).ConfigureAwait(false);

                    if (scanResult != "N/A")
                    {
                        temp.Add(scanResult);
                    }
                });
            
            foreach (var item in temp)
            {
                Data.ActivePorts.Add(item);   
            }
        }
    }
    
    private async Task<string> ScanPortAsync(string port)
    {
        using var tcpClient = new TcpClient();
        using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_timeoutToken);
        
        timeoutTokenSource.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await tcpClient.ConnectAsync(IPAddress, int.Parse(port), timeoutTokenSource.Token).ConfigureAwait(false);
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