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

## Personajes riggeados y animaciones (2026-10-06)

`Editor/Characters/CharacterLibrarySetup.cs` (se ejecuta solo una vez al compilar; también
*Nemequene > Personajes > Construir personajes*) arma el elenco desde `Art/Characters`:

- **Humanoides con rig de Mixamo** (Bachué, Quimue, Furachogua, Custodio, invasor, mujer-águila,
  hombre-caimán con y sin escudo, hombre-murciélago, jefe lagarto-murciélago, Nemequene niño y
  adulto): se importan como Humanoid y comparten los clips de `Nemequene/Animations` mediante
  `_Compartido/Bacata_Humanoid.controller` (locomoción + Talk, Talk2, Point, Reach, Pickup, Lever,
  Button, Open, Pray, Sit, Kneel, Crouch/Stand, Attack, Combo, Spin, Block, PowerUp, DodgeLeft/Right,
  HitLeft/Right/Gut, Death, DeathBack, Land). Los serenos usan *Breathing Idle* (override), los
  combatientes *Orc Idle*.
- **Rig humanoide de Tripo** (Tisquesusa, mujer-cóndor): avatar Humanoid construido con un mapa de
  huesos explícito; la mujer-cóndor usa `Mujer_condor_Optimizado.fbx` (40 k triángulos en vez de 2 M).
- **Criaturas** (serpiente, jaguar, guacamaya): animaciones creadas por código en Blender
  (`tools/Blender/animate_creatures.py` → `<carpeta>/Animations/<Nombre>_Animado.fbx`):
  serpiente Idle, AtaqueA, SacudidaA, AtaqueB, PulsoB, GolpeA, GolpeB, Emerger, Liberada, Reposo;
  jaguar Idle, Caminar, Correr, Embestida, Rugido, Golpe; guacamaya Idle, Despegue, Vuelo, Planeo,
  Aterrizaje.
- El reproductor de Nemequene (`Nemequene_Player.controller`) recibe los mismos estados de acción.
- Prefabs en `Resources/Characters/<Clave>` (raíz en los pies mirando a +Z, `CharacterActions`).

En el juego: quien habla en un diálogo gesticula (dos variantes); en los duelos Nemequene ataca,
bloquea, esquiva y recibe golpes, el enemigo amaga en el aviso y acusa los golpes; «Jaguar» llama
al jaguar, que ruge y embiste; la guacamaya sale volando de la urna vacía (H12). El guardián de
MI09 es el jefe lagarto-murciélago, la cima del mundo superior es la serpiente bicéfala, el
centinela de escudo es el hombre-caimán (pierde el escudo al romperlo). Nemequene recoge hallazgos,
jala la palanca, usa el cuerno y la llave, y se arrodilla en los descansos. En Bacatá: tallado y
entrega del bastón, meditación, muerte del tío y caída de Nemequene, Tisquesusa arrodillado.

Saguanmachica (`Art/Characters/Saguanmachica`, rig Mixamo) ya está en el prólogo y el epílogo.

**Proporción**: Nemequene mide siempre 1,85 m (`View/CharacterScale.cs`). Cada escena había guardado
una escala distinta del modelo (2,83 m en la plaza, 1,96 m en los mundos, 1,78 m en Bacatá);
`PlayerAnimator` mide el cuerpo en su pose base al iniciar y lo ajusta, y el prefab de Bacatá usa
la misma altura. El resto del elenco se mide igual (Bachué 1,70, Quimue 1,95, Custodio 2,10...).

**Enemigos comunes del inframundo** (`MundoInferior/MIDashEnemy.cs`, se colocan solos en las marcas
«(personaje pendiente)» del nivel): se pelean con el impulso; cada golpe se marca en el suelo antes
de llegar (un anillo translúcido que se llena hasta el golpe, `View/TelegraphMark.cs`) y al perder
se arrodillan, se deshacen en partículas y desaparecen (`View/CreatureDissolve.cs`; queda guardado).
Una caída al pozo no les devuelve la vida (solo la derrota de Nemequene), y el combate no se suelta
por los impulsos del propio combate. Todos son obligatorios: un lazo oscuro cierra la salida de su
sala hasta vencerlos (`MundoInferior/MIPassageSeal.cs`: 03, patio de 04 y 07).
- E03 Hombre-caimán (03, plataforma tras el pozo): guardia frontal (por delante el impulso hace la
  mitad, hay que rodearlo), mordida a la marca y coletazo en anillo. Vida 60.
- Dos centinelas caimán al inicio del patio de 04: solo mordida. Vida 30.
- E04 Hombre-murciélago vigía (04, entre los pilares): vuela fuera de alcance y dispara dardos (los
  pilares los detienen); luego cae en picada a la marca y queda en el suelo, expuesto. Vida 40.
- E05 Hombre-caimán de escudo (04, final): el centinela de escudo (cadena de tres impulsos).
- E06 Vigía del cuerno (07): grito que crece en anillo (un impulso durante la carga lo interrumpe y lo
  aturde) y picada. Vida 60.

