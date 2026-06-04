namespace OrquestadorFrontend.Features.Home;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Opacity = 0;
        _ = this.FadeTo(1, 280, Easing.CubicOut);
        // Re-fetch on every appearance so the dashboard reflects the latest
        // state (e.g. after a notification was marked as read elsewhere).
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
