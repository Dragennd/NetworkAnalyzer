using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
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
        private readonly ILatencyMonitorController _latencyMonitorController;
        private readonly IDNSHandler _dnsHandler;
        private readonly GlobalSettings _settings = App.AppHost.Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

        public TracerouteWorker(string targetName, string reportID, ILatencyMonitorController latencyMonitorController, IDNSHandler dnsHandler)
        {
            _latencyMonitorController = latencyMonitorController;
            _dnsHandler = dnsHandler;
            DisplayName = targetName;
            ReportID = reportID;
            MaxHops = _settings.MaxHops;
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
                _latencyMonitorController.SendErrorMessage(LogType.Error, $"Target Failed: {TargetData.DisplayName}\n\nThe target either failed to resolve or is incorrectly formatted.\nReview the specified target and try again.");
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
                    _latencyMonitorController.SendErrorMessage(LogType.Error, "Max traceroute hops has been exceeded. One or more targets may be inaccessible.");
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
}
