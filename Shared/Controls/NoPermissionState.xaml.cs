namespace OrquestadorFrontend.Shared.Controls;

// "No permission" state shown when the active role cannot see a piece of
// content the user already knows about. Does NOT apply to the Whistleblower
// channel — that module remains entirely invisible for unauthorized roles
// (absence, not refusal).
public partial class NoPermissionState : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(NoPermissionState),
        defaultValue: "Sin permiso para ver esto");

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(NoPermissionState),
        defaultValue: "Tu rol actual no tiene acceso a este contenido. Si creés que es un error, contactá al administrador.");

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

    public NoPermissionState()
    {
        InitializeComponent();
    }
}
