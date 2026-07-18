using NetworkAnalyzer_UI_Test.Functions;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface ITracerouteFactory
    {
        TracerouteWorker Create(string target, string reportID);
    }
}
