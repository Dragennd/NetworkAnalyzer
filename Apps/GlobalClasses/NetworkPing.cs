using System.Net.NetworkInformation;
using System.Text;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class NetworkPing
    {
        public static (IPStatus Status, int Latency) PingTest(string ipAddress)
        {
            Ping Ping = new Ping();
            PingOptions Options = new PingOptions();

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;

            PingReply Reply = Ping.Send(ipAddress, timeout, buffer, Options);

            return (Reply.Status, (int)Reply.RoundtripTime);
        }
    }
}
