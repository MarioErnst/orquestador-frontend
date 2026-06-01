namespace OrquestadorFrontend.Features.Reports;

public partial class BoardViewerPage : ContentPage
{
    public BoardViewerPage(BoardViewerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
