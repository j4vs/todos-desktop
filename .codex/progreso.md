# Progreso de implementación — Todo's

Este documento registra el cierre verificable de cada etapa. Una etapa solo se
marca como completada después de implementar o documentar sus entregables y
ejecutar sus validaciones.

## Resumen

| Etapa | Estado | Evidencia principal |
|---|---|---|
| 0. Definición funcional y visual | Completada | Contexto, plan y diseño consolidados |
| 1. Preparación de la solución | Completada | Build Release limpio y smoke test aprobado |
| 2. Dominio y persistencia | Completada | 11 pruebas de dominio/archivos aprobadas |
| 3. Botón central e icono | Completada | 16 pruebas y smoke test WPF aprobados |
| 4. Ventanas TODO | Completada | 19 pruebas y persistencia de ventana comprobada |
| 5. Tareas y progreso | Completada | 21 pruebas y smoke con fixture de tareas |
| 6. Sistema visual y animaciones | Completada | Render WPF real revisado y build limpio |
| 7. Ciclo de vida y recuperación | Completada | 26 pruebas y smoke test aprobados |
| 8. Accesibilidad | Completada | Contraste WCAG, teclado y render real validados |
| 9. Pruebas integrales | Completada | 30 pruebas, carga y proceso WPF aprobados |
| 10. Publicación portable | Completada | EXE único x64 y ZIP ejecutados correctamente |
| 11. Documentación y cierre | Completada | Manual, build, decisiones y entrega final |

## Etapa 0 — Definición funcional y visual

### Decisiones cerradas

- Nombre visible `Todo's` e identificador técnico `Todos`.
- Windows 10/11 x64, WPF, C# y .NET estable con soporte vigente.
- Ejecutable autocontenido sin instalador obligatorio; datos en el perfil local.
- Botón flotante, múltiples listas `Topmost` e icono del área de notificación.
- Inicio con Windows activado por defecto y configurable.
- Tareas editables, reordenables, completables y eliminables con deshacer.
- Cierre individual oculta; salida central o desde notificación guarda y termina.
- Paleta, tipografía, tamaños, composición y animaciones iniciales definidos en
  `/.codex/diseno.md`.

### Validación

- `/.codex/context.md` revisado.
- `/.codex/plan-trabajo.md` revisado y autorizado por el usuario.
- Requisitos que afectan arquitectura resueltos.
- Dirección visual y wireframes documentados.

### Resultado

Etapa válida. Se puede preparar la solución sin inventar flujos pendientes.

## Etapa 1 — Preparación de la solución

### Implementación

- SDK fijado en .NET 8 (`8.0.422`) con Windows Desktop Runtime `8.0.28`.
- Solución `Todos.sln` con proyectos `Todos.App` y `Todos.Tests`.
- Aplicación WPF `net8.0-windows`, ejecutable como usuario normal.
- Referencia integrada a Windows Forms únicamente para el futuro icono de
  notificación, sin paquetes externos.
- Ejecutor de pruebas local y sin NuGet, debido a que el entorno no tiene fuentes
  de paquetes configuradas.
- Configuración común, reglas de formato, exclusiones y manifest de Windows.
- Icono vectorial propio y recurso `.ico` multirresolución.

### Validación ejecutada

```text
dotnet restore Todos.sln
dotnet build Todos.sln --configuration Release --no-restore
dotnet run --project tests/Todos.Tests/Todos.Tests.csproj --configuration Release --no-build
```

Resultado:

- Compilación correcta: 0 advertencias, 0 errores.
- Pruebas: 1 aprobada, 0 fallidas.
- Nombre técnico del ensamblado verificado como `Todos`.

### Resultado

Etapa válida. La solución compila sin dependencias externas y queda preparada
para implementar dominio y persistencia.

## Etapa 2 — Dominio y persistencia

### Implementación

