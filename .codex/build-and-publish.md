# Compilación y publicación

## Entorno objetivo

- Windows 10 x64 o Windows 11 x64.
- Desarrollo: .NET SDK 8 con Windows Desktop.
- Ejecución final: no requiere SDK ni runtime instalado.

La solución se mantiene sin paquetes NuGet de terceros. El ejecutor de pruebas
forma parte del repositorio.

## Regenerar el icono

Si cambia `src\Todos.App\Assets\icon.png`, regenera el `.ico` multirresolución
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

El resultado esperado para la versión 1.1.4 es 41 pruebas aprobadas, 0 fallidas,
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
publish\Todos-1.1.4-win-x64.zip
publish\Todos-1.1.4-win-x64.sha256
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
Get-FileHash .\Todos-1.1.4-win-x64.zip -Algorithm SHA256
```

Compara el resultado con `Todos-1.1.4-win-x64.sha256`.

## Actualización

1. Sal completamente de Todo's.
2. Conserva opcionalmente una copia de `%LOCALAPPDATA%\Todos`.
3. Reemplaza `Todos.exe`.
4. Ejecuta la nueva versión.

La versión 1.1.0 migra automáticamente del esquema 1 al 2, conservando las
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
