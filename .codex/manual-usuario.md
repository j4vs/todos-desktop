# Manual de usuario — Todo's 1.1.4

## Primer inicio

Ejecuta `Todos.exe`. Aparecerá un pequeño botón morado flotante y un icono de
Todo's en el área de notificación de Windows. Si no ves el icono directamente,
abre el chevrón que agrupa aplicaciones en segundo plano.

El programa inicia con Windows de forma predeterminada. Abre el botón flotante y
desmarca **Iniciar Todo's con Windows** si no lo deseas.

## Botón flotante

- Clic: abre o cierra el menú.
- Arrastrar: cambia su posición.
- Permanece por encima de las ventanas TODO sin quitarles el foco.
- **Nueva lista**: crea una ventana TODO.
- **Mostrar todas / Ocultar todas**: cambia la visibilidad sin borrar contenido.
- **Listas ocultas**: abre directamente el listado lateral para recuperar o
  eliminar una lista oculta. La opción queda desactivada cuando no hay listas
  ocultas.
- **Salir**: guarda todo y termina también el proceso en segundo plano.

Desde el icono del área de notificación puedes mostrar Todo's, crear una lista o
salir de forma segura.

## Trabajar con listas

- Haz doble clic en el título, o enfócalo con Tab y pulsa F2, para editarlo.
- Usa el botón circular de color para elegir uno de los siete temas.
- Arrastra desde el encabezado, incluido el título, para mover la lista. Los botones conservan su acción; durante la edición el título permite seleccionar texto.
- Arrastra la esquina inferior derecha para cambiar su tamaño.
- El botón `—` o `Alt+F4` oculta la lista; no borra tareas.
- El botón de papelera elimina la lista después de una confirmación. Esta acción
  no tiene papelera.

## Trabajar con tareas

- Escribe en **Nueva tarea** y presiona `Enter`, o usa el botón `+`.
- Marca la casilla para completar una tarea.
- Desmarca una tarea completada para devolverla al final de pendientes.
- Haz doble clic sobre la fila para editar.
- `Enter` confirma una edición y `Esc` la cancela.
- Arrastra el asa de flechas verticales para reordenar tareas pendientes. La
  barra del color del tema muestra el destino y las filas animan su nueva posición al soltar.
- Usa `Alt+↑` o `Alt+↓` sobre una fila enfocada para reordenar con teclado.
- El botón de papelera elimina una tarea. Durante cinco segundos aparece
  **Deshacer**.
- La sección **Completadas** puede plegarse o desplegarse.

## Agrupaciones de tareas

- Cada lista tiene **General**, que no se elimina ni se renombra.
- En la misma fila de nueva tarea, pulsa el botón con el nombre del grupo.
  Su menú lateral permite elegir el destino o pulsar **+ Grupo** para crearlo.
- Para crear, escribe el nombre y confirma con **Crear** o `Enter`; `Esc` cierra.
- El chevron de cada grupo expande o contrae su contenido. El estado se conserva
  al reiniciar, incluido General; el contador sigue visible cuando está plegado.
- Edita el título de un grupo directamente; se confirma al salir o con `Enter`.
- Arrastra entre filas para elegir una posición, también en sus márgenes. En un
  grupo expandido, el encabezado inserta al principio y el espacio inferior al final.
  Un grupo vacío o plegado recibe la tarea al final y se resalta como destino.
- La tarjeta junto al cursor copia la fila completa a tamaño real: casilla, texto
  e iconos, con superficie y borde del tema. La fila de origen se atenúa.
  La barra superior/inferior señala el lado exacto de inserción.
- El traslado visible se realiza arrastrando. Ya no aparece el botón **⋯** por tarea.
  El clic derecho conserva el menú lateral como alternativa, también en completadas.
- Hover, presionado, foco, selección, casillas, campos y menús siguen el tema de
  la ventana, incluido el botón de agrupación y el botón de agregar.
- Las completadas indican su grupo. Al reabrirse vuelven al final de su grupo.
- La **papelera** del grupo pide confirmación. **Cancelar** conserva el grupo;
  **Eliminar** devuelve todas sus tareas a General, conservando contenido y estado.
  General no tiene esta acción.
- Se rechazan nombres vacíos, duplicados o reservados como General.

## Atajos

| Atajo | Acción |
|---|---|
| `Ctrl+N` en el botón flotante | Crear una lista |
| `Ctrl+N` dentro de una lista | Enfocar la captura de tarea |
| `Enter` | Abrir menú, agregar tarea o confirmar edición según el foco |
| `Esc` | Cerrar menú/selector o cancelar edición |
| `Alt+↑` / `Alt+↓` | Reordenar la tarea pendiente enfocada |
| `Supr` | Eliminar únicamente la fila de tarea enfocada |
| `Alt+F4` en una lista | Ocultar la lista |

`Tab` recorre campos, casillas y botones. Todas las acciones de icono tienen
tooltip y nombre accesible.

## Guardado y recuperación

Los cambios se guardan automáticamente. Al salir, Todo's espera a confirmar el
guardado antes de cerrar. Si el archivo principal se daña, intenta recuperar el
respaldo. Si ambos fallan, abre una sesión vacía sin borrar los archivos dañados.

Datos:

```text
%LOCALAPPDATA%\Todos
```

Para un respaldo manual:

1. Elige **Salir** desde Todo's.
2. Copia la carpeta completa a una ubicación segura.

Para restaurar, cierra Todo's y reemplaza esa carpeta por la copia guardada.

## Mover o actualizar el programa

Puedes mover o reemplazar `Todos.exe`; las listas permanecen en AppData. Al
abrir la nueva ubicación, Todo's repara automáticamente la ruta usada para el
inicio con Windows si la opción continúa activa.

Al actualizar desde 1.0.0 o 1.0.2, las ventanas que todavía conservaban el ancho
predeterminado anterior (340 o 380) se amplían automáticamente a 410. Los
tamaños ajustados manualmente se respetan.

## Si ocurre un problema

- Abre Todo's una segunda vez para recuperar el botón de la instancia existente.
- Revisa `%LOCALAPPDATA%\Todos\todos.log` para información operativa.
- Conserva `state.json` y `state.backup.json` si necesitas recuperar datos.
- No edites esos archivos mientras Todo's esté abierto.

### Actualizar desde 1.0.x a 1.1.0

La primera carga migra automáticamente el esquema 1 a 2. Todas las tareas
anteriores quedan en General. Conserva un respaldo antes de actualizar si
necesitas volver a 1.0.x: esas versiones rechazan el esquema 2 sin sobrescribirlo.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.
