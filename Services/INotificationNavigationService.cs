using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Services;

// Maps a NotificationItem to the Shell route it should open when tapped. Pure
// resolution so it can be reused from both the Notifications page and the
// home "Alertas recientes" block without duplicating the switch.
//
// Returns null when the notification does not point to a routable resource
// (e.g. TargetRef is missing). Callers must check null and decide whether
// the card is tappable or inert.
public interface INotificationNavigationService
{
    string? RouteFor(NotificationItem item);
}
