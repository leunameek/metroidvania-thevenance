# Revisión y completado del sonido — El asedio de Bacatá

Fecha: 2026-10-06. Rama: `claude/asedio-bacata-audio-6b4c53` (sobre `test` 3138d32).

## 1. Cómo se revisó

No hay escucha humana posible desde esta sesión. Cada audio se evaluó con:

- **Medición de señal** de los 99 WAV del proyecto: pico, RMS, factor de cresta, saturación, offset DC,
  arranque y final (cortes), costura de bucle, suelo de ruido, centroide, planitud espectral y bandas.
- **Espectrogramas** de todos los clips (lectura visual de contenido: tonos puros, ruido, golpes, colas).
- **Lectura del código** que los dispara (qué evento, cuándo, cuántas veces, con qué volumen y canal).
- **Partida grabada**: un recorrido automático (título, plaza, Mundo Inferior, Mundo Superior, duelos,
  prólogo y epílogo de Bacatá) que graba la salida real del `AudioListener` y la línea de tiempo de
  cada sonido; después se midió nivel, picos, duplicados y sonidos durante la pausa.

Criterio: se **conserva** lo que representa bien su acción y está limpio; se **repara** lo que suena bien
pero tiene un defecto técnico; se **reemplaza** lo que no corresponde (tonos senoidales "electrónicos"
para objetos de barro, piedra o madera, sonidos de golem de piedra para criaturas, pitidos de interfaz).

Los originales de todo archivo reemplazado o reparado están en `Audio_Originales/` (misma ruta relativa).
Los reemplazos sobrescriben el contenido del mismo archivo: GUID y referencias de escenas y código intactos.

## 2. Identidad sonora

Fantasía muisca, exploración y terror psicológico: misterio, tensión y presencia espiritual sin saturar.
Paleta orgánica: viento, hojas, aves, agua, madera tallada y hueca, barro cocido, piedra, tela, semillas,
oro y plata golpeados (Quimue). Instrumentos ficticios y genéricos: flauta de soplo, silbato de barro,
tronco hendido, tambor de marco, sonajero de semillas. **No se presentan como instrumentos ni música muisca
auténticos**: no hay fuente que lo respalde; son una paleta de fantasía coherente con el arte.

Motivos recurrentes (tomados del guion C): el **motivo del bastón** (madera + flauta, C01, C20, C21),
el **zumbido de dos tonos** de Quimue (oro/plata, C02, C13, C16), el **pulso lunar / solar**, la
**rotura de resonancia** (C17, "no explosión") y la **liberación** en lugar de victoria triunfal (C09).

## 3. Inventario

### 3.1 Conservados sin cambios (funcionan bien)

| Banco | Audios | Motivo |
|---|---|---|
| MIAudio | cuerno, escudo_bloquea, estalactita_aviso, golpe_nucleo, golpe_piedra, gota, guardian_carga, guardian_despierta¹, losa_cae, pendulo, portal, reja_abre, reja_cierra | Piedra, agua y cuerno coherentes con la caverna; limpios. |
| MSAudio | alas_abrir, alas_cerrar, aleteo, escalada_agarre_1-3, escalada_salida, golpe_nucleo, jefe_aviso¹, jefe_despierta¹, portal_viajar, recuperacion, transporte_arranque/bucle/parada | Viento, piedra ligera y alas de cerámica/tela; limpios. |
| UI | Menu_Bruma.wav (música del título) | Pieza ambiental limpia, bucle sin costura. |

¹ Solo suenan con el sustituto de piedra (si falta el modelo de la criatura).

Conservados pero **ya sin uso** (se dejan por si se vuelven a necesitar): MS `aterrizaje`, `salto`,
`paso_piedra_1-4`, `defensa_bloqueo/esquiva/fallida`; MI `guardian_barrido`. Los sustituyen el sistema de
pasos común y el sonido de duelo por enemigo.

### 3.2 Reparados (mismo sonido, defecto corregido)

