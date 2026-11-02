# Plan de trabajo — Todo's

## Versión 1.8.2 — recuperación de clics fuera (2026-10-14)

Reporte: minimización al pulsar dentro y menú del principal que se cierra solo, posiblemente tras tiempo/hibernación; reiniciar restaura el funcionamiento. Reemplazar límites WPF por HWND/bounds físicos, renovar hook al reanudar/cambiar pantallas e invalidar clics previos. Validar escalas, layout nativo pendiente, menús/popups, clic externo y mensajes de reanudación simulados en segundo monitor. No hibernar la computadora del usuario. Conservar datos y esquema 5.

Completado: build limpio, 65/65 pruebas, reproducción controlada del desfase por resize nativo anterior al layout y corrección verificada. Mensajes de recuperación simulados y descarte de acciones antiguas comprobados. Portable 1.8.2.0 extraído y hashes/inicio/reinicio verificados con datos aislados. Hibernación real y disparador exacto del reporte pendientes del usuario.

## Versión 1.8.1 — refinamiento de arrastre de subtareas

Homologar preview, atenuación y feedback con tareas principales; indicador recto y estable sin modificar layout, márgenes válidos como destino y limpieza al cancelar/soltar. Conservar reordenamiento solo dentro de principal y preservar notas. Validación por eventos/renders WPF en segundo monitor y portable.

## Versión 1.8.0 — notas Markdown autorizadas 2026-10-12

Alcance aprobado: una nota por tarea/subtarea, editor y visualizador en pestañas, sin imágenes. Acceso contextual e ícono; guardado automático, importar/exportar .md, preservar en grupos, completado, deshacer y Papelera. Ventana con tema de lista y letra global; familia de interacción para minimización automática. Esquema 5 con migración de 1–4.

Completado: build limpio, 65/65 pruebas, formato y flujos WPF revisados en segundo monitor. ZIP 1.8.0.0 extraído y hashes correctos; migración y reinicio nativos conservaron árboles, notas exactas y preferencias. Evidencia en artifacts/notes-1.8.0-tests.txt y artifacts/notes-1.8.0-package/verification.txt. Interacción física de ratón pendiente.

## Versión 1.7.1 — revisión de menús y compactación 2026-10-12

Menú junto al cursor, fila resaltada, textos explícitos y acciones comunes de tareas/subtareas. Retirar conversión de la interfaz. Avance y flecha junto al checkbox; ocultar etiqueta del launcher al arrastrar. Validar posición/foco/acciones y portátil en datos aislados; ventanas de revisión en monitor secundario.

Completado: build limpio, 60/60 pruebas, renders WPF revisados y monitor secundario comprobado al cargar. ZIP 1.7.1.0 extraído con hashes correctos; dos inicios nativos conservaron esquema, preferencias, asociación, grupos y árboles. Evidencia en artifacts/context-1.7.1-tests.txt y artifacts/context-1.7.1-package/verification.txt. Recorrido físico de ratón pendiente.

## Versión 1.7.0 — subtareas autorizadas 2026-10-12

Un nivel, progreso, plegado, captura consecutiva y edición; completado automático y de conjunto con deshacer. Reordenamiento dentro de la principal, conversión desde tarea existente del mismo grupo sin pasos propios. Preservar árboles en grupos, borrado/deshacer y papelera, limpieza limitada a raíces completadas. Esquema 4, persistencia y pruebas de interfaz antes de preparar el portable.

Completado localmente: build limpio, 60/60 pruebas, vistas WPF revisadas, portable 1.7.0.0 con hashes correctos. Migración y guardado/reinicio nativos verificados con datos aislados, conservando árboles, IDs, avance y plegado. Revisión física de arrastre y escalas mixtas pendiente.

## Versión 1.6.0 — apariencia autorizada 2026-10-12

Persistir letra, tamaño del principal y densidad general. Controles de apariencia en Configuración, recursos dinámicos globales y actualización de notificación. Mantener valores anteriores por defecto, clamping y proporciones del botón. Validar cambios en vivo, extremos, retorno a Mediano y reinicio con datos aislados antes de entregar el portable.

Completado localmente: build limpio, 54/54 pruebas, renders WPF revisados, portable 1.6.0.0 y hashes verificados. Arranque/reinicio con apariencia grande guardada aprobado en datos aislados. Render nativo transparente; revisión física y escalas mixtas pendientes.

## Versión 1.5.0 — minimizar principal autorizado 2026-10-10

Quinta acción circular entre Salir y Configuración. Ocultar principal sin cerrar proceso ni listas. Reemplazar Mostrar Todo's del área de notificación por Mostrar botón / Ocultar botón dinámico. Validar cierre de popups, asociación, recuperación, menú de notificación y portable con datos aislados.

Completado localmente: build limpio, 52/52 pruebas, renders WPF revisados y portable 1.5.0.0 extraído con hashes verificados. Arranque nativo aprobado con datos aislados. Captura del menú en modo render del portable incompleta; interacción física del área de notificación y apariencia del menú portable pendientes.

## Versión 1.4.0 — ajustes y grupos autorizados 2026-10-10

Acercar menú de listas ocultas, compactar papelera y corregir restaurar. Añadir subir/bajar grupos personalizados, acceso explícito para renombrar y vaciar completadas con deshacer de lote. Validar persistencia, regresiones, vistas WPF y portable.

Completado localmente: build limpio, 51/51 pruebas, vistas revisadas y portable 1.4.0.0 extraído, hashes correctos, arranque y render nativos aprobados con datos aislados. Revisión física de ratón/multimonitor pendiente.

## Versión 1.3.1 — ícono circular autorizado 2026-10-10

Adaptar SVG/PNG/ICO a círculo y generar cada resolución desde vector. Verificar
transparencia, recurso del ejecutable, regresiones y paquete portable.

## Versión 1.3.0 — papelera autorizada 2026-10-10

1. Persistir listas borradas completas y migrar esquema 2 a 3.
2. Borrar directamente desde ventana y menú contextual de listas ocultas.
3. Cuarta acción Papelera, restauración y eliminación definitiva con UI propia.
4. Verificar snapshot/reinicio, regresiones WPF, renders y paquete portable.

Estado: completado localmente; build limpio, 49/49 pruebas, renders WPF y portable
1.3.0.0 verificado con datos aislados. Revisión física de ratón/multimonitor pendiente.

Los apartados siguientes documentan versiones anteriores y el alcance histórico
del MVP. La papelera de listas reemplaza la eliminación irreversible del MVP.

## Versión 1.2.3 — ajustes autorizados 2026-10-10

Homologar los botones flotantes con checks blancos más pequeños, fondos de
acento y bordes finos. Conservar las ventanas de tareas. Build limpio y 45/45
pruebas aprobadas; revisión de renders y paquete portable.

## Versión 1.2.2 — ajustes autorizados 2026-10-10

Oscurecer ligeramente botones de listas, checks negros y recuperar
checkbox-checked.svg en configuración activada. Validar WPF y paquete portable.

## Versión 1.2.1 — ajustes autorizados 2026-10-10

1. Etiquetas laterales de mayor tamaño; principal solo en hover; espera de un segundo.
2. Configuración centrada y sin acciones secundarias al abrir; scroll delgado.
3. Igualar el fondo del principal al de la lista y usar check.svg en botones y tareas.
4. Evitar reapertura cuando el clic principal minimiza una asociación visible.
5. Validar regresión, renders WPF, compilación y paquete portable actualizado.

Estado: implementación completada, build limpio y 45/45 pruebas aprobadas.
Evidencia local en progreso.md; revisión física de mouse y monitores pendiente.

## Versión 1.2.0 — alcance autorizado 2026-10-10

1. Persistir la última lista minimizada y la opción de minimización automática
   (desactivada inicialmente), conservando datos de esquema 2.
2. Sustituir el botón PNG por círculo y check vectoriales. Clic izquierdo recupera
   la lista asociada; sin asociación muestra las ocultas. Hover de 2 segundos
   muestra las ocultas. Clic derecho muestra Salir, Configuración y Nueva lista.
3. Mostrar botones redondos con etiquetas, animaciones y adaptación a pantalla.
   Las acciones de clic derecho prevalecen sobre hover. Configuración conserva
   el menú actual solo para inicio con Windows y minimización automática.
