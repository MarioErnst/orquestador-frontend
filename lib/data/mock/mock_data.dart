import '../../core/roles.dart';
import '../models/bi_board.dart';
import '../models/dashboard_kpi.dart';
import '../models/notification_item.dart';
import '../models/report.dart';
import '../models/user_profile.dart';
import '../models/whistleblower_case.dart';

// Centralized sample data for the prototype. Realistic Spanish content
// plausible for an ACHS director-facing application. The day this prototype
// gets wired to the backend, the only thing that should change is which
// repository implementation the providers expose; none of this data ever
// reaches a production build.
//
// Whistleblower access rule (spec §5.6): Comité and Alta Gerencia have
// access; Director does not. The rule is enforced by MockWhistleblowerRepo
// and also queried by the UI to decide whether to render the entry at all.

class MockData {
  MockData._();

  static const Set<UserRole> whistleblowerAllowedRoles = {
    UserRole.comite,
    UserRole.altaGerencia,
  };

  // --- Power BI boards -----------------------------------------------------

  static final List<BiBoard> _allBoards = [
    const BiBoard(
      id: 'pmc',
      name: 'PMC',
      description: 'Panel de Mando Corporativo. Indicadores estratégicos.',
    ),
    const BiBoard(
      id: 'accidentabilidad',
      name: 'Indicadores de Accidentabilidad',
      description: 'Tasa y severidad por sector productivo.',
    ),
    const BiBoard(
      id: 'cobertura',
      name: 'Cobertura de Empresas Afiliadas',
      description: 'Evolución mensual y proyección de cobertura.',
    ),
    const BiBoard(
      id: 'respuesta-hospitalaria',
      name: 'Tiempos de Respuesta Hospitalaria',
      description: 'Atención de urgencia en la red ACHS.',
    ),
    const BiBoard(
      id: 'auditorias',
      name: 'Cumplimiento de Auditorías',
      description: 'Hallazgos abiertos y cerrados por área.',
    ),
  ];

  static List<BiBoard> boardsForRole(UserRole role) {
    switch (role) {
      case UserRole.director:
        return _allBoards
            .where((b) => b.id == 'pmc' || b.id == 'accidentabilidad')
            .toList();
      case UserRole.comite:
        return _allBoards
            .where((b) => b.id != 'respuesta-hospitalaria')
            .toList();
      case UserRole.altaGerencia:
        return _allBoards;
    }
  }

  // --- KPIs ----------------------------------------------------------------

  static List<DashboardKpi> kpisForRole(UserRole role) {
    switch (role) {
      case UserRole.director:
        return const [
          DashboardKpi(
            id: 'kpi-tasa-accidentabilidad',
            label: 'Tasa de accidentabilidad',
            value: '2,9',
            unit: '%',
            status: KpiStatus.warning,
            trend: KpiTrend.down,
            helper: 'vs. trimestre anterior',
          ),
          DashboardKpi(
            id: 'kpi-cobertura',
            label: 'Cobertura empresas',
            value: '68,4',
            unit: '%',
            status: KpiStatus.ok,
            trend: KpiTrend.up,
            helper: 'meta anual 70%',
          ),
          DashboardKpi(
            id: 'kpi-satisfaccion',
            label: 'Satisfacción usuaria',
            value: '4,3',
            unit: '/ 5',
            status: KpiStatus.ok,
            trend: KpiTrend.flat,
          ),
        ];
      case UserRole.comite:
        return const [
          DashboardKpi(
            id: 'kpi-auditorias',
            label: 'Auditorías cerradas',
            value: '12 / 18',
            status: KpiStatus.warning,
            trend: KpiTrend.up,
            helper: 'período en curso',
          ),
          DashboardKpi(
            id: 'kpi-riesgos-criticos',
            label: 'Riesgos críticos abiertos',
            value: '3',
            status: KpiStatus.alert,
            trend: KpiTrend.flat,
          ),
          DashboardKpi(
            id: 'kpi-cumplimiento',
            label: 'Cumplimiento regulatorio',
            value: '97,1',
            unit: '%',
            status: KpiStatus.ok,
            trend: KpiTrend.up,
          ),
        ];
      case UserRole.altaGerencia:
        return const [
          DashboardKpi(
            id: 'kpi-productividad',
            label: 'Productividad operativa',
            value: '108',
            unit: 'idx',
            status: KpiStatus.ok,
            trend: KpiTrend.up,
            helper: 'base 100',
          ),
          DashboardKpi(
            id: 'kpi-ebitda',
            label: 'EBITDA acumulado',
            value: '142',
            unit: 'MM CLP',
            status: KpiStatus.ok,
            trend: KpiTrend.up,
          ),
          DashboardKpi(
            id: 'kpi-rotacion',
            label: 'Rotación de personal',
            value: '6,8',
            unit: '%',
            status: KpiStatus.warning,
            trend: KpiTrend.up,
            helper: 'vs. mismo trimestre',
          ),
          DashboardKpi(
            id: 'kpi-tickets',
            label: 'Tickets críticos abiertos',
            value: '4',
            status: KpiStatus.alert,
            trend: KpiTrend.flat,
          ),
        ];
    }
  }