- Estado general, configuración, posición del botón, ventanas, temas y tareas.
- Reglas de títulos/tareas, límites, orden pendiente y orden de completadas.
- Operaciones para agregar, editar, completar, reabrir, eliminar, restaurar y
  reordenar tareas.
- Normalización de archivos anteriores o incompletos y rechazo seguro de
  versiones futuras desconocidas.
- Persistencia JSON en `%LOCALAPPDATA%\Todos` con archivo temporal, reemplazo,
  respaldo y recuperación ante corrupción.
- Instantáneas profundas para que un guardado no lea estado mutable.
- Guardado serializado y con espera corta para cambios frecuentes.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 11 pruebas aprobadas, 0 fallidas.
- Cobertura comprobada: Unicode, reglas de texto, ciclo completo de tareas,
  duplicados, dimensiones inválidas, versión futura, round-trip JSON, campos
  faltantes, recuperación desde respaldo, fallo de escritura y coalescencia de
  guardados.

### Resultado

Etapa válida. Los datos y sus reglas funcionan sin depender de la interfaz.

## Etapa 3 — Botón central e icono de notificación

### Implementación

- Ventana flotante `52 × 52`, transparente, sin barra de tareas y `Topmost`.
- Clic izquierdo para abrir/cerrar menú y arrastre manual para mover.
- Menú con nueva lista, mostrar/ocultar todas, inicio con Windows y salir.
- Corrección de posición contra el área de trabajo disponible.
- Icono único en el área de notificación con mostrar, nueva lista y salida segura.
- Controlador de aplicación que carga estado, crea controles permanentes y
  coordina el guardado/cierre.
- Modo de smoke test con carpeta de datos aislada y cierre normal automático.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 16 pruebas aprobadas, 0 fallidas.
- Posiciones por defecto, bordes y coordenadas negativas comprobadas.
- Comandos, apertura de menú y ciclo de vida de `NotifyIcon` comprobados.
- Arranque y cierre real de WPF ejecutados con `--smoke-test`.
- La herramienta de inspección de escritorio no expone como ventana controlable
  el botón transparente sin barra de tareas; la revisión visual integral se
  repetirá cuando exista la ventana TODO de mayor tamaño.

### Resultado

Etapa válida. Los controles permanentes funcionan y se liberan al salir.

## Etapa 4 — Administración de ventanas TODO

### Implementación

- Creación y restauración de múltiples ventanas independientes.
- Ventanas pastel, `Topmost`, sin entrada individual en la barra de tareas.
- Título editable, cambio entre siete colores, ocultar y eliminar con confirmación.
- Movimiento por encabezado y redimensionamiento con control inferior derecho.
- Tamaño inicial/mínimo y corrección de posición antes de mostrar.
- `Alt+F4` oculta en lugar de eliminar o salir.
- Mostrar/ocultar todas y recuperación individual desde el menú central.
- Gestor dedicado que crea las ventanas solo cuando se necesitan y las cierra de
  forma controlada al salir.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 19 pruebas aprobadas, 0 fallidas.
- ViewModels, temas, límites, acciones y lista de ocultas comprobados.
- Smoke test real creó una ventana, la mostró, guardó JSON y salió normalmente.
- El JSON aislado conservó título, tema, posición, tamaño y visibilidad.

### Resultado

Etapa válida. El ciclo completo de las ventanas funciona y queda listo para
reemplazar el estado vacío por tareas reales.

## Etapa 5 — Gestión de tareas y progreso

### Implementación

- Captura rápida con `Enter` y botón.
- Validación y límite de 300 caracteres.
- Edición por doble clic o botón, confirmación con `Enter` y cancelación con
  `Esc`.
- Completar, reabrir al final de pendientes y ordenar completadas por fecha.
- Reordenamiento de pendientes desde un control de arrastre.
- Eliminación inmediata con aviso y acción “Deshacer” durante cinco segundos.
- Sección de completadas plegable y persistente.
- Contadores, estado vacío, scroll y actualización automática del resumen.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 21 pruebas aprobadas, 0 fallidas.
- Flujo unitario completo: agregar, ordenar, completar, reabrir, editar, eliminar,
  deshacer y desplegar completadas.
