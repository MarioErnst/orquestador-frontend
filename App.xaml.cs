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
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new Window(_services.GetRequiredService<AppShell>());
}
