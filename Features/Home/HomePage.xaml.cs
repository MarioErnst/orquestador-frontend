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
        // Re-fetch on every appearance so the home dashboard reflects the
        // latest mock state (e.g. after a notification was marked as read).
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
