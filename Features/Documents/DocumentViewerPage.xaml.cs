namespace OrquestadorFrontend.Features.Documents;

public partial class DocumentViewerPage : ContentPage
{
    public DocumentViewerPage(DocumentViewerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
