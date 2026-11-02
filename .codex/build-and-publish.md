# Compilación y publicación

Paquete actual: `publish/Todos-1.8.2-win-x64.zip`, ejecutable en `publish/win-x64-1.8.2/Todos.exe` y sumas en `publish/Todos-1.8.2-win-x64.sha256`. Detección física de clics y recuperación al reanudar/cambiar pantallas; esquema 5 sin cambios. Markdig 1.3.2 se integra al ejecutable y su licencia se incluye en THIRD-PARTY-NOTICES.md. NuGet.Config usa el origen oficial para restaurar la dependencia.

Validación 1.8.2: compilación limpia, 65/65 pruebas, datos aislados y revisión en segundo monitor. Resize nativo previo al layout reprodujo fallo de clasificación antiguo y pasó con la detección nueva. DPI/reanudación simulados, clics pendientes inválidos descartados y menús/auto-minimización conservados. Evidencia `artifacts/outside-click-1.8.2-tests.txt` y `artifacts/outside-click-1.8.2-review/click-recovery.txt`. Portable extraído 1.8.2.0, hashes e inicio/reinicio verificados; evidencia `artifacts/outside-click-1.8.2-package`. Hibernación física pendiente del usuario.

Validación 1.8.1: compilación sin errores, 65/65 pruebas; renders en `artifacts/subtask-drag-1.8.1-review`, ventanas en segundo monitor. NU1900 de auditoría de NuGet por origen temporalmente no disponible; dependencias sin cambios.

Portable 1.8.1.0 extraído y hashes correctos; dos inicios nativos preservaron notas/árboles y preferencias en datos aislados. Evidencia `artifacts/subtask-drag-1.8.1-package/verification.txt`.

Validación 1.8.0: build limpio, 65/65 pruebas. Renders revisados en `artifacts/notes-1.8.0-review`, con ventanas en segundo monitor y posición comprobada. Portable extraído 1.8.0.0 y hashes correctos; cuatro inicios nativos verificaron migración/reinicio conservando notas y árboles en datos aislados. Evidencia en `artifacts/notes-1.8.0-tests.txt` y `artifacts/notes-1.8.0-package/verification.txt`.

Validación 1.7.1: build sin errores ni advertencias, 60/60 pruebas; ZIP extraído, versión 1.7.1.0 y hashes comprobados. Dos ejecuciones nativas con datos aislados conservaron árboles y preferencias. Evidencia en `artifacts/context-1.7.1-tests.txt` y `artifacts/context-1.7.1-package/verification.txt`.

El modo `--smoke-test` con `--data-root` explícito fuerza guardado antes de salir, también cuando ya hay listas, para comprobar migración y reinicio. Usa exclusivamente una carpeta de datos de prueba; el inicio normal no fuerza escrituras adicionales.

## Entorno objetivo

- Windows 10 x64 o Windows 11 x64.
- Desarrollo: .NET SDK 8 con Windows Desktop.
- Ejecución final: no requiere SDK ni runtime instalado.

La solución se mantiene sin paquetes NuGet de terceros. El ejecutor de pruebas
forma parte del repositorio.

## Regenerar el icono

Si cambia `src\Todos.App\Assets\icon.svg`, regenera el PNG y el `.ico` multirresolución
antes de compilar o publicar:

```powershell
.\scripts\build-icon.ps1
```

El script genera `src\Todos.App\Assets\icon.ico` con nueve tamaños, desde 16
hasta 256 px. No hace falta ejecutarlo cuando los recursos de icono no cambiaron.

## Validar el código

Desde la raíz del proyecto:

```powershell
dotnet restore Todos.sln
dotnet build Todos.sln --configuration Release --no-restore
dotnet run --project tests\Todos.Tests\Todos.Tests.csproj --configuration Release --no-build
```

El resultado esperado para la versión 1.3.1 es 49 pruebas aprobadas, 0 fallidas,
0 advertencias y 0 errores.

## Publicar Windows x64

Ejecuta:

```powershell
.\scripts\publish-win-x64.ps1
```

El script restaura los runtime packs x64, usa el perfil
`Properties/PublishProfiles/win-x64.pubxml`, genera el EXE, crea el ZIP y calcula
los SHA-256.

Salidas:

```text
publish\win-x64\Todos.exe
publish\Todos-1.3.1-win-x64.zip
publish\Todos-1.3.1-win-x64.sha256
```

## Comprobar manualmente el paquete

