# Nemequene — inicio de producción hasta semana 8
Fecha: 2026-09-06. Fuente de alcance: documento entregado por el usuario.
Estado: implementación por módulos; **semana 8 todavía no aprobada**.

## Diagnóstico previo
Unity 6000.3.21f1, URP 17.3.0, Input System 1.20.0, Unity Test Framework 1.6.0.
Arquitectura MVC bajo Assets/_Game/Scripts. Prototype.Model es una asamblea independiente:
no puede referenciar Controller/View. Git configurado; hay cambios locales de manos/triggers que
se conservan. No se encontraron AGENTS.md en el repositorio ni GDD o estudios de usuarios entre
los documentos inspeccionados (README y Specs). Assets contiene escenas de recuperación y samples;
no son niveles de Nemequene ni deben eliminarse.

El log consultado registra una compilación exitosa del prototipo. Esto no demuestra una
instalación limpia: manifest.json apuntaba a C:/Users/leuna/Downloads, archivo ausente en este equipo.
Se recupera MediaPipe 0.16.3 desde Library/PackageCache en un tarball relativo al proyecto.
No se cambia Unity ni se instalan librerías nuevas.

## Comparación con aceptación de semana 8
| Sistema | Antes de este módulo | Trabajo requerido |
|---|---|---|
| Movimiento | WASD, salto, suelo, gravedad, rampas, dash/doble salto | Carrera opcional y control relativo a cámara |
| Cámara | Seguimiento fijo y modo de jefe | Experimento de órbita con parámetros y obstáculos |
| Vida/caídas | Health + PlayerRespawn | Reutilizar Health y retorno al spawn en demo |
| Objetos | Inspección de pickups; manos rotan ejes | Tres datos independientes, análisis y objetivo |
| Graybox | Movement como sandbox de mecánicas | Plaza Núñez y accesos cortos separados |
| Combate | BossFightModel: Telegraph/PlayerReact/Resolve; sin ciclo victoria/derrota completo | Nuevo boceto atacar/bloquear/esquivar, sin jefe |
| Voz | KeywordRecognizer, esquiva/dodge fijos | Configuración, tres acciones, estados, fallback y errores |
| Manos | Landmarks y giro de objetos; también usadas por jefe legado | Adaptador, cursor y selección en prueba aislada |
| Escenas | Scene, Movement y samples oficiales | TechnicalDemo_Week08 + laboratorios independientes |
| Diseño/investigación | Specs técnicos | GDD, decisiones, fuentes y pendientes; no inventar estudios |
| Build | Soporte Windows instalado | Build interna después de completar y probar módulos |

La regla nueva prohíbe manos en combate. El BossFightController anterior se conserva en Movement,
pero **no se incorpora a la demo Week08**. Sus habilidades avanzadas no se amplían.

## Plan de implementación y tareas
| ID | Prioridad | Módulo | Resultado verificable | Estado |
|---|---|---|---|---|
| W08-00 | P0 | Dependencias | MediaPipe con ruta relativa y archivo local | Implementado; ver LocalPackages/README.md |
| W08-01 | P0 | Base jugable | Plaza, movimiento/carrera, cámara, colisiones, retorno y reinicio | Implementado en esta entrega |
| W08-02 | P0 | Interacción | 3 objetos SO, texto provisional, rotación mouse, objetivo sin duplicados | Implementado en esta entrega |
| W08-03 | P0 | Turnos | Iniciar, acción, respuesta, vida, victoria/derrota y exploración | Pendiente |
| W08-04 | P0 | Voz | 3 palabras configurables, solo combate, fallback teclado | Pendiente |
| W08-05 | P0 | Manos | Adaptador, cursor, señalar/seleccionar/confirmar objeto | Pendiente |
| W08-06 | P1 | Accesos | Dos secciones cortas graybox y retornos | Pendiente |
| W08-07 | P1 | Investigación | Fuentes institucionales, validación cultural y usuarios reales | Pendiente |
| W08-08 | P1 | QA/build | Pruebas con hardware, regresión, build Windows | Pendiente |

## Escenas
- Creada: Assets/_Game/Scenes/PlazaNunez.unity.
- Planificadas, aún no creadas: CombatVoice_Week08, HandObjects_Week08,
  UnderworldAccess_Week08, UpperWorldAccess_Week08.
- Movement y escenas de MediaPipe se conservan.

## Riesgos y límites
- R01/P0: dependencia local desaparece al clonar si no se distribuye el tarball con Git LFS.
- R02/P0: micrófono/webcam y resultados humanos no se pueden certificar con una compilación.
- R03/P0: turnos existentes no equivalen al nuevo combate; falta separación del gesto de manos.
- R04/P1: KeywordRecognizer depende de Windows, permisos e idioma instalado. Evaluar con
  micrófono español antes de decidir si hace falta Vosk.
- R05/P1: la cámara es experimental; requiere recorrido visual en bordes, rampas y espacios estrechos.
- R06/P1: referencias culturales sin validar; nombres/objetos no prueban un significado histórico.
- R07/P1: documentación del legado contiene afirmaciones antiguas; usar este registro para Week08.
- R08/P1: CameraZoomTrigger legado fue escrito para una zona; varias instancias escriben al mismo
  FOV. No se usa en la nueva escena; antes de generalizarlo se necesita arbitraje de zonas.
- R09/P1: demo usa nuevo controlador de sesión; estados de combate y adaptadores se agregan
  en sus módulos, sin mezclar dependencias MediaPipe en reglas de juego.

## Después de este módulo
Cambios existentes: PlayerController añade opciones opt-in de carrera y referencia de cámara.
Archivos nuevos: Model/Week08 (configuración, datos de objetos, objetivos),
Controller/Week08 (órbita, objeto, sesión), View/Week08 (HUD),
Editor/Week08 (generador y validación), escenas/datos/materiales Week08.
Prueba: ver 03-Ejecucion.md. Evidencia real: ver 04-Validacion.md al finalizar las comprobaciones.
Combate, voz y manos del nuevo alcance siguen pendientes y no se presentan como implementados.
