# Decisión: Arquitectura del prototipo .NET MAUI

Fecha: 2026-06-01
Estado: aceptada
Alcance: frontend (`orquestador-frontend`), rama `feat/maui-prototype`

## Contexto

El proyecto migra de Flutter a .NET MAUI (decisión de cliente, ver §1 del CLAUDE.md raíz). Esta decisión define la arquitectura del prototipo MAUI siguiendo dos restricciones del proyecto:

- Las dos barandas del CLAUDE.md §7: separación estricta UI/datos vía repositorios, y una sola gestión de estado en todo el proyecto.
- Las decisiones de stack ya fijadas: .NET 10 + MAUI + C# + CommunityToolkit.Mvvm + Shell navigation + Material 3.

> **Nota de versionado.** El proyecto se creó originalmente apuntando a **.NET 8** (per CLAUDE.md §1 al momento del scaffold). Se migró a **.NET 10** el 2026-06-01 al instalar el entorno de desarrollo: el SDK local era .NET 10.0.108 (LTS vigente hasta noviembre 2028) y mantener .NET 8 implicaba instalar un SDK paralelo de soporte terminal (noviembre 2026). CLAUDE.md §1 fue actualizado en sincronía. Los `<PackageReference>` de `OrquestadorFrontend.csproj` quedaron pinneados en `Microsoft.Maui.Controls 10.0.108`, `Microsoft.Extensions.Logging.Debug 10.0.0`, `CommunityToolkit.Mvvm 8.4.0`.

El alcance es un prototipo visual sin backend. La conexión a Entra ID / MSAL.NET, backend, Power BI Embedded y Azure OpenAI llega en fases posteriores.

## Opciones evaluadas y decisiones

### 1. Estructura del proyecto

| Opción | Trade-off |
|--------|-----------|
| **Single project (elegida)** | Una sola csproj con folders por capa. Más simple, suficiente para el alcance de prototipo. La disciplina de capas se mantiene por convención de carpetas + namespaces. |
| Multi-project clean architecture (Domain/Application/Infrastructure/Presentation) | Frontera de tipos garantizada por el compilador, pero overengineering para un prototipo de UI sin lógica de negocio. |

**Resultado:** single project, namespace `OrquestadorFrontend.*`, capas separadas por carpetas (`Core`, `Data`, `Services`, `Features`, `Shared`).

### 2. Gestión de estado

| Opción | Trade-off |
|--------|-----------|
| **CommunityToolkit.Mvvm (elegida)** | Source generators producen `INotifyPropertyChanged` y commands con un mínimo de boilerplate. Mantenido por Microsoft. |
| MAUI Community Toolkit MVVM | Mismo paquete, distinto subset. CommunityToolkit.Mvvm es el core de MVVM y suficiente. |
| ReactiveUI | Reactive streams potente pero curva de aprendizaje alta y dependencia externa adicional. |

**Resultado:** CommunityToolkit.Mvvm en todo el proyecto. ViewModels heredan `ObservableObject`; estado vía `[ObservableProperty]` sobre campos privados; acciones vía `[RelayCommand]`. Code-behind sin lógica de datos.

### 3. Modelo de estado asíncrono

| Opción | Trade-off |
|--------|-----------|
| **`ResultState<T>` discriminated union (elegida)** | `abstract record` + variantes `Loading/Data/Empty/Failure`. Forzar pattern matching obliga a manejar los cuatro estados explícitamente. Pages ramifican via `IValueConverter` sobre el tipo concreto. |
| `Status` + `Value` + `Error` properties planas en el ViewModel | Más simple pero el caller olvida estados con facilidad y los converters tienen que verificar combinaciones. |
| `AsyncCommand<T>` con error events | Disponible en MAUI Community Toolkit pero acopla estado a comandos, complicando estados sin acción asociada. |

**Resultado:** `ResultState<T>` en `Core/ResultState.cs`. ViewModel expone `[ObservableProperty] ResultState<T> _state`. Pages combinan `IsLoadingConverter`, `IsDataConverter`, `IsEmptyConverter`, `IsFailureConverter` con `IsVisible` sobre los cuatro controles transversales de `Shared/Controls/`.

### 4. Navegación

| Opción | Trade-off |
|--------|-----------|
| **Shell navigation con guards (elegida)** | Nativa de MAUI, declarativa, soporta TabBar, query parameters, y un evento `Navigating` para guardas de ruta. |
| NavigationPage / push-pop manual | Más control pero mucho boilerplate y sin TabBar declarativo. |
| Custom router | Innecesario, Shell cumple. |

