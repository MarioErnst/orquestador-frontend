namespace OrquestadorFrontend.Features.Whistleblower;

public partial class WhistleblowerCasePage : ContentPage
{
    public WhistleblowerCasePage(WhistleblowerCaseViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
