# Diseño aprobado de trabajo — Todo's

## Estado

Diseño vigente en Todo's 1.1.4. Los tamaños y tiempos documentados corresponden
a la implementación actual y podrán ajustarse en versiones posteriores sin
cambiar los flujos.

## Identidad

- Nombre visible: **Todo's**.
- Identificador técnico: `Todos`.
- Personalidad: tranquila, clara, amable y compacta.
- Tipografía: `Fredoka` incluida en el ejecutable; Windows usa sustitución de
  fuente únicamente para símbolos no presentes en la familia.
- Iconografía: geometrías vectoriales simples con trazos redondeados.

## Paleta inicial

| Tema | Fondo | Superficie | Acento | Texto |
|---|---|---|---|---|
| Mantequilla | `#FFF2B8` | `#FFF9DE` | `#D89A30` | `#332F2A` |
| Rosa | `#FFDCE5` | `#FFF2F5` | `#C96C87` | `#352C31` |
| Lavanda | `#E7DDF8` | `#F6F1FF` | `#7B6FC2` | `#302D3A` |
| Cielo | `#DCEEFF` | `#F0F8FF` | `#568FC2` | `#293440` |
| Menta | `#DDF5E8` | `#F0FBF5` | `#4E9B76` | `#29352F` |
| Durazno | `#FFE2CF` | `#FFF3EA` | `#C98257` | `#392F29` |
| Niebla | `#EAE8E5` | `#F7F6F4` | `#77716B` | `#302E2C` |

El tema inicial de nuevas listas será lavanda. El botón central usará lavanda
oscura con iconografía clara para seguir siendo reconocible sobre cualquier
fondo.

## Dimensiones iniciales

- Botón central: `52 × 52` unidades independientes de dispositivo.
- Ventana nueva: `410 × 420` unidades.
- Ventana mínima: `280 × 220` unidades.
- Esquinas: entre 16 y 22 unidades según el elemento.
- Acciones de icono: área interactiva mínima de 32 unidades; 40 para acciones
  principales.

## Composición de la ventana TODO

1. Encabezado arrastrable con título editable mediante doble clic/F2 y acciones.
2. Indicador breve de pendientes y progreso.
3. Campo compacto y redondeado para agregar una tarea, con etiqueta fija.
4. Lista desplazable de pendientes, con casilla, texto y acciones contextuales.
5. Sección plegable de completadas al final.
6. Mensaje temporal para deshacer una eliminación.
7. Control discreto de redimensionamiento en el borde inferior.

## Interacciones

- Clic izquierdo en el botón central abre/cierra su menú.
- Arrastrar el botón flotante o el encabezado de una lista mueve la ventana
  correspondiente. Los botones y el título en edición conservan su interacción.
- El botón flotante permanece por encima de las ventanas TODO sin quitarles el
  foco.
- **Listas ocultas** abre un submenú lateral con las listas y sus acciones de
  mostrar/eliminar; queda desactivado cuando no hay elementos.
- Reordenar muestra una guía estable del color Accent del tema antes de confirmar la posición.
- Doble clic sobre una tarea inicia edición; `Enter` confirma y `Esc` cancela.
- Completar mueve la tarea a la sección inferior con una transición breve.
- Eliminar una tarea ofrece “Deshacer” durante cinco segundos.
- Eliminar una lista abre confirmación.
- `Alt+F4` sobre una lista únicamente la oculta.
- El icono del área de notificación permite mostrar Todo's o salir guardando.

## Animación

- Hover/pressed: 100–140 ms.
- Apertura de menú: 160 ms.
- Entrada/salida de filas: 180 ms.
- Despliegue de completadas: 200 ms.
- Cambio de color: 220 ms.
- Sin animaciones repetitivas ni movimiento en reposo.
- Si Windows solicita menos animación, se reducen a cambios inmediatos o muy
  breves.

## Accesibilidad

- Todas las acciones con icono tendrán nombre accesible y tooltip.
- El foco será visible con un contorno de alto contraste.
- Color nunca será el único indicador de estado.
- `Enter`, `Esc`, tabulación y barra espaciadora cubrirán los flujos principales.
- El texto mantendrá contraste legible en los siete temas.

## Wireframes funcionales

```text
Botón cerrado          Menú abierto                  Submenú lateral
    [icono]            ┌─────────────────────────┐  ┌────────────────────┐
                       │ Todo's                  │  │ Proyecto API   [×] │
                       │ + Nueva lista           │  │ Frontend       [×] │
                       │ ◉ Mostrar todas         │  └────────────────────┘
                       │ ◌ Ocultar todas         │
                       │ Listas ocultas (2)    › │
                       │ Iniciar con Windows  ☑  │
                       │ Salir                   │
                       └─────────────────────────┘
```

```text
┌────────────────────────────────────────┐
│ Proyecto API   [color][—][borrar]│
│ 3 pendientes · 2 completadas           │
│ Nueva tarea                             │
│ ┌────────────────────────────────────┐ │
│ │ Escribe la siguiente…          [+] │ │
│ └────────────────────────────────────┘ │
│ □ Crear endpoint               [borrar]│
│ □ Implementar vista            [borrar]│
│ □ Ejecutar pruebas        [asa] [borrar]│
│                                        │
│ › Completadas (2)                      │
└────────────────────────────────────────┘
```

## Agrupaciones — 1.1.1

- Captura en una sola fila: entrada, botón de agrupación y agregar. **+ Grupo**
  vive en el menú lateral; selección y traslado reutilizan el estilo de Listas ocultas.
- General primero; chevron para plegar, nombre editable del color Accent del tema,
  contador y papelera en los grupos personalizados. Papelera abre confirmación.
- Se conserva el plegado de cada grupo al reiniciar. El asa usa move-up-down.svg.
- Los grupos vacíos siguen visibles como destinos de arrastre.
- Las filas muestran papelera y asa de arrastre; no hay botón de traslado a grupo.
- Tarjeta a tamaño real con casilla, texto e iconos, origen atenuado y marca en el lado real de inserción. Tanto borde como barra usan Accent y la tarjeta usa Surface del tema.
- Las filas animan su cambio de posición al soltar durante 180 ms.

### Ajuste 1.1.3 — 2026-09-17

Botón de mover a agrupación retirado de las tareas. Colores e interacciones
ligados al tema de cada ventana, incluidos hover de agrupación y agregar,
presionado, foco, casillas, campos, iconos, menús y deshacer.

Ajuste 1.1.4: papeleras, acciones de eliminar y errores conservan el rojo
semántico TodosDangerBrush (#A54459), independiente del tema de la ventana.