- Rechazo de tareas vacías y edición inválida comprobado.
- Smoke test WPF con fixture de 2 pendientes y 1 completada.
- El estado conservó las 3 tareas, su orden y la sección desplegada.

### Resultado

Etapa válida. El flujo principal del producto ya es funcional y persistente.

## Etapa 6 — Sistema visual y animaciones

### Implementación

- Diccionarios globales de tipografía, marca, texto, peligro, foco y radios.
- Estilos compartidos de foco, campos, botones, casillas y tooltips.
- Siete temas pastel completos con fondo, superficie, acento y texto.
- Ventanas redondeadas con sombra, jerarquía compacta y acciones coherentes.
- Marca e icono propios.
- Animación de entrada de 180 ms y popups con transición breve.
- Respeto de la preferencia de Windows para reducir animación.
- Estado vacío, placeholder de captura, filas pendientes/completadas y aviso de
  deshacer refinados visualmente.
- Modo interno de render que genera PNG desde la ventana WPF real.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 21 pruebas aprobadas, 0 fallidas antes del render final.
- Render real generado en `todos-wpf-preview.png`, usando el fixture funcional.
- Revisión visual comprobó: título, contadores, captura, pendientes, acciones,
  sección completada, redimensionamiento, contraste y bordes sin recorte.

### Resultado

Etapa válida. La interfaz real sigue la dirección visual aprobada y no conserva
controles principales con apariencia predeterminada incongruente.

## Etapa 7 — Ciclo de vida, recuperación y Windows

### Implementación

- Instancia única mediante recursos locales de Windows; un segundo inicio activa
  el menú de la instancia que ya está abierta.
- Registro en `HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`, sin
  requerir instalador ni privilegios de administrador.
- Inicio con Windows habilitado por defecto, configurable desde el menú y
  autorreparable si el ejecutable portable cambia de ubicación.
- Los modos de prueba y render no modifican el registro real.
- Corrección por monitor y DPI del área de trabajo disponible.
- Revalidación de posiciones al cambiar monitores, resolución o volver de una
  suspensión.
- Guardado inmediato antes de suspender y cierre bloqueado si el estado no puede
  persistirse.
- Registro operativo local rotativo, sin incluir el contenido de las tareas.
- Manejo de errores de interfaz y segundo plano con mensajes recuperables.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 26 pruebas aprobadas, 0 fallidas.
- Registro de arranque probado mediante almacenamiento simulado: habilitar,
  deshabilitar, rutas con espacios y detección de ejecutable movido.
- Señal entre primera y segunda instancia comprobada con identificadores aislados.
- Creación de log y captura de excepción controlada comprobadas.
- Rechazo del cambio de inicio automático conserva correctamente el valor visible.
- Smoke test WPF real completado con carpeta de datos aislada y sin tocar el
  arranque del usuario.

### Resultado

Etapa válida. Todo's se integra al ciclo de Windows sin instalador, conserva los
datos ante suspensión/cierre y evita instancias duplicadas.

## Etapa 8 — Accesibilidad y facilidad de uso

### Implementación

- Orden de tabulación natural y foco visible en botones, campos y casillas.
- Nombres y ayudas para automatización de Windows en ventanas, campos, botones de
  icono, tareas, selector de color y control de tamaño.
- Tooltips descriptivos para todas las acciones representadas solo con iconos.
- `Ctrl+N` crea una lista desde el control principal y enfoca la captura dentro
  de una lista.
- `Enter` agrega tareas o confirma edición; `Esc` cancela edición o cierra menús.
- Las filas pendientes pueden enfocarse: `Alt+↑` y `Alt+↓` las reordenan, y
  `Supr` elimina únicamente la fila que tiene foco claro.