El centinela de escudo, los jefes (guardián de MI09 y serpiente) y las pruebas del mundo superior
también se desvanecen al ser vencidos (los jefes y las mujeres-ave después de su última línea). Las
pruebas de la mujer-cóndor (antes de las alas) y la mujer-águila (antes de la llave) son obligatorias
también al abrir la escena sola desde el editor.

**Duelos por turnos** (`Controller/Duel/DuelAnimation.cs`): Nemequene queda en guardia (CombatIdle)
todo el duelo; el aviso es una carga: el enemigo levanta el golpe y lo sostiene, y lo suelta cuando
Nemequene responde (su defensa coincide con el golpe) o se acaba el tiempo; el golpe de Nemequene
hace reaccionar al enemigo cuando llega, no al pulsar. Tras cada intercambio vuelve a su sitio (los
esquives ya no lo sacan del mapa). El jaguar (afinidad) ruge, se agazapa, salta y cae; el turno
espera a que aterrice. Sus animaciones se rehicieron con cinemática inversa (patas plantadas, peso,
columna y cola).

**Encajar las piezas** (`Controller/FitPuzzle.cs`): brazaletes y ofrendas del inframundo, runas,
alas y medallón de la llave del mundo superior salen de su mesa girados al azar; hay que girarlos
hasta la posición en que asientan (sin silueta: la luz de la pieza se aviva y suena al acercarse, y
hace clic al asentar). Solo una pieza asentada se encaja («E · Encajar», «tomar»), y solo encajada
cuenta. Ofrendas y runas quedan en su mesa; brazaletes, alas y medallón los toma Nemequene.

**Acciones a tiempo** (`Controller/PlayerInteraction.cs`): palancas, cuerno, cierre, descansos y
hallazgos: Nemequene se gira, hace el gesto y el efecto ocurre en el contacto.

## Naturaleza: suelo, pasto, plantas, agua y cielo (2026-10-07)

Bacatá, la colina y la laguna de Iguaque ya no son planos y esferas: se construyen en tiempo de
ejecución con los shaders de `Resources/Nature` y el código de `Scripts/View/Nature`.

- **Suelo** (`NatureGround`, *Stylized Ground*): una malla redonda modelada por una función de
  altura, plana donde se actúan las escenas, con lomas onduladas después de la empalizada y cerros
  que cierran el horizonte. Se pinta sin texturas: dos verdes mezclados por ruido, manchas de paja
  seca, tierra en las pendientes, barro en la orilla, y caminos y claros de tierra (segmentos y
  círculos declarados en `BacataDirector`). La misma altura sirve para plantar rocas y plantas.
- **Pasto y flores** (`NatureGrass`, *Stylized Grass*): matas de hojas curvas dibujadas con
  instancias de GPU, sin un objeto por mata, en celdas que se descartan cuando quedan fuera de
  cámara. Cada hoja nace del color del suelo y se aclara hacia la punta; el viento las mece en
  ráfagas y el sol las atraviesa a contraluz. En la sabana, pasto verde con flores; en la colina y
  el páramo, pajonal dorado alto.
- **Plantas que se mecen** (`NatureFoliage`, *Stylized Foliage*): juncos, frailejones y los
  árboles y arbustos usan el mismo viento; el tronco queda quieto y la copa se mueve.
- **Agua** (*Stylized Water*): la laguna deja ver el fondo, que se dobla con las ondas y pasa a
  turquesa y luego a azul profundo con la hondura. Refleja el cielo en ángulo rasante, brilla al
  sol, recibe sombras y hace espuma en la orilla. Usa las texturas de profundidad y de opacos de la
  cámara (activas en `PC_RPAsset`). La orilla sale de la cuenca del suelo; los juncos se paran en
  el agua baja.
- **Cielo y grado de color** (`NatureAtmosphere`): el cielo andino procedural (*Nemequene/Andean
  Sky*) con cerros verdes y picos nevados detrás de los cerros de la malla, luz ambiente de tres
  tonos, bruma del color del cielo, y un grado de color suave (tonemapping, calidez, bloom y
  viñeta). Hay tres ambientes: mañana en la sabana, humo del incendio y aire claro del páramo.
- **Árboles y arbustos**: `BacataModelSetup` ya tiene registrados Aliso, Roble, Encenillo, Sauce,
  ManoDeOso, Chilco, Mortino y Chusque (carpetas en `Art/Environments/Bacatá/<Nombre>`; también
  acepta «Mano+de+oso» o «Mortiño»). Después de agregarlos, ejecutar *Nemequene > Campaña > Modelos
  de Bacatá*: `BacataDirector` los planta en bosquecillos alrededor de la aldea, junto a las
  casas, en la colina y en la orilla de la laguna, y los mece con el viento. Mientras falten, esos
  lugares quedan con pasto.
- **Mundo Superior** (`MSGardens`): las matas grises de vegetación baja (E06) se convierten al
  cargar en canteros de pasto y flores que se mecen con el viento de las alturas. La pieza gris
  queda oculta en la escena, así que el inventario del constructor no cambia.

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
