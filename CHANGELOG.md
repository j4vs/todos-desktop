# Historial de cambios

## 1.8.2 — 2026-10-14

- Detección de clics internos mediante ventanas y límites físicos de Windows, evitando depender de conversiones de coordenadas o layout WPF desactualizado.
- Recuperación del monitor de clics al reanudar Windows o cambiar la configuración de pantallas.
- Invalidación de clics pendientes anteriores a suspensión, reanudación, renovación o cierre del monitor, para que no oculten ventanas/menús abiertos después.
- Se conservan minimización automática, acceso rápido, espacios entre botones, notas y esquema 5.

## 1.8.1 — 2026-10-12

- Arrastre de subtareas con la misma vista previa de la fila y atenuación de origen que las tareas principales.
- Indicador de inserción independiente y recto, sin alterar el borde ni el tamaño de la fila; posición estable en espacios entre pasos.
- Resolución de destino en toda la superficie de subtareas de la misma principal, scroll automático y animación al reordenar.
- Limpieza de indicador/vista previa y restauración del estilo de opacidad al soltar o cancelar. Se conserva esquema 5 y las notas.

## 1.8.0 — 2026-10-12

- Una nota Markdown por tarea y subtarea; acceso desde Agregar nota / Ver nota en el menú contextual e ícono en filas con contenido.
- Ventana independiente con pestañas Visualizar y Editar, tema de la lista, letra configurable y tamaño ajustable.
- Visualizador integrado para títulos, énfasis, listas, casillas de lectura, citas, tablas, enlaces, separadores y código. Sin carga de imágenes ni ejecución de HTML.
- Guardado automático con estado confirmado por persistencia real; importación/exportación `.md`, con confirmación antes de reemplazar una nota.
- Las notas se conservan al completar, mover, deshacer y restaurar Papelera. Interactuar con notas o sus diálogos no minimiza su lista; ocultar la lista cierra sus notas conservando el texto.
- Esquema 5 con migración de 1–4. Versiones hasta 1.7.x rechazan el nuevo formato para evitar perder notas.

## 1.7.2 — 2026-10-12

- Al abrir el menú de otra tarea o subtarea, se limpia el resaltado anterior y solo se marca la selección actual.
- El menú contextual deja de repetir el nombre de la tarea o subtarea.

## 1.7.1 — 2026-10-12

- Menús de tareas y subtareas junto al cursor, con resaltado temporal de la fila y nombre del elemento seleccionado.
- Acciones comunes Editar, Completar/Reabrir y Eliminar; las principales añaden Agregar subtarea y destinos explícitos «Mover a…».
- Retirada la opción Convertir en subtarea. Se conservan las subtareas existentes.
- Flecha y avance de subtareas entre la casilla y el título de la principal, también en completadas.
- La etiqueta del botón principal se oculta al comenzar a arrastrarlo.

## 1.7.0 — 2026-10-12

- Subtareas de un nivel con contador de avance y plegado persistente.
- Captura desde clic derecho → Agregar subtarea; Enter agrega consecutivamente y Escape cierra. Edición con doble clic y reordenamiento dentro de la principal por arrastre o Alt+flechas.
- Completar todos los pasos completa la principal. Marcar la principal completa todo el conjunto; Deshacer recupera el progreso anterior. Reabrir un paso reabre la principal; reabrir la principal reabre sus pasos.
- Conversión de una tarea sin subtareas en paso de otra principal del mismo grupo.
- Mover, borrar, deshacer y restaurar listas conservan los árboles. Vaciar completadas conserva los pasos terminados de principales pendientes.
- Esquema 4, con migración automática desde 1–3. Las versiones hasta 1.6.x no leen este formato.

## 1.6.0 — 2026-10-12

- Configuración de letra con −/+ en pasos de 10 % (80–140 %), aplicada a listas, menús, papelera y notificación.
- Tamaño del principal Chico (40), Mediano (52, actual) o Grande (68), manteniendo proporción del check.
- Espaciado general Chico (75 %), Mediano (100 %, actual) o Grande (130 %).
- Preferencias persistentes; actualización en vivo sin reabrir ventanas. Datos anteriores conservan tamaños actuales.

## 1.5.0 — 2026-10-10

