namespace OrquestadorFrontend.Shared.Controls;

// Empty state shown when a repository returns an empty collection. Must not
// read as an error: friendly tone, no alarming color.
public partial class EmptyState : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(EmptyState),
        defaultValue: "Sin contenido por ahora");

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(EmptyState),
        defaultValue: "Cuando haya información nueva, aparecerá aquí.");

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public EmptyState()
    {
        InitializeComponent();
    }
}
