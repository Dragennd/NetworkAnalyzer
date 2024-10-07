namespace NetworkAnalyzer.Apps.GlobalClasses
{
    internal delegate void MenuControllerEventHandler(string data);

    internal delegate void OptionalControlsEventHandler(bool status);

    internal static class MenuController
    {
        public static event MenuControllerEventHandler ActiveAppRequest;

        public static void SendActiveAppRequest(string data)
        {
            ActiveAppRequest.Invoke(data);
        }
    }
}
