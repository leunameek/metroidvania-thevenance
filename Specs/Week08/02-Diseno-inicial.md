# Nemequene — diseño inicial y decisiones
Versión de trabajo 0.1, 2026-09-06. No es arte final ni cierre de semana 8.

## Visión, problema y objetivos
Aventura 3D de exploración tipo metroidvania para PC Windows, con referencias culturales sujetas
a validación y ficción identificada. Problema de diseño propuesto: comunicar contexto cultural
mediante exploración e interpretación de objetos sin confundir ficción con historia. Su pertinencia
para usuarios todavía debe investigarse.
Objetivo general: preparar una base técnica y de diseño para iniciar un vertical slice en semana 9.
Objetivos específicos: verificar locomoción/legibilidad de graybox, completar un ciclo de turnos,
probar voz en combate y selección de objetos por manos, registrar fuentes y resultados reales.

## Decisiones provenientes del alcance
- Unity y Windows; continuar en 6000.3.21f1 hasta evaluar compatibilidad.
- Combate por turnos, con voz exclusivamente dentro del combate.
- Manos exclusivamente para seleccionar, analizar y manipular objetos.
- Objetos culturales como parte del progreso futuro; inventario/puzzles completos quedan fuera.
- Tres planos organizan el mundo jugable. Esta organización es una decisión del documento,
  no una afirmación histórica validada por este repositorio.
- Narrativa con referencias históricas y ficción claramente diferenciadas.
- Producción posterior enfocada en un vertical slice; juego completo fuera del alcance.
- Ningún diseño de rostro, vestuario, ornamentos, proporciones, enemigo o animación final.
- Cápsulas y cubos PLACEHOLDER, sin identidad visual de personajes.

## Narrativa y entorno provisionales
Se usan los textos breves del alcance para orientar una plaza experimental. No hay diálogos
definitivos ni cinemáticas. Plaza Núñez es aquí un nombre funcional: la geometría no pretende
reconstruir un emplazamiento histórico o actual.
Dirección ambiental propuesta: El Dorado (oro, ocre, terracota, barro y luz cálida) y Selva
(verde profundo, turquesa, azul oscuro, violeta, piedra húmeda, vegetación). Estas son
indicaciones artísticas del brief, sin atribuciones culturales nuevas.
En el graybox actual se usan grises y marcas de recorrido; la dirección visual todavía no se produce.

## Público y evidencia de usuarios
16 años en adelante; afinidad por narrativa, exploración, puzzles, mitologías y experiencias culturales.
Este es el público **propuesto en el alcance**, no una conclusión de encuestas.
Encuestas, entrevistas, hábitos, conocimientos culturales, interés por museos y necesidades de
accesibilidad: pendiente de investigación. No hay porcentajes, participantes ni citas inventados.
Preparar consentimiento y guía de prueba: completar recorrido, entender objetivo, leer objetos,
comparar teclado con voz/manos, registrar errores y comodidad. Reclutamiento y resultados pendientes.

## Base de investigación cultural
Ninguna ficha está validada en este módulo. Cada futura ficha debe tener autor/institución,
URL o referencia bibliográfica, fecha de consulta, página/pieza, cita contextual, clasificación,
responsable y límite de uso. No asumir que toda pieza precolombina es muisca.
| Temas | Clasificación actual | Pregunta pendiente |
|---|---|---|
| Cultura muisca | Pendiente de validación | Periodos, territorios y diversidad de fuentes |
| Nemequene, Tisquesusa | Pendiente de validación | Evidencias, cronologías y límites de relatos |
| Bachué, Sué, Chía | Pendiente de validación | Fuentes y variaciones de relatos/nombres |
| Mundo superior, medio e inframundo | Adaptación jugable; historia pendiente | Validar relación entre estructura propuesta y fuentes |
| Poporos, máscaras funerarias | Pendiente de validación | Procedencia y contexto de cada pieza concreta |
| Coca, yopo | Pendiente de validación | Evitar generalizaciones de uso y simbolismo |
| Animales simbólicos | Pendiente de validación | No asignar significados sin fuente |
| Piezas del Museo del Oro | Pendiente de validación | Identificación, colección, atribución y permisos |
| Textos de objetos de esta demo | Elemento ficticio | Textos del brief, no descripción histórica |
| Paletas El Dorado / Selva | Interpretación artística | Documentar referencias ambientales, no personajes |