- Casillas esenciales ampliadas a un área interactiva de 28 × 32 unidades.
- Reducción de animaciones y escalado por DPI se conservan según Windows.
- Cálculo WCAG de contraste añadido como regla comprobable del sistema visual.

### Validación ejecutada

- Compilación Release: 0 advertencias, 0 errores.
- 27 pruebas aprobadas, 0 fallidas.
- Los siete temas superan relación 4.5:1 para texto normal sobre fondo y
  superficie.
- Render WPF real revisado después de los cambios: controles, textos largos,
  foco potencial y áreas de interacción permanecen sin recortes.
- Smoke test real completado con navegación y recursos accesibles cargados.
- Los nombres para Narrador se verificaron en el árbol XAML; una comprobación
  auditiva final queda como revisión manual recomendada en un equipo de uso.

### Resultado

Etapa válida. Los flujos principales no dependen exclusivamente del ratón y las
acciones esenciales tienen texto, ayuda o nombre accesible además del color.

## Etapa 9 — Pruebas integrales y estabilización

### Cobertura añadida

- Archivo principal y respaldo simultáneamente dañados: inicia vacío, informa el
  problema y conserva ambos archivos como evidencia recuperable.
- Instantánea profunda: cambios posteriores de la interfaz no alteran el estado
  que ya estaba en proceso de guardado.
- Carga acumulada de 20 listas, 1,000 tareas, siete colores, posiciones mixtas y
  ventanas visibles/ocultas.
- Round-trip integral de ese estado con conservación de títulos Unicode, orden,
  tamaños y visibilidad.

### Validación ejecutada

- Build Release completo: 0 advertencias, 0 errores.
- 30 pruebas aprobadas, 0 fallidas, en aproximadamente 0.7 segundos.
- El round-trip de 20 listas y 1,000 tareas quedó bajo el límite preventivo de
  cinco segundos.
- Búsqueda de pendientes técnicos: no hay `TODO`, `FIXME` ni funciones de
  producto sin implementar; los `NotSupportedException` encontrados pertenecen
  únicamente al retorno no aplicable de convertidores WPF.
- Smoke test del ejecutable Release: código de salida 0.
- Ciclo real medido en aproximadamente 3.0 segundos, incluyendo la pausa interna
  de validación de 0.9 segundos; memoria observada al estar listo: 47.6 MB.
- Render final posterior a accesibilidad revisado sin recortes o regresiones.
- El directorio todavía no es un repositorio Git, por lo que no existe una
  revisión de diff; la compilación y las pruebas cubren directamente todos los
  archivos de la solución.

### Resultado

Etapa válida. No existen defectos conocidos de pérdida de datos o bloqueo en los
flujos comprobables de esta versión.

## Etapa 10 — Publicación portable

### Implementación

- Perfil `win-x64` Release, autocontenido y de archivo único.
- Runtime .NET y librerías nativas incluidos dentro de `Todos.exe`.
- Símbolos y archivos de desarrollo excluidos.
- Icono, descripción `Todo's`, producto y versión `1.0.0` incrustados.
- Artefacto directo y paquete ZIP de un solo archivo.
- Sumas SHA-256 para comprobar integridad.

### Artefactos

- `publish/win-x64/Todos.exe`: 68.69 MiB.
- `publish/Todos-1.0.0-win-x64.zip`: 63.21 MiB aproximadamente.
- `publish/Todos-1.0.0-win-x64.sha256`: hashes del EXE y ZIP.

### Validación ejecutada

- Restauración específica y publicación para `win-x64` correctas.
- Carpeta publicada contiene únicamente `Todos.exe`.
- Ejecutable publicado iniciado directamente, con código de salida 0 y guardado
  real de estado.
- Render PNG generado desde el EXE autocontenido, verificando recursos WPF,
  estilos, icono y ensamblados incluidos.
- El ejecutable se copió a otra carpeta y recuperó 1 lista y 3 tareas desde la
  misma ubicación de datos, validando actualización/movimiento portable.