| Audio | Defecto medido | Corrección |
|---|---|---|
| MI caida, estalactita_golpe, guardian_cae, guardian_golpe, losa_grieta, palanca, escudo_rompe | El ruido del golpe se cortaba en seco 20–32 dB por encima del silencio | Fundido de 0,2 s antes del corte |
| MS cierre_puerta | Corte brusco a −29 dB (audible) | Fundido de 0,35 s |
| MS jefe_golpe, aterrizaje_fuerte, portal_llegada | Corte de cola | Fundido de 0,2 s |
| MS musica_exploracion (33,8 s), musica_percusion (33,0 s) | Bucle con ~2 s y ~1 s de silencio; capas de distinta duración que se desfasaban | Plegado exacto a 32,0 s (75 ppm), colas sobre el inicio |
| MS musica_jefe (20,4 s) | ~1 s de silencio en cada vuelta | Plegado a 19,2 s |
| MI musica_guardian (17,2 s) | ~1,2 s de silencio en cada vuelta | Plegado a 16,0 s |
| MS vuelo_viento, viento_alturas, portal_zumbido | Escalón en la costura del bucle (clic) | Fundido cruzado de potencia constante |

### 3.3 Reemplazados (no correspondían)

| Audio | Problema | Nuevo contenido |
|---|---|---|
| Plaza: Step, Jump, Land, Dash | Golpe sinusoidal de 84 Hz; salto = "boing" ascendente 220→480 Hz | Pisada de cuero sobre piedra, impulso con tela, aterrizaje con peso |
| Plaza: Inspect, Rotate, Complete, Victory | Pitidos senoidales puros (587 Hz, 740 Hz, acorde mayor) | Aire + pieza de barro; fricción suave; frase de descubrimiento; motivo del bastón |
| Plaza: Attack, Impact, Guard, Warning | Ruido + seno; aviso = zumbido trémolo de 185 Hz | Bastón cortando el aire; madera sobre el guardián de práctica; bloqueo de madera; crujido de madera tallada + tambor |
| Plaza: Portal | Barridos senoidales | Soplo, tubo de viento, barro y tambor |
| Plaza: Ambience_Plaza / Inferior / Superior | Ruido con pájaro sintético; **zumbido de 55 Hz**; **tono puro de 220 Hz** | Viento, hojas y aire bajo; caverna con aire y agua lejana; viento de altura |
| UI_Select, MI/MS ui_foco, ui_confirmar, ui_abrir, ui_error, no_disponible | Pitidos senoidales (880–1200 Hz) | Madera hueca, barro, tela; bloqueo = doble golpe sordo |
| MI/MS hallazgo_abrir, hallazgo_confirmar, ofrenda, descanso, nucleo_abre, cierre_insertar, alas_aviso | Campanas senoidales | Barro, oro, piedra y soplo; aviso de alas = doble golpe de madera |
| MI/MS victoria | Fanfarria de acorde mayor | Frase de **liberación** (flauta, sonajero, tambor que se apaga): el guion pide honrar, no triunfar |
| MI/MS derrota, dano | Drone de órgano; golpe que **terminaba cortado** (−37 dB) | Tambor grave y flauta que cae; golpe al cuerpo con tela |
| MI ambiente_caverna | 71 % de energía bajo 200 Hz (zumbido en quinta) | Corriente de aire, sub suave y agua lejana |

### 3.4 Añadidos (`Assets/_Game/Resources/GameAudio`, 172 clips)

- **Interfaz (UI, 12)**: foco ×3, confirmar, atrás, abrir, cerrar, bloqueado, ajuste, pestaña, objetivo, diálogo.
- **Movimiento (Foley, 45)**: pasos por superficie — piedra ×6, piedra de cueva húmeda ×6, tierra ×6,
  madera ×4, agua ×4 —, aterrizajes por superficie y fuerte, salto ×3, impulso ×3; tomar, girar ×3 y
  soltar objetos; abrir y cerrar inspección.
- **Combate (35)**: bastón ×3; impactos según material — piedra, criatura, escamas, plumas, espíritu
  (oro/plata), guardián de práctica —; bloqueo ×2, esquiva ×2, cubrirse, parada, daño recibido ×3,
  contraataque, interrumpir, anclar, vincular, cambio de turno, lazo roto; afinidad del jaguar (pulso,
  embestida).
- **Criaturas (42)**, cada una con identidad propia:
  - E07 caimán-murciélago: respiración de presencia, bramido al despertar, siseo-gruñido de aviso,
    ataque (mandíbula y ala de cuero), heridas, liberación; chillido de murciélago para el barrido.
  - E08 serpiente bicéfala: escamas sobre piedra (presencia), siseo grave = cabeza A, agudo = cabeza B,
    ataque, heridas, liberación.
  - E09 mujer-cóndor: resoplido siseante (los cóndores no cantan) y alas pesadas.
  - E10 mujer-águila: grito descendente agudo y alas rápidas, picado.
  - E11 Quimue: zumbido de dos tonos, aviso lunar (plata) y solar (oro), ataque, heridas, rotura de resonancia.
  - Jaguar (llamada ronca "serrucho"), guacamaya (graznido y alas), guardián de práctica (crujido de aviso).
