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
    private readonly DatabaseHandler _dbHandler;
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    
    public ReportService(DatabaseHandler dbHandler)
    {
        _dbHandler = dbHandler;
    }
    
    public async Task NewReport(
        ReportExplorerData sessionData, 
        LatencyMonitorReportEntries? userDefinedTarget,
        bool isDateRangeChecked, 
        string startTime, 
        string endTime,
        string reportGenerationMode)
    {
        try
        {
            if (sessionData.Mode == ReportMode.LatencyMonitor)
            {
                ILatencyMonitorReportHandler newLMReport;

                if (userDefinedTarget == null)
                {
                    return;
                }

                if (reportGenerationMode == "CSV")
                {
                    if (isDateRangeChecked)
                    {
                        newLMReport = new LatencyMonitorCSVReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID, isDateRangeChecked, startTime, endTime);
                    }
                    else
                    {
                        newLMReport = new LatencyMonitorCSVReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID);
                    }
                }
                else
                {
                    if (isDateRangeChecked)
                    {
                        newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID, isDateRangeChecked, startTime, endTime);
                    }
                    else
                    {
                        newLMReport = new LatencyMonitorHTMLReportHandler(_dbHandler, sessionData.ReportGUID, userDefinedTarget.TracerouteGUID);
                    }
                }

                Filename = await newLMReport.GenerateReport();
            }
            else if (sessionData.Mode == ReportMode.IPScanner)
            {
                IIPScannerReportHandler newIPSReport;

                if (reportGenerationMode == "CSV")
                {
                    newIPSReport = new IPScannerCSVReportHandler(_dbHandler, sessionData.ReportGUID);
                }
                else
                {
                    newIPSReport = new IPScannerHTMLReportHandler(_dbHandler, sessionData.ReportGUID);
                }
                
                Filename = await newIPSReport.GenerateReport();
            }

            if (VerifyReportGenerated(Path.Combine(GlobalSettings.ReportDirectory, Filename)))
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                    "Report Generated Successfully", 
                   $"Report {Filename} was generated successfully.", 
                        NotificationType.Info));

                await OpenReportsDirectoryAsync();
            }
            else
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                    "Report Generation Failed",
                   $"Report {Filename} failed to generate. See logs for details.",
                        NotificationType.Error));
            }
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error);
        }
    }
    
    public async Task OpenReportsDirectoryAsync()
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
        
            startInfo.ArgumentList.Add(GlobalSettings.ReportDirectory);

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
    
    private bool VerifyReportGenerated(string reportFilename) =>
        File.Exists(reportFilename);
}