4. Ocultar listas al hacer clic fuera cuando la preferencia esté activa, respetando
   menús, diálogos y gestos internos. Eliminar la lista asociada vuelve al morado
   original sin elegir otra. Conservar asociación al mostrarla y al reiniciar.
5. Validar persistencia, interacciones WPF, revisión de renders y paquete 1.2.0;
   actualizar documentación y distinguir pruebas locales de recorrido físico.

Estado: implementación y validación local completadas. Release limpio, 44/44
pruebas, renders y ZIP autocontenido verificados. La revisión física de gestos,
clic fuera con otras aplicaciones y monitores de DPI mixto queda pendiente.
Evidencia y detalle de las etapas en progreso.md, sección de versión 1.2.0.

> **Estado de ejecución (actualizado 2026-09-17): completado.** Las etapas 0 a 11 fueron
> implementadas y validadas en orden. La evidencia, comandos y resultados están
> registrados en `/.codex/progreso.md`. El plan conserva la línea base 1.0.0
> como registro histórico; los ajustes posteriores 1.0.1–1.0.3 también están
> documentados ahí. Las ampliaciones 1.1.0 a 1.1.2 están al final; los artefactos
> vigentes se encuentran en `/publish`.

## 1. Propósito del documento

Este documento será la guía de implementación del programa descrito en
`/.codex/context.md`. Su objetivo es dividir el desarrollo en etapas pequeñas,
verificables y ordenadas, evitando avanzar a una fase sin haber validado la
anterior.

El plan todavía no representa código implementado. Cuando se autorice el inicio
del desarrollo, se seguirá este documento como fuente principal del alcance. Si
durante el proyecto cambia un requisito, primero se actualizará este plan y
después se modificará el código.

---

## 2. Resumen de etapas

| Etapa | Objetivo principal | Resultado verificable |
|---|---|---|
| 0 | Cerrar decisiones funcionales y visuales | Requisitos y bocetos aprobados |
| 1 | Preparar la solución y sus convenciones | Aplicación vacía que compila y abre |
| 2 | Definir dominio y persistencia | Datos guardados y restaurados de forma segura |
| 3 | Construir controles permanentes | Botón flotante e icono de notificación funcionales |
| 4 | Construir las ventanas TODO | Crear, mover, redimensionar, ocultar y borrar listas |
| 5 | Implementar tareas y progreso | Agregar, completar, reabrir y reordenar tareas |
| 6 | Aplicar el sistema visual y animaciones | Interfaz consistente, moderna y fluida |
| 7 | Robustecer ciclo de vida y monitores | Restauración confiable y posiciones válidas |
| 8 | Incorporar accesibilidad y facilidad de uso | Uso claro con ratón y teclado |
| 9 | Ejecutar pruebas integrales | Casos críticos aprobados sin pérdida de datos |
| 10 | Publicar la versión portable | Ejecutable autocontenido probado en equipo limpio |
| 11 | Documentar y preparar evolución | Manual, decisiones y backlog finalizados |

Las etapas se realizarán en ese orden. Cada una termina con una puerta de
revisión; no se continúa hasta cumplir sus criterios de aceptación.

---

## 3. Visión del producto

Crear **Todo's**, una utilidad de escritorio para Windows, pequeña y rápida, que permita
mantener visibles listas breves de pasos mientras el usuario trabaja en otras
aplicaciones. El producto debe sentirse como una combinación entre Notas rápidas
y una lista de tareas, pero enfocada en procesos cortos que requieren recordar el
siguiente paso.

El programa estará compuesto por:

1. Un botón central flotante, siempre visible y movible.
2. Cero o más ventanas TODO independientes, también flotantes.
3. Persistencia local para recuperar contenido, apariencia, visibilidad, tamaño
   y posición.
4. Una distribución portable que no dependa de un servidor ni de una cuenta.
5. Un icono de Todo's en el área de notificación de Windows para identificar que
   el programa continúa ejecutándose en segundo plano y permitir cerrarlo de
   forma segura.

### Principios del producto

- Capturar una tarea debe tomar pocos segundos.
- El siguiente paso pendiente debe ser evidente de un vistazo.
- La interfaz debe ocupar poco espacio y no interrumpir el trabajo.
- Ninguna acción normal debe provocar pérdida silenciosa de información.
- La estética es parte del alcance, no una mejora opcional al final.
- Las animaciones deben comunicar cambios, sin hacer lenta la aplicación.
- El programa debe funcionar completamente sin conexión a Internet.

---

## 4. Alcance funcional de la primera versión

### 4.1 Botón central

El botón central deberá:

- Permanecer por encima de las ventanas normales de Windows.
- Poder moverse mediante arrastre.
- Recordar su última posición al cerrar y volver a abrir el programa.
- Abrir y cerrar un menú compacto.
- Crear una nueva ventana TODO.
- Mostrar la lista de ventanas ocultas.
- Volver a mostrar una ventana oculta de forma individual.
- Ocultar todas las ventanas TODO visibles.
- Mostrar todas las ventanas TODO ocultas.
- Permitir activar o desactivar “Iniciar Todo's con Windows” desde el menú o su
  sección de configuración.
- Cerrar por completo la aplicación desde una acción explícita.
- Comunicar visualmente cuándo el menú está abierto.
- Evitar que un arrastre accidental se interprete como clic.
- Usar inicialmente un tamaño aproximado de 52 × 52 unidades independientes de
  dispositivo; el tamaño se ajustará durante la validación visual.

### 4.2 Ventanas TODO

Cada ventana deberá:

- Tener un título editable.
- Mantenerse siempre visible sobre las ventanas normales.
- Moverse libremente por la pantalla.
- Poder redimensionarse dentro de límites mínimos razonables.
- Recordar su posición y tamaño.
- Mostrar su color o tema seleccionado.
- Permitir cambiar entre una paleta pastel predefinida.
- Permitir ocultarse sin eliminarse.
- Permitir eliminarse mediante una confirmación clara.
- Mostrar las tareas pendientes.
- Mostrar u ocultar la sección de tareas completadas.
- Reflejar el progreso de manera discreta.
- Conservar su estado aunque la aplicación se cierre inesperadamente, siempre
  que el último cambio ya haya podido guardarse.

### 4.3 Tareas

El usuario deberá poder:

- Agregar una tarea con texto breve.
- Editar una tarea existente.
- Confirmar la creación con teclado y con ratón.
- Marcar una tarea como completada.
- Regresar una tarea completada a pendientes.
- Reordenar las tareas pendientes mediante arrastre.
- Eliminar una tarea de forma intencional.
- Ver las tareas completadas en una sección separada.
- Colapsar o desplegar la sección de completadas.
- Deshacer durante un periodo breve la eliminación de una tarea individual.

### 4.4 Restauración al iniciar

Cuando se vuelva a abrir la aplicación:

- El botón central aparecerá en su última posición válida.
- Se recrearán todas las listas conservadas.
- Solo se mostrarán automáticamente las listas que estaban visibles al cerrar.
- Las listas ocultas seguirán disponibles desde el botón central.
- Cada lista visible recuperará posición, tamaño, color, título, tareas, orden y
  estado de la sección de completadas.
- Si cambió la configuración de monitores, ningún elemento deberá quedar fuera
  del área visible.

### 4.5 Integración con Windows

- Todo's será compatible con Windows 10 de 64 bits y Windows 11 de 64 bits. La
  versión mínima concreta y su compatibilidad con el .NET elegido se verificarán
  al iniciar la Etapa 1.
- El botón central y las ventanas TODO no aparecerán como aplicaciones
  independientes en la barra de tareas.
- Mientras Todo's esté ejecutándose, mostrará un único icono en el área de
  notificación de Windows, accesible desde la zona donde Windows agrupa los
  iconos en segundo plano.
- La ubicación exacta del icono —visible directamente o dentro del menú del
  chevrón— depende de la configuración de Windows y del usuario; la aplicación
  no intentará forzarla.
- El menú del icono permitirá como mínimo abrir/mostrar Todo's y salir.
- La acción “Salir” del icono guardará el estado antes de terminar el proceso,
  igual que la acción equivalente del botón central.
- Todo's ofrecerá una opción para activar o desactivar su ejecución al iniciar
  sesión en Windows.
