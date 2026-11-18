using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class PortHandler
{
    private IPScannerData Data { get; set; }
    private string IPAddress { get; }
    private ScanType ScanType { get; }
    private string CustomPortCollection { get; }
    List<string> Services = new() { "22", "23", "80", "443", "445", "3389" };
    private readonly CancellationToken _timeoutToken = CancellationToken.None;
    private readonly CancellationTokenSource _portScanCTS = new();
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly IPScannerController _ipScannerController = App.AppHost.Services.GetRequiredService<IPScannerController>();

    public PortHandler(string ipAddress, ScanType scanType, IPScannerData data, string portCollection)
    {
        Data = data;
        IPAddress = ipAddress;
        ScanType = scanType;
        CustomPortCollection = portCollection;

        _ipScannerController.SetEmergencyStopStatus += CancelScan;
    }

    public async Task StartPortScanAsync()
    {
        if (ScanType == ScanType.Custom)
        {
            try
            {
                ConcurrentBag<string> temp = new();
                string[] ports = CustomPortCollection.Split(",");
                await Parallel.ForEachAsync(
                    ports,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 1,
                        CancellationToken = _portScanCTS.Token
                    },
                    async (port, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var scanResult = await ScanPortAsync(port.Trim()).ConfigureAwait(false);

                        if (scanResult != "N/A")
                        {
                            temp.Add(scanResult);
                        }
                    });
            
                foreach (var item in temp)
                {
                    Data.ActivePorts.Add(item);

                    if (Services.Any(a => a == item))
                    {
                        Data.ActiveServices.Add(new ActiveService(item, IPAddress));
                    }
                }
                
                Data.ActiveServices.Add(new ActiveService("Ping", IPAddress));
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the cancellation should bubble up and processed out in the View Model
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
                await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            }
            finally
            {
                _portScanCTS.Dispose();
                _ipScannerController.SetEmergencyStopStatus -= CancelScan;
            }
        }
        else if (ScanType == ScanType.Common)
        {
            try
            {
                ConcurrentBag<string> temp = new();
                string[] ports = { "21", "22", "23", "25", "69", "80", "115", "156", "443", "445", "547", "3389" };
                await Parallel.ForEachAsync(
                    ports,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 1,
                        CancellationToken = _portScanCTS.Token
                    },
                    async (port, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var scanResult = await ScanPortAsync(port.Trim()).ConfigureAwait(false);

                        if (scanResult != "N/A")
                        {
                            temp.Add(scanResult);
                        }
                    });

                foreach (var item in temp)
                {
                    Data.ActivePorts.Add(item);

                    if (Services.Any(a => a == item))
                    {
                        Data.ActiveServices.Add(new ActiveService(item, IPAddress));
                    }
                }

                Data.ActiveServices.Add(new ActiveService("Ping", IPAddress));
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the cancellation should bubble up and processed out in the View Model
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
                await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            }
            finally
            {
                _portScanCTS.Dispose();
                _ipScannerController.SetEmergencyStopStatus -= CancelScan;
            }
        }
        else if (ScanType == ScanType.Full)
        {
            try
            {
                ConcurrentBag<string> temp = new();
                await Parallel.ForEachAsync(
                    AsyncEnumerable.Range(1, 65535),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 1
                    },
                    async (port, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var scanResult = await ScanPortAsync(port.ToString()).ConfigureAwait(false);

                        if (scanResult != "N/A")
                        {
                            temp.Add(scanResult);
                        }
                    });

                foreach (var item in temp)
                {
                    Data.ActivePorts.Add(item);

                    if (Services.Any(a => a == item))
                    {
                        Data.ActiveServices.Add(new ActiveService(item, IPAddress));
                    }
                }

                Data.ActiveServices.Add(new ActiveService("Ping", IPAddress));
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the cancellation should bubble up and processed out in the View Model
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
                await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            }
            finally
            {
                _portScanCTS.Dispose();
                _ipScannerController.SetEmergencyStopStatus -= CancelScan;
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

    private void CancelScan(bool cancel)
    {
        if (cancel)
        {
            _portScanCTS.Cancel();   
        }
    }
}