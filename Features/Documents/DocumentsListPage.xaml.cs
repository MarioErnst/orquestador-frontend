namespace OrquestadorFrontend.Features.Documents;

public partial class DocumentsListPage : ContentPage
{
    private readonly DocumentsListViewModel _vm;

    public DocumentsListPage(DocumentsListViewModel vm)
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