- Acción Minimizar entre Salir y Configuración: oculta el botón principal y sus menús sin cerrar el programa ni ocultar las listas.
- El área de notificación reemplaza Mostrar Todo's por Mostrar botón / Ocultar botón según la visibilidad real.
- Doble clic en el ícono recupera el botón. Posición y asociación permanecen; al reiniciar vuelve visible.

## 1.4.0 — 2026-10-10

- Menú contextual de listas ocultas junto al círculo seleccionado.
- Papelera compacta y punta de la flecha de restaurar alineada.
- Flechas para reordenar grupos personalizados y lápiz para renombrarlos; orden persistente.
- Botón discreto para vaciar completadas con deshacer de todo el lote durante cinco segundos.

## 1.3.1 — 2026-10-10

- Ícono circular morado con borde y check blancos, consistente con el botón principal.
- PNG transparente e ICO de nueve tamaños generados directamente desde el SVG,
  para el ejecutable y el área de notificación.

## 1.3.0 — 2026-10-10

- Papelera persistente para listas completas; borrar las mueve directamente sin
  confirmación nativa. Conserva tareas, grupos, colores y tamaño tras reiniciar.
- Menú contextual estilizado en las listas ocultas para moverlas a papelera.
- Nuevo botón Papelera entre Configuración y Nueva lista. Ventana con scroll,
  estado vacío, restauración y confirmación propia para eliminación definitiva.
- Esquema 3 con migración automática desde versiones anteriores.
- 49 pruebas, incluyendo persistencia, restauración y flujo de interfaz WPF.

## 1.2.3 — 2026-10-10

- Botones flotantes con fondo de acento, checks blancos más pequeños y bordes
  blancos finos, coherentes con las demás acciones.
- Los colores de las ventanas de tareas se conservan.

## 1.2.2 — 2026-10-10

- Check negro en el botón principal y en los botones de listas ocultas.
- Fondos de esos botones un 10 % más oscuros, conservando el tono pastel.
  Los fondos de las ventanas y el morado de las acciones se conservan.
- Opciones de configuración activadas vuelven a usar checkbox-checked.svg;
  las tareas marcadas conservan check.svg.

## 1.2.1 — 2026-10-10

- Etiquetas laterales de 13 px; cambian de lado si falta espacio. Botón principal
  sin etiqueta fija, título solo durante hover y despliegue de ocultas en un segundo.
- Configuración centrada verticalmente; al abrirla oculta las acciones secundarias.
- Scroll delgado y redondeado en las listas, sin flechas.
- Color del acceso rápido igual al fondo pastel de la lista asociada.
- check.svg en el botón principal, listas ocultas y casillas marcadas de tareas.
- Corregida la reapertura accidental al pulsar el botón principal mientras su
  lista está abierta con minimización automática activada.
- 45 pruebas, incluidas regresión de clic y revisión de interfaz WPF.

## 1.2.0 — 2026-10-10

- Botón principal circular vectorial, con borde y check blancos.
- Acceso a la última lista minimizada, con su color y título persistidos.
- Al eliminar la asociación vuelve al morado original, sin elegir otra lista.
- Sin asociación clic izquierdo muestra ocultas; hover continuo de dos segundos
  también las muestra como botones redondos con su color y título.
- Clic derecho muestra Salir, Configuración y Nueva lista, siempre morados.
- Despliegues animados, prioridad de acciones y cierre al hacer clic fuera.
- Configuración con inicio Windows y minimización automática opcional, apagada inicialmente.
- Scroll para muchas listas ocultas y adaptación a bordes de pantalla.
- 44 pruebas, incluyendo persistencia y eventos/renders WPF del nuevo control.

## 1.1.4 — 2026-09-17

- Restaurado el rojo semántico de las papeleras, acciones de eliminar y mensajes
  de error, independiente del tema de cada ventana.

## 1.1.3 — 2026-09-17

- Retirado el botón de mover a agrupación en tareas pendientes y completadas.
- Colores locales por ventana para hover, presionado, foco, selección, casillas,
  campos, iconos, menús y aviso de deshacer; el botón flotante mantiene su identidad.
- Verificados los estados hover de captura y agregar en los siete temas.

## 1.1.2 — 2026-09-17

- Indicador de inserción y tarjeta de arrastre con el tema de la ventana.
- Vista previa a tamaño real que conserva casilla, texto e iconos de la tarea.
- Destino unificado entre filas, márgenes y grupos; la barra no se reinicia al mover el cursor.
- Movimiento desde todo el encabezado, sin botón dedicado. Doble clic o F2 para editar el título.

