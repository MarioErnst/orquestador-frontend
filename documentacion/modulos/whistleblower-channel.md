# Módulo: Canal de Denuncias

## Qué hace

Implementa la entrada y vista de detalle del Canal de Denuncias en el frontend. Es el módulo más sensible del prototipo y recibe tratamiento reforzado: triple verificación de acceso, ausencia total para roles no autorizados, audit badge visible, tratamiento visual sobrio y distinto del resto de la app.

> **Lectura obligatoria adicional**: `documentacion/seguridad/whistleblower-channel.md` para el análisis completo de amenazas, mitigaciones y supuestos.

## Archivos clave

- `Features/Whistleblower/WhistleblowerListPage.xaml` / `.cs` + `WhistleblowerListViewModel.cs` — Lista de casos con estado, código de referencia y categoría. Audit badge prominente al inicio. Background en `WhistleblowerSurface` (beige sobrio) para diferenciar visualmente del resto de la app. Tap → ruta al detalle.
- `Features/Whistleblower/WhistleblowerCasePage.xaml` / `.cs` + `WhistleblowerCaseViewModel.cs` — Detalle del caso. Recibe `id` por `[QueryProperty]`. Muestra metadatos (estado, fecha de ingreso, categoría) en un card sobrio. Bloque de "Contenido del caso" es un placeholder porque el alcance lo decide el área de cumplimiento ACHS (Propuesta 1 §13, Propuesta 2 §11). Audit badge también presente.
- `Features/Whistleblower/WhistleblowerStatusConverters.cs` — Dos converters:
  - `WhistleblowerStatusLabelConverter` — `WhistleblowerStatus` → "Recibido" / "En investigación" / "Cerrado".
  - `WhistleblowerStatusColorConverter` — resuelve el color sobrio según estado contra `Application.Current.Resources` (`StatusWarn`, `StatusAlert`, `StatusOk`).

## Decisiones de tratamiento visual (Propuesta 2 §7.2)

- **Surface beige sobria** (`WhistleblowerSurface = #F5F2E8`) en lugar del fondo blanco normal. El cambio de tono comunica "espacio aparte que se trata con más cuidado" sin necesidad de explicarlo.
- **`WhistleblowerCardBorder`** en lugar de `CardBorder` normal: sin stroke, fondo más cálido, padding consistente con el resto.
- **`AuditBadge` siempre visible** al inicio de las dos pantallas. Es una señal honesta al usuario de que su consulta queda registrada (per Propuesta 1 §8.3).
- **Sin íconos alarmistas**: ni candados, ni shields rojos. La sobriedad transmite seriedad sin generar ansiedad.
- **Placeholder explícito en el detalle**: el bloque "Contenido del caso" deja documentado que el alcance está pendiente, evitando que el equipo de UI tome la decisión por defecto.

## Decisiones de acceso

- **Ausencia total para roles sin permiso**: la entrada en `HomePage` se renderiza solo si `_whistleblower.HasAccess(role)`. Para Director, el Border no existe en el visual tree. No es un botón gris ni una pantalla "Sin permiso" — es literalmente nada.
- **Triple capa de verificación** (defensa en profundidad CLAUDE.md §8):
  1. **UI**: `HomeViewModel.HasWhistleblowerAccess` controla `IsVisible` del entry point.
  2. **Shell guard**: `AppShell.OnNavigating` cancela cualquier navegación que contenga "whistleblower" si el rol no tiene acceso. Cubre deep links, notificaciones stale, intentos manuales.
  3. **Repository**: `MockWhistleblowerRepository.GetCasesAsync` / `GetCaseAsync` chequean `HasAccess` ANTES de cualquier `await` o simulación. Si el gate falla, lanzan `WhistleblowerAccessDeniedException` que el ViewModel traduce a un mensaje genérico ("No tenés acceso a este módulo") sin revelar detalles.

## Cómo se integra

- Las rutas `whistleblower` y `whistleblower/case` se registran en `AppShell.xaml.cs` con `Routing.RegisterRoute`.
- `MockData.WhistleblowerAllowedRoles = { Comite, AltaGerencia }` — Director está fuera por defecto. Decisión preservada del prototipo Flutter, confirmar con el equipo ACHS.
- `WhistleblowerCardBorder` y `WhistleblowerSurface` viven en `Resources/Styles/` para que ambos pantallas las usen consistentemente.

## Pendientes

- **Producción**: auditoría escrita por el backend ANTES de entregar el recurso (Propuesta 1 §8.3). Si el escribir auditoría falla, el acceso se rechaza con error genérico.
- **Producción**: cifrado a nivel de columna en SQL Database para los datos del Canal (Propuesta 1 §8.2). Frontend no ve datos en claro distintos de los que el backend autoriza explícitamente.
- **Decisión pendiente del cliente**: contenido completo del caso o solo metadatos en `WhistleblowerCasePage`. El placeholder actual no compromete la decisión.
- **Tests automatizados obligatorios** (gatillo CLAUDE.md §19.2): cuando llegue el endpoint real, todos los casos de acceso deben tener tests: rol autorizado → 200, rol no autorizado → 403, sin token → 401, token expirado → 401, registro de auditoría presente.