Fuentes por consultar (no citadas como evidencia aún): catálogo y publicaciones del Museo del Oro/
Banco de la República; publicaciones académicas y fuentes de patrimonio pertinentes. No existe
integración oficial con el museo. Se deberá contrastar cronistas, interpretación posterior y
perspectivas contemporáneas; cualquier conflicto se registra, no se rellena por inferencia.

## Equipo, metodología y stakeholders
Roles propuestos: producción, programación, diseño de niveles, documentación/investigación cultural,
QA/accesibilidad y arte ambiental posterior. Nombres y dedicación: pendientes de asignar.
Stakeholders propuestos: equipo, dirección docente, jugadores de prueba, asesores culturales.
Instituciones museales/comunidades no se consideran participantes confirmados.
Tablero inicial: tabla W08 en 01-Diagnostico-y-plan.md. Revisión semanal por módulo, tareas pequeñas,
criterios verificables, evidencia de pruebas y registro de riesgos. Ningún tablero externo creado.

## Cronograma orientativo de 16 semanas
| Semanas | Resultado previsto | Situación |
|---|---|---|
| 1–2 | GDD, investigación, riesgos, estructura y pipelines | Documentación inicial; investigación pendiente |
| 3–4 | Movimiento, cámara, graybox, interacción, mini demo | Primer módulo técnico iniciado |
| 5–6 | Investigación de usuarios, pruebas de voz/manos | Resultados pendientes |
| 7–8 | Decisiones, turnos, escenas aisladas, evaluación técnica | Backlog W08 pendiente de cierre |
| 9–10 | Producir recorrido acotado del vertical slice y probar progreso por objeto | Solo planificación |
| 11–12 | Integrar voz/manos según evidencia, accesibilidad y un puzzle de slice | Solo planificación |
| 13–14 | Iterar entorno/interfaz del slice y pruebas con usuarios | Solo planificación |
| 15–16 | Estabilizar, evaluar, documentar y entregar slice interno | Solo planificación |

## Backlog semanas 9–16 (no desarrollado)
P0: validar alcance exacto de slice y sus métricas antes de integrar; convertir experimentos
de turnos/adaptadores en sistemas probados; recorrido completo acotado y un objetivo de progreso.
P1: puzzle ambiental seleccionado, dirección de entornos/objetos/UI validada, alternativas
accesibles a micrófono/webcam, sesiones reales y corrección de problemas de uso.
P2: optimización y presentación interna. Personajes finales, mundos completos, jefes,
transformaciones, publicación y colaboración oficial con museo requieren otro alcance;
no se dan por autorizados en este backlog.

## Pipeline técnico inicial
Conservar Model/Controller/View. Nuevas pruebas en subcarpetas Week08 y datos SO en Data/Week08.
Modelo no conoce controladores. Próximos adaptadores entregarán comandos/posiciones simples a
controladores; bibliotecas no deben aparecer en reglas de combate/objetivos.
Nombres: Player_PLACEHOLDER, Object_01_PLACEHOLDER, GRAYBOX_, escena Feature_Week08.
Cada asset nuevo debe conservar su .meta. Usar Git LFS para tarballs y binarios, revisar cambios
antes de commit. No versionar Library/Temp/Logs/Builds.
DCC: modelar graybox a escala de metros, aplicar transformaciones, exportar FBX con ejes compatibles,
importar con escala 1 y verificar un cubo de 1 m y pivotes. Blender/Maya no se han ejecutado aquí.
Texturas futuras: base color sRGB; mapas de datos lineales; importar normales como Normal map;
crear material URP/Lit y comprobar escala/lectura. No se producen texturas finales en esta fase.
