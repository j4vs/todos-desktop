# Decisiones técnicas vigentes — Todo's 1.1.4

## Plataforma

- **C# + WPF sobre .NET 8:** integración nativa con ventanas, DPI, área de
  notificación y registro de Windows, con una interfaz completamente diseñada.
- **Windows 10/11 x64:** alcance explícito del MVP.
- **MVVM ligero sin framework externo:** reduce tamaño conceptual, dependencias y
  riesgos de distribución.
- **Sin instalador obligatorio:** la aplicación es un único EXE autocontenido.
- **Recursos integrados:** Fredoka, el icono multirresolución y los SVG propios
  se incorporan al ejecutable; no dependen de archivos visuales externos.

## Estado y persistencia

- Un agregado `AppState` contiene configuración, botón, ventanas y tareas.
- JSON legible en `%LOCALAPPDATA%\Todos`, fuera de la carpeta portable.
- Guardado con instantánea profunda, archivo temporal, reemplazo atómico y
  respaldo.
- Escrituras frecuentes agrupadas; cierre y suspensión fuerzan guardado.
- El contenido de tareas no se escribe en el log operativo.
- El esquema tiene versión; una versión futura desconocida se rechaza sin
  sobrescribir datos.

## Ventanas

- Botón y listas usan `Topmost`, `ShowInTaskbar=false` y ventanas sin marco.
- El botón reafirma su orden superior con `SetWindowPos(HWND_TOPMOST,
  SWP_NOACTIVATE)` al mostrar o activar listas, sin quitarles el foco.
- `TodoWindowManager` separa la representación WPF del estado persistente.
- Posiciones se expresan en unidades independientes de DPI y se corrigen contra
  el monitor disponible.
- Un `Mutex` y un evento local garantizan instancia única y reactivación.
- Las listas nuevas miden `410 × 420`, con mínimo `280 × 220`; estados que aún
  conservan los anchos predeterminados históricos de 340 o 380 se normalizan a
  410.

## Interacción y presentación

- El movimiento empieza desde todo HeaderArea al superar el umbral de arrastre.
  WindowHeaderInputPolicy excluye botones y el título mientras está en edición.
  Doble clic/F2 activa la edición del título; no existe un botón dedicado a mover.
- La edición de tareas se inicia con doble clic y no duplica esa acción con un
  botón de lápiz.
- El reordenamiento muestra guía estable y desplazamiento animado al soltar. Antes
  de animar, `TaskDragVisualPolicy` clona transformaciones congeladas por WPF.
- Las listas ocultas se muestran en un submenú lateral directo, con acciones
  para restaurar o eliminar; el acceso se desactiva cuando la colección está
  vacía.
- El icono `.ico` contiene nueve tamaños entre 16 y 256 px y se regenera desde
  `Assets/icon.png` mediante `scripts/build-icon.ps1`.

## Integración con Windows

- `NotifyIcon` mantiene presencia cuando todas las listas están ocultas.
- Inicio automático mediante la clave `Run` de `HKCU`, solo para el usuario
  actual y sin elevación.
- El comando registrado cita la ruta del EXE e incluye `--autostart`.
- Al mover el EXE, la ruta se repara en la siguiente ejecución manual.

## Calidad

- Pruebas propias sin NuGet para que el repositorio funcione incluso sin fuentes
  externas configuradas.
- Lógica de dominio, persistencia, ventanas, ciclo de vida, accesibilidad y carga
  se prueban separadamente.
- Modo `--smoke-test` y render WPF real permiten validar el proceso y los recursos
  con datos aislados sin alterar el inicio del usuario.
- Los colores se verifican con cálculo WCAG y umbral 4.5:1.

## Límites conscientes del MVP

- Sin sincronización, cuentas, nube, fechas límite ni recordatorios.
- Sin papelera permanente para listas; las tareas solo tienen deshacer breve.
- Sin firma de código ni instalador en la versión actual.
- Windows decide si el icono aparece directamente o dentro del chevrón.
- La revisión final con Narrador y hardware multimonitor de escalas mixtas es
  manual porque depende de un entorno físico.

## Agrupaciones y arrastre — 1.1.0

- Esquema 2: `TodoWindowState.Groups` contiene identificadores y nombres;
  `TodoTaskItem.GroupId = null` representa General, también para datos de esquema 1.
- Orden pendiente consecutivo dentro de cada grupo. Completar/reabrir mantiene
  grupo e identidad. Quitar un grupo devuelve sus tareas a General; deshacer
  tolera que la agrupación de la tarea eliminada ya no exista.
- `AppStateSnapshot` copia grupos y asignaciones; referencias huérfanas se
  normalizan a General. Versiones 1.0.x rechazan el esquema 2 sin sobrescribirlo.
- Captura con grupo seleccionado; secciones de pendientes por agrupación y una
  sección común de completadas que indica la clasificación de cada tarea.
- Arrastre dentro de la misma ventana: TasksScrollViewer resuelve un único destino
  con ResolveTaskDrop para DragOver y Drop, incluyendo los márgenes entre filas.
  Se excluye el origen del cálculo; los puntos medios determinan antes/después.
  Grupos expandidos muestran inserción incluso en sus encabezados y espacios;
  los vacíos o plegados reciben al final. El menú permite mover sin arrastrar.
- `ShowDropCue` recibe `insertAfterTarget`, igual que el cálculo de destino.
  Canvas sin hit testing para la tarjeta que sigue al cursor; limpieza en `finally`.
  RenderTargetBitmap copia la fila completa a su DPI y tamaño reales. La tarjeta
  usa Surface/Accent; no crea una ventana Popup que pueda interferir con OLE.
  La barra usa Accent y no reinicia una animación en cada DragOver.
  El origen se atenúa y las filas animan sus posiciones anteriores hasta las
  definitivas al soltar. Se respeta la reducción de movimiento de Windows.
- Un solo Padding en el host del editor; placeholder dentro de su plantilla.
  La prueba WPF compara las coordenadas reales de texto y placeholder.

- Smoke/render con `--data-root` explícito usan una identidad de instancia
  derivada de esa carpeta, sin reactivar la instancia habitual del usuario.
  El modo normal y `--autostart` mantienen la identidad `Todos`.

## Ajustes de agrupación — 1.1.1

- `TodoTaskGroup.IsExpanded` e `IsGeneralGroupExpanded` en la ventana persisten
  el plegado. Son campos opcionales con valor true predeterminado; se mantiene
  el esquema 2. Las instantáneas copian ambos y los ViewModels notifican guardado.
- Un Popup lateral compartido sirve para selección de captura y traslado; el
  modo de captura incluye creación de grupo. Se usa el estilo de Listas ocultas,
  sin ComboBox ni menú contextual nativo visible.
- Captura en una fila. Menús admiten Escape, Tab y flechas; cierran al ocultar
  la ventana. La confirmación mantiene Cancelar como foco inicial.
- Eliminar un grupo solo muta el estado después de confirmar; conserva tareas.
- Encabezados y chevron enlazados al Accent del tema de la ventana.

## Recursos de tema por ventana — 1.1.3

WindowThemeResources crea brushes locales a partir de Surface, Accent y Text.
Los recursos dinámicos se actualizan al cambiar ColorTheme; los controles
compartidos resuelven el ámbito de su ventana sin mutar Application.Resources.
Incluye hover, presionado, foco, selección, casillas, iconos, edición, menús,
confirmaciones y deshacer. Las muestras de la paleta conservan su propio color.
Se retira el botón de traslado en pendientes y completadas; el menú de clic
derecho se conserva como alternativa sin añadir controles visibles.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.
