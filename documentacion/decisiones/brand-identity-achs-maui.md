# Decisión: Adopción de la identidad visual ACHS Masterbrand (.NET MAUI)

Fecha: 2026-06-01
Estado: aceptada
Alcance: frontend (`orquestador-frontend`), versión .NET MAUI
Reemplaza: `brand-identity-achs.md` (versión Flutter de develop-flutter, 2026-05-22)

## Contexto

Este documento porta la decisión de identidad visual originalmente tomada en la rama `develop-flutter` (commit `225ffd8` y vecinos) al nuevo stack .NET MAUI introducido en `feat/maui-prototype`. Los activos físicos —logos PNG y fuentes Variable— se reusan tal cual, byte-exactos respecto del origen. Lo que cambia es la implementación de los tokens y de las jerarquías tipográficas: deja de ser un `theme.dart` con `ColorScheme` y `TextTheme` de Material 3, y pasa a ser un conjunto de `ResourceDictionary` XAML mergeados desde `App.xaml`.

La premisa de marca no cambia: la app es la superficie del **Masterbrand** para Directores, miembros del Comité y Alta Gerencia. El manual exige que la comunicación del Masterbrand no se mezcle con las paletas de submarcas (Azul Salud, Morado Servicios), y esa restricción se mantiene en código: solo se exponen tokens de Masterbrand en `Colors.xaml`.

## Fuentes normativas

- "Manual de Marca Achs", julio 2023, páginas 64-65 (colores) y 71-77 (tipografía y casos de uso).
- Material Design 3 (Color Scheme y Type Scale) — adaptado a tokens XAML.
- CLAUDE.md raíz, §1 (stack MAUI fijo), §2 (seguridad blindada), §5 (dudas con opciones), §7 (una responsabilidad por archivo), §10 (documentación).
- `documentacion/decisiones/brand-identity-achs.md` (versión Flutter, en `develop-flutter`) — fuente de la decisión original.

## Implementación en MAUI

### Archivos clave

- `Resources/Fonts/ACHSNuevaSans-VF.ttf` — Variable Font Sans (122 KB).
- `Resources/Fonts/ACHSNuevaSerif-VF.ttf` — Variable Font Serif (96 KB).
- `Resources/Images/achs_masterbrand_positiva_preferida.png` — logo Masterbrand preferido (20 KB).
- `Resources/Images/achs_masterbrand_positiva_restringida.png` — logo Masterbrand restringido (64 KB).
- `Resources/Styles/Colors.xaml` — tokens de color (raw + Material 3 semantic mapping + estado + canal de denuncias).
- `Resources/Styles/Typography.xaml` — escala tipográfica Material 3 mapeada a Sans / Serif por slot.
- `Resources/Styles/Styles.xaml` — estilos base de botones, tarjetas, layouts.
- `MauiProgram.cs` — registro de Variable Fonts vía `ConfigureFonts` con alias `ACHSNuevaSans` y `ACHSNuevaSerif`.
- `App.xaml` — merge de los tres `ResourceDictionary` en orden colors → typography → styles.

### Registro de fuentes

Las VF se registran como una sola familia por archivo. MAUI mapea `FontAttributes="Bold"` a la variación más pesada disponible:

```csharp
fonts.AddFont("ACHSNuevaSans-VF.ttf", "ACHSNuevaSans");
fonts.AddFont("ACHSNuevaSerif-VF.ttf", "ACHSNuevaSerif");
```

### Mapeo de color (Resources/Styles/Colors.xaml)

| Token XAML | Hex | Slot Material 3 / uso |
|------------|-----|------------------------|
| `BrandGreen` | `#13C045` | seed → `Primary` |
| `BrandGreenDark` | `#004C14` | `OnPrimaryContainer`, texto sobre superficies claras |
| `BrandNeon` | `#7EFF45` | override de `Tertiary` (acento puntual) |
| `BrandBeige` | `#EAEADE` | token expuesto, `SurfaceVariant` |
| `BrandBlack` | `#000000` | `OnTertiary`, texto sobre neón |
| `BrandGreenComplementary` | `#81D877` | token expuesto, gráficos |
| `BrandGreenComplementaryDark` | `#27933E` | `Secondary` |
| `StatusOk` | `#1F7A4F` | estado OK en KPIs y casos |
| `StatusWarn` | `#B7791F` | estado de advertencia |
| `StatusAlert` | `#B42318` | estado de alerta y `Error` |
| `WhistleblowerSurface` | `#F5F2E8` | superficie sobria del Canal de Denuncias |
| `WhistleblowerAccent` | `#27933E` | acento contenido del Canal de Denuncias |