- La ejecución automática estará activada por defecto en la primera ejecución.
- Esta integración se realizará para el usuario actual y sin requerir permisos
  de administrador ni instalador.

---

## 5. Fuera de alcance inicial

Para mantener la primera versión compacta, quedan fuera del MVP:

- Sincronización en la nube.
- Inicio de sesión o cuentas de usuario.
- Aplicaciones para macOS, Linux, Android o iOS.
- Colaboración entre varias personas.
- Fechas límite, recordatorios o notificaciones programadas.
- Subtareas, etiquetas, categorías o proyectos complejos.
- Archivos adjuntos, Markdown enriquecido o imágenes en tareas.
- Integraciones directas con Codex u otras aplicaciones.
- Historial completo de cambios o papelera recuperable.
- Actualizaciones automáticas.
- Temas creados libremente mediante selector RGB.

Estos puntos podrán evaluarse después del MVP y no deben introducirse de forma
anticipada durante la implementación.

---

## 6. Base técnica propuesta

### 6.1 Tecnología

- Lenguaje: C#.
- Plataforma: .NET para Windows, usando una versión estable con soporte vigente
  al comenzar la implementación.
- Interfaz: WPF con XAML.
- Patrón de presentación: MVVM.
- Persistencia inicial: archivo JSON local, versionado mediante un campo de
  versión de esquema.
- Distribución final: publicación autocontenida para Windows x64, preferentemente
  como un solo ejecutable o como un paquete portable pequeño si las pruebas de
  publicación muestran que es más confiable.
- Identidad visible: `Todo's`.
- Identificadores técnicos internos: nombres seguros sin apóstrofo, por ejemplo
  `Todos`, para namespace, proceso, carpeta de datos y entrada de inicio.

La versión exacta de .NET y las dependencias se fijarán en la Etapa 1, después de
verificar el entorno disponible y el soporte vigente. No se adoptarán librerías
visuales antes de comprobar su mantenimiento, licencia y compatibilidad.

### 6.2 Estructura lógica prevista

La solución deberá separar responsabilidades desde el inicio:

```text
Todos.sln
└── src/
    └── Todos.App/
        ├── Application/       Coordinación de casos de uso
        ├── Domain/            Entidades y reglas del producto
        ├── Infrastructure/    Archivos, sistema operativo y serialización
        ├── Presentation/
        │   ├── ViewModels/
        │   ├── Views/
        │   ├── Controls/
        │   ├── Behaviors/
        │   ├── Converters/
        │   └── Themes/
        ├── Assets/            Fuentes, iconos y recursos propios
        └── App.xaml
```

La estructura podrá ajustarse si WPF requiere una distribución distinta, pero
se conservarán estas fronteras:

- Las vistas no decidirán cómo se guardan los datos.
- El modelo de dominio no dependerá de controles de WPF.
- La interacción con monitores, ventanas y archivos estará encapsulada.
- Los estilos visuales estarán centralizados y no repetidos por ventana.

### 6.3 Servicios previstos

- `IAppStateRepository`: cargar y guardar el estado completo.
- `IWindowStateService`: capturar y restaurar posición, tamaño y visibilidad.
- `IDisplayService`: consultar monitores y asegurar posiciones visibles.
- `IAppLifecycleService`: coordinar inicio, guardado y cierre real.
- `IWindowManager`: crear, mostrar, ocultar y destruir ventanas TODO.
- `INotificationAreaService`: crear el icono de segundo plano y manejar su menú.
- `IStartupRegistrationService`: consultar, activar y desactivar el inicio con
  Windows para el usuario actual.
- `IDialogService`: confirmaciones coherentes con el diseño.
- `IClock`: proporcionar fechas de creación/modificación sin acoplar pruebas al
  reloj del sistema.

Los nombres finales pueden cambiar, pero no sus responsabilidades.

---

## 7. Modelo de datos previsto

### 7.1 Estado general

```text
AppState
- SchemaVersion
- LauncherState
- Settings
- TodoWindows[]
- LastSavedAt
```

### 7.2 Botón central

```text
LauncherState
- PositionX
- PositionY
- DisplayId opcional
```

### 7.3 Configuración

```text
AppSettings
- RunAtWindowsStartup = true
- HasSeenFirstRunHint
```

El valor persistido expresa la preferencia del usuario. Durante la carga se
comparará con el registro real de inicio de Windows para detectar si el sistema o
el usuario lo desactivaron externamente.

### 7.4 Ventana TODO

```text
TodoWindowState
- Id
- Title
- ColorTheme
- PositionX
- PositionY
- Width
- Height
- IsVisible
- AreCompletedTasksExpanded
- Tasks[]
- CreatedAt
- UpdatedAt
```

### 7.5 Tarea

```text
TodoTask
- Id
- Text
- IsCompleted
- PendingOrder
- CompletedAt opcional
- CreatedAt
- UpdatedAt
```

### 7.6 Reglas de integridad

- Todos los identificadores serán únicos y estables.
- El título no podrá guardarse como texto vacío; se aplicará un título por
  defecto si el usuario lo deja en blanco.
- El título será de una sola línea y tendrá un máximo inicial de 80 caracteres.
- Una tarea no podrá crearse con texto vacío o compuesto solo por espacios.
- El texto de una tarea tendrá un máximo inicial de 300 caracteres. Será una sola
  entrada lógica, aunque visualmente pueda ocupar varias líneas por ajuste de
  texto.
- Se normalizarán espacios al inicio y final sin alterar el contenido interno.
- El orden de pendientes será determinista y consecutivo después de reordenar,
  completar, reabrir o eliminar.
- Las tareas completadas conservarán un orden predecible, inicialmente por fecha
  de finalización más reciente.
- Al reabrir una tarea completada, se colocará al final de las pendientes.
- Las dimensiones se limitarán a mínimos y máximos válidos.
- Las coordenadas guardadas siempre se validarán antes de mostrarse.
- Los valores de tema desconocidos caerán en un tema predeterminado.
- Un archivo de una versión anterior deberá migrarse o abrirse con valores por
  defecto seguros; nunca se asumirá que todos los campos existen.

---

## 8. Estrategia de persistencia

### 8.1 Ubicación

Los datos de usuario se almacenarán fuera de la carpeta del ejecutable, dentro de
la ubicación de datos locales de aplicación de Windows para Todo's. Esto permite que la aplicación
siga siendo portable como distribución, sin exigir permisos de escritura en su
directorio ni perder datos al reemplazar el ejecutable.

Antes de implementar se definirá y documentará la ruta exacta. También se
evaluará ofrecer posteriormente un modo verdaderamente portable que guarde junto
al ejecutable cuando exista un archivo de configuración explícito, pero ese modo
no forma parte del MVP.

La distribución portable no significa que los datos viajen junto con el
ejecutable: el binario podrá copiarse o reemplazarse, mientras las listas se
mantendrán en el perfil del usuario.

### 8.2 Cuándo guardar

Se aplicará guardado automático después de cambios relevantes:

- Crear, ocultar, mostrar o eliminar una ventana.
- Cambiar título o color.
- Agregar, completar, reabrir, eliminar o reordenar tareas.
- Cambiar el estado de la sección de completadas.
- Terminar el movimiento o redimensionamiento de una ventana.
- Terminar el movimiento del botón central.
- Cerrar correctamente la aplicación.

Para movimientos, redimensionamientos y escritura de texto se usará una espera
corta antes de guardar, evitando escribir el archivo en cada píxel o pulsación.

### 8.3 Escritura segura

El repositorio de estado deberá:

1. Serializar el estado en memoria.
2. Escribir primero a un archivo temporal en la misma ubicación.
3. Forzar el cierre correcto del flujo.
4. Reemplazar de forma atómica el archivo principal cuando sea posible.
5. Conservar una copia de respaldo reciente.
6. No sobrescribir el último estado válido si la serialización falla.

Al iniciar:

- Se intentará cargar el archivo principal.
- Si está corrupto, se intentará la copia de respaldo.
- Si ninguno funciona, se iniciará con estado vacío y se informará de forma no
  intrusiva; los archivos dañados no se eliminarán automáticamente.
- Los errores se registrarán localmente sin incluir contenido sensible de las
  tareas salvo que sea indispensable para diagnosticar.

### 8.4 Concurrencia

- El programa funcionará como instancia única.
- Si se intenta abrir una segunda instancia, esta activará o hará visible el
  botón central de la instancia existente y terminará.
