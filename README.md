# Metroidvania Thevenance

Prototipo de metroidvania en 3D. Este documento explica como instalar y correr el proyecto en tu maquina.

## Requisitos

- Unity 6000.3.21f1 (instalar esta version exacta desde Unity Hub para evitar problemas de compatibilidad).
- Git y Git LFS.
- Camara web (necesaria solo para la funcionalidad de tracking de manos, ver seccion correspondiente).

## Clonar el repositorio

```
git clone <url-del-repositorio>
```

Abrir la carpeta del proyecto desde Unity Hub, seleccionando la version 6000.3.21f1.

## Instalar el plugin de MediaPipe

El proyecto usa `MediaPipeUnityPlugin` 0.16.3 (de homuler) para el tracking de manos. El tarball oficial esta incluido en `LocalPackages` y `Packages/manifest.json` lo referencia mediante una ruta relativa, por lo que no hay que descargarlo ni seleccionarlo manualmente en Package Manager.

Como el tarball es un binario grande, esta versionado con Git LFS. Antes de abrir el proyecto por primera vez, ejecutar:

```
git lfs install
git lfs pull
```

Los samples `Official Solutions` necesarios para el tracking tambien estan incluidos en el repositorio. Unity debe resolver el paquete automaticamente al abrir el proyecto.

## Escenas del proyecto

- `Assets/Prototype/Scenes/Movement.unity`: escena principal, es la que hay que abrir para jugar.
- `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark Detection/Hand Landmark Detection.unity`: se carga sola de forma aditiva al iniciar la escena principal (ver `HandTrackingBootstrapper`), no hace falta abrirla manualmente.

Ambas escenas ya estan agregadas en `Build Settings`, es un requisito de Unity para poder cargarlas por nombre en tiempo de ejecucion.

## Controles

- `A` / `D`: moverse en el eje X.
- `W` / `S`: moverse en el eje Z.
- `Espacio`: saltar.
- `Shift izquierdo`: dash (requiere haber recolectado al menos un power up de dash).
- `E`: interactuar con un objeto recolectable (inspeccionarlo, y de nuevo para confirmarlo).
- `Escape`: cancelar la inspeccion sin recolectar el objeto.
- Durante la inspeccion, la rotacion del objeto se controla con el mouse, o con las manos si la camara detecta ambas: la mano derecha abierta controla la rotacion vertical, la mano izquierda abierta controla la rotacion horizontal. Cerrar el puno congela esa rotacion en su lugar.

## Sobre el tracking de manos

Es una funcionalidad opcional durante la ejecucion: si no hay camara disponible o el tracking no puede inicializarse, la inspeccion de objetos sigue funcionando con mouse. El paquete de MediaPipe si es necesario para compilar el proyecto, pero no es necesario tener una camara conectada para jugar.

## Funciona en Mac

Si. El paquete de MediaPipe que se descarga en el paso de instalacion ya incluye los binarios nativos para Windows, macOS (Intel y Apple Silicon) y Linux dentro del mismo archivo, Unity elige automaticamente el correcto segun el sistema operativo. En macOS y Windows el plugin corre en modo CPU (el modo GPU no esta soportado en esos sistemas, no es necesario cambiar nada, ya viene configurado asi por defecto).

Una advertencia especifica de macOS: la primera vez que se abra el proyecto, Gatekeeper puede bloquear la libreria nativa (`libmediapipe_c.dylib`) por no estar firmada. Si esto pasa, hay que ir a `Configuracion del Sistema > Privacidad y Seguridad` y autorizarla desde ahi (o click derecho sobre el archivo dentro de la carpeta del paquete y elegir `Abrir`).

## Problemas conocidos

- El objeto `Face` del personaje (usado como referencia de camara durante la inspeccion) todavia tiene una esfera visible temporal, se va a reemplazar por un objeto vacio cuando este el modelo final del personaje.
- El sistema de dano y de romper objetos con el dash todavia no esta implementado, es la siguiente etapa del prototipo.
- La sensibilidad de rotacion por manos y la deteccion de mano abierta/cerrada son valores iniciales sin ajustar del todo, pueden necesitar calibracion segun la camara de cada uno.
