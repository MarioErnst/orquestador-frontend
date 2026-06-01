using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Core;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Auth;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ISessionService _session;

    public LoginViewModel(ISessionService session)
    {
        _session = session;
    }

    // PROTOTYPE: production replaces this with the real MSAL.NET flow against
    // Microsoft Entra ID. The role would then come from the access token's
    // claims, not from a manual picker.
    [RelayCommand]
    private async Task SignInAsync(UserRole role)
    {
        _session.SignIn(role);
        await Shell.Current.GoToAsync("//main/home");
    }
}
