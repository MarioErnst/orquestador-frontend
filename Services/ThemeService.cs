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

    public AppearanceMode CurrentMode => _currentMode;

    public event EventHandler<AppearanceMode>? ModeChanged;

    public async Task InitializeAsync()
    {
        var persisted = await ReadPersistedAsync().ConfigureAwait(false);
        ApplyOnMainThread(persisted);
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

            _currentMode = mode;
            ModeChanged?.Invoke(this, mode);
        });
    }
}