  // --- Reports / Documents -------------------------------------------------

  static final DateTime _now = DateTime(2026, 5, 15);

  static final List<Report> _allReports = [
    Report(
      id: 'rep-memoria-2025',
      title: 'Memoria Anual 2025',
      description: 'Resumen institucional, financiero y operativo del año.',
      kind: ReportKind.pdf,
      publishedAt: _now.subtract(const Duration(days: 21)),
      aiSummary:
          'La Memoria Anual 2025 destaca una baja de 0,4 puntos en la tasa de '
          'accidentabilidad respecto al año previo, alcanzando 2,9%. El programa '
          'de prevención en sector construcción y la modernización de la red '
          'hospitalaria son los factores principales del avance. El documento '
          'detalla además la inversión en infraestructura por 18 mil millones '
          'de pesos y la expansión de cobertura a 7 nuevas regiones.',
    ),
    Report(
      id: 'rep-sustentabilidad-q1',
      title: 'Informe de Sustentabilidad Q1 2026',
      description: 'Avance del plan ESG, métricas trimestrales.',
      kind: ReportKind.pdf,
      publishedAt: _now.subtract(const Duration(days: 9)),
      aiSummary:
          'El reporte trimestral muestra cumplimiento del 87% de las metas ESG '
          'comprometidas, con foco en reducción de huella de carbono y '
          'fortalecimiento de programas de salud ocupacional. Tres áreas '
          'requieren atención prioritaria del Directorio.',
    ),
    Report(
      id: 'rep-video-directorio',
      title: 'Mensaje del Directorio - Cierre de año',
      description: 'Video institucional, duración 7 minutos.',
      kind: ReportKind.video,
      publishedAt: _now.subtract(const Duration(days: 4)),
    ),
    Report(
      id: 'rep-plan-accidentes',
      title: 'Plan de Acción Frente a Accidentes Graves',
      description: 'Procedimientos actualizados según norma vigente.',
      kind: ReportKind.pdf,
      publishedAt: _now.subtract(const Duration(days: 35)),
      aiSummary:
          'El plan incorpora dos nuevos protocolos de respuesta inmediata para '
          'siniestros en faenas mineras y portuarias. Define responsables por '
          'región, tiempos máximos de despliegue y matriz de comunicación con '
          'autoridades.',
    ),
    Report(
      id: 'rep-sharepoint-comite',
      title: 'Carpeta Comité de Auditoría 2026',
      description: 'Acceso a SharePoint corporativo (permisos heredados).',
      kind: ReportKind.sharepointLink,
      publishedAt: _now.subtract(const Duration(days: 60)),
    ),
    Report(
      id: 'rep-video-bienvenida',
      title: 'Video de Bienvenida para Nuevos Directores',
      description: 'Inducción institucional, duración 12 minutos.',
      kind: ReportKind.video,
      publishedAt: _now.subtract(const Duration(days: 75)),
    ),
  ];