- Solo una operación de guardado podrá escribir el archivo a la vez.
- Los cambios acumulados durante una escritura generarán un guardado posterior,
  evitando perder el último estado.

---

## 9. Sistema de diseño

### 9.1 Dirección visual

La interfaz buscará una apariencia ligera, cálida y moderna:

- Ventanas compactas sin la barra tradicional de Windows.
- Esquinas redondeadas y sombras suaves.
- Fondos pastel con contraste suficiente.
- Jerarquía tipográfica clara.
- Iconos vectoriales simples y consistentes.
- Espaciado uniforme y controles con áreas de interacción cómodas.
- Estados de foco, selección, arrastre y deshabilitado claramente visibles.
- Ausencia de bordes, degradados o efectos decorativos innecesarios.

### 9.2 Tokens visuales

Antes de construir todas las vistas se definirán recursos compartidos para:

- Paleta de temas pastel.
- Colores de texto primario, secundario y deshabilitado.
- Colores de éxito, advertencia, peligro y foco.
- Tipografía, tamaños y pesos.
- Escala de espaciado.
- Radios de esquina.
- Elevaciones y sombras.
- Duraciones y curvas de animación.
- Tamaños de iconos y botones.
- Alturas mínimas de campos y filas.

No se escribirán colores, fuentes o medidas repetidas directamente en cada
control cuando puedan vivir como recursos del sistema de diseño.

### 9.3 Paleta inicial a explorar

Se prepararán entre cinco y ocho variantes pastel, por ejemplo:

- Amarillo mantequilla.
- Rosa suave.
- Lavanda.
- Azul cielo.
- Verde menta.
- Durazno.
- Gris cálido.

Cada variante debe definir fondo, superficie secundaria, acento, texto y borde.
No bastará con cambiar únicamente el fondo; se verificará contraste y legibilidad
en todos los estados.

### 9.4 Tipografía e iconos

- Se elegirá una tipografía con licencia compatible para incluirla dentro del
  ejecutable o sus recursos.
- Se verificará legibilidad en escalas de Windows de 100 %, 125 %, 150 % y 200 %.
- Los iconos serán SVG/Geometry o una fuente de iconos con licencia adecuada.
- Todos los iconos importantes tendrán descripción accesible.
- No se usarán emojis como sustituto de iconos funcionales principales.

### 9.5 Animaciones

Se contemplan animaciones breves para:

- Apertura y cierre del menú central.
- Aparición de una nueva ventana TODO.
- Inserción y eliminación de tareas.
- Paso de una tarea a completadas y viceversa.
- Despliegue de la sección de completadas.
- Cambio de tema.
- Estados hover, pressed y foco.
- Indicación del punto de inserción al reordenar.

Reglas:

- Las animaciones no bloquearán la interacción ni retrasarán el guardado.
- El estado final será correcto aunque una animación se interrumpa.
- Se respetará la preferencia de Windows de reducir animaciones cuando sea
  posible.
- Se evitarán desenfoques o sombras costosas en listas grandes.

---

## 10. Experiencia de usuario detallada

### 10.1 Primera ejecución

1. Se abre únicamente el botón central en una posición segura.
2. Una indicación breve explica que puede moverse y usarse para crear una lista.
3. La ayuda desaparece después de la primera interacción y no vuelve a mostrarse.
4. El usuario puede crear la primera ventana sin configurar nada.
5. Se registra el inicio automático de Todo's para el usuario actual, porque la
   preferencia inicial estará activada.

### 10.2 Crear una ventana

1. El usuario abre el menú central.
2. Selecciona “Nueva lista”.
3. Se crea una ventana cerca del botón central o en una zona visible libre.
4. El título entra en modo de edición con un valor sugerido.
5. El campo para la primera tarea queda disponible inmediatamente.
6. La ventana se guarda desde su creación, incluso si permanece vacía.

Si la posición sugerida se superpone completamente con otra lista, se aplicará
un desplazamiento escalonado. Siempre se priorizará mantener la ventana dentro
del área de trabajo visible.

### 10.3 Agregar tareas

- Existirá una entrada compacta claramente reconocible.
- `Enter` agregará la tarea.
- Una entrada vacía no realizará ninguna acción ni mostrará un error agresivo.
- Después de agregar, el foco regresará al campo para permitir captura continua.
- La nueva tarea se añadirá al final de pendientes.

### 10.4 Editar tareas

- La edición estará disponible mediante doble clic y mediante una acción de
  edición visible al interactuar con la fila.
- La edición reutilizará las mismas reglas de longitud y texto vacío que la
  creación.
- `Enter` guardará la edición y `Esc` la cancelará.
- Perder el foco guardará únicamente si el contenido es válido; el comportamiento
  exacto se comprobará en el prototipo para evitar cambios accidentales.
- Editar una tarea no modificará su estado, posición ni fecha de finalización.
- El cambio se persistirá al confirmarlo.

### 10.5 Completar y reabrir

- Marcar la casilla iniciará una transición corta.
- La tarea dejará pendientes y aparecerá en completadas.
- Si la sección de completadas está cerrada, su contador se actualizará sin
  abrirla automáticamente.
- Las completadas se ordenarán de la más recientemente completada a la más antigua.
- Reabrir una tarea la colocará al final de pendientes.
- La operación se guardará inmediatamente.

### 10.6 Reordenar

- Solo las tareas pendientes podrán reordenarse en el MVP.
- La fila arrastrada y el destino tendrán indicadores visibles.
- El desplazamiento automático funcionará cuando la lista tenga scroll.
- Al soltar, se actualizará el orden y se guardará una sola vez.
- Cancelar el arrastre conservará el orden anterior.
- Se deberá ofrecer una alternativa por teclado si resulta razonable durante la
  fase de accesibilidad.

### 10.7 Ocultar y mostrar

- Ocultar una lista no cambia su contenido.
- El menú central mostrará las listas ocultas usando su título y color.
- Seleccionar una lista oculta la restaurará en su última posición válida.
- “Ocultar todas” no ocultará el botón central.
- “Mostrar todas” respetará la posición de cada lista y corregirá únicamente las
  que hayan quedado fuera de pantalla.

### 10.8 Eliminar

- La acción será distinta visualmente de ocultar.
- Requerirá confirmación con el título de la lista.
- La confirmación ofrecerá “Eliminar” y “Cancelar”, siendo cancelar la opción
  segura predeterminada.
- Una vez confirmada, se cerrará la ventana, se eliminará del estado y se
  guardará.
- Para el MVP no habrá papelera; esto deberá explicarse en la confirmación.
- Eliminar una tarea individual no abrirá una confirmación. La fila desaparecerá
  y se mostrará durante unos segundos una acción “Deshacer”.
- Si se elige “Deshacer”, la tarea recuperará su estado y posición anteriores.
- Al vencer el periodo de deshacer, la eliminación quedará confirmada. No habrá
  historial ni papelera permanente.

### 10.9 Icono en el área de notificación

- El icono existirá mientras el proceso esté en ejecución, aunque todas las
  listas estén ocultas.
- Su menú contextual incluirá como mínimo “Mostrar Todo's” y “Salir”.
- “Mostrar Todo's” recuperará el botón central y permitirá acceder a las listas;
  no creará una segunda instancia.
- “Salir” ejecutará exactamente el mismo flujo de guardado y cierre seguro que el
  botón central.
- Los menús del botón y del icono no mantendrán estados contradictorios.
- El icono tendrá tooltip y nombre accesible `Todo's`.

### 10.10 Inicio con Windows

- La opción se mostrará como un interruptor “Iniciar Todo's con Windows”.
- El interruptor será accesible desde el menú central, sin requerir editar
  archivos ni abrir herramientas de Windows.
- Estará activada por defecto en la primera ejecución.
- Activarla registrará el ejecutable actual para el usuario que inició sesión.
- Desactivarla eliminará únicamente la entrada propia de Todo's.
- El registro incluirá un argumento que identifique el inicio automático, para
  distinguirlo de una apertura manual durante diagnóstico y recuperación.
- Si el ejecutable se mueve, la entrada anterior puede dejar de funcionar. En una
  apertura manual, Todo's detectará una ruta desactualizada y podrá corregirla
  manteniendo la preferencia del usuario.
