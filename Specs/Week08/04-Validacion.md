# Validación del primer módulo — 2026-09-06

## Resultado
- Compilación completa de scripts de la copia de validación: exit code 0.
- Generación de escena por Unity: WEEK08_SCENE_CREATED y exit code 0.
- EditMode: **19/19 pruebas aprobadas**, 0 fallidas, 0 omitidas.
  Incluye 17 pruebas existentes y dos de ExplorationObjectiveModel.
- Play Mode con paso de simulación de 1/60 s: **30 comprobaciones aprobadas** y
  WEEK08_SMOKE_PASS.
- Revisión de referencias de escena, tres datos SO, .meta y diff: completada.

Comprobaciones de juego: sesión/player presentes, tres objetos, separación respecto a jefe/
hardware, suelo, caminar (2.50008 m en aproximadamente 0.5 s), carrera mayor que caminar sin dash,
despegue/aterrizaje, pared, rampa, seguimiento y órbita, proximidad/análisis/registro/bloqueo de
locomoción/salida para cada uno de los tres objetos, objetivo completo, retorno por caída y
reinicio con objetivos en cero.

## Entorno y evidencia
Unity 6000.3.21f1, Windows, ejecución batch en Logs/Week08/ValidationProject.
La copia usa los mismos fuentes/escenas y versiones de paquetes. Para evitar descargas,
las dependencias de registro de esa copia apuntan a los paquetes ya presentes en la caché;
MediaPipe se resuelve desde el nuevo tarball. Es una validación local, no una instalación
desde cero en un equipo externo ni una build ejecutable.
La escena/materiales/datos se generaron con Unity en esa copia y se copiaron al proyecto junto
con sus GUID. No se reemplazaron las escenas anteriores.

Archivos de evidencia locales (ignorados por Git):
- Logs/Week08/scene-generation-final.log
- Logs/Week08/playmode-60fps.log
- Logs/Week08/editmode-results.xml
- Logs/Week08/editmode-tests.log

Resultado XML de EditMode también conservado en Evidence/EditMode-results.xml.

## Incidencias que siguen abiertas
1. El indexador interno UnityEditor.Search.SearchDatabase emitió ArgumentOutOfRangeException
   durante el arranque de la copia. No proviene de los scripts nuevos; las pruebas continuaron.
   No declarar el Editor libre de toda excepción ni eliminar cachés del usuario para ocultarla.
2. Advertencias CS0618 en paquetes y samples ya existentes. No actualizamos esas dependencias.
3. Ensayos iniciales con frames variables dieron menos desplazamiento del esperado
   (aproximadamente 0.46–0.74 m frente a 2.5 m). Se corrigieron la preparación del teclado y
   el ciclo de ejecución del ensayo, y se verificó el caso a 60 Hz. Falta caracterizar
   locomoción/física a baja tasa de cuadros con pruebas controladas; no se certifica independencia
   del framerate.
4. No se verificó visualmente el HUD/graybox en una sesión interactiva ni con todos los tamaños
   de pantalla. Revisar cámara, legibilidad y sensaciones con una persona.
5. Micrófono, cámara web y reconocimiento no se ejecutaron en esta demo. Las pruebas de
   hardware del nuevo alcance permanecen pendientes.
6. No se generó build Windows en este primer módulo. Existe el soporte instalado; se reserva
   la build de aceptación para cerrar los módulos del backlog W08.

## Repetir pruebas
- Modelos: Window > General > Test Runner > EditMode > Run All.
- Smoke: en una copia del proyecto, Unity -batchmode -nographics -projectPath RUTA_COPIA
  -executeMethod Week08SmokeValidation.Run -logFile RUTA_LOG.
  No añadir -quit: el ensayo sale al completar/fallar. Requiere licencia local disponible.
  El runner se rechaza en un Editor interactivo; no está unido a ninguna escena de juego.
  Week08SmokeDriver solo compila en Editor y se elimina del player.
- Prueba manual: 03-Ejecucion.md.

## Alcance realmente entregado
Módulo inicial de exploración/graybox y análisis, con documentación de diagnóstico/diseño.
El placeholder de combate es inerte. Los accesos superior/inferior son reservas señalizadas,
no secciones jugables completas. Combate por turnos, voz configurable y manos aisladas se
implementarán después según 01-Diagnostico-y-plan.md. **Esto no cierra toda la semana 8.**
