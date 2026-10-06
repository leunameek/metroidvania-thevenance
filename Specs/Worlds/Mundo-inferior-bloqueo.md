# Mundo inferior · nivel jugable (etapas 4–8 de la guía, sin personajes)

Escena: `Assets/Worlds/MundoInferior/Scenes/MundoInferior_Blockout.unity`, generada por
`Assets/Prototype/Editor/MundoInferior/MundoInferiorBlockoutBuilder.cs` (menú **Nemequene › Mundo Inferior ›
Construir nivel**; se ejecuta sola una vez por versión, clave `MundoInferior.Experience.v2`). La escena se regenera
completa en cada ejecución: no editarla a mano, cambiar el constructor.

## Recorrido completo (1 de octubre de 2026)

| Zona | Qué hay ahora | Resultado guardado |
|---|---|---|
| 01 Umbral | Portal de llegada y regreso (E), puerta del atajo, ofrenda 1 | Zona descubierta |
| 02 Santuario | Semilla (inspección, doble salto), descanso, ensayo, ofrenda 2 en el suelo de recuperación | `semilla`, checkpoint 02 |
| 03 Brazaletes | Brazaletes 1 (impulso 1), palanca del atajo (abre 03 y 01), pozo de ensayo, ofrenda 3 | `brazaletes1`, `atajo_03_01` |
| 04 Centinelas | Brazaletes 2 y 3, carril de cadena, **centinela de escudo provisional**, ofrenda 4 | `brazaletes2/3`, `salida_04` |
| 05 Péndulos | Dos péndulos desfasados con golpe (15) y caída a pinchos (10) | — |
| 06 Derrumbe | Losa aislada y tres losas que ceden, dos estalactitas con sombra, palanca del corredor, ofrenda 5 | `corredor_06` |
| 07 Cuerno | Pozo con apoyos, cuerno (inspección) | `cuerno` |
| 08 Antesala | Descanso, soporte del cuerno que abre la reja ritual, ofrenda 6 | `reja_cuerno`, checkpoint 08 |
| 09 Guardián | **Guardián provisional**: barrido, onda que se salta, dos piedras anunciadas; núcleo expuesto | `guardian`, portal de victoria |

Los personajes (centinelas, vigía, custodios y los guardianes reales) quedan como marcadores
`... (personaje pendiente)` en sus posiciones de la guía. El centinela de escudo y el guardián del fondo son
sustitutos de piedra que ya cumplen las reglas de bloqueo (tercera cadena para romper el escudo; daño solo al
núcleo expuesto) para cambiarlos por el personaje sin tocar el flujo.

## Sistemas (Prototype.Runtime, `Controller/MundoInferior`)

- `MIProgress`: estado persistente por ranura (`Bacata.MI.v1.<ranura>`, ranura que fija `GameSaveStore` a través de
  `WorldTravel.SaveSlot`). Las capacidades se derivan de los hallazgos (nunca se suman), así que no hay duplicados.
  Una partida nueva o una ranura borrada reinician el mundo inferior. Las ofrendas aparecen en el Archivo cultural.
- `MundoInferiorBlockout` (director): HUD, objetivo en una frase, contadores, avisos E, pausa (Esc), daño con un solo
  evento por caída y 1 s de protección, derrota con regreso al último descanso, ambiente y goteo.
- `MIFind` (hallazgos y ofrendas con inspección: E examina, ratón o A/D giran, E confirma, Esc devuelve),
  `MILever`, `MIRest`, `MIHornSocket`, `MISlab`, `MIStalactite`, `MIShieldSentinel`, `MIGuardian`, `MIGate`
  (lee su flag), `MIPortal` (flag requerido), `MIPendulum` (sonido al cruzar el carril).
- Vista: `View/MundoInferior/MIHud.cs` con el kit visual de Bacatá.

## Arte, animación y sonido

- Texturas tileables propias (`tools/textures/generate_mundo_inferior_textures.py`): suelo azul verdoso en las caras
  transitables y piedra azul grisácea en los costados (paleta de la guía 5.1). Los bloques usan UV en metros.
- Decorado con el kit Tripo fuera de los carriles, paredes de caverna lejanas, partículas de polvo, goteo y destellos,
  luces por sala, cristales que respiran, hallazgos que flotan y giran, rejas, palancas, losas, piedras y portal animados.
- 35 sonidos sintetizados (`tools/audio/generate_mundo_inferior_audio.py` → `Resources/MIAudio`).

## Validación (1 de octubre de 2026)

Prueba PlayMode en una copia del proyecto: arranque sin capacidades, semilla con inspección y doble salto una sola
vez, descanso con checkpoint, ofrenda contada, palanca que abre las dos puertas del atajo, inicio del guardián al
entrar a la arena, y derrota que regresa al descanso conservando los hallazgos. Capturas revisadas sala por sala.

---

# Mundo inferior · bloqueo jugable (etapas 1–3 de la guía)

