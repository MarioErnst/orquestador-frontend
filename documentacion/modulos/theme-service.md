# Módulo: ThemeService

Owner: `Services/IThemeService.cs` + `Services/ThemeService.cs`

## Qué hace

Gestiona la preferencia de apariencia (Sistema / Claro / Oscuro) de la
aplicación en runtime. Lee la preferencia persistida al arranque y
permite cambiarla desde la UI (pantalla de Perfil → "Apariencia").

## Cómo se usa desde la UI

El servicio expone:

- `CurrentMode` — modo activo (`AppearanceMode.System`, `Light` o `Dark`).
- `ModeChanged` — evento que se dispara en el hilo de UI cuando cambia el
  modo.
- `InitializeAsync()` — lee la preferencia persistida y aplica el tema.
  Lo invoca el composition root (`App.xaml.cs`) una vez al arranque.
- `SetModeAsync(mode)` — persiste y aplica un nuevo modo. Idempotente.

El consumidor típico es un ViewModel que se suscribe a `ModeChanged` y
expone propiedades para data binding (ej. `ProfileViewModel` con tres
booleanos `IsAppearanceSystem`, `IsAppearanceLight`, `IsAppearanceDark`
para un grupo de `RadioButton`).

## Cómo se integra

El servicio se registra como singleton en `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IThemeService, ThemeService>();
```

`App.xaml.cs` resuelve el servicio del mismo `IServiceProvider` que usa
para `AppShell` y dispara `InitializeAsync()` justo después de
`InitializeComponent()`, en modo fire-and-forget (no se awaitea para no
bloquear el launch path).

## Persistencia

La preferencia vive en `SecureStorage` (Keychain en iOS, Android Keystore
en Android), bajo la clave `appearance-mode`, con el nombre del enum
serializado como string (`System`, `Light`, `Dark`).

La elección de SecureStorage (en lugar de `Preferences`) es deliberada:
el resto del estado per-usuario del prototipo viene de SecureStorage, y
mantener una única superficie de almacenamiento simplifica la auditoría
y el ciclo de vida (limpieza al desinstalar la app, etc.). El dato en sí
es cosmético y no es PII; ver decisión completa en
`documentacion/decisiones/visual-system-m3-dark-mode.md`.

## Manejo de errores

`SecureStorage.GetAsync` y `SetAsync` pueden lanzar excepciones en
dispositivos Android con políticas corporativas restrictivas o keystore
en estado inconsistente tras un factory reset. El servicio captura esas
excepciones y degrada al default `AppearanceMode.System` para lectura y
mantiene el modo en memoria sólo durante la sesión para escritura. No
loguea el detalle (CLAUDE.md §6: sin secretos ni datos sensibles en
logs; aunque acá no haya secretos, mantener la norma uniforme).

## Aplicación del tema

Cuando se aplica un modo, el servicio hace:

```csharp
Application.Current.UserAppTheme = mode switch
{
    AppearanceMode.Light => AppTheme.Light,
    AppearanceMode.Dark => AppTheme.Dark,
    _ => AppTheme.Unspecified,
};
```

`AppTheme.Unspecified` deja que MAUI siga la preferencia del SO en
tiempo real. La aplicación ocurre en el hilo principal usando
`MainThread.BeginInvokeOnMainThread`, porque `UserAppTheme` no es
thread-safe.

## Workarounds para bugs de `AppThemeBinding` en MAUI 10

