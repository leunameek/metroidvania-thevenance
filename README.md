# Metroidvania Thevenance

## El asedio de BacatÃ¡ â€” Plaza NÃºÃ±ez / lobby y tutorial

Escena de inicio: `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity`.
El menÃº estÃ¡ aislado de la plaza y ofrece Nueva partida, ConfiguraciÃ³n y Salir.
El tÃ­tulo de montaÃ±as y sol se centra sobre el poblado y la laguna al amanecer, con el personaje
a la izquierda y tres botones de piedra debajo. La bruma y el agua se animan suavemente.
Movimiento reducido detiene el efecto; el tÃ­tulo y los botones permanecen fijos.
Nueva partida carga directamente `Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity`.
La plaza incluye arquitectura de piedra, jardines, fuente, tres estaciones de objetos con
MediaPipe, duelo de entrenamiento por turnos y portales al mundo inferior y superior.
Cada portal lleva a un umbral explorable con retorno; los mundos completos quedan para su
desarrollo posterior. `Movement` conserva el prototipo anterior.

Abre la escena Nemequene_MainMenu y pulsa Play para probar el inicio completo. ConfiguraciÃ³n
abre primero Accesibilidad. Flechas o Tab eligen, Enter confirma y Esc regresa. M silencia
la mÃºsica del menÃº. CÃ¡mara y micrÃ³fono son optativos y se calibran desde la pausa del juego.
No aparece Continuar porque todavÃ­a no existe guardado. Al abrir la escena de la plaza
directamente en el editor se entra al tutorial, sin repetir el menÃº.

Abre la escena y pulsa Play. WASD mueve, Shift corre, Espacio salta, Q hace dash y
botÃ³n derecho + mouse gira la cÃ¡mara. E interactÃºa. C activa/pausa la cÃ¡mara; M selecciona
la alternativa con mouse. Esc abre la pausa, Tab el diario y H los controles; V silencia;
R solicita confirmaciÃ³n antes de reiniciar la visita completa. Los ajustes de volumen,
texto, contraste, grÃ¡ficos y dispositivos estÃ¡n en los menÃºs de configuraciÃ³n.
El combate muestra sus controles propios: E atacar, Espacio esquivar y F bloquear.

La interfaz muestra las ayudas solo cuando hacen falta: interacciÃ³n por proximidad, vida durante
el combate o con daÃ±o y objetivos breves al cambiar. La pausa reÃºne las consultas en Diario
y los ajustes en ConfiguraciÃ³n, con la misma estÃ©tica del inicio.

- [Entrega de interfaz: integraciÃ³n, evidencias y pendientes](Assets/Nemequene/UI/Documentation/ENTREGA.md)
- [Sistema visual y componentes](DESIGN.md)
- [Wireframes completos y recorrido en Figma](Specs/UI/Figma/README.md)
- [Arquitectura, lÃ­mites entre mÃ³dulos y validaciÃ³n de build](Specs/Architecture-Review.md)
- [MenÃº independiente, mÃºsica y validaciÃ³n](Assets/Nemequene/UI/Documentation/04-Menu-inicio.md)
- Ejecutable local, una vez compilado: `Builds/Nemequene/Nemequene.exe`. Conserva la carpeta completa junto al ejecutable.

- [DiseÃ±o, recorrido, audio y alcance de la plaza](Specs/Week08/05-Plaza-Lobby-Tutorial.md)

- [DiagnÃ³stico y tareas](Specs/Week08/01-Diagnostico-y-plan.md)
- [DiseÃ±o inicial y pendientes de investigaciÃ³n](Specs/Week08/02-Diseno-inicial.md)
- [Controles e instrucciones](Specs/Week08/03-Ejecucion.md)
- [ValidaciÃ³n del primer mÃ³dulo](Specs/Week08/04-Validacion.md)

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

## OrganizaciÃ³n y preparaciÃ³n de commits

- `Assets/`: escenas, scripts y recursos del juego, siempre junto a sus archivos `.meta`.
- `Packages/` y `ProjectSettings/`: dependencias y configuraciÃ³n compartida de Unity.
- `LocalPackages/`: paquete de MediaPipe con ruta relativa, distribuido mediante Git LFS.
- `ArtSource/`: fuentes de arte locales, excluidas de esta entrega ligera; los recursos
  que usa el juego se conservan en `Assets/`.
- `Specs/`, `DESIGN.md` y `PRODUCT.md`: diseÃ±o, documentaciÃ³n y evidencias.
- `tools/`: utilidades del proyecto; `.vsconfig` declara las herramientas de Visual Studio.

Las cachÃ©s de Unity, builds, logs, proyectos generados por el IDE, entornos locales y
copias de seguridad de Blender quedan excluidos por `.gitignore`. Los ZIP de Nemequene
y del atlas de interfaz tambiÃ©n se excluyen porque sus contenidos ya estÃ¡n extraÃ­dos;
los originales locales se conservan. Los recursos binarios se gestionan con Git LFS,
incluidas las texturas con extensiÃ³n en mayÃºsculas y los modelos `.glb`.

