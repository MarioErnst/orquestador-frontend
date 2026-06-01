# Módulo: Andamiaje del proyecto MAUI

## Qué hace

Sienta el chasis del proyecto .NET MAUI sobre el que se construyen las features: el csproj con paquetes pinneados, el bootstrap de `MauiProgram.cs`, la `App` raíz que carga estilos, el `AppShell` que orquesta navegación, las plataformas Android e iOS, y los recursos visuales (íconos, splash, estilos).

## Archivos clave

- `OrquestadorFrontend.csproj` — target frameworks `net8.0-android;net8.0-ios`, paquetes `Microsoft.Maui.Controls 8.0.100`, `Microsoft.Extensions.Logging.Debug 8.0.1`, `CommunityToolkit.Mvvm 8.4.0`. Versiones exactas per CLAUDE.md §8 (no `latest`, no rangos).
- `MauiProgram.cs` — registro completo de DI: repositorios singleton, servicios singleton, pages/ViewModels transient, fuentes Variable.
- `App.xaml` / `App.xaml.cs` — `ResourceDictionary` merged (colores, tipografía, estilos) + converters de estado registrados como recursos globales. `MainPage = AppShell` (inyectado por DI).
- `AppShell.xaml` / `AppShell.xaml.cs` — `TabBar` con 4 pestañas + `Routing.RegisterRoute` para rutas profundas + handler `Navigating` para el guard del Canal de Denuncias.
- `Platforms/Android/` — `MainActivity` con tema MAUI, `MainApplication` que crea la MAUI app, `AndroidManifest.xml` con permisos mínimos (`INTERNET`, `ACCESS_NETWORK_STATE`).
- `Platforms/iOS/` — `AppDelegate`, `Program`, `Info.plist` con orientaciones soportadas.
- `Resources/AppIcon/` — `appicon.svg` (fondo verde Masterbrand) + `appiconfg.svg` (mark "A" placeholder). Pendiente icono oficial.
- `Resources/Splash/` — `splash.svg` placeholder con verde Masterbrand.

## Decisiones

- **Solo iOS y Android** como targets. Sin Windows ni Mac Catalyst — fuera del alcance de la Propuesta 1.
- **`SingleProject=true`**: estructura de recursos unificada (no MVVM cross-platform clásico de Xamarin.Forms).
- **Identidad gráfica desde el día uno**: icono y splash usan verde Masterbrand `#13C045`. Los placeholders están claramente marcados como tales en los comentarios SVG.
- **`ApplicationId = cl.achs.directivos.orquestador`**: namespace TLD chileno, coherente con el cliente.

## Cómo se integra

- `MauiProgram.CreateMauiApp()` se invoca desde `Platforms/Android/MainApplication.cs` y `Platforms/iOS/AppDelegate.cs`.
- `App` se inyecta via DI con `AppShell` ya resuelto (que a su vez recibe `ISessionService` e `IWhistleblowerRepository`).
- Los assets en `Resources/Fonts/` y `Resources/Images/` los recoge el implicit glob de MAUI 8 — no requiere `<ItemGroup>` manual en el csproj.

## Pendientes

- Reemplazar SVGs placeholder por activos oficiales ACHS de launcher.
- Configurar pipeline CI/CD (gatillo §19.3 CLAUDE.md).
