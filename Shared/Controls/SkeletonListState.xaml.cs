namespace OrquestadorFrontend.Shared.Controls;

// Placeholder skeleton shown while a list is loading for the first time.
// Pulses opacity in a sinusoidal loop to communicate activity without
// an ActivityIndicator that obscures the content area.
public partial class SkeletonListState : ContentView
{
    public SkeletonListState()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e) => StartShimmer();
    private void OnUnloaded(object? sender, EventArgs e) => this.AbortAnimation("Shimmer");

    private void StartShimmer()
    {
        var animation = new Animation(v => Opacity = v, 0.35, 1.0, Easing.SinInOut);
        animation.Commit(this, "Shimmer", length: 900, repeat: () => IsVisible);
    }
}