## 1.1.1 — 2026-09-17

- Captura en una sola fila: campo, botón de agrupación y agregar tarea.
- Selección y creación de grupos en un menú lateral con el estilo de Listas ocultas.
- Menú personalizado para mover tareas, también mediante clic derecho y teclado.
- Títulos y chevron de grupos según el tema de la ventana.
- Plegado independiente por grupo, incluido General, conservado al reiniciar.
- Papelera para grupos y confirmación con Cancelar antes de mover sus tareas a General.
- Icono de arrastre cambiado a `move-up-down`.
- 41 pruebas aprobadas, con eventos WPF de menús, plegado y confirmación.

## 1.1.0 — 2026-09-17

- Agrupaciones por lista: General permanente, creación y renombrado de grupos.
- Captura en el grupo seleccionado; traslado por arrastre, botón de opciones o
  menú contextual, también para tareas completadas.
- Quitar una agrupación devuelve sus tareas a General sin eliminarlas.
- Migración automática de esquema 1 a 2; las tareas anteriores quedan en General.
- Corregida la inversión del indicador de inserción antes/después de una fila.
- Tarjeta con el texto de la tarea junto al cursor, origen atenuado y transición
  de las filas al soltar. Desplazamiento automático cerca de los bordes.
- Eliminado el padding duplicado del editor; cursor y placeholder alineados.
- Modos de prueba con datos explícitos usan una instancia separada de la aplicación habitual.
- 40 pruebas, incluyendo eventos WPF de arrastre y comprobación del cursor real.

## 1.0.3 — 2026-09-01

- Corregido el error que cancelaba el reordenamiento al intentar animar
  transformaciones congeladas por WPF.
- Añadida una prueba de regresión para transformaciones de arrastre congeladas.
- Ancho inicial aumentado a 410 unidades; ventanas con anchos predeterminados
  anteriores se ajustan automáticamente.
- Submenú de listas ocultas simplificado para mostrar directamente las listas,
  con títulos más grandes.
- Opción **Listas ocultas** desactivada cuando no existen elementos.
- Eliminados borde y sombra adicionales del botón flotante; se usa directamente
  el acabado del icono.
- El botón flotante se reafirma como ventana superior sin robar el foco cada vez
  que una lista se activa o se muestra.

## 1.0.2 — 2026-09-01

- Icono actualizado y optimizado con nueve tamaños para que Windows lo escale
  correctamente en el botón, el ejecutable y el área de notificación.
- Menú principal simplificado y listas ocultas trasladadas a un submenú lateral.
- Estados hover y pressed unificados con tonos morados.
- Ventanas nuevas más anchas y resumen de tareas más legible.
- Captura de nueva tarea más compacta, con etiqueta fija y bordes redondeados.
- Edición de tareas destacada con una superficie redondeada y foco morado.
- Eliminado el botón redundante de edición; se conserva el doble clic.
- Iconos de movimiento más visibles y botón `+` con esquinas redondeadas.
- Guía de inserción animada y transición al reordenar tareas por arrastre.

## 1.0.1 — 2026-09-01

- Corregidas todas las acciones del menú principal que eran interceptadas por el
  gesto de arrastre del botón flotante.
- Mejorado el contraste de hover de **Nueva lista**.
- Nuevo icono de aplicación y área de notificación.
- Tipografía Fredoka integrada dentro del ejecutable.
- Iconos SVG propios en acciones, casillas, tareas y menús.
- Nuevo botón dedicado para mover cada ventana TODO.
- Listas ocultas convertidas en una sección desplegable, con acciones para
  mostrar y eliminar cada lista.
- Refinamiento visual del botón flotante y su menú.

## 1.0.0 — 2026-08-31

Primera versión funcional de Todo's.

- Botón flotante, menú central e icono del área de notificación.
- Múltiples listas flotantes con título, color, movimiento y tamaño persistentes.
- Captura, edición, completado, reapertura, reordenamiento y eliminación de
  tareas con deshacer.
- Ocultar/restaurar listas y salida segura.
- Persistencia JSON local con respaldo y recuperación.
- Inicio con Windows activado por defecto y configurable.
- Instancia única, recuperación ante suspensión y cambio de monitores.
- Siete temas pastel, animaciones reducidas según Windows y accesibilidad por
  teclado.
- Publicación portable autocontenida para Windows x64.
