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
  ceden, estalactitas, IA de enemigos, jefe, portales conectados con la plaza y guardado.
- Pendiente de cámara: en la llegada a 01 el marco A04 queda entre la cámara y el jugador.

## Teclas de prueba

`1` doble salto · `2` sube el impulso (máx. 3) · `Q` impulso · `G` abre/cierra todas las rejas ·
`F1–F9` ir a la sala · `F12` oculta la ayuda.

## Validación (2026-10-01)

Prueba PlayMode en una copia del proyecto: las nueve salas apoyan al jugador en su altura y los nueve conectores
se recorren en ambos sentidos con el CharacterController real; los péndulos oscilan.
