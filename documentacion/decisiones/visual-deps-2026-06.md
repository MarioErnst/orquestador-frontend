# Decisión: dependencias del sistema visual extendido

Fecha: 2026-06-03
Estado: aceptada
Alcance: frontend (`orquestador-frontend`), `OrquestadorFrontend.csproj`
Relaciona con: `visual-system-m3-dark-mode.md`, `maui-controls-pin-2026-06.md`

## Contexto

En la iteración del 2026-06-03 el cliente validó la app en el dispositivo
físico y reportó que se ve "horrible y básica". Investigación profunda
(documentada en el commit que introduce este archivo) reveló dos causas:

1. Las fonts de marca y el logo nunca llegaron al APK porque el csproj no
   declaraba `MauiFont` y `MauiImage` explícitamente. Esto se arregló en
   un commit aparte.
2. MAUI puro no llega al nivel "premium" que pide el cliente: faltan
   feedback táctil (TouchBehavior, haptics), micro-interacciones, hero
   visuals (gradients custom, skeleton loaders), y controles de
   formulario Material 3 reales (los nativos siguen siendo Material 2).

CLAUDE.md §1 requiere autorización del humano para cualquier librería
fuera del stack pinneado. El humano autorizó las tres siguientes en la
sesión del 2026-06-03.

## Dependencias autorizadas

### CommunityToolkit.Maui 14.1.1

- **Vendor:** Microsoft (`CommunityToolkit/Maui`, MIT).
- **Por qué:** `TouchBehavior` (feedback de press con scale/opacity),
  `AnimationBehavior` (micro-interacciones encadenables), `Popup`
  (modales custom alineados al patrón M3), `StatusBarBehavior` (color de
  status bar theme-aware), `HapticFeedback` integrado. Es el hermano
  visual de `CommunityToolkit.Mvvm` que ya usamos.
- **Por qué ahora y no antes:** la versión 14.x requiere
  `Microsoft.Maui.Controls >= 10.0.60`. El pin previo era 10.0.20 (ver
  `maui-controls-pin-2026-06.md`). Esta decisión mueve el pin a 10.0.70
  (última stable del wave 10.x al 2026-05-21) y deja el workload local
  en `maui-android 10.0.20/10.0.100` (no hay manifest más reciente
  publicado para .NET 10 GA; el package corre adelante del workload sin
  problema documentado).
- **Footprint:** ~600 KB de assemblies adicionales en el APK release.

### SkiaSharp.Views.Maui.Controls 3.119.4

- **Vendor:** Microsoft / mono / community (`mono/SkiaSharp`, MIT).
- **Por qué:** acceso a `SKCanvasView` y `SKImageInfo` para hero visuals
  (gradients sofisticados, skeleton loaders shimmer-animados, charts
  custom). Sin Skia, el techo visual de MAUI queda en `BoxView` +
  `LinearGradientBrush` simple — insuficiente para sentir "premium".
- **Footprint:** ~4 MB de libSkiaSharp.so por ABI (arm64-v8a, x86_64).
  En un APK que ya pesa ~84 MB, el impacto relativo es bajo.

### UraniumUI.Material 2.16.0

- **Vendor:** Enis Necipoglu (`enisn/UraniumUI`, Apache 2.0).
- **Por qué:** `TextField` con label flotante M3, `TimePickerField`,
  `DatePickerField`, `SkeletonView`, `AutoCompleteView`, `TabView`,
  `Chip`, `Avatar`, `BackdropView`. Reemplaza los `Entry`/`Picker`
  genéricos por componentes con polish Material 3 real, sin que
  tengamos que componerlos a mano. Permite cumplir con el principio
  "claridad sobre densidad" de la Propuesta 2 §1.2 con menos código.
- **Cuidado:** UraniumUI define su propio sistema de tokens. Mantenemos
  los nuestros (Colors.xaml, Shape.xaml, Spacing.xaml, Elevation.xaml,
  Typography.xaml) como fuente de verdad y solo usamos los control
  templates de UraniumUI, mapeando colores y forma vía propiedades
  explícitas o estilos derivados. Si en una iteración futura el mapeo se
  vuelve costoso, se evalúa adoptar parcialmente los tokens de
  UraniumUI.
- **Footprint:** ~1.5 MB de assemblies y resources.

## Cambios al csproj

- `Microsoft.Maui.Controls` 10.0.20 → 10.0.70 (matchea el requirement
  mínimo de CT.Maui 14.x).
- Tres nuevos `PackageReference` con versión exacta.

## Inicialización en `MauiProgram.cs`

```
builder
    .UseMauiApp<App>()
    .UseMauiCommunityToolkit()
    .UseSkiaSharp()
    .UseUraniumUI()
    .UseUraniumUIMaterial()
    .ConfigureFonts(...)
```

El orden importa: `UseMauiApp<T>()` siempre primero (analyzer MCT001 lo
exige). El resto puede ir en cualquier orden, pero se respeta el orden
de Community Toolkit (primero su core), Skia, y UraniumUI al final
porque registra sus propios handlers.

## Análisis de seguridad (§2 CLAUDE.md)

| Dep | Activo | Vectores | Mitigación |
|-----|--------|----------|-----------|
| CommunityToolkit.Maui 14.1.1 | UI behaviors, popups, haptic | Ninguno conocido. Solo APIs locales. No red, no IO de datos. | Versión exacta pinneada. CVE check pendiente §19.3. Microsoft-backed reduce riesgo de supply chain. |
| SkiaSharp.Views.Maui.Controls 3.119.4 | Render Skia | Bibliotecas nativas embebidas (libSkiaSharp.so). Históricamente Skia tuvo CVEs por crash en parsing de imágenes maliciosas. En esta app no procesamos imágenes desde fuentes no confiables (solo recursos propios). | Versión exacta pinneada. No procesar imágenes de origen no confiable sin sanitizar. Versión 3.119.4 es reciente, sin CVE pública activa. |
| UraniumUI.Material 2.16.0 | UI controls | OSS comunitario, single-maintainer. Riesgo de bus-factor. Sin red, sin IO de datos sensibles. | Versión exacta pinneada. Evaluar bi-anualmente continuidad del proyecto. Si se discontinúa, los control templates son reemplazables con MAUI puro (más curro). |

Ninguna toca autenticación, autorización, manejo de datos sensibles, ni
red. Todas pinneadas a versión exacta, sin rangos.

## Acciones de seguimiento

1. Cuando se levante el CI/CD (§19.3), agregar `dotnet list package
   --vulnerable --include-transitive` y registrar el resultado.
2. Revisar bi-anualmente continuidad del proyecto UraniumUI; si se
   discontinúa, evaluar reemplazo.
3. Cuando el cliente confirme que ya no se requieren más iteraciones de
   pulido y la app pasa a fase de integración con el backend real,
   revisar si todas las APIs de las tres libs siguen siendo necesarias.
4. Cuando el workload `maui-android` publique un manifest más reciente,
   evaluar alinear el pin del csproj a esa versión y bajar al package
   matching para volver a la simetría workload/package.

## Archivos tocados

- `OrquestadorFrontend.csproj` (pin de Maui.Controls + 3 nuevas refs).
- `MauiProgram.cs` (initializers + using).
- `documentacion/decisiones/visual-deps-2026-06.md` (este documento).
