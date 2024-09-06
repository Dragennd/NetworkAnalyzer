namespace NetworkAnalyzer.Apps.GlobalClasses
{
    internal delegate void MenuControllerEventHandler(string data);

    internal static class MenuController
    {
        public static event MenuControllerEventHandler activeAppRequest;

        public static void SendActiveAppRequest(string data)
        {
            activeAppRequest.Invoke(data);
        }
    }
}