- El ZIP se extrajo en una carpeta aislada: contenía un único EXE, inició con
  código 0 y creó su estado.
- El equipo disponible sí tiene .NET instalado; por ello no simula literalmente
  un Windows limpio. La publicación `SelfContained=true`, sus runtime packs y la
  ejecución del archivo único comprueban que el SDK no es un requisito del
  paquete. La prueba final en otro Windows 10/11 sigue recomendada antes de una
  distribución masiva.
- No se modificó el inicio real de sesión del equipo durante las pruebas; la ruta
  publicada y el alta/baja se validaron con el registro simulado de la etapa 7.

### Resultado

Etapa válida. Existe un paquete portable ejecutable sin instalador y sin instalar
.NET por separado.

## Etapa 11 — Documentación y cierre

### Documentación entregada

- `README.md`: presentación, uso, privacidad, desarrollo y estructura.
- `/.codex/manual-usuario.md`: flujos, atajos, respaldo, restauración y soporte.
- `/.codex/build-and-publish.md`: validación, publicación, integridad y
  actualización.
- `/.codex/decisiones-tecnicas.md`: arquitectura, seguridad de datos y límites.
- `/.codex/backlog.md`: mejoras posteriores separadas del MVP.
- `CHANGELOG.md`: versión 1.0.0.
- `THIRD-PARTY-NOTICES.md`: dependencias y recursos distribuidos.
- `scripts/publish-win-x64.ps1`: publicación reproducible, ZIP y SHA-256.

### Validación final ejecutada

- El script de publicación completo terminó correctamente.
- Build Release final: 0 advertencias, 0 errores.
- Suite final: 30 aprobadas, 0 fallidas.
- EXE final: código 0 y creación de estado comprobada.
- Hash SHA-256 del EXE y del ZIP coincide con el archivo entregado.
- Código, pruebas, documentación y artefactos finales inventariados.

### Revisiones externas recomendadas

No bloquean que el programa se use, pero deben realizarse antes de una
distribución masiva:

- Prueba física en un Windows 10 x64 y un Windows 11 x64 sin SDK.
- Sesión con Narrador y monitores reales de escalas mixtas.
- Firma de código para reputación de Windows si se distribuirá públicamente.

### Resultado

Etapa válida. Las 12 etapas del plan están completadas; Todo's 1.0.0 queda listo
para uso portable y cuenta con instrucciones reproducibles para mantenerlo.

## Ajustes posteriores — Todo's 1.0.1

### Problema observado

El video de revisión mostró que los botones del menú principal solo cambiaban a
su estado presionado. La captura de puntero para arrastrar el botón flotante
recibía también los eventos de los controles contenidos en el `Popup`, los
marcaba como atendidos e impedía que llegaran al comando.

### Correcciones e implementación

- Área de arrastre separada del `Popup` y protegida por una política que solo
  acepta descendientes visuales reales del botón flotante.
- Prueba de regresión para comprobar que un botón del menú no se interpreta como
  gesto de movimiento.
- Hover primario con morado más luminoso y texto blanco permanente.
- Icono nuevo `icon.ico/png/svg` aplicado al EXE, botón y área de notificación.
- Fredoka incorporada como recurso local en cinco pesos.
- Renderizador ligero de los SVG propios para botones, casillas y acciones.
- Botón exclusivo de movimiento en el encabezado; el resto del encabezado deja
  de iniciar desplazamientos accidentales.
- Sección **Listas ocultas** al mismo nivel que mostrar/ocultar todas, desplegable
  con chevron y acciones independientes para mostrar o eliminar.
- Botón y menú principal refinados en jerarquía, espaciado, bordes y sombra.
- Nuevo render interno del menú para poder revisar cambios visuales futuros.

### Validación

- Build Release: 0 advertencias, 0 errores.
- 31 pruebas aprobadas, 0 fallidas.
- Render real de ventana TODO y menú principal revisado con Fredoka e iconos SVG.
- Metadatos de Fredoka confirmados: copyright del proyecto y licencia SIL OFL
  1.1; avisos de terceros actualizados para la distribución.