Antes de cada commit, revisar lo que se va a incluir:

```sh
git add --all
git diff --cached --stat
git diff --cached --check
git lfs status
git lfs fsck
```

DespuÃ©s de revisar los cambios, crear el commit y subir la rama actual:

```sh
git commit -m "feat: integrate Bacata menu, plaza tutorial and game assets"
git push origin HEAD
```

El hook de Git LFS sube los binarios durante el push. El conjunto de recursos ocupa
varios GB: comprobar el almacenamiento y la transferencia disponibles para LFS en
la cuenta del repositorio antes de la primera subida. Evitar `git add --force` sobre
cachÃ©s o builds y no mover recursos dentro de `Assets/` sin conservar sus `.meta`.

## Instalar el plugin de MediaPipe

El proyecto usa `MediaPipeUnityPlugin` (de homuler) 0.16.3 para el tracking de manos.
El paquete se recuperÃ³ en `LocalPackages` y el manifest usa una ruta relativa. Ejecutar
`git lfs install` y `git lfs pull` despuÃ©s de clonar; ver [procedencia y hash](LocalPackages/README.md).
Si el tarball no estÃ¡ disponible, la fuente oficial para recuperarlo es:

1. Descargar el archivo `com.github.homuler.mediapipe-0.16.3.tgz` desde la pagina de releases del proyecto:
   `https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3`
2. Guardar el archivo en `LocalPackages/com.github.homuler.mediapipe-0.16.3.tgz`;
   mantener la referencia relativa del manifest.
3. Unity va a mostrar una advertencia indicando que no puede verificar la firma del paquete. Esto es normal para paquetes que no vienen del Unity Registry, no bloquea nada, se puede continuar sin problema.
4. Las muestras `Official Solutions` ya están versionadas en `Assets/Samples`; no es necesario volver a importarlas para abrir el proyecto.

No sustituir la ruta por Downloads de un usuario: eso impide resolver el paquete en otros equipos.

## Escenas del proyecto

- `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity`: menÃº principal; primera escena en Build Settings.
- `Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity`: lobby/tutorial; se carga desde Nueva partida.
- `Assets/Prototype/Scenes/Movement.unity`: prototipo anterior de movimiento, pickups y jefe.
- `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/Hand Landmark Detection.unity`: la plaza la carga al pulsar C; el prototipo Movement la carga al iniciar. No hace falta abrirla manualmente.

Estas escenas ya estÃ¡n agregadas en `Build Settings`, un requisito de Unity para poder cargarlas en tiempo de ejecuciÃ³n.

## Controles del prototipo anterior Movement

- `A` / `D`: moverse en el eje X.
- `W` / `S`: moverse en el eje Z.
- `Espacio`: saltar.
- `Shift izquierdo`: dash (requiere haber recolectado al menos un power up de dash).
- `E`: interactuar con un objeto recolectable (inspeccionarlo, y de nuevo para confirmarlo).
- `Escape`: cancelar la inspeccion sin recolectar el objeto.
- Durante la inspeccion, la rotacion del objeto se controla con el mouse, o con las manos si la camara detecta ambas: la mano derecha abierta controla la rotacion vertical, la mano izquierda abierta controla la rotacion horizontal. Cerrar el puno congela esa rotacion en su lugar.

## Sobre el tracking de manos

Es una funcionalidad opcional: si no hay camara disponible o el plugin no esta instalado, la inspeccion de objetos sigue funcionando igual con mouse. No es necesario tener la camara conectada para poder jugar.

## Funciona en Mac

Si. El paquete de MediaPipe que se descarga en el paso de instalacion ya incluye los binarios nativos para Windows, macOS (Intel y Apple Silicon) y Linux dentro del mismo archivo, Unity elige automaticamente el correcto segun el sistema operativo. En macOS y Windows el plugin corre en modo CPU (el modo GPU no esta soportado en esos sistemas, no es necesario cambiar nada, ya viene configurado asi por defecto).

Una advertencia especifica de macOS: la primera vez que se abra el proyecto, Gatekeeper puede bloquear la libreria nativa (`libmediapipe_c.dylib`) por no estar firmada. Si esto pasa, hay que ir a `Configuracion del Sistema > Privacidad y Seguridad` y autorizarla desde ahi (o click derecho sobre el archivo dentro de la carpeta del paquete y elegir `Abrir`).

## Problemas conocidos

- El objeto `Face` del personaje (usado como referencia de camara durante la inspeccion) todavia tiene una esfera visible temporal, se va a reemplazar por un objeto vacio cuando este el modelo final del personaje.
- El sistema de dano y de romper objetos con el dash todavia no esta implementado, es la siguiente etapa del prototipo.
- La sensibilidad de rotacion por manos y la deteccion de mano abierta/cerrada son valores iniciales sin ajustar del todo, pueden necesitar calibracion segun la camara de cada uno.
