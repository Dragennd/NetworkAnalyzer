using NetworkAnalyzer.Functions;

namespace NetworkAnalyzer.Interfaces
{
    internal interface ITracerouteFactory
    {
        TracerouteWorker Create(string target, string reportID);
    }
}
