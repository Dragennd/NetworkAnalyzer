using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.EventControllers
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