- Si Windows o el usuario deshabilitan externamente la aplicación de inicio, la
  interfaz deberá reflejar el estado real o informar la diferencia, sin crear
  entradas repetidamente durante un arranque automático.

### 10.11 Cerrar la aplicación

- Cerrar una ventana TODO mediante su control de ocultar solo la ocultará.
- `Alt+F4` sobre una ventana TODO solo la ocultará.
- La salida completa se realizará desde el botón central o desde el icono del
  área de notificación.
- Al seleccionar “Salir”, se capturará el estado final de todas las ventanas.
- Se esperará un tiempo acotado a que finalice el guardado.
- Si el guardado falla, se informará y se permitirá cancelar la salida para no
  perder cambios.

---

## 11. Comportamiento de ventanas y monitores

### 11.1 Ventanas siempre visibles

- Botón central y listas usarán comportamiento superior (`Topmost`).
- Se verificará que una lista no robe foco continuamente a otras aplicaciones.
- Crear o editar sí podrá activar temporalmente la ventana correspondiente.
- Los diálogos propios deberán aparecer por encima de la ventana que los abrió.

### 11.2 Movimiento

- Las zonas arrastrables estarán definidas y no interferirán con botones o texto.
- Se guardará la posición al finalizar el arrastre.
- Se permitirá mover entre monitores.
- Se conservará una franja mínima accesible dentro del área visible.
- Se decidirá en prototipo si existe ajuste suave a los bordes; no se implementará
  sin validar que no resulte molesto.

### 11.3 Redimensionamiento

- La ventana tendrá un tamaño mínimo que mantenga utilizables título, acciones y
  campo de tarea.
- El tamaño máximo no superará innecesariamente el área de trabajo disponible.
- El contenido central utilizará scroll cuando no quepa.
- El redimensionamiento podrá realizarse desde bordes o un indicador discreto.
- El tamaño se persistirá al finalizar la operación.

### 11.4 Cambios de configuración

Se probarán estos escenarios:

- Desconectar un monitor secundario mientras la aplicación está abierta.
- Cerrar usando dos monitores y abrir usando solo uno.
- Cambiar cuál monitor es principal.
- Cambiar resolución.
- Cambiar escala de pantalla.
- Usar monitores colocados a la izquierda o arriba del principal, generando
  coordenadas negativas.
- Suspender y reanudar Windows.

La regla general será mover lo mínimo necesario para recuperar el elemento; no
se deben resetear todas las posiciones por un solo cambio de monitor.

---

## 12. Plan de implementación por etapas

## Etapa 0 — Definición funcional y visual

### Objetivo

Cerrar decisiones que afectan directamente la arquitectura o la experiencia
antes de escribir la aplicación.

### Actividades

- Revisar este plan con el usuario.
- Mantener `Todo's` como nombre visible y `Todos` como identificador técnico.
- Validar el soporte real de Windows 10 x64 y Windows 11 x64 con la versión de
  .NET seleccionada.
- Mantener la distribución sin instalador, con ejecutable autocontenido y datos
  dentro del perfil local del usuario.
- Usar 52 × 52 como tamaño inicial aproximado del botón central y ajustar durante
  la revisión visual.
- Abrir y cerrar el menú central con clic izquierdo; usar el arrastre solo para
  mover el botón.
- Elegir la primera paleta de colores.
- Elegir referencias visuales y tipografía.
- Realizar bocetos de baja fidelidad para:
  - Botón central cerrado.
  - Menú central abierto.
  - Ventana vacía.
  - Ventana con pendientes y completadas.
  - Selector de color.
  - Confirmación de eliminación.
- Usar aproximadamente 410 × 420 como tamaño inicial y 280 × 220 como tamaño
  mínimo de una ventana, medidos en unidades independientes de dispositivo; se
  podrán ajustar después de probar el diseño.
- Incluir edición de tareas.
- No implementar una minimización independiente: las listas se ocultarán.
- Hacer que `Alt+F4` o el cierre de una lista solo la oculten.
- Incluir el icono del área de notificación y el inicio automático activado por
  defecto dentro del MVP.

### Entregables

- Requisitos pendientes resueltos en `/.codex/`.
- Bocetos o mockups aprobados.
- Paleta, tipografía y estilo base aprobados.
- Plan actualizado con todas las decisiones.

### Criterios de aceptación

- No quedan dudas que modifiquen el modelo de datos o la navegación principal.
- Se puede describir el flujo completo sin inventar comportamientos.
- El usuario aprueba comenzar el código.

### Puerta de revisión

No comenzar la Etapa 1 sin autorización explícita.

---

## Etapa 1 — Preparación de la solución

### Objetivo

Crear una base compilable, pequeña y mantenible, todavía sin implementar el flujo
completo del producto.

### Actividades

- Verificar los SDK de .NET instalados.
- Seleccionar y fijar la versión estable objetivo.
- Crear solución y proyecto WPF.
- Configurar análisis estático, advertencias y formato.
- Definir `nullable` y convenciones de nombres.
- Preparar inyección de dependencias y arranque de la aplicación.
- Crear estructura de carpetas.
- Configurar recursos globales de XAML.
- Agregar `.gitignore` cuando exista repositorio Git.
- Crear pruebas unitarias si el entorno del proyecto las separa en otro proyecto.
- Incorporar solo las dependencias estrictamente necesarias.
- Documentar comandos de compilación, prueba y publicación.
- Confirmar que el proyecto funciona sin privilegios de administrador.

### Validaciones

- Restauración de dependencias.
- Compilación Debug y Release.
- Arranque y cierre de una ventana mínima.
- Ejecución inicial de pruebas.
- Revisión de advertencias.

### Criterios de aceptación

- La solución compila desde una copia limpia.
- La aplicación abre y cierra sin errores.
- No existen dependencias visuales sin justificación.
- Las instrucciones para compilar son reproducibles.

---

## Etapa 2 — Dominio y persistencia

### Objetivo

Implementar el estado del producto y garantizar que pueda guardarse y recuperarse
antes de construir todas las ventanas.

### Actividades

- Crear entidades del estado general, listas y tareas.
- Implementar reglas de validación y valores predeterminados.
- Definir DTO de persistencia separado cuando resulte necesario.
- Añadir `SchemaVersion` desde el primer formato.
- Implementar serialización JSON legible y determinista.
- Resolver la ruta local de almacenamiento.
- Implementar escritura temporal y reemplazo seguro.
- Implementar respaldo del último archivo válido.
- Implementar carga con recuperación desde respaldo.
- Implementar coordinación de guardados concurrentes.
- Implementar espera corta para cambios frecuentes.
- Crear una estrategia simple de migración de esquema.
- Implementar el mecanismo de instancia única.
- Registrar errores técnicos mínimos.

### Pruebas unitarias mínimas

- Estado vacío se guarda y carga.
- Varias listas y tareas conservan todos sus campos.
- Caracteres Unicode, acentos y saltos permitidos se preservan.
- Campos desconocidos no rompen la carga.
- Campos opcionales ausentes reciben valores predeterminados.
- Tema desconocido usa el tema predeterminado.
- Orden de tareas queda normalizado.
- Archivo principal corrupto recupera el respaldo.
- Fallo al escribir no destruye el archivo anterior.
- Dos solicitudes de guardado terminan conservando el estado más reciente.
- Segunda instancia no crea un segundo escritor.

### Criterios de aceptación

- El estado puede persistirse sin depender de ninguna vista.
- Existe recuperación documentada ante corrupción.
- Las pruebas críticas de integridad pasan.

---

## Etapa 3 — Botón central e icono de notificación

### Objetivo

Construir los puntos de control permanentes de la aplicación: el botón flotante
y el icono que representa a Todo's en segundo plano.

### Actividades

- Crear ventana sin marco para el botón.
- Configurar fondo transparente solo si no perjudica rendimiento o nitidez.
- Implementar modo siempre visible.
- Implementar arrastre y diferenciación entre clic y movimiento.
- Aplicar 52 × 52 unidades independientes de dispositivo como tamaño inicial.
- Guardar posición al terminar el arrastre.
- Restaurar posición y corregirla según monitores disponibles.
- Diseñar estados normal, hover, pressed, foco y menú abierto.
- Implementar menú con acciones inicialmente conectadas a comandos vacíos o a
  servicios de ventana conforme avance el plan.
