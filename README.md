# orquestador-frontend

App móvil para los **Directivos de la Asociación Chilena de Seguridad (ACHS)**:
Directores, miembros del Comité y Alta Gerencia. Cliente del sistema descrito
en `Propuesta 1 — Arquitectura, Infraestructura y Seguridad` y
`Propuesta 2 — Diseño y Experiencia Visual`.

Este repositorio contiene el **prototipo visual en Flutter**: pantallas
navegables con datos de ejemplo (mock), sin backend ni autenticación real.
Es la base del proyecto final — no se desecha. El día que se conecte el
backend, solo se reemplaza la capa de datos; las pantallas no cambian.

## Stack

- Flutter `3.44.0` stable / Dart `3.12.0`
- Material 3
- Riverpod (gestión de estado) y go_router (navegación) — se incorporan en la
  rama siguiente
- Una sola base de código para iOS, Android y desarrollo en web

## Arquitectura

Layered + feature-first, descrita por la sección 4 del documento
`Contexto/INSTRUCCIONES_CLAUDE_CODE_prototipo_flutter.md`:

```
lib/
  main.dart                   # solo monta App()
  app/                        # widget raíz, tema, router
  core/                       # tipos transversales (roles, result state)
  data/
    models/                   # modelos puros inmutables
    repositories/             # contratos (interfaces)
    mock/                     # implementaciones mock + datos de ejemplo
  providers/                  # Riverpod (a partir de la próxima rama)
  features/                   # pantallas por feature
  shared/widgets/             # componentes reutilizables
```

Reglas no negociables (las "dos barandas" de la spec):

1. **Las pantallas nunca acceden a datos directamente.** Todo va por un
   repositorio expuesto vía Riverpod. Hoy ese repositorio es un mock; mañana
   es un cliente HTTP del backend ACHS. La UI no se entera del cambio.
2. **Una sola gestión de estado en todo el proyecto: Riverpod.** No se mezcla
   con otros gestores ni con `setState` para datos.

La documentación operativa de cada módulo vive en `documentacion/modulos/`.

## Cómo correrlo

Requiere Flutter 3.44.0 en `PATH` y un binario de Chromium o Chrome accesible
vía la variable de entorno `CHROME_EXECUTABLE` (Fedora: típicamente
`/usr/bin/chromium-browser`).

```bash
# Verificar el entorno
flutter doctor

# Correr en Chrome (target principal de desarrollo)
flutter run -d chrome

# Correr en Linux desktop (si tenés clang/ninja/gtk instalados)
flutter run -d linux

# Correr en un emulador o dispositivo Android
flutter run -d <device-id>      # ver con: flutter devices
```

Para detener el `flutter run`, apretar `q` en la terminal donde corre.

## Tests y análisis estático

```bash
flutter analyze                 # análisis estático (lints)
flutter test                    # tests unitarios y de widget
flutter build web               # compilar para web (output en build/web/)
```

## Convención de ramas

- `main` — versión publicada. Cada commit acá representa una entrega.
- `develop` — integración entre features estables.
- `feat/<nombre>` — rama por unidad de trabajo, sale de `develop` y vuelve a
  `develop` cuando está lista para mostrar.

Push solo cuando el humano responsable lo indica de forma explícita; ver las
reglas del `CLAUDE.md` del proyecto.

## Estado actual

Rama `feat/initial-scaffold`:

- Andamiaje completo (estructura, theme Material 3, tipos transversales).
- Capa de datos completa en mocks (5 dominios, datos realistas en español).
- Pantalla placeholder navegable en Chrome para verificar el tema aplicado.

Pendiente (próximas ramas): Riverpod + go_router, login + selector de rol,
shell con barra inferior, pantallas por feature, componentes transversales
(loading / empty / error / no permission), audit badge del Canal de
Denuncias.
