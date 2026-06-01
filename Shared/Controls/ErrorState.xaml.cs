using System.Windows.Input;

namespace OrquestadorFrontend.Shared.Controls;

// Error state shown when a repository call fails. The message is plain
// Spanish — never a technical exception. CLAUDE.md section 9 forbids leaking
// stack traces to users; ViewModels translate exceptions before placing them
// in ResultState.Failure.
public partial class ErrorState : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(ErrorState),
        defaultValue: "No pudimos cargar la información");

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(ErrorState),
        defaultValue: "Revisá tu conexión e intentá de nuevo. Si el problema persiste, avisanos.");

    public static readonly BindableProperty RetryCommandProperty = BindableProperty.Create(
        nameof(RetryCommand), typeof(ICommand), typeof(ErrorState),
        defaultValue: null);

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

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public ErrorState()
    {
        InitializeComponent();
    }
}
