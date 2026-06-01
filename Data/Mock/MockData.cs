using OrquestadorFrontend.Core;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Data.Mock;

// Centralized sample data for the prototype. Realistic Spanish content
// plausible for an ACHS director-facing application. The day this prototype
// gets wired to the backend, the only thing that should change is which
// repository implementation DI resolves; none of this data ever reaches a
// production build.
//
// Whistleblower access rule (Propuesta 1 section 8, Propuesta 2 section 4.6):
// Comite and AltaGerencia have access; Director does not. The rule is
// enforced by MockWhistleblowerRepository and also queried by the UI to
// decide whether to render the entry at all.
internal static class MockData
{
    public static readonly IReadOnlySet<UserRole> WhistleblowerAllowedRoles =
        new HashSet<UserRole> { UserRole.Comite, UserRole.AltaGerencia };

    // Stable reference date so all dates render the same on every run.
    private static readonly DateTime _now = new(2026, 5, 15);

    // --- Power BI boards ---------------------------------------------------

    private static readonly IReadOnlyList<BiBoard> _allBoards = new BiBoard[]
    {
        new("pmc",
            "PMC",
            "Panel de Mando Corporativo. Indicadores estratégicos."),
        new("accidentabilidad",
            "Indicadores de Accidentabilidad",
            "Tasa y severidad por sector productivo."),
        new("cobertura",
            "Cobertura de Empresas Afiliadas",
            "Evolución mensual y proyección de cobertura."),
        new("respuesta-hospitalaria",
            "Tiempos de Respuesta Hospitalaria",
            "Atención de urgencia en la red ACHS."),
        new("auditorias",
            "Cumplimiento de Auditorías",
            "Hallazgos abiertos y cerrados por área.")
    };

    public static IReadOnlyList<BiBoard> BoardsForRole(UserRole role) => role switch
    {
        UserRole.Director => _allBoards.Where(b => b.Id is "pmc" or "accidentabilidad").ToList(),
        UserRole.Comite => _allBoards.Where(b => b.Id != "respuesta-hospitalaria").ToList(),
        UserRole.AltaGerencia => _allBoards,
        _ => Array.Empty<BiBoard>()
    };

    // --- KPIs --------------------------------------------------------------

    public static IReadOnlyList<DashboardKpi> KpisForRole(UserRole role) => role switch
    {
        UserRole.Director => new DashboardKpi[]
        {
            new("kpi-tasa-accidentabilidad",
                "Tasa de accidentabilidad",
                "2,9",
                KpiStatus.Warning,
                Unit: "%",
                Trend: KpiTrend.Down,
                Helper: "vs. trimestre anterior"),
            new("kpi-cobertura",
                "Cobertura empresas",
                "68,4",
                KpiStatus.Ok,
                Unit: "%",
                Trend: KpiTrend.Up,
                Helper: "meta anual 70%"),
            new("kpi-satisfaccion",
                "Satisfacción usuaria",
                "4,3",
                KpiStatus.Ok,
                Unit: "/ 5",
                Trend: KpiTrend.Flat)
        },
        UserRole.Comite => new DashboardKpi[]
        {
            new("kpi-auditorias",
                "Auditorías cerradas",
                "12 / 18",
                KpiStatus.Warning,
                Trend: KpiTrend.Up,
                Helper: "período en curso"),
            new("kpi-riesgos-criticos",
                "Riesgos críticos abiertos",
                "3",
                KpiStatus.Alert,
                Trend: KpiTrend.Flat),
            new("kpi-cumplimiento",
                "Cumplimiento regulatorio",
                "97,1",
                KpiStatus.Ok,
                Unit: "%",
                Trend: KpiTrend.Up)
        },
        UserRole.AltaGerencia => new DashboardKpi[]
        {
            new("kpi-productividad",
                "Productividad operativa",
                "108",
                KpiStatus.Ok,
                Unit: "idx",
                Trend: KpiTrend.Up,
                Helper: "base 100"),
            new("kpi-ebitda",
                "EBITDA acumulado",
                "142",
                KpiStatus.Ok,
                Unit: "MM CLP",
                Trend: KpiTrend.Up),
            new("kpi-rotacion",
                "Rotación de personal",
                "6,8",
                KpiStatus.Warning,
                Unit: "%",
                Trend: KpiTrend.Up,
                Helper: "vs. mismo trimestre"),
            new("kpi-tickets",
                "Tickets críticos abiertos",
                "4",
                KpiStatus.Alert,
                Trend: KpiTrend.Flat)
        },
        _ => Array.Empty<DashboardKpi>()
    };

    // --- Reports / Documents -----------------------------------------------

