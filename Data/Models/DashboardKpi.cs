namespace OrquestadorFrontend.Data.Models;

public enum KpiStatus
{
    Ok,
    Warning,
    Alert
}

public enum KpiTrend
{
    Up,
    Down,
    Flat
}

// A KPI tile shown on the home screen. The visible KPIs vary by role.
public sealed record DashboardKpi(
    string Id,
    string Label,
    string Value,
    KpiStatus Status,
    string? Unit = null,
    KpiTrend? Trend = null,
    string? Helper = null);
