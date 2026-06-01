using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Notifications;

public sealed partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationsRepository _notifications;
    private readonly ISessionService _session;
    private readonly INotificationNavigationService _notificationNav;

    [ObservableProperty]
    private ResultState<IReadOnlyList<NotificationItem>> _state =
        new Loading<IReadOnlyList<NotificationItem>>();

    public NotificationsViewModel(
        INotificationsRepository notifications,
        ISessionService session,
        INotificationNavigationService notificationNav)
    {
        _notifications = notifications;
        _session = session;
        _notificationNav = notificationNav;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        State = new Loading<IReadOnlyList<NotificationItem>>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var list = await _notifications.GetNotificationsAsync(role);
            var ordered = list.OrderByDescending(n => n.ReceivedAt).ToList();
            State = ordered.Count == 0
                ? new Empty<IReadOnlyList<NotificationItem>>()
                : new Data<IReadOnlyList<NotificationItem>>(ordered);
        }
        catch (Exception)
        {
            State = new Failure<IReadOnlyList<NotificationItem>>(
                "No pudimos cargar las notificaciones. Intentá de nuevo.");
        }
    }

    [RelayCommand]
    private async Task OpenAsync(NotificationItem item)
    {
        if (item is null) return;
        await _notifications.MarkAsReadAsync(item.Id);

        var route = _notificationNav.RouteFor(item);
        if (!string.IsNullOrEmpty(route))
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    [RelayCommand]
    private async Task OpenPreferencesAsync()
    {
        await Shell.Current.GoToAsync("notifications/preferences");
    }
}
