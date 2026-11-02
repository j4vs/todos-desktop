Necesito que me ayudes a desarrollar un programa para windows a partir de mi problemática

Estado actual 1.8.2: corrección defensiva de clics fuera tras tiempo/reanudación. ScreenInteraction utiliza HWND y bounds físicos de Windows; no PointFromScreen/ActualWidth de WPF. OutsideClickMonitor renueva el hook al reanudar/cambiar pantallas e invalida acciones pendientes con generación en suspensión/renovación/cierre. Conserva snapshot del clic principal y huecos de popups. Esquema 5 sin cambios. Validación de reanudación simulada y DPI/layout en segundo monitor; hibernación real pendiente del usuario.

Estado actual 1.8.1: arrastre de subtareas homologado con tareas: preview real, atenuación del origen, línea independiente sin borde redondeado ni cambio de altura, destino estable en márgenes, scroll y animación al reordenar dentro de la principal. Se mantiene esquema 5 y notas Markdown. Pruebas WPF en segundo monitor; portable 1.8.1.

Estado actual 1.8.0: una nota Markdown por tarea/subtarea, pestañas Visualizar/Editar en ventana propia, guardado automático real e importar/exportar .md con confirmación de reemplazo. Ícono en filas con notas y acceso contextual Agregar nota/Ver nota. Sin imágenes; render nativo de WPF con Markdig 1.3.2, sin HTML ejecutable. Notas conservadas en snapshot, grupos, completado, eliminación/deshacer y Papelera. Ventana y diálogos de notas pertenecen a la lista a efectos de minimización automática. Esquema 5 migra 1–4. Pruebas y revisión en segundo monitor; portable 1.8.0.

Estado actual 1.7.2: corrección del resaltado exclusivo al cambiar de tarea/subtarea y retirada de su nombre del menú contextual. Portable compilado y empaquetado para revisión del usuario, sin ejecutar pruebas ni abrir ventanas en esta entrega. Se conserva esquema 4.

Estado actual 1.7.1: subtareas de un nivel, avance/plegado junto al checkbox, captura/edición en línea y orden por arrastre/teclado dentro de la principal. Menú junto al cursor con fila resaltada y acciones Editar, Completar/Reabrir, Eliminar; principales añaden Agregar subtarea y «Mover a [grupo]». Retirada la opción de conversión. La etiqueta del principal se oculta durante su arrastre. Completar todos los pasos completa la raíz; casilla de raíz completa todos y ofrece Deshacer. Árbol conservado al mover grupo, borrar/deshacer y restaurar Papelera. Esquema 4 migra versiones 1–3. Ventanas de pruebas dirigidas al monitor secundario si existe.

Estado anterior 1.6.0: apariencia global desde Configuración, letra −/+ (80–140 %), tamaño del principal Chico/Mediano/Grande (40/52/68) y espaciado Chico/Mediano/Grande (.75/1/1.3). Aplicación inmediata y guardado en AppSettings/snapshot; esquema 3 compatible. Mediano y letra 100 % conservan valores anteriores.

Estado anterior 1.5.0: acción Minimizar entre Salir y Configuración oculta solo el principal y sus menús. Menú del área de notificación Mostrar botón / Ocultar botón dinámico; doble clic recupera el principal. Visibilidad de sesión, botón visible al reiniciar. Las listas, posición y acceso rápido permanecen.

Estado anterior 1.4.0: menú de lista oculta junto al círculo, papelera compacta y flecha restaurar corregida. Grupos personalizados reordenables con flechas y renombrables con lápiz; General fijo. Vaciar completadas por lista con deshacer de lote de cinco segundos. Se mantiene esquema 3.

Uso codex para desarrollar aplicaciones web, generalmente separo en dos proyectos de codex para un proyecto
- Un proyecto para frontend
- Otro proyecto para backend

La mayoría del tiempo voy trabajando con ambos:
- Tarea en backend se encarga de realizar endpoints
- Tarea frontend se encarga de implementar vistas

---

A veces necesito hacer varias etapas/pasos pero como hago varias cosas se me llega a olvidar. Ej ahorita estoy corrijiendo un módulo en un proyecto, entonces la tarea backend está haciendo endpoints. Luego, le tengo que decir a la tarea frontend que haga la vista/ajustes, cuando acabe hare pruebas, cuando haga pruebas y correcciones si es que hay tengo que pedirle a tarea backend que borre endpoints legacy que ya no se usan, luego tengo que pedir más cosas que van en serie. Entonces hay veces que se me olvida que es lo siguiente que tengo que hacer

Así que me gustaría hacer un "TODO LIST" sencillo y compacto para poder hacer ahí anotaciones rápidas e ir marcando progreso. No quiero usar programas ya existentes porque no cumplen con mis requisitos

---

Necesito algo similar a "Notas rápidas" de windows: Ventanas pequeñas que se puedan quedar visibles todo el tiempo en pantalla aunque abra otrosp programas, así todo el tiempo lo puedo visualizar

---

Necesito un botón central que también se quede visible todo el tiempo. Este botón será el encargado de:
- Crear una nueva "ventana" de todo
- Cerrar el programa
- Ocultar "ventanas" todo (individualmente o todas)

