using OrquestadorFrontend.Core;

namespace OrquestadorFrontend.Features.Auth;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    // PROTOTYPE: the role picker stands in for the real Entra ID claim. In
    // production this code-behind disappears — the role comes from the token.
    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var pick = await DisplayActionSheetAsync(
            title: "Ingresar como (prototipo)",
            cancel: "Cancelar",
            destruction: null,
            buttons: new[] { "Director", "Comité", "Alta Gerencia" });

        var role = pick switch
        {
            "Director" => (UserRole?)UserRole.Director,
            "Comité" => UserRole.Comite,
            "Alta Gerencia" => UserRole.AltaGerencia,
            _ => null
        };

        if (role is not null && _vm.SignInCommand.CanExecute(role))
        {
            await _vm.SignInCommand.ExecuteAsync(role);
        }
    }
}
