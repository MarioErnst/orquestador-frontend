namespace OrquestadorFrontend.Services;

// Owns the appearance preference of the running app: reads it from secure
// storage at startup, applies the matching MAUI AppTheme, and persists any
// change requested from the UI (Profile / Apariencia).
//
// The service exposes the operations the UI needs and nothing more: there
// is no leak of the underlying storage mechanism, and no async setup for
// callers that only want to read the current mode.
public interface IThemeService
{
    // Current mode. Reflects the persisted value once InitializeAsync has
    // run; before that, falls back to System.
    AppearanceMode CurrentMode { get; }

    // Raised after CurrentMode changes, on the UI thread, so view models
    // bound to the toggle can refresh without polling.
    event EventHandler<AppearanceMode>? ModeChanged;

    // Reads the persisted preference and applies it to the running
    // Application. Safe to call multiple times. Invoked once from the
    // composition root at app start.
    Task InitializeAsync();

    // Persists the new preference and applies it immediately. Idempotent
    // when the mode does not change.
    Task SetModeAsync(AppearanceMode mode);
}
