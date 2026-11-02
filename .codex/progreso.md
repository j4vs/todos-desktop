# Progreso de implementación — Todo's

Este documento registra el cierre verificable de cada etapa. Una etapa solo se
marca como completada después de implementar o documentar sus entregables y
ejecutar sus validaciones.

## Versión actual — 1.8.2 (2026-10-14)

Reporte de minimización al pulsar dentro y cierre inmediato del menú principal, posiblemente tras tiempo/hibernación. Corrección de detección por HWND/bounds físicos, renovación del hook en Resume/DisplaySettingsChanged e invalidación por generación de acciones anteriores a suspensión, renovación o cierre. Conserva auto-minimización, snapshot del clic principal, popups/espacios transparentes y esquema 5.

Build limpio: 0 advertencias y 0 errores después de refrescar restauración. Suite 65/65. Pruebas WPF en segundo monitor: contextos DPI Unaware/System/PerMonitor con restauración del contexto original, clics en listas/popups/notas, clic externo, menú recién abierto, recuperación simulada, descarte de acciones antiguas e interrupción durante preparación/disposición. Se reprodujo la clasificación incorrecta original cuando un resize nativo precede el layout WPF: geometría antigua False y detección nueva True para un punto interno. Evidencia artifacts/outside-click-1.8.2-review/click-recovery.txt y artifacts/outside-click-1.8.2-tests.txt. No se hibernó la computadora; el disparador exacto del reporte y la recuperación tras hibernación real siguen pendientes de revisión del usuario.

Portable publish/Todos-1.8.2-win-x64.zip, versión extraída 1.8.2.0 y hashes correctos. Inicio/reinicio nativos con datos aislados. Evidencia artifacts/outside-click-1.8.2-package. Sin alterar la instancia ni los datos del usuario.

## Versión anterior — 1.8.1

Portable extraído 1.8.1.0 y hashes EXE/ZIP verificados. Dos inicios nativos aislados con salida 0 conservaron esquema 5, preferencias, asociación, grupos, IDs, árboles, orden, progreso, plegado y notas exactas. Evidencia artifacts/subtask-drag-1.8.1-package/verification.txt. Paquete publish/Todos-1.8.1-win-x64.zip.

Arrastre de subtareas comparte preview y atenuación con tareas; cálculo continuo en SubtaskDropSurface, indicador independiente recto sin cambiar altura/borde, márgenes de inserción, scroll y animación. Limpieza común al soltar/cancelar con restauración de opacidad local/estilo. Se conserva esquema 5 y notas.

Compilación sin errores; 65/65 pruebas. Regresión WPF ampliada: eventos repetidos DragOver/DragLeave internos sin apagar indicador, altura/borde estables, preview con píxeles reales/tamaño correcto, origen al 30 %, restauración de estilo y limpieza, inserción en margen y reordenamiento. Renders en artifacts/subtask-drag-1.8.1-review; informe artifacts/subtask-drag-1.8.1-tests.txt. Ventanas en segundo monitor. Compilación/publicación produjo NU1900 por consulta de auditoría de NuGet no disponible; dependencia Markdig sin cambios. Comprobación física del arrastre queda pendiente del usuario.

## Versión anterior — 1.8.0 (2026-10-12)

Una nota Markdown por tarea/subtarea, ventana de edición/visualización en pestañas y acceso contextual/ícono. Guardado automático con confirmación de escritura real; importación/exportación .md con confirmación de reemplazo. Render nativo, sin imágenes ni HTML ejecutable; tema/fuentes en vivo. Notas preservadas en grupos, completado, eliminación/deshacer y Papelera. La nota y sus diálogos pertenecen a la lista para auto-minimización; ocultar la lista cierra sus notas conservando cambios. Esquema 5 migra 1–4.

Build sin advertencias ni errores; 65/65 pruebas. Cobertura: migración, notas nulas/dañadas, Unicode/espacios, snapshot profundo, persistencia/reinicio, grupos/Papelera/deshacer, importación cancelada/confirmada, exportación, fallo de escritura y aviso de guardado obsoleto. WPF: pestañas, foco, formato/casillas/tablas/código, accesos, nota única, tema/letra, cierre al borrar/ocultar y protección de minimización automática. Ventanas de notas comprobadas en segundo monitor. Renders revisados en artifacts/notes-1.8.0-review; informe artifacts/notes-1.8.0-tests.txt. No certifica interacción física de ratón ni escalas mixtas.

Portable 1.8.0.0 extraído y hashes EXE/ZIP correctos. Cuatro ejecuciones nativas aisladas con salida 0 comprobaron migración 4 a 5 y reinicio con notas, conservando preferencias, asociación, listas, grupos, IDs, orden, progreso, plegado y Markdown exacto de principales/hijos. Evidencia artifacts/notes-1.8.0-package/verification.txt. Paquete publish/Todos-1.8.0-win-x64.zip, incluyendo avisos/licencia de Markdig. Sin modificar datos ni detener la instancia del usuario.

## Versión anterior — 1.7.2 (2026-10-12)

Portable generado con las correcciones de resaltado exclusivo y menú sin nombre de tarea/subtarea. Se mantiene esquema 4. Solo compilación y empaquetado por solicitud del usuario, sin ejecutar pruebas ni abrir el programa; revisión visual pendiente. Paquete publish/Todos-1.7.2-win-x64.zip y sumas publish/Todos-1.7.2-win-x64.sha256.

## Versión anterior — 1.7.1 (2026-10-12)

Corrección posterior en código: al abrir un menú se limpia explícitamente el resaltado de todas las tareas y subtareas de la lista antes de seleccionar la nueva; el cierre tardío de un popup anterior no borra la selección del menú recién abierto. Retirado el nombre de la tarea del menú. Por solicitud del usuario, solo compilación Release: 0 errores y 0 advertencias, sin ejecutar pruebas ni abrir ventanas. Verificación visual a cargo del usuario. El ZIP 1.7.1 mencionado abajo corresponde al cierre anterior y no se regeneró en esta corrección.

Menú contextual junto al cursor y resaltado transitorio de tarea/subtarea; título y acciones claras Editar, Completar/Reabrir, Eliminar. Principales: Agregar subtarea y «Mover a [grupo]». Conversión retirada de la interfaz. Flecha/avance junto al checkbox en pendientes y completadas. Etiqueta del principal oculta al comenzar arrastre. Se mantiene esquema 4 y datos existentes.

