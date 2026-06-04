namespace OrquestadorFrontend.Features.Whistleblower;

public partial class WhistleblowerListPage : ContentPage
{
    private readonly WhistleblowerListViewModel _vm;

    public WhistleblowerListPage(WhistleblowerListViewModel vm)
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