- **Ambiente (28)**: capas de plaza, caverna, alturas, aldea de mañana, aldea en llamas, colina, refugio
  (interior), laguna; fuente, hoguera y corriente como fuentes espaciales; aves, hojas, piedra que se
  asienta, chasquidos de fuego e insectos dispersos.
- **Música (10)**: plaza (baja), duelo final con Quimue, prólogo de Bacatá (motivo del bastón), epílogo
  (tensión), legado; frases de descubrimiento, objetivo, liberación, derrota y portal.

Las criaturas y la música son **diseños estilizados**, no grabaciones de las especies ni música tradicional.

## 4. Integración (qué suena y cuándo)

- **Mezclador común** `GameAudio` + `GameAudioHost` (`Scripts/Controller/Audio`): pools de voces 2D y 3D,
  variaciones sin repetir la anterior (±3 % de tono, ±1,5 dB), límite por sonido (intervalo mínimo y
  copias simultáneas), prioridades (avisos y daño primero). `MIAudio`, `MSAudio` y `PlazaAudio` son
  fachadas: mantienen su API y referencias.
- **Pasos** `PlayerFootsteps` (se añade solo al jugador en cada escena): el paso suena cuando el pie del
  rig humanoide se apoya (sincronizado con la animación de andar/correr), con respaldo por distancia; la
  superficie sale del collider (`AudioSurface` o nombre/material) o del valor de la escena (plaza y Mundo
  Superior: piedra; Mundo Inferior: piedra de cueva). Salto, aterrizaje según la caída e impulso.
  Escalada y vuelo conservan sus sonidos propios.
- **Duelos por turnos** `DuelAudio` (un solo lugar): cambio de turno, aviso del enemigo según origen
  (cabeza A/B, luna/sol), defensa elegida, golpe del enemigo, daño, golpe del bastón sincronizado con el
  impacto (0,38 s) y sobre el material del enemigo, herida del enemigo, cuerno, vincular, jaguar,
  liberación. Los escenarios solo añaden lo suyo (piedras que caen, onda en el suelo, núcleo de cristal);
  se quitaron los sonidos duplicados de cada escenario.
- **Entrenamiento de la plaza**: el impacto ahora llega en el pico de la estocada (0,16 s), la ventana
  "¡AHORA!" tiene su propio aviso y el error usa el golpe al cuerpo.
- **Interacción**: tomar/girar/soltar/cerrar inspecciones (plaza, Mundo Inferior y Superior), urnas,
  palancas, rejas, cerraduras; portales de la plaza con zumbido espacial cuando están abiertos y
  "pulso grave" en el momento en que se abren; portal cerrado = sonido de bloqueo.
- **Interfaz**: foco, confirmar, ajustes de deslizador, pestañas, abrir/cerrar menús (plaza, título, pausas
  de los mundos), objetivo nuevo, avance de diálogo.
- **Escenas**: la plaza tiene capa de viento y hojas, música baja, fuente espacial y aves alrededor; la
  caverna, aire y agua, piedras y murciélagos lejanos; las alturas conservan su viento y música por capas
  con un ave lejana; Bacatá cambia de capa en cada lugar (aldea, colina con aves al amanecer e insectos de
  noche, refugio con hoguera, laguna) y suena la flecha, la caída y el agua que se abre.

## 5. Mezcla

- **Canales y ajustes**: volumen general, **música** (nuevo, también en el título), efectos, ambiente,
  **voces y criaturas** (nuevo) e interfaz. Los ajustes antiguos se migran (`menuMusic` → `music`).
- **Audio espacial** (3D, sin Doppler) para fuentes del escenario: criaturas, fuente, hoguera, portales,
  rejas, piedras. **Interfaz y cuerpo del jugador sin posicionamiento** (2D).
- **Transiciones**: música y capas ambientales con fundido cruzado de potencia constante; al cambiar de
  escena, los sonidos del mundo anterior se detienen y la música/capa que nadie vuelve a pedir se apaga en
  1,5 s; la pantalla de carga baja todo; los bucles largos empiezan en un punto aleatorio.
- **Pausa**: los efectos y voces se congelan y se reanudan donde estaban, nada nuevo empieza detrás del
  menú; música y ambiente bajan y se oscurecen (filtro paso bajo); la interfaz sigue sonando.