- Asegurar que el menú no quede fuera de pantalla.
- Permitir cerrar el menú con `Esc`, clic exterior y repetición del botón.
- Evitar que el botón y las listas aparezcan como aplicaciones independientes en
  la barra de tareas.
- Crear un único icono de Todo's en el área de notificación.
- Implementar su menú con “Mostrar Todo's” y “Salir”.
- Conectar “Salir” con el guardado y cierre coordinado, sin terminar el proceso de
  manera abrupta.
- Eliminar correctamente el icono al terminar la aplicación.
- Añadir nombre accesible y navegación por teclado.

### Pruebas

- Clic abre y cierra menú.
- Arrastrar no ejecuta accidentalmente una opción.
- Posición persiste tras reinicio.
- Posiciones en bordes y coordenadas negativas se restauran correctamente.
- Menú se adapta a los cuatro bordes de pantalla.
- Botón continúa accesible después de cambio de resolución.
- Icono permanece accesible con todas las listas ocultas.
- “Mostrar Todo's” recupera el botón sin crear otra instancia.
- “Salir” desde el icono guarda los últimos cambios.
- Ninguna lista genera un botón independiente en la barra de tareas.

### Criterios de aceptación

- El botón puede permanecer abierto durante trabajo normal sin estorbar.
- Siempre puede recuperarse visualmente.
- Su posición se conserva con precisión razonable.
- El icono representa de forma clara que Todo's continúa en segundo plano.

---

## Etapa 4 — Administración de ventanas TODO

### Objetivo

Crear y controlar múltiples ventanas independientes, todavía priorizando su ciclo
de vida sobre las funciones avanzadas de tareas.

### Actividades

- Implementar `IWindowManager`.
- Crear una ventana por cada `TodoWindowState` visible.
- Implementar creación de listas con identificador único.
- Implementar barra de título personalizada.
- Implementar movimiento y redimensionamiento.
- Aplicar aproximadamente 410 × 420 unidades como tamaño inicial y 280 × 220
  como mínimo, sujetos a ajuste visual.
- Implementar cambio de título.
- Implementar ocultar individualmente.
- Implementar mostrar individualmente desde el botón central.
- Implementar ocultar todas y mostrar todas.
- Implementar confirmación y eliminación definitiva.
- Implementar selección de color desde la paleta.
- Restaurar todas las propiedades al reiniciar.
- Evitar que cerrar una ventana por mecanismos de Windows destruya el estado si
  la intención definida es ocultar.
- Interceptar `Alt+F4` para ocultar la lista sin salir ni eliminarla.
- Mantener diálogos asociados a su ventana propietaria.

### Pruebas

- Crear una, varias y muchas ventanas.
- Ocultar y mostrar en distintos órdenes.
- Cambiar títulos duplicados sin confundir identificadores.
- Eliminar una ventana sin afectar otras.
- Cerrar y abrir conservando visibles y ocultas.
- Mover y redimensionar en varios monitores.
- Verificar que cada ventana conserva su color.

### Criterios de aceptación

- La administración de ventanas cumple el contexto original.
- Una lista oculta nunca se interpreta como eliminada.
- El estado visual restaurado coincide con el guardado.

---

## Etapa 5 — Gestión de tareas y progreso

### Objetivo

Completar el flujo esencial de captura y seguimiento.

### Actividades

- Crear ViewModel de lista y tarea.
- Implementar captura y validación de texto.
- Implementar agregado con `Enter` y botón.
- Implementar edición con doble clic, acción visible, `Enter` para confirmar y
  `Esc` para cancelar.
- Implementar completar y reabrir.
- Implementar eliminación de tarea con acción temporal para deshacer.
- Implementar lista de pendientes.
- Implementar sección separada de completadas.
- Implementar desplegar/colapsar completadas.
- Guardar ese estado por ventana.
- Implementar contador o indicador de progreso discreto.
- Implementar reordenamiento mediante arrastre.
- Mantener orden correcto después de todas las operaciones.
- Gestionar scroll y foco después de agregar.
- Definir y comprobar comportamiento con listas vacías y largas.

### Pruebas unitarias

- Agregar rechaza texto vacío.
- Crear y editar respetan máximos de 300 caracteres.
- Editar conserva estado, orden y fecha de finalización.
- Completar retira de pendientes y asigna fecha.
- Reabrir limpia fecha y coloca en orden definido.
- Eliminar reajusta orden y deshacer recupera posición y estado.
- Reordenar principio, medio y final produce el orden esperado.
- Reordenar cancelado no modifica datos.
- Alternar completadas persiste por lista.

### Pruebas de interacción

- Captura rápida de varias tareas solo con teclado.
- Edición y cancelación mediante teclado y ratón.
- Reordenamiento con scroll.
- Completar varias tareas consecutivamente.
- Reabrir una tarea con la sección desplegada.
- Reiniciar y comprobar contenido, estado y orden.

### Criterios de aceptación

- Todas las operaciones básicas son rápidas y predecibles.
- Cada cambio persiste.
- El orden nunca depende accidentalmente del orden visual de creación de controles.

---

## Etapa 6 — Sistema visual y animaciones

### Objetivo

Transformar la interfaz funcional en el producto visual aprobado.

### Actividades

- Implementar diccionarios de recursos del sistema de diseño.
- Incorporar tipografía e iconos con sus licencias.
- Crear controles o estilos reutilizables para botones, campos, casillas, menús,
  tooltips, listas y diálogos.
- Aplicar paletas pastel completas.
- Crear selector de color accesible.
- Ajustar jerarquía, alineación, densidad y espacios.
- Implementar sombras y esquinas redondeadas.
- Añadir animaciones definidas en la sección visual.
- Evitar que animaciones y transparencias degraden el uso de CPU en reposo.
- Comprobar contraste.
- Probar textos largos y escalado.
- Crear estados vacíos y mensajes breves coherentes.

### Validación visual

- Comparar cada vista con los mockups aprobados.
- Capturar botón y ventanas en todos los temas.
- Revisar 100 %, 125 %, 150 % y 200 % de escala.
- Revisar fondo claro, oscuro y con mucho contenido detrás.
- Confirmar que las sombras no recortan contenido.
- Confirmar que los iconos comunican su acción o tienen tooltip.
- Medir consumo de CPU con la aplicación sin interacción.

### Criterios de aceptación

- El diseño se percibe consistente en todos los estados.
- No quedan controles con apariencia predeterminada incongruente.
- Las animaciones se sienten suaves y no obstaculizan acciones rápidas.
- El usuario aprueba una revisión visual antes de continuar.

---

## Etapa 7 — Ciclo de vida, recuperación y escenarios del sistema

### Objetivo

Hacer confiable la aplicación frente a condiciones reales de Windows.

### Actividades

- Coordinar cierre desde el botón central.
- Coordinar el mismo cierre seguro desde el icono del área de notificación.
- Interceptar cierres individuales según la regla aprobada.
- Vaciar guardados pendientes antes de salir.
- Implementar reactivación desde una segunda instancia.
- Implementar el registro de inicio para el usuario actual mediante la entrada
  `Run` correspondiente, sin instalador ni privilegios administrativos.
- Activar la preferencia de inicio automáticamente en la primera ejecución.
- Implementar el interruptor para registrar o retirar la entrada propia.
- Validar y corregir de forma segura una ruta desactualizada cuando el ejecutable
  haya sido movido y Todo's se abra manualmente.
- Respetar una desactivación externa de Windows y evitar reescribir la entrada
  durante cada arranque automático.
- Manejar suspensión y reanudación.
- Revalidar posiciones cuando cambian monitores.
- Evitar ventanas huérfanas y referencias retenidas después de eliminar.
- Definir comportamiento ante errores de carga y guardado.
- Añadir registro rotativo pequeño si se confirma necesario.
- Verificar que un error en una ventana no cierre silenciosamente todo el programa.
- Probar recuperación tras terminación forzada.

### Criterios de aceptación

- El cierre normal no pierde el último cambio.
- El programa siempre reaparece en una zona visible.
- La segunda instancia no duplica datos ni ventanas.
- Activar o desactivar el inicio con Windows funciona sin administrador.
- El primer uso deja el inicio activado y el usuario puede desactivarlo.
- Los errores recuperables se explican sin mensajes técnicos agresivos.

