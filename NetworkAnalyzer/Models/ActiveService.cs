using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;

namespace NetworkAnalyzer.Models;

internal class ActiveService
{
    public string ServiceName { get; set; }
    public string Port { get; }
    private string IPAddress { get; }
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();

    public ActiveService(string port, string ipAddress)
    {
        Port = port;
        IPAddress = ipAddress;
        
        SetServiceName();
    }

    public async Task LaunchServiceSessionAsync()
    {
        switch (Port)
        {
            case "22":
                await StartSSHSessionAsync();
                break;
            case "23":
                await StartTelnetSessionAsync();
                break;
            case "80":
                await StartHTTPSessionAsync();
                break;
            case "443":
                await StartHTTPSSessionAsync();
                break;
            case "445":
                await StartSMBSessionAsync();
                break;
            case "3389":
                await StartRDPSessionAsync();
                break;
            case "Ping":
                await StartPingSessionAsync();
                break;
            case "Traceroute":
                await StartTracerouteSessionAsync();
                break;
        };
    }
    
    private void SetServiceName()
    {
        ServiceName = Port switch
        {
            "22" => "SSH",
            "23" => "Telnet",
            "80" => "HTTP",
            "443" => "HTTPS",
            "445" => "SMB",
            "3389" => "RDP",
            "Ping" => "Ping",
            "Traceroute" => "Traceroute"
        };
    }
    
    private string GetActiveLinuxTerminal()
    {
        var activeLinuxDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        if (activeLinuxDesktop == "KDE")
        {
            return "konsole";
        }
        
        if (activeLinuxDesktop == "gnome")
        {
            return "gnome-terminal";
        }

        return "Unknown";
    }
    
    private async Task StartSSHSessionAsync()
    {
        string fileProcess = string.Empty;
        string arguments = string.Empty;
        
        try
        {
            if (OperatingSystem.IsLinux())
            {
                fileProcess = GetActiveLinuxTerminal();
                arguments = $"-e bash -c \"read -p 'Username: ' username; ssh \"$username\"@{IPAddress}; exec bash\"";
            }

            if (OperatingSystem.IsWindows())
            {
                fileProcess = "cmd.exe";
                arguments = $"/k \"set /p username=Username: ^& SSH %username%@{IPAddress}\"";
            }
        
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = fileProcess,
                Arguments = arguments,
                UseShellExecute = false
            };
        
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open Terminal",
                    $"An error occurred while opening the {fileProcess} Terminal. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartTelnetSessionAsync()
    {
        string fileProcess = string.Empty;
        string arguments = string.Empty;
        
        try
        {
            if (OperatingSystem.IsLinux())
            {
                fileProcess = GetActiveLinuxTerminal();
                arguments = $"-e bash -c \"telnet {IPAddress}; exec bash\"";
            }

            if (OperatingSystem.IsWindows())
            {
                fileProcess = "cmd.exe";
                arguments = $"/k telnet {IPAddress}";
            }
        
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = fileProcess,
                Arguments = arguments,
                UseShellExecute = false
            };
        
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open Terminal",
                    $"An error occurred while opening the {fileProcess} Terminal. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartHTTPSessionAsync()
    {
        try
        {
            Process process = new();
            ProcessStartInfo startInfo = new();
            startInfo.UseShellExecute = false;

            if (OperatingSystem.IsLinux())
            {
                startInfo.FileName = "xdg-open";
                startInfo.Arguments = $"http://{IPAddress}";
            }

            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = $"http://{IPAddress}";
            }

            if (OperatingSystem.IsMacOS())
            {
                startInfo.FileName = "open";
                startInfo.Arguments = $"http://{IPAddress}";
            }

            process.StartInfo = startInfo;
            process.Start();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open Web Browser",
                    $"An error occurred while opening the system Web Browser. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartHTTPSSessionAsync()
    {
        try
        {
            Process process = new();
            ProcessStartInfo startInfo = new();
            startInfo.UseShellExecute = false;

            if (OperatingSystem.IsLinux())
            {
                startInfo.FileName = "xdg-open";
                startInfo.Arguments = $"https://{IPAddress}";
            }

            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = $"https://{IPAddress}";
            }

            if (OperatingSystem.IsMacOS())
            {
                startInfo.FileName = "open";
                startInfo.Arguments = $"https://{IPAddress}";
            }

            process.StartInfo = startInfo;
            process.Start();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open Web Browser",
                    $"An error occurred while opening the system Web Browser. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartSMBSessionAsync()
    {
        try
        {
            string fileProcess = string.Empty;

            if (OperatingSystem.IsLinux())
            {
                fileProcess = "xdg-open";
            }

            if (OperatingSystem.IsWindows())
            {
                fileProcess = "explorer.exe";
            }

            if (OperatingSystem.IsMacOS())
            {
                fileProcess = "open";
            }
        
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                FileName = fileProcess,
                UseShellExecute = false
            };

            process.StartInfo = startInfo;
            process.Start();
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open File Browser",
                    $"An error occurred while opening the system File Browser. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartRDPSessionAsync()
    {
        try
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                FileName = "mstsc.exe",
                Arguments = $"/v: {IPAddress}"
            };
        
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();   
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to open RDP Client",
                    $"An error occurred while opening the system RDP Client. See logs for details.",
                    NotificationType.Error));
        }
    }
    
    private async Task StartPingSessionAsync()
    {
        // To-Do: Add logic to start a ping
    }
    
    private async Task StartTracerouteSessionAsync()
    {
        // To-Do: Add logic to start a traceroute
    }
}