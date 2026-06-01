# orquestador-frontend

App móvil para directivos ACHS — prototipo visual en .NET MAUI.

## Stack

- **.NET 10** + **.NET MAUI** (C#).
- **CommunityToolkit.Mvvm** para gestión de estado (ViewModels con `[ObservableProperty]` y `[RelayCommand]`).
- **Microsoft.Extensions.DependencyInjection** para inyección de dependencias.
- **Shell navigation** de .NET MAUI, con guardas de ruta por rol.
- **Material 3** como base de diseño visual, vía estilos MAUI en `Resources/Styles/`.
- **Identidad ACHS Masterbrand** (`#13C045` verde, tipografías ACHS Nueva Sans / ACHS Nueva Serif).

## Estado actual

Prototipo visual sin backend. Los datos se sirven desde implementaciones mock detrás de interfaces de repositorio. La conexión a backend, la autenticación real (Entra ID / MSAL.NET), Power BI Embedded y Azure OpenAI son fases posteriores.

## Requisitos para correr

- .NET 10 SDK con el workload de MAUI: `sudo dotnet workload install maui-android` (para iOS adicionalmente `maui-ios` en un Mac).
- Android: emulador Android API 26+ y Android SDK.
- iOS: macOS con Xcode (la compilación de iOS solo es posible desde macOS).

## Cómo correrlo

```bash
# Restaurar dependencias
dotnet restore

# Correr en emulador Android
dotnet build -t:Run -f net10.0-android

# Correr en iOS (solo macOS)
dotnet build -t:Run -f net10.0-ios
```

## Estructura

Ver `documentacion/decisiones/architecture-maui.md` para la decisión de arquitectura, y `Contexto/INSTRUCCIONES_CLAUDE_CODE_prototipo_maui.md` (en la raíz general del proyecto) para la especificación detallada del prototipo.

## Documentación

- `documentacion/decisiones/` — decisiones de diseño y arquitectura.
- `documentacion/modulos/` — un archivo por módulo implementado.
- `documentacion/seguridad/` — análisis de seguridad por feature.
