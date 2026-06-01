namespace OrquestadorFrontend.Features.Documents.Controls;

// AI summary block shown next to the original document. The label and the
// "asistencia a la lectura" note enforce the rule from Propuesta 1 §9.1:
// the IA assists, it does not replace the source.
public partial class AiSummaryBlock : ContentView
{
    public static readonly BindableProperty SummaryProperty = BindableProperty.Create(
        nameof(Summary), typeof(string), typeof(AiSummaryBlock), string.Empty);

    public string Summary
    {
        get => (string)GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    public AiSummaryBlock()
    {
        InitializeComponent();
    }
}
