# Integración, componentes y extensión

## Abrir el resultado

Abre `Assets/Prototype/Scenes/Week08/TechnicalDemo_Week08.unity` en Unity 6000.3.21f1 y pulsa Play. La UI se instala automáticamente sobre esa escena mediante el evento sceneLoaded. Si regeneras assets, usa **Tools → Nemequene → UI → Build assets**. El prefab raíz y el tema se cargan desde Resources/Nemequene.

No se requieren paquetes nuevos. Los recursos esenciales de TextMeshPro proceden del paquete uGUI ya instalado. Noto Sans y Noto Serif se incluyen con licencia abierta. No hay descargas en tiempo de ejecución.

## Responsabilidades

| Archivo / controlador | Responsabilidad |
|---|---|
| UIManager | Composición, ciclo de escena, pausa, cursor, confirmación y audio UI |
| ScreenManager | Historial, exclusión de pantallas y restauración del foco |
| MenuController | Inicio, onboarding, menú, pausa, objetivos, créditos, derrota y cierre |
| UIFactory | Componentes uGUI/TMP, retícula, layouts, raycasts y navegación |
| SettingsManager / SettingsUIController | Datos versionados, controles, validación y persistencia |
| AccessibilityManager | Escala real, contraste, cámara, tiempos de combate y efectos |
| HUDController | Vida por evento y objetivos; zona, interacción y estado contextual |
| CombatUIController | Presentación del modelo de turnos; botones delegan al combate existente |
| VoiceUIController | Reconocedor existente, escucha por turno, Ctrl, calibración y errores |
| HandTrackingUIController | Cursor suavizado, zona muerta, atracción y selección por permanencia |
| CalibrationController | Dispositivos, medición RMS, ruido ambiental, prueba de voz y guía de manos |
| InspectionUIController | Instrucción, giro existente, zoom, progreso y cultura/ficción |
| MapUIController / UIMapGraphic | Descubrimientos de sesión, caminos recorridos y archivo |
| PoporoUIController / IPoporoInventory | Presentación y contrato para enlazar un inventario futuro |
| DialogueUIController / DialogueData | Diálogo por datos, páginas, avance manual/automático, revelado ajustable y omisión |
| SubtitleController | Páginas de hasta tres líneas, tiempo de lectura, hablante, opacidad y descripción de sonidos |
| TutorialController | Secuencia contextual que observa movimiento, inspección y duelo |
| NotificationManager | Un elemento reutilizado, avisos agrupados y duración configurable |
| LoadingScreenController | Carga de escena y recuperación; prueba reversible de resolución |

Vida, objetivos, pantallas y fases usan eventos. El HUD adapta proximidad, estado de dispositivos y progreso continuo con lecturas acotadas (6–10 Hz); la barra de reacción se refresca a 20 Hz solo visible. No hay búsquedas globales por frame en la nueva UI. El árbol se crea una vez; las pantallas ocultas se desactivan. Las suscripciones y capturas se liberan al destruir la raíz.

La UI guarda la escala temporal anterior al abrir el primer panel y la restaura al cerrar el último. También llama SetHelp para impedir entrada que no dependa de deltaTime. El render pipeline se clona en memoria para aplicar gráficos sin modificar el asset original; se restaura al destruir la UI.

## Cambios puntuales en sistemas existentes

- TechnicalDemoController: evento ViewChanged, acceso a objetos/portales, bandera ManagedUI, parámetros de presentación y bloqueo de atajos durante pausa.
- PlazaCombatModel: multiplicador de reacción limitado a 1–3, con 2.4 segundos como valor original.
- PlazaCombatController: respeto de pausa en acciones públicas, Escape delegado a UI y multiplicador de destello.
- ExplorationOrbitCamera: sensibilidad, inversión vertical y suavizado configurables; movimiento y colisiones conservados.
- PlazaHandSession: selección de cámara, cancelación de una activación pendiente y desactivación explícita.
- VoiceCommandRecognizer: mantiene Dodge/esquiva y añade atacar/bloquear; confianza mínima, error de disponibilidad y liberación al desactivar.
- PortalVeil.shader: rename de una variable reservada que impedía compilar en Windows.
- ProjectSettings.asset: resolución inicial 1920 × 1080.

## Añadir contenido

