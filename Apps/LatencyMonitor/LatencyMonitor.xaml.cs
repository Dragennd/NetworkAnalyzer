using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public partial class LatencyMonitor : UserControl
    {
        public LatencyMonitor()
        {
            InitializeComponent();

            // Assign the Target Name fields to the MouseEnter event so their DNS Host Entry is dynamically updated
            TxtIPInfo1S.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo1D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo2S.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo2D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo3S.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo3D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo4S.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo4D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo5S.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo5D.MouseEnter += TargetName_MouseEnter;
        }

        // Generate a tooltip containing the DNS Host Entry
        private async void TargetName_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                string ipAddress = await ResolveDNSHostEntryAsync(textBox.Text);

                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    ToolTip toolTip = new()
                    {
                        Style = (Style)FindResource("DarkModeInfoToolTip"),
                        Content = $"Resolved IP Address\n{ipAddress}"
                    };

                    ToolTipService.SetToolTip(textBox, toolTip);
                }
            }
            catch (SocketException)
            {
                // Do nothing
                // If the address can't be resolved, just don't display one
            }
        }

        private async Task<string> ResolveDNSHostEntryAsync(string textBoxText)
        {
            IPHostEntry temp =  await Dns.GetHostEntryAsync(textBoxText);
            return temp.AddressList.First(addr => addr.AddressFamily == AddressFamily.InterNetwork).ToString();
        }
    }
}