Build limpio, 60/60 pruebas. Eventos WPF comprobaron posición junto al punto de clic, resaltado y limpieza al cerrar, edición/foco de principal, completar/deshacer, eliminar/deshacer, destinos explícitos, avance en línea y ocultación de etiqueta al iniciar arrastre. Las ventanas de revisión se colocaron en el monitor secundario y se verificó su monitor real al cargar. Renders revisados en artifacts/context-1.7.1-review; informe artifacts/context-1.7.1-tests.txt. Interacción física de ratón y escalas mixtas sigue pendiente.

ZIP extraído 1.7.1.0 y hashes EXE/ZIP correctos. Dos ejecuciones nativas aisladas con salida 0 conservaron esquema 4, preferencias, asociación, IDs, grupos y árboles de tareas con orden, progreso y plegado. Evidencia artifacts/context-1.7.1-package/verification.txt. Paquete publish/Todos-1.7.1-win-x64.zip. Sin alterar datos ni detener la instancia del usuario.

## Versión anterior — 1.7.0 (2026-10-12)

Subtareas de un nivel: contador/plegado, captura consecutiva con Enter, edición, arrastre/teclado dentro de principal y conversión desde tarea sin pasos del mismo grupo. Completar todos los pasos completa la principal; completar principal cascada con Deshacer de estado. Reapertura conserva coherencia. Borrado/deshacer, limpieza de completadas, grupos y papelera preservan el árbol y su contexto. Esquema 4 migra 1–3 y evita pérdida al abrir con versiones antiguas.

Build sin advertencias ni errores; 60/60 pruebas. Se probaron ciclos/conversión inválida, progreso mixto/undo sin perder edición, preservación de raíces y pasos, retorno a General al quitar grupos, snapshot/reinicio/plegado/Papelera y migración. Eventos WPF verifican menú, captura/foco/Enter, drag/drop de pasos y conversión. Renders revisados en artifacts/subtasks-1.7.0-review; evidencia artifacts/subtasks-1.7.0-tests.txt. Prueba física de ratón y escalas mixtas pendiente.

Portable 1.7.0.0 extraído del ZIP; SHA-256 de EXE y ZIP verificados. Ejecuciones nativas con guardado real en datos aislados comprobaron migración 3→4 conservando tres tareas y dos reinicios conservando IDs, orden, progreso 1/3 y plegado de subtareas. Evidencia artifacts/subtasks-1.7.0-package. Paquete publish/Todos-1.7.0-win-x64.zip. La revisión visual corresponde a renders WPF, no a interacción física de ratón.

## Versión anterior — 1.6.0 (2026-10-12)

Configuración de letra con −/+ (80–140 %), tamaño principal 40/52/68 y espaciado .75/1/1.3. Recursos dinámicos globales actualizan fuentes, márgenes, padding y alturas en listas, menús, etiquetas y papelera; fuente del menú de notificación actualizada. Preferencias normalizadas y copiadas en snapshot, esquema 3 compatible. Propiedad de tamaño separada evita que el tamaño nativo de WPF rompa la actualización del principal.

Build sin errores ni advertencias; 54/54 pruebas. Persistencia/reinicio, valores por defecto, límites, invalidación de recursos, tamaños 40/52/68, vuelta a valores actuales y extremos de fuente/espaciado verificados. Renders revisados en artifacts/appearance-1.6.0-review y evidencia artifacts/appearance-1.6.0-tests.txt. Interacción física, dos monitores y escalas mixtas siguen pendientes.

Portable 1.6.0.0 extraído del ZIP, SHA-256 de EXE y ZIP verificados. Arranque y reinicio con preferencias grandes guardadas aprobados usando datos aislados. La captura nativa de ventana en modo render volvió a salir transparente; la apariencia se revisó con los renders WPF de la suite, no con esa captura. Paquete publish/Todos-1.6.0-win-x64.zip.

## Versión anterior — 1.5.0 (2026-10-10)

Minimizar entre Salir y Configuración oculta principal y menús sin terminar el proceso ni cerrar listas. Mostrar botón / Ocultar botón sustituye Mostrar Todo's en notificación y se sincroniza con la visibilidad real. Doble clic recupera el principal. Visibilidad de sesión, esquema 3 sin cambios.

Build sin advertencias ni errores; 52/52 pruebas. Se verificaron quinta acción y orden, cierre de popups/hover, recuperación, asociación y menú de notificación dinámico con creación y salida conservadas. Evidencia artifacts/minimize-1.5.0-tests.txt y renders revisados en artifacts/minimize-1.5.0-review. Interacción física del área de notificación pendiente.

Portable 1.5.0.0 extraído del ZIP, hashes de EXE y ZIP verificados. Arranque de prueba aprobado con datos aislados. La captura del menú en modo render del portable salió incompleta/transparente y no certifica su apariencia; el render WPF de la suite sí muestra las cinco acciones completas (artifacts/minimize-1.5.0-review/launcher-actions.png). Paquete publish/Todos-1.5.0-win-x64.zip.

## Versión anterior — 1.4.0 (2026-10-10)

Menú de lista oculta anclado al círculo. Papelera de una fila por lista, flecha restaurar alineada. Grupos personalizados con lápiz y flechas para cambiar nombre y orden; General fijo. Vaciar completadas con deshacer del lote. Liberación idempotente del temporizador de deshacer al cerrar.

Build sin errores ni advertencias y 51/51 pruebas aprobadas, incluyendo reordenamiento/renombrado después de reiniciar y limpieza/deshacer conservando agrupación, fechas e identidad. Eventos WPF verifican acciones de grupos y anclaje del menú. Renders revisados en artifacts/groups-1.4.0-review; evidencia artifacts/groups-1.4.0-tests.txt. Revisión física de ratón y multimonitor pendiente.

Portable 1.4.0.0 extraído del ZIP; SHA-256 de EXE y ZIP verificados. Arranque de prueba y render nativo de papelera aprobados con datos aislados, sin modificar datos del usuario ni inicio con Windows. Evidencia artifacts/groups-1.4.0-package/papelera.png. Paquete publish/Todos-1.4.0-win-x64.zip.

## Versión anterior — 1.3.1 (2026-10-10)

