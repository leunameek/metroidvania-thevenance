# Wireframes de El asedio de Bacatá

[Abrir el archivo editable en Figma](https://www.figma.com/design/UScmu79X81R49LLxqkB7Ir)

El archivo reúne **80 pantallas editables** del recorrido de interfaz del proyecto. La página `01 · Wireframes y recorrido` comienza con un índice enlazado a nueve secciones:

| Sección | Pantallas | Alcance |
| --- | ---: | --- |
| Inicio independiente | 8 | Nueva partida, Configuración, Salir, créditos y carga |
| Configuración y accesibilidad | 11 | Texto, contraste, movimiento, subtítulos, audio, gráficos y controles |
| Exploración y tutorial | 10 | Plaza Núñez, ayudas, objetivos, estaciones, portales y retorno |
| Inspección de objetos | 6 | Progreso, confirmación, manos y pérdida de seguimiento |
| Combate por turnos | 10 | Acciones, reacción, bloqueo, esquiva, resultado y errores |
| Pausa y diario | 7 | Pausa, mapa, objetivos y archivo cultural |
| Dispositivos y calibración | 10 | Micrófono, cámara y alternativas de entrada |
| Recuperación y cierre | 10 | Confirmaciones, derrota, fin, carga y mensajes |
| Extensiones y adaptación | 8 | Estados futuros identificados, texto al 150 % y formato 16:10 |

La página `00 · Guía y alcance` explica el recorrido y la distinción entre funciones actuales y propuestas. La página `02 · Componentes de wireframe` contiene los elementos reutilizables, estados, estilos de texto y variables de color. El menú principal tiene su propio flujo: muestra **Nueva partida**, **Configuración** y **Salir**; llega a la plaza solo después de iniciar una partida. Las opciones opcionales de cámara y micrófono aparecen con rutas alternativas.

Para revisar la navegación, abre la página de wireframes en modo presentación y entra por el índice o por uno de sus flujos: inicio, exploración, combate y calibración. Los botones y varias teclas (`Esc`, `Tab`, `H`, `E` y `Enter` según contexto) conducen a los estados correspondientes. Las transiciones de carga avanzan automáticamente como representación del flujo; su duración no mide la carga de Unity.

Los wireframes documentan el corte vertical actual y muestran algunas extensiones pendientes con esa etiqueta. Las escenas de los mundos completos, un sistema de guardado, inventario de consumibles y una conclusión narrativa requieren diseño e implementación posteriores; estas láminas no los presentan como funciones terminadas. La inspección por manos y los comandos de voz se representan como estados de interfaz, sin simular hardware en Figma.

Se verificaron las 80 pantallas, sus dimensiones y textos, y los destinos del prototipo: **310 conexiones entre pantallas**, más nueve accesos desde el índice, sin destinos rotos en la revisión. También se inspeccionaron visualmente las nueve secciones y ejemplos de progreso y texto ampliado. El documento de origen para la interfaz es [DESIGN.md](../../../DESIGN.md); el alcance de producto está en [PRODUCT.md](../../../PRODUCT.md). El inventario editable de pantallas y la validación estructural quedan en `wireframe-inventory.json` y `validation-final.json`.
