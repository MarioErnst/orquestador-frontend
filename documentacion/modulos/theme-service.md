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
tiempo real. Las propiedades de color en los estilos están bindadas via
`AppThemeBinding`, así que el cambio se propaga automáticamente al
siguiente frame sin necesidad de re-crear las vistas.

La aplicación ocurre en el hilo principal usando
`MainThread.BeginInvokeOnMainThread`, porque `UserAppTheme` no es
thread-safe.

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
