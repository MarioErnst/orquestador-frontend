namespace OrquestadorFrontend.Features.Auth;

public partial class BiometricReauthPage : ContentPage
{
    public BiometricReauthPage(BiometricReauthViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
