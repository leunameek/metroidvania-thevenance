# Validación de la interfaz

Este documento conserva la validación de la primera integración. El menú de inicio fue reemplazado después por una escena independiente; los resultados vigentes para ese cambio están en `04-Menu-inicio.md`: 76 comprobaciones de menú y 61 de regresión de la plaza, con sus evidencias en `Specs/UI/TitleMenu`. Las capturas antiguas de menú y el recorrido de Primera configuración de este registro ya no describen el inicio actual.

## Prueba de Play Mode

Unity **6000.3.21f1**, Windows, URP, GPU NVIDIA RTX 4060. Se ejecutó una copia del proyecto en `Builds/UIValidationProject`, conservando la sesión abierta del proyecto original.

Resultado: **64 comprobaciones aprobadas; cero errores de ejecución del juego**. Evidencia: `Specs/UI/Evidence/validation.txt`.

Se verificó:

- Raíz, TextMeshPro y fuentes cargadas, inicio con pausa y dispositivos apagados.
- Primera configuración mediante teclado; Continuar desactivado sin guardado.
- Apertura, foco y regreso de configuración, accesibilidad, calibración, pausa, mapa, objetivos, poporo, archivo, ayuda, créditos, derrota y cierre.
- Ajustes recuperados desde PlayerPrefs, navegación con Tab y texto al 150 %.
- Modificación real del tiempo de reacción de 2.4 a 4.8 segundos.
- Movimiento preservado, pausa de movimiento y cancelación de confirmaciones.
- Repetición inmediata del tutorial desde su primera instrucción.
- Diálogo con pausa real, revelado gradual, avance manual y automático. Los datos de prueba son instrucciones de controles, no narrativa nueva.
- Subtítulos largos paginados, un máximo de tres líneas por página y tamaño independiente del texto general.
- Tres lecciones completadas mediante estados del dispositivo mouse de Input System, sin atribuirlas a cámara.
- Archivo de descubrimientos y mapa.
- Temporizador de combate detenido al pausar, defensas y victoria.
- Desbloqueo real de portales, visita y retorno de ambos mundos.
- Estado no disponible de voz en batch, con salida convencional.
- Reinicio asíncrono, progreso reiniciado y una sola raíz UI después de la carga.
- Nueva partida solicita reinicio también al regresar al menú sin haber descubierto piezas.

El mapa incluye todas las piezas cuyo análisis ya se completó, además de las encontradas por proximidad; la prueba lo comprueba explícitamente.

## Pruebas de modelos existentes

**29 pruebas Edit Mode aprobadas; cero fallos y cero omitidas.** Se ejecutaron las suites existentes de habilidades, gestos, objetivos, enemigos, jefe y tutorial de plaza. Informe NUnit: `Specs/UI/Evidence/EditMode-results.xml`.

## Resoluciones

Capturas reales del Canvas renderizado por Unity:

| Caso | Evidencia |
|---|---|
| 1280 × 720 | menu-1280x720.png |
| 1920 × 1080 | menu-1920x1080.png |
| 2560 × 1440 | menu-2560x1440.png |
| 1920 × 1200, 16:10 | menu-1920x1200.png |
| Texto 150 %, contraste alto, 720p | accessibility-150-720p.png |
| Texto 150 %, contraste alto, 16:10 | accessibility-150-1610.png |
| Inspección, combate, archivo y mundos | otras capturas de Specs/UI/Evidence |
| Diálogo con texto de prueba al 150 % | dialogue-validation-fixture.png |

La inspección visual corrigió el orden de lectura de la inspección, la posición de los avisos y el renderizador del mapa. Las capturas no sustituyen una prueba completa con distintas pantallas físicas o escalado DPI de Windows.

## Build de Windows

Compilación final **StandaloneWindows64, Development Build: Succeeded, 0 errores**. Ejecutable: `Builds/Nemequene/Nemequene.exe`.

Se ejecutó ese mismo binario con `--nemequene-ui-smoke` y el resultado fue **PASS, 0 errores**. Comprobó raíz UI, fuentes TMP, dispositivos apagados al inicio, apertura del menú, exploración activa y pausa efectiva. La captura se obtuvo del render real del player mediante Camera y Canvas en RenderTexture, porque el modo batch no presenta una ventana interactiva.

Evidencias: `Specs/UI/Evidence/windows-smoke.txt`, `windows-main-menu.png` y `windows-build.txt`. El diagnóstico no sustituye una partida manual completa ni una medición de rendimiento en otros equipos. La compilación y el diagnóstico usaron la misma versión del código que pasó las pruebas de Play Mode y Edit Mode.

## Errores encontrados

1. Recursos esenciales TMP ausentes: incorporados desde el paquete uGUI instalado.
2. Shader PortalVeil con identificador HLSL reservado `point`: corregido a `localPoint` sin cambiar su cálculo.
3. CanvasRenderer ausente en UIMapGraphic: corregido mediante RequireComponent.
4. **Diagnóstico del editor pendiente:** una excepción de UnityEditor.Search.SearchDatabase al iniciar la copia limpia. El informe la registra como `Editor search diagnostics: 1`, separada de los errores de juego; no se oculta ni se declara resuelta. No proviene de los controladores UI.
5. Regresos al menú sin piezas descubiertas y repetición de tutorial: corregidos y cubiertos por la validación final.
6. Captura ScreenCapture incompatible con el player en batch: el diagnóstico de desarrollo ahora renderiza explícitamente la cámara y el Canvas en RenderTexture.

## Hardware y contenido pendientes

No se ha verificado reconocimiento de voz humana ni seguimiento de manos reales en esta ejecución. La prueba automatizada no accede a cámara/micrófono. Hay que comprobar permisos Windows, ruido, idioma de voz, desconexión y reconexión de dispositivos en una sesión interactiva.

Poporo e inventario, guardado y diálogos narrativos dependen de sistemas o datos inexistentes. Sus superficies y contratos no constituyen prueba de esos sistemas futuros.

## Repetir

Play Mode en copia aislada:

```powershell
Unity.exe -batchmode -projectPath <copia> -executeMethod Nemequene.UI.Editor.UIValidation.Run -logFile <log>
```

Build Windows:

```powershell
Unity.exe -batchmode -projectPath <copia> -executeMethod Nemequene.UI.Editor.UIAssetBuilder.WindowsBuild -logFile <log>
```

La build de desarrollo incluye un diagnóstico explícito `--nemequene-ui-smoke` que comprueba raíz, fuentes, dispositivos optativos, menú, exploración y pausa; se cierra al terminar y escribe Evidence junto al ejecutable. Nunca se ejecuta durante una partida normal.

```powershell
.\Builds\Nemequene\Nemequene.exe -batchmode --nemequene-ui-smoke -logFile <log>
```
