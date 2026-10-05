using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Services;

internal class ReportService
{
    private string Filename { get; set; } = string.Empty;
    private readonly IDatabaseHandler _dbHandler;
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    
    public ReportService(
        IDatabaseHandler dbHandler)
    {
        _dbHandler = dbHandler;
    }
    
    public async Task NewReport(
        ReportExplorerData sessionData, 
        LatencyMonitorReportEntries userDefinedTarget, 
        bool isDateRangeChecked, 
        string startTime, 
        string endTime)
    {
        try
        {
            if (sessionData.Mode == ReportMode.LatencyMonitor)
            {
                LatencyMonitorHTMLReportHandler newLMReport;

                if (userDefinedTarget == null)
                {
                    return;
                }

                if (isDateRangeChecked)
                {
                    newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID, isDateRangeChecked, startTime, endTime);
                }
                else
                {
                    newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID);
                }

                Filename = await newLMReport.GenerateReport();
            }
            else if (sessionData.Mode == ReportMode.IPScanner)
            {
                IPScannerHTMLReportHandler newIPSReport = new(_dbHandler, sessionData.ReportGUID);

                Filename = await newIPSReport.GenerateReport();
            }

            if (VerifyReportGenerated(Filename))
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                    "Report Generated Successfully", 
                   $"Report {Filename.Split('\\')[3]} was generated successfully.", 
                        NotificationType.Info));

                OpenReportsDirectory();
            }
            else
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                    "Report Generation Failed",
                   $"Report {Filename.Split('\\')[3]} failed to generate. See logs for details.",
                        NotificationType.Error));
            }
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error);
        }
    }
    
    public void OpenReportsDirectory()
    {
        string fileProcess = string.Empty;

        if (OperatingSystem.IsLinux())
        {
            fileProcess = "dolphin";
            // To-Do: Add a check for other common Linux file viewer processes
        }

        if (OperatingSystem.IsWindows())
        {
            fileProcess = "explorer.exe";
        }
        
        // To-Do: Add a check for Mac
        
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            FileName = fileProcess,
            Arguments = $"{GlobalSettings.ReportDirectory}"
        };

        process.StartInfo = startInfo;
        process.Start();
    }
    
    private bool VerifyReportGenerated(string reportFilename) =>
        File.Exists(reportFilename);
}