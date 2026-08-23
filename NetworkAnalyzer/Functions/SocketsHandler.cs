using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class SocketsHandler
{
    
    private LogHandler _logHandler;
    
    public SocketsHandler(LogHandler logHandler)
    {
        _logHandler = logHandler;
    }
    
    public async Task<bool> GrantSocketAccessAsync(string executablePath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pkexec",
            UseShellExecute = false,
            RedirectStandardError = true
        };
        
        processStartInfo.ArgumentList.Add("setcap");
        processStartInfo.ArgumentList.Add("cap_net_raw+ep");
        processStartInfo.ArgumentList.Add(executablePath);
        
        using var process = Process.Start(processStartInfo);

        if (process == null)
            return false;
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            await _logHandler.CreateLogEntry(await process.StandardError.ReadToEndAsync(), LogType.Error);
            return false;
        }
        
        // To-Do: Add logic to handle if the user's desktop environment doesn't have pkexec
        // and inform the user to run the command manually with sudo instead

        return true;
    }

    public async Task<bool> GetPkexecStatus() => 
        File.Exists("/usr/bin/pkexec") || File.Exists("/bin/pkexec");
}