# Módulo: Capa de datos mock

## Qué hace

Provee los datos del prototipo a través de **interfaces de repositorio**. El día que se conecte el backend, basta con sustituir las implementaciones `Mock*Repository` por `Api*Repository` que implementen los mismos contratos. Ninguna página ni ViewModel cambia.

## Archivos clave

### `Data/Models/` — Records inmutables

- `BiBoard`, `DashboardKpi` (+ `KpiStatus`, `KpiTrend`), `NotificationItem` (+ `NotificationKind`), `Report` (+ `ReportKind`), `UserProfile`, `WhistleblowerCase` (+ `WhistleblowerStatus`).
- Todos `sealed record` con parámetros posicionales. Propiedades init-only por diseño. Inmutabilidad evita mutación accidental entre páginas.

### `Data/Repositories/` — Contratos

- `IBoardsRepository`, `IDashboardRepository`, `INotificationsRepository`, `IProfileRepository`, `IReportsRepository`, `IWhistleblowerRepository`.
- Métodos `async` con sufijo `Async` y retorno `IReadOnlyList<T>` para colecciones (no `List<T>` — protege contra mutación por el caller).
- `INotificationsRepository` tiene método `MarkAsReadAsync` y manejo de preferencias (defaults a todas habilitadas).
- `IWhistleblowerRepository` expone `HasAccess(role)` además de los métodos async; las páginas y el guard de Shell lo consultan sin tocar al backend.
- `WhistleblowerAccessDeniedException` se exporta como tipo público para que callers puedan distinguir el caso de error de acceso del resto.

### `Data/Mock/` — Implementaciones

- `MockLatency.SimulateAsync()` — helper que añade 400–800 ms aleatorios para que los estados de carga sean visibles en demo.
- `MockData` — datos centralizados en español: cinco tableros Power BI (`PMC`, `Accidentabilidad`, `Cobertura`, etc.), KPIs distintos por rol, seis informes/videos/links SharePoint, cuatro casos del Canal de Denuncias, perfiles por rol.
- `Mock*Repository` (6) — cada uno:
  - Implementa la interfaz correspondiente.
  - Tiene una bandera pública `SimulateError` para demostrar el `ErrorState` durante una presentación.
  - Mantiene state en memoria donde corresponde (`MockNotificationsRepository` recuerda qué notificaciones se marcaron como leídas y las preferencias).
  - `MockWhistleblowerRepository` verifica `HasAccess` ANTES de cualquier `await` o simulación de error, garantizando que el gate no dependa de timing.

## Decisiones

- **Repositorios Singleton en DI** porque mantienen estado en memoria (notificaciones leídas, preferencias). Si fueran Transient, cada navegación perdería el read state.
- **Datos ficticios pero realistas** en español neutro/chileno — informes plausibles para una mutual de seguridad, nombres y cargos coherentes, fechas razonables alrededor del `_now = 2026-05-15` para estabilidad entre runs.
- **`IReadOnlyList<T>` en lugar de `List<T>`**: el caller no puede mutar accidentalmente la colección retornada.
- **Acceso del Canal de Denuncias en datos, no en UI**: `MockData.WhistleblowerAllowedRoles = { Comite, AltaGerencia }`. La regla vive en la capa de datos para que UI y guard de Shell consulten la misma fuente.

## Cómo se integra

- Los repositorios se registran en `MauiProgram.cs` como `Singleton`. Los ViewModels los reciben por constructor (DI).
- `MockData` es `internal static` — el resto del código solo accede a los datos vía los repositorios, nunca leyendo `MockData` directamente.

## Pendientes

- Implementaciones `Api*Repository` que hablen con el backend real (fase post-prototipo).
- Cuando exista persistencia local, decidir si las preferencias de notificación se cachean fuera de memoria.