`Background` y `Surface` quedan neutrales (blanco / off-white) para evitar imponer beige como fondo global. Cuando una pantalla quiera usar beige, lo hará explícitamente con `{StaticResource BrandBeige}`. Esa decisión se revisa cuando aterricen las pantallas rediseñadas.

### Mapeo tipográfico (Resources/Styles/Typography.xaml)

Manual página 77, "Casos de uso":

| Style key | Familia | Tamaño | Atributos | Motivo |
|-----------|---------|--------|-----------|--------|
| `DisplayLarge` / `DisplayMedium` | ACHSNuevaSerif | 57 / 45 | — | Títulos editoriales grandes |
| `DisplaySmall` | ACHSNuevaSerif | 36 | — | Transición a tamaños menores |
| `HeadlineLarge` / `Medium` / `Small` | ACHSNuevaSerif | 32 / 28 / 24 | Bold | Jerarquía con contraste vs body |
| `TitleLarge` / `Medium` / `Small` | ACHSNuevaSans | 22 / 16 / 14 | Bold | Subtítulos |
| `BodyLarge` / `Medium` / `Small` | ACHSNuevaSans | 16 / 14 / 12 | — | Lectura corrida |
| `LabelLarge` / `Medium` / `Small` | ACHSNuevaSans | 14 / 12 / 11 | Bold | Botones, chips, captions |

Se añade un `Style` implícito `TargetType="Label"` con `BasedOn={StaticResource BodyMedium}` para que cualquier `<Label/>` sin estilo explícito caiga al Sans de marca en lugar del system font.

### Estilos de componente (Resources/Styles/Styles.xaml)

- `PrimaryButton`, `TonalButton`, `OutlinedButton`, `TextButton` — variantes Material 3 mapeadas a la paleta brand. Todos respetan `MinimumHeightRequest=44` para cumplir tamaño mínimo de toque accesible.
- `CardBorder` — `Border` con `RoundRectangle` de 16, stroke neutral, background `Surface`. Base de `AppCard` y de tarjetas KPI.
- `WhistleblowerCardBorder` — variante sobria sin stroke, background `WhistleblowerSurface`. Distingue visualmente el módulo del Canal de Denuncias.
- `ContentPage` implícito con background `Background`.

## Opciones evaluadas y decisiones

Se preservan las decisiones de la versión Flutter; solo cambia el mecanismo de implementación. Para evitar duplicación, se referencian:

1. **Variable Fonts vs static por peso** — preserva la decisión Flutter: Variable Fonts (un único `.ttf` por familia). El bundle de fuentes se mantiene en 218 KB total.
2. **Color seed `#13C045` Verde Masterbrand** — preserva. `Primary` = `BrandGreen` en `Colors.xaml`. El verde oscuro queda disponible como token `BrandGreenDark` para componentes que necesiten más contraste.
3. **Verde Neón `#7EFF45` solo como acento puntual** — preserva. Ocupa el slot `Tertiary` de Material 3 con `OnTertiary = BrandBlack`. No se usa como fondo de pantalla ni como botón principal.
4. **Colores de estado sobrios (verde `#1F7A4F` / ámbar / rojo)** — preserva. Permite distinguir indicadores OK del chrome verde corporativo de la app.

## Accesibilidad

Los cálculos de contraste de la versión Flutter se preservan porque los colores son idénticos:

- `BrandGreen #13C045` sobre blanco: contraste ~2.5:1 — insuficiente para texto normal. **Regla operativa:** texto sobre superficies `Primary` va con `OnPrimary` (blanco). Verde Masterbrand nunca se usa como color de texto sobre fondo blanco. Para botones `PrimaryButton` (texto blanco sobre verde) el contraste sube a ~6:1, válido AA.
- `BrandNeon #7EFF45` sobre blanco: contraste ~1.5:1 — solo decorativo. Texto sobre neón siempre es negro (`OnTertiary = BrandBlack`, contraste ~14:1).
- `BrandGreenDark #004C14` sobre blanco: contraste ~12:1 — apto para texto en cualquier tamaño.
- Status colors mantienen contraste ≥4.5:1 sobre superficies claras (verificado para `StatusOk` ~5.4:1, `StatusWarn` ~4.5:1, `StatusAlert` ~6.4:1).

Adicionalmente, todos los estilos de botón en `Styles.xaml` declaran `MinimumHeightRequest=44` y `MinimumWidthRequest=44` para garantizar el tamaño mínimo de toque accesible en pantallas táctiles.

## Limitación específica de MAUI: pesos de Variable Font

En la versión Flutter, cada peso de Variable Font se registraba en `pubspec.yaml` apuntando al mismo archivo. Flutter renderizaba la variación correspondiente al `FontWeight` solicitado.

