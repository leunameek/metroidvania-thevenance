# Metroidvania Thevenance

## El asedio de Bacatá — Plaza Núñez / lobby y tutorial

Escena de inicio: `Assets/_Game/Scenes/MainMenu.unity`.
El menú está aislado de la plaza y ofrece Nueva partida, Configuración y Salir.
El título de montañas y sol se centra sobre el poblado y la laguna al amanecer, con el personaje
a la izquierda y tres botones de piedra debajo. La bruma y el agua se animan suavemente.
Movimiento reducido detiene el efecto; el título y los botones permanecen fijos.
Nueva partida carga directamente `Assets/_Game/Scenes/PlazaNunez.unity`.
La plaza incluye arquitectura de piedra, jardines, fuente, tres estaciones de objetos con
MediaPipe, duelo de entrenamiento por turnos y portales al mundo inferior y superior.
Cada portal lleva a un umbral explorable con retorno; los mundos completos quedan para su
desarrollo posterior. `Movement` conserva el prototipo anterior.

Abre la escena Nemequene_MainMenu y pulsa Play para probar el inicio completo. Configuración
abre primero Accesibilidad. Flechas o Tab eligen, Enter confirma y Esc regresa. M silencia
la música del menú. Cámara y micrófono son optativos y se calibran desde la pausa del juego.
No aparece Continuar porque todavía no existe guardado. Al abrir la escena de la plaza
directamente en el editor se entra al tutorial, sin repetir el menú.

Abre la escena y pulsa Play. WASD mueve, Shift corre, Espacio salta, Q hace dash y
botón derecho + mouse gira la cámara. E interactúa. C activa/pausa la cámara; M selecciona
la alternativa con mouse. Esc abre la pausa, Tab el diario y H los controles; V silencia;
R solicita confirmación antes de reiniciar la visita completa. Los ajustes de volumen,
texto, contraste, gráficos y dispositivos están en los menús de configuración.
El combate muestra sus controles propios: E atacar, Espacio esquivar y F bloquear.

Todas esas teclas son las predeterminadas: en Configuración > Controles cada acción (moverse,
saltar, correr, dash, agarrar/interactuar, alas, combate, sistema) admite dos teclas y el cambio vale
en la plaza y en los dos mundos (`GameBindings`). Accesibilidad incluye la elección de cámara y
micrófono con vista previa y medidor. Gráficos aplica de verdad modo de pantalla, resolución (prueba
de 15 s), tasa de refresco, escala de render con FSR 1.0 o STP, V-Sync, límite de FPS, preajustes,
sombras, SSAO, reflejos, texturas, anisotrópico, distancia, antialiasing, efectos, latencia, HDR,
brillo y gamma en todas las escenas (`GraphicsRuntime`). Los créditos se editan en
`Assets/_Game/UI/Resources/Nemequene/credits.json` (los nombres «Por confirmar» están pendientes).

La interfaz muestra las ayudas solo cuando hacen falta: interacción por proximidad, vida durante
el combate o con daño y objetivos breves al cambiar. La pausa reúne las consultas en Diario
y los ajustes en Configuración, con la misma estética del inicio.

- [Entrega de interfaz: integración, evidencias y pendientes](Assets/_Game/UI/Documentation/ENTREGA.md)
- [Sistema visual y componentes](DESIGN.md)
- [Wireframes completos y recorrido en Figma](Specs/UI/Figma/README.md)
- [Arquitectura, límites entre módulos y validación de build](Specs/Architecture-Review.md)
- [Menú independiente, música y validación](Assets/_Game/UI/Documentation/04-Menu-inicio.md)
- Ejecutable local, una vez compilado: `Builds/Nemequene/Nemequene.exe`. Conserva la carpeta completa junto al ejecutable.

- [Diseño, recorrido, audio y alcance de la plaza](Specs/Week08/05-Plaza-Lobby-Tutorial.md)

- [Diagnóstico y tareas](Specs/Week08/01-Diagnostico-y-plan.md)
- [Diseño inicial y pendientes de investigación](Specs/Week08/02-Diseno-inicial.md)
- [Controles e instrucciones](Specs/Week08/03-Ejecucion.md)
- [Validación del primer módulo](Specs/Week08/04-Validacion.md)

Prototipo de metroidvania en 3D. Este documento explica como instalar y correr el proyecto en tu maquina.

## Requisitos

- Unity 6000.3.21f1 (instalar esta version exacta desde Unity Hub para evitar problemas de compatibilidad).
- Git.
- Git LFS, necesario para modelos, texturas, audio y el paquete local de MediaPipe.
- Camara web (necesaria solo para la funcionalidad de tracking de manos, ver seccion correspondiente).

## Clonar el repositorio

```
git lfs install
git clone https://github.com/leunameek/metroidvania-thevenance.git
cd metroidvania-thevenance
git lfs pull
```

Abrir la carpeta del proyecto desde Unity Hub, seleccionando la version 6000.3.21f1.

## Organización y preparación de commits

