namespace NetworkAnalyzer.Apps.Models
{
    internal class ReportExplorerData
    {
        public string ReportGUID { get; set; }
        public ReportMode Mode { get; set; }
        public string? StartDateTime { get; set; }
        public string? EndDateTime { get; set; } = "N/A";
        public string? FriendlyName { get; set; }
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(FriendlyName))
                {
                    return FriendlyName;
                }
                else
                {
                    return ReportGUID;
                }
            }
            set
            {
                FriendlyName = value;
            }
        }
    }
}
