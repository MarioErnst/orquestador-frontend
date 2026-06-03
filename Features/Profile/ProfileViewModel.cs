using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Profile;

public sealed partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileRepository _profile;
    private readonly ISessionService _session;
    private readonly IThemeService _theme;

    // Suppresses the property setter -> SetModeAsync feedback loop while
    // synchronising the three radio booleans after a ModeChanged event.
    private bool _syncingFromService;

    [ObservableProperty]
    private ResultState<UserProfile> _state = new Loading<UserProfile>();

    [ObservableProperty] private UserProfile? _profileData;
    [ObservableProperty] private string _roleLabel = string.Empty;

    [ObservableProperty] private bool _isAppearanceSystem;
    [ObservableProperty] private bool _isAppearanceLight;
    [ObservableProperty] private bool _isAppearanceDark;

    public ProfileViewModel(
        IProfileRepository profile,
        ISessionService session,
        IThemeService theme)
    {
        _profile = profile;
        _session = session;
        _theme = theme;

        SyncAppearanceFromService(_theme.CurrentMode);
        _theme.ModeChanged += OnThemeChanged;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        State = new Loading<UserProfile>();
        try
        {
            var role = _session.CurrentRole ?? UserRole.Director;
            var data = await _profile.GetProfileAsync(role);
            ProfileData = data;
            RoleLabel = RoleLabelFor(data.Role);
            State = new Data<UserProfile>(data);
        }
        catch (Exception)
        {
            State = new Failure<UserProfile>(
                "No pudimos cargar tu perfil. Intentá de nuevo.");
        }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Cerrar sesión",
            "¿Querés cerrar la sesión actual?",
            "Cerrar sesión",
            "Cancelar");

        if (!confirmed) return;

        _session.SignOut();
        await Shell.Current.GoToAsync("//login");
    }

    partial void OnIsAppearanceSystemChanged(bool value)
    {
        if (value && !_syncingFromService)
        {
            _ = _theme.SetModeAsync(AppearanceMode.System);
        }
    }

    partial void OnIsAppearanceLightChanged(bool value)
    {
        if (value && !_syncingFromService)
        {
            _ = _theme.SetModeAsync(AppearanceMode.Light);
        }
    }

    partial void OnIsAppearanceDarkChanged(bool value)
    {
        if (value && !_syncingFromService)
        {
            _ = _theme.SetModeAsync(AppearanceMode.Dark);
        }
    }

    private void OnThemeChanged(object? sender, AppearanceMode mode)
        => SyncAppearanceFromService(mode);

    private void SyncAppearanceFromService(AppearanceMode mode)
    {
        _syncingFromService = true;
        try
        {
            IsAppearanceSystem = mode == AppearanceMode.System;
            IsAppearanceLight = mode == AppearanceMode.Light;
            IsAppearanceDark = mode == AppearanceMode.Dark;
        }
        finally
        {
            _syncingFromService = false;
        }
    }

    private static string RoleLabelFor(UserRole role) => role switch
    {
        UserRole.Director => "Miembro del Directorio",
        UserRole.Comite => "Comité",
        UserRole.AltaGerencia => "Alta Gerencia",
        _ => string.Empty
    };
}
