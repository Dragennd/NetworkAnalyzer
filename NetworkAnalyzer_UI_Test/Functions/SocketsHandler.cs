using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.Functions;

internal class SocketsHandler
{
    private string ExecutablePath { get; }
    private LogHandler _logHandler;
    
    public SocketsHandler(LogHandler logHandler)
    {
        ExecutablePath = Environment.ProcessPath!;
        _logHandler = logHandler;
    }
    
    public async Task<bool> GrantSocketAccessAsync()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pkexec",
            UseShellExecute = false,
            RedirectStandardError = true
        };
        
        processStartInfo.ArgumentList.Add("setcap");
        processStartInfo.ArgumentList.Add("cap_net_raw+ep");
        processStartInfo.ArgumentList.Add(ExecutablePath);
        
        using var process = Process.Start(processStartInfo);

        if (process == null)
            return false;
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            await _logHandler.CreateLogEntry(await process.StandardError.ReadToEndAsync(), LogType.Error);
            return false;
        }

        return true;
    }
}