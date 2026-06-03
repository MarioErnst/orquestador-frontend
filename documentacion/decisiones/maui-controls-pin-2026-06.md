# Decisión: pin de Microsoft.Maui.Controls a 10.0.20 alineado al workload

Fecha: 2026-06-02
Estado: aceptada (transitoria)
Alcance: frontend (`orquestador-frontend`), `OrquestadorFrontend.csproj`

## Contexto

El workload `maui-android` instalado en la máquina de desarrollo es `10.0.20`
(la versión de MAUI que viene con el .NET 10 GA del 2026-05). El package
`Microsoft.Maui.Controls` referenciado en `OrquestadorFrontend.csproj` debe
coincidir exactamente con la versión del workload: si el package es mayor, el
build falla con un mismatch de assemblies.

El csproj traía pinneado `10.0.108` (decisión heredada de una iteración
posterior al instalar el workload) y el build local fallaba consistentemente.
Para destrabar la iteración del prototipo —que se ejecuta en el dispositivo
Xiaomi físico, sin emulador disponible en Fedora 43— se bajó el pin a
`10.0.20`.

## Implicancia de seguridad (§2, §8 CLAUDE.md)

- **Activo afectado:** dependencia del cliente. No toca autenticación,
  autorización, datos sensibles ni red.
- **Vector introducido:** ninguno conocido. El downgrade va de un patch
  posterior (`.108`) a un patch anterior (`.20`) **dentro del mismo major
  10.0**. No se conocen CVEs públicas de `Microsoft.Maui.Controls 10.0.20`
  vigentes a la fecha de este documento.
- **Mitigación:** este pin es transitorio. La acción de seguimiento (ver
  abajo) levanta el workload local y el package del csproj juntos cuando se
  ponga en marcha el CI y el chequeo automático de CVEs (§19.3).
- **Estándar aplicado:** OWASP ASVS V14.2 (gestión de dependencias). Versión
  exacta pinneada (sin rango, sin `latest`), changelog conocido, decisión
  documentada.

## Acción de seguimiento

1. Cuando se levante el pipeline de CI (gatillo §19.3 de CLAUDE.md),
   ejecutar `dotnet list package --vulnerable --include-transitive` contra
   este csproj y registrar el resultado.
2. Alinear el workload local de MAUI al último patch publicado de la línea
   `10.0.x` (o `10.x` mayor si Microsoft saca uno con migración soportada).
3. Subir el pin de `Microsoft.Maui.Controls` a la misma versión del workload
   actualizado y revisar el changelog incremental para hallazgos de
   seguridad.
4. Borrar este documento cuando la acción 3 se complete; reemplazarlo por
   una nota corta en `brand-identity-achs-maui.md` si se mantiene como
   restricción permanente del proyecto.

## Restricción adicional aplicada al csproj

Para permitir el build local en Fedora 43 sin toolchain de macOS, se agregó
un `TargetFrameworks` condicional que excluye `net10.0-ios` en Linux. iOS
sigue siendo destino oficial del proyecto (Propuesta 1 §1) — esta exclusión
solo aplica a builds locales en Linux, no al pipeline de CI/CD que se
ejecutará en una máquina con soporte iOS.

## Archivos tocados

- `OrquestadorFrontend.csproj` (downgrade del package + condicional Linux).
- `documentacion/decisiones/maui-controls-pin-2026-06.md` (este documento).