**Texto:** añade una entrada key/value a `Resources/Nemequene/es.json` y consúmela con UIStrings.Get. No dupliques texto de controles en prefabs. Admite parámetros `{0}` y saltos de línea. Un idioma futuro puede añadir otro catálogo y resolverlo en UIStrings.

**Pantalla:** añade un UIScreen; registra el panel mediante ScreenManager.Register o MenuController.Page; coloca componentes de UIFactory; enlaza con Show y Back. Si consulta datos de juego, suscríbete y libera el evento en Dispose.

**Objeto:** conserva AnalyzableObjectData y su clasificación de evidencia. Añade la estación a la lista del TechnicalDemoController. Completar su análisis activa la entrada. No marques HistoricalValidated sin una fuente revisada por el equipo.

**Poporo:** implementa IPoporoInventory en el sistema de inventario cuando exista y llama `UIManager.Instance.Poporo.Bind(inventory)`. El adaptador debe emitir Changed y cantidades reales; la UI no crea consumibles ni decide costes.

**Diálogo:** crea DialogueData, añade claves de hablante/texto al catálogo y llama `UIManager.Instance.Dialogue.Show(asset)` en un punto de exploración autorizado por la narrativa. El texto se divide en páginas mediante TMP. Enter primero revela la página y luego avanza; el avance automático y la velocidad se configuran en Jugabilidad. Al pausar se conserva la lectura. No se incluye un diálogo inventado. La evidencia de validación utiliza instrucciones de controles como datos de prueba, sin incorporarlas a la narrativa.

**Subtítulo de sonido:** llama `Subtitles.Show("", texto, segundos, true)` al reproducir un sonido relevante. La transición del portal ya lo utiliza.

**Comando:** añade una frase y su acción al reconocedor, y permite su fase en VoiceUIController. El controlador de combate conserva la autoridad sobre cuándo una acción es válida.

## Particularidades de dispositivos

KeywordRecognizer usa el micrófono y el idioma del sistema de reconocimiento de Windows. El selector de calibración afecta al medidor Microphone, no promete reconfigurar Windows. La ganancia disponible es la del medidor. La API devuelve categorías de confianza, no porcentajes; se muestran Baja/Media/Alta.

La prueba de ruido mide RMS durante tres segundos. Un timeout de prueba se comunica como comando no entendido; no se inventa una transcripción. El reconocimiento real depende de los permisos, el idioma de voz de Windows y hardware.

El tracker utiliza los landmarks existentes. La guía verifica presencia, desplazamiento y permanencia con mano abierta. La vista previa solo aparece por petición dentro de calibración. La iluminación se presenta como una indicación, porque el sistema actual no entrega una medida validada de iluminación.

## Dependencias aún abiertas

- **Guardado/checkpoints:** no existen. Continuar permanece desactivado. Reiniciar comienza la visita completa y pide confirmación; no se simula un último punto guardado.
- **Poporo:** existe pantalla y contrato de datos; no hay recursos ficticios ni acciones de consumo añadidas.
- **Narrativa y créditos autorales:** faltan contenidos aprobados, retratos y secuencias de diálogos. Los controladores existen, pero no equivalen a una narrativa final integrada.
- **Voz/cámara reales:** requieren prueba con hardware y configuración de Windows. Batch valida fallback, nunca reconocimiento real.
- **Gráficos/audio:** se exponen los canales y controles funcionales existentes. No hay controles inertes de música/voces separadas, salida de audio, brillo/gamma o reasignación de teclas; el texto explica dónde corresponda.
- **Acabado avanzado:** hay 30 prefabs de componentes, la raíz y un controlador reutilizable de tooltip. Las tarjetas, indicadores y paneles de dominio son plantillas para conectar datos. Hotspots culturales, ilustraciones aprobadas y todas las variantes artísticas del encargo no están terminados. No se presenta esta base como la totalidad de los 51 apartados terminada.

## Fuentes técnicas

- [Unity KeywordRecognizer](https://docs.unity3d.com/ScriptReference/Windows.Speech.KeywordRecognizer.html)
- [TextMeshPro CreateFontAsset](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_FontAsset.CreateFontAsset.html)
- [Noto: fuentes y licencia](https://github.com/notofonts/noto-fonts)
- Las APIs de MediaPipe se contrastaron con WebCamSource, ImageSource y VisionTaskApiRunner instalados en el proyecto.
