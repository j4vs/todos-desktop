# Manual de usuario — Todo's 1.8.2

## Grupos y limpieza de completadas

- Usa las flechas junto al nombre para subir o bajar grupos personalizados. El orden se guarda al reiniciar; General permanece primero.
- El lápiz selecciona el nombre del grupo para renombrarlo. Enter o salir del campo guarda; nombres vacíos, General y duplicados se rechazan.
- La papelera discreta junto a Completadas vacía solo las tareas completadas de esa lista. Puedes recuperar todo el lote con Deshacer durante cinco segundos.
- La papelera de listas muestra tarjetas compactas con restaurar y eliminar en una sola fila. El menú de listas ocultas se ancla al círculo seleccionado.

## Primer inicio

Ejecuta `Todos.exe`. Aparecerá un pequeño botón morado flotante y un icono de
Todo's en el área de notificación de Windows. Si no ves el icono directamente,
abre el chevrón que agrupa aplicaciones en segundo plano.

El programa inicia con Windows de forma predeterminada. Haz clic derecho en el botón flotante, abre **Configuración** y
desmarca **Iniciar Todo's con Windows** si no lo deseas.

## Botón flotante y acceso rápido

- Círculo vectorial con borde blanco y check blanco más pequeño; arrastrarlo cambia su posición.
- Al minimizar una lista, adopta su color de acento con brillo reducido un 10 %. Su título solo
  aparece al pasar el cursor, a la izquierda o a la derecha si falta espacio. Al comenzar a arrastrar el botón, la etiqueta se oculta.
- Clic izquierdo: muestra y enfoca la última lista minimizada. La asociación
  permanece al mostrarla y se conserva al salir y volver a abrir Todo's.
- Al eliminar la lista asociada vuelve al morado original, sin elegir otra.
- Sin asociación, el clic izquierdo muestra las listas ocultas.
- Hover continuo de un segundo: muestra las ocultas como botones redondos,
  con check.svg, su color pastel y nombre al lado. Pulsa uno para recuperar esa lista.
- Clic derecho: sustituye las ocultas por **Salir**, **Minimizar**, **Configuración**,
  **Papelera** y **Nueva lista**, de arriba hacia abajo, siempre morados.
- Las acciones prevalecen sobre hover hasta cerrarse. Clic fuera o Escape cierra
  el despliegue con animación. Los bordes se adaptan y las colecciones largas usan scroll.
- Permanece por encima de las ventanas TODO sin quitarles el foco.
- **Nueva lista**: crea una ventana TODO.
- Clic derecho sobre el círculo o nombre de una lista oculta: **Mover a papelera**.
  También puedes abrir ese menú con Shift+F10 o la tecla de menú contextual.
- **Salir**: guarda todo y termina también el proceso en segundo plano.

**Minimizar** oculta solo el botón principal y sus menús. Las listas abiertas permanecen visibles y el programa sigue funcionando.
Desde el icono del área de notificación puedes elegir **Mostrar botón** u **Ocultar botón**, según el estado actual, crear una lista o salir de forma segura. Doble clic recupera el botón. Su posición y asociación se conservan al ocultarlo; al reiniciar el programa vuelve visible.

## Papelera

Clic derecho en el principal → **Papelera**. Las listas se conservan aquí incluso
al cerrar y reiniciar, con sus tareas pendientes/completadas, grupos, color y tamaño.
**Restaurar** recupera y abre la lista. El botón rojo permite **Eliminar definitivamente**
y muestra una confirmación dentro de la misma ventana; **Cancelar** conserva la lista.
Escape cancela la confirmación o cierra Papelera. La colección admite scroll.
Enviar la lista asociada a Papelera devuelve el principal al morado original;
restaurarla no reasigna automáticamente el acceso rápido.

## Configuración

Haz clic derecho en el círculo y pulsa **Configuración**:

Abrir Configuración oculta los botones secundarios. Los textos están centrados
verticalmente y las barras de desplazamiento son delgadas, sin flechas.

- **Iniciar Todo's con Windows**: activado inicialmente, como en versiones anteriores.
- **Minimizar listas automáticamente**: desactivado inicialmente. Al activarlo,
  hacer clic fuera de una lista la oculta. Sus selectores, menús y confirmaciones
  forman parte de la propia lista y no causan una minimización accidental.

Ambas preferencias se guardan.

### Apariencia

Desde Configuración también puedes ajustar:

- **Tamaño de letra:** − y + reducen o aumentan el texto en pasos de 10 %, entre 80 % y 140 %. El porcentaje actual se muestra entre ambos botones.
- **Tamaño del botón principal:** Chico (40), Mediano (52, tamaño anterior) o Grande (68), en unidades independientes de la escala de pantalla.
- **Espaciado general:** Chico para compactar, Mediano para conservar el espaciado anterior o Grande para separar más los elementos.

Los cambios se aplican inmediatamente y se conservan al reiniciar. Afectan listas, menús, etiquetas, campos y papelera. El texto del menú de notificación también adapta su tamaño. Si Configuración necesita más espacio, permite desplazarse por su contenido. Cambiar el espaciado no cambia el tamaño elegido para el círculo principal.

Una opción activada muestra checkbox-checked.svg, con casilla y check.

Con minimización automática activa, pulsar el botón principal mientras su lista
asociada está abierta solo la oculta. El siguiente clic vuelve a recuperarla.

## Trabajar con listas

