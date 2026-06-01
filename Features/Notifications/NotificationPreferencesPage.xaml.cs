namespace OrquestadorFrontend.Features.Notifications;

public partial class NotificationPreferencesPage : ContentPage
{
    private readonly NotificationPreferencesViewModel _vm;

    public NotificationPreferencesPage(NotificationPreferencesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
