# Historial de cambios

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
