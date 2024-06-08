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

        private void TargetName_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                string ipAddress = Dns.GetHostEntry(textBox.Text).AddressList.First(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    ToolTip toolTip = new();
                    toolTip.Style = (Style)FindResource("DarkModeInfoToolTip");
                    toolTip.Content = $"Resolved IP Address\n{ipAddress}";
                    ToolTipService.SetToolTip(textBox, toolTip);
                }
            }
            catch (SocketException)
            {
                // Do nothing
                // If the address can't be resolved, just don't display one
            }
        }
    }
}
