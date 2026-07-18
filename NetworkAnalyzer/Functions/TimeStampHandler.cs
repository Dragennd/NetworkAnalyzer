using System;
using System.Threading.Tasks;

namespace NetworkAnalyzer.Functions
{
    internal static class TimeStampHandler
    {
        // Return the current datetime 
        public static async Task<DateTime> CalculateTimeStampAsync()
        {
            return await Task.FromResult(DateTime.Now);
        }
    }
}
