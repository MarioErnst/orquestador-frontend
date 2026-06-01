using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;

namespace OrquestadorFrontend.Features.Notifications;

public sealed partial class NotificationPreferencesViewModel : ObservableObject
{
    private readonly INotificationsRepository _notifications;

    [ObservableProperty]
    private ObservableCollection<PreferenceItem> _preferences = new();

    [ObservableProperty] private bool _isLoading = true;

    public NotificationPreferencesViewModel(INotificationsRepository notifications)
    {
        _notifications = notifications;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        var prefs = await _notifications.GetPreferencesAsync();

        Preferences = new ObservableCollection<PreferenceItem>(
            prefs.Select(kvp => new PreferenceItem(kvp.Key, LabelFor(kvp.Key), kvp.Value, this)));
        IsLoading = false;
    }

    internal async Task SetPreferenceAsync(NotificationKind kind, bool enabled)
    {
        await _notifications.SetPreferenceAsync(kind, enabled);
    }

    private static string LabelFor(NotificationKind kind) => kind switch
    {
        NotificationKind.NewBoard => "Nuevos tableros publicados",
        NotificationKind.IndicatorThresholdExceeded => "Indicadores que superan umbral",
        NotificationKind.NewReport => "Nuevos informes disponibles",
        NotificationKind.WhistleblowerAlert => "Alertas del Canal de Denuncias",
        _ => kind.ToString()
    };
}

public sealed partial class PreferenceItem : ObservableObject
{
    private readonly NotificationPreferencesViewModel _parent;

    public NotificationKind Kind { get; }
    public string Label { get; }

    [ObservableProperty]
    private bool _enabled;

    public PreferenceItem(NotificationKind kind, string label, bool enabled, NotificationPreferencesViewModel parent)
    {
        Kind = kind;
        Label = label;
        _enabled = enabled;
        _parent = parent;
    }

    partial void OnEnabledChanged(bool value) => _ = _parent.SetPreferenceAsync(Kind, value);
}
