namespace NetworkAnalyzer.Apps.GlobalClasses
{
    internal delegate void MenuControllerEventHandler(string data);

    internal delegate void OptionalControlsEventHandler(bool status);

    internal static class MenuController
    {
        public static event MenuControllerEventHandler activeAppRequest;
        public static event OptionalControlsEventHandler optionalControlsVisibility;

        public static void SendActiveAppRequest(string data)
        {
            activeAppRequest.Invoke(data);
        }

        public static void SendControlVisibilityRequest(bool status)
        {
            optionalControlsVisibility.Invoke(status);
        }
    }
}
