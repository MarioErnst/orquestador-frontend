namespace OrquestadorFrontend.Data.Models;

public enum NotificationKind
{
    NewBoard,
    IndicatorThresholdExceeded,
    NewReport,
    WhistleblowerAlert
}

// A notification entry shown in the Notifications feature and used by the home
// screen's "Alertas recientes" summary block.
public sealed record NotificationItem(
    string Id,
    NotificationKind Kind,
    string Title,
    string Body,
    DateTime ReceivedAt,
    string? TargetRef = null,
    bool Read = false);
