namespace OrquestadorFrontend.Shared.Controls;

public partial class AppBarTitle : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(AppBarTitle),
            defaultValue: string.Empty);

    public AppBarTitle()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private async void OnAvatarTapped(object? sender, TappedEventArgs e)
    {
        // Profile is registered as a top-level route in AppShell. The
        // navigation is fire-and-forget on purpose: this control is the
        // entry point, not the owner of the resulting page lifecycle.
        try
        {
            await Shell.Current.GoToAsync("profile");
        }
        catch
        {
            // If the route is somehow not yet available (race during
            // shell setup), the user can retry; silent failure is the
            // right default for a navigation entry point.
        }
    }
}
