using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Features.Home.Controls;

// A KPI tile shown on the home dashboard. Renders the brand-styled card with
// label, value, unit, status pill and optional helper line. Status colour
// derives from KpiStatus, not from a free-form palette: a tablet of KPIs
// stays semantically consistent.
public partial class KpiCard : ContentView
{
    public static readonly BindableProperty KpiProperty = BindableProperty.Create(
        nameof(Kpi), typeof(DashboardKpi), typeof(KpiCard),
        propertyChanged: OnKpiChanged);

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty UnitProperty = BindableProperty.Create(
        nameof(Unit), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty StatusLabelProperty = BindableProperty.Create(
        nameof(StatusLabel), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty StatusColorProperty = BindableProperty.Create(
        nameof(StatusColor), typeof(Color), typeof(KpiCard), Colors.Transparent);

    public static readonly BindableProperty HelperProperty = BindableProperty.Create(
        nameof(Helper), typeof(string), typeof(KpiCard), string.Empty);

    public static readonly BindableProperty HasHelperProperty = BindableProperty.Create(
        nameof(HasHelper), typeof(bool), typeof(KpiCard), false);

    public DashboardKpi? Kpi
    {
        get => (DashboardKpi?)GetValue(KpiProperty);
        set => SetValue(KpiProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string StatusLabel
    {
        get => (string)GetValue(StatusLabelProperty);
        set => SetValue(StatusLabelProperty, value);
    }

    public Color StatusColor
    {
        get => (Color)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public string Helper
    {
        get => (string)GetValue(HelperProperty);
        set => SetValue(HelperProperty, value);
    }

    public bool HasHelper
    {
        get => (bool)GetValue(HasHelperProperty);
        set => SetValue(HasHelperProperty, value);
    }

    public KpiCard()
    {
        InitializeComponent();
    }

    private static void OnKpiChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not KpiCard card || newValue is not DashboardKpi kpi) return;

        card.Label = kpi.Label;
        card.Value = kpi.Value;
        card.Unit = kpi.Unit ?? string.Empty;
        card.Helper = kpi.Helper ?? string.Empty;
        card.HasHelper = !string.IsNullOrEmpty(kpi.Helper);

        var (statusLabel, statusColor) = ResolveStatus(kpi.Status);
        card.StatusLabel = statusLabel;
        card.StatusColor = statusColor;
    }

    private static (string Label, Color Color) ResolveStatus(KpiStatus status)
    {
        var ok = (Color)Application.Current!.Resources["StatusOk"];
        var warn = (Color)Application.Current.Resources["StatusWarn"];
        var alert = (Color)Application.Current.Resources["StatusAlert"];

        return status switch
        {
            KpiStatus.Ok => ("● En meta", ok),
            KpiStatus.Warning => ("● Atención", warn),
            KpiStatus.Alert => ("● Crítico", alert),
            _ => (string.Empty, Colors.Transparent)
        };
    }
}
