using NetworkAnalyzer.Apps.Home.Interfaces;

namespace NetworkAnalyzer.Apps.Home.Functions
{
    internal delegate void HomeChangelogUpdateEventHandler();

    internal class HomeController: IHomeController
    {
        public event HomeChangelogUpdateEventHandler UpdateChangelog;

        public void SendUpdateChangelogRequest()
        {
            UpdateChangelog.Invoke();
        }
    }
}
