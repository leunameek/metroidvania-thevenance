# El asedio de Bacatá · sistema de interfaz

> **Sistema vigente desde el 29 de septiembre de 2026:** [diseño integral](Specs/UI/Diseno-integral-2026-09-29.md), derivado del [documento entregado por el usuario](Specs/UI/El_Asedio_de_Bacata_UI_Design_System.md) y de su [referencia visual](Specs/UI/Referencia-visual-proporcionada.png). Sus colores, tipografía, pantallas y flujo sustituyen las decisiones incompatibles del registro histórico que sigue abajo. La implementación actual usa piedra tallada, pergamino, hueso y oro envejecido; el inicio incluye ranuras de partida, y la pausa incluye guardado e inventario de hallazgos. El [atlas navegable](Specs/UI/prototipo-integral.html) y su [ZIP autónomo](Specs/UI/Atlas-navegable-Bacata.zip) permiten revisar la composición.

> **Revisión del 29 de septiembre de 2026 · referencias de UI (bloques 1–4):** paleta Oro Muisca / pergamino / rojo ceremonial, títulos en **Cinzel**, placas talladas para botones y teclas, HUD reordenado sin superposiciones y tabla de contraste medida. Detalle en la [sección de revisión del diseño integral](Specs/UI/Diseno-integral-2026-09-29.md#revisión--referencias-de-ui-bloques-14-29-de-septiembre-de-2026).

## Registro histórico · 15 de septiembre de 2026

Dirección fijada por el encargo: piedra oscura, verdes profundos, oro mate y marfil. La geometría es funcional; no se atribuyen significados culturales a los bordes y controles. Dentro del juego, el escenario existente aporta ambiente. El inicio tiene una escena independiente y una ilustración de paisaje propia. No se modifica la geometría ni el diseño de personajes.

La exploración pertenece al modo **Experience**: la UI ocupa los bordes y deja libre al personaje. Menús, calibración y consultas pertenecen al modo **Operate**: encabezado, acción principal, contenido desplazable y regreso estable.

La fuente ceremonial es **Noto Serif Regular**; la funcional es **Noto Sans Regular**. Se incluyen los archivos y la licencia OFL. Los atlas SDF incorporan caracteres latinos, tildes, ñ y puntuación española.

## Inicio independiente

`Nemequene_MainMenu` presenta directamente Nueva partida, Configuración y Salir. Sigue la última referencia del usuario: título de montañas y sol centrado arriba, tres placas de piedra marrón con bordes y adornos dorados debajo, personaje con lanza y tela tejida a la izquierda, poblado y lago al amanecer. Las acciones usan Noto Serif en mayúsculas, con letras centradas horizontal y verticalmente. La selección ilumina la placa, aclara el texto y añade contorno dorado. El fondo completo se adapta al viewport sin recortar la tela ni el sol.

El flujo se simplifica a menú → partida, o menú → configuración → menú. No hay pantalla «presiona cualquier tecla», asistente obligatorio de dispositivos ni Continuar inactivo. Accesibilidad es la primera sección de Configuración. Los créditos, audio, pantalla y controles se consultan allí. Los ajustes persisten y se comparten con el juego.

La música original «Bruma del umbral» pertenece solo a esta escena, tiene volumen independiente y atajo M para silenciar. El paisaje tiene bruma en movimiento y una ondulación leve en el agua, mediante un único material. El emblema y los controles permanecen fijos. El efecto se congela con movimiento reducido, al abrir otra vista y al perder el foco de la aplicación. La salida musical conserva un fundido breve. Detalle en `Assets/Nemequene/UI/Documentation/04-Menu-inicio.md`.

### Título y paisaje vivo

La referencia más reciente fija el nombre visible **El asedio de Bacatá** y sustituye el emblema anterior por montañas, sol, letras de piedra desgastada y un pequeño remate geométrico. Se importa como PNG con transparencia real. Fondo, título y placas son recursos separados; el texto de los botones sigue siendo texto real. Al ampliar el texto, las placas se ensanchan conservando el centro. Alto contraste usa superficies lisas oscuras para priorizar la lectura. Los títulos funcionales del resto de pantallas conservan Noto Serif, marfil y oro. Los identificadores técnicos `Nemequene` se conservan para no alterar referencias, escenas ni preferencias. Evidencias en `Specs/UI/CenteredMenu`.

## Interfaz esencial · 15 de septiembre de 2026

El menú principal es la referencia para todas las superficies: Noto Serif en títulos, Noto Sans en lectura, verde casi negro, marfil y foco dorado con marca geométrica. La pausa usa el mismo velo lateral sobre el escenario; configuración y diario comparten navegación lateral y contenido desplazable. Se eliminan las leyendas permanentes de teclado, los subtítulos explicativos de cada acción y las etiquetas flotantes de las estaciones.

- **Exploración:** ningún control permanente. Vida solo con daño o durante combate. Nombre de zona durante tres segundos; objetivo durante unos segundos al cambiar, consultable siempre en el diario. Interacción únicamente cerca de una estación, duelo o portal. La cámara activada conserva un indicador para poder pausarla.
- **Pausa:** Reanudar, Diario, Configuración, Menú principal y Salir. El diario reúne mapa, objetivos y archivo; no se ofrece inventario vacío. Reiniciar está en Jugabilidad y conserva confirmación.
- **Inspección:** nombre, instrucción de la lección y del método de entrada activo, progreso y salida. El contexto completo de las piezas se conserva en el archivo. Los problemas de seguimiento incluyen la alternativa con mouse.
- **Combate:** vida, resistencia del guardián, turno, señal y acciones disponibles. Atacar aparece en el turno de ataque; ambas defensas durante la reacción; Volver al terminar. La voz solo aparece cuando está habilitada. Se mantienen ambas decisiones de defensa y las reglas del entrenamiento.
- **Ayuda y dispositivos:** H abre controles; también están en Configuración. Las calibraciones siguen siendo opcionales. Los avisos no repiten las instrucciones de la inspección ni los resultados del combate al regresar a explorar.
- **Diálogos, subtítulos, carga y confirmaciones:** misma paleta y botones. Contador de página solo para textos con varias páginas; ninguna acción inoperante durante carga. Los modales parten de Cancelar y conservan navegación por teclado.

## Tokens

| Función | Color |
|---|---|
| Fondo | #111815 |
| Superficie secundaria | #19352D |
| Selección de superficie | #2E5A47 |
| Verde claro | #6E9478 |
| Oro / oro claro | #D2A744 / #E8CC78 |
| Ocre / terracota / piedra | #A66E32 / #9F4938 / #817867 |
| Marfil / texto | #F0E7D2 / #F4ECD9 |
| Texto oscuro | #171A17 |
| Confirmación / advertencia / error | #5F9C78 / #D6A43D / #B84C3F |
| Información / desactivado / foco | #4F8492 / #686B64 / #F1D47F |

Los estados mantienen etiquetas escritas; el color no es el único indicador. La vida muestra cantidad y barra; los turnos y resultados tienen encabezados; los accesos explican su bloqueo. Los acentos por mundo se aplican sin mover controles.

## Escala y composición

Referencia 1920 × 1080; Canvas Scaler con Scale With Screen Size, Match Width Or Height = 0.5. Márgenes del 5 % horizontal y 4 % vertical. Encabezado principal 72; pantalla 44; sección 28–36; lectura 22–24; ayuda 18–20. Escala accesible 100, 125 y 150 %. Botones de al menos 56 unidades y carril de slider de 48. Separaciones de 8, 16, 24 y 48.

Los paneles usan anchors proporcionales. Los cuerpos extensos viven en ScrollRect con RectMask2D. El ajuste de altura se limita al contenido de las listas; no se reconstruye el árbol de UI en Update. TextMeshPro calcula la altura del texto y los Layout Groups dimensionan botones.

## Movimiento y audio

Selección de botón 120 ms; entrada de panel 220 ms; desaparición de aviso 300 ms; barra de vida interpolada. El ajuste de movimiento reducido aplica valores inmediatos y reduce las transiciones de portal. No se agregan vibraciones ni flashes blancos. Se reutiliza un sonido discreto de inspección ya existente, con volumen independiente y limitación de frecuencia.

## Componentes

`UIFactory` produce botones, botones de confirmación, selectores cíclicos, interruptores textuales, sliders, paneles, listas desplazables, textos, separadores y barras. `UIControlFeedback` conserva foco de teclado y desplaza el ScrollRect hasta el control seleccionado. `UIValueBinding` actualiza los valores cuando cambian los ajustes. Los controladores componen los HUD, calibración, mapa, archivo y modales.

La librería incluye 30 prefabs de componentes y una raíz funcional: botones, controles, paneles, indicadores y barras. `UIStatePresenter` permite configurar normal, hover, foco, presión, selección, desactivado, carga, éxito, advertencia y error. Los prefabs de dominio son superficies reutilizables a las que un controlador enlaza datos; no inventan datos del juego. Las pantallas se construyen una vez por escena con la fábrica y sus controladores; no hay una copia serializada independiente de cada pantalla.

Las capturas de Unity en `Specs/UI/Evidence/` son la referencia visual implementada. El mapa de composición y las limitaciones aparecen en la documentación técnica.
