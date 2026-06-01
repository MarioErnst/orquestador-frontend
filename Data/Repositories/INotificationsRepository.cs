using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Repositories;

// Contract for the Notifications feature. Preferences live here too because
// the same channel decides what arrives and what the user can mute.
public interface INotificationsRepository
{
    Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(UserRole role);

    // Idempotent: calling on an already-read item is a noop. The prototype
    // keeps the read set in memory; production will persist it server-side so
    // the read state survives reinstalls.
    Task MarkAsReadAsync(string notificationId);

    // Per-kind opt-in/opt-out. Defaults to all enabled.
    Task<IReadOnlyDictionary<NotificationKind, bool>> GetPreferencesAsync();
    Task SetPreferenceAsync(NotificationKind kind, bool enabled);
}
