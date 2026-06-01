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
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
