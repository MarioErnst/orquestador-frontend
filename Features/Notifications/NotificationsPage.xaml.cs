namespace OrquestadorFrontend.Features.Notifications;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _vm;

    public NotificationsPage(NotificationsViewModel vm)
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
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
