using NetworkAnalyzer.Apps.LatencyMonitor;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class Notifications
    {
        public static string ProcessResponseCodes(DataStore.ResponseCode responsecode)
        {
            switch (responsecode)
            {
                case DataStore.ResponseCode.Empty_Input_Exception:
                    return "Entry cannot be blank.";

                case DataStore.ResponseCode.Input_Less_Than_Starting_Port_Exception:
                    return "Ending Port cannot be less than the Starting Port.";

                case DataStore.ResponseCode.Invalid_IP_Address_Exception:
                    return "One or more IP Addresses are incorrectly formatted or a DNS name cannot be resolved.";

                case DataStore.ResponseCode.Empty_Data_Collection_Exception:
                    return "No data to generate report with. Please run the Latency Monitor first.";

                default:
                    return null;
            }
        }
    }
}
