# Módulo: Controles compartidos

## Qué hace

Provee los seis `ContentView` reutilizables que dan consistencia visual y de comportamiento a todas las pantallas del prototipo. Son la única forma autorizada de renderizar estados de carga, vacío, error y sin permiso. Implementarlos una sola vez evita inconsistencias entre features.

## Archivos clave (en `Shared/Controls/`)

- `LoadingState.xaml` / `.cs` — `ActivityIndicator` en color `Primary` + texto. Propiedad bindable `Message` (default "Cargando…").
- `EmptyState.xaml` / `.cs` — Título + mensaje. Bindable `Title` y `Message` con defaults amables.
- `ErrorState.xaml` / `.cs` — Título en color `Error` + mensaje + botón Reintentar. Bindable `Title`, `Message`, `RetryCommand`. El botón se enlaza al `LoadCommand` del ViewModel.
- `NoPermissionState.xaml` / `.cs` — Mensaje sobrio para roles sin acceso (NO aplica al Canal de Denuncias, que se renderiza con ausencia total).
- `AuditBadge.xaml` / `.cs` — Píldora "Acceso registrado" en `SecondaryContainer`. Bindable `Label`. Se usa exclusivamente en pantallas del Canal de Denuncias.
- `SectionHeader.xaml` / `.cs` — Título + subtítulo opcional + slot `Trailing` para acciones inline ("Ver todo", etc.). Detecta subtítulo vacío para ocultar la segunda línea.

## Decisiones

- **No hay `AppCard` como ContentView separada**: el estilo `CardBorder` en `Resources/Styles/Styles.xaml` ya provee la apariencia. Las páginas usan `<Border Style="{StaticResource CardBorder}">` directamente. Si necesitan tap, agregan `TapGestureRecognizer` inline. Decisión por simplicidad — un control wrapper agregaba complejidad de templating sin beneficio.
- **No hay íconos**: el Manual de Marca ACHS no incluye íconos ilustrados (sección 4 lo lista como pendiente). Los controles usan tipografía y forma para diferenciar estados. El `AuditBadge` es una píldora sin ícono.
- **Bindable properties simples**: el ContentView tiene `x:Name="Root"` y los hijos bindean con `Source={x:Reference Root}`. Patrón estándar de MAUI.
- **`MinimumHeightRequest = 44`** en botones (definido en `Styles.xaml`) para cumplir tamaño táctil accesible. Heredado por todos los estilos de botón.

## Cómo se integra

Cada página que carga datos sigue el patrón:

```xml
<Grid>
    <shared:LoadingState IsVisible="{Binding State, Converter={StaticResource IsLoadingConverter}}" />
    <shared:ErrorState   IsVisible="{Binding State, Converter={StaticResource IsFailureConverter}}"
                         Message="{Binding State, Converter={StaticResource FailureMessageConverter}}"
                         RetryCommand="{Binding LoadCommand}" />
    <shared:EmptyState   IsVisible="{Binding State, Converter={StaticResource IsEmptyConverter}}"
                         Title="..." Message="..." />
    <CollectionView IsVisible="{Binding State, Converter={StaticResource IsDataConverter}}"
                    ItemsSource="{Binding State, Converter={StaticResource DataValueConverter}}">
        ...
    </CollectionView>
</Grid>
```

Los converters viven en `Core/ResultStateConverters.cs` y se registran globalmente en `App.xaml`. Branch logic vive en XAML, no en code-behind, manteniendo la Baranda 2.

## Pendientes

- Cuando lleguen íconos ilustrados oficiales de ACHS, agregar variantes con ícono a los estados.
