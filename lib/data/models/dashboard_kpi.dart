// A KPI tile shown on the home screen. The visible KPIs vary by role.

enum KpiStatus {
  ok,
  warning,
  alert,
}

enum KpiTrend {
  up,
  down,
  flat,
}

class DashboardKpi {
  final String id;
  final String label;
  final String value;
  final String? unit;
  final KpiStatus status;
  final KpiTrend? trend;
  // Short helper text shown below the value (e.g., the comparison window).
  final String? helper;

  const DashboardKpi({
    required this.id,
    required this.label,
    required this.value,
    required this.status,
    this.unit,
    this.trend,
    this.helper,
  });
}
