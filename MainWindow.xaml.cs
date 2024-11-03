using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        // Window Controls
        public MainWindow()
        {
            InitializeComponent();
            SizeToContent = SizeToContent.Manual;
        }

        public void TbtnBase_Checked(object sender, RoutedEventArgs e)
        {
            var darkModeDictionary = new ResourceDictionary();
            var lightModeDictionary = new ResourceDictionary();

            darkModeDictionary.Source = new Uri("Styles/DarkModeTheme.xaml", UriKind.RelativeOrAbsolute);
            lightModeDictionary.Source = new Uri("Styles/LightModeTheme.xaml", UriKind.RelativeOrAbsolute);

            DockPanel.SetDock(TbtnSlider, Dock.Right);
            Application.Current.Resources.MergedDictionaries.Add(lightModeDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(darkModeDictionary);
        }

        public void TbtnBase_Unchecked(object sender, RoutedEventArgs e)
        {
            var darkModeDictionary = new ResourceDictionary();
            var lightModeDictionary = new ResourceDictionary();

            darkModeDictionary.Source = new Uri("Styles/DarkModeTheme.xaml", UriKind.RelativeOrAbsolute);
            lightModeDictionary.Source = new Uri("Styles/LightModeTheme.xaml", UriKind.RelativeOrAbsolute);

            DockPanel.SetDock(TbtnSlider, Dock.Left);
            Application.Current.Resources.MergedDictionaries.Add(darkModeDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(lightModeDictionary);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Do nothing
                // The DragMove() method only supports the primary mouse button
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            GridCloseOptions.Visibility = Visibility.Visible;
            GridCloseOptionsBG.Visibility = Visibility.Visible;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TBIcon.Dispose();
        }

        private void MinimizeToSystemTray_Click(object sender, RoutedEventArgs e)
        {
            MainForm.Visibility = Visibility.Hidden;
            GridCloseOptions.Visibility = Visibility.Hidden;
            GridCloseOptionsBG.Visibility = Visibility.Hidden;
        }

        private void CancelClose_Click(object sender, RoutedEventArgs e)
        {
            GridCloseOptions.Visibility = Visibility.Hidden;
            GridCloseOptionsBG.Visibility = Visibility.Hidden;
        }

        private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainForm.Visibility = Visibility.Visible;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TBIcon.Dispose();
        }
    }
}