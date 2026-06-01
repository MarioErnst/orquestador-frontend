using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend.Features.Auth;

public sealed partial class BiometricReauthViewModel : ObservableObject
{
    private readonly ISessionService _session;

    public BiometricReauthViewModel(ISessionService session)
    {
        _session = session;
    }

    // PROTOTYPE: real biometric reauth uses Microsoft.Identity.Client +
    // platform Keychain/Keystore. Here the confirm command just routes to home.
    [RelayCommand]
    private async Task ConfirmAsync()
    {
        await Shell.Current.GoToAsync("//main/home");
    }

    [RelayCommand]
    private async Task UseOtherAccountAsync()
    {
        _session.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
