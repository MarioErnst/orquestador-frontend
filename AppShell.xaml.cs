using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Repositories;
using OrquestadorFrontend.Features.Auth;
using OrquestadorFrontend.Features.Documents;
using OrquestadorFrontend.Features.Notifications;
using OrquestadorFrontend.Features.Profile;
using OrquestadorFrontend.Features.Reports;
using OrquestadorFrontend.Features.Whistleblower;
using OrquestadorFrontend.Services;

namespace OrquestadorFrontend;

public partial class AppShell : Shell
{
    private readonly ISessionService _session;
    private readonly IWhistleblowerRepository _whistleblower;

    public AppShell(ISessionService session, IWhistleblowerRepository whistleblower)
    {
        InitializeComponent();

        _session = session;
        _whistleblower = whistleblower;

        // Standalone routes (not part of the TabBar). Resolved via DI.
        Routing.RegisterRoute("biometric-reauth", typeof(BiometricReauthPage));
        Routing.RegisterRoute("board", typeof(BoardViewerPage));
        Routing.RegisterRoute("document", typeof(DocumentViewerPage));
        Routing.RegisterRoute("notifications/preferences", typeof(NotificationPreferencesPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));

        // Whistleblower routes. Reinforced sensitivity: every navigation is
        // re-checked by the guard below before it resolves a page.
        Routing.RegisterRoute("whistleblower", typeof(WhistleblowerListPage));
        Routing.RegisterRoute("whistleblower/case", typeof(WhistleblowerCasePage));

        Navigating += OnNavigating;
    }

    // Defense in depth (Propuesta 1 §8, Propuesta 2 §5.6). The Home entry
    // only renders for roles with access, but a direct deep link or a stale
    // notification could still target the channel. We re-check on every
    // navigation and cancel silently — no error message reveals the module's
    // existence.
    private void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target?.Location?.OriginalString ?? string.Empty;

        if (!target.Contains("whistleblower", StringComparison.OrdinalIgnoreCase))
            return;

        var role = _session.CurrentRole ?? UserRole.Director;
        if (!_whistleblower.HasAccess(role))
        {
            e.Cancel();
        }
    }
}
