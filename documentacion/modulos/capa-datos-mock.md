# Capa de datos (mock)

## Propósito

Implementa la **Baranda 1** de la sección 3 de la spec del prototipo:
separación estricta entre UI y datos. Toda pantalla del prototipo va a
consultar un repositorio, y hoy ese repositorio devuelve datos de ejemplo
(mock). El día que se conecte el backend, solo se reemplaza la
implementación; las pantallas no se enteran.

## Ubicación

```
lib/data/
  models/                  # tipos puros inmutables del dominio
  repositories/            # interfaces (contratos) que la UI consume
  mock/                    # implementaciones mock + datos de ejemplo
    _latency.dart          # helper compartido de latencia simulada
    mock_data.dart         # datos de ejemplo centralizados
    mock_*_repository.dart # una implementación por dominio
```

## Modelos (`lib/data/models/`)

| Archivo | Modelo | Notas |
|---|---|---|
| `report.dart` | `Report` + `ReportKind { pdf, video, sharepointLink }` | Item de la feature Documentos. PDFs llevan `aiSummary` opcional para el bloque "Resumen generado por IA" del visor. |
| `dashboard_kpi.dart` | `DashboardKpi` + `KpiStatus`, `KpiTrend` | Tile del Inicio. El estado se acompaña con texto/ícono, nunca solo color. |
| `bi_board.dart` | `BiBoard` | Entrada de tablero Power BI. El filtrado por rol lo hace el repo. |
| `notification_item.dart` | `NotificationItem` + `NotificationKind` | Cuatro tipos: nuevo tablero, indicador supera umbral, nuevo informe, alerta del canal de denuncias. |
| `whistleblower_case.dart` | `WhistleblowerCase` + `WhistleblowerStatus` | Caso del canal de denuncias. `bodyPlaceholder` queda pendiente de definición del área de cumplimiento. |
| `user_profile.dart` | `UserProfile` | Datos del usuario logueado; en producción viene de Entra ID. |

Todos los modelos son `class` (no `record`) con campos `final` y constructor
`const`. Sin generadores de código por ahora.

## Repositorios — interfaces (`lib/data/repositories/`)

Una interfaz por dominio. Cada interfaz expone solo lo que la UI necesita
pedirle al sistema. No hay "métodos por las dudas".

- `ReportsRepository`: `getReports(role)`, `getReport(id, role)`.
- `BoardsRepository`: `getBoards(role)`, `getBoard(id, role)`.
- `NotificationsRepository`: `getNotifications(role)`, `getPreferences()`,
  `setPreference(kind, enabled)`.
- `WhistleblowerRepository`: `hasAccess(role)` (síncrono — es regla por rol),
  `getCases(role)`, `getCase(id, role)`.
- `ProfileRepository`: `getProfile(activeRole)`.

`WhistleblowerRepository` también exporta `WhistleblowerAccessDenied`: una
excepción que se lanza si alguien llama a `getCases`/`getCase` con un rol sin
permiso. La UI no debería llegar a verla nunca (la entrada al módulo no se
renderiza para esos roles), pero la capa de datos no asume eso: re-valida.

## Implementaciones mock (`lib/data/mock/`)

### Latencia simulada — `_latency.dart`

```dart
Future<void> simulateLatency(); // delay 400–800 ms aleatorio
```

Todas las llamadas mock pasan por este `await` para que los estados de
"cargando" sean visibles y la demo se sienta real (sección 7 de la spec).

### Datos centralizados — `mock_data.dart`

Reúne **todos** los datos de ejemplo del prototipo:

- 5 tableros Power BI: `PMC` (pedido explícito de la spec), Accidentabilidad,
  Cobertura, Respuesta Hospitalaria, Auditorías. El filtrado por rol se hace
  en `boardsForRole(role)`.
- KPIs distintos por rol (3 a 4 por rol) con valores realistas para una
  mutual de seguridad.
- 6 documentos: mix de PDFs (con resumen IA mock), un video institucional y
  un enlace SharePoint. El SharePoint del Comité de Auditoría queda excluido
  para Director.
- 3–4 notificaciones, agregando la alerta del canal de denuncias solo para
  Comité/Alta Gerencia.
- 4 casos de canal de denuncias con códigos `DEN-2026-XXX`.
- Un `UserProfile` por rol con nombre, posición y correo plausibles.

Todo el texto está en **español neutro/chileno**, con valores plausibles
para ACHS (montos en CLP, regiones, categorías de riesgo).

### Repositorios mock

Cada `Mock*Repository` implementa la interfaz correspondiente. Comparten dos
detalles:

- Una bandera pública `bool simulateError = false` que, cuando se prende, hace
  que cualquier llamada lance una excepción. Útil para demostrar el
  `error_state` en la presentación al cliente.
- Latencia artificial (`simulateLatency()`) antes de devolver, para
  visibilizar los estados de "cargando".

`MockNotificationsRepository` además guarda las preferencias en un `Map`
in-memory, que dura toda la sesión.

`MockWhistleblowerRepository` **chequea acceso antes que latencia**: si el
rol no tiene permiso lanza `WhistleblowerAccessDenied` inmediatamente. Es
otra defensa además de la ausencia visual del módulo (sección 5.6 de la
spec).

## Regla de seguridad: acceso al Canal de Denuncias

Definida una sola vez en `mock_data.dart`:

```dart
static const Set<UserRole> whistleblowerAllowedRoles = {
  UserRole.comite,
  UserRole.altaGerencia,
};
```

Esa constante:

1. Define quién accede al módulo. Director queda fuera por defecto.
2. La consulta `MockWhistleblowerRepository.hasAccess` que la UI usará para
   **no renderizar** la entrada al canal en pantallas como Inicio.
3. La validan internamente los métodos `getCases`/`getCase` para que la
   capa de datos no confíe ciegamente en la UI (defensa en profundidad —
   sección 8 del `CLAUDE.md` raíz).

Cuando el cliente confirme el rol exacto que accede, este `Set` se actualiza
en un único commit y se documenta el motivo.

## Lo que esta rama NO hace (queda para ramas siguientes)

- **Providers de Riverpod**: la UI no consume aún los repos. Se exponen sin
  contenedor de DI; la próxima rama agrega `flutter_riverpod` y los providers
  de `repository_providers.dart` y `session_provider.dart`.
- **Navegación con go_router**: ninguna ruta todavía. El `home` del
  `MaterialApp` es un placeholder que muestra el tema aplicado.
- **Pantallas**: `lib/features/*` no existe.
- **Componentes compartidos** (`loading_state`, `empty_state`, etc.): se
  construyen junto con la primera pantalla que los necesite.

## Verificación

- `flutter analyze`: sin issues.
- `flutter test`: 1 test (smoke test del widget raíz), en verde.

La capa de datos no tiene tests propios en esta rama. Cuando se incorporen
las pantallas, los tests del happy/error path por feature usarán los mocks
con `simulateError` para cubrir el `error_state`.
