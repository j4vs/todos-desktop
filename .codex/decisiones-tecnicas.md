# Decisiones técnicas vigentes — Todo's 1.8.2

## Clics fuera y recuperación — 1.8.2

MSLLHOOKSTRUCT proporciona coordenadas de pantalla por monitor. ScreenInteraction compara WindowFromPhysicalPoint/GetAncestor (GA_ROOT) con HwndSource. Como respaldo para zonas transparentes, GetWindowRect se consulta en contexto PER_MONITOR_AWARE temporal, restaurando siempre el contexto previo. Se evita convertir mediante PointFromScreen o usar el tamaño de layout WPF como límite: ambos pueden no coincidir con la ventana nativa durante recuperación/cambio de geometría. No se modifica la conciencia DPI global ni el manifiesto.

OutsideClickMonitor captura una generación antes de preparar el efecto del clic y verifica su vigencia al ejecutarlo en Dispatcher. InvalidatePendingClicks usa Interlocked; Restart instala el nuevo hook antes de retirar el anterior, preservando el anterior si falla la instalación. Dispose invalida todo y no puede reactivarse. El controlador invalida al suspender y antes de recuperar por Resume/DisplaySettingsChanged; la recuperación en prioridad Send renueva el monitor y recoloca ventanas. Se conserva captura anterior al clic de la asociación del principal y se evitan cierres posteriores de menús abiertos por un clic interno. Tests usan coordenadas físicas reales del HWND, no un round-trip WPF que ocultaría diferencias de escala.

Referencias: https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msllhookstruct y https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-windowfromphysicalpoint.

## Arrastre de subtareas — 1.8.1

BeginRowDrag/EndRowDrag comparten captura de fila, atenuación, overlay y limpieza entre tareas y subtareas; la opacidad se restaura con su valor local original o ClearValue para respetar estilos. SubtaskDropSurface procesa eventos de toda la colección. ResolveSubtaskDrop usa bounds estables, mitades y márgenes, excluyendo el origen y manteniendo el mismo padre. Indicadores separados usan ShowDropCue y no cambian BorderThickness ni layout; DragLeave solo los limpia al salir de la superficie. ApplyAnimatedSubtaskMove utiliza transforms mutables de contenido y desplazamiento previo/posterior; esquema y datos no cambian.

## Notas Markdown — 1.8.0

TodoTaskItem.NotesMarkdown conserva el texto original sin normalizar espacios ni saltos. Snapshot copia la nota de raíces/hijos y esquema 5 impide que versiones anteriores la descarten. Normalización admite notas nulas como vacías y rescata elementos con texto dañado que conservan notas. Setter del ViewModel actualiza estado y programa guardado existente; SaveCompleted informa snapshot real/error y el controlador solo reconoce notas cuyo contenido coincide con el snapshot, evitando marcar una edición posterior como guardada.

Markdig 1.3.2 (BSD-2-Clause) analiza Markdown. MarkdownNoteRenderer convierte su árbol a FlowDocument nativo, con HTML deshabilitado, imágenes como texto y enlaces externos limitados a http/https/mailto al pulsar. No requiere WebView, navegador ni conexión para visualizar. NuGet.Config fija el origen oficial y THIRD-PARTY-NOTICES.md incluye licencia. TaskNotesWindow reutiliza tema/densidad/fuentes y confirma reemplazo antes de aplicar importación. MarkdownNoteFiles importa texto y exporta mediante temporal y reemplazo, sin truncar el destino en caso de fallo previo.

TodoWindow mantiene una ventana de nota por ID, la integra en ContainsInteraction y protege diálogos de archivo. Ocultar/cerrar la lista o quitar la tarea cierra la nota sin descartar cambios ya enviados al modelo. Cambios de tema y letra se aplican en vivo; suscripciones se retiran al cerrar. Pruebas WPF se ejecutan en segundo monitor cuando existe, con comprobación de monitor real.

## Menús y arrastre — 1.7.1

El popup de tareas usa RelativePoint con coordenadas locales del cursor y margen de 8/6; WPF ajusta su posición a los bordes de pantalla. Apertura por teclado usa un punto de respaldo junto a la fila. IsContextTarget es estado transitorio del ViewModel, sin cambio del esquema 4. Las acciones capturan su tarea antes de cerrar el popup; editar recupera foco del campo. Se retira la conversión de la interfaz, conservando dominio y datos existentes. Flecha/avance es una plantilla reutilizada en pendientes y completadas. BeginLauncherDrag oculta NamePopup antes de mover el principal. Las pruebas WPF colocan sus ventanas en el monitor secundario y verifican su monitor real al cargar.

## Subtareas — 1.7.0

TodoTaskItem agrega Subtasks y AreSubtasksExpanded. Un solo nivel; el orden se representa con el arreglo hijo. SubtaskOperations centraliza completado, sincronización de principal, conversión validada y reordenamiento. GroupId de hijos sigue a la principal. GetPending/GetCompleted y contadores consideran raíces. Normalización repara IDs, asignación de grupo y estado derivado; aplana niveles adicionales de datos dañados conservando pasos válidos. Snapshot copia profundamente los árboles.

Undo de completado restaura estados, fechas y posición sin sobrescribir textos editados. Undo de borrado restaura árbol o paso en su posición. La captura mantiene el foco al cerrar el menú y después de Enter. Payload de arrastre separado impide que un paso active traslado de principales entre grupos; el teclado distingue la fila enfocada para no mover accidentalmente su principal. Esquema 4 evita que aplicaciones anteriores descarten pasos desconocidos al guardar.

