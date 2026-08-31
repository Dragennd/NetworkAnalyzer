using System;
using NetworkAnalyzer.Enums;

namespace NetworkAnalyzer.Models;

public class FilterTargetData
{
    public string UserDefinedTargetAddress { get; set; }
    public string? UserDefinedTargetName { get; set; }
    public string TracerouteTargetAddress { get; set; }
    public string? TracerouteTargetName { get; set; }
    public string UserDefinedTargetGUID { get; set; }
    public string TracerouteTargetGUID { get; set; }
    public string GUID { get; set; }

    public FilterTargetData(
        string userDefinedTargetAddress, 
        string userDefinedTargetName, 
        string tracerouteTargetAddress, 
        string tracerouteTargetName,
        string userDefinedTargetGUID, 
        string tracerouteTargetGUID)
    {
        UserDefinedTargetAddress = userDefinedTargetAddress;
        UserDefinedTargetName = userDefinedTargetName;
        TracerouteTargetAddress = tracerouteTargetAddress;
        TracerouteTargetName = tracerouteTargetName;
        UserDefinedTargetGUID = userDefinedTargetGUID;
        TracerouteTargetGUID = tracerouteTargetGUID;
        GUID = Guid.NewGuid().ToString();
    }
}