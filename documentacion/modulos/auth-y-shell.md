# Módulo: Auth y Shell

## Qué hace

Implementa el flujo de acceso del usuario (Login + reacceso biométrico) y la navegación principal de la app (Shell con TabBar y rutas registradas). En esta fase de prototipo, el "login" es un selector ficticio de rol que se intercambia por el flujo real de Entra ID / MSAL.NET en la fase posterior.

## Archivos clave

### Auth

- `Features/Auth/LoginPage.xaml` / `.cs` — Pantalla sobria con logo Masterbrand, nombre de la app y botón único "Iniciar sesión con cuenta Microsoft". El botón abre un `DisplayActionSheet` con Director / Comité / Alta Gerencia. La elección se delega al `LoginViewModel.SignInCommand`.
- `Features/Auth/LoginViewModel.cs` — `SignInCommand(UserRole)` que invoca `ISessionService.SignIn(role)` y navega a `//main/home`.
- `Features/Auth/BiometricReauthPage.xaml` / `.cs` — Pantalla de confirmación rápida con botón "Usar biometría" y alternativa "Usar otra cuenta".
- `Features/Auth/BiometricReauthViewModel.cs` — `ConfirmCommand` navega a `//main/home`. `UseOtherAccountCommand` cierra sesión y vuelve a `//login`.

### Shell + navegación

- `AppShell.xaml` — TabBar con 4 pestañas (Inicio, Reportes, Documentos, Notificaciones). LoginPage como ShellContent standalone fuera del TabBar (sin NavBar ni TabBar visibles).
- `AppShell.xaml.cs` — Registra rutas profundas con `Routing.RegisterRoute(...)`:
  - `biometric-reauth` → BiometricReauthPage
  - `board` → BoardViewerPage
  - `document` → DocumentViewerPage
  - `notifications/preferences` → NotificationPreferencesPage
  - `profile` → ProfilePage
  - `whistleblower` → WhistleblowerListPage
  - `whistleblower/case` → WhistleblowerCasePage
- Suscribe `Navigating` para el guard del Canal de Denuncias (ver `seguridad/whistleblower-channel.md`).

### Servicios

- `Services/ISessionService.cs` — Contrato: `CurrentRole`, `SignIn(role)`, `SignOut()`. Implementa `INotifyPropertyChanged` para que el guard del Shell reaccione a cambios.
- `Services/SessionService.cs` — Implementación con `[ObservableProperty]` de CommunityToolkit.Mvvm. Singleton en DI.

## Decisiones

- **Login como ShellContent standalone**: vive dentro del Shell pero con NavBar y TabBar ocultos. Permite que después del sign-in el `GoToAsync("//main/home")` deje al usuario en la TabBar principal sin reconstruir la Shell.
- **Role picker como `DisplayActionSheet`** en code-behind: es UI affordance, no lógica de datos, por lo que no rompe la regla de "code-behind sin lógica de datos" del §7 CLAUDE.md. Se delega al ViewModel en cuanto se obtiene la elección.
- **`SessionService` Singleton**: el rol activo debe ser el mismo en cualquier punto de la app — Shell guard, ViewModels, etc. Singleton garantiza una sola fuente de verdad.
- **`INotifyPropertyChanged` en `ISessionService`**: permite que en el futuro las pantallas reaccionen a cambios de sesión (ej. forzar refresh de Home si el rol cambia).
- **PROTOTIPO**: ambos ViewModels tienen comentarios `// PROTOTIPO:` que marcan dónde entra MSAL.NET en producción.

## Cómo se integra

- `MauiProgram.cs` registra todas las páginas y ViewModels como Transient, y `SessionService` como Singleton.
- `App.xaml.cs` recibe `AppShell` por DI; `AppShell.xaml.cs` recibe `ISessionService` e `IWhistleblowerRepository` por DI.
- La Shell expone `Shell.Current` globalmente — los ViewModels llaman `Shell.Current.GoToAsync(...)` para navegar.

## Pendientes

- Reemplazar selector de rol por flujo real de Entra ID via MSAL.NET con `IPublicClientApplication`.
- Implementar biometría real con `Microsoft.Maui.Authentication` (BiometricAuth API) o platform-specific.
- Agregar política de timeout de sesión (≤30 min sin uso per CLAUDE.md §8) — usa `Application.Current.Dispatcher` para detectar inactividad.
