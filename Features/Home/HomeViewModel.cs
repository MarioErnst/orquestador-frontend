using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Home;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly IDashboardRepository _dashboard;
    private readonly IReportsRepository _reports;
    private readonly IBoardsRepository _boards;
    private readonly INotificationsRepository _notifications;
    private readonly IWhistleblowerRepository _whistleblower;
    private readonly INotificationNavigationService _notificationNav;

    [ObservableProperty]
    private ResultState<IReadOnlyList<DashboardKpi>> _kpisState =
        new Loading<IReadOnlyList<DashboardKpi>>();

    [ObservableProperty]
    private ResultState<IReadOnlyList<NotificationItem>> _recentAlertsState =
        new Loading<IReadOnlyList<NotificationItem>>();

    [ObservableProperty] private Report? _latestReport;
    [ObservableProperty] private BiBoard? _featuredBoard;
    [ObservableProperty] private bool _hasWhistleblowerAccess;
    [ObservableProperty] private string _greeting = string.Empty;

    public HomeViewModel(
        ISessionService session,
        IDashboardRepository dashboard,
        IReportsRepository reports,
        IBoardsRepository boards,
        INotificationsRepository notifications,
        IWhistleblowerRepository whistleblower,
        INotificationNavigationService notificationNav)
    {
        _session = session;
        _dashboard = dashboard;
        _reports = reports;
        _boards = boards;
        _notifications = notifications;
        _whistleblower = whistleblower;
        _notificationNav = notificationNav;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var role = _session.CurrentRole ?? UserRole.Director;
        Greeting = BuildGreeting(role);
        HasWhistleblowerAccess = _whistleblower.HasAccess(role);

        KpisState = new Loading<IReadOnlyList<DashboardKpi>>();
        RecentAlertsState = new Loading<IReadOnlyList<NotificationItem>>();

        try
        {
            var kpis = await _dashboard.GetKpisAsync(role);
            KpisState = kpis.Count == 0
                ? new Empty<IReadOnlyList<DashboardKpi>>()
                : new Data<IReadOnlyList<DashboardKpi>>(kpis);
        }
        catch (Exception)
        {
            KpisState = new Failure<IReadOnlyList<DashboardKpi>>(
                "No pudimos cargar los indicadores. Intentá de nuevo.");
        }

        try
        {
            var reports = await _reports.GetReportsAsync(role);
            LatestReport = reports.OrderByDescending(r => r.PublishedAt).FirstOrDefault();
        }
        catch (Exception)
        {
            LatestReport = null;
        }

        try
        {
            var boards = await _boards.GetBoardsAsync(role);
            FeaturedBoard = boards.FirstOrDefault();
        }
        catch (Exception)
        {
            FeaturedBoard = null;
        }

        try
        {
            var notifications = await _notifications.GetNotificationsAsync(role);
            var recent = notifications.OrderByDescending(n => n.ReceivedAt).Take(3).ToList();
            RecentAlertsState = recent.Count == 0
                ? new Empty<IReadOnlyList<NotificationItem>>()
                : new Data<IReadOnlyList<NotificationItem>>(recent);
        }
        catch (Exception)
        {
            RecentAlertsState = new Failure<IReadOnlyList<NotificationItem>>(
                "No pudimos cargar las alertas recientes.");
        }
    }

    [RelayCommand]
    private async Task OpenLatestReportAsync()
    {
        if (LatestReport is null) return;
        await Shell.Current.GoToAsync($"document?id={Uri.EscapeDataString(LatestReport.Id)}");
    }

    [RelayCommand]
    private async Task OpenFeaturedBoardAsync()
    {
        if (FeaturedBoard is null) return;
        await Shell.Current.GoToAsync($"board?id={Uri.EscapeDataString(FeaturedBoard.Id)}");
    }

    [RelayCommand]
    private async Task OpenWhistleblowerAsync()
    {
        // The Shell guard re-checks the role; this command only runs because
        // HasWhistleblowerAccess made the entry visible in the first place.
        await Shell.Current.GoToAsync("//main/whistleblower");
    }

    [RelayCommand]
    private async Task OpenNotificationAsync(NotificationItem item)
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
    private async Task OpenAllNotificationsAsync()
    {
        await Shell.Current.GoToAsync("//main/notifications");
    }

    private static string BuildGreeting(UserRole role) => role switch
    {
        UserRole.Director => "Hola, Director",
        UserRole.Comite => "Hola, Comité",
        UserRole.AltaGerencia => "Hola, Alta Gerencia",
        _ => "Hola"
    };
}