**Resultado:** `AppShell.xaml` con `TabBar` de 4 pestañas (Inicio, Reportes, Documentos, Notificaciones). Las rutas profundas (`board`, `document`, `whistleblower`, `whistleblower/case`, `notifications/preferences`, `profile`, `biometric-reauth`) se registran con `Routing.RegisterRoute(...)`. `LoginPage` es un `ShellContent` standalone con `Shell.NavBarIsVisible=False` y `Shell.TabBarIsVisible=False`. Después del sign-in la app navega a `//main/home`.

### 5. Inyección de dependencias

| Opción | Trade-off |
|--------|-----------|
| **`Microsoft.Extensions.DependencyInjection` (elegida)** | Estándar de .NET, integrado con MAUI a través de `builder.Services`. Shell y MAUI resuelven páginas vía DI automáticamente. |
| Sin DI (instanciación manual) | Inviable: las páginas dependen de ViewModels que dependen de repos que dependen de services. |
| Otro contenedor (Autofac, etc.) | Innecesario, MS DI cubre. |

**Resultado:** Todo se registra en `MauiProgram.cs`. Lifetimes:
- Repositorios mock y servicios (`SessionService`, `NotificationNavigationService`) → `Singleton` (mantienen estado en memoria que debe sobrevivir entre navegaciones).
- Pages y ViewModels → `Transient` (instancia fresca por navegación).
- `AppShell` → `Singleton` (existe una sola Shell por proceso).

### 6. Estilos y tokens

Tokens centralizados en `Resources/Styles/`:
- `Colors.xaml` — paleta Masterbrand + mapeo Material 3.
- `Typography.xaml` — escala Material 3 mapeada a ACHS Nueva Sans / Serif (ver `brand-identity-achs-maui.md`).
- `Styles.xaml` — botones, cards, layouts. `MinimumHeightRequest=44` en botones para accesibilidad táctil.

Ningún color o tamaño hardcodeado en las páginas. Cuando llegue la identidad visual definitiva, basta con tocar `Resources/Styles/`.

### 7. Guard del Canal de Denuncias

El módulo más sensible recibe doble protección:

1. **Visibilidad condicional en Inicio**: la entrada al módulo se renderiza solo si `_whistleblower.HasAccess(role)`. Para roles sin acceso, el módulo no existe en la interfaz (ausencia, no botón deshabilitado). Per Propuesta 1 §8, Propuesta 2 §5.6.
2. **Guard de ruta en `AppShell`**: el evento `Navigating` cancela cualquier intento de navegar a una ruta que contenga "whistleblower" si el rol activo no tiene permiso. Defensa en profundidad: el frontend nunca puede saltarse la verificación del backend, pero el frontend también verifica antes de cualquier rendering.
3. **Repository garantiza el contrato**: `MockWhistleblowerRepository.GetCasesAsync` y `GetCaseAsync` lanzan `WhistleblowerAccessDeniedException` ANTES de cualquier simulación de latencia o error si el rol no tiene acceso. Triple capa, alineado con el principio del CLAUDE.md §8 "cada capa re-valida".

## Estructura de carpetas implementada

```
OrquestadorFrontend.csproj
MauiProgram.cs                 # DI registration + font config
App.xaml / App.xaml.cs         # ResourceDictionary merged, MainPage = Shell
AppShell.xaml / AppShell.xaml.cs   # TabBar + route registration + guard

Resources/
  AppIcon/                     # SVG placeholders (icono oficial pendiente)
  Splash/                      # SVG placeholder
  Fonts/                       # ACHSNuevaSans-VF.ttf, ACHSNuevaSerif-VF.ttf
  Images/                      # achs_masterbrand_positiva_preferida/restringida
  Styles/                      # Colors.xaml, Typography.xaml, Styles.xaml

Platforms/
  Android/                     # MainActivity, MainApplication, AndroidManifest
  iOS/                         # AppDelegate, Program, Info.plist

Core/                          # UserRole, ResultState, converters
Data/
  Models/                      # 6 records inmutables
  Repositories/                # 6 interfaces + WhistleblowerAccessDeniedException
  Mock/                        # 6 implementaciones + MockLatency + MockData
Services/
  ISessionService / SessionService
  INotificationNavigationService / NotificationNavigationService
Features/
  Auth/      (Login, BiometricReauth + VMs)
  Home/      (HomePage + KpiCard + VM)
  Reports/   (ReportsList, BoardViewer + VMs)
  Documents/ (DocumentsList, DocumentViewer + AiSummaryBlock + ReportKindLabelConverter + VMs)
  Notifications/ (Notifications, NotificationPreferences + VMs)
  Whistleblower/ (WhistleblowerList, WhistleblowerCase + WhistleblowerStatusConverters + VMs)
  Profile/   (ProfilePage + VM)
Shared/
  Controls/                    # 6 ContentView reutilizables
```