---

## Etapa 8 — Accesibilidad y facilidad de uso

### Objetivo

Garantizar que la estética no reduzca claridad ni control.

### Actividades

- Definir orden lógico de tabulación.
- Proporcionar foco visible.
- Añadir nombres accesibles a botones de icono.
- Verificar lectura por Narrador de Windows en los flujos principales.
- Añadir tooltips donde un icono pueda ser ambiguo.
- Confirmar áreas de clic suficientes.
- Permitir operar captura, completado y menús principales con teclado.
- Verificar contraste de cada tema.
- Respetar escalado y tamaño de texto.
- Respetar reducción de animaciones cuando sea viable.
- Revisar mensajes de confirmación y error en lenguaje sencillo.

### Atajos candidatos a validar

- `Enter`: agregar tarea o confirmar edición.
- `Esc`: cerrar menú, selector o cancelar edición.
- `Ctrl+N`: crear nueva lista cuando el botón central tenga foco.
- `Delete`: eliminar tarea solo cuando exista selección clara y confirmación
  apropiada.

Los atajos no se fijarán definitivamente sin comprobar que no generen acciones
destructivas inesperadas.

### Criterios de aceptación

- Los flujos principales funcionan sin depender exclusivamente del ratón.
- Ningún botón esencial se identifica únicamente por color.
- El foco y los textos siguen siendo visibles en todas las paletas.

---

## Etapa 9 — Pruebas integrales y estabilización

### Objetivo

Validar el producto completo antes de generar la distribución final.

### Matriz funcional mínima

| Área | Caso |
|---|---|
| Inicio | Primera ejecución sin archivo de datos |
| Inicio | Restauración con listas visibles y ocultas |
| Inicio | Archivo principal corrupto y respaldo válido |
| Inicio | Archivo principal y respaldo corruptos |
| Listas | Crear, titular, cambiar color, ocultar, mostrar y eliminar |
| Tareas | Agregar, editar, completar, reabrir, eliminar, deshacer y reordenar |
| Estado | Reiniciar después de cada operación principal |
| Ventanas | Mover y redimensionar en un monitor |
| Monitores | Mover entre dos monitores con escalas distintas |
| Monitores | Desconectar el monitor donde estaba una lista |
| Cierre | Salir con un guardado pendiente |
| Cierre | Salir desde el botón central y desde el icono de notificación |
| Instancia | Intentar abrir dos veces la aplicación |
| Windows | Confirmar que listas y botón no aparecen individualmente en la barra |
| Windows | Activar/desactivar inicio y comprobar el siguiente inicio de sesión |
| Windows | Mover el ejecutable y reparar una ruta de inicio desactualizada |
| Entrada | Textos con acentos, Unicode y longitud límite |
| Rendimiento | Varias listas y cientos de tareas acumuladas |
| Visual | Todos los colores, estados, escalas y animaciones |
| Accesibilidad | Teclado, foco, tooltips y Narrador |

### Pruebas no funcionales

- Medir tiempo de inicio.
- Revisar memoria con varias ventanas abiertas y después de eliminarlas.
- Revisar CPU en reposo y durante animaciones.
- Verificar que no existan escrituras continuas sin cambios.
- Dejar la aplicación abierta durante una jornada de trabajo.
- Realizar ciclos repetidos de abrir/cerrar.
- Probar rutas de datos sin permisos de escritura simulando el fallo.
- Probar nombres y tareas suficientemente largos.

### Corrección de defectos

- Clasificar defectos por pérdida de datos, bloqueo, funcionalidad, accesibilidad
  y presentación.
- Corregir primero pérdida de datos y bloqueos.
- Añadir prueba de regresión para cada defecto crítico reproducible.
- No compensar un defecto de arquitectura con lógica duplicada en la vista.
- Repetir la matriz afectada después de cada corrección.

### Criterios de aceptación

- No existen defectos conocidos que puedan perder datos.
- No existen bloqueos reproducibles en flujos principales.
- Todos los criterios de etapas anteriores continúan cumpliéndose.
- La revisión visual final está aprobada.

---

## Etapa 10 — Publicación portable

### Objetivo

Entregar una compilación que pueda copiarse y ejecutarse fácilmente en Windows.

### Actividades

- Compilar en Release.
- Publicar autocontenido para Windows x64.
- Evaluar publicación como archivo único frente a carpeta portable.
- Verificar fuentes, iconos y librerías nativas incluidas.
- Añadir icono y metadatos de versión al ejecutable.
- Evitar incluir símbolos, archivos temporales o configuración de desarrollo.
- Generar suma de comprobación del artefacto.
- Probar en un equipo o entorno limpio sin SDK de .NET.
- Probar ejecución desde una carpeta normal sin administrador.
- Probar actualización manual reemplazando el ejecutable y conservando datos.
- Probar alta y baja del inicio automático con un usuario estándar.
- Confirmar que la entrada de inicio apunta al artefacto publicado, no a una ruta
  de desarrollo.
- Confirmar que el icono de notificación aparece y desaparece correctamente en la
  versión publicada.
- Documentar ubicación de datos y método de respaldo.
- Entregar el MVP sin exigir instalador. Un instalador podrá evaluarse después
  solo como método adicional de distribución.

### Criterios de aceptación

- La aplicación funciona sin instalar .NET por separado.
- No requiere permisos de administrador.
- El inicio automático funciona sin instalar la aplicación.
- Reemplazar la aplicación no elimina las listas existentes.
- La versión publicada pasa la matriz crítica, no solo la ejecución desde el IDE.

---

## Etapa 11 — Documentación y cierre del MVP

### Objetivo

Dejar el producto mantenible y listo para uso cotidiano.

### Entregables

- README de compilación y ejecución.
- Manual breve de usuario.
- Explicación de dónde se guardan los datos.
- Instrucciones para copia de seguridad y restauración.
- Instrucciones para activar/desactivar el inicio con Windows y para corregirlo si
  se mueve el ejecutable.
- Registro de decisiones técnicas relevantes.
- Lista de dependencias y licencias.
- Historial de cambios de la primera versión.
- Backlog separado para ideas posteriores.
- Copia actualizada de este plan con etapas marcadas como terminadas.

### Criterios de aceptación

- Un desarrollador puede compilar el programa siguiendo la documentación.
- Un usuario puede instalar/copiar, usar, respaldar y actualizar la aplicación
  sin conocer el código.
- El MVP queda cerrado sin incorporar silenciosamente funciones del backlog.

---

## 13. Estrategia de pruebas

### 13.1 Pruebas unitarias

Se concentrarán en reglas sin interfaz:

- Normalización de datos.
- Orden de tareas.
- Transiciones pendiente/completada.
- Validación de posiciones y tamaños.
- Migración del esquema JSON.
- Recuperación y coordinación de guardado.
- Comandos de ViewModel cuando puedan probarse sin ventanas reales.

### 13.2 Pruebas de integración

- Repositorio real sobre una carpeta temporal.
- Guardado, reemplazo, respaldo y recuperación.
- Inicio de aplicación a partir de estados preparados.
- Servicios de ventanas usando abstracciones cuando sea posible.
- Activación de la instancia existente.

### 13.3 Pruebas manuales

WPF y la administración de ventanas requieren validación visual real:

- Arrastre y redimensionamiento.
- Siempre visible frente a aplicaciones comunes.
- Foco y edición.
- Varios monitores y escalas.
- Animaciones y rendimiento.
- Uso prolongado.
- Apariencia de la publicación Release.

Una compilación exitosa no será suficiente para considerar terminada una fase
visual o de ventanas.

### 13.4 Evidencia por fase

Al cerrar cada etapa se registrará:

- Archivos principales modificados.
- Funciones implementadas.
- Pruebas ejecutadas y resultado.
- Advertencias conocidas.
- Capturas visuales cuando aplique.
- Decisiones o desviaciones del plan.

---

## 14. Rendimiento esperado

No se fijarán cifras definitivas hasta contar con un prototipo, pero se buscará:

- Inicio perceptiblemente rápido.
- Consumo de CPU prácticamente nulo en reposo.
- Arrastre y animaciones fluidas.
- Guardados sin bloquear la interfaz.
- Uso razonable con al menos 20 ventanas conservadas, aunque solo una parte esté
  visible.
