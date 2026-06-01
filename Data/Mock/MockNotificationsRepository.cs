using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Data.Mock;

public sealed class MockNotificationsRepository : INotificationsRepository
{
    public bool SimulateError { get; set; }

    // In-memory preferences for the prototype. Default: all kinds enabled.
    private readonly Dictionary<NotificationKind, bool> _preferences =
        Enum.GetValues<NotificationKind>().ToDictionary(k => k, _ => true);

    // Ids of notifications the user already read. MockData items are
    // immutable, so the repo overlays this set onto each item at read time.
    private readonly HashSet<string> _readIds = new();

    public async Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(UserRole role)
    {
        await MockLatency.SimulateAsync();
        if (SimulateError) throw new InvalidOperationException("Mock: failed to load notifications");

        return MockData.NotificationsForRole(role)
            .Where(n => _preferences.GetValueOrDefault(n.Kind, true))
            .Select(ApplyReadState)
            .ToList();
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await MockLatency.SimulateAsync();
        _readIds.Add(notificationId);
    }

    public async Task<IReadOnlyDictionary<NotificationKind, bool>> GetPreferencesAsync()
    {
        await MockLatency.SimulateAsync();
        return new Dictionary<NotificationKind, bool>(_preferences);
    }

    public async Task SetPreferenceAsync(NotificationKind kind, bool enabled)
    {
        await MockLatency.SimulateAsync();
        _preferences[kind] = enabled;
    }

    private NotificationItem ApplyReadState(NotificationItem n)
        => (_readIds.Contains(n.Id) && !n.Read) ? n with { Read = true } : n;
}
