using CommunityToolkit.Mvvm.ComponentModel;
using OrquestadorFrontend.Core;

namespace OrquestadorFrontend.Services;

public sealed partial class SessionService : ObservableObject, ISessionService
{
    [ObservableProperty]
    private UserRole? _currentRole;

    public void SignIn(UserRole role) => CurrentRole = role;

    public void SignOut() => CurrentRole = null;
}