Ícono circular morado con borde y check blancos. SVG, PNG e ICO actualizados;
ICO contiene nueve tamaños dibujados directamente del vector, todos con esquinas
transparentes verificadas. EXE y notificación comparten el recurso del ejecutable.
Evidencia de suite: artifacts/icon-1.3.1-tests.txt.
Build limpio y 49/49 pruebas aprobadas. ZIP 1.3.1 extraído con versión 1.3.1.0,
SHA-256 de ZIP/EXE correctos. Windows extrajo el ícono circular de 32 px del EXE,
con esquinas transparentes, mediante la misma API usada para notificación.
Preview: artifacts/icon-1.3.1-package/icon-extracted.png. Paquete:
publish/Todos-1.3.1-win-x64.zip. La caché visual de Explorer no se modificó.

## Versión anterior — 1.3.0 (2026-10-10)

Papelera persistente de listas completas. Borrado directo desde ventana o menú
contextual de lista oculta. Cuarta acción entre Configuración y Nueva lista.
Restauración con identidad, grupos, colores y tareas intactos; eliminación
definitiva con confirmación propia. Schema 3 migra datos previos.
Build limpio; 49/49 pruebas. Renders en artifacts/recycle-bin-1.3.0-review y
evidencia de suite en artifacts/recycle-bin-1.3.0-tests.txt. Revisión física pendiente.
Portable 1.3.0.0 extraído del ZIP y SHA-256 de ZIP/EXE verificados. Render nativo
de Papelera y smoke con papelera/estado anterior terminaron con código 0 en
artifacts/recycle-bin-1.3.0-package. Conservó una lista con tres tareas y un grupo;
la carga de esquema 2 se guardó como 3. Render no modificó su fixture y los modos
aislados no usan listas reales ni modifican el inicio con Windows.
Paquete: publish/Todos-1.3.0-win-x64.zip; EXE: publish/win-x64-1.3.0/Todos.exe.

## Versión anterior — 1.2.3 (2026-10-10)

Botones flotantes con Accent un 10 % más oscuro y checks blancos más pequeños:
principal 27 × 22, ocultas 20 × 17. Bordes de 2 y 1,5 px. Ventanas conservadas.
Evidencia: artifacts/launcher-1.2.3-review y artifacts/launcher-1.2.3-tests.txt.
Build Release sin errores ni advertencias; 45/45 pruebas aprobadas.
Portable 1.2.3.0 extraído del ZIP: SHA-256 de ZIP/EXE correctos; render de siete
colores y smoke aislado terminaron con código 0, fixture intacto. Renders en
artifacts/launcher-1.2.3-package, paquete en publish/Todos-1.2.3-win-x64.zip.

## Versión anterior — 1.2.2 (2026-10-10)

Checks negros en principal y ocultas; RGB del fondo pastel reducido un 10 % solo
en esos botones, conservando ventanas y acciones. Configuración activada usa
checkbox-checked.svg mediante trigger específico; tareas mantienen check.svg.
Renders y suite: artifacts/launcher-1.2.2-review y artifacts/launcher-1.2.2-tests.txt.
Build Release sin errores ni advertencias, 45/45 pruebas aprobadas. Renders
revisados: checks negros y casilla marcada de configuración centrada.
Portable publish/win-x64-1.2.2/Todos.exe: render y smoke aislados terminaron
con código 0, fixture persistido intacto. ZIP publish/Todos-1.2.2-win-x64.zip
extraído con FileVersion 1.2.2.0 y hashes de ZIP/EXE coincidentes con el SHA-256.

## Versión anterior — 1.2.1 (2026-10-10)

Ajustes autorizados: etiquetas laterales de 13 px con cambio de lado según
espacio, título principal solo en hover, despliegue tras un segundo, configuración
centrada que oculta acciones, scroll de 10 px con thumb redondeado de 6 px,
color idéntico al Background de la lista y check.svg en principal/ocultas/tareas.

Se captura la intención de clic antes de la minimización externa: si la lista
asociada estaba visible con auto-minimización, el clic solo la oculta. Se conserva
el acceso rápido para el siguiente clic. Nueva regresión comprueba este orden.

Build Release: 0 errores y 0 advertencias. Suite: 45/45 aprobadas, incluida
comparación de color, alineación de configuración, título en hover, cambio de
lado de etiquetas y desplazamiento funcional con 100 listas ocultas.
Evidencia local: artifacts/launcher-1.2.1-tests.txt y renders en
artifacts/launcher-1.2.1-review. No sustituye prueba física de ratón/multimonitor.
Paquete actualizado: publish/Todos-1.2.1-win-x64.zip y publish/win-x64-1.2.1/Todos.exe.
ZIP extraído verificado: FileVersion 1.2.1.0, SHA-256 de ZIP/EXE coincidentes.
Render y smoke del EXE portable terminaron con código 0 en carpetas aisladas de
artifacts/launcher-1.2.1-package. La asociación y fixture persistido se conservaron
sin cambios; inicio vacío guardó esquema 2 y auto-minimización false. No se usaron
datos reales ni se cambió el registro de inicio Windows.

## Versión anterior — 1.2.0 (2026-10-10)

Alcance aprobado por el usuario y completado con sus dos correcciones: eliminar
la lista asociada devuelve el botón al morado sin seleccionar otra, y el clic
izquierdo sin asociación abre las ocultas igual que el hover.

### Implementación

- Botón circular de Ellipse/Path, borde/check blancos y morado original #9285D8.
- Acceso directo a la última lista minimizada, título/color actualizados y
  asociación persistida al reiniciar; referencia inexistente se normaliza a null.
- Hover continuo de dos segundos para ocultas; clic derecho reemplaza por Salir,
  Configuración y Nueva lista. Las acciones prevalecen sobre hover.
- Círculos secundarios de 42 × 42 y etiquetas, transición de 150 ms, cierre
  animado con clic fuera/Escape, colocación adaptativa y scroll con muchas listas.
- Configuración conserva el estilo anterior para inicio Windows y minimización
  automática, apagada inicialmente. Preferencia y asociación incluidas en snapshots.
- Observación de clics externos sin consumir entrada ni polling. Respeto de popups
  propios y confirmaciones. Captura previa evita ocultar una lista recién recuperada.
- Se mantiene esquema 2. SDK 8.0.422 fijado en global.json y script de publicación
  admite OutputDirectory para generar una versión sin reemplazar un EXE en uso.

### Validación local completada

- Build Release: 0 errores, 0 advertencias; suite: 44/44 aprobadas.
- Persistencia/snapshot/reinicio, compatibilidad de campos ausentes, eliminación
  sin fallback, prioridad de acciones y recuperación de una asociación ya visible.
- WPF real: círculo sin bitmap, clic derecho, dos opciones de configuración,
  cierre después de animación, popups internos y scroll con 30 listas.
