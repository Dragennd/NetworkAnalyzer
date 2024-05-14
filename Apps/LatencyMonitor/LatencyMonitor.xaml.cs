using NetworkAnalyzer.Apps.GlobalClasses;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public partial class LatencyMonitor : UserControl
    {
        public bool SimpleMode { get; set; } = true;
        public static bool IsRunning { get; set; } = false;

        public LatencyMonitor()
        {
            InitializeComponent();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            DataStore.ClearDataStorage();

            if (!CheckUserInput())
            {
                return;
            }

            ProcessStateChange();

            do
            {
                DataStore.PacketsSent++;

                foreach (string ipAddress in DataStore.IPAddresses)
                {
                    if (!DataStore.LiveData.ContainsKey(ipAddress))
                    {
                        await Task.Run(() => LatencyMonitorFunction.InitializeData(ipAddress));
                        UpdateUserInterface(ipAddress);
                        continue;
                    }

                    await Task.Run(() => LatencyMonitorFunction.ProcessData(ipAddress));
                    UpdateUserInterface(ipAddress);
                }

                await Task.Delay(1000);
            } while (IsRunning);

            foreach (string ipAddress in DataStore.IPAddresses)
            {
                await Task.Run(() => LatencyMonitorReport.ProcessFinalEntry(ipAddress));
                UpdateUserInterface(ipAddress);
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e) => ProcessStateChange();

        private void BtnModeSwitcher_Click(object sender, RoutedEventArgs e)
        {
            // If Detailed Mode is active do this
            if (SimpleMode == false)
            {
                GridDetailedMode.Visibility = Visibility.Hidden;
                GridSimpleMode.Visibility = Visibility.Visible;

                SimpleMode = true;
            }
            // If Simple Mode is active do this
            else
            {
                GridDetailedMode.Visibility = Visibility.Visible;
                GridSimpleMode.Visibility = Visibility.Hidden;

                SimpleMode = false;
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string responseCodeMessage;

            if ((responseCodeMessage = DataValidation.ValidateReportContent()) != null)
            {
                MessageBox.Show(responseCodeMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            else
            {
                LatencyMonitorReport.GenerateHTMLReport();
                MessageBox.Show("Report has been created at C:\\BWIT\\" + LatencyMonitorReport.GenerateReportNumber() + ".html", "Report Created", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

                try
                {
                    Process.Start("explorer.exe", @"C:\BWIT");
                }
                catch
                {

                }
            }
        }

        public void UpdateUserInterface(string ipAddress)
        {
            var lastDataSet = DataStore.LiveData[ipAddress].LastOrDefault();

            TxtPacketsSent.Text = DataStore.PacketsSent.ToString();

            TxtIPInfo1S.Text = TxtIPAddress01.Text;
            TxtIPInfo2S.Text = TxtIPAddress02.Text;
            TxtIPInfo3S.Text = TxtIPAddress03.Text;
            TxtIPInfo4S.Text = TxtIPAddress04.Text;
            TxtIPInfo5S.Text = TxtIPAddress05.Text;
            TxtIPInfo1D.Text = TxtIPAddress01.Text;
            TxtIPInfo2D.Text = TxtIPAddress02.Text;
            TxtIPInfo3D.Text = TxtIPAddress03.Text;
            TxtIPInfo4D.Text = TxtIPAddress04.Text;
            TxtIPInfo5D.Text = TxtIPAddress05.Text;

            if (lastDataSet.IPAddress == TxtIPInfo1S.Text)
            {
                TxtIPInfo1S.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[0].ToString();
                TxtIPInfo1D.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[0].ToString();
                TxtStatusInfo1S.Text = lastDataSet.ConnectionStatus;
                TxtLatencyInfo1S.Text = lastDataSet.Latency.ToString();
                TxtStatusInfo1D.Text = lastDataSet.ConnectionStatus;
                TxtLowestInfo1D.Text = lastDataSet.LowestLatency.ToString();
                TxtHighestInfo1D.Text = lastDataSet.HighestLatency.ToString();
                TxtAverageInfo1D.Text = lastDataSet.AverageLatency.ToString();
                TxtPacketsLostInfo1D.Text = lastDataSet.PacketsLostTotal.ToString();
            }
            else if (lastDataSet.IPAddress == TxtIPInfo2S.Text)
            {
                TxtIPInfo2S.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[1].ToString();
                TxtIPInfo2D.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[1].ToString();
                TxtStatusInfo2S.Text = lastDataSet.ConnectionStatus;
                TxtLatencyInfo2S.Text = lastDataSet.Latency.ToString();
                TxtStatusInfo2D.Text = lastDataSet.ConnectionStatus;
                TxtLowestInfo2D.Text = lastDataSet.LowestLatency.ToString();
                TxtHighestInfo2D.Text = lastDataSet.HighestLatency.ToString();
                TxtAverageInfo2D.Text = lastDataSet.AverageLatency.ToString();
                TxtPacketsLostInfo2D.Text = lastDataSet.PacketsLostTotal.ToString();
            }
            else if (lastDataSet.IPAddress == TxtIPInfo3S.Text)
            {
                TxtIPInfo3S.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[2].ToString();
                TxtIPInfo3D.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[2].ToString();
                TxtStatusInfo3S.Text = lastDataSet.ConnectionStatus;
                TxtLatencyInfo3S.Text = lastDataSet.Latency.ToString();
                TxtStatusInfo3D.Text = lastDataSet.ConnectionStatus;
                TxtLowestInfo3D.Text = lastDataSet.LowestLatency.ToString();
                TxtHighestInfo3D.Text = lastDataSet.HighestLatency.ToString();
                TxtAverageInfo3D.Text = lastDataSet.AverageLatency.ToString();
                TxtPacketsLostInfo3D.Text = lastDataSet.PacketsLostTotal.ToString();
            }
            else if (lastDataSet.IPAddress == TxtIPInfo4S.Text)
            {
                TxtIPInfo4S.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[3].ToString();
                TxtIPInfo4D.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[3].ToString();
                TxtStatusInfo4S.Text = lastDataSet.ConnectionStatus;
                TxtLatencyInfo4S.Text = lastDataSet.Latency.ToString();
                TxtStatusInfo4D.Text = lastDataSet.ConnectionStatus;
                TxtLowestInfo4D.Text = lastDataSet.LowestLatency.ToString();
                TxtHighestInfo4D.Text = lastDataSet.HighestLatency.ToString();
                TxtAverageInfo4D.Text = lastDataSet.AverageLatency.ToString();
                TxtPacketsLostInfo4D.Text = lastDataSet.PacketsLostTotal.ToString();
            }
            else if (lastDataSet.IPAddress == TxtIPInfo5S.Text)
            {
                TxtIPInfo5S.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[4].ToString();
                TxtIPInfo5D.ToolTip = "Resolved IPv4 Address: " + DataStore.ResolvedName[4].ToString();
                TxtStatusInfo5S.Text = lastDataSet.ConnectionStatus;
                TxtLatencyInfo5S.Text = lastDataSet.Latency.ToString();
                TxtStatusInfo5D.Text = lastDataSet.ConnectionStatus;
                TxtLowestInfo5D.Text = lastDataSet.LowestLatency.ToString();
                TxtHighestInfo5D.Text = lastDataSet.HighestLatency.ToString();
                TxtAverageInfo5D.Text = lastDataSet.AverageLatency.ToString();
                TxtPacketsLostInfo5D.Text = lastDataSet.PacketsLostTotal.ToString();
            }
        }

        public void ProcessStateChange()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                TxtCurrentStatus.Foreground = Brushes.Green;
                TxtCurrentStatus.Text = "RUNNING";
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
                BtnGenerateReport.IsEnabled = false;

                TxtIPAddress01.IsReadOnly = true;
                TxtIPAddress02.IsReadOnly = true;
                TxtIPAddress03.IsReadOnly = true;
                TxtIPAddress04.IsReadOnly = true;
                TxtIPAddress05.IsReadOnly = true;

                ClearLatencyTestResults();
            }
            else
            {
                IsRunning = false;
                TxtCurrentStatus.Foreground = Brushes.Red;
                TxtCurrentStatus.Text = "IDLE";
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
                BtnGenerateReport.IsEnabled = true;

                TxtIPAddress01.IsReadOnly = false;
                TxtIPAddress02.IsReadOnly = false;
                TxtIPAddress03.IsReadOnly = false;
                TxtIPAddress04.IsReadOnly = false;
                TxtIPAddress05.IsReadOnly = false;
            }
        }

        public void ClearLatencyTestResults()
        {
            List<TextBox> AllWPFControls = new List<TextBox>
            {
                TxtIPInfo1D, TxtIPInfo2D, TxtIPInfo3D, TxtIPInfo4D, TxtIPInfo5D,
                TxtIPInfo1S, TxtIPInfo2S, TxtIPInfo3S, TxtIPInfo4S, TxtIPInfo5S,
                TxtLatencyInfo1S, TxtLatencyInfo2S, TxtLatencyInfo3S, TxtLatencyInfo4S, TxtLatencyInfo5S,
                TxtStatusInfo1D, TxtStatusInfo2D, TxtStatusInfo3D, TxtStatusInfo4D, TxtStatusInfo5D,
                TxtStatusInfo1S, TxtStatusInfo2S, TxtStatusInfo3S, TxtStatusInfo4S, TxtStatusInfo5S,
                TxtLowestInfo1D, TxtLowestInfo2D, TxtLowestInfo3D, TxtLowestInfo4D, TxtLowestInfo5D,
                TxtHighestInfo1D, TxtHighestInfo2D, TxtHighestInfo3D, TxtHighestInfo4D, TxtHighestInfo5D,
                TxtAverageInfo1D, TxtAverageInfo2D, TxtAverageInfo3D, TxtAverageInfo4D, TxtAverageInfo5D,
                TxtPacketsLostInfo1D, TxtPacketsLostInfo2D, TxtPacketsLostInfo3D, TxtPacketsLostInfo4D, TxtPacketsLostInfo5D
            };

            for (int i = 0; i < 45; i++)
            {
                AllWPFControls[i].Text = String.Empty;
            };
        }

        public bool CheckUserInput()
        {
            string[] userInput = { TxtIPAddress01.Text, TxtIPAddress02.Text, TxtIPAddress03.Text, TxtIPAddress04.Text, TxtIPAddress05.Text };
            string responseCodeMessage;

            // Make sure the user input fields aren't all empty
            // and validate the user input
            if ((responseCodeMessage = DataValidation.ValidateAtLeastOneFieldHasData(userInput)) != null)
            {
                MessageBox.Show(responseCodeMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return false;
            }
            else
            {
                foreach (string input in userInput)
                {
                    if ((!string.IsNullOrEmpty(input)) && (responseCodeMessage = DataValidation.ValidateIPAddress(input)) != null)
                    {
                        DataValidation.ResolveDNSNameForTooltip(input);
                        MessageBox.Show(responseCodeMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return false;
                    }
                    else if (!string.IsNullOrEmpty(input))
                    {
                        DataValidation.ResolveDNSNameForTooltip(input);
                        DataStore.IPAddresses.Add(input);
                    }
                }
                return true;
            }
        }
    }
}
