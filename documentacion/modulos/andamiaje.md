# Andamiaje del prototipo Flutter

## Propósito

Esta es la base del repositorio `orquestador-frontend/`. Define la estructura
de carpetas, el tema visual provisional y los tipos transversales que el resto
del prototipo va a usar. Implementa el paso 1 de la sección 9 (orden de
trabajo) de `Contexto/INSTRUCCIONES_CLAUDE_CODE_prototipo_flutter.md`.

## Stack y versiones

- Flutter `3.44.0` stable, Dart `3.12.0`.
- Material 3 (`useMaterial3: true`).
- Una sola base de código para web (desarrollo), Android (validación) e iOS
  (scaffolding listo aunque solo compile en macOS).
- `org` para los identificadores de bundle: `cl.achs.directivos`.

## Estructura de carpetas vigente

```
lib/
  main.dart                  # solo monta App(); sin lógica
  app/
    app.dart                 # widget raíz, MaterialApp con tema y placeholder
    theme.dart               # tokens y ThemeData light/dark
  core/
    roles.dart               # enum UserRole + extensión con etiquetas
    result_state.dart        # sealed class Loading/Data/Empty/Failure<T>
  data/
    models/                  # modelos puros inmutables
    repositories/            # interfaces (contratos)
    mock/                    # implementaciones mock + datos de ejemplo
```

`lib/providers/`, `lib/features/` y `lib/shared/` aún no existen: se crean en
las ramas siguientes cuando haya código para poner ahí. La regla "ningún
archivo con más de una responsabilidad" del `CLAUDE.md` se respeta desde el
inicio.

## Tema (`lib/app/theme.dart`)

- Color primario provisional: azul corporativo profundo
  (`Color(0xFF003D7C)`), usado como semilla del `ColorScheme.fromSeed`. Cuando
  llegue la marca definitiva de ACHS, **solo este archivo cambia** — no hay
  colores sueltos en ningún otro lugar.
- Paleta sobria de estados: `statusOk`, `statusWarn`, `statusAlert`. La spec
  exige acompañar el color con texto o ícono, nunca color-only.
- Tokens de geometría: `radius`, `spacing`.
- `ThemeData` se construye con `useMaterial3: true`. Se exponen `light()` y
  `dark()` — el `dark` viene gratis con Material 3 y se deja activo desde el
  inicio porque conviene probarlo temprano.
- `cardTheme`, `filledButtonTheme` y `outlinedButtonTheme` están centralizados
  para garantizar que todas las superficies y botones respeten el radio y los
  touch targets mínimos (44×44 px, regla móvil del `CLAUDE.md` raíz).

## Tipos transversales (`lib/core/`)

### `roles.dart`

```dart
enum UserRole { director, comite, altaGerencia }
```

Los nombres y la cantidad de roles **deben coincidir** con la Propuesta 2 y
con el contrato del backend cuando se construya. La extensión `UserRoleX.label`
expone la etiqueta en español para la UI.

### `result_state.dart`

```dart
sealed class ResultState<T> { ... }
final class Loading<T> ...
final class Data<T> { final T value; ... }
final class Empty<T> ...
final class Failure<T> { final String message; final Object? cause; ... }
```

Modela los cuatro estados transversales que toda pantalla con datos debe
manejar (sección 6 de la spec). `sealed` obliga al compilador a verificar
que el `switch` cubra los cuatro casos — si alguien agrega una variante,
todas las pantallas que ya consumen `ResultState` muestran un error de
compilación hasta cubrirla.

## Decisiones tomadas y por qué

- **No usar `freezed` / `json_serializable` por ahora.** Los modelos son
  inmutables a mano. Cuando lleguen los DTOs reales del backend y queramos
  generadores de código, lo evaluamos. Hoy agregar generadores es ruido sin
  beneficio.
- **No agregar `flutter_riverpod` ni `go_router` todavía.** Esta rama solo
  trae andamiaje + capa de datos mock; los providers y la navegación viven
  en su propia rama. La regla del `CLAUDE.md` es no acumular dependencias
  sin uso.
- **Dark theme habilitado desde el inicio.** Material 3 lo da gratis a partir
  del seed; activarlo desde el día 1 evita sorpresas más adelante.
- **Sin `.gitkeep`** en las carpetas que vendrán después. Aparecen cuando
  tienen archivos.

## Verificación realizada

- `flutter analyze`: sin issues.
- `flutter test`: 1 test (smoke test del widget raíz) en verde.
- `flutter build web`: compila en ~55 s. Output queda en `build/web/`
  (gitignored).

Para correrlo en tu navegador:

```bash
cd orquestador-frontend
flutter run -d chrome
```

(Requiere `CHROME_EXECUTABLE` apuntando a un binario de Chromium/Chrome, ya
configurado en `~/.zshrc` durante el setup del entorno.)
