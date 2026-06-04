namespace OrquestadorFrontend.Features.Reports;

public partial class ReportsListPage : ContentPage
{
    private readonly ReportsListViewModel _vm;

    public ReportsListPage(ReportsListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Opacity = 0;
        _ = this.FadeToAsync(1, 280, Easing.CubicOut);
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