- Controlador real con ventanas WPF: clic interno conserva la lista; preferencia
  desactivada no oculta; clic externo oculta y actualiza asociación; una lista
  recuperada después de capturar el clic permanece visible.
- Renders revisados: artifacts/launcher-1.2.0-review y prueba guardada en
  artifacts/launcher-1.2.0-tests.txt. El render vectorial usa dimensiones/DPI reales.
- Paquete publish/Todos-1.2.0-win-x64.zip, SHA-256 y EXE
  publish/win-x64-1.2.0/Todos.exe generados. ZIP extraído: versión 1.2.0.0,
  hashes verificados, smoke exit 0 y JSON nuevo de esquema 2 con auto-minimizar false.
- Render del EXE extraído con dos listas ocultas, grupos, asociación y preferencia
  activa: exit 0, recursos correctos y fixture sin modificaciones. Evidencia en
  artifacts/launcher-1.2.0-package. Modos aislados no modifican inicio Windows real.
- La ejecución autocontenida requirió validación fuera del sandbox; se usaron
  exclusivamente carpetas de prueba del workspace y una identidad de instancia propia.

### Revisión física pendiente

Las pruebas usan eventos, coordenadas y renders WPF. Sigue pendiente el recorrido
con ratón de hover prolongado, arrastre, clic fuera frente a otras aplicaciones
y monitores físicos con DPI mixto; no se presenta el build como prueba de esos gestos.

## Resumen histórico del MVP 1.0.0

| Etapa | Estado | Evidencia principal |
|---|---|---|
| 0. Definición funcional y visual | Completada | Contexto, plan y diseño consolidados |
| 1. Preparación de la solución | Completada | Build Release limpio y smoke test aprobado |
| 2. Dominio y persistencia | Completada | 11 pruebas de dominio/archivos aprobadas |
| 3. Botón central e icono | Completada | 16 pruebas y smoke test WPF aprobados |
| 4. Ventanas TODO | Completada | 19 pruebas y persistencia de ventana comprobada |
| 5. Tareas y progreso | Completada | 21 pruebas y smoke con fixture de tareas |
| 6. Sistema visual y animaciones | Completada | Render WPF real revisado y build limpio |
| 7. Ciclo de vida y recuperación | Completada | 26 pruebas y smoke test aprobados |
| 8. Accesibilidad | Completada | Contraste WCAG, teclado y render real validados |
| 9. Pruebas integrales | Completada | 30 pruebas, carga y proceso WPF aprobados |
| 10. Publicación portable | Completada | EXE único x64 y ZIP ejecutados correctamente |
| 11. Documentación y cierre | Completada | Manual, build, decisiones y entrega final |

## Etapa 0 — Definición funcional y visual

### Decisiones cerradas

- Nombre visible `Todo's` e identificador técnico `Todos`.
- Windows 10/11 x64, WPF, C# y .NET estable con soporte vigente.
- Ejecutable autocontenido sin instalador obligatorio; datos en el perfil local.
- Botón flotante, múltiples listas `Topmost` e icono del área de notificación.
- Inicio con Windows activado por defecto y configurable.
- Tareas editables, reordenables, completables y eliminables con deshacer.
- Cierre individual oculta; salida central o desde notificación guarda y termina.
- Paleta, tipografía, tamaños, composición y animaciones iniciales definidos en
  `/.codex/diseno.md`.

### Validación

- `/.codex/context.md` revisado.
- `/.codex/plan-trabajo.md` revisado y autorizado por el usuario.
- Requisitos que afectan arquitectura resueltos.
- Dirección visual y wireframes documentados.

### Resultado

Etapa válida. Se puede preparar la solución sin inventar flujos pendientes.

## Etapa 1 — Preparación de la solución

### Implementación

- SDK fijado en .NET 8 (`8.0.422`) con Windows Desktop Runtime `8.0.28`.
- Solución `Todos.sln` con proyectos `Todos.App` y `Todos.Tests`.
- Aplicación WPF `net8.0-windows`, ejecutable como usuario normal.
- Referencia integrada a Windows Forms únicamente para el futuro icono de
  notificación, sin paquetes externos.
- Ejecutor de pruebas local y sin NuGet, debido a que el entorno no tiene fuentes
  de paquetes configuradas.
- Configuración común, reglas de formato, exclusiones y manifest de Windows.
- Icono vectorial propio y recurso `.ico` multirresolución.

### Validación ejecutada

```text
dotnet restore Todos.sln
dotnet build Todos.sln --configuration Release --no-restore
dotnet run --project tests/Todos.Tests/Todos.Tests.csproj --configuration Release --no-build
```

Resultado:

- Compilación correcta: 0 advertencias, 0 errores.
- Pruebas: 1 aprobada, 0 fallidas.
- Nombre técnico del ensamblado verificado como `Todos`.

### Resultado

Etapa válida. La solución compila sin dependencias externas y queda preparada
para implementar dominio y persistencia.

## Etapa 2 — Dominio y persistencia

### Implementación

- Estado general, configuración, posición del botón, ventanas, temas y tareas.
- Reglas de títulos/tareas, límites, orden pendiente y orden de completadas.
- Operaciones para agregar, editar, completar, reabrir, eliminar, restaurar y
  reordenar tareas.
- Normalización de archivos anteriores o incompletos y rechazo seguro de
  versiones futuras desconocidas.
- Persistencia JSON en `%LOCALAPPDATA%\Todos` con archivo temporal, reemplazo,
  respaldo y recuperación ante corrupción.
- Instantáneas profundas para que un guardado no lea estado mutable.
- Guardado serializado y con espera corta para cambios frecuentes.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 11 pruebas aprobadas, 0 fallidas.
- Cobertura comprobada: Unicode, reglas de texto, ciclo completo de tareas,
  duplicados, dimensiones inválidas, versión futura, round-trip JSON, campos
  faltantes, recuperación desde respaldo, fallo de escritura y coalescencia de
  guardados.

### Resultado

Etapa válida. Los datos y sus reglas funcionan sin depender de la interfaz.

## Etapa 3 — Botón central e icono de notificación

### Implementación

- Ventana flotante `52 × 52`, transparente, sin barra de tareas y `Topmost`.
- Clic izquierdo para abrir/cerrar menú y arrastre manual para mover.
- Menú con nueva lista, mostrar/ocultar todas, inicio con Windows y salir.
- Corrección de posición contra el área de trabajo disponible.
- Icono único en el área de notificación con mostrar, nueva lista y salida segura.
- Controlador de aplicación que carga estado, crea controles permanentes y
  coordina el guardado/cierre.
