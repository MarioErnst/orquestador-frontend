namespace OrquestadorFrontend.Shared.Controls;

// "Acceso registrado" pill for the Whistleblower channel screens.
// Propuesta 2 §5.6 requires it to be visible in a prominent place of the
// list/detail screens as an honest signal that every access is audited.
public partial class AuditBadge : ContentView
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(AuditBadge),
        defaultValue: "Acceso registrado");

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public AuditBadge()
    {
        InitializeComponent();
    }
}
