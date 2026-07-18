using NetworkAnalyzer_UI_Test.EventControllers;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IHomeController
    {
        event HomeChangelogUpdateEventHandler UpdateChangelog;

        void SendUpdateChangelogRequest();
    }
}
