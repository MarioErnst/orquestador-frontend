using System.ComponentModel;
using OrquestadorFrontend.Core;

namespace OrquestadorFrontend.Services;

// Active session for the prototype. The state is intentionally narrow: the
// only thing the app needs to know is which role is currently impersonated
// after the mock login. A null value means "no session" and the Shell
// redirects to the login flow.
//
// In production this surface will be replaced by a real auth layer (Entra ID
// via MSAL.NET) and the role will be derived from claims, not selected by
// hand. Consumers will not need to change.
public interface ISessionService : INotifyPropertyChanged
{
    UserRole? CurrentRole { get; }

    void SignIn(UserRole role);
    void SignOut();
}
