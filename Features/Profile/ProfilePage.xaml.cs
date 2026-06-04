namespace OrquestadorFrontend.Features.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _vm;

    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
        AnimateIn();
    }

    // Staggered fade-in + upward slide: hero first, then cards 80 ms later.
    // Both start at Opacity=0 (set in XAML) so nothing flashes on navigate-in.
    private void AnimateIn()
    {
        HeroSection.TranslationY = 12;
        CardsSection.TranslationY = 16;

        var heroFade = HeroSection.FadeTo(1, 300, Easing.CubicOut);
        var heroSlide = HeroSection.TranslateTo(0, 0, 300, Easing.CubicOut);

        Task.Delay(80).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() =>
        {
            var cardsFade = CardsSection.FadeTo(1, 280, Easing.CubicOut);
            var cardsSlide = CardsSection.TranslateTo(0, 0, 280, Easing.CubicOut);
        }));

        _ = Task.WhenAll(heroFade, heroSlide);
    }
}