    private static readonly IReadOnlyList<Report> _allReports = new Report[]
    {
        new("rep-memoria-2025",
            "Memoria Anual 2025",
            "Resumen institucional, financiero y operativo del año.",
            ReportKind.Pdf,
            _now.AddDays(-21),
            AiSummary:
                "La Memoria Anual 2025 destaca una baja de 0,4 puntos en la tasa " +
                "de accidentabilidad respecto al año previo, alcanzando 2,9%. El " +
                "programa de prevención en sector construcción y la modernización " +
                "de la red hospitalaria son los factores principales del avance. " +
                "El documento detalla además la inversión en infraestructura por " +
                "18 mil millones de pesos y la expansión de cobertura a 7 nuevas " +
                "regiones."),
        new("rep-sustentabilidad-q1",
            "Informe de Sustentabilidad Q1 2026",
            "Avance del plan ESG, métricas trimestrales.",
            ReportKind.Pdf,
            _now.AddDays(-9),
            AiSummary:
                "El reporte trimestral muestra cumplimiento del 87% de las metas " +
                "ESG comprometidas, con foco en reducción de huella de carbono y " +
                "fortalecimiento de programas de salud ocupacional. Tres áreas " +
                "requieren atención prioritaria del Directorio."),
        new("rep-video-directorio",
            "Mensaje del Directorio - Cierre de año",
            "Video institucional, duración 7 minutos.",
            ReportKind.Video,
            _now.AddDays(-4)),
        new("rep-plan-accidentes",
            "Plan de Acción Frente a Accidentes Graves",
            "Procedimientos actualizados según norma vigente.",
            ReportKind.Pdf,
            _now.AddDays(-35),
            AiSummary:
                "El plan incorpora dos nuevos protocolos de respuesta inmediata " +
                "para siniestros en faenas mineras y portuarias. Define " +
                "responsables por región, tiempos máximos de despliegue y matriz " +
                "de comunicación con autoridades."),
        new("rep-sharepoint-comite",
            "Carpeta Comité de Auditoría 2026",
            "Acceso a SharePoint corporativo (permisos heredados).",
            ReportKind.SharepointLink,
            _now.AddDays(-60)),
        new("rep-video-bienvenida",
            "Video de Bienvenida para Nuevos Directores",
            "Inducción institucional, duración 12 minutos.",
            ReportKind.Video,
            _now.AddDays(-75))
    };

    public static IReadOnlyList<Report> ReportsForRole(UserRole role)
    {
        // The compliance SharePoint folder is comite/altaGerencia only.
        return _allReports.Where(r =>
            !(r.Id == "rep-sharepoint-comite" && role == UserRole.Director)).ToList();
    }

    // --- Notifications -----------------------------------------------------

    public static IReadOnlyList<NotificationItem> NotificationsForRole(UserRole role)
    {
        var list = new List<NotificationItem>
        {
            new("not-1",
                NotificationKind.NewBoard,
                "Nuevo tablero disponible",
                "Se publicó el tablero \"Cumplimiento de Auditorías\" para tu perfil.",
                _now.AddHours(-4),
                TargetRef: "auditorias"),
            new("not-2",
                NotificationKind.IndicatorThresholdExceeded,
                "Indicador supera umbral",
                "La tasa de accidentabilidad superó el 3% en la regional centro.",
                _now.AddHours(-22),
                TargetRef: "accidentabilidad"),
            new("not-3",
                NotificationKind.NewReport,
                "Nuevo informe publicado",
                "Informe de Sustentabilidad Q1 2026 disponible.",
                _now.AddDays(-2),
                TargetRef: "rep-sustentabilidad-q1",
                Read: true)
        };

        if (WhistleblowerAllowedRoles.Contains(role))
        {
            list.Add(new NotificationItem(
                "not-4",
                NotificationKind.WhistleblowerAlert,
                "Caso de canal de denuncias actualizado",
                "El caso DEN-2026-014 pasó a \"En investigación\".",
                _now.AddDays(-1),
                TargetRef: "case-014"));
        }

        return list;
    }

    // --- Whistleblower cases -----------------------------------------------

    private static readonly IReadOnlyList<WhistleblowerCase> _whistleblowerCases = new WhistleblowerCase[]
    {
        new("case-014",
            "DEN-2026-014",
            WhistleblowerStatus.InInvestigation,
            "Conflicto de interés",
            _now.AddDays(-6)),
        new("case-012",
            "DEN-2026-012",
            WhistleblowerStatus.Received,
            "Uso indebido de recursos",
            _now.AddDays(-2)),
        new("case-007",
            "DEN-2026-007",
            WhistleblowerStatus.Closed,
            "Acoso laboral",
            _now.AddDays(-41)),
        new("case-003",
            "DEN-2026-003",
            WhistleblowerStatus.Closed,
            "Irregularidad administrativa",
            _now.AddDays(-70))
    };

    public static IReadOnlyList<WhistleblowerCase> WhistleblowerCases() => _whistleblowerCases;

    // --- User profile ------------------------------------------------------

    public static UserProfile ProfileForRole(UserRole role) => role switch
    {
        UserRole.Director => new UserProfile(
            "usr-dir-001",
            "Carlos Pérez González",
            "cperez@directorio.achs.cl",
            "Miembro del Directorio",
            UserRole.Director),
        UserRole.Comite => new UserProfile(
            "usr-com-001",
            "María Rojas Inostroza",
            "mrojas@directorio.achs.cl",
            "Comité de Auditoría",
            UserRole.Comite),
        UserRole.AltaGerencia => new UserProfile(
            "usr-ag-001",
            "Juan Soto Valdés",
            "jsoto@gerencia.achs.cl",
            "Gerente General",
            UserRole.AltaGerencia),
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}
