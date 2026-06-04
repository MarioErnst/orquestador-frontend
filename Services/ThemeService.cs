using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace OrquestadorFrontend.Services;

// Default implementation of IThemeService.
//
// Storage: the preference key is held in SecureStorage. The value itself is
// purely cosmetic (System / Light / Dark) and contains no PII, but the rest
// of the app already standardises on the platform keychain/keystore for
// per-user state, so honouring the same surface keeps the access pattern
// uniform and the auditing story consistent (CLAUDE.md section 6).
//
// Application: when the mode resolves to System, AppTheme.Unspecified is
// applied so MAUI keeps tracking the OS preference live. When the mode is
// Light or Dark, AppTheme.Light or AppTheme.Dark is forced regardless of
// the OS setting.
internal sealed class ThemeService : IThemeService
{
    private const string PreferenceKey = "appearance-mode";

    private AppearanceMode _currentMode = AppearanceMode.System;
    private bool _shellNavigationHooked;

    public AppearanceMode CurrentMode => _currentMode;

    public event EventHandler<AppearanceMode>? ModeChanged;

    public async Task InitializeAsync()
    {
        var persisted = await ReadPersistedAsync().ConfigureAwait(false);
        ApplyOnMainThread(persisted);
        SubscribeToSystemThemeChanges();
    }

