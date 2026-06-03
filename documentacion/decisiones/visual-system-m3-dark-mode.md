# Decisión: sistema visual Material 3 + ACHS Masterbrand con dark mode

Fecha: 2026-06-03
Estado: aceptada
Alcance: frontend (`orquestador-frontend`), versión .NET MAUI
Relaciona con: `brand-identity-achs-maui.md` (paleta y tipografía base),
`maui-controls-pin-2026-06.md` (pin del package MAUI Controls)

## Contexto

Tras un primer arranque del prototipo en el dispositivo Xiaomi físico, el
cliente solicitó un pase de pulido visual completo y la incorporación de
modo claro / oscuro con toggle manual. La paleta y la tipografía base ya
existían (ver `brand-identity-achs-maui.md`), pero el sistema cubría solo
los slots semánticos básicos de Material 3 y estaba enteramente hardcoded
para light scheme: cards sin elevación, sin tonal layers, sin estados
visuales, sin tokens de shape/spacing/elevation reutilizables.

Esta decisión documenta el sistema visual ampliado y el approach técnico
elegido para dark mode.

## Decisiones

### 1. Material 3 + ACHS Masterbrand como base estilística

Material 3 sigue siendo el sistema base, aplicado con el masterbrand ACHS
encima. Esta decisión preserva la decisión cerrada del stack
(Propuesta 1 §1) y de marca (`brand-identity-achs-maui.md`). No se
introduce ninguna librería de terceros (UraniumUI, Syncfusion, etc.); el
sistema se construye con los primitivos de MAUI 10.

### 2. Dark mode vía `AppThemeBinding` por `Setter` (idiomatic MAUI)

Cada `Setter` de propiedad de color en los estilos referencia un par
`Light = {StaticResource X}, Dark = {StaticResource DarkX}`. La razón:

- Es el patrón oficial de MAUI para responder al cambio de `AppTheme`
  sin código adicional.
- Es reactivo: cuando `Application.Current.UserAppTheme` cambia, MAUI
  re-evalúa los bindings y los controles se actualizan en el siguiente
  frame.
- Evita la trampa de `StaticResource` cacheado que ocurre cuando se
  intenta swap de `MergedDictionary` en runtime.

Los tokens semánticos sin prefijo (`Primary`, `Surface`, etc.) tienen el
valor del light scheme. Los tokens con prefijo `Dark` (`DarkPrimary`,
`DarkSurface`, ...) tienen el valor del dark scheme. Convención
documentada en la cabecera de `Resources/Styles/Colors.xaml`.

### 3. Tonal layers Material 3 expressive

Se agregaron las cinco capas tonales (`SurfaceContainerLowest`,
`SurfaceContainerLow`, `SurfaceContainer`, `SurfaceContainerHigh`,
`SurfaceContainerHighest`) en ambos esquemas, más sus contrapartes Dark.
Estas capas reemplazan parcialmente a las sombras pesadas: la diferencia
de tono entre el fondo de la página y el de la card produce la sensación
de elevación sin overuse de cast shadow, lo que es coherente con Material
3 expressive y se ve más limpio en pantallas OLED en dark mode.

### 4. Sistema de tokens reutilizables: Shape, Spacing, Elevation

Se introdujeron tres nuevos `ResourceDictionary`:

- `Shape.xaml` — 8 tokens de `CornerRadius` (4/8/12/16/24/28 + None/Full).
- `Spacing.xaml` — 12 tokens de espaciado en grilla de 4 pt.
- `Elevation.xaml` — 5 niveles de `Shadow` para cada esquema.

Los estilos componentizan estos tokens en lugar de números mágicos. Las
páginas, idealmente, también. Esto elimina la deriva del rhythm visual a
medida que el proyecto crece.

### 5. Iconografía: Google Material Icons v4.0.0

Se descartó Material Symbols Outlined VF (~5 MB) por footprint.
Material Icons v4.0.0 (228 KB, Apache 2.0) cubre los iconos que necesita
el chrome de la app (home, bar_chart, description, notifications) y el
estilo es coherente con M3. El archivo se commitea pinneado al tag
v4.0.0 del repo `google/material-design-icons`. SHA256 del archivo:

```
320d3688e085f8485936ee044e694fecb35f3eaf0e68a3efe98bdaf41eaed987
```

Si alguna iteración futura necesita más cobertura iconográfica que la
que ofrece v4.0.0, evaluar Material Symbols Outlined (regular weight,
no VF) como reemplazo más completo pero con mayor footprint.

### 6. Toggle de apariencia en Perfil con persistencia en SecureStorage

Tres opciones para el usuario: Sistema / Claro / Oscuro. La preferencia
se persiste en `SecureStorage` (Keychain/Keystore) usando la clave
`appearance-mode`. La elección de SecureStorage en lugar de `Preferences`
es por consistencia con el resto del estado per-usuario; el dato en sí
es cosmético y no es PII. Detalle del servicio en
`documentacion/modulos/theme-service.md`.

## Paleta dark scheme

Derivada del ACHS Masterbrand verde (`#13C045`) y lifteada para dark:

| Token | Valor | Notas |
|-------|-------|-------|
| `DarkPrimary` | `#4ADB6A` | Verde brand lifteado para mantener legibilidad sobre superficies oscuras. |
| `DarkOnPrimary` | `#003919` | Ink verde oscuro para texto sobre Primary en dark. |
| `DarkPrimaryContainer` | `#00541F` | Container Primary en dark. |
| `DarkOnPrimaryContainer` | `#B8F0C9` | Texto sobre container Primary en dark. |
| `DarkBackground` | `#0F1011` | Casi negro pero no pure (#000) para reducir fatiga OLED. |
| `DarkSurface` | `#161718` | Un escalón arriba del background. |
| `DarkSurfaceContainerLowest..Highest` | `#0A0B0B` → `#323537` | Cinco niveles tonales para stack de cards. |
| `DarkOnSurface` | `#E3E2E5` | Off-white para body text. |
| `DarkOnSurfaceVariant` | `#C4C7C8` | Slate variant para texto secundario. |
| `DarkOutline` / `DarkOutlineVariant` | `#8F9192` / `#43474A` | Borders. |
| `DarkError` | `#FFB4AB` | Error lifteado. |
| `DarkScrim` | `#000000` | Overlay puro (alpha aplicada por consumidor). |
| `DarkWhistleblowerSurface` | `#1F1E1A` | Warm-dark distinto del DarkSurface regular. |
| `DarkWhistleblowerAccent` | `#5DCC8B` | Verde sobrio lifteado para el módulo. |

Status colors (`StatusOk`, `StatusWarn`, `StatusAlert`) también tienen
Dark equivalentes (`DarkStatusOk`, `DarkStatusWarn`, `DarkStatusAlert`)
y containers (`DarkStatusOkContainer`, etc.) lifteados para legibilidad
en superficies oscuras.

## Contraste / accesibilidad

Verificación de contraste WCAG AA (4.5:1 para body, 3:1 para large /
icons) en los pares críticos del dark scheme:

| Par | Contraste estimado | Cumple AA |
|-----|--------------------|-----------|
| `DarkOnSurface` sobre `DarkSurface` | ~13.5:1 | ✓ todo tamaño |
| `DarkOnSurfaceVariant` sobre `DarkSurface` | ~9.6:1 | ✓ todo tamaño |
| `DarkOnSurface` sobre `DarkBackground` | ~14.8:1 | ✓ todo tamaño |
| `DarkOnPrimary` sobre `DarkPrimary` | ~5.6:1 | ✓ body |
| `DarkOnPrimaryContainer` sobre `DarkPrimaryContainer` | ~9.2:1 | ✓ body |
| `DarkOnSecondaryContainer` sobre `DarkSecondaryContainer` | ~8.8:1 | ✓ body |
| `DarkError` sobre `DarkSurface` | ~7.8:1 | ✓ body |

Status colors en dark:
- `DarkStatusOk` (`#5DCC8B`) sobre `DarkSurface` (`#161718`) ≈ 7.3:1 ✓
- `DarkStatusWarn` (`#E4A845`) sobre `DarkSurface` ≈ 8.5:1 ✓
- `DarkStatusAlert` (`#FF8A7A`) sobre `DarkSurface` ≈ 7.1:1 ✓

Adicionalmente el KpiCard del dashboard usa **dot + label** para
comunicar estado (no depende solo del color, WCAG 1.4.1).

Touch targets ≥ 44pt preservados en todos los estilos de botón, switch,
checkbox y radio (Manual ACHS sección 8.4 y CLAUDE.md §7).

## Análisis de seguridad (§2 CLAUDE.md)

- **Activo tocado:** assets visuales (tokens, fonts) y la preferencia
  cosmética del usuario. No toca autenticación, autorización, datos del
  Canal de Denuncias, ni red.
- **Vectores introducidos:**
  - Bundle: +228 KB por la font de Material Icons. Aceptable para una app
    de directivos distribuida por Intune.
  - Dependencia visual de un asset externo (Material Icons v4.0.0,
    Apache 2.0). Pinneado por tag + sha256 documentado arriba.
  - `SecureStorage` lee/escribe una clave nueva (`appearance-mode`). No
    es PII. Falla degrada al default System sin crash (Catch genérico
    documentado en `ThemeService`).
- **Mitigaciones:**
  - Font font firmada/checkable (sha256 en este documento).
  - Apariencia no expone ningún canal nuevo de input externo.
  - `ThemeService` no loguea valores; cumple §6 (sin secretos en logs).
- **Supuestos:** la app sigue siendo de distribución privada por
  Microsoft Intune, no pública. Si en algún momento se publica, revisar
  EULA de Google Material Icons (Apache 2.0 permite redistribución).
- **Estándares aplicables:** OWASP MASVS (sección de assets estáticos),
  WCAG 2.2 AA (contraste y no-color-only).

## Archivos tocados

- `Resources/Styles/Colors.xaml` — set Dark + tonal layers + inverse + scrim.
- `Resources/Styles/Shape.xaml`, `Spacing.xaml`, `Elevation.xaml` — nuevos.
- `Resources/Styles/Typography.xaml` — TextColor por AppThemeBinding.
- `Resources/Styles/Styles.xaml` — Buttons, Cards, FieldBorder, Switch,
  Entry, Picker, etc. con AppThemeBinding y VisualStates.
- `Resources/Fonts/MaterialIcons-Regular.ttf` — nuevo asset.
- `AppShell.xaml` — Shell theming + 4 iconos de tabs.
- `App.xaml` — orden de merge actualizado.
- `MauiProgram.cs` — registro de la font y del `IThemeService`.
- `Services/AppearanceMode.cs`, `IThemeService.cs`, `ThemeService.cs` — nuevos.
- `Features/Auth/LoginPage.xaml`, `BiometricReauthPage.xaml` — rediseño hero.
- `Features/Home/HomePage.xaml`, `KpiCard.xaml` — surface containers, status dot.
- `Features/Profile/ProfileViewModel.cs`, `ProfilePage.xaml` — toggle Apariencia.
- 12 archivos XAML de Features migrados a AppThemeBinding.
- `documentacion/decisiones/visual-system-m3-dark-mode.md` — este documento.

## Pendientes para iteraciones futuras

1. Logo en versión negativa (logo blanco) para usar en dark mode sin el
   contenedor light. El logo actual queda dentro de un container
   `SurfaceContainerLowest` para preservar contraste en dark — workaround
   válido pero no la solución de marca definitiva.
2. Validar contraste con el cliente / accesibilidad de ACHS (eventual
   chequeo formal con herramienta de auditoría).
3. Cuando aparezca el set oficial de iconos de marca ACHS, reemplazar
   Google Material Icons.
4. Ampliar VisualStates al `Border` tappable (HomePage acción rápida,
   Reports list, etc.) para feedback de pressed; hoy solo los `Button`
   nativos lo tienen.