- Manejo razonable de cientos de tareas distribuidas entre listas.

Se evitará optimizar prematuramente, pero cualquier temporizador permanente,
efecto visual o suscripción de ventana deberá justificarse y liberarse al cerrar.

---

## 15. Seguridad, privacidad y confiabilidad

- Los datos permanecerán locales.
- La aplicación no enviará telemetría ni contenido a Internet en el MVP.
- No requerirá privilegios de administrador.
- No ejecutará texto introducido como comandos ni lo interpretará como código.
- Las rutas de archivos serán determinadas por la aplicación, no por el texto de
  las tareas.
- Los mensajes de error no mostrarán contenido completo de las listas salvo que
  sea necesario.
- La copia de respaldo priorizará recuperación frente a limpieza automática.
- Cualquier dependencia externa deberá revisar licencia y mantenimiento.

---

## 16. Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Apariencia WPF predeterminada anticuada | Producto visualmente inconsistente | Sistema de diseño propio desde el prototipo |
| Ventanas fuera de pantalla | El usuario no puede recuperar listas | Validación contra áreas de trabajo y pruebas multimonitor |
| Pérdida/corrupción del JSON | Pérdida de información | Escritura temporal, reemplazo atómico y respaldo |
| Guardados excesivos durante arrastre | Consumo y bloqueos | Guardar al finalizar y aplicar espera corta |
| Muchas ventanas `Topmost` estorban | Mala experiencia | Ocultar individual/todas y control central siempre accesible |
| Confusión entre ocultar y eliminar | Eliminación accidental | Acciones diferenciadas y confirmación destructiva |
| Sombras/transparencias costosas | Alto consumo en reposo | Medición temprana y efectos moderados |
| Dependencia visual abandonada | Mantenimiento difícil | Preferir recursos propios o librería madura y mínima |
| Publicación funciona en IDE pero no portable | Entrega inutilizable | Probar artefacto Release en entorno limpio |
| Dos instancias sobrescriben datos | Estado inconsistente | Instancia única y escritor serializado |
| Mover el ejecutable rompe el inicio automático | Todo's deja de abrir con Windows | Detectar la ruta desactualizada en una apertura manual y permitir corregirla |
| Windows desactiva externamente el inicio | El interruptor no refleja la realidad | Consultar el registro real y no sobrescribir la decisión externa en cada arranque |
| Cambio futuro del formato | Datos antiguos incompatibles | `SchemaVersion` y migraciones desde la primera versión |
| Alcance crece antes del MVP | Retrasos y complejidad | Respetar sección fuera de alcance y backlog separado |

---

## 17. Definición de terminado por funcionalidad

Una funcionalidad solo se considerará terminada cuando:

- Cumpla el requisito funcional y sus casos límite conocidos.
- Esté conectada al guardado y restauración si modifica estado.
- Tenga manejo de errores razonable.
- Use el sistema de diseño compartido.
- Sea operable con los medios definidos para accesibilidad.
- Incluya pruebas unitarias o de integración cuando la lógica lo permita.
- Haya sido comprobada manualmente si depende de ventanas o presentación.
- No introduzca advertencias nuevas sin documentar.
- La documentación afectada esté actualizada.
- El resultado Release se haya comprobado cuando la fase lo requiera.

---

## 18. Decisiones confirmadas y ajustes visuales pendientes

### 18.1 Decisiones confirmadas

1. Nombre visible: `Todo's`.
2. Compatibilidad objetivo: Windows 10 x64 y Windows 11 x64.
3. Distribución: ejecutable autocontenido sin instalador obligatorio; los datos
   se guardan en el perfil local del usuario.
4. Las tareas se pueden editar después de agregarlas.
5. `Alt+F4` y el cierre de una ventana TODO únicamente la ocultan.
6. El botón central y las listas no aparecen como aplicaciones independientes en
   la barra de tareas.
7. Todo's sí aparece en el área de notificación y desde su icono puede mostrarse
   o cerrarse guardando el estado.
8. Clic izquierdo abre/cierra el menú central; arrastrar mueve el botón.
9. El botón inicia aproximadamente en 52 × 52 unidades.
10. Las listas inician aproximadamente en 410 × 420 y tienen un mínimo inicial de
    280 × 220 unidades; estos tamaños se podrán afinar visualmente.
11. Las completadas se ordenan por finalización más reciente; al reabrirse pasan
    al final de pendientes.
12. Eliminar una tarea ofrece deshacer brevemente; eliminar una lista completa
    requiere confirmación.
13. Títulos de una línea con máximo inicial de 80 caracteres y tareas con máximo
    inicial de 300 caracteres.
14. El menú central siempre inicia cerrado.
15. La opción de iniciar con Windows forma parte del MVP y estará activada por
    defecto, pero podrá desactivarse.

### 18.2 Ajustes que se resolverán con el prototipo

- Cantidad y colores definitivos de la paleta pastel.
- Tipografía definitiva.
- Afinación de tamaños después de ver el programa a escala real.
- Duración exacta de animaciones y del periodo para deshacer.
- Distribución visual exacta de los botones de cada lista.

Estos ajustes no bloquean la arquitectura. Se validarán visualmente en las puertas
de revisión correspondientes antes de dar por terminado el diseño.

---

## 19. Posibles mejoras posteriores al MVP

Se mantendrán como backlog y no se implementarán durante las etapas anteriores:

- Papelera o historial permanente de eliminaciones.
- Atajos globales configurables.
- Modo compacto adicional por lista.
- Fijar temporalmente una lista por debajo de otras ventanas.
- Fechas, recordatorios y notificaciones.
- Búsqueda entre listas.
- Exportar e importar respaldos desde la interfaz.
- Modo portable con datos junto al ejecutable.
- Temas oscuro y alto contraste dedicados.
- Sincronización opcional.
- Integración con Codex para asociar una lista con una tarea o proyecto.
- Actualizaciones automáticas firmadas.

Cada mejora futura requerirá su propio análisis, actualización del modelo de datos
y criterios de aceptación.

---

## 20. Orden de ejecución obligatorio

Cuando el usuario autorice el desarrollo:

1. Leer nuevamente `/.codex/context.md` y este plan.
2. Revisar si existen nuevas decisiones o archivos del proyecto.
3. Ejecutar únicamente la etapa autorizada.
4. No anticipar funciones de etapas posteriores salvo infraestructura mínima
   explícitamente necesaria.
5. Validar la etapa con sus pruebas y criterios.
6. Informar resultado, archivos modificados y cualquier desviación.
7. Actualizar la documentación si cambió una decisión o contrato.
8. Esperar la revisión indicada antes de pasar a la siguiente puerta.

Este orden busca conservar el alcance, prevenir pérdida de datos y asegurar que
la calidad visual se valide en el producto real, no únicamente por compilación.

## Ampliación posterior — 1.1.0 (2026-09-17)

El plan anterior es histórico. El usuario autorizó agrupaciones de tareas,
corregir la marca de inserción, mostrar la tarea durante el arrastre y alinear
cursor/placeholder. Se implementaron grupos persistentes por lista, General
predeterminado, traslado por arrastre y menú, renombrado y eliminación de grupo
sin pérdida de tareas, esquema 2, animación y regresiones WPF. Los artefactos
vigentes pasan a 1.1.0; las evidencias están en progreso.md.

## Ajustes posteriores — 1.1.1 (2026-09-17)

Completados los ajustes solicitados: captura en una sola fila, menús laterales
para seleccionar/crear y mover, colores de encabezado según tema, acordeones
persistentes, confirmación al quitar agrupaciones, papelera y move-up-down.
Se mantienen los contratos de tareas, grupos y traslado sin pérdida de contenido.

## Ajustes posteriores — 1.1.2 (2026-09-17)

- [x] Resolver inserción entre filas y márgenes con la misma lógica de destino.
- [x] Indicador y tarjeta del tema; copia visual completa a tamaño real.
- [x] Mover desde el encabezado, conservando botones y edición por doble clic/F2.
- [x] Pruebas WPF de regresión y renders revisados.

### Ajuste 1.1.3 — 2026-09-17

Botón de mover a agrupación retirado de las tareas. Colores e interacciones
ligados al tema de cada ventana, incluidos hover de agrupación y agregar,
presionado, foco, casillas, campos, iconos, menús y deshacer.
