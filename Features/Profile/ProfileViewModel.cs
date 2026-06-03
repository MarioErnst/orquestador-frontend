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

    [ObservableProperty]
    private ResultState<UserProfile> _state = new Loading<UserProfile>();

    [ObservableProperty] private UserProfile? _profileData;
    [ObservableProperty] private string _roleLabel = string.Empty;

    public ProfileViewModel(IProfileRepository profile, ISessionService session)
    {
        _profile = profile;
        _session = session;
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

    private static string RoleLabelFor(UserRole role) => role switch
    {
        UserRole.Director => "Miembro del Directorio",
        UserRole.Comite => "Comité",
        UserRole.AltaGerencia => "Alta Gerencia",
        _ => string.Empty
    };
}
