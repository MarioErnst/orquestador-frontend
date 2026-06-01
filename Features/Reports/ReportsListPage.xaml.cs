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
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
