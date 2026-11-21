using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces;

internal interface IIPScannerReportHandler
{
    public Task<string> GenerateReport();
}