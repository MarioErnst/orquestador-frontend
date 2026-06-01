# Módulo: Home (Dashboard por rol)

## Qué hace

Pantalla principal post-login. Responde a la pregunta "¿cómo están mis indicadores?" con tres bloques:

1. **Indicadores clave** — grilla de KPIs filtrados por rol (de `IDashboardRepository`).
2. **Accesos rápidos** — último informe, tablero habitual, y entrada al Canal de Denuncias (solo si el rol tiene acceso).
3. **Alertas recientes** — últimas tres notificaciones (de `INotificationsRepository`).

## Archivos clave

- `Features/Home/HomePage.xaml` / `.cs` — Layout en tres `VerticalStackLayout` con `ScrollView` global. Refresca en `OnAppearing` para reflejar cambios (ej. notificaciones recién marcadas como leídas).
- `Features/Home/HomeViewModel.cs` — Maneja múltiples `ResultState<T>` en paralelo (KPIs y alertas), además de propiedades simples (`LatestReport`, `FeaturedBoard`, `HasWhistleblowerAccess`, `Greeting`). El `LoadCommand` dispara todas las cargas y maneja errores por sección.
- `Features/Home/Controls/KpiCard.xaml` / `.cs` — Tarjeta KPI con label, valor, unidad opcional, etiqueta de estado coloreada (`StatusOk`/`StatusWarn`/`StatusAlert`), helper opcional. Recibe un `DashboardKpi` completo y deriva los visuales internamente.

## Decisiones

- **Múltiples ResultStates, no uno único**: la página tiene secciones con estados independientes. Si los KPIs fallan, las alertas siguen mostrándose y viceversa. Cada `[ObservableProperty]` ResultState representa una sección.
- **Whistleblower entry con `IsVisible` enlazado a `HasWhistleblowerAccess`**: para roles sin acceso, el Border no existe en el visual tree. Per Propuesta 2 §5.6, ausencia total, no botón deshabilitado.
- **`OnAppearing` recarga**: el dashboard se considera "dato fresco" cada vez que aparece. Si esto se volviera costoso con backend real, agregar TTL.
- **`KpiCard` resuelve colores en runtime** vía `Application.Current.Resources["StatusOk"]` porque el `DashboardKpi.Status` es un enum: el converter en XAML directo sería más limpio pero requeriría exponer el enum a XAML; el código-behind del control es trivial y aislado.

## Cómo se integra

- `HomePage` se monta como `ShellContent` del tab `home` de la TabBar.
- `HomeViewModel` recibe seis dependencias: `ISessionService`, `IDashboardRepository`, `IReportsRepository`, `IBoardsRepository`, `INotificationsRepository`, `IWhistleblowerRepository`, `INotificationNavigationService`. Es el ViewModel con más dependencias del proyecto — refleja que Home agrega información de muchas fuentes.
- Los comandos de navegación (`OpenLatestReport`, `OpenFeaturedBoard`, `OpenWhistleblower`, `OpenNotification`, `OpenAllNotifications`) usan rutas relativas o absolutas registradas en `AppShell`.

## Pendientes

- Cuando el `IDashboardRepository` provea datos del backend real, considerar polling/refresh manual.
- Agregar pull-to-refresh en la `ScrollView`.
