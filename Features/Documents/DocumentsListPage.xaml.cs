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
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