- Modo de smoke test con carpeta de datos aislada y cierre normal automático.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 16 pruebas aprobadas, 0 fallidas.
- Posiciones por defecto, bordes y coordenadas negativas comprobadas.
- Comandos, apertura de menú y ciclo de vida de `NotifyIcon` comprobados.
- Arranque y cierre real de WPF ejecutados con `--smoke-test`.
- La herramienta de inspección de escritorio no expone como ventana controlable
  el botón transparente sin barra de tareas; la revisión visual integral se
  repetirá cuando exista la ventana TODO de mayor tamaño.

### Resultado

Etapa válida. Los controles permanentes funcionan y se liberan al salir.

## Etapa 4 — Administración de ventanas TODO

### Implementación

- Creación y restauración de múltiples ventanas independientes.
- Ventanas pastel, `Topmost`, sin entrada individual en la barra de tareas.
- Título editable, cambio entre siete colores, ocultar y eliminar con confirmación.
- Movimiento por encabezado y redimensionamiento con control inferior derecho.
- Tamaño inicial/mínimo y corrección de posición antes de mostrar.
- `Alt+F4` oculta en lugar de eliminar o salir.
- Mostrar/ocultar todas y recuperación individual desde el menú central.
- Gestor dedicado que crea las ventanas solo cuando se necesitan y las cierra de
  forma controlada al salir.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 19 pruebas aprobadas, 0 fallidas.
- ViewModels, temas, límites, acciones y lista de ocultas comprobados.
- Smoke test real creó una ventana, la mostró, guardó JSON y salió normalmente.
- El JSON aislado conservó título, tema, posición, tamaño y visibilidad.

### Resultado

Etapa válida. El ciclo completo de las ventanas funciona y queda listo para
reemplazar el estado vacío por tareas reales.

## Etapa 5 — Gestión de tareas y progreso

### Implementación

- Captura rápida con `Enter` y botón.
- Validación y límite de 300 caracteres.
- Edición por doble clic o botón, confirmación con `Enter` y cancelación con
  `Esc`.
- Completar, reabrir al final de pendientes y ordenar completadas por fecha.
- Reordenamiento de pendientes desde un control de arrastre.
- Eliminación inmediata con aviso y acción “Deshacer” durante cinco segundos.
- Sección de completadas plegable y persistente.
- Contadores, estado vacío, scroll y actualización automática del resumen.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 21 pruebas aprobadas, 0 fallidas.
- Flujo unitario completo: agregar, ordenar, completar, reabrir, editar, eliminar,
  deshacer y desplegar completadas.
- Rechazo de tareas vacías y edición inválida comprobado.
- Smoke test WPF con fixture de 2 pendientes y 1 completada.
- El estado conservó las 3 tareas, su orden y la sección desplegada.

### Resultado

Etapa válida. El flujo principal del producto ya es funcional y persistente.

## Etapa 6 — Sistema visual y animaciones

### Implementación

- Diccionarios globales de tipografía, marca, texto, peligro, foco y radios.
- Estilos compartidos de foco, campos, botones, casillas y tooltips.
- Siete temas pastel completos con fondo, superficie, acento y texto.
- Ventanas redondeadas con sombra, jerarquía compacta y acciones coherentes.
- Marca e icono propios.
- Animación de entrada de 180 ms y popups con transición breve.
- Respeto de la preferencia de Windows para reducir animación.
- Estado vacío, placeholder de captura, filas pendientes/completadas y aviso de
  deshacer refinados visualmente.
- Modo interno de render que genera PNG desde la ventana WPF real.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 21 pruebas aprobadas, 0 fallidas antes del render final.
- Render real generado en `todos-wpf-preview.png`, usando el fixture funcional.
- Revisión visual comprobó: título, contadores, captura, pendientes, acciones,
  sección completada, redimensionamiento, contraste y bordes sin recorte.

### Resultado

Etapa válida. La interfaz real sigue la dirección visual aprobada y no conserva
controles principales con apariencia predeterminada incongruente.

## Etapa 7 — Ciclo de vida, recuperación y Windows

### Implementación

- Instancia única mediante recursos locales de Windows; un segundo inicio activa
  el menú de la instancia que ya está abierta.
- Registro en `HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`, sin
  requerir instalador ni privilegios de administrador.
- Inicio con Windows habilitado por defecto, configurable desde el menú y
  autorreparable si el ejecutable portable cambia de ubicación.
- Los modos de prueba y render no modifican el registro real.
- Corrección por monitor y DPI del área de trabajo disponible.
- Revalidación de posiciones al cambiar monitores, resolución o volver de una
  suspensión.
- Guardado inmediato antes de suspender y cierre bloqueado si el estado no puede
  persistirse.
- Registro operativo local rotativo, sin incluir el contenido de las tareas.
- Manejo de errores de interfaz y segundo plano con mensajes recuperables.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 26 pruebas aprobadas, 0 fallidas.
- Registro de arranque probado mediante almacenamiento simulado: habilitar,
  deshabilitar, rutas con espacios y detección de ejecutable movido.
- Señal entre primera y segunda instancia comprobada con identificadores aislados.
- Creación de log y captura de excepción controlada comprobadas.
- Rechazo del cambio de inicio automático conserva correctamente el valor visible.
- Smoke test WPF real completado con carpeta de datos aislada y sin tocar el
  arranque del usuario.

### Resultado

Etapa válida. Todo's se integra al ciclo de Windows sin instalador, conserva los
datos ante suspensión/cierre y evita instancias duplicadas.

## Etapa 8 — Accesibilidad y facilidad de uso

### Implementación

- Orden de tabulación natural y foco visible en botones, campos y casillas.
- Nombres y ayudas para automatización de Windows en ventanas, campos, botones de
  icono, tareas, selector de color y control de tamaño.
- Tooltips descriptivos para todas las acciones representadas solo con iconos.
- `Ctrl+N` crea una lista desde el control principal y enfoca la captura dentro
  de una lista.
- `Enter` agrega tareas o confirma edición; `Esc` cancela edición o cierra menús.
- Las filas pendientes pueden enfocarse: `Alt+↑` y `Alt+↓` las reordenan, y
  `Supr` elimina únicamente la fila que tiene foco claro.