## Ajustes posteriores — Todo's 1.0.2

### Revisión recibida

La segunda revisión solicitó actualizar nuevamente la identidad, simplificar el
menú, sustituir el acordeón de listas ocultas por un submenú y mejorar la
legibilidad e interacción de las tareas. El video confirmó que el orden sí se
guardaba, pero no mostraba una posición de inserción durante el arrastre.

### Implementación

- `icon.png/svg` actualizado y `icon.ico` regenerado con nueve resoluciones,
  desde 16 hasta 256 px, para evitar escalados deficientes de Windows.
- Encabezado del menú reducido a **Todo's**, sin logo ni subtítulo.
- **Listas ocultas** convertido en un `Popup` lateral independiente, con mostrar
  y eliminar por elemento.
- Tokens morados comunes aplicados a estados hover y pressed; se eliminaron los
  estados azules nativos en las acciones revisadas.
- Ancho inicial de nuevas listas aumentado de 340 a 380 unidades.
- Resumen de pendientes/completadas ampliado; controles de movimiento más
  visibles y símbolo de cuatro flechas con mayor peso visual.
- Captura **Nueva tarea** más compacta, etiquetada y con bordes redondeados;
  botón `+` redondeado.
- Texto de tareas aumentado a 14 unidades y edición en una superficie morada,
  redondeada y claramente enfocada.
- Botones redundantes de lápiz eliminados; el doble clic continúa iniciando la
  edición.
- Reordenamiento con cálculo por mitad superior/inferior, guía morada animada,
  desplazamiento de la fila destino y transición final de acomodado.
- Prueba nueva para las posiciones de inserción y actualización de pruebas de
  ancho predeterminado y submenú.

### Validación visual intermedia

- Render de la ventana a 380 px revisado sin recortes de título.
- Render del menú principal revisado sin logo ni subtítulo.
- Render independiente del submenú revisado con estados y acciones moradas.
- La automatización visual de Windows no expone estas ventanas WPF sin marco y
  sin entrada en la barra de tareas; se mantienen los renders internos y las
  pruebas de política de interacción como validación reproducible.

### Cierre de versión

- Build Release final: 0 advertencias y 0 errores.
- Suite final: 32 pruebas aprobadas y 0 fallidas.
- Render del modo de edición revisado con borde, fondo y foco redondeados.
- Publicación portable 1.0.2 generada con avisos de terceros.
- Ejecutable extraído del ZIP: código de salida 0 y estado guardado.
- SHA-256 del ejecutable y del ZIP verificados contra el archivo publicado.

## Ajustes posteriores — Todo's 1.0.3

### Diagnóstico

El tercer video mostró el cursor de arrastre como no permitido y el cuadro de
**Error inesperado**. El log local registró `InvalidOperationException`: WPF
había congelado el `ScaleTransform` declarado en la plantilla y el código
intentaba animarlo durante `DragOver`.

### Correcciones

- Política visual que clona transformaciones `ScaleTransform` y
  `TranslateTransform` congeladas antes de animarlas.
- Prueba STA que congela explícitamente ambas transformaciones y confirma que
  las copias mutables pueden animarse.
- Ancho predeterminado aumentado a 410 unidades; estados que conservan los
  anchos predeterminados anteriores de 340 o 380 se normalizan a 410.
- Submenú lateral sin encabezado duplicado: su contenido comienza directamente
  con las listas ocultas y usa títulos de 14 unidades.
- Comando y botón de listas ocultas desactivados cuando la colección está vacía.
- Botón flotante sin borde, margen ni sombra de WPF; el PNG aporta todo el
  acabado visual.
- Reafirmación de z-order mediante `SetWindowPos(HWND_TOPMOST, SWP_NOACTIVATE)`
  al mostrar o activar una ventana TODO, de modo que el botón permanece encima
  sin robar foco.

