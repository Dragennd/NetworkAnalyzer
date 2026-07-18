using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IHomeController
    {
        event HomeChangelogUpdateEventHandler UpdateChangelog;

        void SendUpdateChangelogRequest();
    }
}