- `Assets/_Game/`: escenas, arte, audio y código del juego, siempre junto a sus `.meta`
  (detalle en [Estructura de `Assets/`](#estructura-de-assets)).
- `Packages/` y `ProjectSettings/`: dependencias y configuración compartida de Unity.
- `LocalPackages/`: paquete de MediaPipe con ruta relativa, distribuido mediante Git LFS.
- `ArtSource/`: fuentes de arte locales, excluidas de esta entrega ligera; los recursos
  que usa el juego se conservan en `Assets/`.
- `Specs/`, `DESIGN.md` y `PRODUCT.md`: diseño, documentación y evidencias.
- `tools/`: utilidades del proyecto; `.vsconfig` declara las herramientas de Visual Studio.

Las cachés de Unity, builds, logs, proyectos generados por el IDE, entornos locales y
copias de seguridad de Blender quedan excluidos por `.gitignore`. Los ZIP de Nemequene
y del atlas de interfaz también se excluyen porque sus contenidos ya están extraídos;
los originales locales se conservan. Los recursos binarios se gestionan con Git LFS,
incluidas las texturas con extensión en mayúsculas y los modelos `.glb`.

Antes de cada commit, revisar lo que se va a incluir:

```sh
git add --all
git diff --cached --stat
git diff --cached --check
git lfs status
git lfs fsck
```

Después de revisar los cambios, crear el commit y subir la rama actual:

```sh
git commit -m "feat: integrate Bacata menu, plaza tutorial and game assets"
git push origin HEAD
```

El hook de Git LFS sube los binarios durante el push. El conjunto de recursos ocupa
varios GB: comprobar el almacenamiento y la transferencia disponibles para LFS en
la cuenta del repositorio antes de la primera subida. Evitar `git add --force` sobre
cachés o builds y no mover recursos dentro de `Assets/` sin conservar sus `.meta`.

## Estructura de `Assets/`

Todo lo propio del juego está en `Assets/_Game`; el resto de carpetas de `Assets/` son de
Unity o de terceros (`Samples` de MediaPipe, `TextMesh Pro`, `Settings` de URP,
`StreamingAssets` con el modelo de manos e `InputSystem_Actions`).

```
Assets/_Game/
  Scenes/                 MainMenu, PlazaNunez, MundoInferior, MundoSuperior (en Build Settings)
    Dev/                  Movement (prototipo anterior) y PlazaNunez_Graybox (respaldo)
  Art/
    Characters/
      Nemequene/          modelo, material, prefab visual, Animator y Animations/ (Mixamo)
      Legacy/             versión anterior de Nemequene (ThevenanceHero), sin uso en escenas
    Equipment/Alas/       alas con rig del mundo superior
    Environments/
      PlazaNunez/         Models/ (props Tripo, guardián de entrenamiento), Materials/, Meshes/
      MundoInferior/      Models/ (kit Tripo) y Materials/ (Kit, Textures)
      MundoSuperior/      Models/ (piezas T/A/O/E por ID) y Materials/
    Shaders/              AndeanSky, PortalVeil, MSCloud
  Audio/PlazaNunez/       efectos y ambientes del hub
  Resources/              cargado en tiempo de ejecución: MIAudio/, MSAudio/, MIParticulas
  UI/                     sistema de interfaz Nemequene (Scripts, Editor, Prefabs, Fonts,
                          Resources/Nemequene, Tests, Documentation)
  Scripts/                código del juego: Model/, Controller/, View/ y Tests/ (asmdefs)
  Editor/                 constructores y ajustes de importación por zona
                          (PlazaNunez, MundoInferior, MundoSuperior, Nemequene)
  Data/PlazaNunez/        ScriptableObjects del hub
  Prefabs/                enemigos, pickups y bootstrap de manos
  Materials/              materiales del prototipo Movement
```

Reglas: mover o renombrar recursos siempre desde Unity (o junto con su `.meta`), y si un
script del editor usa una ruta `Assets/_Game/...`, actualizarla en el mismo cambio.

## Instalar el plugin de MediaPipe

El proyecto usa `MediaPipeUnityPlugin` (de homuler) 0.16.3 para el tracking de manos.
El paquete se recuperó en `LocalPackages` y el manifest usa una ruta relativa. Ejecutar
`git lfs install` y `git lfs pull` después de clonar; ver [procedencia y hash](LocalPackages/README.md).
Si el tarball no está disponible, la fuente oficial para recuperarlo es:

1. Descargar el archivo `com.github.homuler.mediapipe-0.16.3.tgz` desde la pagina de releases del proyecto:
   `https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3`
2. Guardar el archivo en `LocalPackages/com.github.homuler.mediapipe-0.16.3.tgz`;
   mantener la referencia relativa del manifest.
3. Unity va a mostrar una advertencia indicando que no puede verificar la firma del paquete. Esto es normal para paquetes que no vienen del Unity Registry, no bloquea nada, se puede continuar sin problema.
4. Las muestras `Official Solutions` ya están versionadas en `Assets/Samples`; no es necesario volver a importarlas para abrir el proyecto.

No sustituir la ruta por Downloads de un usuario: eso impide resolver el paquete en otros equipos.

## Escenas del proyecto

- `Assets/_Game/Scenes/MainMenu.unity`: menú principal; primera escena en Build Settings.
- `Assets/_Game/Scenes/PlazaNunez.unity`: lobby/tutorial; se carga desde Nueva partida.
- `Assets/_Game/Scenes/MundoInferior.unity`: mundo inferior; se entra por el portal de la plaza.
- `Assets/_Game/Scenes/MundoSuperior.unity`: mundo superior; se entra por el portal de la plaza.
- `Assets/_Game/Scenes/Dev/Movement.unity`: prototipo anterior de movimiento, pickups y jefe.
- `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/Hand Landmark Detection.unity`: la plaza y los mundos la cargan al pulsar C; el prototipo Movement la carga al iniciar. No hace falta abrirla manualmente.

Estas escenas ya están agregadas en `Build Settings`, un requisito de Unity para poder cargarlas en tiempo de ejecución.

## Controles del prototipo anterior Movement

- `A` / `D`: moverse en el eje X.
- `W` / `S`: moverse en el eje Z.
- `Espacio`: saltar.
- `Shift izquierdo`: dash (requiere haber recolectado al menos un power up de dash).
- `E`: interactuar con un objeto recolectable (inspeccionarlo, y de nuevo para confirmarlo).
- `Escape`: cancelar la inspeccion sin recolectar el objeto.
- Durante la inspeccion, la rotacion del objeto se controla con el mouse, o con las manos si la camara detecta ambas: la mano derecha abierta controla la rotacion vertical, la mano izquierda abierta controla la rotacion horizontal. Cerrar el puno congela esa rotacion en su lugar.

## Manos y voz en la plaza y los mundos

Las manos (MediaPipe) y la voz (reconocimiento de Windows) hacen lo mismo que las teclas; el teclado sigue funcionando siempre. La camara se activa con `C` en la plaza y en los dos mundos, y queda activa al viajar entre escenas. La voz se activa en Configuracion (o en la pausa de cada mundo); en la plaza se habla manteniendo `Ctrl izquierdo` (pulsar para hablar), en los mundos escucha de forma continua.

| Accion | Teclado | Manos | Voz |
| --- | --- | --- | --- |
| Interactuar (estacion, hallazgo, palanca, portal, guardian) | `E` | palma abierta quieta ~1 s (barra dorada) | «examinar», «usar», «activar», «entrar», «enfrentar» |
| Girar la pieza inspeccionada | mouse / `A` `D` `W` `S` | mover la mano abierta | — |
| Tomar el hallazgo / volver con la leccion aprendida | `E` | puño sostenido ~0,7 s | «tomar», «recoger», «confirmar» |
| Cerrar la inspeccion sin tomar | `Esc` | — | «salir», «volver», «cerrar» |
| Atacar en tu turno (plaza, guardian de la cima) | `E` | cerrar la mano en puño | «atacar», «ataca», «golpe» |
| Bloquear | `F` | dos palmas abiertas arriba | «bloquear», «bloquea», «escudo» |
| Esquivar | `Espacio` | barrido lateral rapido de la mano abierta | «esquiva», «esquivar» |
| Impulso contra el centinela y el guardian del fondo | `Q` | puño (solo cerca del combate) | «impulso» (en cualquier lugar) |

Cada gesto solo cuenta en su turno: lo que se haga antes del aviso se descarta. El panel de la esquina inferior derecha de los mundos indica el estado de camara y microfono y el gesto que aplica en ese momento, y una cinta muestra lo que acaba de reconocerse («Puño · Impulso»). El reconocimiento de voz usa el idioma de voz de Windows: con Windows en español funciona mejor.

## Sobre el tracking de manos

Es una funcionalidad opcional: si no hay camara disponible o el plugin no esta instalado, la inspeccion de objetos sigue funcionando igual con mouse. No es necesario tener la camara conectada para poder jugar.

## Funciona en Mac

Si. El paquete de MediaPipe que se descarga en el paso de instalacion ya incluye los binarios nativos para Windows, macOS (Intel y Apple Silicon) y Linux dentro del mismo archivo, Unity elige automaticamente el correcto segun el sistema operativo. En macOS y Windows el plugin corre en modo CPU (el modo GPU no esta soportado en esos sistemas, no es necesario cambiar nada, ya viene configurado asi por defecto).

Una advertencia especifica de macOS: la primera vez que se abra el proyecto, Gatekeeper puede bloquear la libreria nativa (`libmediapipe_c.dylib`) por no estar firmada. Si esto pasa, hay que ir a `Configuracion del Sistema > Privacidad y Seguridad` y autorizarla desde ahi (o click derecho sobre el archivo dentro de la carpeta del paquete y elegir `Abrir`).

## Problemas conocidos

- El objeto `Face` del personaje (usado como referencia de camara durante la inspeccion) todavia tiene una esfera visible temporal, se va a reemplazar por un objeto vacio cuando este el modelo final del personaje.
- La sensibilidad de rotacion por manos y la deteccion de mano abierta/cerrada son valores iniciales sin ajustar del todo, pueden necesitar calibracion segun la camara de cada uno.
