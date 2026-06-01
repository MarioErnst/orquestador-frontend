namespace OrquestadorFrontend.Shared.Controls;

// Loading state shown while a ViewModel is fetching data. Per the design
// spec the screen must never go blank; this control always renders the
// activity indicator and a calm message.
public partial class LoadingState : ContentView
{
    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(LoadingState),
        defaultValue: "Cargando…");

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingState()
    {
        InitializeComponent();
    }
}
