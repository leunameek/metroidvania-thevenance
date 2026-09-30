# Menú de inicio independiente

## Problema resuelto

El menú anterior se dibujaba encima de la plaza, exigía pasar por Inicio y Primera configuración y presentaba siete acciones al mismo nivel. El nuevo inicio tiene una escena propia y tres acciones: **Nueva partida**, **Configuración** y **Salir**. No se carga el tutorial ni se crean sus controladores de cámara o voz hasta comenzar una partida.

Nueva partida lleva directamente a la Plaza Núñez. No solicita permisos ni calibración obligatoria. Configuración abre Accesibilidad; audio, pantalla, controles y créditos quedan como secciones. Esc vuelve conservando el foco en la acción utilizada. Salir pide confirmación y selecciona Cancelar inicialmente. El menú de pausa regresa a esta misma escena, descargando la plaza.

## Identidad y accesibilidad

- Ilustración de poblado, laguna y montañas al amanecer, con personaje y tela a la izquierda y sol a la derecha. El fondo completo se adapta a cada viewport.
- Título «El asedio de Bacatá» con montañas y sol, centrado arriba según la referencia más reciente del usuario. Placas de piedra marrón, bordes y adornos de oro envejecido, Noto Serif en mayúsculas para las tres acciones.
- Las tres acciones caben en pantalla sin desplazamiento, también al ampliar texto.
- Foco con contorno dorado y cambio de iluminación/texto. Las placas se ensanchan al ampliar texto; alto contraste usa superficies uniformes oscuras.
- Flechas, Tab, Shift+Tab, Enter y Esc; mouse sobre controles grandes. La entrada estándar de Unity conserva navegación de mando, aunque esta ejecución no acredita un mando físico.
- Tamaño 100/125/150 %, contraste alto, movimiento reducido y opciones de subtítulos/reacción compartidas con el juego.
- El contenido de Configuración usa scroll y lleva automáticamente el control seleccionado a la vista.
- Resolución reversible durante 15 segundos; cancelar o agotar el tiempo restaura el modo anterior.
- El paisaje tiene bruma lenta y una ondulación leve limitada al agua. Título y botones permanecen fijos. Movimiento reducido congela el efecto; también se pausa en Configuración, confirmaciones, carga y al perder el foco. No usa vídeo, capturas de cámara ni bibliotecas adicionales.

## Archivos

| Archivo | Función |
|---|---|
| Scenes/Nemequene_MainMenu.unity | Primera escena de Build Settings; cámara, listener y raíz de menú |
| Scripts/TitleMenuController.cs | Composición, configuración, foco, transiciones, carga y música |
| Scripts/Components/TitleMenuButton.cs | Estados de selección y respuesta accesible |
| Scripts/Components/TitleMenuBackdrop.cs | Velo de lectura y variante de contraste |
| Scripts/Components/TitleMenuAtmosphere.cs | Reloj ambiental, preferencias y ciclo de vida del material |
| Resources/Nemequene/MenuAtmosphere.shader | Bruma y agua en un único pase sobre el paisaje |
| Resources/Nemequene/Title_BacataMountains.png | Título de montañas y sol con transparencia |
| Resources/Nemequene/Menu_BacataValley.png | Poblado y lago al amanecer, personaje y tela lateral |
| Resources/Nemequene/Menu_StoneButton.png | Placa sin texto, importada como sprite con bordes adaptables |
| Resources/Nemequene/Menu_Bruma.wav | Música original de 60 segundos en bucle |
| Editor/TitleMenuSceneBuilder.cs | Generación de escena e importación de arte/audio |
| Editor/TitleMenuValidation.cs | Recorrido automatizado, resoluciones y capturas reales |
| Tools/generate_menu_music.py, en la raíz | Fuente reproducible de la composición |

También se actualizaron UIManager, MenuController y LoadingScreenController para separar la entrada de aplicación del tutorial; SettingsManager y SettingsUIController comparten el volumen musical. Los textos continúan centralizados en es.json. Las pruebas existentes se adaptaron al nuevo punto de entrada.

## Música

«Bruma del umbral»: composición original por síntesis, sin muestras ni música de terceros. Armonía lenta, notas pulsadas, aire filtrado y colas suaves. Dura 60 segundos, estéreo, 44.1 kHz; Unity la importa como Vorbis en streaming. Volumen inicial 30 % del canal musical, multiplicado por el volumen general. M silencia/restaura el canal; también hay un slider en Configuración → Sonido. Se detiene al descargar la escena y no acompaña al tutorial.