Este botón "central" lo podré ir moviendo por la pantalla por si me estorba la visualización, lo mueva. Al cerrarse el programa y volver a abrir tiene que recordar su última posición en pantalla

---

Para las ventanas:
- Estas recibirán un título
- Podré agregar tareas
- Podré marcar tareas como completadas
- Podré moverlo por la pantalla
- Tendrá un botón para ocultar la ventana
- Tendrá un botón para borrar
- Otro botón para cambiarle de color
- Hacer toggle a la sección de tareas completadas
- Reordenar tareas pendientes
- Redimensionar la ventana

---

Al cerrar el programa desde el botón, deberá de guardar las ventanas para cuando abra de nuevo el programa, abra las ventanas que tenía que sí estaban visibles en la posición que estaba. Las ventanas que estaban ocultas por default no las muestra

---

Usar un lenguaje que consideres apropiado para realizar el programa. De preferencia que este acabe siendo portable

---

Vamos también a priorizar el diseño, tiene que ser muy estético moderno, con tipografía bonita, colores pastel, animaciones agradables, íconos

---

La aplicación deberá de ser fácil de usar

---

## Estado implementado

La necesidad anterior dio origen a **Todo's 1.3.1**, una aplicación WPF portable
para Windows 10/11 x64. El alcance vigente incluye:

- botón flotante de 52 × 52 que conserva su posición y permanece por encima de
  las ventanas TODO;
- listas independientes de 410 × 420 al crearse, redimensionables desde un
  mínimo de 280 × 220;
- creación, edición por doble clic, completado, reapertura, eliminación con
  deshacer y reordenamiento visual de tareas;
- encabezado arrastrable para mover cada lista, siete colores pastel y tipografía
  Fredoka integrada;
- botón circular vectorial con acceso rápido a la última lista minimizada;
- hover de un segundo para recuperar listas ocultas, acciones redondas con clic
  derecho y configuración de minimización automática opcional;
- persistencia local automática, respaldo, recuperación, instancia única e
  inicio con Windows activado de forma predeterminada;
- icono en el área de notificación y salida segura con guardado confirmado;
- distribución autocontenida en un único EXE, sin instalador ni runtime de .NET
  requerido en el equipo final.

Los detalles operativos actuales están en `/.codex/manual-usuario.md`; las
decisiones técnicas vigentes, en `/.codex/decisiones-tecnicas.md`; y el historial
de implementación y validación, en `/.codex/progreso.md`.

### Acceso rápido — 1.3.1

El ícono del ejecutable y de notificación es circular morado con check y borde
blancos. SVG canónico genera PNG e ICO con nueve tamaños, sin fondo cuadrado.

La asociación, título y color de la última lista minimizada se conservan al
reiniciar. Clic izquierdo la muestra/enfoca, o despliega ocultas si no hay
asociación. Eliminar la lista asociada vuelve al morado original sin fallback.
Hover continuo de un segundo muestra ocultas; clic derecho muestra acciones
moradas Salir, Configuración, Papelera y Nueva lista y prevalece sobre hover. Configuración
contiene inicio con Windows y minimización automática opcional, apagada al inicio.
Clic fuera/Escape cierra los desplegables animados. Se migra al esquema 3.

Etiquetas laterales de 13 px, a derecha si no hay espacio a izquierda. El título
principal aparece solo en hover y el color usa el acento un 10 % más oscuro.
check.svg se usa en principal, ocultas y tareas marcadas. Scroll delgado de 10 px.
Checks de principal y ocultas blancos y más pequeños; configuración activada usa checkbox-checked.svg.
Configuración oculta acciones y centra textos. Con auto-minimización, pulsar
principal mientras su lista está abierta solo la oculta, sin volver a abrirla.

### Papelera — 1.3.0

Borrar una lista la mueve sin confirmación nativa a TrashedWindows. Clic derecho
sobre una lista oculta ofrece Mover a papelera. El nuevo botón Papelera abre
una ventana con Restaurar y eliminación definitiva con confirmación propia.
Persisten listas completas, tareas y grupos; restaurar conserva identidad y abre
la ventana. Eliminar la asociación la limpia; restaurar no elige un acceso rápido.

### Ampliación 1.1.0

Agrupaciones por lista con General predeterminado, captura por grupo y traslado
entre grupos. Quitar una agrupación conserva sus tareas en General. El arrastre
incluye tarjeta visible, indicador de inserción corregido y transición de filas;
el editor de nueva tarea alinea cursor y placeholder. Se migra el esquema 1 a 2.

### Ajustes 1.1.1

Captura de una fila, selección/creación y traslado mediante menús laterales,
encabezados según tema, agrupaciones plegables con estado persistente,
confirmación y papelera para quitar grupos, e icono move-up-down para arrastrar.

### Ajustes 1.1.2

Destino de arrastre unificado para filas y márgenes; indicador estable según tema.
Vista previa de la fila completa, a tamaño real, con casilla e iconos y colores
de la ventana. Encabezado arrastrable sin botón de movimiento; título editable
por doble clic o F2. No cambia el esquema de datos.

### Ajuste 1.1.3 — 2026-09-17

Botón de mover a agrupación retirado de las tareas. Colores e interacciones
ligados al tema de cada ventana, incluidos hover de agrupación y agregar,
presionado, foco, casillas, campos, iconos, menús y deshacer.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.
