namespace NetworkAnalyzer.Apps.Home
{
    internal delegate void HomeChangelogUpdateEventHandler();

    internal class HomeConnectionUtility
    {
        public static event HomeChangelogUpdateEventHandler UpdateChangelogRequest;

        public static void SendUpdateChangelogRequest()
        {
            UpdateChangelogRequest.Invoke();
        }
    }
}