`AppThemeBinding` *debería* propagarse automáticamente cuando cambia
`UserAppTheme`, pero hay una familia de bugs abiertos en `dotnet/maui`
(#6596, #20243 y relacionados) por los cuales ciertas propiedades
quedan congeladas en el tema anterior hasta que se reinicia la app o se
re-crea la vista. El servicio compensa esto con cuatro pasadas
adicionales después de setear `UserAppTheme`, todas dentro de
`ForceShellChromeRefresh`:

1. **Chrome del Shell.** Se reasignan imperativamente
   `Shell.BackgroundColor`, `ForegroundColor`, `TitleColor` y los
   colores del `TabBar` leyendo los `Color` resueltos desde
   `Application.Resources`.
2. **Background de `ContentPage`.** Para cada `ContentPage`
   materializada (visible, en navigation stack, en modal stack o en una
   tab no activa), se reasigna `BackgroundColor` con el `Color` del
   tema actual. Las páginas del Canal de Denuncias usan
   `WhistleblowerSurface` para conservar su superficie cálida.
3. **Items de `CollectionView`.** RecyclerView no propaga el evento
   `RequestedThemeChanged` a `ViewHolder`s desacoplados. Se setea
   `ItemsSource = null; ItemsSource = src;` para forzar recreación de
   los items desde el `DataTemplate`, evaluando el `AppThemeBinding`
   nuevo.
4. **Setters de `Style` con `AppThemeBinding`.** Cuando un Setter
   define un color con `AppThemeBinding`, el valor se cachea en la
   primera aplicación del Style y no se refresca al cambiar tema —
   éste era el síntoma reportado en las tarjetas de Accesos rápidos
   ("Último informe publicado" y "Tablero habitual" se quedaban con el
   color del tema anterior). El helper `StyleAppThemeRefresher` recorre
   el árbol visual de cada página, lee `Style.Setters` (incluyendo
   `BasedOn`), y para cada Setter cuyo `Value` es un `AppThemeBinding`
   primero invoca `RemoveBinding(property)` para matar el binding
   roto y después `SetValue(property, color)` con el `Color` resuelto
   del slot `Light` / `Dark`. Sin el `RemoveBinding` el binding roto
   re-dispara en el mismo dispatch cycle y pisa el valor local con
   el color del tema anterior. Es genérico: cualquier `Style` nuevo
   del proyecto queda cubierto sin tocar este servicio.

El servicio dispara las cuatro pasadas dos veces: una síncronamente al
cambio de modo y otra deferida con `MainThread.BeginInvokeOnMainThread`
para ganar a cualquier propagación tardía de `AppThemeBinding` que
podría intentar pisar los valores imperativos.

## Refresh de tabs lazy en `Shell.Navigated`

Las tabs de `AppShell.xaml` están definidas con
`ContentTemplate="{DataTemplate ...}"`, así que sus páginas son lazy:
sólo se materializan la primera vez que se entra a la tab y a partir
de ahí viven en un cache interno de `ShellContent` que **no** está
expuesto vía `ShellContent.Content` ni vía el árbol visual del Shell
mientras la tab no es la actual. Esto significa que cuando el usuario
toggle Apariencia desde la pantalla de Perfil (que está en el stack
modal sobre la tab activa) ni desde otra tab, las cuatro pasadas de
arriba sólo alcanzan a la página visible — el resto de las tabs
materializadas conservan los `AppThemeBinding` cacheados del tema
anterior y se ven con el color viejo apenas el usuario navega de
vuelta.

Para cerrar ese gap, `ThemeService` se suscribe una vez a
`Shell.Navigated` (con `EnsureShellNavigationHooked`, idempotente).
Cada vez que una página se vuelve visible, dispara
`StyleAppThemeRefresher.Refresh` sobre ella en el siguiente cycle del
dispatcher. La navegación ya garantiza que la página está
materializada y en el árbol visual, así que el walker la encuentra y
reaplica los colores del tema actual.

## Análisis de seguridad

- **Activo:** preferencia visual del usuario (no PII, no secreto).
- **Vectores:** lectura/escritura local en SecureStorage. Sin red.
- **Mitigaciones:** captura de excepciones de SecureStorage; sin logs
  sensibles; sin exposición fuera del dispositivo.
- **Estándar aplicable:** OWASP MASVS (V1 / V2 storage). El módulo no
  almacena datos sensibles, pero usa la superficie correcta.

## Pendientes

- Suscribirse a `Application.RequestedThemeChanged` para registrar en
  audit log si en algún momento se decide auditar cambios de UX
  (probablemente no aplica — preferencia cosmética).
- Tests unitarios cuando el disparador §19.2 de CLAUDE.md (primera ruta
  autenticada real) se cumpla y se monte el proyecto de tests.
