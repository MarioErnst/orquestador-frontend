# Seguridad: Auth y Shell (prototipo MAUI)

Fecha: 2026-06-01
Alcance: frontend, módulos Auth + Shell en `feat/maui-prototype`
Documento normativo: Propuesta 1 §6 (Seguridad en la capa móvil) y §7 (Gestión de identidad, acceso y sesión).

## Activos tocados

- Identidad del usuario (en producción: token de Entra ID; en prototipo: rol mock).
- Punto de entrada a la app — toda la superficie post-login es accesible desde aquí.
- Rol activo: condiciona qué información ve y a qué módulos accede el usuario.

## Modelo de amenazas (STRIDE)

| Amenaza | Vector | Mitigación en este frontend |
|---------|--------|-----------------------------|
| **Spoofing** | Suplantación de identidad. | Prototipo: no aplica (login mock). Producción: MSAL.NET + Entra ID + MFA + acceso condicional (Propuesta 1 §6.3, §7.1). |
| **Tampering** | Modificación del binario para alterar el flujo de login. | Producción: Intune detecta jailbreak/root y bloquea ejecución (Propuesta 1 §6.2). Prototipo no detecta. |
| **Repudiation** | Usuario niega haber accedido. | Producción: cada login se registra en Entra ID logs + auditoría en Application Insights (Propuesta 1 §10). Prototipo: no aplica. |
| **Information disclosure** | Credenciales o tokens expuestos. | Producción: tokens en proceso protegido del SO (Microsoft Authenticator) + Keychain/Keystore para refresh tokens (Propuesta 1 §6.3). Prototipo: no maneja credenciales. |
| **Denial of service** | Bombardeo del endpoint de login. | Producción: rate limit de Entra ID + WAF (Propuesta 1 §5.2). Prototipo: irrelevante. |
| **Elevation of privilege** | Rol manipulado para acceder a recursos no autorizados. | **Doble validación**: el frontend valida el rol del Shell guard (cancela rutas del Canal de Denuncias). El backend re-valida en cada endpoint (CLAUDE.md §8 "cada capa re-valida"). |

## Mitigaciones implementadas

### 1. Mock auth claramente marcado

**Dónde**: `LoginPage.xaml.cs` (DisplayActionSheet con selector de rol), `LoginViewModel.cs` (SignIn).

Cada lugar donde el flujo de login es ficticio tiene un comentario `// PROTOTYPE:` que indica explícitamente que en producción se reemplaza por MSAL.NET. Esto previene que el mock acumule lógica que después haya que desentrelazar.

### 2. Shell guard centralizado

**Dónde**: `AppShell.xaml.cs`, evento `Navigating`.

Toda navegación pasa por este handler antes de resolverse. Hoy solo verifica acceso al Canal de Denuncias, pero es el punto natural para agregar verificaciones futuras (sesión expirada → redirect a login, dispositivo comprometido → bloqueo, etc.).

La cancelación es silenciosa (`e.Cancel()`), nunca lanza al usuario una pantalla de error que revele estado interno.

### 3. Sesión Singleton, no estática

**Dónde**: `Services/SessionService.cs` registrado como Singleton en `MauiProgram.cs`.

El rol activo vive en `ISessionService.CurrentRole`. No hay variables estáticas globales (`UserRole.Active`) que serían más difíciles de testear y de reemplazar. Cualquier componente que necesita el rol lo recibe por DI.

`SessionService` implementa `INotifyPropertyChanged`, así que en el futuro las pantallas pueden reaccionar a cambios de rol (ej. forzar re-fetch de Home si el rol cambia).

### 4. Sign-out con confirmación

**Dónde**: `ProfileViewModel.SignOutAsync`.

Previene cierre accidental que costaría re-MFA al usuario. Después de confirmar, `_session.SignOut()` limpia el estado y navega a `//login` con la TabBar oculta.

### 5. Login fuera del TabBar

**Dónde**: `AppShell.xaml`.

La `ShellContent` de login es standalone con `Shell.TabBarIsVisible=False` y `Shell.NavBarIsVisible=False`. No se puede llegar a la pantalla de login desde la TabBar ni viceversa salvo via `GoToAsync("//login")` explícito. Esto evita que un usuario sin sesión llegue por accidente a una pantalla autenticada.

## Supuestos

- El prototipo NO autentica realmente. Toda decisión de seguridad de identidad ocurre en Entra ID en producción.
- El backend re-valida el token recibido del cliente per Propuesta 1 §4.2 — el frontend no es la última línea de defensa.
- Microsoft Intune está activo en producción y aplica las políticas de cumplimiento (cifrado del store local, no copy/paste fuera de la app, bloqueo en dispositivos comprometidos) per Propuesta 1 §6.2.

## Vías de ataque revisadas (per CLAUDE.md §2)

- ✅ Inyección — no aplicable, no hay strings de origen no confiable que se rendericen.
- ✅ Autenticación rota — prototipo: no aplica. Producción: MSAL.NET con sus garantías.
- ✅ Autorización rota o saltable — Shell guard del Canal de Denuncias documentado en `seguridad/whistleblower-channel.md`.
- ✅ Exposición de datos sensibles en logs — el `MockData.ProfileForRole` retorna correos ficticios pero realistas. Si alguno coincidiera con uno real por casualidad, sería casualidad sin impacto en el prototipo. En producción los correos vienen de Entra ID y no se loguean (CLAUDE.md §6).
- ✅ Tokens — no aplicable al prototipo. En producción tokens de vida corta (≤1 hora), evaluación continua de acceso, renovación rotada (Propuesta 1 §7.3).
- ✅ Configuración — la sesión vive en memoria del proceso. No se persiste a disco. Para el prototipo es seguro; en producción la persistencia será la del MSAL token cache, en Keychain/Keystore.
- ✅ CORS / headers — no aplicable al frontend MAUI nativo (no hay browser, no hay CORS). Headers HTTPS van a aplicar al backend (CLAUDE.md §8).

## Pendientes / acciones de seguimiento

- Integrar MSAL.NET con configuración Entra ID corporativa (tenant ACHS, client id, redirect URI).
- Implementar biometric reauth real con `Microsoft.Maui.Authentication` o platform native.
- Aplicar políticas de Intune App SDK: cifrado de datos locales, bloqueo de copy/paste fuera de la app, borrado remoto selectivo.
- Implementar timeout de sesión por inactividad (≤30 min per CLAUDE.md §8).
- Implementar evaluación continua de acceso: si Entra ID revoca el token, la app debe cerrar sesión casi en tiempo real (Propuesta 1 §7.3).
- Certificate pinning hacia el gateway corporativo cuando se introduzcan llamadas HTTP.