En MAUI 8 hay menor control granular: el sistema mapea `FontAttributes="Bold"` a la variación más pesada disponible, pero no expone los pesos intermedios (300/500/600) como propiedades de `Style`. La jerarquía de Material 3 se preserva por tamaño y por uso de `Bold`, pero los matices de Semibold vs Medium quedan aproximados. La diferencia es perceptible solo a tamaños grandes.

**Acción de seguimiento:** evaluar en la fase de alta fidelidad si conviene registrar el VF múltiples veces con alias por peso (técnica que MAUI soporta) o aceptar la aproximación.

## Limitaciones del set de assets recibido (heredadas)

- **Solo dos PNG de logo**, ambos en versión positiva (Masterbrand sobre fondo claro). No hay versión negativa (logo blanco sobre fondo color) ni versiones verticales del Masterbrand. Esto restringe el splash y el header de auth: el splash actual usa un placeholder textual ("A") sobre `#13C045`, pendiente de logo oficial.
- **Falta favicon/icono dedicado para launcher Android/iOS.** El logo Masterbrand tiene "caja" obligatoria según manual; el manual indica que solo se aplica sin caja en favicon y avatar de redes. El `MauiIcon` actual usa SVG placeholder con `#13C045` como fondo y una "A" como mark — pendiente activos oficiales de launcher.
- **No hay íconos ilustrados** (sección 4 del manual los menciona). Se usarán íconos del sistema (Material Symbols vía glyph fonts si llega a hacer falta) hasta que ACHS los provea.

## Pendientes para iteraciones futuras

1. Pedir versión negativa del logo Masterbrand y versión vertical.
2. Pedir íconos de launcher iOS/Android oficiales para reemplazar el placeholder SVG.
3. Decidir si la `Surface` global se mueve a `BrandBeige` cuando lleguen los mockups de pantallas rediseñadas.
4. Revisar uso del Verde Neón pantalla por pantalla cuando se rediseñen las features actuales.
5. Confirmar política de fallback de fuente cuando el dispositivo no pueda renderizar el VF (manual sugiere Arial y Times New Roman como sustitutos de sistema).
6. Evaluar registro multi-alias del VF para acceder a pesos intermedios (Semibold, Medium).

## Extensión: dark mode + sistema visual M3 expressive (2026-06-03)

La paleta y la jerarquía tipográfica descriptas arriba siguen siendo la
fuente de verdad del light scheme. El 2026-06-03 se incorporó el dark
scheme, los tonal layers Material 3 expressive, los tokens reutilizables
de shape / spacing / elevation, una fuente de iconos (Material Icons
v4.0.0) y un servicio de tema con toggle manual en Perfil. Toda esa
extensión se documentó en `visual-system-m3-dark-mode.md`; este
documento se mantiene como referencia de la base de marca y queda
explícitamente complementario al nuevo.

## Análisis de seguridad de este cambio

- **Activo tocado:** assets estáticos, tokens de color y tipografía. No toca autenticación, autorización, manejo de datos sensibles ni red.
- **Vectores introducidos:**
  - Tamaño del bundle: +338 KB (122 KB Sans + 96 KB Serif + 20 KB + 64 KB de logos). Aceptable para una app de directivos distribuida por Intune.
  - Licencia de fuentes: tipografías propietarias ACHS, aplicación construida para ACHS — uso autorizado por el cliente. Si la app llegara a publicarse en una tienda pública (no es el caso, va por Intune según Propuesta 1), habría que revisar la EULA específica.
- **Mitigaciones:** ninguna requerida más allá de las propias del proceso (commits granulares, no incluir secretos en assets — verificado en `git diff --staged` antes de cada commit, ver §14 CLAUDE.md).
- **Supuestos:** la app sigue siendo de distribución privada por Microsoft Intune, no pública.
- **Estándares aplicables:** OWASP MASVS (sección de assets estáticos), CIS Azure Foundations Benchmark (no aplica al frontend en esta fase).

## Archivos tocados

- `Resources/Fonts/ACHSNuevaSans-VF.ttf` (nuevo, byte-exacto desde `develop-flutter`).
- `Resources/Fonts/ACHSNuevaSerif-VF.ttf` (nuevo, byte-exacto desde `develop-flutter`).
- `Resources/Images/achs_masterbrand_positiva_preferida.png` (nuevo, byte-exacto).
- `Resources/Images/achs_masterbrand_positiva_restringida.png` (nuevo, byte-exacto).
- `Resources/Styles/Colors.xaml` (nuevo).
- `Resources/Styles/Typography.xaml` (nuevo).
- `Resources/Styles/Styles.xaml` (nuevo).
- `MauiProgram.cs` (registro de fuentes).
- `App.xaml` (merge de los tres ResourceDictionary).
- `documentacion/decisiones/brand-identity-achs-maui.md` (este documento).
