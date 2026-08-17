using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Functions;

internal class TracerouteWorker
{
    private string ReportID { get; set; }
    private string DisplayName { get; set; }
    private string TargetName { get; set; }
    private string TargetAddress { get; set; }
    private string TracerouteGUID { get; set; }
    private string CurrentTarget { get; set; }
    private int Hop { get; set; } = 1;
    private int MaxHops { get; set; }
    private bool EmergencyStop { get; set; } = false;
    private LatencyMonitorData TargetData { get; set; }
    private readonly LatencyMonitorController _latencyMonitorController = App.AppHost.Services.GetRequiredService<LatencyMonitorController>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    private readonly IDNSHandler _dnsHandler;

    public TracerouteWorker(string targetName, string reportID, IDNSHandler dnsHandler)
    {
        _dnsHandler = dnsHandler;
        DisplayName = targetName;
        ReportID = reportID;
        MaxHops = GlobalSettings.MaxHops;
        TracerouteGUID = Guid.NewGuid().ToString();
    }

    public async Task NewTracerouteDataAsync()
    {
        await SetTargetsAsync();

        var z = new TargetWorker(
            reportID: ReportID,
            displayName: DisplayName,
            targetName: TargetName,
            targetAddress: TargetAddress,
            tracerouteGUID: TracerouteGUID,
            isUserDefinedTarget: true,
            status: LatencyMonitorTargetStatus.Active
        );

        TargetData = await z.NewTargetDataAsync();

        if (TargetData.TargetStatus == LatencyMonitorTargetStatus.NoResponse)
        {
            _latencyMonitorController.SendSetSessionStatusRequest(LatencyMonitorSessionStatus.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    $"Target Failed: {TargetData.DisplayName}", 
                   "The target either failed to resolve or is incorrectly formatted. Review the specified target and try again.", 
                    NotificationType.Error));
            return;
        }

        _latencyMonitorController.SendSetLiveTargetRequest(TargetData);
        _latencyMonitorController.SendSetSelectedTargetRequest(TargetData);
        _latencyMonitorController.SetStopCode += SetEmergencyStop;

        do
        {
            var hopData = await GetNextHopDataAsync();
            CurrentTarget = hopData.Item1;

            if (CurrentTarget == TargetAddress)
            {
                TargetData.Hop = Hop;
            }
            else
            {
                var u = new TargetWorker(
                    reportID: ReportID,
                    displayName: CurrentTarget,
                    targetName: hopData.Item3,
                    targetAddress: CurrentTarget,
                    tracerouteGUID: TracerouteGUID,
                    isUserDefinedTarget: false,
                    hop: Hop,
                    status: hopData.Item2
                );

                _latencyMonitorController.SendSetTracerouteRequest(await u.NewTargetDataAsync());
            }

            Hop++;

            if (Hop > MaxHops)
            {
                _mainController.SendAddNotificationRequest(
                    new NotificationInfo(
                        $"Target Failed: {TargetData.DisplayName}", 
                       $"Max traceroute hops for {TargetData.DisplayName} in the current Latency Monitor session has been exceeded. One or more targets may be inaccessible.", 
                        NotificationType.Error));
                break;
            }

            if (EmergencyStop)
            {
                break;
            }
        } while (CurrentTarget != TargetAddress);

        _latencyMonitorController.SendSetTracerouteRequest(TargetData);
        _latencyMonitorController.SetStopCode -= SetEmergencyStop;
    }

    #region Private Methods
    private async Task<(string, LatencyMonitorTargetStatus, string)> GetNextHopDataAsync()
    {
        PingReply response;
        string target;
        string name;
        LatencyMonitorTargetStatus status;

        var options = new PingOptions()
        {
            Ttl = Hop,
            DontFragment = true,
        };

        using (var ping = new Ping())
        {
            response = await ping.SendPingAsync(TargetAddress, 4000, new byte[32], options);
        }

        if (response.Address.ToString() == "0.0.0.0")
        {
            target = "Request timed out";
        }
        else
        {
            target = response.Address.ToString();
        }

        using (var ping = new Ping())
        {
            try
            {
                response = await ping.SendPingAsync(target, 4000, new byte[32]);
            }
            catch (ArgumentException)
            {
                status = LatencyMonitorTargetStatus.NoResponse;
            }
            catch (PingException)
            {
                status = LatencyMonitorTargetStatus.NoResponse;
            }
        }

        if (response.Status == IPStatus.Success)
        {
            status = LatencyMonitorTargetStatus.Active;
            name = await _dnsHandler.GetDeviceNameAsync(target);
        }
        else if (response.Status != IPStatus.Success && target != "Request timed out")
        {
            status = LatencyMonitorTargetStatus.Inactive;
            name = await _dnsHandler.GetDeviceNameAsync(target);
        }
        else if (response.Status != IPStatus.Success && target == "Request timed out")
        {
            status = LatencyMonitorTargetStatus.NoResponse;
            name = "Request timed out";
        }
        else
        {
            status = LatencyMonitorTargetStatus.None;
            name = "Request timed out";
        }

        return (target, status, name);
    }

    private async Task SetTargetsAsync()
    {
        TargetName = await _dnsHandler.GetDeviceNameAsync(DisplayName);
        TargetAddress = await _dnsHandler.ResolveIPAddressFromDNSAsync(DisplayName);
    }

    private void SetEmergencyStop(bool stop)
    {
        EmergencyStop = stop;
    }
    #endregion Private Methods
}