- Casillas esenciales ampliadas a un área interactiva de 28 × 32 unidades.
- Reducción de animaciones y escalado por DPI se conservan según Windows.
- Cálculo WCAG de contraste añadido como regla comprobable del sistema visual.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 27 pruebas aprobadas, 0 fallidas.
- Los siete temas superan relación 4.5:1 para texto normal sobre fondo y
  superficie.
- Render WPF real revisado después de los cambios: controles, textos largos,
  foco potencial y áreas de interacción permanecen sin recortes.
- Smoke test real completado con navegación y recursos accesibles cargados.
- Los nombres para Narrador se verificaron en el árbol XAML; una comprobación
  auditiva final queda como revisión manual recomendada en un equipo de uso.

### Resultado

Etapa válida. Los flujos principales no dependen exclusivamente del ratón y las
acciones esenciales tienen texto, ayuda o nombre accesible además del color.

## Etapa 9 — Pruebas integrales y estabilización

### Cobertura añadida

- Archivo principal y respaldo simultáneamente dañados: inicia vacío, informa el
  problema y conserva ambos archivos como evidencia recuperable.
- Instantánea profunda: cambios posteriores de la interfaz no alteran el estado
  que ya estaba en proceso de guardado.
- Carga acumulada de 20 listas, 1,000 tareas, siete colores, posiciones mixtas y
  ventanas visibles/ocultas.
- Round-trip integral de ese estado con conservación de títulos Unicode, orden,
  tamaños y visibilidad.

### Validación ejecutada

- Build Release completo: 0 advertencias, 0 errores.
- 30 pruebas aprobadas, 0 fallidas, en aproximadamente 0.7 segundos.
- El round-trip de 20 listas y 1,000 tareas quedó bajo el límite preventivo de
  cinco segundos.
- Búsqueda de pendientes técnicos: no hay `TODO`, `FIXME` ni funciones de
  producto sin implementar; los `NotSupportedException` encontrados pertenecen
  únicamente al retorno no aplicable de convertidores WPF.
- Smoke test del ejecutable Release: código de salida 0.
- Ciclo real medido en aproximadamente 3.0 segundos, incluyendo la pausa interna
  de validación de 0.9 segundos; memoria observada al estar listo: 47.6 MB.
- Render final posterior a accesibilidad revisado sin recortes o regresiones.
- El directorio todavía no es un repositorio Git, por lo que no existe una
  revisión de diff; la compilación y las pruebas cubren directamente todos los
  archivos de la solución.

### Resultado

Etapa válida. No existen defectos conocidos de pérdida de datos o bloqueo en los
flujos comprobables de esta versión.

## Etapa 10 — Publicación portable

### Implementación

- Perfil `win-x64` Release, autocontenido y de archivo único.
- Runtime .NET y librerías nativas incluidos dentro de `Todos.exe`.
- Símbolos y archivos de desarrollo excluidos.
- Icono, descripción `Todo's`, producto y versión `1.0.0` incrustados.
- Artefacto directo y paquete ZIP de un solo archivo.
- Sumas SHA-256 para comprobar integridad.

### Artefactos

- `publish/win-x64/Todos.exe`: 68.69 MiB.
- `publish/Todos-1.0.0-win-x64.zip`: 63.21 MiB aproximadamente.
- `publish/Todos-1.0.0-win-x64.sha256`: hashes del EXE y ZIP.

### Validación ejecutada

- Restauración específica y publicación para `win-x64` correctas.
- Carpeta publicada contiene únicamente `Todos.exe`.
- Ejecutable publicado iniciado directamente, con código de salida 0 y guardado
  real de estado.
- Render PNG generado desde el EXE autocontenido, verificando recursos WPF,
  estilos, icono y ensamblados incluidos.
- El ejecutable se copió a otra carpeta y recuperó 1 lista y 3 tareas desde la
  misma ubicación de datos, validando actualización/movimiento portable.
- El ZIP se extrajo en una carpeta aislada: contenía un único EXE, inició con
  código 0 y creó su estado.
- El equipo disponible sí tiene .NET instalado; por ello no simula literalmente
  un Windows limpio. La publicación `SelfContained=true`, sus runtime packs y la
  ejecución del archivo único comprueban que el SDK no es un requisito del
  paquete. La prueba final en otro Windows 10/11 sigue recomendada antes de una
  distribución masiva.
- No se modificó el inicio real de sesión del equipo durante las pruebas; la ruta
  publicada y el alta/baja se validaron con el registro simulado de la etapa 7.

### Resultado

Etapa válida. Existe un paquete portable ejecutable sin instalador y sin instalar
.NET por separado.

## Etapa 11 — Documentación y cierre

### Documentación entregada

- `README.md`: presentación, uso, privacidad, desarrollo y estructura.
- `/.codex/manual-usuario.md`: flujos, atajos, respaldo, restauración y soporte.
- `/.codex/build-and-publish.md`: validación, publicación, integridad y
  actualización.
- `/.codex/decisiones-tecnicas.md`: arquitectura, seguridad de datos y límites.
- `/.codex/backlog.md`: mejoras posteriores separadas del MVP.
- `CHANGELOG.md`: versión 1.0.0.
- `THIRD-PARTY-NOTICES.md`: dependencias y recursos distribuidos.
- `scripts/publish-win-x64.ps1`: publicación reproducible, ZIP y SHA-256.

### Validación final ejecutada

- El script de publicación completo terminó correctamente.
- Build Release final: 0 advertencias, 0 errores.
- Suite final: 30 aprobadas, 0 fallidas.
- EXE final: código 0 y creación de estado comprobada.
- Hash SHA-256 del EXE y del ZIP coincide con el archivo entregado.
- Código, pruebas, documentación y artefactos finales inventariados.

### Revisiones externas recomendadas

No bloquean que el programa se use, pero deben realizarse antes de una
distribución masiva:

- Prueba física en un Windows 10 x64 y un Windows 11 x64 sin SDK.
- Sesión con Narrador y monitores reales de escalas mixtas.
- Firma de código para reputación de Windows si se distribuirá públicamente.

### Resultado

Etapa válida. Las 12 etapas del plan están completadas; Todo's 1.0.0 queda listo
para uso portable y cuenta con instrucciones reproducibles para mantenerlo.

## Ajustes posteriores — Todo's 1.0.1

### Problema observado

El video de revisión mostró que los botones del menú principal solo cambiaban a
su estado presionado. La captura de puntero para arrastrar el botón flotante
recibía también los eventos de los controles contenidos en el `Popup`, los
marcaba como atendidos e impedía que llegaran al comando.

