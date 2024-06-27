using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Net.Http;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Home.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Home
{
    public partial class Home : UserControl
    {
        private GitHubResponse? Response { get; set; }
        private bool HasUpdatesBeenChecked { get; set; } = false;

        public Home()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!HasUpdatesBeenChecked)
                {
                    GitHubRequestHandler handler = new();
                    Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());

                    if (Response.VersionInfo.Find(a => a.Build == CurrentBuild) != null)
                    {
                        GetVersionStatusAsync();
                        await GenerateChangeLogAsync();
                    }
                    else
                    {
                        TxtChangeLog.Text = "ChangeLog failed to load.\nServer may be unavailable or version manifest may be out of date.";
                    }
                }
            }
            catch (InvalidOperationException)
            {
                TxtChangeLog.Text = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }
            catch (HttpRequestException)
            {
                TxtChangeLog.Text = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }
            catch (TaskCanceledException)
            {
                TxtChangeLog.Text = "ChangeLog failed to load.\nPlease check your internet connection and relaunch the app to try again.";
            }

            HasUpdatesBeenChecked = true;
        }

        private async void BtnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            GitHubRequestHandler handler = new();
            Response = await handler.ProcessEncodedResponse(await handler.GetRepositoryManifest());

            GetVersionStatusAsync();
        }

        private void GetVersionStatusAsync()
        {
            Paragraph parx = new()
            {
                TextAlignment = TextAlignment.Center
            };

            if (Response.LatestVersion != CurrentBuild)
            {
                Run run1 = new("A new version is available!\nClick ");
                Run run2 = new(" to download it.");
                Run run3 = new("here");

                Hyperlink link = new(run3);
                link.NavigateUri = new Uri("https://github.com/Dragennd/NetworkAnalyzer/releases");

                link.RequestNavigate += Hyperlink_RequestNavigate;

                parx.Inlines.Add(run1);
                parx.Inlines.Add(link);
                parx.Inlines.Add(run2);

                TxtUpdateInfo.Document.Blocks.Clear();
                TxtUpdateInfo.Document.Blocks.Add(parx);
            }
            else
            {
                Run run1 = new("NetworkAnalyzer is Up-to-Date!\n");
                Run run2 = new("Last Checked: ");
                Run run3 = new(DateTime.Now.ToString("MM/dd/yyyy HH:mm"));

                parx.Inlines.Add(run1);
                parx.Inlines.Add(run2);
                parx.Inlines.Add(run3);

                TxtUpdateInfo.Document.Blocks.Clear();
                TxtUpdateInfo.Document.Blocks.Add(parx);
            }
        }

        private async Task GenerateChangeLogAsync()
        {
            TxtChangeLog.Text = $"General Notes:\n{await GetGeneralNotes()}\n\nNew Features:\n{await GetNewFeatures()}\n\nBug Fixes:\n{await GetBugFixes()}";
        }

        private async Task<string> GetGeneralNotes()
        {
            var info = Response.VersionInfo.Find(a => a.Build == CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.GeneralNotes)));
        }

        private async Task<string> GetNewFeatures()
        {
            var info = Response.VersionInfo.Find(a => a.Build == CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.NewFeatures)));
        }

        private async Task<string> GetBugFixes()
        {

            var info = Response.VersionInfo.Find(a => a.Build == CurrentBuild);
            return await Task.FromResult(string.Join(Environment.NewLine, info.ChangeLog.Select(a => a.BugFixes)));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