### Validación intermedia

- Build Release: 0 advertencias y 0 errores.
- 33 pruebas aprobadas, 0 fallidas.
- Render de ventana a 410 unidades revisado.
- Render de menú, submenú directo y botón sin borde revisados.

### Cierre de versión

- Paquete portable 1.0.3 generado correctamente.
- Ejecutable extraído: código de salida 0 y persistencia inicial confirmada.
- SHA-256 del EXE y ZIP coinciden con el archivo publicado.
- Metadatos del ejecutable confirman la versión 1.0.3.
- Render final realizado desde el ejecutable publicado.

## Auditoría de alineación documental — 2026-09-17

### Alcance revisado

- Versión, dimensiones, recursos visuales y límites definidos en el proyecto.
- Comportamiento real del botón flotante, submenú de listas ocultas, movimiento
  de ventanas y reordenamiento de tareas.
- Persistencia, inicio con Windows, distribución portable y artefactos 1.0.3.
- Suite vigente de 33 pruebas y comandos reproducibles de compilación.

### Documentos alineados

- `context.md`: estado implementado y rutas canónicas de consulta.
- `decisiones-tecnicas.md`: decisiones acumuladas hasta 1.0.3.
- `diseno.md`: flujos y wireframes actuales, sin controles retirados.
- `manual-usuario.md`: orden superior del botón, feedback de arrastre y
  normalización de anchos anteriores.
- `build-and-publish.md` y `README.md`: regeneración del icono y funciones
  actuales.
- `backlog.md`: retirado el indicador de destino, ya implementado; se conserva
  como pendiente únicamente el auto-scroll para listas largas.
- `plan-trabajo.md`: distingue la línea base histórica 1.0.0 de los artefactos
  vigentes 1.0.3.

No se modificó código de producto durante esta auditoría.

## Agrupaciones, arrastre y captura — Todo's 1.1.0 (2026-09-17)

### Implementación

- Grupos con identidad propia por lista; General permanente y predeterminado.
- Creación, renombrado, captura por grupo, movimiento por arrastre/menú y
  eliminación de agrupación sin eliminar tareas.
- Esquema 2 y migración desde 1; instantáneas y normalización actualizadas.
- Corrección del booleano invertido entre cálculo de destino y marca visual.
- Tarjeta junto al cursor, origen atenuado, transición de filas al soltar,
  desplazamiento automático en bordes y limpieza al cancelar.
- Padding duplicado eliminado y placeholder integrado en la plantilla.

### Validación

- Compilación Release sin errores ni advertencias.
- 40 pruebas aprobadas: incluye migración, persistencia, grupos, edición de grupo,
  preservación de completadas y deshacer después de quitar una agrupación.
- Vista WPF real: alineación del cursor y marcas antes/después verificadas.
- Eventos WPF de DragOver/Drop: subir, bajar, mover entre grupos y a grupo vacío.
- Renders revisados en `artifacts/groups-review` a 560, 410 y 280 unidades.
- La tarjeta se comprobó con su texto real, abierta y sin interceptar hit testing.
- La revisión física prolongada del gesto con ratón y monitores de DPI mixto
  permanece manual; los eventos automatizados no sustituyen esa revisión.

### Cierre del paquete 1.1.0

- ZIP autocontenido generado con versión 1.1.0.0 y avisos de terceros.
- EXE extraído: lectura y migración en memoria de un fixture de esquema 1,
  representado en General sin perder tareas. Sin ediciones no se reescribe el JSON.
- Segundo arranque con grupos: conserva asignaciones y nombres; render del
  ejecutable publicado revisado. SHA-256 del EXE y ZIP verificados.
- Se añadió aislamiento del mutex para smoke/render con carpeta explícita.
  La identidad habitual sigue siendo
  `Todos`; los modos aislados usan una identidad derivada de su carpeta.
- Regresión añadida para identidades de instancia. Suite final: 40/40.

