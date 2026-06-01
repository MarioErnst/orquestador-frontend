namespace OrquestadorFrontend.Shared.Controls;

// Section header used to separate blocks inside a screen (e.g. "Indicadores
// clave", "Accesos rápidos", "Alertas recientes" on Home). Establishes the
// title / subtitle hierarchy mandated by Propuesta 2 section 8.
//
// The optional Trailing slot is for inline actions such as "Ver todo" or
// "Configurar".
public partial class SectionHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(SectionHeader),
        defaultValue: string.Empty);

    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(SectionHeader),
        defaultValue: string.Empty,
        propertyChanged: OnSubtitleChanged);

    public static readonly BindableProperty TrailingProperty = BindableProperty.Create(
        nameof(Trailing), typeof(View), typeof(SectionHeader),
        defaultValue: null);

    public static readonly BindableProperty HasSubtitleProperty = BindableProperty.Create(
        nameof(HasSubtitle), typeof(bool), typeof(SectionHeader),
        defaultValue: false);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public View? Trailing
    {
        get => (View?)GetValue(TrailingProperty);
        set => SetValue(TrailingProperty, value);
    }

    public bool HasSubtitle => (bool)GetValue(HasSubtitleProperty);

    public SectionHeader()
    {
        InitializeComponent();
    }

    private static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var header = (SectionHeader)bindable;
        header.SetValue(HasSubtitleProperty, !string.IsNullOrEmpty(newValue as string));
    }
}
