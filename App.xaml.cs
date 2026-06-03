using OrquestadorFrontend.Services;

namespace OrquestadorFrontend;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    // AppShell is resolved lazily inside CreateWindow so that App.xaml resources
    // (Colors, Typography, Styles) are merged before AppShell.xaml is parsed.
    // Injecting AppShell directly here would build it before InitializeComponent,
    // leaving Application.Current.Resources empty and breaking StaticResource lookups.
    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;

        // Fire-and-forget the theme service so the persisted preference is
        // applied as early as possible. The call returns before the window
        // is created, so the worst case is a single frame where the OS
        // theme is shown before the persisted Light/Dark override takes
        // effect — acceptable for a cosmetic preference, and avoids
        // blocking the launch path on a SecureStorage round-trip.
        _ = _services.GetRequiredService<IThemeService>().InitializeAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new Window(_services.GetRequiredService<AppShell>());
}
