# Seguridad: Canal de Denuncias (prototipo MAUI)

Fecha: 2026-06-01
Alcance: frontend, módulo Canal de Denuncias en `feat/maui-prototype`
Documento normativo: Propuesta 1 §8 (Tratamiento reforzado del Canal de Denuncias), Propuesta 2 §5.6 y §7.2.

## Activo a proteger

La existencia misma del módulo, su contenido (casos), la identidad de quienes denunciaron, y la trazabilidad de quién accedió. La confidencialidad es crítica — una filtración compromete a denunciantes y a la organización en planos legal, reputacional y humano.

## Modelo de amenazas (STRIDE, frontend solamente)

| Amenaza | Vector | Mitigación en este frontend |
|---------|--------|-----------------------------|
| **Information disclosure (existencia)** | Un rol no autorizado infiere que el módulo existe por ver un botón deshabilitado, un mensaje "Sin permiso", o un link en el header. | **Ausencia total**: el entry point en Home no se renderiza para roles sin acceso. No es `IsEnabled=False`, no es texto en gris. El visual tree no contiene el Border. Aplica también a notificaciones: el tipo `WhistleblowerAlert` solo se inserta en `MockData.NotificationsForRole` para roles autorizados. |
| **Information disclosure (contenido)** | Un rol no autorizado llega a la pantalla via deep link, notificación stale o navegación manual. | **Triple gate**: (1) UI no renderiza la entrada, (2) `AppShell.OnNavigating` cancela cualquier ruta que contenga "whistleblower" si el rol no autoriza, (3) `MockWhistleblowerRepository` valida `HasAccess` ANTES de cualquier `await` y lanza `WhistleblowerAccessDeniedException`. El ViewModel traduce a mensaje genérico ("No tenés acceso a este módulo"), no revela contenido. |
| **Elevation of privilege** | Un usuario manipula el `ISessionService.CurrentRole` desde código (debugger). | El prototipo no tiene tamper detection del proceso. **En producción** Microsoft Intune + MASVS detectan dispositivo comprometido (root/jailbreak) y bloquean la app antes de exponer datos. El frontend no es la última línea de defensa: el backend re-valida el rol y registra auditoría per Propuesta 1 §4.2 y §8.3. |
| **Repudiation** | Un acceso no queda registrado y el usuario niega haber visto un caso. | El prototipo **no escribe auditoría** (no hay backend). **En producción** el backend escribe el registro de auditoría ANTES de entregar el contenido (Propuesta 1 §8.3, §10.1). Si el escribir falla, el acceso se rechaza. |
| **Tampering** | Un atacante modifica el binario para saltarse las verificaciones. | **En producción** MASVS provee resistencia a manipulación (binary protection) + Intune detección de jailbreak. El prototipo no implementa estas defensas porque no maneja datos reales. |
| **Spoofing** | Un atacante se hace pasar por usuario autorizado. | Fuera del alcance del frontend. Microsoft Entra ID + MFA + Conditional Access cubren esto (Propuesta 1 §7.1). El prototipo no autentica realmente. |
| **Denial of service** | Un usuario bombardea la pantalla con cargas. | Mitigado por `MockLatency` (deja respirar al sistema) y por el patrón Singleton del repo (estado en memoria, sin contención de DB). En producción los rate limits del API Management cubren el endpoint. |

## Mitigaciones implementadas

### 1. Ausencia total para roles sin permiso

**Dónde**: `Features/Home/HomePage.xaml`, sección "Accesos rápidos".

```xml
<Border Style="{StaticResource WhistleblowerCardBorder}"
        IsVisible="{Binding HasWhistleblowerAccess}">
    ...
</Border>
```

`HasWhistleblowerAccess` lo asigna `HomeViewModel.LoadAsync` después de consultar `_whistleblower.HasAccess(role)`. Para Director, el bool es `false`, el Border tiene `IsVisible=False` y no consume layout space ni queda en el visual tree para inspección.

**Estándar aplicable**: OWASP MASVS — minimum information disclosure.

### 2. Guard de ruta en Shell

**Dónde**: `AppShell.xaml.cs`, handler `OnNavigating`.

```csharp
private void OnNavigating(object? sender, ShellNavigatingEventArgs e)
{
    var target = e.Target?.Location?.OriginalString ?? string.Empty;
    if (!target.Contains("whistleblower", StringComparison.OrdinalIgnoreCase))
        return;
    var role = _session.CurrentRole ?? UserRole.Director;
    if (!_whistleblower.HasAccess(role))
        e.Cancel();
}
```

Cubre:
- Deep links manuales (`Shell.Current.GoToAsync("whistleblower")`).
- Notificaciones stale después de cambio de rol.
- Intentos de navegación lateral desde otros pages.

La cancelación es silenciosa — no hay snackbar ni error ni navegación de error. El usuario simplemente no llega. Esto evita confirmar la existencia del módulo a un rol no autorizado.

**Estándar aplicable**: Microsoft Zero Trust — verificar siempre, asumir compromiso.

### 3. Gate en el repositorio (data layer)

**Dónde**: `Data/Mock/MockWhistleblowerRepository.cs`.

