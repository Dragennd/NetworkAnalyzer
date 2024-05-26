using NetworkAnalyzer.Apps.LatencyMonitor;
using System.Net;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class DataValidation
    {
        public static string ValidateAtLeastOneFieldHasData(string[] userInput)
        {
            int counter = 0;

            for (int i = 0; i < userInput.Length; i++)
            {
                if (string.IsNullOrEmpty(userInput[i]))
                {
                    counter++;
                }
            }

            if (counter == 5)
            {
                return Notifications.ProcessResponseCodes(DataStore.ResponseCode.EmptyInputException);
            }
            else
            {
                return Notifications.ProcessResponseCodes(DataStore.ResponseCode.DataIsValid);
            }
        }

        public static string ValidateIPAddress(string ipAddress)
        {
            bool ValidateIP = IPAddress.TryParse(ipAddress, out _);
            var countOctets = ipAddress.Split('.');

            if (ValidateIP && countOctets.Length == 4)
            {
                return Notifications.ProcessResponseCodes(DataStore.ResponseCode.DataIsValid);
            }
            else
            {
                try
                {
                    var hostEntry = Dns.GetHostEntry(ipAddress).AddressList.First(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    return Notifications.ProcessResponseCodes(DataStore.ResponseCode.DataIsValid);
                }
                catch
                {
                    return Notifications.ProcessResponseCodes(DataStore.ResponseCode.InvalidIPAddressException);
                }
            }
        }

        public static void ResolveDNSNameForTooltip(string dnsName)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(dnsName).AddressList.First(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                DataStore.ResolvedName.Add(hostEntry);
            }
            catch
            {
                string hostEntry = "0.0.0.0";
                IPAddress.TryParse(hostEntry, out IPAddress emptyAddress);

                DataStore.ResolvedName.Add(emptyAddress);
            }
        }

        public static string ValidateReportContent()
        {
            if (!DataStore.LiveData.Any())
            {
                return Notifications.ProcessResponseCodes(DataStore.ResponseCode.EmptyDataCollectionException);
            }
            else
            {
                return Notifications.ProcessResponseCodes(DataStore.ResponseCode.DataIsValid);
            }
        }
    }
}
