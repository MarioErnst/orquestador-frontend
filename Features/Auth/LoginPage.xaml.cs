using OrquestadorFrontend.Core;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace OrquestadorFrontend.Features.Auth;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        Application.Current!.RequestedThemeChanged += OnSystemThemeChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Application.Current is { } app)
        {
            app.RequestedThemeChanged -= OnSystemThemeChanged;
        }
    }

    private void OnSystemThemeChanged(object? sender, AppThemeChangedEventArgs e)
        => HeroCanvas.InvalidateSurface();

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

    // Hero gradient. A soft radial brush from the brand green at the top
    // fading to the page surface at the bottom; a faint secondary glow on
    // the lower band adds depth without competing with the logo. Theme is
    // resolved at paint time so the appearance toggle reflows immediately.
    private void OnHeroPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var surface = e.Surface;
        var canvas = surface.Canvas;
        canvas.Clear();

        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        var brandTop = isDark
            ? new SKColor(0x4A, 0xDB, 0x6A, 0x55)
            : new SKColor(0x13, 0xC0, 0x45, 0x4D);
        var brandBottom = isDark
            ? new SKColor(0x0F, 0x10, 0x11, 0xFF)
            : new SKColor(0xFA, 0xFA, 0xFA, 0xFF);

        var topGlow = isDark
            ? new SKColor(0x27, 0x93, 0x3E, 0x33)
            : new SKColor(0x7E, 0xFF, 0x45, 0x33);

        using (var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, info.Height),
                new[] { brandTop, brandBottom },
                new[] { 0f, 1f },
                SKShaderTileMode.Clamp),
            IsAntialias = true,
        })
        {
            canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), bgPaint);
        }

        using (var glowPaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(info.Width * 0.5f, info.Height * 0.25f),
                info.Width * 0.7f,
                new[] { topGlow, SKColors.Transparent },
                new[] { 0f, 1f },
                SKShaderTileMode.Clamp),
            IsAntialias = true,
        })
        {
            canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), glowPaint);
        }
    }
}
