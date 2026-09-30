# Entrega de interfaz de Nemequene

> **Actualización del 29 de septiembre de 2026:** esta entrega documenta la revisión anterior. El estado vigente de las interfaces, las tres ranuras de guardado, el inventario de hallazgos y los flujos completos está en `Specs/UI/Diseno-integral-2026-09-29.md`. El prototipo navegable está en `Specs/UI/prototipo-integral.html`. Las cifras de validación de abajo pertenecen a septiembre 14 y no verifican los cambios actuales.

## Revisión actual · interfaz esencial

La revisión del 15 de septiembre simplifica el HUD y los menús y unifica su estética con el inicio. La especificación actual está en `DESIGN.md`, sección «Interfaz esencial», y las nuevas evidencias en `Specs/UI/Essential`. Los resultados y capturas anteriores documentan versiones previas.

## Ejecutar

**Actualización del menú:** el punto de entrada es ahora `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity`. Tiene tres acciones principales, paisaje y música propios, y descarga el tutorial hasta pulsar Nueva partida. Ver `04-Menu-inicio.md` y `Specs/UI/TitleMenu` para los cambios y su validación. Los resultados anteriores de esta entrega se conservan como registro de la base de interfaz.

En Unity 6000.3.21f1 abre `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity` y pulsa Play. La raíz del tutorial se instala automáticamente al cargar la plaza. El ejecutable Windows se entrega en `Builds/Nemequene/Nemequene.exe`; distribuye la carpeta completa, incluidos Nemequene_Data, UnityPlayer.dll y MonoBleedingEdge.

Menú → Nueva partida → exploración. Configuración es opcional y abre Accesibilidad primero. En el juego, Esc pausa, Tab abre el diario, H muestra controles y R pide confirmar el reinicio. Teclado y mouse permiten completar el recorrido. Los dispositivos solo se activan por solicitud del jugador.

## Bloques implementados

Validación del 14 de septiembre de 2026: **64 comprobaciones de interfaz y 29 pruebas existentes aprobadas; build Windows y diagnóstico del ejecutable correctos, sin errores de ejecución**. Permanece un diagnóstico interno del buscador del editor, documentado por separado. Los resultados completos están en `03-Validacion.md`.

| Bloque | Archivos principales | Cómo comprobarlo |
|---|---|---|
| Auditoría, flujo y composición | 01-Auditoria-y-navegacion.md; PRODUCT.md; DESIGN.md en la raíz | Leer disponibilidad real de sistemas, wireframes y navegación |
| Sistema visual | Scripts/Data/UITheme.cs; Fonts; Resources/Nemequene/Theme.asset | Dos familias, escala 100/125/150 %, contraste alto |
| Componentes | Prefabs: 30 componentes; Scripts/Components; Resources/Nemequene/UI_Root.prefab | Foco, Tab, mouse, desactivado, confirmación y controles |
| Inicio, configuración y pausa | UIManager; ScreenManager; MenuController; SettingsUIController | Abrir, cambiar opciones, regresar; verificar pausa real |
| Exploración y piezas | HUDController; InspectionUIController; MapUIController | Moverse, examinar las tres estaciones, consultar descubrimientos |
| Combate y voz | CombatUIController; VoiceUIController; CalibrationController | E/atacar, Espacio/esquivar, F/bloquear; probar voz con hardware |
| Manos | HandTrackingUIController; CalibrationController | Activar cámara, calibrar, seleccionar; volver a mouse si falla |
| Recursos y narrativa | PoporoUIController; DialogueUIController; SubtitleController; datos asociados | Estado vacío honesto del poporo; enlazar datos autorizados |
| Cierre y recuperación | LoadingScreenController; MenuController | Reinicio confirmado, derrota por evento de vida, cierre de la visita |
| Verificación | Editor/UIValidation.cs; Scripts/UIDevelopmentSmoke.cs; 03-Validacion.md | Pruebas de Unity y diagnóstico explícito de la build |

Los controladores indicados están en `Assets/Nemequene/UI/Scripts`. Las modificaciones a sistemas previos se enumeran en `02-Integracion-y-extension.md`. Se conservaron sus cambios anteriores, escenas, geometría, personajes y contenido cultural.

## Alcance pendiente

No existe un sistema de guardado/checkpoints ni un inventario de poporo: no se ofrecen Continuar ni un acceso al inventario vacío. No se añadieron mecánicas de consumo, recompensas ficticias ni progreso guardado simulado.

Las tres piezas existentes conservan su clasificación de interpretación artística. Faltan datos históricos validados, narrativa y créditos autorales aprobados, retratos, ilustraciones y hotspots culturales. Las tarjetas y demás prefabs de dominio son componentes reutilizables que necesitan esos datos; no son contenido final de todos los mundos.

El seguimiento de manos, la voz humana y los permisos/desconexiones requieren una prueba interactiva con hardware. El resultado automatizado no demuestra reconocimiento real. Los errores encontrados y resultados exactos están en `03-Validacion.md` y `Specs/UI/Evidence`.

La integración funcional del vertical slice está disponible para revisión. La totalidad del encargo artístico y los sistemas inexistentes no se declaran terminados.
