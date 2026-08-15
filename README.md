# Metroidvania Thevenance

Prototipo de metroidvania en 3D. Este documento explica como instalar y correr el proyecto en tu maquina.

## Requisitos

- Unity 6000.3.21f1 (instalar esta version exacta desde Unity Hub para evitar problemas de compatibilidad).
- Git.
- Camara web (necesaria solo para la funcionalidad de tracking de manos, ver seccion correspondiente).

## Clonar el repositorio

```
git clone <url-del-repositorio>
```

Abrir la carpeta del proyecto desde Unity Hub, seleccionando la version 6000.3.21f1.

## Instalar el plugin de MediaPipe

El proyecto usa `MediaPipeUnityPlugin` (de homuler) para el tracking de manos. Este paquete no se instala solo al clonar el repositorio porque se agrego desde un archivo local (tarball), y esa ruta es especifica de la maquina donde se instalo originalmente. Cada persona del equipo tiene que instalarlo una vez en su propia maquina siguiendo estos pasos:

1. Descargar el archivo `com.github.homuler.mediapipe-0.16.3.tgz` desde la pagina de releases del proyecto:
   `https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3`
2. En Unity: `Window > Package Manager` -> boton `+` (arriba a la izquierda) -> `Add package from tarball...` -> seleccionar el archivo descargado.
3. Unity va a mostrar una advertencia indicando que no puede verificar la firma del paquete. Esto es normal para paquetes que no vienen del Unity Registry, no bloquea nada, se puede continuar sin problema.
4. En Package Manager, con el paquete seleccionado, abrir la pestana `Samples` e importar `Official Solutions`.

Despues de este paso, `Packages/manifest.json` va a mostrar un cambio local en tu maquina (la ruta al tarball va a apuntar a donde vos lo descargaste). Eso es esperado, no hace falta commitear ese cambio ni preocuparse por el.

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

Es una funcionalidad opcional: si no hay camara disponible o el plugin no esta instalado, la inspeccion de objetos sigue funcionando igual con mouse. No es necesario tener la camara conectada para poder jugar.

## Funciona en Mac

Si. El paquete de MediaPipe que se descarga en el paso de instalacion ya incluye los binarios nativos para Windows, macOS (Intel y Apple Silicon) y Linux dentro del mismo archivo, Unity elige automaticamente el correcto segun el sistema operativo. En macOS y Windows el plugin corre en modo CPU (el modo GPU no esta soportado en esos sistemas, no es necesario cambiar nada, ya viene configurado asi por defecto).

Una advertencia especifica de macOS: la primera vez que se abra el proyecto, Gatekeeper puede bloquear la libreria nativa (`libmediapipe_c.dylib`) por no estar firmada. Si esto pasa, hay que ir a `Configuracion del Sistema > Privacidad y Seguridad` y autorizarla desde ahi (o click derecho sobre el archivo dentro de la carpeta del paquete y elegir `Abrir`).

## Problemas conocidos

- El objeto `Face` del personaje (usado como referencia de camara durante la inspeccion) todavia tiene una esfera visible temporal, se va a reemplazar por un objeto vacio cuando este el modelo final del personaje.
- El sistema de dano y de romper objetos con el dash todavia no esta implementado, es la siguiente etapa del prototipo.
- La sensibilidad de rotacion por manos y la deteccion de mano abierta/cerrada son valores iniciales sin ajustar del todo, pueden necesitar calibracion segun la camara de cada uno.