- Haz doble clic en el título, o enfócalo con Tab y pulsa F2, para editarlo.
- Usa el botón circular de color para elegir uno de los siete temas.
- Arrastra desde el encabezado, incluido el título, para mover la lista. Los botones conservan su acción; durante la edición el título permite seleccionar texto.
- Arrastra la esquina inferior derecha para cambiar su tamaño.
- El botón `—` o `Alt+F4` oculta la lista; no borra tareas.
- El botón de papelera mueve la lista completa a Papelera inmediatamente.

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

## Notas Markdown

- Cada tarea y subtarea tiene una nota propia. Clic derecho → **Agregar nota** abre el editor; si ya existe contenido, el menú ofrece **Ver nota**.
- Una fila con notas muestra un pequeño ícono de documento junto al área de su título. Púlsalo para abrir la nota. Una misma nota no abre ventanas duplicadas.
- La ventana adopta el color de la lista y la letra configurada. Arrastra su título para moverla y su esquina inferior derecha para cambiar el tamaño.
- **Editar** permite escribir o pegar Markdown, con saltos de línea y tabulaciones. **Visualizar** muestra títulos, negrita/cursiva/tachado, listas, casillas, citas, tablas, enlaces, separadores y bloques de código.
- Notas nuevas abren Editar; existentes abren Visualizar. Las casillas de la vista son de lectura y no completan tareas. Para cambiarlas, edita `[ ]` o `[x]` en el texto.
- Se guarda automáticamente. **Guardando…** indica una escritura pendiente y **Guardado** confirma la escritura de los datos. Si falla, se muestra el problema y puedes conservar una copia con Exportar.
- **Importar .md** lee un archivo local. Si la nota tiene contenido, pide confirmación antes de reemplazarlo; Cancelar conserva el texto anterior. **Exportar .md** guarda una copia del Markdown, sin modificar la nota.
- Los enlaces web o de correo se abren al pulsarlos. La vista no carga imágenes ni ejecuta HTML. Las referencias de imagen se representan como texto, conservando el Markdown original en el editor y la exportación.
- Cerrar con la X o Escape conserva el contenido. Ocultar la lista cierra sus ventanas de notas; trabajar dentro de una nota o sus diálogos de archivo no activa la minimización automática de esa lista.
- Completar, mover entre grupos, borrar/deshacer y restaurar una lista desde Papelera conservan las notas. Para quitar una nota, borra su contenido en Editar; desaparecerá el ícono de la fila.
- La actualización a 1.8.0 migra automáticamente al esquema 5. Las versiones 1.7.x y anteriores no leen ese formato; para volver a ellas se necesita un respaldo previo.

## Subtareas

- Clic derecho sobre una tarea principal → **Agregar subtarea** abre la captura debajo de ella. Enter agrega el paso y permite escribir el siguiente; el botón + también agrega. Escape cierra la captura.
- El contador **1/3**, entre la casilla y el título de la principal, indica pasos completados y totales. Pulsa su flecha para mostrar u ocultar los pasos; se recuerda el plegado al reiniciar.
- Doble clic edita el paso. Enter guarda y Escape cancela. Su papelera elimina solo ese paso y ofrece Deshacer durante cinco segundos.
- Arrastra el asa de una subtarea sobre otro paso de la misma principal para reordenarla. Aparece una copia de la fila junto al cursor y el origen se atenúa. La línea recta señala la inserción, también en los espacios entre pasos; al soltar se anima el cambio. Salir del área retira el indicador y cancelar restaura la fila. También puedes enfocar la fila y usar Alt+↑/↓. No se arrastran entre principales o grupos.
- Al terminar todos los pasos se completa automáticamente la principal. Marcar su casilla completa todo el conjunto y permite Deshacer. Reabrir un paso reabre la principal conservando los demás terminados; reabrir la principal reabre todos sus pasos.
- Clic derecho en una tarea o subtarea abre el menú junto al cursor y resalta su fila mientras está abierto. **Editar tarea/subtarea**, **Completar/Reabrir** y **Eliminar** indican qué elemento afectan. En una principal con pasos, completar/reabrir y eliminar afectan todo el conjunto. Las principales también ofrecen **Agregar subtarea** y **Mover a [grupo]**. Ya no se ofrece convertir tareas en subtareas.
- Mover una principal entre grupos mueve también sus pasos. Borrarla elimina el conjunto; Deshacer lo recupera completo. Papelera conserva y restaura los pasos junto con la lista.
- Los contadores de lista y grupo cuentan principales. Los pasos completados permanecen debajo de su principal, tachados. Vaciar completadas elimina principales terminadas y sus pasos, pero conserva pasos terminados de principales pendientes.

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
  El clic derecho abre el menú junto al cursor como alternativa, también en completadas.
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
| `Enter` / `Espacio` en el botón flotante | Acceso rápido, o listas ocultas sin asociación |
| `Shift+F10` / tecla Menú en el botón flotante | Mostrar acciones |
| `↓` en el botón flotante | Mostrar listas ocultas |
| `Enter` | Agregar tarea o confirmar edición según el foco |
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

### Actualizar a 1.3.0

Los datos anteriores se migran al esquema 4 sin perder listas, tareas o preferencias.
Conserva un respaldo anterior a la actualización si necesitas volver a 1.6.x o anteriores: no leen el esquema 4.

### Actualización histórica desde 1.0.x a 1.1.0

La primera carga migra automáticamente el esquema 1 a 2. Todas las tareas
anteriores quedan en General. Conserva un respaldo antes de actualizar si
necesitas volver a 1.0.x: esas versiones rechazan el esquema 2 sin sobrescribirlo.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.