## Lo que NO está en esta versión (y por qué)

- **Sin backend, sin autenticación real**: per spec, esta fase es prototipo visual. MSAL.NET, Entra ID, Power BI Embedded, Azure OpenAI llegan en fases posteriores.
- **Sin tests automatizados**: per CLAUDE.md §19.2, los tests se vuelven obligatorios cuando se cierra el prototipo visual y empieza la integración con backend. Antes de ese momento son aspiracionales.
- **Sin CI/CD pipeline**: per CLAUDE.md §19.3, el análisis de CVEs en CI se activa con el primer pipeline operativo. Pendiente para la fase de backend.
- **Sin localización (i18n)**: el prototipo es español neutro/chileno. Cuando se decida internacionalización, se mueve a recursos `.resx`.
- **Sin dark mode**: la estructura `AppThemeBinding` queda disponible cuando se decida soportarlo, sin requerir refactor.
- **Icono de launcher placeholder**: SVG verde con "A". Pendiente el icono oficial ACHS (Manual de Marca requiere variante con caja).

## Análisis de seguridad

- **Activo tocado**: ningún activo crítico todavía. Es prototipo sin backend, sin autenticación real, sin datos reales. UI con datos mock en memoria.
- **Vectores introducidos**:
  - Mock auth via role picker en LoginPage. Está claramente marcado `// PROTOTIPO:` y no involucra credenciales reales.
  - Datos mock con nombres realistas en español: ficticios, en `Data/Mock/MockData.cs`. Verificable por revisión de código.
  - Bundle de assets (~350 KB de fuentes + logos): aceptable para distribución por Intune.
- **Mitigaciones aplicadas**:
  - Guard de ruta del Canal de Denuncias desde el día uno, contra `ISessionService` (no contra propiedad de la página). Per CLAUDE.md §8 "cada capa re-valida".
  - Audit badge presente en el módulo desde el primer render — refleja la decisión de seguridad de la Propuesta 1 §8.
  - Resumen IA siempre junto al documento original, etiquetado como tal (`AiSummaryBlock`).
  - Ausencia total del módulo (no render condicional con `IsEnabled=False`) para roles sin permiso.
  - Sin colores, strings sensibles ni paths absolutos en código. Tokens centralizados, `.gitignore` excluye secretos.
  - `WhistleblowerAccessDeniedException` se chequea ANTES de simular latencia para que el gate no dependa de timing.
- **Estándares aplicables**:
  - OWASP MASVS — aplicable parcialmente: separación de responsabilidades, mínima retención local (todo en memoria), no se persiste nada sensible.
  - Microsoft Zero Trust — visible en la triple verificación del Canal de Denuncias: UI absence + Shell guard + Repository exception.

## Archivos tocados

Todo el contenido de la rama `feat/maui-prototype` desde el commit `Remove Flutter project scaffold` hasta este documento. Ver el log de commits para granularidad por archivo.

## Pendientes para iteraciones futuras

1. Implementar autenticación real con MSAL.NET (Entra ID).
2. Reemplazar `Mock*Repository` por `Api*Repository` que llamen al backend.
3. Implementar el visor de Power BI en `BoardViewerPage` con un `HybridWebView` que hospeda la librería JavaScript `powerbi-client` (no existe un control nativo de Power BI para MAUI); el backend entrega `embedUrl` y un embed token de vida corta.
4. Implementar visor PDF real en `DocumentViewerPage` y reproductor de video.
5. Integrar resumen IA generado por backend en `AiSummaryBlock`.
6. Implementar registro de auditoría — los accesos al Canal de Denuncias deben escribirse vía backend ANTES de entregar la respuesta.
7. Setup tests automatizados (gatillo §19.2).
8. Setup pipeline CI/CD con análisis de CVEs (gatillo §19.3).
9. Localización a otros idiomas si el alcance lo requiere.
10. Reemplazar icono launcher placeholder por el oficial ACHS.
