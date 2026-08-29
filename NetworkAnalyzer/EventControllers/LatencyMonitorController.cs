using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.EventControllers;

internal delegate void LatencyMonitorDataEventHandler(LatencyMonitorData data);
internal delegate void LatencyMonitorStringEventHandler(string data);
internal delegate void LatencyMonitorEmergencyStopEventHandler(bool stop);
internal delegate void LatencyMonitorSessionStatusEventHandler(LatencyMonitorSessionStatus status);

internal delegate void LatencyMonitorHistoryDataEventHandler(List<LatencyMonitorReportEntries> data);
internal delegate void LatencyMonitorHistoryReportEventHandler(LatencyMonitorReport report);
internal delegate void LatencyMonitorHistoryReportEntryEventHandler(ObservableCollection<LatencyMonitorReportEntries> reportEntries);
internal delegate void LatencyMonitorHistoryDistinctTargetEventHandler(FilterTargetData targetData);

internal class LatencyMonitorController
{
    public event LatencyMonitorDataEventHandler? SetSelectedTargetData;
    public event LatencyMonitorDataEventHandler? SetLiveTargetData;
    public event LatencyMonitorDataEventHandler? SetTracerouteData;
    public event LatencyMonitorDataEventHandler? UpdateLiveTargetData;
    public event LatencyMonitorDataEventHandler? UpdateTracerouteData;
    public event LatencyMonitorDataEventHandler? SetTracerouteTargets;
    public event LatencyMonitorStringEventHandler? SetSelectedTargetGuid;
    public event LatencyMonitorStringEventHandler RemoveFilter;
    public event LatencyMonitorEmergencyStopEventHandler? SetStopCode;
    public event LatencyMonitorSessionStatusEventHandler? SetSessionStatus;

    public event LatencyMonitorHistoryDataEventHandler? SetHistoryData;
    public event LatencyMonitorHistoryReportEventHandler? SetHistoryReport;
    public event LatencyMonitorHistoryReportEntryEventHandler? SetHistoryReportEntries;
    public event LatencyMonitorHistoryDistinctTargetEventHandler? SetHistoryDistinctTarget;

    public void SendSetSelectedTargetRequest(LatencyMonitorData data)
    {
        SetSelectedTargetData?.Invoke(data);
    }

    public void SendSetLiveTargetRequest(LatencyMonitorData data)
    {
        SetLiveTargetData?.Invoke(data);
    }

    public void SendSetTracerouteRequest(LatencyMonitorData data)
    {
        SetTracerouteData?.Invoke(data);
    }

    public void SendUpdateLiveTargetRequest(LatencyMonitorData data)
    {
        UpdateLiveTargetData?.Invoke(data);
    }

    public void SendUpdateTracerouteRequest(LatencyMonitorData data)
    {
        UpdateTracerouteData?.Invoke(data);
    }

    public void SendSetSelectedTargetGUIDRequest(string data)
    {
        SetSelectedTargetGuid?.Invoke(data);
    }

    public void SendStopCodeRequest(bool stop)
    {
        SetStopCode?.Invoke(stop);
    }

    public void SendSetTracerouteTargetsRequest(LatencyMonitorData data)
    {
        SetTracerouteTargets?.Invoke(data);
    }

    public void SendRemoveFilterRequest(string guid)
    {
        RemoveFilter?.Invoke(guid);
    }

    public void SendSetSessionStatusRequest(LatencyMonitorSessionStatus status)
    {
        SetSessionStatus?.Invoke(status);
    }
    
    public void SendHistoryDataRequest(List<LatencyMonitorReportEntries> data)
    {
        SetHistoryData?.Invoke(data);
    }

    public void SendSetHistoryReportRequest(LatencyMonitorReport report)
    {
        SetHistoryReport?.Invoke(report);
    }

    public void SendSetHistoryReportEntriesRequest(ObservableCollection<LatencyMonitorReportEntries> reportEntries)
    {
        SetHistoryReportEntries?.Invoke(reportEntries);
    }

    public void SendSetHistoryDistinctTargetRequest(FilterTargetData targetData)
    {
        SetHistoryDistinctTarget?.Invoke(targetData);
    }
}