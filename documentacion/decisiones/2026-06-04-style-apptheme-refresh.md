# Decisión: refresh genérico de `AppThemeBinding` en Setters de `Style`

- Fecha: 2026-06-04
- Autor: humano (operador del repo)
- Estado: aplicada
- Disparador: bug observado en `Features/Home/HomePage.xaml` — las tarjetas
  de Accesos rápidos ("Último informe publicado" y "Tablero habitual")
  se quedaban con el `BackgroundColor` del tema anterior al cambiar de
  Apariencia.

## Contexto

`ThemeService` ya tenía workarounds para la familia de bugs de
`AppThemeBinding` (`dotnet/maui#6596`, `dotnet/maui#20243`) en tres
superficies:

1. Chrome del `Shell` (`BackgroundColor`, `TabBar`, etc.).
2. `ContentPage.BackgroundColor`.
3. Items de `CollectionView` (vía reset de `ItemsSource`).

Pero ninguna de las tres cubría el caso en el que un `AppThemeBinding`
se declara dentro de un `Setter` de `Style` que se aplica a un control
suelto del árbol visual (ej. un `Border` con
`Style="{StaticResource CardBorder}"`). El `Setter` resuelve el binding
una vez y el valor queda congelado.

Esto afectaba al menos a `CardBorder`, `ElevatedCardBorder` y
`WhistleblowerCardBorder` en `Resources/Styles/Styles.xaml`, y a los
estilos de `Resources/Styles/Typography.xaml` cuyo `TextColor` se setea
con `AppThemeBinding`.

## Opciones consideradas

1. **Mapa hardcodeado de `StyleKey → ColorKey`.** Extender el patrón
   existente de `ForceShellChromeRefresh` con un diccionario por
   `Style` conocido. Sencillo pero frágil: cualquier `Style` nuevo
   requeriría sumar entradas al mapa.
2. **Re-aplicar el `Style` (`Style = null; Style = original;`)** sobre
   cada `Border` / `Label` del árbol. Genérico pero opaco: re-dispara
   *todos* los Setters, no sólo los del binding de tema, e introduce
   riesgo de side effects con triggers o animaciones.
3. **Migrar a `ResourceDictionary` por tema** (`LightTheme.xaml` /
   `DarkTheme.xaml`) reemplazando `AppThemeBinding` por
   `StaticResource`. Solución estructural y definitiva, pero refactor
   invasivo sobre toda la base de estilos justo después de cerrar el
   ciclo de UI polish.
4. **Walker genérico que reaplica sólo Setters con `AppThemeBinding`.**
   Recorre el árbol visual, lee `Style.Setters` (incluyendo `BasedOn`),
   y para cada Setter cuyo `Value` sea `AppThemeBinding` resuelve
   `Light` / `Dark` y asigna vía `SetValue` directo. Genérico, mínimo
   ruido en el resto de Setters, sin requerir mantener mapas.

## Decisión

Se elige la opción 4, con tres piezas que juntas hacen que el fix se
sostenga incluso con tabs lazy:

1. **`Services/StyleAppThemeRefresher.cs`** — walker estático.
   Recorre un `IVisualTreeElement` y, para cada `VisualElement` con
   `Style`, enumera Setters (incluyendo `BasedOn`). Para cada Setter
   cuyo `Value` sea `AppThemeBinding`, **primero** llama
   `RemoveBinding(property)` para matar el binding roto y **después**
   `SetValue(property, resolved)` con el `Color` resuelto del slot
   `Light` / `Dark`. Sin el `RemoveBinding`, el binding cacheado
   re-dispara en el mismo dispatch cycle y pisa el valor local con el
   color del tema anterior.

2. **`ThemeService.ForceStyleSettersRefresh`** — pasada agregada al
   final de `ForceShellChromeRefresh`. Itera
   `EnumerateContentPages(shell)` (currentPage + materializadas en
   navigation/modal stacks) y dispara el walker en cada una.

