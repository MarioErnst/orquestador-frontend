# Módulo: Reports y Documents

## Qué hace

**Reports**: lista de tableros Power BI (filtrada por rol) + visor a pantalla completa con placeholder de Power BI Embedded.

**Documents**: lista mixta de informes PDF, videos institucionales y enlaces a SharePoint + visor universal que ramifica el rendering según el tipo, incluyendo el bloque de resumen IA junto al documento original.

## Archivos clave

### Reports

- `Features/Reports/ReportsListPage.xaml` / `.cs` + `ReportsListViewModel.cs` — Lista vertical de boards con título y descripción. Tap → `Shell.Current.GoToAsync("board?id=...")`.
- `Features/Reports/BoardViewerPage.xaml` / `.cs` + `BoardViewerViewModel.cs` — Recibe `id` por `[QueryProperty]`, carga el board y muestra header (nombre, descripción) + placeholder del tablero embebido. `TabBarIsVisible=False` para ocupar pantalla completa.

### Documents

- `Features/Documents/DocumentsListPage.xaml` / `.cs` + `DocumentsListViewModel.cs` — Lista vertical de reports (PDF + video + SharePoint). Cada tarjeta tiene una píldora con el tipo (`PDF`/`Video`/`SharePoint`) en `PrimaryContainer`. Tap → `document?id=...`.
- `Features/Documents/DocumentViewerPage.xaml` / `.cs` + `DocumentViewerViewModel.cs` — Visor universal. Carga el report y expone `IsPdf`, `IsVideo`, `IsSharepoint`, `HasAiSummary` para que la XAML ramifique con `IsVisible`. Cada tipo tiene su propio placeholder visualmente distinto (visor PDF gris claro, reproductor de video negro, card de SharePoint).
- `Features/Documents/Controls/AiSummaryBlock.xaml` / `.cs` — Border en `SecondaryContainer` con etiqueta "Resumen generado por IA" + el resumen + nota "Asistencia a la lectura. El documento original es la fuente."
- `Features/Documents/ReportKindLabelConverter.cs` — Convierte `ReportKind` a label español ("PDF"/"Video"/"SharePoint"). Registrado en `App.xaml`.

## Decisiones

- **Visor único `DocumentViewerPage` en lugar de PdfViewer/VideoPlayer separados**: el flujo desde notificaciones es uniforme (`document?id=...`), no requiere conocer el tipo en navegación. El page resuelve el tipo después de cargar el report y muestra el placeholder apropiado. Reduce número de routes y simplifica `NotificationNavigationService`.
- **`AiSummaryBlock` siempre etiquetado como IA**: el chrome usa `SecondaryContainer` distinto del card normal para diferenciar visualmente. La nota "asistencia a la lectura" refuerza la regla de Propuesta 1 §9.1: la IA asiste, no reemplaza.
- **Placeholders explícitos para Power BI, PDF y video**: comentarios `// PROTOTYPE:` en XAML y en VMs marcan qué se reemplaza en producción (visor de Power BI vía `HybridWebView` + librería JS `powerbi-client` con token del backend, visor PDF nativo, reproductor de video).
- **`[QueryProperty]` con `OnXChanged` partial method**: cada vez que Shell asigna el query parameter, el ViewModel dispara `LoadAsync` sin necesidad de `OnAppearing`.

## Cómo se integra

- Las rutas `board` y `document` se registran en `AppShell.xaml.cs`.
- Reports y Documents son tabs de la TabBar principal (`main/reports`, `main/documents`).
- Visores son sub-rutas que ocultan la TabBar (`Shell.TabBarIsVisible=False`).
- El `NotificationNavigationService` mapea `NotificationKind.NewReport` → `document?id=...`, llegando al visor universal.

## Pendientes

- Implementar el visor real de Power BI en `BoardViewerPage` con un `HybridWebView` que hospeda la librería JavaScript `powerbi-client` (no existe control nativo de Power BI para MAUI; el render ocurre en un WebView). El `embedUrl` y el embed token de vida corta vienen del backend per Propuesta 1 §5.7. La WebView requiere CSP restrictivo y `powerbi-client` pinneado a versión exacta (CLAUDE.md §16 y §8).
- Implementar visor PDF nativo (paquete community o iframe). La URL del PDF viene firmada del backend, vida corta ≤15 minutos per CLAUDE.md §8.
- Implementar reproductor de video con `MediaElement` de MAUI Community Toolkit, con URL firmada.
- Bloquear copy/paste y screenshots cuando esté activa la política de Intune (en producción).
