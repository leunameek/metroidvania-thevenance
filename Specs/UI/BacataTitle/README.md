# El asedio de Bacatá · título y paisaje vivo

## Entrega

El emblema de la referencia del usuario se integra sobre el paisaje existente del menú. El título ocupa la parte superior izquierda; Nueva partida, Configuración y Salir quedan debajo, con foco dorado, texto legible y los controles habituales. Créditos y confirmación de salida usan el mismo nombre visible. Se conservan los nombres técnicos de escenas, preferencias y ejecutable.

El fondo usa un único material: bruma lenta sobre el valle y ondulación leve sobre el agua. La animación no desplaza el título ni las acciones. Se detiene con Movimiento reducido, al entrar a otra vista y cuando la aplicación pierde el foco. El material se libera al abandonar la escena.

## Arte

- Archivo integrado: `Assets/_Game/UI/Resources/Nemequene/Title_Bacata.png`, PNG RGBA de 1672 × 941, con transparencia real.
- Herramienta: generación/edición de imágenes integrada (`image_gen`), a partir de la referencia adjunta del usuario.
- El paisaje `Menu_Lagoon.png` se conserva. La animación se calcula en Unity; no requiere vídeo ni servicios externos durante el juego.

Prompt utilizado:

> Use case: background-extraction. Asset type: transparent game title emblem for an actual Unity main menu. Edit the attached reference by extracting ONLY the central gold and black stone title emblem with its jaguar on the left, eagle on the right, central sun, lower paired serpents and ornamental rods. Keep the reference lettering exactly: upper line 'EL ASEDIO DE', very large lower line 'BACATÁ' with acute accent on final Á. Preserve aged gold, carved stone, bevel lighting and composition of emblem. Remove ALL landscape, sky, plants, fog, buildings and rectangular background: genuine transparent RGBA alpha everywhere outside the emblem and through its negative spaces, no checkerboard drawn into the image. Center the entire emblem fully uncropped in a wide canvas, with about 4% transparent margin. No extra words, watermarks, scenery or glow halo. Deliver a high-resolution transparent PNG usable as a UI texture.

## Validación

El recorrido automatizado comprueba la composición en cuatro resoluciones y texto al 150 %, las tres acciones, teclado, ratón, contraste, entrada a partida y regreso. Incluye compilación del shader, transparencia del emblema, diferencia de píxeles en dos capturas separadas por ocho segundos, pausa de la animación detrás de Configuración y congelación con Movimiento reducido.

**61 comprobaciones aprobadas y 0 errores de ejecución**, registradas en `validation.txt`. Capturas en 1280 × 720, 1920 × 1080, 2560 × 1440 y 1920 × 1200, incluyendo texto al 150 %. `ambient-before.png` y `ambient-after.png` muestran el paisaje con ocho segundos de separación. Los diagnósticos internos del buscador del editor se distinguen de los errores del juego.

**Windows:** compilación completada sin errores y prueba del ejecutable entregado aprobada (inicio, configuración, partida, pausa y regreso). Registros: `windows-build.txt` y `windows-smoke.txt`; captura del ejecutable: `windows-main-menu.png`. Ejecutable actualizado: `Builds/Nemequene/Nemequene.exe`.