1. Copia el ZIP a un Windows 10/11 x64 de prueba.
2. Extrae el archivo en una carpeta normal del usuario.
3. Confirma que existen `Todos.exe` y los avisos de licencia de terceros.
4. Ejecútalo sin permisos de administrador y sin instalar .NET.
5. Crea una lista y una tarea; oculta la lista y sal desde el icono de
   notificación.
6. Abre de nuevo y comprueba la restauración.
7. Activa y desactiva el inicio automático y valida el siguiente inicio de
   sesión.
8. Mueve el EXE, ábrelo y comprueba que la configuración se repara.
9. Prueba Narrador y escalas 100 %, 125 %, 150 % y 200 %.

## Integridad

En PowerShell:

```powershell
Get-FileHash .\Todos.exe -Algorithm SHA256
Get-FileHash .\Todos-1.3.1-win-x64.zip -Algorithm SHA256
```

Compara el resultado con `Todos-1.3.1-win-x64.sha256`.

## Actualización

1. Sal completamente de Todo's.
2. Conserva opcionalmente una copia de `%LOCALAPPDATA%\Todos`.
3. Reemplaza `Todos.exe`.
4. Ejecuta la nueva versión.

La versión 1.3.1 migra automáticamente al esquema 3 sin perder listas ni tareas.
Volver a 1.2.x o anteriores requiere un respaldo previo; no leen esquema 3.
Históricamente, la versión 1.1.0 migraba automáticamente del esquema 1 al 2, conservando las
tareas anteriores en General. Para volver a 1.0.x se necesita un respaldo previo;
esas versiones rechazan el esquema 2 sin sobrescribirlo.

## Validación de agrupaciones y presentación

La suite incluye migración, round-trip de grupos, movimiento entre grupos,
completado/reapertura, eliminación de grupo sin pérdida, deshacer e instantáneas.
Una prueba STA crea la vista WPF real y verifica cursor, marcas antes/después,
eventos de arrastre ascendente/descendente, traslado entre grupos y grupo vacío.
Para guardar renders de esa prueba, define `TODOS_TEST_RENDER_DIR` con una carpeta
de salida antes de ejecutar `Todos.Tests.exe` (anchos 560, 410 y 280, y tarjeta).
El script de publicación obtiene la versión directamente del proyecto.

La suite WPF también comprueba captura de una fila, cambio de tema del encabezado,
plegado, elección y creación desde menú lateral, traslado desde el menú y
cancelación/confirmación al quitar un grupo. La persistencia comprueba plegado
después de guardar y reiniciar. Renders: `artifacts/groups-refinement-review`.

La regresión 1.1.4 verifica DragOver repetido y Drop sobre el margen entre filas,
el color de la marca y del preview, sus dimensiones y contenido del bitmap,
el destino plegado y las exclusiones de botones/edición en el encabezado.
Evidencia: `artifacts/drag-refinement-review` y `artifacts/drag-refinement-tests.txt`.
La validación usa eventos y renders WPF; no sustituye un recorrido físico con ratón.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.

## Papelera 1.3.1

49 pruebas: listas completas conservadas tras reinicio, snapshots independientes,
restauración, migración de esquema 2 a 3, eliminación definitiva selectiva con
confirmación/cancelación, idempotencia y eventos de menú/papelera WPF reales.
Renders: artifacts/recycle-bin-1.3.1-review. Revisar también físicamente los clics.

El modo --render-recycle-bin-preview acepta una ruta PNG y debe usarse con
--data-root aislado para revisar la Papelera del EXE portable. Al igual que los
otros modos de render, usa una instancia separada y omite inicio con Windows.

## Nueva interfaz

La suite añade acceso rápido con asociación visible/oculta, eliminación sin
fallback, prioridad de acciones, snapshot y reinicio. La prueba WPF revisa
círculo vectorial, ocultas, clic derecho, configuración, cierre animado y scroll
con 100 listas. También comprueba labels laterales, título solo en hover, un
segundo de espera, check.svg, color Background, configuración centrada y sin
acciones abiertas, scrollbar de 10 px y clic sin reapertura accidental.
También verifica check blanco más pequeño, color de acento oscurecido y checkbox-checked.svg
para configuración. Renders actuales: artifacts/recycle-bin-1.3.1-review.
No sustituye la revisión física de hover, arrastre y clic fuera con otras apps.

Para publicar sin reemplazar un ejecutable en uso:

```powershell
.\scripts\publish-win-x64.ps1 -OutputDirectory publish\win-x64-1.3.1
```

El EXE queda en esa carpeta; el ZIP y SHA-256 mantienen sus rutas bajo publish.
En un entorno restringido, TEMP/TMP pueden apuntar a artifacts/test-temp para
ejecutar las pruebas de reemplazo íntegramente dentro del workspace.