## Apariencia global — 1.6.0

AppSettings guarda FontSizeStep (-2 a 4), LauncherSize y SpacingSize (0/1/2). Valores por defecto 0/1/1 conservan apariencia anterior; normalizador repara valores inválidos y snapshot copia los tres campos. Esquema 3 permanece compatible.

AppearanceResources actualiza recursos dinámicos de tipografía, márgenes, padding y alturas desde valores base, sin acumular escalas. Fuente crece 10 % por paso; densidades .75/1/1.3. Alturas de controles acompañan densidad y crecimiento de letra. Iconos mantienen sus geometrías; diámetro del principal 40/52/68 y check proporcional. Propiedad de dependencia separada conserva el enlace de tamaño ante cambios nativos de Width/Height en WPF. Posición se limita al área de trabajo usando dimensiones actuales. Menú WinForms de notificación usa fuente propia actualizada y liberada al reemplazarla/cerrar.

## Visibilidad del botón — 1.5.0

Minimizar usa Hide de WPF, detiene hover y cierra inmediatamente los popups e invalida animaciones pendientes. No cierra ventanas de tareas ni modifica esquema 3. KeepAboveTodoWindows ignora el principal oculto. IsVisibleChanged sincroniza el texto del menú de notificación y Opening lo comprueba de nuevo. La visibilidad es de sesión; al reiniciar el principal aparece visible. Doble clic y activación externa lo recuperan.

## Ajustes 1.4.0

El orden de grupos usa el orden del arreglo persistido, sin cambio de esquema 3. General queda fijo. RefreshTasks sincroniza el orden de los view models conservando selección y plegado. Vaciar completadas retiene temporalmente el lote para Deshacer, conservando IDs, fechas y agrupación; no mueve la lista a papelera. El contexto de lista oculta se posiciona respecto del botón circular, no la fila completa.

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
- Las listas ocultas se recuperan con botones circulares desde hover de dos
  segundos, o clic izquierdo sin asociación. Para eliminar se abre la lista.
- El icono `.ico` contiene nueve tamaños entre 16 y 256 px y se regenera desde
  `Assets/icon.svg` mediante `scripts/build-icon.ps1`. Desde 1.3.1 es circular;
  cada tamaño ICO se dibuja desde el vector y también se genera icon.png de 512 px.

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
- Las listas tienen papelera persistente; tareas individuales conservan deshacer breve.
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

## Papelera — 1.3.0

- AppState.TrashedWindows guarda TodoWindowState completo y TrashedAt. Esquema 3;
  listas previas migran sin pérdida y papelera empieza vacía. Normalización recorre
  activas y borradas con IDs únicos; borradas nunca son visibles ni acceso rápido.
- RecycleBinOperations mueve/restaura/elimina por ID; mover limpia asociación,
  restaurar conserva tareas, grupos, tema, geometría e identidad y abre la lista.
- AppStateSnapshot clona también papelera profundamente; mismo guardado y respaldo.
- RecycleBinViewModel maneja confirmación definitiva. RecycleBinWindow es única,
  redondeada, con scroll y se cierra al salir. La primera eliminación no es modal.
- ListMenuPopup se abre desde el círculo o etiqueta oculta (o menú de teclado),
  comparte estilo de Configuración e integración con cierre por clic externo.

## Botón y acceso rápido — 1.3.0

- LauncherSurface usa Ellipse y SvgIcon check.svg: círculo y ventana de 52 × 52,
  borde blanco/check blanco más pequeño, morado original #9285D8. NamePopup lateral solo durante
  hover. Etiquetas de 13 px, a izquierda o derecha según el espacio del monitor.
- LauncherState.LastMinimizedWindowId persiste la asociación. Mostrar no la borra;
  eliminarla la deja en null sin fallback. Referencias inexistentes se normalizan
  a null. GetLauncherBackground reduce un 10 % los canales RGB de Accent;
  principal y ocultas comparten el mismo cálculo sin modificar los temas de ventanas.
- AppSettings.AutoMinimizeWindows es opcional, false inicialmente. Ambos campos
  participan en instantáneas y JSON; desde 1.3.0 se usa el esquema 3.
- DispatcherTimer espera un segundo de hover continuo. Clic derecho reemplaza
  ocultas por cuatro acciones y bloquea hover. Escape/clic fuera cierra. Animación
  de opacity/translate durante 150 ms y cierre posterior de Popup, respetando Windows.
- Un Popup presenta acciones u ocultas, colocado arriba/abajo con scroll.
  SettingsPopup contiene exclusivamente las dos preferencias persistidas y
  cierra las acciones al abrirse. CheckBox centra contenido e icono; usa check.svg
  para tareas y checkbox-checked.svg con Tag Configuration en opciones activadas.
  ScrollViewer tiene plantilla explícita con barra de 10 px y thumb de 6 px.
- OutsideClickMonitor observa clics mediante un hook Windows sin consumirlos ni
  alterar foco; no hace polling. Captura candidatos antes de procesar el clic
  para no ocultar la lista recién recuperada. ScreenInteraction incluye popups
  internos, incluido ListMenuPopup. Se libera al salir.
- El hook captura PreparePrimaryClick antes de ocultar listas por clic externo.
  Con auto-minimización, una asociación visible al presionar se oculta y no se
  recupera al soltar; una asociación ya oculta se muestra. Arrastrar descarta la acción.
- global.json fija SDK 8.0.422 con latestPatch; el destino sigue net8.0-windows.