Escena: `Assets/Worlds/MundoInferior/Scenes/MundoInferior_Blockout.unity`, generada por
`Assets/Prototype/Editor/MundoInferior/MundoInferiorBlockoutBuilder.cs` (menú **Nemequene › Mundo Inferior ›
Construir bloqueo**; se ejecuta sola una vez, clave `MundoInferior.Blockout.v1`). La escena se regenera
completa en cada ejecución: no editarla a mano, cambiar el constructor.

## Kit optimizado

`tools/Blender/optimize_mundo_inferior.py` (Blender 5.2, `blender -b --python tools/Blender/optimize_mundo_inferior.py [-- <carpeta> IDs]`)
reduce los Tripo de `Assets/Models/Mundo_Inferior` (~2 M triángulos, 55–65 MB cada uno) a
`<carpeta>/<ID>_Optimizado.fbx` (15–60 k triángulos; 44 modelos, 47 MB en total). La tabla carpeta → ID
está en el script. `Mundo_Inferior` está en `.gitignore`: los modelos y sus `.fbm` no viajan con git.
No se optimizaron `stone+arch+3d+model` (A01a, exportación dañada de 464 triángulos) ni
`stone+table+3d+model (1)` (copia idéntica de H03a).

## Disposición global

Cada sala tiene su origen en el centro del borde de entrada (guía 1.2) y se gira con su raíz.

| Sala | Entrada (mundo) | Giro Y | Altura | Notas |
|---|---|---|---|---|
| 01 Umbral | (110, 0, 80) | −90 | 0 | |
| 02 Santuario | (94, −2, 80) | −90 | −2 | Ensayo: T01 a 1,8 y llegada a 2,6 (locales) |
| 03 Brazaletes | (75, 2, 80) | −90 | +2 | |
| 04 Centinelas | (31.5, −6, 78) | 180 | −6 | 44 m de fondo |
| 05 Péndulos | (36, −6, 28) | 90 | −6 | |
| 06 Derrumbe | (64, −10, 5) | −90 | −10 | |
| 07 Cuerno | (24, −14, 5) | −90 | −14 | |
| 08 Antesala | (28, −14, 26) | −90 | −14 | |
| 09 Guardián | (16, −14, 26) | −90 | −14 | Continúa el suelo de 08 |

Conectores (`Traversal`): rampas de 2 m de desnivel cada 4 m (26,6°) con rellanos de 2 m; las esquinas solo
unen tramos planos. El atajo 03→01 pasa al sur de 02 (z 70). El corredor seguro de 06 desemboca en el
acceso 07→08 antes de la reja del cuerno (en vez de una entrada lateral propia en 08).

## Qué hay y qué falta

- Suelos, paredes y apoyos son cajas con collider; el kit Tripo es solo visual (sin colliders, salvo
  altares, palancas y bots). Los personajes sin modelo (C01, C02a, C03a, C04, C05) son el bot de
  entrenamiento de la plaza, a la altura indicada en la guía.
- Funciona: recuperación por pozo (`MIFallZone`) y una sola red de seguridad global a y −30
  (`MundoInferiorBlockout`), cámara fija que gira por sala (`MICameraZone` → `ExplorationOrbitCamera.SetYaw`),
  péndulos que oscilan (35°, 3 s, desfase de media oscilación) y cuyo golpe devuelve al ancla, rejas
  `MIGate` con bisagra.
- Pendiente (etapas 4+): recompensas reales e idempotentes, palancas y soporte del cuerno con E, losas que
  ceden, estalactitas, IA de enemigos, jefe, portal de victoria de 09 y guardado del progreso propio del mundo inferior.
- Pendiente de cámara: en la llegada a 01 el marco A04 queda entre la cámara y el jugador.

## Viaje desde y hacia la plaza

`WorldTravel` (Prototype.Runtime) conecta las escenas: el portal del mundo inferior de Plaza Núñez (world −1),
una vez superado el tutorial, carga esta escena en lugar del umbral de demostración del hub; el jugador llega
al Spawn 01. En 01, el A04 se usa con E (sin teletransporte por contacto) y vuelve a la plaza: se restauran
lecciones, entrenamiento, mundos visitados y tiempo de juego, y el jugador aparece 4,5 m delante del portal,
fuera de su radio. El constructor añade la escena a Build Settings. El A04 de 09 queda inactivo hasta el jefe.
El mundo superior sigue usando su umbral dentro del hub.

## Teclas de prueba

`1` doble salto · `2` sube el impulso (máx. 3) · `Q` impulso · `G` abre/cierra todas las rejas ·
`F1–F9` ir a la sala · `F12` oculta la ayuda.

## Validación (2026-10-01)

Prueba PlayMode en una copia del proyecto: las nueve salas apoyan al jugador en su altura y los nueve conectores
se recorren en ambos sentidos con el CharacterController real; los péndulos oscilan.