    // Lazy ShellContent.ContentTemplate (used for every tab in AppShell)
    // keeps tab pages out of the visual tree and out of
    // ShellContent.Content while their tab is not active, so they cannot
    // be reached from ForceShellChromeRefresh when the user toggles the
    // theme from another tab. The Setters of those non-current pages
    // still cache the original theme's colours and the cards/labels
    // would stay frozen until the page is rebuilt.
    //
    // The fix is to refresh the Style Setters of whichever page becomes
    // current after a Shell navigation event: the lazy page is now
    // materialised and visible, so the walker can re-apply the resolved
    // colours for the current theme.
    private void EnsureShellNavigationHooked()
    {
        if (_shellNavigationHooked)
        {
            return;
        }

        var shell = Shell.Current;
        if (shell is null)
        {
            return;
        }

        shell.Navigated -= OnShellNavigated;
        shell.Navigated += OnShellNavigated;
        _shellNavigationHooked = true;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var app = Application.Current;
        if (app is null || sender is not Shell shell)
        {
            return;
        }

        // Defer to the next dispatcher cycle so the navigation
        // transition has time to materialise the destination page
        // before the walker reads its visual tree.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Shell.Current?.CurrentPage is ContentPage page)
            {
                StyleAppThemeRefresher.Refresh(page, app.RequestedTheme);
            }
        });
    }

    // When the mode is System we still need to refresh the status bar
    // when the OS-level scheme changes (user pulls the quick-settings
    // toggle, sunrise/sunset auto-mode, etc.). MAUI updates the
    // AppThemeBinding pairs on its own; the status bar is platform-level
    // and does not, so the service reacts to keep both in sync.
    private void SubscribeToSystemThemeChanges()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeChanged -= OnSystemRequestedThemeChanged;
        app.RequestedThemeChanged += OnSystemRequestedThemeChanged;
    }

    private void OnSystemRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (_currentMode != AppearanceMode.System)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is { } app)
            {
                ForceShellChromeRefresh(app);
            }

            ApplyStatusBarFor(e.RequestedTheme);
        });
    }

    public async Task SetModeAsync(AppearanceMode mode)
    {
        if (mode == _currentMode)
        {
            return;
        }

        await PersistAsync(mode).ConfigureAwait(false);
        ApplyOnMainThread(mode);
    }

    private static async Task<AppearanceMode> ReadPersistedAsync()
    {
        try
        {
            var raw = await SecureStorage.Default.GetAsync(PreferenceKey).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(raw)
                && Enum.TryParse<AppearanceMode>(raw, ignoreCase: true, out var parsed))
            {
                return parsed;
            }
        }
        catch
        {
            // SecureStorage can throw on some Android devices where the
            // keystore is unavailable (corporate restrictions, broken
            // keystore after factory reset). In those cases the preference
            // defaults to System rather than crashing the app on launch.
        }

        return AppearanceMode.System;
    }

    private static async Task PersistAsync(AppearanceMode mode)
    {
        try
        {
            await SecureStorage.Default.SetAsync(PreferenceKey, mode.ToString())
                .ConfigureAwait(false);
        }
        catch
        {
            // Same fallback as in ReadPersistedAsync: if SecureStorage is
            // unavailable, the preference still applies for the rest of
            // the session even though it cannot survive a restart.
        }
    }

    private void ApplyOnMainThread(AppearanceMode mode)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var app = Application.Current;
            if (app is null)
            {
                return;
            }

            app.UserAppTheme = mode switch
            {
                AppearanceMode.Light => AppTheme.Light,
                AppearanceMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };

            // First pass: run synchronously so Shell chrome and page
            // backgrounds flip as soon as UserAppTheme changes.
            ForceShellChromeRefresh(app);
            ApplyStatusBarFor(app.RequestedTheme);

            // Idempotent: hooks Shell.Navigated once so any lazy tab
            // page (HomePage, ReportsListPage, etc.) gets its Style
            // Setters re-evaluated the moment it becomes visible after
            // a theme toggle.
            EnsureShellNavigationHooked();

            _currentMode = mode;
            ModeChanged?.Invoke(this, mode);

            // Second pass: deferred to the next message-queue cycle.
            // In .NET MAUI 10, AppThemeBinding updates are dispatched
            // asynchronously after UserAppTheme changes; the second pass
            // ensures our programmatic colours win over any binding that
            // fires late and tries to override them.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current is { } a)
                {
                    ForceShellChromeRefresh(a);
                }
            });
        });
    }

    // Workaround for dotnet/maui#6596 and #20243: Shell.BackgroundColor,
    // Shell.ForegroundColor, Shell.TitleColor and the TabBar colour
    // setters do not always refresh when Application.UserAppTheme changes
    // at runtime, even though the AppThemeBinding pair is supposed to be
    // reactive. The result was the page chrome staying frozen in the
    // previous scheme until the next navigation. The fix is to read the
    // resolved palette from Application.Resources and re-assign each
    // Shell property imperatively. The work is cheap (a handful of
    // dictionary lookups), runs once per toggle, and matches what MAUI
    // should be doing for us.
    private static void ForceShellChromeRefresh(Application app)
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            return;
        }

        var isDark = app.RequestedTheme == AppTheme.Dark;
        var surface = isDark ? "DarkSurface" : "Surface";
        var onSurface = isDark ? "DarkOnSurface" : "OnSurface";
        var surfaceContainer = isDark ? "DarkSurfaceContainer" : "SurfaceContainer";
        var primary = isDark ? "DarkPrimary" : "Primary";
        var onSurfaceVariant = isDark ? "DarkOnSurfaceVariant" : "OnSurfaceVariant";
        var background = isDark ? "DarkBackground" : "Background";

        if (TryResolveColor(app, surface, out var surfaceColor))
        {
            shell.BackgroundColor = surfaceColor;
        }

        if (TryResolveColor(app, onSurface, out var onSurfaceColor))
        {
            Shell.SetForegroundColor(shell, onSurfaceColor);
            Shell.SetTitleColor(shell, onSurfaceColor);
        }

        if (TryResolveColor(app, surfaceContainer, out var tabBg))
        {
            Shell.SetTabBarBackgroundColor(shell, tabBg);
        }

        if (TryResolveColor(app, primary, out var primaryColor))
        {
            Shell.SetTabBarForegroundColor(shell, primaryColor);
            Shell.SetTabBarTitleColor(shell, primaryColor);
        }

        if (TryResolveColor(app, onSurfaceVariant, out var unselectedColor))
        {
            Shell.SetTabBarUnselectedColor(shell, unselectedColor);
        }

        // ContentPage.BackgroundColor with AppThemeBinding does not refresh
        // on UserAppTheme changes either (same bug family). Walk every
        // ContentPage currently materialised under the Shell — TabBar tabs,
        // modal stack, navigation stack — and re-assign the page
        // background imperatively so the body of the screen flips along
        // with the chrome.
        var whistleblowerKey = isDark ? "DarkWhistleblowerSurface" : "WhistleblowerSurface";
        var hasDefaultBg = TryResolveColor(app, background, out var defaultBg);
        var hasWhistleblowerBg = TryResolveColor(app, whistleblowerKey, out var whistleblowerBg);

        if (hasDefaultBg || hasWhistleblowerBg)
        {
            ForceBackgroundOnContentPages(shell, defaultBg, whistleblowerBg, hasDefaultBg, hasWhistleblowerBg);
        }

        // CollectionView (backed by Android RecyclerView) does not propagate
        // RequestedThemeChanged to its item views — the recycled ViewHolders
        // are detached from the window during idle time and miss the event.
        // Resetting ItemsSource forces RecyclerView to discard all current
        // views and recreate them from the DataTemplate, which evaluates
        // AppThemeBindings with the current scheme.
        ForceCollectionViewsRefresh(shell);

        // Final pass: AppThemeBinding declared inside Style Setters does
        // not refresh on UserAppTheme changes (dotnet/maui#6596 family).
        // The generic refresher walks every materialised page and
        // re-applies the resolved colour for any Setter that uses an
        // AppThemeBinding, including Setters inherited via BasedOn.
        // Runs after the CollectionView reset so recreated DataTemplate
        // instances are covered as well.
        ForceStyleSettersRefresh(shell, app.RequestedTheme);
    }

    private static void ForceStyleSettersRefresh(Shell shell, AppTheme currentTheme)
    {
        foreach (var page in EnumerateContentPages(shell))
        {
            StyleAppThemeRefresher.Refresh(page, currentTheme);
        }
    }

    private static void ForceCollectionViewsRefresh(Shell shell)
    {
        foreach (var page in EnumerateContentPages(shell))
        {
            ResetCollectionViewsInElement(page);
        }
    }

    private static void ResetCollectionViewsInElement(IVisualTreeElement element)
    {
        if (element is CollectionView cv)
        {
            var src = cv.ItemsSource;
            if (src is not null)
            {
                cv.ItemsSource = null;
                cv.ItemsSource = src;
            }
            return;
        }

        foreach (var child in element.GetVisualChildren())
        {
            ResetCollectionViewsInElement(child);
        }
    }

    private static void ForceBackgroundOnContentPages(
        Shell shell,
        Color defaultBg,
        Color whistleblowerBg,
        bool hasDefaultBg,
        bool hasWhistleblowerBg)
    {
        foreach (var page in EnumerateContentPages(shell))
        {
            // Whistleblower pages keep their distinct warm surface; every
            // other ContentPage gets the regular page background.
            var typeName = page.GetType().Name;
            var isWhistleblower = typeName.StartsWith("Whistleblower", StringComparison.Ordinal);

            if (isWhistleblower && hasWhistleblowerBg)
            {
                page.BackgroundColor = whistleblowerBg;
            }
            else if (!isWhistleblower && hasDefaultBg)
            {
                page.BackgroundColor = defaultBg;
            }
        }
    }

    private static IEnumerable<ContentPage> EnumerateContentPages(Shell shell)
    {
        // The currently visible page is the most important target: it is
        // always materialised, even when ShellContent uses lazy
        // ContentTemplate resolution (so the iteration below over
        // shell.Items would miss it). Yielded first so the user sees the
        // active screen flip immediately.
        if (shell.CurrentPage is ContentPage current)
        {
            yield return current;
        }

        foreach (var item in shell.Items)
        {
            foreach (var section in item.Items)
            {
                foreach (var content in section.Items)
                {
                    if (content.Content is ContentPage cp && !ReferenceEquals(cp, shell.CurrentPage))
                    {
                        yield return cp;
                    }
                }
            }
        }

        // Pages pushed via GoToAsync (Navigation stack on top of the
        // current section). Includes pages like Profile, BoardViewer,
        // DocumentViewer, NotificationPreferences when they are open.
        if (shell.Navigation?.NavigationStack is { } nav)
        {
            foreach (var page in nav)
            {
                if (page is ContentPage cp && !ReferenceEquals(cp, shell.CurrentPage))
                {
                    yield return cp;
                }
            }
        }

        if (shell.Navigation?.ModalStack is { } modals)
        {
            foreach (var page in modals)
            {
                if (page is ContentPage cp && !ReferenceEquals(cp, shell.CurrentPage))
                {
                    yield return cp;
                }
            }
        }
    }

    private static bool TryResolveColor(Application app, string key, out Color color)
    {
        if (app.Resources.TryGetValue(key, out var raw) && raw is Color resolved)
        {
            color = resolved;
            return true;
        }

        color = Colors.Transparent;
        return false;
    }

    // Status bar colour and text style must match the resolved theme so the
    // chrome reads as one piece with the app. CommunityToolkit.Maui exposes
    // the platform-specific calls behind a single API; failures are
    // swallowed because some headless contexts (test runners, snapshot
    // captures) do not have a window to update.
    private static void ApplyStatusBarFor(AppTheme resolvedTheme)
    {
        try
        {
            var (color, style) = resolvedTheme == AppTheme.Dark
                ? (Color.FromArgb("#1C1F20"), StatusBarStyle.LightContent)
                : (Color.FromArgb("#F1F0EB"), StatusBarStyle.DarkContent);

            StatusBar.SetColor(color);
            StatusBar.SetStyle(style);
        }
        catch
        {
            // Headless / unsupported context — ignored.
        }
    }
}
