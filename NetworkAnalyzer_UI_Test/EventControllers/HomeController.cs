using NetworkAnalyzer_UI_Test.Interfaces;

namespace NetworkAnalyzer_UI_Test.EventControllers
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
