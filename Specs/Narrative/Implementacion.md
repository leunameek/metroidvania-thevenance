# Campaña de El asedio de Bacatá — qué está implementado (2026-10-06)

La historia H01-H21 del guion ya se juega de principio a fin con cinemáticas simples y figuras
provisionales. Cuando lleguen los modelos riggeados (ver `Personajes-y-animaciones.md`) se
sustituyen sin tocar la lógica.

## Recorrido

1. **Prólogo en Bacatá** (`Scenes/Bacata.unity`, `BacataDirector`): partida nueva → H01 el bastón,
   H02 la visión, H03 los siete días y Bachué (poporo y mapa) → Plaza Núñez.
2. **Plaza**: H04 al llegar; H05 en cada estación y antes del duelo de práctica; al terminar el
   tutorial se abre solo el portal inferior.
3. **Mundo inferior**: líneas de H06-H09 al entrar en cada sala y al encontrar cada pieza; el Custodio
   de Raíces junto al altar de la semilla responde preguntas; la **coca** aparece en el altar del
   tercer par cuando están la semilla y los tres brazaletes; la **máscara de Chía (sellada)** aparece
   junto al soporte del cuerno cuando se abre la reja; el guardián de MI09 pide ambas cosas y es un
   **duelo por turnos** (E07, dos ataduras que solo rompe «Jaguar»). Al vencerlo Chía queda libre.
4. **Plaza, mitad del viaje**: Bachué junto a la fuente recibe Chía y da el yopo (H11); de las tres
   urnas junto al portal superior, la del medio está vacía (H12) y abre el mundo superior.
5. **Mundo superior**: líneas de H13-H15 con las runas, las alas y la llave; dos pruebas nuevas por
   turnos: **mujer-cóndor** (E09, después de la runa 2, junto a la terraza de 03) y **mujer-águila**
   (E10, en la terraza de la llave; la llave solo se ofrece tras superarla). La cima es la
   **serpiente bicéfala** (E08, cabezas A/B); al vencerla aparece la **máscara de Sué** en el altar.
6. **Plaza, Quimue**: al volver con Sué entra Quimue (H17) y espera en el círculo de entrenamiento;
   **duelo final** por turnos (E11, anclajes de luna y sol que se cortan con «Vincular»).
7. **Desenlace**: hablar con Bachué (H18) → Bacatá en llamas (H19), refugio de Tunja (H20), Iguaque y
   Furachogua (H21) → créditos → menú.

El portal superior permanece cerrado hasta la urna. Al abrir una escena directamente desde el
editor (sin ranura de guardado) todo está abierto y la afinidad del jaguar disponible, para probar.

## Pruebas: saltar a un capítulo (temporal)

En el editor y en builds de desarrollo aparece a la izquierda el botón **Capítulos · F9**. Cada
entrada borra la historia y los dos mundos de la ranura actual, deja la partida justo al inicio
de ese capítulo (los diálogos anteriores cuentan como escuchados) y carga su escena. Código:
`Controller/Campaign/CampaignJump.cs` y `View/Campaign/ChapterSkipMenu.cs`; quitar ambos antes de
la versión final. Nota: el registro propio de la plaza (estaciones y entrenamiento) no se borra,
así que «Continuar» desde el menú tras saltar hacia atrás puede reabrir el portal inferior.

## Voz, manos y carga

- La voz viene **activada** y escucha sin mantener Ctrl (los ajustes guardados antes se migraron
  una vez; en Ajustes se puede volver a pedir Ctrl para hablar). Con la voz activa, cada aviso
  muestra la palabra que hay que decir en lugar de la tecla («examinar», «entrar», «hablar»,
  «tomar», «salir», «impulso»…); sin voz vuelve a mostrar la tecla (`Controller/VoicePrompt.cs`).
- Inspección con manos como en el prototipo inicial (`Controller/HandInspection.cs`): mano izquierda
  abierta gira, mano derecha abierta inclina, dos puños detienen, puño sostenido toma. La usan las
  estaciones de la plaza, los hallazgos de ambos mundos (coca, máscaras, runas…) y las **urnas**,
  que ahora se toman y hay que girar/inclinar para ver dentro antes de usarlas
  (`PlazaNunez/PlazaPieceInspection.cs`).
