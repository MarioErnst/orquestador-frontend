using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Services;

public sealed class NotificationNavigationService : INotificationNavigationService
{
    public string? RouteFor(NotificationItem item)
    {
        var targetRef = item.TargetRef;
        if (string.IsNullOrEmpty(targetRef)) return null;

        return item.Kind switch
        {
            NotificationKind.NewBoard => $"board?id={Uri.EscapeDataString(targetRef)}",
            NotificationKind.IndicatorThresholdExceeded => $"board?id={Uri.EscapeDataString(targetRef)}",
            NotificationKind.NewReport => $"document?id={Uri.EscapeDataString(targetRef)}",
            NotificationKind.WhistleblowerAlert => $"whistleblower/case?id={Uri.EscapeDataString(targetRef)}",
            _ => null
        };
    }
}
