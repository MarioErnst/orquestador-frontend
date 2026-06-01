# Módulo: Notifications y Profile

## Qué hace

**Notifications**: historial de notificaciones push recibidas + pantalla de preferencias (un switch por tipo de alerta). Tap en una notificación → ruta al contenido relacionado, vía `INotificationNavigationService`.

**Profile**: pantalla de soporte con datos del usuario, rol activo y botón de cerrar sesión con confirmación.

## Archivos clave

### Notifications

- `Features/Notifications/NotificationsPage.xaml` / `.cs` + `NotificationsViewModel.cs` — Lista de notificaciones ordenadas por fecha descendente. Cada item tiene `Title`, `Body` y un punto verde (`Ellipse`) si está no leída. Tap → `MarkAsReadAsync` + navegación según el kind. Toolbar item "Preferencias" lleva a la pantalla de configuración.
- `Features/Notifications/NotificationPreferencesPage.xaml` / `.cs` + `NotificationPreferencesViewModel.cs` — Lista de `PreferenceItem` (clase wrapper con `ObservableProperty Enabled`). Cambiar el `Switch` invoca `_parent.SetPreferenceAsync(kind, enabled)` que persiste vía `INotificationsRepository`.

### Profile

- `Features/Profile/ProfilePage.xaml` / `.cs` + `ProfileViewModel.cs` — Card con nombre completo, posición, correo y rol del usuario. Botón outlined "Cerrar sesión" con `DisplayAlert` de confirmación. Mensaje explicativo: "La gestión de identidad y permisos vive en los servicios corporativos".

### Servicios

- `Services/INotificationNavigationService.cs` + `NotificationNavigationService.cs` — Mapea `NotificationKind` + `TargetRef` a una ruta Shell:
  - `NewBoard` / `IndicatorThresholdExceeded` → `board?id=...`
  - `NewReport` → `document?id=...`
  - `WhistleblowerAlert` → `whistleblower/case?id=...`
  - El `TargetRef` se escapa con `Uri.EscapeDataString` para que un id con caracteres reservados no rompa la ruta.

## Decisiones

- **`PreferenceItem` como wrapper observable**: cada switch necesita reaccionar al toggle del usuario y persistir. Modelar como item ObservableObject permite usar `partial void OnEnabledChanged` para llamar al repo sin cablear handlers en code-behind.
- **`MarkAsRead` ANTES de navegar**: la operación es idempotente y barata. Se espera (`await`) para que el repo registre el cambio antes de navegar; al volver a Home, el item ya no muestra el punto verde.
- **`SignOut` con confirmación**: la app de directivos tiene información sensible. Cerrar sesión por accidente cuesta tiempo de re-MFA. El `DisplayAlert` previene tap accidental.
- **Mensaje del perfil sobre identidad corporativa**: hace explícito al usuario que esta pantalla solo refleja la sesión, no edita permisos. Refuerza la postura de Microsoft Entra ID como fuente de verdad (Propuesta 1 §5.1).

## Cómo se integra

- `notifications` es tab de la TabBar principal.
- `profile` y `notifications/preferences` son rutas standalone registradas en `AppShell.xaml.cs`.
- `INotificationNavigationService` es Singleton y se inyecta en cualquier ViewModel que necesite navegar a partir de una notificación (Home y NotificationsViewModel).
- Las preferencias viven en `MockNotificationsRepository` como `Dictionary<NotificationKind, bool>`. Cuando se reemplace por API repository, ese state se persistirá server-side y sobrevivirá reinstalaciones.

## Pendientes

- Push real con Azure Notification Hubs + Logic Apps (Propuesta 1 §5.6).
- Persistir preferencias server-side via `Api*Repository`.
- Indicador de "todas leídas" en la TabBar de Notificaciones cuando el contador de no-leídas sea cero.