- **Prioridades / ducking**: durante diálogos la música baja 6 dB; durante los duelos con voz activada la
  música baja 9 dB, el ambiente 6 dB y los efectos 2 dB para que el micrófono oiga al jugador, no al juego;
  las frases musicales (descubrimiento, liberación) bajan la música mientras suenan.
- **Voz y micrófono**: la música no lleva voces ni cantos (nada que el reconocedor pueda confundir con
  una palabra), y se baja en las ventanas de escucha. La lista de palabras (VoiceVocabulary) no se tocó.
- **Indicadores visuales**: los sonidos esenciales también se escriben como subtítulo de sonido cuando
  "Subtítulos de sonidos" está activo: "Cuerno responde", "Siseo grave · cabeza A", "Pulso lunar",
  "Pulso grave · umbral activo", "Rotura de resonancia", etc. Los avisos de duelo ya tienen su marca en el HUD.
- **Importación**: efectos cortos descomprimidos y precargados (sin retraso en el primer uso); música y
  capas largas en streaming Vorbis. Se quitó la normalización de Unity para respetar los niveles del banco.

Niveles objetivo (ajustes por defecto, medidos en partida): capas ambientales ≈ −30 dBFS RMS, música
≈ −26 dB, efectos con picos entre −6 y −15 dBFS, nunca saturación.

## 6. Procedencia y licencia

Todo el audio nuevo y reemplazado es **síntesis procedimental original** generada por
`tools/audio/generate_game_audio.py` (con `dsp.py` y `sources.py`, solo numpy): sin muestras, sin
grabaciones de terceros, sin descargas. Pertenece al proyecto como el resto del código. Los bancos
anteriores (MI, MS, plaza, menú) también eran síntesis original del proyecto.

Regenerar: `python tools/audio/generate_game_audio.py` (todo) o por grupos
(`ui foley combat creatures ambience music replace repair`), luego `python tools/audio/write_audio_metas.py`.

## 7. Verificación

- EditMode: `AudioMixTests` (canales, ducking, pausa, límites, variaciones).
- PlayMode: `GameAudioTests` (banco y variantes, límite de copias, pausa, crossfade y ducking, cambio de escena).
- EditMode 90/90 y PlayMode 19/19 (incluidas las pruebas previas del proyecto) en una copia de trabajo.
- Recorrido grabado (copia de trabajo, no incluido en el repo), salida real del `AudioListener`:

| Tramo | Sonoridad aprox. | Pico | Saturación |
|---|---|---|---|
| Menú de título | −27 LUFS | −14 dBFS | 0 |
| Plaza explorando / en pausa | −30 / −33 LUFS | −16 dBFS | 0 |
| Mundo Inferior explorando | −34 LUFS (más oscuro a propósito) | −17 dBFS | 0 |
| Duelo caimán-murciélago | −27 LUFS | −7 dBFS | 0 |
| Mundo Superior / cóndor / águila | −29 / −27 / −26 LUFS | −7 dBFS | 0 |
| Duelo final con Quimue | −24 LUFS | −7 dBFS | 0 |
| Prólogo / epílogo de Bacatá | −29 / −28 LUFS | −9 dBFS | 0 |

  Sin sonidos duplicados (<60 ms), ningún efecto ni voz iniciado durante la pausa, 0 errores en consola;
  pasos cada ~0,4–0,5 s siguiendo el pie del rig, con superficie correcta en cada mundo. Nota: en modo
  batch el teclado simulado no llega al juego, así que el recorrido mueve el cuerpo del jugador directamente.

## 8. Pendientes

- **Escucha humana**: todo se validó por medición; conviene una escucha con auriculares para ajustar gusto.
- **Micrófono real**: el ducking protege el reconocimiento en teoría; no se probó con micrófono y altavoces.
- **Grabaciones de campo**: para más realismo (aves andinas, agua, fuego, pasos) se pueden sustituir clips
  por grabaciones CC0/CC-BY (p. ej. Freesound) manteniendo nombres; no se descargó nada.
- **Enemigos comunes del Mundo Inferior** (caimán y murciélago, E03-E06): otra sesión los está
  implementando; cuando lleguen, enganchar `Criaturas/caiman_*` y `murcielago_chillido` a sus eventos.
- **Transformación del jugador** (C08 completa) y **voces de personajes**: no existen en el juego; el
  canal "voces y criaturas" ya está listo para ellas.
- **Pasos en madera, tierra y agua**: los sonidos existen; los suelos actuales son de piedra, así que solo
  sonarán cuando un suelo lo indique (nombre o `AudioSurface`).
