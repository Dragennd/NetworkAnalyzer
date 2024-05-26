using NetworkAnalyzer.Apps.LatencyMonitor;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class Notifications
    {
        public static string ProcessResponseCodes(DataStore.ResponseCode responsecode)
        {
            switch (responsecode)
            {
                case DataStore.ResponseCode.EmptyInputException:
                    return "Entry cannot be blank.";

                case DataStore.ResponseCode.InputLessThanStartingPortException:
                    return "Ending Port cannot be less than the Starting Port.";

                case DataStore.ResponseCode.InvalidIPAddressException:
                    return "One or more IP Addresses are incorrectly formatted or a DNS name cannot be resolved.";

                case DataStore.ResponseCode.EmptyDataCollectionException:
                    return "No data to generate report with. Please run the Latency Monitor first.";

                default:
                    return null;
            }
        }
    }
}