Las medidas objetivas y la procedencia están en `Specs/UI/TitleMenu/music-validation.json`. No representa una reconstrucción musical muisca.

## Arte y procedencia

El fondo actual, el título y las placas se prepararon con la herramienta integrada de edición de imágenes a partir de la referencia del usuario. Son arte de fantasía; no constituyen una reconstrucción histórica. Los prompts actuales están en `Specs/UI/CenteredMenu/art-prompts.md`.

Prompt del paisaje anterior, conservado como antecedente:

> Use case: stylized-concept. Asset type: full-screen background artwork for the Windows game Nemequene's dedicated main menu, 16:9 landscape, 2560x1440 or larger. Generate only background art, NO TEXT and NO UI. Cinematic painterly environment concept art, mature restrained fantasy rooted in a Colombian Andean highland cloud forest and a still mountain lagoon at predawn. Composition supports readable menu on left: the left 45 percent is deep shadowed soft forest silhouettes and mist with very low visual detail, no bright highlights. Right half is the focal artwork: layered green mountains and weathered dark rocks framing a distant lagoon, a low pale warm golden sun diffused through mist near upper right, faint warm reflection on water. Fine natural vegetation at the bottom right, rich painterly textures, confident forms and depth, inviting quiet mystery, atmospheric perspective. Palette deep pine green #111815 #19352D, muted moss, aged warm gold #D2A744, soft ivory mist. Art fills the full image edge to edge, visually legible cinematic matte painting. No people, no buildings, no symbols, no artifacts, no text, no lettering, no logo, no watermark, no neon, no futuristic elements, no fake archaeological motifs. This is fictional environmental art, not a historical reconstruction.

## Cómo probar

Abre `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity` en Unity 6000.3.21f1 y pulsa Play, o ejecuta `Builds/Nemequene/Nemequene.exe`. Comprueba Nueva partida, Esc → Volver al menú, las secciones de Configuración, el tamaño de texto y la cancelación de Salir. Abrir TechnicalDemo_Week08 directamente permite probar solo el tutorial.

En una copia aislada puede ejecutarse `-executeMethod Nemequene.UI.Editor.TitleMenuValidation.Run`. `UIAssetBuilder.WindowsBuild` incluye la escena de inicio como primera escena y conserva las otras escenas habilitadas. El resultado y las capturas se guardan en `Specs/UI/TitleMenu` al recoger las evidencias de la copia de pruebas.

## Validación del menú

La revisión actual se documenta en `Specs/UI/CenteredMenu/README.md`. La versión anterior del emblema queda en `Specs/UI/BacataTitle/README.md`. Las cifras siguientes corresponden a la versión inicial del menú.

**76 comprobaciones aprobadas, cero errores de ejecución.** Se verificaron las tres acciones exactas, foco inicial, flechas, Tab, Enter, Escape, click real de mouse mediante Input System, configuración sin cargar la plaza, silencio/restauración musical, preferencias, confirmaciones, resolución reversible, carga directa y regreso desde pausa.

Capturas reales de Unity: 1280 × 720, 1920 × 1080, 2560 × 1440, 1920 × 1200; adicionalmente texto 150 % y alto contraste en 720p y 16:10. Se comprobaron límites de texto y contraste mínimo 4.5:1 para las acciones enfocadas. El marcado visual del foco quedó confirmado en las capturas finales. Esta verificación no declara compatibilidad probada con lectores de pantalla ni con dispositivos físicos de asistencia.

Errores corregidos durante la integración: creación de escena desde un editor batch sin escena guardada; referencia de script al trasladar la escena al proyecto original; tinte heredado que reducía el contraste del foco. Permanece el diagnóstico interno de UnityEditor.Search.SearchDatabase ya documentado, separado de los errores de ejecución.

La regresión de la plaza pasó **61 comprobaciones, cero errores de ejecución**: movimiento, pausa, tres piezas, archivo, mapa, combate, portales, diálogo, subtítulos, reinicio y regreso al nuevo menú. Incluye una migración de preferencias anteriores que conserva volumen y tamaño de texto y añade el volumen musical inicial sin restablecer los ajustes del jugador. Informe: `Specs/UI/TitleMenu/gameplay-regression.txt`.