- Cada cambio de escena pasa por una pantalla de carga con la ilustración y el nombre del destino
  (`Controller/SceneLoader.cs`).

## Controles de diálogo y duelo

- Diálogo: E / Espacio / Enter / clic o decir «siguiente» («next») avanza; **mantener Esc 1 s** salta
  (aplica lo mismo que terminar).
- Duelo: 1 Atacar, 2 Contraatacar, 3 Jaguar, 4 Cuerno, 5 Anclar, 6 Interrumpir, 7 Vincular;
  Z / X (o flechas) eligen Cabeza A/B o Luna/Sol; F Bloquear, Espacio Esquivar, G Cubrir, R Parar.
  También botones en pantalla y voz en español o inglés (atacar/attack, bloquear/block, esquivar/
  dodge, cubrir/cover, parar/parry, contraatacar/counter, jaguar, cuerno/horn, anclar/anchor,
  interrumpir/interrupt, vincular/bind, izquierda/left, derecha/right, luna/moon, sol/sun).
  La decisión ofensiva no tiene tiempo; la defensa se responde al aparecer «¡RESPONDE!».
- Los enemigos comunes del inframundo (caimán, murciélago, escudo, vigía) siguen con el dash.

## Dónde está el código

| Pieza | Archivo |
|---|---|
| Estado de la campaña, capítulos, objetivos | `Scripts/Model/Campaign/CampaignModel.cs`, `CampaignFlags.cs` |
| Guion en datos | `Resources/Narrative/historia.json` (generado con `tools/narrative/build_story_json.py`) |
| Dónde suena cada línea | `Scripts/Model/Campaign/StoryTriggers.cs` |
| Guardado por ranura | `Scripts/Controller/Campaign/CampaignProgress.cs` (clave `Bacata.Campaign.v1.<ranura>`) |
| Reproductor de diálogos y cámara | `Scripts/Controller/Campaign/StoryPlayer.cs`, `StoryActor.cs`, `View/Campaign/StoryDialogueView.cs` |
| Motor de duelo y encuentros | `Scripts/Model/Duel/TurnDuelModel.cs`, `DuelEncounters.cs` |
| Duelo en escena y HUD | `Scripts/Controller/Duel/TurnDuelController.cs`, `View/Duel/TurnDuelHUD.cs` |
| Inframundo | `MIGuardian.cs` (E07), `MundoInferiorBlockout.SpawnStoryPieces` (coca, Chía, Custodio) |
| Mundo superior | `MSGuardian.cs` (E08), `MSDuelEncounter.cs` (E09/E10), `MundoSuperiorDirector.SpawnStoryPieces` (Sué) |
| Plaza | `PlazaCampaign.cs` (Bachué, urnas, Quimue, desenlace), `PlazaStoryPoint.cs` |
| Prólogo y epílogo | `Scripts/Controller/Campaign/BacataDirector.cs`, `Editor/Campaign/BacataSceneBuilder.cs` |

## Sustituir las figuras provisionales

- Personas en la escena de Bacatá: asignar el prefab en el `BacataDirector` (o volver a ejecutar
  *Nemequene > Campaña > Crear o actualizar escena de Bacatá*, que busca
  `Art/Characters/<Nombre>/<Nombre>.prefab`).
- Personajes y objetos creados en tiempo de ejecución: un prefab en `Resources/Characters/<Nombre>`
  (Bachue, Quimue, CustodiodeRaices, MujerCondor, MujerAguila) o `Resources/Props/<Nombre>` (Coca,
  MascaraChia, MascaraSue, UrnaVacia, UrnaMemoria) reemplaza la figura automáticamente.

## Pendiente

- Animaciones y modelos (lista en `Personajes-y-animaciones.md`); transformación en jaguar y
  guacamaya como cinemática con los modelos.
- Cinemáticas C04-C17 más elaboradas (hoy: líneas con encuadre del hablante y fundidos).
