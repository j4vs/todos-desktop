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
opción puede desactivarse desde clic derecho → Configuración en el botón flotante y no requiere
permisos de administrador.

## Funciones principales

- Botón flotante circular vectorial, movible y siempre encima, con acceso rápido a la última lista minimizada.
- Minimizar el botón flotante y mostrarlo u ocultarlo desde el área de notificación sin cerrar la aplicación.
- Apariencia configurable: letra con −/+, botón principal Chico/Mediano/Grande y espaciado general Chico/Mediano/Grande, con aplicación inmediata y persistencia.
- Múltiples listas independientes, redimensionables y siempre encima.
- Siete temas pastel.
- Agrupaciones de tareas por lista, con General predeterminado, renombrado, reordenamiento y traslado entre grupos.
- Vaciar tareas completadas por lista con deshacer de lote.
- Subtareas de un nivel con avance junto al checkbox, plegado, captura en línea, edición y reordenamiento.
- Una nota Markdown por tarea o subtarea, con editor y visualizador en pestañas, guardado automático e importación/exportación `.md`.
- Menús de tareas junto al cursor, fila resaltada y acciones explícitas para editar, completar/reabrir, eliminar y mover entre grupos.
- Tareas editables, reordenables, completables, reabribles y eliminables con
  deshacer.
- Ocultar una lista con `Alt+F4` sin borrarla.
- Recuperar listas ocultas con hover de un segundo o clic sin asociación.
- Etiquetas laterales, título principal solo en hover, checks SVG y scroll delgado.
- Clic derecho: botones redondos para salida, configuración, papelera y nueva lista.
- Papelera persistente para restaurar listas completas o eliminarlas definitivamente.
- Clic derecho en una lista oculta para moverla directamente a papelera.
- Minimización automática opcional al hacer clic fuera de una lista.
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

El ícono circular se define en `src\Todos.App\Assets\icon.svg`.
`.\scripts\build-icon.ps1` regenera el PNG y los nueve tamaños del ICO directamente
desde ese vector; el ejecutable y el área de notificación comparten ese ícono.

La guía detallada está en
[compilación y publicación](.codex/build-and-publish.md).

## Estructura

- `src/Todos.App`: aplicación WPF, dominio, persistencia e integración Windows.
- `tests/Todos.Tests`: pruebas unitarias e integrales sin paquetes externos.
- `.codex`: contexto, diseño, plan, progreso y documentación del producto.
- `publish`: artefactos finales ignorados por control de versiones.

Versión actual: **1.8.2**. La actualización migra los datos anteriores al esquema 5,
conservando listas, tareas y preferencias. Versiones 1.7.x y anteriores no pueden
leer este formato; para regresar a ellas se necesita un respaldo previo.
