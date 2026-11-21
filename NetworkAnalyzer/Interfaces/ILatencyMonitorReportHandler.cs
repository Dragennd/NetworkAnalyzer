using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces;

internal interface ILatencyMonitorReportHandler
{
    public Task<string> GenerateReport();
}