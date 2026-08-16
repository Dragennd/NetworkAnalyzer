using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Material.Icons;

namespace NetworkAnalyzer.Models;

internal class NotificationInfo
{
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string GUID { get; private set; }
    public MaterialIconKind Icon  { get; private set; }
    public IBrush TitleBorderColor { get; private set; }
    public IBrush TitleIconColor  { get; private set; }

    public NotificationInfo(string title, string body, MaterialIconKind icon)
    {
        Title = title;
        Body = body;
        Icon = icon;
        GUID = Guid.NewGuid().ToString();
        TitleBorderColor = SetTitleBorderColor();
        TitleIconColor = SetTitleIconColor();
    }

    public async Task ClearNotification()
    {
        
    }

    private IBrush SetTitleBorderColor()
    {
        return Icon switch
        {
            MaterialIconKind.AlertOutline => Brushes.Yellow, // Warning
            MaterialIconKind.AlertOctagonOutline => Brushes.Red, // Error
            MaterialIconKind.InformationOutline => Brushes.DeepSkyBlue, // Information
            _ => Brushes.White // Default
        };
    }

    private IBrush SetTitleIconColor()
    {
        return Icon switch
        {
            MaterialIconKind.AlertOutline => Brushes.Yellow, // Warning
            MaterialIconKind.AlertOctagonOutline => Brushes.Red, // Error
            MaterialIconKind.InformationOutline => Brushes.DeepSkyBlue, // Information
            _ => Brushes.White // Default
        };
    }
}