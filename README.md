# Todo's

Todo's es una aplicación portable para Windows 10/11 x64 que mantiene pequeñas
listas de tareas sobre las demás ventanas. No requiere cuenta, conexión a
Internet, servidor, instalador ni una instalación separada de .NET.

El repositorio del proyecto se llama **`todos-desktop`**.

## Desarrollo con IA

Este proyecto fue desarrollado con inteligencia artificial, mediante OpenAI
Codex, a partir de requisitos y revisiones de su autor. La IA se utilizó para
implementar código, realizar ajustes y preparar pruebas y documentación.

El uso de IA corresponde al proceso de desarrollo: la aplicación funciona
localmente y no necesita servicios de IA para utilizarse.

## Descargar y usar

1. Descarga el paquete ZIP desde la sección **Releases** del repositorio, si hay
   una versión publicada. Si todavía no hay paquetes disponibles, puedes
   generarlo siguiendo la sección [Desarrollo](#desarrollo).
2. Extrae `Todos.exe` en una carpeta donde quieras conservar el programa.
3. Ejecuta `Todos.exe` como usuario normal.
4. Windows puede colocar su icono dentro del chevrón del área de notificación.

En la primera ejecución, Todo's queda configurado para iniciar con Windows. La
opción puede desactivarse desde el menú del botón flotante y no requiere
permisos de administrador.

## Funciones principales

- Botón flotante movible, siempre visible y por encima de las ventanas TODO.
- Múltiples listas independientes, redimensionables y siempre encima.
- Siete temas pastel.
- Agrupaciones de tareas por lista, con General predeterminado y traslado entre grupos.
- Tareas editables, reordenables, completables, reabribles y eliminables con
  deshacer.
- Ocultar una lista con `Alt+F4` sin borrarla.
- Recuperar o eliminar listas ocultas desde el submenú del botón central.
- Icono de Todo's en el área de notificación con acceso y salida segura.
- Guardado local automático, respaldo y recuperación ante archivos dañados.
- Instancia única: abrir el programa de nuevo recupera la instancia existente.

Consulta el [manual de usuario](.codex/manual-usuario.md) para todos los gestos y
atajos.

## Datos y privacidad

Todo's trabaja completamente sin conexión. Guarda el estado en:

```text
%LOCALAPPDATA%\Todos\state.json
```

En esa misma carpeta conserva `state.backup.json` y un log operativo rotativo.
El log no incluye el texto de las tareas. Para respaldar tus datos, sal de
Todo's y copia la carpeta `%LOCALAPPDATA%\Todos`.

## Desarrollo

Requisitos: Windows x64 y .NET 8 SDK con Windows Desktop.

```powershell
dotnet restore Todos.sln
dotnet build Todos.sln --configuration Release --no-restore
dotnet run --project tests\Todos.Tests\Todos.Tests.csproj --configuration Release --no-build
```

Para ejecutar desde el código:

```powershell
dotnet run --project src\Todos.App\Todos.App.csproj --configuration Release
```

Para generar el paquete portable:

```powershell
.\scripts\publish-win-x64.ps1
```

El script genera el ejecutable y el ZIP en `publish/`; estos archivos no se
incluyen en el código fuente del repositorio.

Si se reemplaza `src\Todos.App\Assets\icon.png`, primero debe regenerarse el
icono multirresolución con `.\scripts\build-icon.ps1`.

La guía detallada está en
[compilación y publicación](.codex/build-and-publish.md).

## Estructura

- `src/Todos.App`: aplicación WPF, dominio, persistencia e integración Windows.
- `tests/Todos.Tests`: pruebas unitarias e integrales sin paquetes externos.
- `.codex`: contexto, diseño, plan, progreso y documentación del producto.
- `publish`: artefactos finales ignorados por control de versiones.

Versión actual: **1.1.4**. La actualización migra los datos anteriores al esquema 2;
las tareas existentes se conservan en General. El formato nuevo requiere 1.1.0
o posterior; para regresar a 1.0.x se necesita un respaldo previo a la actualización.