```csharp
public async Task<IReadOnlyList<WhistleblowerCase>> GetCasesAsync(UserRole role)
{
    if (!HasAccess(role)) throw new WhistleblowerAccessDeniedException(role);
    await MockLatency.SimulateAsync();
    if (SimulateError) throw new InvalidOperationException(...);
    return MockData.WhistleblowerCases();
}
```

La verificación ocurre ANTES de cualquier `await` o simulación de error. Razón: el gate no debe depender de timing ni interactuar con la bandera de simulación. Aunque el SimulateError esté en true, si el rol no tiene acceso, la excepción del gate llega primero.

**Estándar aplicable**: CLAUDE.md §8 — "cada capa re-valida". UI valida, Shell guard valida, repository valida.

### 4. Audit badge visible (transparencia con el usuario)

**Dónde**: `Shared/Controls/AuditBadge.xaml`, instanciado en `WhistleblowerListPage` y `WhistleblowerCasePage`.

Es una píldora con texto "Acceso registrado". Comunica honestamente al usuario que el acceso queda en el log. No es una advertencia: es una nota institucional sobria.

**Estándar aplicable**: Propuesta 2 §7.2, §8.1. Decisión de diseño con intención de seguridad.

### 5. Mensajes genéricos en errores

**Dónde**: `WhistleblowerListViewModel.LoadAsync` catch blocks.

Si se captura `WhistleblowerAccessDeniedException`, el mensaje al usuario es: "No tenés acceso a este módulo." — genérico. Si es otra excepción, "No pudimos cargar los casos. Intentá de nuevo." — también genérico.

Nunca se muestra stack trace, nombre de tabla, ni detalle interno. CLAUDE.md §9.

### 6. Tratamiento visual diferenciado

**Dónde**: `Resources/Styles/Colors.xaml` define `WhistleblowerSurface`, `WhistleblowerAccent`. `Styles.xaml` define `WhistleblowerCardBorder`.

Las pantallas usan `BackgroundColor="{StaticResource WhistleblowerSurface}"` y cards en `WhistleblowerCardBorder`. El cambio de superficie comunica "espacio aparte" sin necesidad de explicarlo. Propuesta 2 §7.2.

## Supuestos

- El prototipo NO conecta a backend. Toda la lógica de auditoría real (escribir antes de entregar, cifrado a nivel de columna, mensaje firmado) llega en la fase posterior.
- El backend es la última línea de defensa: el frontend confía en el rol que recibe (mock) pero en producción el backend re-valida contra el token Entra ID per Propuesta 1 §4.2.
- Los datos mock (`MockData.WhistleblowerCases()`) son ficticios. Ningún caso, código de referencia, fecha o categoría corresponde a información real.
- La distribución de la app es privada vía Microsoft Intune. No se publica a tiendas. Per CLAUDE.md §1.

## Vías de ataque revisadas (per CLAUDE.md §2)

- ✅ Inyección — no aplicable, no hay queries ni renderizado de strings de origen no confiable. Datos mock estáticos.
- ✅ Autenticación rota — prototipo no autentica realmente; gate de rol vía mock service. Producción usa Entra ID.
- ✅ **Autorización rota o saltable** — triple verificación documentada arriba. Esta es la principal mitigación del módulo.
- ✅ Exposición de datos sensibles en logs — `Microsoft.Extensions.Logging.Debug` solo activo en `#if DEBUG`. No se loguea PII ni contenido de casos. Producción usa Application Insights con filtro server-side (Propuesta 1 §10.2).
- ✅ Tokens — no aplicable al prototipo (no hay tokens). En producción tokens de vida corta y evaluación continua de acceso per CLAUDE.md §8.
- ✅ Manejo de archivos — no aplicable, no se sube ni descarga nada del Canal en el prototipo.
- ✅ Configuración — sin secretos, sin connection strings. Tokens de color y estilos son configuración pública. `.gitignore` excluye `.env`, `secrets.json`, etc.
- ✅ Dependencias — `Microsoft.Maui.Controls 8.0.100`, `CommunityToolkit.Mvvm 8.4.0`, `Microsoft.Extensions.Logging.Debug 8.0.1`. Versiones pinneadas (CLAUDE.md §8). Análisis automático de CVEs se activa con primer CI (gatillo §19.3).

## Pendientes / acciones de seguimiento

- Cuando se integre backend (gatillo §19.4 commits coordinados): escritura de auditoría ANTES de entregar contenido, cifrado a nivel de columna, retención documentada per CLAUDE.md §6.
- Cuando se cierre el prototipo visual (gatillo §19.2): tests automatizados obligatorios para todos los caminos de acceso del Canal: autorizado → 200, no autorizado → 403/exception, sin sesión → redirect a login, dispositivo comprometido → bloqueo Intune.
- Confirmar con área de cumplimiento ACHS el contenido visible del detalle de caso (decisión pendiente Propuesta 1 §13, §9.4).
- Decisión explícita ACHS sobre si el contenido del Canal se procesa con IA (Propuesta 1 §9.4) — por defecto NO en este prototipo y NO en la primera versión productiva.
