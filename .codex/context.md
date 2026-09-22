Necesito que me ayudes a desarrollar un programa para windows a partir de mi problemática

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

La necesidad anterior dio origen a **Todo's 1.1.4**, una aplicación WPF portable
para Windows 10/11 x64. El alcance vigente incluye:

- botón flotante de 52 × 52 que conserva su posición y permanece por encima de
  las ventanas TODO;
- listas independientes de 410 × 420 al crearse, redimensionables desde un
  mínimo de 280 × 220;
- creación, edición por doble clic, completado, reapertura, eliminación con
  deshacer y reordenamiento visual de tareas;
- encabezado arrastrable para mover cada lista, siete colores pastel y tipografía
  Fredoka integrada;
- ocultar, mostrar y eliminar listas ocultas desde un submenú lateral; la opción
  queda desactivada cuando no existen listas ocultas;
- persistencia local automática, respaldo, recuperación, instancia única e
  inicio con Windows activado de forma predeterminada;
- icono en el área de notificación y salida segura con guardado confirmado;
- distribución autocontenida en un único EXE, sin instalador ni runtime de .NET
  requerido en el equipo final.

Los detalles operativos actuales están en `/.codex/manual-usuario.md`; las
decisiones técnicas vigentes, en `/.codex/decisiones-tecnicas.md`; y el historial
de implementación y validación, en `/.codex/progreso.md`.

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