3. **`ThemeService.EnsureShellNavigationHooked` + `OnShellNavigated`** —
   suscripción a `Shell.Navigated`. Cobertura del caso que llevó al
   bug reportado: las tabs de `AppShell.xaml` usan
   `ContentTemplate="{DataTemplate ...}"`, así que sus páginas son
   lazy. Cuando el usuario toggle el tema desde la página de Perfil
   (`Shell.Current.CurrentPage == ProfilePage`), `HomePage` está
   materializada en memoria pero su contenido **no** aparece bajo
   `ShellContent.Content` ni bajo `EnumerateContentPages`, así que la
   pasada de `ForceStyleSettersRefresh` no la alcanza. La página queda
   con los `AppThemeBinding` cacheados del tema viejo hasta que se
   reconstruye. El hook a `Shell.Navigated` resuelve esto: cada vez
   que una página se vuelve visible, el walker corre sobre ella y
   re-aplica los colores resueltos del tema actual.

## Consecuencias

- **Escalabilidad.** Cualquier `Style` nuevo del proyecto queda
  cubierto automáticamente, sin tocar `ThemeService` ni el helper.
- **Acoplamiento a internals de MAUI.** `Microsoft.Maui.Controls.AppThemeBinding`
  está marcado `internal`. El helper lo accede por reflection cacheada
  al cargar la clase; si MAUI cambia el namespace o el nombre, los
  caches quedan nulos y el helper hace no-op (las otras tres pasadas
  siguen vigentes — el peor caso es que reaparezca el bug original, no
  un crash).
- **Costo en runtime.** El walker recorre cada `ContentPage`
  materializada al cambiar tema. El recorrido es O(n) sobre el árbol
  visible, con reflection cacheada y operaciones cheap (lectura de
  PropertyInfo + `SetValue`). Se ejecuta solamente en el cambio de
  modo, no por frame.
- **Cobertura.** Setters declarados directamente y los heredados vía
  `BasedOn` están cubiertos. El walker mantiene el orden de precedencia
  de MAUI ("derived gana sobre base") usando un `HashSet<BindableProperty>`
  para evitar que un Setter de la base pise al derivado.
- **Inline `AppThemeBinding`.** El walker NO cubre el caso en el que
  `AppThemeBinding` se declara inline directamente sobre una propiedad
  de un control (ej. `Border BackgroundColor="{AppThemeBinding ...}"`).
  Ese caso está cubierto sólo para `ContentPage.BackgroundColor` por
  `ForceBackgroundOnContentPages`. Si en el futuro se reporta un caso
  inline en otro elemento, se extenderá entonces.
- **Pérdida del binding reactivo.** `RemoveBinding` mata el
  `AppThemeBinding` original, así que después de la primera pasada
  esa propiedad ya no responde sola a `RequestedThemeChanged`. Por
  diseño: el walker se ejecuta en cada toggle (vía
  `ForceShellChromeRefresh`) y en cada navegación de Shell (vía
  `OnShellNavigated`), así que el reactor de tema imperativo es el
  walker, no el binding.

## Análisis de seguridad

- **Activo afectado:** ninguno crítico — el cambio es puramente cosmético
  (color de fondo / borde / texto de superficies UI).
- **Vectores de ataque introducidos:** ninguno. El helper no toca red,
  almacenamiento, autenticación, autorización ni auditoría. La
  reflection es sobre un tipo del propio runtime de MAUI, no sobre
  ensamblados de terceros ni cargados dinámicamente.
- **Mitigaciones:** el helper hace no-op si los handles de reflection
  quedan nulos (ej. cambio interno de MAUI), evitando excepciones que
  pudieran cortar el render del Shell.
- **Estándar aplicable:** N/A (sin superficie de seguridad afectada).

## Referencias

- Issue tracker MAUI: `dotnet/maui#6596`, `dotnet/maui#20243`.
- Archivos tocados:
  - `Services/StyleAppThemeRefresher.cs` (nuevo)
  - `Services/ThemeService.cs` (nueva pasada `ForceStyleSettersRefresh`)
  - `documentacion/modulos/theme-service.md` (sección "Workarounds para
    bugs de `AppThemeBinding` en MAUI 10")