## Refinamiento de agrupaciones — 1.1.1 (2026-09-17)

- Captura de una fila; creación dentro del menú lateral. Menú compartido para
  selección y traslado, siguiendo el Popup de Listas ocultas.
- Encabezados y chevron del tema; plegado por grupo persistido en esquema 2.
- Confirmación personalizada con Cancelar enfocado antes de quitar un grupo.
- Papelera y recurso move-up-down.svg aplicados.
- Release sin errores ni advertencias; 41 pruebas aprobadas.
- Prueba WPF real de una fila, tema azul, acordeón, selección/creación en menú,
  traslado por menú, cancelar y confirmar eliminación preservando tareas.
- Round-trip e instantánea del estado de acordeones y carga con defaults abiertos.
- Renders revisados a 560, 410 y 280; menú de selección, traslado y confirmación
  en artifacts/groups-refinement-review.

## Refinamiento de arrastre y encabezado — 1.1.2 (2026-09-17)

El video mostró que los márgenes entre filas activaban el manejador del grupo,
cambiando la marca de inserción por un destino global. Se centralizaron DragOver
y Drop en TasksScrollViewer con un único cálculo de posición. La barra conserva
su ancho y usa Accent; solo grupos vacíos o plegados se resaltan completos.

La tarjeta es un RenderTargetBitmap de la fila, con DPI/tamaño reales, casilla,
texto e iconos. Se dibuja en Canvas sin hit testing y con colores Surface/Accent;
se elimina al terminar o cancelar. Se conserva la animación al soltar.

Se retiró el botón de mover. Todo el encabezado inicia el movimiento al superar
el umbral; los botones quedan excluidos. Doble clic/F2 permite editar el título.

Compilación Release: 0 errores y 0 advertencias. Suite: 41/41 aprobadas.
La regresión incluye margen entre filas, varios DragOver consecutivos, Drop en
ese mismo margen, grupos plegados, bitmap no vacío y tamaño/color del preview,
y política de encabezado. Renders revisados en artifacts/drag-refinement-review.
La comprobación fue con eventos/renders WPF; el recorrido físico sigue siendo manual.

Cierre 1.1.2: ZIP publicado y ejecutable extraído verificado con código de salida 0.
Metadatos 1.1.2.0, SHA-256, migración, grupos y reinicio correctos.
Render final revisado en artifacts/groups-release-0397f2c40d004fbeb1063a3d11649697.


## Temas completos de ventana — 1.1.3 (2026-09-17)

Retirado el botón de mover a agrupación de pendientes y completadas. Se conserva
el drag and drop y la alternativa contextual. Recursos locales dinámicos según
Surface/Accent/Text; los siete temas cubren hover, presionado, foco, selección,
casillas, campos, iconos, menús y deshacer sin cambiar los recursos globales.

Prueba WPF ampliada: hover real mediante propiedades de estado en los siete
temas, presionado de agregar, selección y ausencia del botón de traslado.
Renders: artifacts/window-theme-review. Informe: artifacts/window-theme-tests.txt.

Cierre 1.1.3: compilación sin advertencias ni errores y 41/41 pruebas aprobadas.
ZIP extraído: versión 1.1.3.0, migración, grupos, reinicio y SHA-256 correctos.
Se publicó en publish/win-x64-1.1.3 porque publish/win-x64/Todos.exe estaba en uso.
No se interrumpió la instancia del usuario. Paquete: publish/Todos-1.1.3-win-x64.zip.


## Rojo semántico de eliminación — 1.1.4 (2026-09-17)
Papeleras de listas, grupos y tareas, confirmación Eliminar y errores vuelven a TodosDangerBrush (#A54459). Los demás controles mantienen su tema.
Compilación limpia, 41/41 pruebas, renders revisados en artifacts/danger-color-review. ZIP 1.1.4 extraído: migración, grupos, reinicio, versión y SHA-256 verificados. Publicación en publish/win-x64-1.1.4 sin interrumpir la instancia abierta.