### Correcciones e implementación

- Área de arrastre separada del `Popup` y protegida por una política que solo
  acepta descendientes visuales reales del botón flotante.
- Prueba de regresión para comprobar que un botón del menú no se interpreta como
  gesto de movimiento.
- Hover primario con morado más luminoso y texto blanco permanente.
- Icono nuevo `icon.ico/png/svg` aplicado al EXE, botón y área de notificación.
- Fredoka incorporada como recurso local en cinco pesos.
- Renderizador ligero de los SVG propios para botones, casillas y acciones.
- Botón exclusivo de movimiento en el encabezado; el resto del encabezado deja
  de iniciar desplazamientos accidentales.
- Sección **Listas ocultas** al mismo nivel que mostrar/ocultar todas, desplegable
  con chevron y acciones independientes para mostrar o eliminar.
- Botón y menú principal refinados en jerarquía, espaciado, bordes y sombra.
- Nuevo render interno del menú para poder revisar cambios visuales futuros.

### Validación

- Build Release: 0 advertencias, 0 errores.
- 31 pruebas aprobadas, 0 fallidas.
- Render real de ventana TODO y menú principal revisado con Fredoka e iconos SVG.
- Metadatos de Fredoka confirmados: copyright del proyecto y licencia SIL OFL
  1.1; avisos de terceros actualizados para la distribución.

## Ajustes posteriores — Todo's 1.0.2

### Revisión recibida

La segunda revisión solicitó actualizar nuevamente la identidad, simplificar el
menú, sustituir el acordeón de listas ocultas por un submenú y mejorar la
legibilidad e interacción de las tareas. El video confirmó que el orden sí se
guardaba, pero no mostraba una posición de inserción durante el arrastre.

### Implementación

- `icon.png/svg` actualizado y `icon.ico` regenerado con nueve resoluciones,
  desde 16 hasta 256 px, para evitar escalados deficientes de Windows.
- Encabezado del menú reducido a **Todo's**, sin logo ni subtítulo.
- **Listas ocultas** convertido en un `Popup` lateral independiente, con mostrar
  y eliminar por elemento.
- Tokens morados comunes aplicados a estados hover y pressed; se eliminaron los
  estados azules nativos en las acciones revisadas.
- Ancho inicial de nuevas listas aumentado de 340 a 380 unidades.
- Resumen de pendientes/completadas ampliado; controles de movimiento más
  visibles y símbolo de cuatro flechas con mayor peso visual.
- Captura **Nueva tarea** más compacta, etiquetada y con bordes redondeados;
  botón `+` redondeado.
- Texto de tareas aumentado a 14 unidades y edición en una superficie morada,
  redondeada y claramente enfocada.
- Botones redundantes de lápiz eliminados; el doble clic continúa iniciando la
  edición.
- Reordenamiento con cálculo por mitad superior/inferior, guía morada animada,
  desplazamiento de la fila destino y transición final de acomodado.
- Prueba nueva para las posiciones de inserción y actualización de pruebas de
  ancho predeterminado y submenú.

### Validación visual intermedia

- Render de la ventana a 380 px revisado sin recortes de título.
- Render del menú principal revisado sin logo ni subtítulo.
- Render independiente del submenú revisado con estados y acciones moradas.
- La automatización visual de Windows no expone estas ventanas WPF sin marco y
  sin entrada en la barra de tareas; se mantienen los renders internos y las
  pruebas de política de interacción como validación reproducible.

### Cierre de versión

- Build Release final: 0 advertencias y 0 errores.
- Suite final: 32 pruebas aprobadas y 0 fallidas.
- Render del modo de edición revisado con borde, fondo y foco redondeados.
- Publicación portable 1.0.2 generada con avisos de terceros.
- Ejecutable extraído del ZIP: código de salida 0 y estado guardado.
- SHA-256 del ejecutable y del ZIP verificados contra el archivo publicado.

## Ajustes posteriores — Todo's 1.0.3

### Diagnóstico

El tercer video mostró el cursor de arrastre como no permitido y el cuadro de
**Error inesperado**. El log local registró `InvalidOperationException`: WPF
había congelado el `ScaleTransform` declarado en la plantilla y el código
intentaba animarlo durante `DragOver`.

### Correcciones

- Política visual que clona transformaciones `ScaleTransform` y
  `TranslateTransform` congeladas antes de animarlas.
- Prueba STA que congela explícitamente ambas transformaciones y confirma que
  las copias mutables pueden animarse.
- Ancho predeterminado aumentado a 410 unidades; estados que conservan los
  anchos predeterminados anteriores de 340 o 380 se normalizan a 410.
- Submenú lateral sin encabezado duplicado: su contenido comienza directamente
  con las listas ocultas y usa títulos de 14 unidades.
- Comando y botón de listas ocultas desactivados cuando la colección está vacía.
- Botón flotante sin borde, margen ni sombra de WPF; el PNG aporta todo el
  acabado visual.
- Reafirmación de z-order mediante `SetWindowPos(HWND_TOPMOST, SWP_NOACTIVATE)`
  al mostrar o activar una ventana TODO, de modo que el botón permanece encima
  sin robar foco.

### Validación intermedia

- Build Release: 0 advertencias y 0 errores.
- 33 pruebas aprobadas, 0 fallidas.
- Render de ventana a 410 unidades revisado.
- Render de menú, submenú directo y botón sin borde revisados.

### Cierre de versión

- Paquete portable 1.0.3 generado correctamente.
- Ejecutable extraído: código de salida 0 y persistencia inicial confirmada.
- SHA-256 del EXE y ZIP coinciden con el archivo publicado.
- Metadatos del ejecutable confirman la versión 1.0.3.
- Render final realizado desde el ejecutable publicado.

## Auditoría de alineación documental — 2026-09-17

### Alcance revisado

- Versión, dimensiones, recursos visuales y límites definidos en el proyecto.
- Comportamiento real del botón flotante, submenú de listas ocultas, movimiento
  de ventanas y reordenamiento de tareas.
- Persistencia, inicio con Windows, distribución portable y artefactos 1.0.3.
- Suite vigente de 33 pruebas y comandos reproducibles de compilación.

### Documentos alineados

- `context.md`: estado implementado y rutas canónicas de consulta.
- `decisiones-tecnicas.md`: decisiones acumuladas hasta 1.0.3.
- `diseno.md`: flujos y wireframes actuales, sin controles retirados.
- `manual-usuario.md`: orden superior del botón, feedback de arrastre y
  normalización de anchos anteriores.