  static List<Report> reportsForRole(UserRole role) {
    // The compliance SharePoint folder is comite/altaGerencia only.
    return _allReports.where((r) {
      if (r.id == 'rep-sharepoint-comite' && role == UserRole.director) {
        return false;
      }
      return true;
    }).toList();
  }

  // --- Notifications -------------------------------------------------------

  static List<NotificationItem> notificationsForRole(UserRole role) {
    final base = <NotificationItem>[
      NotificationItem(
        id: 'not-1',
        kind: NotificationKind.newBoard,
        title: 'Nuevo tablero disponible',
        body:
            'Se publicó el tablero "Cumplimiento de Auditorías" para tu perfil.',
        receivedAt: _now.subtract(const Duration(hours: 4)),
        targetRef: 'auditorias',
      ),
      NotificationItem(
        id: 'not-2',
        kind: NotificationKind.indicatorThresholdExceeded,
        title: 'Indicador supera umbral',
        body:
            'La tasa de accidentabilidad superó el 3% en la regional centro.',
        receivedAt: _now.subtract(const Duration(hours: 22)),
        targetRef: 'accidentabilidad',
      ),
      NotificationItem(
        id: 'not-3',
        kind: NotificationKind.newReport,
        title: 'Nuevo informe publicado',
        body: 'Informe de Sustentabilidad Q1 2026 disponible.',
        receivedAt: _now.subtract(const Duration(days: 2)),
        targetRef: 'rep-sustentabilidad-q1',
        read: true,
      ),
    ];
    if (whistleblowerAllowedRoles.contains(role)) {
      base.add(
        NotificationItem(
          id: 'not-4',
          kind: NotificationKind.whistleblowerAlert,
          title: 'Caso de canal de denuncias actualizado',
          body: 'El caso DEN-2026-014 pasó a "En investigación".',
          receivedAt: _now.subtract(const Duration(days: 1)),
          targetRef: 'case-014',
        ),
      );
    }
    return base;
  }

  // --- Whistleblower cases -------------------------------------------------

  static final List<WhistleblowerCase> _whistleblowerCases = [
    WhistleblowerCase(
      id: 'case-014',
      referenceCode: 'DEN-2026-014',
      status: WhistleblowerStatus.inInvestigation,
      category: 'Conflicto de interés',
      receivedAt: _now.subtract(const Duration(days: 6)),
    ),
    WhistleblowerCase(
      id: 'case-012',
      referenceCode: 'DEN-2026-012',
      status: WhistleblowerStatus.received,
      category: 'Uso indebido de recursos',
      receivedAt: _now.subtract(const Duration(days: 2)),
    ),
    WhistleblowerCase(
      id: 'case-007',
      referenceCode: 'DEN-2026-007',
      status: WhistleblowerStatus.closed,
      category: 'Acoso laboral',
      receivedAt: _now.subtract(const Duration(days: 41)),
    ),
    WhistleblowerCase(
      id: 'case-003',
      referenceCode: 'DEN-2026-003',
      status: WhistleblowerStatus.closed,
      category: 'Irregularidad administrativa',
      receivedAt: _now.subtract(const Duration(days: 70)),
    ),
  ];

  static List<WhistleblowerCase> whistleblowerCases() =>
      List.unmodifiable(_whistleblowerCases);

  // --- User profile --------------------------------------------------------

  static UserProfile profileForRole(UserRole role) {
    switch (role) {
      case UserRole.director:
        return const UserProfile(
          id: 'usr-dir-001',
          fullName: 'Carlos Pérez González',
          email: 'cperez@directorio.achs.cl',
          position: 'Miembro del Directorio',
          role: UserRole.director,
        );
      case UserRole.comite:
        return const UserProfile(
          id: 'usr-com-001',
          fullName: 'María Rojas Inostroza',
          email: 'mrojas@directorio.achs.cl',
          position: 'Comité de Auditoría',
          role: UserRole.comite,
        );
      case UserRole.altaGerencia:
        return const UserProfile(
          id: 'usr-ag-001',
          fullName: 'Juan Soto Valdés',
          email: 'jsoto@gerencia.achs.cl',
          position: 'Gerente General',
          role: UserRole.altaGerencia,
        );
    }
  }
}
