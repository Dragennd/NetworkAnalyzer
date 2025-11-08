using NetworkAnalyzer.Apps.Home.Functions;

namespace NetworkAnalyzer.Apps.Home.Interfaces
{
    internal interface IHomeController
    {
        event HomeChangelogUpdateEventHandler UpdateChangelog;

        void SendUpdateChangelogRequest();
    }
}