- `build-and-publish.md` y `README.md`: regeneración del icono y funciones
  actuales.
- `backlog.md`: retirado el indicador de destino, ya implementado; se conserva
  como pendiente únicamente el auto-scroll para listas largas.
- `plan-trabajo.md`: distingue la línea base histórica 1.0.0 de los artefactos
  vigentes 1.0.3.

No se modificó código de producto durante esta auditoría.

## Agrupaciones, arrastre y captura — Todo's 1.1.0 (2026-09-17)

### Implementación

- Grupos con identidad propia por lista; General permanente y predeterminado.
- Creación, renombrado, captura por grupo, movimiento por arrastre/menú y
  eliminación de agrupación sin eliminar tareas.
- Esquema 2 y migración desde 1; instantáneas y normalización actualizadas.
- Corrección del booleano invertido entre cálculo de destino y marca visual.
- Tarjeta junto al cursor, origen atenuado, transición de filas al soltar,
  desplazamiento automático en bordes y limpieza al cancelar.
- Padding duplicado eliminado y placeholder integrado en la plantilla.

### Validación

- Compilación Release sin errores ni advertencias.
- 40 pruebas aprobadas: incluye migración, persistencia, grupos, edición de grupo,
  preservación de completadas y deshacer después de quitar una agrupación.
- Vista WPF real: alineación del cursor y marcas antes/después verificadas.
- Eventos WPF de DragOver/Drop: subir, bajar, mover entre grupos y a grupo vacío.
- Renders revisados en `artifacts/groups-review` a 560, 410 y 280 unidades.
- La tarjeta se comprobó con su texto real, abierta y sin interceptar hit testing.
- La revisión física prolongada del gesto con ratón y monitores de DPI mixto
  permanece manual; los eventos automatizados no sustituyen esa revisión.

### Cierre del paquete 1.1.0

- ZIP autocontenido generado con versión 1.1.0.0 y avisos de terceros.
- EXE extraído: lectura y migración en memoria de un fixture de esquema 1,
  representado en General sin perder tareas. Sin ediciones no se reescribe el JSON.
- Segundo arranque con grupos: conserva asignaciones y nombres; render del
  ejecutable publicado revisado. SHA-256 del EXE y ZIP verificados.
- Se añadió aislamiento del mutex para smoke/render con carpeta explícita.
  La identidad habitual sigue siendo
  `Todos`; los modos aislados usan una identidad derivada de su carpeta.
- Regresión añadida para identidades de instancia. Suite final: 40/40.

## Refinamiento de agrupaciones — 1.1.1 (2026-09-17)

- Captura de una fila; creación dentro del menú lateral. Menú compartido para
  selección y traslado, siguiendo el Popup de Listas ocultas.
- Encabezados y chevron del tema; plegado por grupo persistido en esquema 2.
- Confirmación personalizada con Cancelar enfocado antes de quitar un grupo.
- Papelera y recurso move-up-down.svg aplicados.
- Release sin errores ni advertencias; 41 pruebas aprobadas.
- Prueba WPF real de una fila, tema azul, acordeón, selección/creación en menú,
  traslado por menú, cancelar y confirmar eliminación preservando tareas.
- Round-trip e instantánea del estado de acordeones y carga con defaults abiertos.
- Renders revisados a 560, 410 y 280; menú de selección, traslado y confirmación
  en artifacts/groups-refinement-review.

## Refinamiento de arrastre y encabezado — 1.1.2 (2026-09-17)

El video mostró que los márgenes entre filas activaban el manejador del grupo,
cambiando la marca de inserción por un destino global. Se centralizaron DragOver
y Drop en TasksScrollViewer con un único cálculo de posición. La barra conserva
su ancho y usa Accent; solo grupos vacíos o plegados se resaltan completos.

La tarjeta es un RenderTargetBitmap de la fila, con DPI/tamaño reales, casilla,
texto e iconos. Se dibuja en Canvas sin hit testing y con colores Surface/Accent;
se elimina al terminar o cancelar. Se conserva la animación al soltar.

Se retiró el botón de mover. Todo el encabezado inicia el movimiento al superar
el umbral; los botones quedan excluidos. Doble clic/F2 permite editar el título.

Compilación Release: 0 errores y 0 advertencias. Suite: 41/41 aprobadas.
La regresión incluye margen entre filas, varios DragOver consecutivos, Drop en
ese mismo margen, grupos plegados, bitmap no vacío y tamaño/color del preview,
y política de encabezado. Renders revisados en artifacts/drag-refinement-review.
La comprobación fue con eventos/renders WPF; el recorrido físico sigue siendo manual.

Cierre 1.1.2: ZIP publicado y ejecutable extraído verificado con código de salida 0.
Metadatos 1.1.2.0, SHA-256, migración, grupos y reinicio correctos.
Render final revisado en artifacts/groups-release-0397f2c40d004fbeb1063a3d11649697.


## Temas completos de ventana — 1.1.3 (2026-09-17)

Retirado el botón de mover a agrupación de pendientes y completadas. Se conserva
el drag and drop y la alternativa contextual. Recursos locales dinámicos según
Surface/Accent/Text; los siete temas cubren hover, presionado, foco, selección,
casillas, campos, iconos, menús y deshacer sin cambiar los recursos globales.

Prueba WPF ampliada: hover real mediante propiedades de estado en los siete
temas, presionado de agregar, selección y ausencia del botón de traslado.
Renders: artifacts/window-theme-review. Informe: artifacts/window-theme-tests.txt.

Cierre 1.1.3: compilación sin advertencias ni errores y 41/41 pruebas aprobadas.
ZIP extraído: versión 1.1.3.0, migración, grupos, reinicio y SHA-256 correctos.
Se publicó en publish/win-x64-1.1.3 porque publish/win-x64/Todos.exe estaba en uso.
No se interrumpió la instancia del usuario. Paquete: publish/Todos-1.1.3-win-x64.zip.


## Rojo semántico de eliminación — 1.1.4 (2026-09-17)
Papeleras de listas, grupos y tareas, confirmación Eliminar y errores vuelven a TodosDangerBrush (#A54459). Los demás controles mantienen su tema.
Compilación limpia, 41/41 pruebas, renders revisados en artifacts/danger-color-review. ZIP 1.1.4 extraído: migración, grupos, reinicio, versión y SHA-256 verificados. Publicación en publish/win-x64-1.1.4 sin interrumpir la instancia abierta.

