# Guía completa del mundo superior
## El asedio de Bacatá — recorrido, montaje y piezas de producción

**Versión 1.0 · 5 de octubre de 2026**

Documento de diseño para construir el nivel en Unity. Desarrolla el [boceto de ascenso](Mundo-superior.md) y conserva sus ocho zonas, los dos pares de portales, las dos runas, las alas, los dos yopos opcionales, la llave, el transporte y el jefe de la cima.

Las posiciones y cifras siguientes son **valores iniciales de producción**, no resultados de una escena ya construida. Las decisiones nuevas se identifican como propuestas. El objetivo es que diseño, modelado, programación, sonido e interfaz trabajen sobre la misma especificación. Este documento no modifica escenas ni implementa las mecánicas.

![Boceto del mundo superior](Mundo-superior/Boceto-ascenso-v1.png)

## Índice

1. Visión, alcance y experiencia del jugador.
2. Convenciones de escala, coordenadas y montaje.
3. Recorrido completo y condiciones de progreso.
4. Movimiento, habilidades e interacción.
5. Montaje de las ocho zonas y del ramal opcional.
6. Portales, escalada, vuelo, llave y plataforma móvil.
7. Jefe de la cima y reglas de combate.
8. Guardado, recuperación y estados persistentes.
9. Catálogo completo de piezas de modelado.
10. Inventario y cantidades por zona.
11. Ensamblajes, prefabs y jerarquía de la escena.
12. Colisiones, volúmenes y marcadores invisibles.
13. Cámara, encuadre y orientación.
14. Dirección artística, materiales y ambientación muisca.
15. Animaciones, efectos y luces.
16. Sonido y música.
17. Interfaz, objetivos y textos de interacción.
18. Integración con el proyecto existente.
19. Orden de construcción y entregables.
20. Validación y criterios de aceptación.
21. Fuentes, archivos y límites de la propuesta.

## 1. Visión, alcance y experiencia del jugador

### 1.1. Qué es el mundo superior

Es un conjunto de islas y terrazas de piedra ocre suspendidas entre nubes, organizado como un ascenso de aproximadamente 70 m de desnivel entre la llegada y la arena. La imagen constante de la cima ayuda al jugador a reconocer hacia dónde progresa. La piedra clara, el cielo turquesa, los tejidos, la cerámica y los detalles de metal cálido diferencian este mundo de la caverna azulada y de raíces del mundo inferior.

La experiencia tiene cuatro etapas: **descubrir un mecanismo → adquirir una habilidad → usarla para ganar altura → alcanzar y abrir la cima**. La escalada entrega una primera sensación de conquista vertical; el vuelo amplía después la libertad entre islas. Las mejoras cambian rutas concretas y se enseñan antes de exigir precisión.

La lámina es un mapa conceptual. Sus números, flechas, arcos de colores y panel lateral pertenecen a la documentación. No se modelan como carteles flotantes dentro del juego. Sí se traducen en superficies reconocibles, marcos, apoyos, sonidos y objetivos breves.

### 1.2. Qué incluye esta versión

- Ocho zonas principales, una isla opcional para Yopo 2 y seis islas lejanas de fondo.
- Seis portales físicos: cuatro internos, uno de retorno inicial y uno de retorno tras la victoria.
- Seis hallazgos únicos: Runa 1, Runa 2, Alas, Yopo 1, Yopo 2 y Llave.
- Tres descansos persistentes, una pared escalable, cinco vuelos obligatorios y dos vuelos opcionales de ida/vuelta.
- Un transporte con dos muelles, un cierre de llave y una arena con un jefe.
- Una ruta completa viable sin Yopo 1 ni Yopo 2.
- Un kit de **41 referencias de modelado**, con **451 instancias iniciales de piezas** al incluir decoración, brazos/piernas del jefe y fragmentos reutilizables. Esas cifras no cuentan componentes, prefabs raíz, VFX ni marcadores.

No se añaden enemigos comunes, personajes de misión ni diálogos obligatorios en esta versión. El jefe es el único actor hostil; las pruebas anteriores se centran en aprender navegación. Si después se agregan centinelas, requieren una revisión de encuentros y cantidades separada.

### 1.3. Sensación y ritmo

| Tramo | Sensación buscada | Qué aprende el jugador |
|---|---|---|
| 01–02 | Llegada, observación y curiosidad | Identificar altar, descanso y portal |
| 02–03–02 | Descubrimiento y retorno corto | Una mejora abre una ruta ya vista |
| Pared 02–04 | Ascenso tangible | Leer apoyos y completar una escalada |
| 04–05 | Libertad y primer vuelo | Activar alas y aterrizar con margen |
| P3–P4 y 06 | Aplicación de la habilidad | Encadenar vuelos con recuperación en cada isla |
| 06–07 | Preparación de la cima | Recoger llave y usar transporte estable |
| 07–08 | Anticipación y cierre | Abrir el acceso y afrontar el jefe |
| Victoria | Resolución y regreso | Conservar progreso y volver a la plaza |

Objetivo de duración para una primera visita: 20–35 minutos, incluyendo lectura e intentos. Es una meta de diseño que deberá medirse. Un recorrido experto será menor. No introducir temporizadores globales, desgaste permanente de alas ni pérdida de objetos al caer.

### 1.4. Decisiones nuevas que cierran el diseño

1. Se conserva el retorno de 03 a 02 para utilizar la pared de escalada.
2. Los portales internos se activan con Runa 1 y usan E; tocar su superficie no teletransporta.
3. Runa 2 habilita exclusivamente las superficies marcadas de este nivel.
4. Las alas proporcionan vuelo controlado, limitado por un presupuesto renovable al aterrizar.
5. Para este diseño se propone un perfil local de movimiento del mundo superior: el impulso terrestre se conserva, pero el impulso no se inicia ni encadena en el aire. El vuelo cubre la movilidad aérea. Esta regla requiere código nuevo y no cambia los otros mundos.
6. Yopo 1 y 2 suman potencia de combate permanente; no son llaves, combustible ni consumibles obligatorios.
7. La llave se registra en inventario; el personaje no necesita cargar una malla física durante el transporte.
8. El jefe usa combate de turnos reactivos, coherente con la dirección del producto y el tutorial de la plaza. Las alas no sustituyen las acciones de defensa durante ese encuentro.
9. Una caída de exploración recupera al jugador sin daño en esta primera versión. La derrota del jefe sí procede del agotamiento de vida.
10. El documento y las coordenadas tienen prioridad sobre cualquier ambigüedad de perspectiva de la imagen.

## 2. Convenciones de escala, coordenadas y montaje

### 2.1. Unidades y pivotes

**1 unidad de Unity = 1 metro.** El humano de referencia mide aproximadamente 1,75 m; verificar la cápsula real, cuyo controlador existente se configura por escena. Dimensiones escritas como ancho X × alto Y × fondo Z. Un diámetro se indica con Ø.

Las coordenadas usan **(X; Y; Z)**. X positivo va hacia el lado derecho del plano de montaje, Y positivo sube y Z positivo avanza hacia el fondo. Todas las zonas de esta propuesta mantienen rotación de raíz Y = 0°. La cámara puede cambiar de orientación sin girar la geometría.

La raíz de cada zona está en el **centro de su superficie principal**, no en su entrada. Una posición local se convierte en global sumando la raíz. Ejemplo: un altar en 02, local (-2; 0; -1), tiene posición global (-2; 4; 18).

Los marcadores del personaje indican **pies sobre suelo**. No copiar sin compensación esos valores a una cápsula centrada en la raíz. El método PlayerController.Teleport recibe una posición de Transform, no un punto de pies. Calcular la compensación según CharacterController.center, height y la configuración del rig; con una cápsula de altura 2 y centro 0 suele ser aproximadamente +1 m en Y.

### 2.2. Raíces de las zonas y dimensiones

| Zona / subzona | Raíz global (X; Y; Z) | Superficie útil inicial | Cómo se relaciona con las demás |
|---|---|---|---|
| 01 Umbral | (0; 0; 0) | 12 × 12 m | Escaleras de salida hacia Z positivo |
| 02 Primera runa | (0; 4; 19) | 12 × 12 m + anexo de 4 × 4 | Debajo de 04; portal a 03 |
| 03 Runa de escalada | (38; 6; 19) | 12 × 12 m | Isla separada, accesible inicialmente por P1/P2 |
| 04 Alas | (0; 24; 19) | 12 × 12 m + anexo de 4 × 4 | Encima de 02; salida aérea hacia 05 |
| Ramal Yopo 2 | (-29; 26; 22) | 4 × 4 m | A la izquierda de 04; ida/vuelta con alas |
| 05 Portal azul | (34; 28; 19) | 6 × 6 m | Isla independiente después del primer vuelo |
| 06 Inicio P4 | (60; 42; 19) | 6 × 6 m | Portal azul antes de la secuencia de vuelo |
| 06 Apoyo A | (85; 46; 19) | 6 × 6 m | Primer aterrizaje |
| 06 Apoyo B | (110; 50; 25) | 6 × 6 m | Segundo aterrizaje, desplazamiento lateral |
| 06 Apoyo C | (135; 54; 19) | 6 × 6 m | Tercer aterrizaje |
| 06 Llave | (158; 58; 19) | 12 × 12 m | Altar fijo y muelle de partida |
| 07 Antesala | (132; 64; 68) | 12 × 12 m | Muelle de llegada, descanso y puerta |
| 08 Cima | (132; 70; 96,5) | 24 × 24 m | Arena y retorno tras victoria |

06 tiene un único contenedor de zona con raíz en Inicio P4; los otros apoyos usan offsets locales. El transporte asciende de Y 58 a Y 64. Los grandes desplazamientos globales son deliberados: explican separación física y permiten un trazado real, aunque el boceto comprima esas distancias.

### 2.3. Cómo formar las terrazas

T01 es una losa de 6 × 6 m con pivote en el centro superior. Una terraza de 12 × 12 se monta con cuatro losas en centros locales (-3; 0; -3), (3; 0; -3), (-3; 0; 3), (3; 0; 3). Una de 24 × 24 usa dieciséis losas, combinando X y Z de {-9, -3, 3, 9}.

La superficie navegable tiene un collider continuo por terraza o colliders por losa ajustados sin escalones accidentales. Las juntas son visuales; no forman huecos. La parte gruesa de la losa queda por debajo de Y = 0 de su raíz. Las bandas de borde T06 cubren perímetros exteriores; no se colocan en juntas internas.

T02 forma los anexos de escalada y la isla opcional. Los muelles A07 son geometría propia. La plataforma A08 también es propia: no convertir una losa estática en transporte sin su control de movimiento y soporte al personaje.

### 2.4. Alturas que deben recorrerse físicamente

- 01→02: +4 m mediante dos escaleras T05, no un salto de 4 m.
- 02→04: +20 m mediante escalada, no veinte saltos consecutivos.
- 04→05: +4 m y un hueco de aproximadamente 21 m entre bordes próximos.
- P3→P4: cambio instantáneo de posición y +14 m mediante portal.
- P4→A→B→C→Llave: cuatro vuelos, cada uno con +4 m.
- Llave→07: +6 m mediante transporte diagonal.
- 07→08: +6 m mediante tres escaleras T05.

Las superficies de 01, 02, 03 y 04 pueden proyectarse unas sobre otras. Los cuerpos de pilares **no pueden rellenar una terraza inferior**. En concreto, la torre que sostiene 04 ocupa la franja posterior de 02, Z global 21–25; el altar y P1 quedan delante. No construir un bloque macizo de 12 × 12 × 20 que haga desaparecer la sala 02.

### 2.5. Parámetros iniciales y calibración

El PlayerController del repositorio declara valores base de movimiento 5 m/s, multiplicador de sprint 1,6, salto de 1,5 m, gravedad -20 m/s² y escalada 4 m/s. Son valores de código; los serializados en cada escena pueden ser distintos.

Con salto simple y esos valores, el tiempo ideal de ida/vuelta a la misma altura es aproximadamente 0,775 s. El alcance horizontal ideal es 3,87 m caminando y 6,20 m corriendo, antes de considerar cápsula, bordes, variaciones de suelo o aterrizaje. No usar esa cuenta como prueba de una distancia jugable.

El modelo actual de dash aumenta la velocidad al encadenar y permite iniciar otro dash al terminar. Por eso **no basta multiplicar 12 m/s por 0,2 s y por tres** para declarar que una separación bloquea al jugador. Antes de cerrar los huecos hay que medir salto doble y cadenas reales; la regla local de dash terrestre de 1.4 es parte de esta propuesta y exige implementación explícita.

Vuelo propuesto: velocidad horizontal 8 m/s, ascenso/descenso controlado 2,5 m/s y presupuesto de 4 s. Alcance ideal horizontal de 32 m y subida ideal de 10 m por activación completa. Los vuelos obligatorios se diseñan con margen, no al límite. Revisar el alcance hasta un punto seguro dentro de la isla, no solo hasta su borde.

## 3. Recorrido completo y condiciones de progreso

### 3.1. Historia de una primera visita

El jugador llega desde la plaza a 01. Ve el sol y, en la distancia, la cima; encuentra un portal de regreso detrás y una escalera enfrente. Puede descansar antes de avanzar.

Sube a 02. El altar de Runa 1 está en suelo alcanzable sin mejoras. La pared posterior tiene apoyos visibles, pero no permite escalar. P1 está apagado hasta confirmar la runa. Yopo 1 está en un lateral y no cierra la salida.

Tras obtener Runa 1, activa P1 con E. Aparece en 03 mirando hacia el altar de Runa 2. El espacio es seguro y no pide la habilidad que todavía está recogiendo. Al confirmarla, el objetivo señala regresar por P2. La misma pareja devuelve a 02.

Ahora la pared marcada responde. Se engancha al comienzo de la escalada y asciende a 04. El tramo tiene referencias visuales intermedias; caer recupera en la base. Arriba encuentra las alas y un descanso.

Al recoger las alas se muestra la ayuda de vuelo. Desde el anexo de 04 practica el primer cruce a 05. Si desea explorar, puede ir antes a la isla de Yopo 2 y volver. Ninguna de esas dos decisiones consume una llave o altera la ruta principal.

P3, en 05, conecta con P4 al comienzo de 06. Desde P4 asciende volando a A, B y C, recuperando el presupuesto sobre cada apoyo. El último vuelo desemboca en la terraza de la llave. La inspección sucede sobre suelo fijo.

El transporte espera junto al muelle. Sube, viaja hacia arriba y atrás, y desembarca en 07. Allí descansa y utiliza el medallón en el receptáculo del cierre. El paso se abre y sigue abierto.

Sube la escalera final hacia 08. Puede observar la arena antes de iniciar el combate. Activa el encuentro desde su marca segura. Supera los turnos reactivos del guardián; la victoria activa el portal de salida. Al regresar a la plaza conserva hallazgos, zonas descubiertas y victoria.

### 3.2. Grafo de conexiones

| Origen → destino | Tipo | Requisito | Retorno |
|---|---|---|---|
| Plaza → 01 | Cambio de escena | Acceso al mundo superior desde la plaza | 01→plaza disponible desde el inicio |
| 01 ↔ 02 | Escaleras | Ninguno | Caminando |
| 02 ↔ 03 | P1/P2 | Runa 1 | Misma pareja, con nueva pulsación de E |
| 02 ↔ 04 | Escalada | Runa 2 | Descender por la misma pared |
| 04 ↔ Yopo 2 | Vuelo | Alas | Aterrizar, recuperar presupuesto y volar de vuelta |
| 04 ↔ 05 | Vuelo | Alas | Vuelo inverso; descenso de 4 m |
| 05 ↔ 06 Inicio | P3/P4 | Runa 1; P3 se alcanza con Alas | Misma pareja |
| 06 Inicio ↔ A ↔ B ↔ C ↔ Llave | Vuelos | Alas | Cada tramo admite vuelo inverso |
| 06 Llave ↔ 07 | Transporte | Ninguno adicional | Mismo transporte y dos muelles |
| 07 ↔ 08 | Cierre y escalera | Llave registrada y cierre activado | Libre antes del combate y después de victoria |
| 08 → plaza | Cambio de escena | Jefe vencido | Nueva entrada a 01 al volver a visitar el mundo |

La plataforma no exige la llave para subir: si alguien alcanza 07 sin recogerla, encuentra el bloqueo y puede volver al altar. Evita un encierro accidental. La llave tampoco exige completar los yopos.

### 3.3. Estados de progreso y objetivo actual

| Estado | Condición persistente | Nuevo acceso / objetivo |
|---|---|---|
| S0 | Sin hallazgos | Llegar al altar de Runa 1 en 02 |
| S1 | ms_runa_portales | Usar P1 para llegar a 03 |
| S2 | ms_runa_escalada | Volver por P2 y subir la pared de 02 |
| S3 | ms_alas | Volar a 05; opcionalmente visitar Yopo 2 |
| S4 | Llegada a 06 registrada | Subir por los apoyos hacia la llave |
| S5 | ms_llave | Llegar a 07 y activar el receptáculo |
| S6 | ms_cierre_abierto | Subir a 08 e iniciar el encuentro |
| S7 | ms_jefe_vencido | Usar la salida de la cima |

S4 se deriva de la zona descubierta y no reemplaza la posesión de las alas. Los yopos son flags independientes; recogerlos no cambia el orden de S0–S7. Volver a una sala no vuelve a ejecutar su premio.

### 3.4. Bloqueos visibles

| Bloqueo | Qué se ve antes de resolverlo | Mensaje / respuesta |
|---|---|---|
| P1 sin Runa 1 | Marco sin superficie luminosa | «La runa de portales activa este paso» |
| Pared sin Runa 2 | Apoyos y remate de la terraza superior | «Necesitas la runa de escalada» |
| Vuelo 04→05 sin Alas | Isla visible, hueco amplio y pedestal de alas cercano | «Las alas permiten cruzar entre islas» |
| Cierre de 07 sin Llave | Hueco circular en el receptáculo y puerta sólida | «Falta el medallón de la llave» |
| Salida de 08 antes de victoria | Marco gris apagado | «La salida se activará al vencer al guardián» |

No ocultar requisitos tras un texto largo ni colocar una recompensa detrás de su propio bloqueo.

## 4. Movimiento, habilidades e interacción

### 4.1. Controles propuestos

| Acción | Teclado / mouse | Comportamiento |
|---|---|---|
| Moverse | WASD | En el plano de la cámara de la zona |
| Correr | Shift | Solo en suelo o durante aproximación normal |
| Saltar | Espacio | Salto simple; segundo salto si ya se posee |
| Impulso terrestre | Q | Perfil de exploración con sprint habilitado; no se inicia en aire en esta propuesta |
| Interactuar | E | Hallazgo, portal, descanso, receptáculo o enganche a pared |
| Subir / bajar por pared | W / S | Movimiento vertical en la superficie activa |
| Soltarse de pared | Espacio | Sale al estado de caída; no concede un salto extra |
| Activar / cerrar alas | F | Disponible solo tras recogerlas y fuera del combate |
| Vuelo horizontal | WASD | Velocidad normalizada para no ganar velocidad en diagonal |
| Subir / bajar en vuelo | Espacio / Ctrl | Control vertical mientras quede presupuesto |
| Cancelar inspección | Esc | Regresa sin recoger; no abre pausa en la misma pulsación |
| Confirmar hallazgo | E o Enter | Solo después de abrir la inspección |
| Pausa | Esc | Congela navegación, transporte y combate |
| Diario / mapa | Tab | Consulta progreso; no activa acciones del mundo |
| Atacar en combate | E | Solo durante turno de ataque |
| Esquivar en combate | Espacio | Solo durante ventana reactiva |
| Bloquear en combate | F | Solo durante ventana reactiva |

Los controles de vuelo y escalada son una propuesta de entrada nueva; deben exponerse en ajustes y en la ayuda. F cambia de función al entrar en combate, con un contexto visible. No activar alas y bloqueo con una misma pulsación.

Cámara, micrófono y reconocimiento de manos siguen siendo opcionales. Todos los hallazgos se pueden inspeccionar con mouse y teclado. La voz, si se integra, se limita a acciones del combate y debe mostrar su resultado.

### 4.2. Estados de locomoción

Estados mínimos: Suelo, Salto/Caída, Escalada, Vuelo, EnTransporte, Inspección, Teletransporte, Combate, Recuperación y Pausa. Inspección, Teletransporte, Combate y Recuperación capturan las acciones; no pueden coexistir con otra interacción. Pausa conserva el estado anterior y lo restaura.

Un solo sistema autoriza el desplazamiento del CharacterController. No tener PlayerController moviendo al jugador en un Update y otro script de vuelo moviéndolo por segunda vez. El director del nivel decide el modo; el motor ejecuta el movimiento autorizado.

### 4.3. Recogida e inspección común

1. Desde suelo estable y en alcance horizontal de 2,2 m se ofrece «E · Examinar …».
2. El objeto no es accesible a través de una pared ni desde otra altura. Comprobar altura de pies, línea de visión y disponibilidad.
3. E abre una vista de inspección. Congelar desplazamiento y guardar cámara, pose del objeto y referencias originales.
4. Mouse o WASD rotan la pieza. La lectura no requiere completar un gesto o una rotación exacta.
5. Una nueva pulsación de E o Enter confirma. El mismo E que abrió no confirma automáticamente.
6. Escribir el flag del hallazgo antes de presentar la recompensa. Aplicar la habilidad de forma idempotente.
7. Restaurar cámara y locomoción. Ocultar la malla recogible; pedestal y soporte permanecen.
8. Esc restaura todo sin registrar el objeto. No consumir presupuesto de vuelo ni dejar input bloqueado.
9. Al cargar partida, cada altar consulta su flag antes de aparecer y no muestra un hallazgo ya obtenido.

Yopo 1 y 2 pueden usar un pequeño soporte de suelo, integrado en la propia pieza, sin pedestal O07. No añadir dos pedestales al inventario salvo que se cambie esta decisión.

### 4.4. Presupuesto de vuelo y recuperación

La primera pulsación de F en aire despliega las alas. Propuesta: horizontal 8 m/s, vertical ±2,5 m/s, duración útil 4 s. Activar desde suelo inicia un despegue corto controlado de 0,8 m, incluido en el presupuesto, sin otro salto gratuito.

La carga disminuye mientras las alas están abiertas. Cerrarlas no repone tiempo: se conserva el restante de ese despegue. Reabrir en el mismo vuelo usa el saldo; no concede otros 4 s. Ascender y moverse a la vez usa el mismo tiempo, con ejes horizontal y vertical controlados.

Al agotar la carga, las alas se cierran y continúa la caída normal; antes, a 25% de saldo, se muestra un aviso visual y sonoro suave. No cambiar repentinamente la dirección horizontal. Reducir velocidad hacia la de locomoción aérea al cerrar, con una transición corta.

Recarga completa tras 0,6 s sobre una superficie reconocida como apoyo seguro. El transporte también puede recargar cuando el personaje está apoyado de verdad. Un roce lateral contra pared, una nube decorativa, una baranda o un trigger no recarga. Los apoyos de escalada no recargan vuelo.

### 4.5. Compatibilidad con mejoras del mundo inferior

La visita funciona sin doble salto ni brazaletes. Si existen en el guardado del jugador, se conservan; no se vuelven a premiar ni se escriben flags del mundo inferior desde este nivel.

La restricción de dash aéreo es una propuesta local identificada en 1.4. Si se decide conservar el dash aéreo existente, habrá que redimensionar huecos y vuelo juntos después de medir su alcance máximo; las coordenadas presentes no garantizan los bloqueos con ese comportamiento. No ocultar este cambio como un ajuste de arte.

## 5. Montaje de las zonas
### 5.1. Zona 01 — Umbral y llegada

**Raíz:** (0; 0; 0). **Piso:** 12 × 12 m, cuatro T01. **Función:** orientar y permitir volver a la plaza desde el primer minuto.

| Elemento | Posición local de montaje | Orientación / función |
|---|---|---|
| Spawn de entrada | (0; 0; -2) | Pies; mirar +Z, hacia la escalera |
| Retorno inicial | (-3; 0; -2) | A01+A02; frente hacia el interior de la terraza |
| Descanso D01 | (3; 0; -1) | O09; sin pedestal de hallazgo |
| Inicio de escalera | (0; 0; 6) | Conexión física a 02 |
| Mirador | (0; 0; 3) | Suelo despejado; la cima queda en el encuadre |

El portal de llegada de la plaza no necesita un segundo portal de aparición aparte del retorno: la escena coloca al jugador en Spawn, fuera del volumen de interacción. La base del retorno no invade el paso central. La señal visual del descanso se ve desde Spawn.

Pretiles en seis tramos de 3 m sobre laterales y borde posterior. Dejar abierto el acceso de escalera y un mirador de 4 m, con borde legible. Las dos vasijas y el paño son decoración; no muestran prompt. Las plantas quedan junto a bordes, no en el eje de caminar.

**Lo que hace el jugador:** reconocer la salida, activar D01 si desea y avanzar a las escaleras. **Lo que se registra:** 01 descubierta; D01 como descanso si se activa. **Peligro:** ninguno obligatorio. Una salida voluntaria hacia el vacío recupera en Spawn sin daño.

**Comprobación:** al volver de la plaza a esta escena aparece con input disponible, cámara correcta y fuera del portal. No existe una interacción de hallazgo que distraiga de la salida.

### 5.2. Zona 02 — Primera runa, portal y pared observada

**Raíz:** (0; 4; 19). **Piso:** 12 × 12 m, cuatro T01. La franja posterior Z global 21–25 está ocupada por el soporte de 04; el área de altar y portal queda delante.

| Elemento | Local a raíz 02 | Global de referencia |
|---|---|---|
| Altar Runa 1 | (-2; 0; -1) | (-2; 4; 18) |
| Yopo 1 | (-4; 0; -3) | (-4; 4; 16) |
| Portal P1 | (3; 0; -1) | (3; 4; 18) |
| Llegada desde P2 | (3; 0; -4) | (3; 4; 15) |
| Anexo de base de escalada | (8; 0; 2) | (8; 4; 21) |
| Enganche de pared | (6,8; 0; 3) | (6,8; 4; 22) |
| Recovery de la pared | (8; 0; 2) | (8; 4; 21) |

La base de escalada usa T02 y se conecta al borde este de la sala en la franja libre Z 19–21. El personaje camina desde el altar hacia el lado derecho y entra en el anexo; no cruza el volumen macizo del pilar.

**Escaleras desde 01:** dos T05. Pivotes globales (0; 0; 6) y (0; 2; 9,5), rotación 0°. Cada módulo sube 2 m en 3,5 m de recorrido; el segundo termina en (0; 4; 13), coincidente con el borde sur de 02. Usar collider en rampa lisa por debajo de los peldaños visuales, con apoyos laterales; comprobar que el groundStickSpeed del controlador evita perder suelo al descender.

**Altar:** O07 en suelo, O08 sobre Y local 0,9 y O01 en su anclaje aproximadamente Y 1,12. El frente mira hacia la aproximación desde 01. Halo pequeño cálido, sin confundirlo con la energía rosada de P1.

**Antes de la runa:** P1 está apagado y la pared no permite enganche. **Después:** registrar ms_runa_portales, encender P1/P2 y P3/P4, mostrar «La runa activa los portales». No teletransportar al jugador automáticamente.

Yopo 1 usa O04 sobre suelo, con un punto de inspección estable a su lado. Recogerlo suma una mejora opcional; omitirlo mantiene el avance. El objeto no flota sin soporte.

**Comprobación:** Runa 1 es alcanzable sin otras habilidades; P1 no ofrece viaje antes de recogerla; ninguna escalera, puente o salto normal conecta físicamente 02 con 03. No colocar un puente entre esas dos islas por imitar una perspectiva ambigua de la lámina.

### 5.3. Zona 03 — Runa de escalada y vuelta a una ruta conocida

**Raíz:** (38; 6; 19). **Piso:** 12 × 12 m, cuatro T01.

| Elemento | Posición local | Función |
|---|---|---|
| Portal P2 | (-2; 0; -1) | Llegada y regreso hacia P1 |
| Spawn P2 | (-2; 0; -4) | Pies en suelo; mirar hacia el altar |
| Altar Runa 2 | (2; 0; 1) | O07+O08+O02 |
| Recovery de sala | (0; 0; -2) | Punto estable central |
| Mirador hacia pared | (-4; 0; 3) | Vista a la torre de 04 |

P2 aparece activo porque el viaje exigió Runa 1. El altar está visible al terminar la transición. Las dos vasijas y un paño se colocan detrás del altar, separados del portal y del camino de retorno. Los pretiles de esta sala protegen el sector de inspección; el borde del mirador permanece legible.

**Secuencia:** llegar → examinar O02 → confirmar → registrar ms_runa_escalada → ofrecer objetivo de retorno → usar P2. En la ficha: «Permite subir por las paredes con apoyos». No prometer escalada universal sobre todas las superficies.

El retorno a 02 usa el Spawn de P1 en (3; 4; 15), no la posición del marco. Después, caminar al anexo de pared. Al volver a mirar 03 el altar está vacío y el portal sigue activo.

**Comprobación:** cancelar la inspección deja O02 visible; confirmar dos veces no duplica habilidad; el regreso no recoge Yopo 1 por proximidad; una recarga de partida después de O02 conserva la posibilidad de escalar.

### 5.4. Conexión 02–04 — Torre y escalada

El soporte principal de 04 es una masa de **12 × 20 × 4 m**, situada en X global -6…6, Y 4…24, Z 21…25. Tres columnas de 4 m de ancho forman la masa. Por columna: dos T03 de 8 m y un T04 de 4 m. Centros X {-4, 0, 4}, Z 23; caras superiores de los módulos en Y 12, 20 y 24.

La cara escalable es la cara **este, X = 6**, con carril centrado en Z = 22. Colocar veinte A06 con centros de anclaje en (6; 4,5+i; 22), i = 0…19, saliendo hacia +X. Sus apoyos son lectura visual y contacto controlado, no veinte mini plataformas de salto libre.

| Marcador | Global de pies / referencia | Uso |
|---|---|---|
| Base segura | (8; 4; 21) | Recovery y aproximación |
| Enganche | (6,8; 4; 22) | Colocación al iniciar escalada |
| Referencia intermedia 1 | (6,8; 10; 22) | Franja visual; sin descanso persistente |
| Referencia intermedia 2 | (6,8; 17; 22) | Franja visual; sin premio |
| Salida superior | (8; 24; 21) | Anexo T02 sobre 04 |

El volumen de detección inicial puede centrarse en (6,6; 14; 22), tamaño (1,4; 20,5; 3). No etiquetarlo directamente Ladder mientras siga activo el comportamiento heredado que engancha sin runa. El control nuevo valida ms_runa_escalada, aproxima al carril y mueve respecto de su normal.

Velocidad inicial: 4 m/s; escalada directa de 20 m dura aproximadamente 5 s, más enganche/salida. W sube, S baja. A/D permite corrección limitada dentro de ±0,4 m del carril, no rodear la torre. Espacio suelta; no añade impulso de salto. No gastar energía de alas durante escalada.

Al alcanzar Y de pies 23,7, iniciar un movimiento controlado de salida hacia (8; 24; 21) de aproximadamente 0,35 s, comprobando espacio libre. Una vez ambos pies están sobre la superficie, regresar a locomoción. Descender desde 04 exige E en el borde de pared, orientado hacia el carril; no engancharse automáticamente al caminar cerca.

**Fallo:** soltarse o abandonar el carril recupera en Base segura si no existe una plataforma válida alcanzada. La caída no invoca un descanso de 01 ni borra runas. Evitar que el volumen de caída de la pared cubra el piso de 02 o el carril autorizado.

### 5.5. Zona 04 — Alas, descanso y salida aérea

**Raíz:** (0; 24; 19). **Piso:** 12 × 12 m, cuatro T01; anexo T02 en (8; 0; 2).

| Elemento | Posición local | Función |
|---|---|---|
| Altar de alas | (-2; 0; 0) | O07+O08+O03 |
| Descanso D04 | (-3; 0; -3) | O09 |
| Anexo superior | (8; 0; 2) | Salida de escalada y plataforma de lanzamiento |
| Salida de pared | (8; 0; 2) | Pies; despejada |
| Punto de ensayo | (8; 0; 2) | Mirar hacia 05 |
| Recovery principal | (0; 0; -2) | Centro seguro tras caída |
| Despegue opcional | (-4; 0; 3) | Mirar hacia isla Yopo 2 |

El jugador llega al anexo, entra en la terraza y ve el altar. El disco de descanso queda lejos del punto de salida para que E no seleccione la pared cuando se desea guardar. Los paños de esta sala ayudan a reconocer altura; las enredaderas cuelgan por debajo y no ocultan el primer hueco.

**Al recoger Alas:** registrar ms_alas, mostrar el control F y el presupuesto de vuelo, habilitar la animación de alas del personaje. O03 en el altar representa el hallazgo; el recurso equipado se convierte en una instancia visual reutilizada de la misma referencia, sin otorgar otro premio. Esa instancia equipada no se incluye en las 451 piezas ambientales; se explica en inventario.

**Práctica:** hacer primero un despegue corto sobre la propia terraza, con la ayuda «F · Abrir alas; Espacio / Ctrl · Subir o bajar». Permitir cerrar y volver a aterrizar. El vuelo hacia 05 parte del anexo, con el destino visible desde suelo seguro.

Desde el borde este del anexo X = 10 hasta el borde oeste de 05 X = 31 hay unos 21 m; el destino está 4 m arriba. El punto seguro de llegada en 05 es aproximadamente (32,5; 28; 19). No iniciar el ensayo desde el centro del altar, que añade distancia innecesaria.

**Comprobación:** sin Alas el modo de vuelo no se activa; el primer cruce tiene al menos 25% de margen de presupuesto en el perfil previsto. Si el despegue consume demasiado tiempo, ajustar alcance o separación antes de añadir efectos. D04 guarda el progreso y permite volver a intentar.

### 5.6. Ramal opcional — Yopo 2

**Raíz:** (-29; 26; 22). **Piso:** un T02 de 4 × 4 m. La isla se ve desde el lado izquierdo de 04, con una silueta pequeña y un único objeto destacado.

| Elemento | Posición global | Función |
|---|---|---|
| Piso | (-29; 26; 22) | Superficie estable |
| Yopo 2 | (-29; 26; 23) | O05; apoyado en suelo |
| Llegada / Recovery | (-29; 26; 21,5) | Pies en parte despejada |
| Retorno en 04 | (-4; 24; 22) | Aterrizaje seguro en borde occidental |

Recorrido de ida: aproximarse al borde oeste de 04, desplegar alas, subir 2 m y aterrizar. La separación entre el borde oeste de 04 y el borde este de esta isla es 21 m. Recargar 0,6 s antes de la inspección o del retorno. El vuelo inverso desciende 2 m y termina dentro del piso de 04.

La isla no tiene portal, descanso persistente ni otra llave. Su marker de Recovery sirve mientras se está en el ramal; al salir, restaurar el apoyo seguro de 04. Una recarga de partida no aparece permanentemente aquí: usa D04 si es el descanso más reciente.

El inventario asigna T02 y O05 de este ramal a la columna **Ramal/fondo**, evitando sumarlos dos veces en 04. No añadir la segunda jarra sin etiqueta que aparecía en una versión descartada de la imagen.

### 5.7. Zona 05 — Isla del portal azul

**Raíz:** (34; 28; 19). **Piso:** un T01 de 6 × 6 m. Su propósito es convertir el primer vuelo exitoso en acceso al siguiente tramo.

| Elemento | Local | Función |
|---|---|---|
| Portal P3 | (0; 0; 1) | Marco azul; frente -Z |
| Spawn desde P4 | (0; 0; -1,5) | Fuera del marco y sobre suelo |
| Recovery | (-1,5; 0; 0) | A 1,5 m del borde oeste |
| Llegada del vuelo | (-1,5; 0; 0) | Corresponde a global (32,5; 28; 19) |

P3 se coloca en la mitad posterior para dejar frente y aterrizaje libres. No poner barandas en el lado por el que aterriza el jugador. Una vasija decorativa ocupa el borde posterior; las plantas no se superponen al trigger.

P3 puede estar encendido desde obtener Runa 1. Su adquisición espacial exige las alas; no se bloquea otra vez con una segunda condición artificial de inventario. La ayuda contextual muestra «E · Viajar a Camino de la llave».

**Comprobación:** al aterrizar, el personaje recupera vuelo; E pertenece al portal y no a una vasija decorativa; volver desde P4 no provoca rebote automático. No conectar esta isla por escalera a la torre de escalada.

### 5.8. Zona 06 — P4, cuatro vuelos y terraza de la llave

**Raíz del contenedor:** (60; 42; 19). Incluye cinco superficies principales: Inicio P4, A, B, C y Llave. Inicio, A, B y C usan un T01 cada uno; Llave usa cuatro.

| Ensamblaje | Local a raíz 06 | Tamaño útil |
|---|---|---|
| Piso Inicio P4 | (0; 0; 0) | 6 × 6 |
| Portal P4 | (0; 0; 1) | A01+A02 sobre ese piso |
| Spawn P4 | (0; 0; -1,5) | Pies |
| Apoyo A | (25; 4; 0) | 6 × 6 |
| Apoyo B | (50; 8; 6) | 6 × 6 |
| Apoyo C | (75; 12; 0) | 6 × 6 |
| Terraza Llave | (98; 16; 0) | 12 × 12 |
| Altar de llave | (98; 16; 1) | Global (158; 58; 20) |
| Muelle de salida | (98; 16; 8) | Global (158; 58; 27) |

Los centros no son puntos obligatorios de despegue. El jugador sale cerca de un borde y aterriza dentro del siguiente apoyo con al menos 1,5 m de suelo hasta el borde. Cada Recovery usa el centro o una posición interior despejada, nunca la punta más cercana.

| Vuelo | Desnivel | Separación aproximada entre bordes | Presentación |
|---|---|---|---|
| P4→A | +4 m | 19 m en X | Primer tramo, recto y visible |
| A→B | +4 m | 19 m en X; destino desplazado +6 m en Z | Introduce corrección de dirección |
| B→C | +4 m | 19 m en X; destino vuelve -6 m en Z | Repite corrección en sentido inverso |
| C→Llave | +4 m | 14 m en X | Llegada más generosa y recompensa |

En los tramos con desplazamiento lateral, la distancia real depende del punto de salida y aterrizaje. Medir el recorrido diagonal completo; la cifra en X no es el largo final del vuelo. El apoyo B debe verse antes de abandonar A. No usar nubes opacas que escondan el siguiente borde.

P4 está **al inicio**, no en el altar de llave. Saltarse A y B debe exceder el presupuesto de vuelo; la carga completa solo vuelve al apoyar en suelo seguro. Los vuelos de retorno están permitidos y sus cambios de altura son descendentes.

**Llave:** O07+O08+O06 en global (158; 58; 20). Recogerla registra ms_llave y muestra «Lleva el medallón al cierre de la cima». Es un solo objeto lógico; la representación del receptáculo ocupado se activará en 07 sin crear otro hallazgo.

**Muelle:** A07 con pivote superior en (158; 58; 27), superficie de 4 × 4 m, conecta el borde norte Z 25 de la terraza con Z 29. El transporte se aborda en ese borde exterior. Un pretil corto deja libre el embarque. No poner el altar sobre la plataforma móvil.

**Comprobación:** se puede inspeccionar la llave cuando el transporte está lejos; una caída después del hallazgo conserva ms_llave; los puntos seguros de A/B/C no son descansos persistentes; recargar partida usa el último descanso persistente, no la última isla temporal.

### 5.9. Conexión 06–07 — Transporte

| Referencia | Coordenada global del centro superior |
|---|---|
| Muelle de partida | (158; 58; 27) |
| Centro de plataforma al partir | (158; 58; 32) |
| Centro de plataforma al llegar | (132; 64; 55) |
| Muelle de llegada | (132; 64; 60) |

A08 mide 4 m de ancho en X y 6 m de fondo en Z. En el extremo de partida, su borde sur Z 29 toca el borde norte Z 29 del muelle. En el extremo final, su borde norte Z 58 toca el borde sur Z 58 del muelle de llegada. La plataforma mantiene la misma rotación; no gira durante el trayecto.

La diferencia de centros es (-26; +6; +23). Longitud 3D aproximada: 35,2 m. Velocidad inicial 3 m/s, viaje de unos 11,7 s más aceleración y frenado. Espera 4 s en cada extremo. Un ciclo completo dura aproximadamente 31–34 s según la curva de salida.

El transporte hace ida y vuelta siempre que la escena está activa. No desaparece tras llevar al jugador; no exige una palanca adicional. Cualquier visitante puede esperar en un muelle y volver. La llave controla exclusivamente el cierre de 07.

### 5.10. Zona 07 — Antesala, descanso y receptáculo

**Raíz:** (132; 64; 68). **Piso:** 12 × 12 m, cuatro T01.

| Elemento | Local | Función |
|---|---|---|
| Muelle de llegada | (0; 0; -8) | A07, comparte borde Z 62 con la sala |
| Llegada segura | (0; 0; -3) | Pies; fuera del transporte |
| Descanso D07 | (-3; 0; -1) | O09 |
| Cierre de llave | (0; 0; 6) | A03+A04, cruza la salida a escaleras |
| Receptáculo | (2,1; 1,2; 5,6) | O10; sobre lateral derecho del marco |
| Punto de interacción | (2; 0; 3,5) | Pies en suelo, frente al receptáculo |
| Recovery / derrota del jefe | (0; 0; -2) | Pies, alejado de entrada de arena |

El marco A03 está en global (132; 64; 74). Su hueco permite un paso central de unos 3,2 m. La hoja A04 abre hacia un nicho lateral, sin atravesar al personaje ni bloquear el disco de descanso. El receptáculo se reconoce como una cavidad circular, coherente con O06.

**Sin llave:** E ofrece una indicación breve; la hoja sigue cerrada. **Con llave:** una pulsación de E registra ms_cierre_abierto, activa el medallón visual del receptáculo y abre la hoja. El objeto lógico ms_llave no se borra ni se convierte en consumible.

El descanso de 07 registra todas las mejoras, recupera vida y se convierte en punto de reintento del jefe. El inicio de combate también actualiza el checkpoint a 07, aunque el jugador no haya activado el disco, para evitar rehacer los vuelos por omisión.

**Escalera final:** tres T05 con pivotes globales (132; 64; 74), (132; 66; 77,5) y (132; 68; 81). Termina en (132; 70; 84,5), borde sur de 08. No interponer otro hueco o salto después de abrir la puerta.

### 5.11. Zona 08 — Arena de la cima

**Raíz:** (132; 70; 96,5). **Piso:** 24 × 24 m, dieciséis T01.

| Elemento | Posición local | Uso |
|---|---|---|
| Acceso de escalera | (0; 0; -12) | Continuidad desde 07 |
| Punto de observación | (0; 0; -8) | Vista completa antes del combate |
| Marca de jugador | (0; 0; -5) | Posición segura del encuentro |
| Guardián | (0; 0; 1) | Pies; mirar hacia -Z |
| Inicio del encuentro | (0; 0; -6) | E explícita; no comienza al descubrir sala |
| Retorno de victoria | (8; 0; 0) | Portal gris, activo tras ms_jefe_vencido |
| Spawn de revisión | (0; 0; -8) | Fuera de la marca de combate |
| Centro de abrigo circular | (0; 0; 8) | Fondo de arena, detrás del jefe |

El abrigo tiene radio de pared 3,6 m y cubierta de radio 4 m; se extiende hasta Z local 12. La cubierta puede sobresalir apenas del piso, apoyada en sus postes, sin invadir el espacio de pelea. Las bandas de borde forman el perímetro; los 24 pretiles de 3 m cubren aproximadamente 72 m del perímetro de 96 m, dejando paso de acceso y zonas de mirador.

El espacio central útil del encuentro queda libre de vasijas, rocas, postes y plantas altas. Los pretiles limitan caídas sin encerrar la cámara. El portal de salida queda visible desde la marca del jugador, separado del jefe.

**Antes del encuentro:** caminar y observar; volver a 07 si se desea. **Durante:** bloquear entrada de locomoción libre y aplicar turnos reactivos. **Después:** restaurar exploración, abrir salida y permitir regresar por la escalera si se desea.

La hoja de llave en 07 conserva su estado abierto durante el combate. El encuentro cierra la **interacción de salida y el control de locomoción**; no necesita una segunda puerta física de arena. Evita añadir una pieza o una llave no previstas.

## 6. Especificación de mecanismos

### 6.1. Portales internos y retornos

| Portal | Zona | Destino | Condición |
|---|---|---|---|
| Retorno inicial | 01 | Plaza Núñez, frente al portal superior | Siempre disponible en este mundo |
| P1 rosado | 02 | Spawn P2 de 03 | ms_runa_portales |
| P2 rosado | 03 | Spawn P1 de 02 | ms_runa_portales |
| P3 azul | 05 | Spawn P4 de 06 | ms_runa_portales |
| P4 azul | 06 Inicio | Spawn P3 de 05 | ms_runa_portales |
| Salida de cima | 08 | Plaza Núñez, frente al portal superior | ms_jefe_vencido |

Cada portal combina A02, A01, plano de efecto, trigger de interacción, destination marker, luz y componente lógico. La malla del marco tiene colisión sólida; el hueco central permanece vacío. El plano luminoso no lleva collider de pared.

Secuencia: validar condición → bloquear input e interacción → fundido breve de 0,2 s → cancelar velocidades, vuelo y escalada → teletransportar a marcador seguro → orientar personaje y cámara → fundido de vuelta → desbloquear. Para los internos no se carga otra escena.

Usar nueva pulsación de E y un bloqueo de reentrada mínimo de 0,75 s. Además, exigir abandonar el volumen de destino o una nueva interacción deliberada; el tiempo por sí solo no evita un loop si la acción se mantiene presionada.

Los pares se identifican por nombre y efecto además de color. P1/P2: contorno ovalado con dos muescas. P3/P4: contorno ovalado con tres muescas. Las muescas pueden ser submallas o textura sobre el mismo marco; no requieren otro modelo base.

### 6.2. Escalada

El motor valida Runa 2, superficie permitida, distancia al carril, espacio de cápsula y estado de exploración. El enganche cancela velocidad vertical anterior y cadenas de dash. Al soltarse, salir o teletransportarse, limpiar referencias y contadores del carril.

No activar OnGrounded por rozar A06. No invocar interacción con el pedestal de 04 desde la pared. Los límites inferior y superior son referencias explícitas; el movimiento no puede pasar por el techo o continuar subiendo dentro del piso.

La animación sigue velocidad real: detenido en pared usa pose de agarre; subiendo/bajando usa ciclo; salida superior usa transición corta. Las manos/pies deben acercarse a los apoyos, pero una primera versión puede usar agarre simple sin IK completa si conserva legibilidad.

### 6.3. Vuelo

Condiciones: Alas obtenidas, estado compatible, presupuesto positivo, cámara/motor disponibles y volumen libre para abrir alas. El vuelo se cancela al inspeccionar, teletransportar, iniciar combate o recuperar una caída. En todos esos casos, limpiar el estado y decidir recarga desde una superficie válida.

El tiempo de vuelo se pausa con el juego. El diario no debe dar recarga gratuita si el jugador está en aire. Los cambios de configuración no teletransportan ni reinician el presupuesto.

Los efectos de viento indican movimiento; no aplican fuerza a la cápsula. Esta versión no contiene corrientes ascendentes, trampas de viento ni nubes pisables. Añadirlas después implicaría otro catálogo y otro balance de vuelo.

### 6.4. Transporte y soporte al CharacterController

El transporte recorre una curva sin colisionar con pilares y mantiene suelo plano. Aplicar el delta de posición del soporte al personaje antes de su locomoción del frame, incluyendo Y. El CharacterController debe seguir el ascenso de 6 m; no basta sumar delta XZ.

No combinar parentado del jugador a la plataforma y aplicación manual del delta: produciría doble desplazamiento. Preferir un seguimiento de soporte explícito que se libera al saltar, volar, abandonar el piso, teletransportarse o morir.

Al pausar: congelar trayecto, espera, velocidad y locomoción; conservar fase. Al continuar: seguir desde el mismo punto sin salto. Al recuperar o cargar partida: colocar transporte en un extremo válido, sin intentar reproducir una fase antigua con personaje desaparecido.

Ambos muelles tienen suelo estable y marcas visuales del borde de embarque. Separación máxima inicial en reposo: 0,08 m; corregir colliders y rampas de borde. No exigir saltar sobre un transporte que pasa a velocidad máxima.

### 6.5. Llave y cierre

La llave necesita tres representaciones: hallazgo O06 en 06, icono de inventario y medallón insertado en O10. Son representaciones del mismo flag; la última puede reutilizar la malla O06 como hijo del receptáculo, añadiendo una instancia visual de montaje que se contabiliza aparte.

La hoja registra apertura antes de animar. Si se guarda durante la animación y se recarga, aparece abierta por su estado lógico. Apertura propuesta de 95° en 0,8 s hacia un nicho a la izquierda de la salida, con sonido de piedra y tope final. El collider de la hoja sigue la malla; el trigger de interacción pertenece al receptáculo, no a la puerta móvil.

### 6.6. Yopos

IDs únicos: ms_yopo_1 y ms_yopo_2. Propuesta de efecto: +5 de daño de ataque cada uno. Base del encuentro 20; con uno 25; con ambos 30. Calcular desde flags, nunca sumar otra vez al cargar o visitar el altar.

La ficha identifica la mejora como ficción del juego. El valor +5 no se atribuye a una práctica histórica. No representar preparación, consumo o instrucciones de uso reales. El modelo de jarra es un marcador de hallazgo que deberá sustituirse o validarse con referencia cultural específica antes del arte final.

## 7. Jefe de la cima

### 7.1. Identidad y función

**Nombre de producción:** Guardián de la cima. Personaje ficticio de piedra clara y metal cálido, núcleo circular visible y silueta ancha de aproximadamente 6 m. No asignarle identidad de deidad ni nombre histórico sin un desarrollo narrativo posterior.

Su función es cerrar el ascenso con un duelo de turnos reactivos. La navegación ya demostró escalada y vuelo; aquí se evalúan ataque, lectura del aviso y defensa. El combate no exige dash ni haber completado el mundo inferior.

### 7.2. Valores iniciales

| Parámetro | Valor propuesto |
|---|---|
| Vida del jugador | 100, o el máximo real si el sistema global lo configura distinto |
| Vida del jefe | 240 |
| Daño base del ataque del jugador | 20 |
| Daño con un Yopo / dos | 25 / 30 |
| Daño por reacción fallida | 20 |
| Defensa correcta | 0 daño |
| Aviso antes de ventana reactiva | 1,8 s |
| Ventana reactiva normal | 2,4 s |
| Respuesta visual tras defensa | 1,2 s |
| Inicio de fase 2 | Vida del jefe ≤120 |
| Golpes mínimos para ganar | 12 sin yopos; 10 con uno; 8 con ambos |

El número de golpes es techo de 240 dividido por daño; no es la duración completa. Debe incluir tiempos de defensa y animación. La quinta defensa fallida desde vida 100 provoca derrota si no hubo recuperación; el descanso antes del jefe restaura vida.

### 7.3. Secuencia de turno

1. **Preparación:** E en marca de inicio registra checkpoint 07, coloca al jugador en su marca y orienta al jefe. Abrir UI de combate, cerrar prompts del mundo y quitar control de vuelo.
2. **Ataque del jugador:** esperar E sin tiempo límite. Ejecutar animación y aplicar daño una vez por acción. El núcleo responde visualmente; no exige acertar un collider con dash.
3. **Victoria inmediata:** si vida del jefe llega a 0, no ejecutar una represalia final.
4. **Aviso:** mostrar silueta del ataque y verbo de respuesta durante 1,8 s.
5. **Ventana reactiva:** 2,4 s para una sola respuesta aceptada. Espacio esquiva; F bloquea.
6. **Resolución:** correcta = 0 daño; incorrecta o vencimiento = 20 daño. No aceptar pulsaciones nuevas una vez resuelta la ventana.
7. **Feedback:** 1,2 s, comprobar vida del jugador y regresar al turno de ataque si sigue vivo.
8. **Derrota:** recuperar en 07 con vida completa; jefe vuelve a 240 y fase inicial; conservar hallazgos y cierre abierto.
9. **Victoria:** registrar ms_jefe_vencido una vez, desactivar lógica hostil, abrir portal de cima y devolver exploración.

La pausa congela Remaining y animaciones. Pulsar E para iniciar no ejecuta el primer ataque en el mismo frame. Presionar Espacio durante aviso no cuenta como defensa anticipada automática.

### 7.4. Ataques y lectura

| Ataque | Silueta / aviso | Defensa | Animación y efecto |
|---|---|---|---|
| Golpe frontal | Núcleo y brazo se iluminan; «Bloquea» | F | Brazo baja; flash y golpe al escudo visual |
| Barrido lateral | Brazo se abre hacia un lado; «Esquiva» | Espacio | Desplazamiento corto lateral del jugador y estela |
| Lluvia de fragmentos | Dos sombras en marca del jugador; «Esquiva» | Espacio | Dos C06 caen visualmente; daño lo decide el modelo |
| Pulso del núcleo | Aro pequeño se abre; «Bloquea» | F | VFX radial; sin nuevo collider de daño independiente |

Fase 1 alterna frontal y barrido; fase 2 añade fragmentos y pulso en un ciclo determinista. Mantener los tiempos de reacción; la segunda fase cambia la lectura, no acelera sin aviso. Semilla o ciclo debe reiniciarse igual al reintentar para aprender el patrón.

Las piedras y el aro son presentación. El modelo de combate aplica daño una vez. No añadir DamageTrigger a esos efectos y producir daño doble. Los seis C06 son una reserva para efectos y restos; máximo dos fragmentos activos por ataque.

### 7.5. Accesibilidad y salida

Permitir multiplicador de ventana entre 1 y 3, siguiendo la ayuda de reacción del tutorial. No exigir reconocer el color: mostrar «Bloquea» / «Esquiva», icono y silueta. Con movimiento reducido, eliminar sacudida de cámara y flashes fuertes; conservar el aviso y su tiempo.

Antes de iniciar se puede regresar a 07. Durante el combate, pausa ofrece «Reintentar desde la antesala» y «Abandonar encuentro», ambas restauran vida y posicionan en 07 sin registrar victoria. No devolver directamente a un punto de aire o al transporte.

## 8. Guardado y recuperación

### 8.1. Datos persistentes por ranura

| Dato | Valor / forma | Cuándo se escribe |
|---|---|---|
| Versión | 1 | Al crear el estado |
| Flags de hallazgos | ms_runa_portales, ms_runa_escalada, ms_alas, ms_yopo_1, ms_yopo_2, ms_llave | Al confirmar cada objeto |
| Cierre abierto | ms_cierre_abierto | Antes de iniciar apertura |
| Jefe vencido | ms_jefe_vencido | Antes de presentar victoria |
| Máscara de zonas | Ocho bits para 01…08 | Primera entrada válida a cada zona |
| Ramal Yopo 2 descubierto | Booleano opcional separado | Primera llegada al ramal |
| Checkpoint | 01, 04 o 07 | Descanso; 07 también al iniciar jefe |
| Tiempo de visita | Segundos acumulados | Integración con WorldTravel / guardado global |
| Última versión de datos | Número de esquema | Para migración explícita |

Guardar inmediatamente los hallazgos; no esperar al siguiente descanso para conservar una runa. Los descansos determinan posición de recarga y derrota. La partida sin ranura, al abrir escena directamente en editor, usa memoria de sesión y lo indica en la herramienta de prueba.

Propuesta de namespace separado: Bacata.MS.v1.<slot>. No reutilizar Bacata.MI.v1 ni llamar MIProgress.Set para este mundo. Nueva partida o borrado de ranura elimina el estado del mundo superior y del inferior mediante sus autoridades respectivas.

### 8.2. Punto temporal y descanso persistente

**Apoyo seguro temporal:** última plataforma válida para una caída de exploración. Se actualiza solo después de apoyar la cápsula al menos 0,6 s y tener área libre alrededor. No se conserva al cerrar el juego.

**Descanso persistente:** 01, 04 o 07. Al recargar, aparecer allí con habilidades y objetos ya obtenidos. Puede implicar repetir una escalada o una parte del vuelo si todavía no se llegó al siguiente descanso; no implica perder sus recompensas.

No usar automáticamente cualquier punto grounded: una baranda, un borde de 20 cm, la parte superior del marco o una piedra decorativa no son puntos de recuperación válidos.

### 8.3. Qué ocurre al fallar

| Situación | Posición de vuelta | Qué se reinicia | Qué se conserva |
|---|---|---|---|
| Caída de 01/02/03 | Recovery de esa sala | Velocidades y acciones temporales | Hallazgos |
| Caída de pared | Base segura de 02 | Enganche y velocidad vertical | Ambas runas |
| Caída 04→05 | Recovery de 04 | Vuelo y su presupuesto | Alas |
| Caída al Yopo 2 antes de aterrizar | Recovery de 04 | Vuelo | Flags previos |
| Caída al salir de isla Yopo 2 | Su apoyo temporal, si era válido | Vuelo | Yopo 2 |
| Caída de P4/A/B/C | Último apoyo estable del tramo | Vuelo | Hallazgos y zonas |
| Caída del transporte | Muelle de origen del último embarque | Soporte y velocidad; transporte sigue su ciclo | Llave |
| Caída en 07/08 fuera de combate | Recovery de sala | Locomoción | Cierre y progreso |
| Derrota contra jefe | Recovery de 07 | Vida, encuentro y efectos hostiles | Hallazgos; puerta abierta |
| Recarga de partida | Último D01/D04/D07 | Estado temporal completo | Datos persistentes |

La caída de exploración no resta vida en esta propuesta inicial. Si después se desea daño, revisarlo con los descansos y las plataformas pequeñas; no activar por accidente los 30 puntos heredados de PlayerRespawn.

### 8.4. Reconstrucción al cargar

Orden recomendado: cargar ranura → validar datos → crear/activar geometry y sistemas → aplicar habilidades → ocultar hallazgos recogidos → actualizar portales → abrir cierre si procede → ocultar o desactivar jefe si vencido → situar jugador en descanso válido → orientar cámara → habilitar input.

Si ms_cierre_abierto está activo, conservar o reconstruir ms_llave como requisito consistente. Si ms_alas está activo, asegurar las dos runas anteriores al migrar datos de una versión antigua; no dar todas las mejoras por un JSON corrupto. Un guardado ilegible vuelve a un estado seguro inicial y comunica el problema a la interfaz de carga.

No persistir posición de plataforma, contador de vuelo, fase de combate, referencia de pared ni coordenadas en aire. La victoria no permite que el jefe reaparezca al volver de la plaza.

## 9. Catálogo de piezas de modelado
### 9.1. Reglas comunes de entrega

Entregar cada modelo en escala métrica, con Transform de importación uniforme, rotación coherente y pivote comprobado. Los artistas pueden reutilizar geometría entre O04/O05 y entre módulos de pilar; una referencia de producción identifica función o variante, no necesariamente una escultura distinta.

Las **41 referencias** son de modelado. Sus colliders, animaciones, materiales, iconos y componentes se detallan aparte. Un prefab de altar incluye varias referencias; no se suma como una malla adicional. Las cantidades son de la propuesta completa inicial, no un límite de rendimiento ni una lectura de una escena existente.

Usar nombres MS_<ID>_<Nombre>, conservar archivos .meta en Unity y aplicar escala en la herramienta de modelado antes de exportar. Las piezas móviles necesitan pivote de movimiento; los objetos inspeccionables necesitan una raíz central distinta del pivote de colocación cuando corresponda.

### 9.2. Tabla general de referencias

| ID | Pieza | Dimensiones iniciales en m | Pivote | Instancias |
|---|---|---|---|---|
| T01 | Losa de terraza | 6 × 1,2 × 6 | Centro de cara superior | 45 |
| T02 | Anexo / isla pequeña | 4 × 1,2 × 4 | Centro de cara superior | 3 |
| T03 | Cuerpo de pilar | 4 × 8 × 4 | Centro de cara superior | 25 |
| T04 | Remate corto de pilar | 4 × 4 × 4 | Centro de cara superior | 11 |
| T05 | Escalera de diez peldaños | 3,5 × 2 × 3,5 | Centro del borde bajo | 5 |
| T06 | Banda de borde | 6 × 1,2 × 0,3 | Centro superior del tramo | 84 |
| T07 | Remate inferior roto | 4 × 2 × 4 | Centro de cara superior | 20 |
| T08 | Panel de muro | 4 × 4 × 0,6 | Centro inferior | 19 |
| T09 | Roca decorativa | 4 × 3 × 4 | Centro inferior | 12 |
| A01 | Marco de portal | 3 × 3,8 × 0,6 | Centro inferior del paso | 6 |
| A02 | Base de portal | 4 × 0,3 × 3 | Centro superior | 6 |
| A03 | Marco del cierre de llave | 4,4 × 4 × 0,8 | Centro inferior del paso | 1 |
| A04 | Hoja del cierre | 3,2 × 3 × 0,4 | Bisagra izquierda inferior | 1 |
| A05 | Pretil | 3 × 1,1 × 0,4 | Centro inferior | 54 |
| A06 | Apoyo de escalada | 1,2 × 0,3 × 0,6 | Centro posterior contra muro | 20 |
| A07 | Muelle | 4 × 0,8 × 4 | Centro superior | 2 |
| A08 | Plataforma móvil | 4 × 0,6 × 6 | Centro superior | 1 |
| O01 | Runa de portales | Ø 0,35; grosor 0,06 | Centro del objeto | 1 |
| O02 | Runa de escalada | Ø 0,35; grosor 0,06 | Centro del objeto | 1 |
| O03 | Par de alas | 0,65 × 0,4 × 0,2 en altar | Centro entre las dos alas | 1 |
| O04 | Yopo 1 — marcador de hallazgo | 0,25 × 0,35 × 0,25 | Centro inferior | 1 |
| O05 | Yopo 2 — marcador de hallazgo | 0,25 × 0,35 × 0,25 | Centro inferior | 1 |
| O06 | Llave medallón | Ø 0,28; grosor 0,05 | Centro del objeto | 1 |
| O07 | Pedestal de hallazgo | 1,5 × 0,9 × 1,2 | Centro inferior | 4 |
| O08 | Soporte intercambiable | 0,4 × 0,2 × 0,4 | Centro inferior | 4 |
| O09 | Disco de descanso | Ø 2; altura 0,15 | Centro inferior | 3 |
| O10 | Receptáculo de llave | 0,45 × 0,7 × 0,2 | Centro posterior | 1 |
| E01 | Vasija decorativa | 0,35 × 0,5 × 0,35 | Centro inferior | 18 |
| E02 | Paño tejido | 0,9 × 2 × 0,03 | Centro superior de suspensión | 14 |
| E03 | Poste de abrigo circular | 0,2 × 3 × 0,2 | Centro inferior | 8 |
| E04 | Cubierta vegetal circular | Ø 8; altura 3 | Centro inferior | 1 |
| E05 | Panel curvo del abrigo | Arco 60°; radio 3,6; altura 2,4 | Centro inferior del arco | 6 |
| E06 | Grupo de vegetación baja | 1 × 0,6 × 1 | Centro inferior | 24 |
| E07 | Enredadera colgante | 0,4 × 4 × 0,3 | Anclaje superior | 28 |
| E08 | Isla lejana | 8 × 20 × 8 | Centro superior | 6 |
| C01 | Torso del guardián | 3,5 × 3 × 2 | Centro de pelvis | 1 |
| C02 | Cabeza del guardián | 1,3 × 1,2 × 1,2 | Base del cuello | 1 |
| C03 | Brazo del guardián | 1 × 2,6 × 1 | Centro del hombro | 2 |
| C04 | Pierna del guardián | 1 × 2,2 × 1 | Centro de cadera | 2 |
| C05 | Núcleo del guardián | Ø 0,65 | Centro del núcleo | 1 |
| C06 | Fragmento de piedra | Entre 0,5 y 0,9 por lado | Centro de masa | 6 |

### 9.3. Fichas de montaje de cada pieza

#### T01 — Losa de terraza

**Tamaño:** 6 × 1,2 × 6 m. **Pivote:** Centro de cara superior. **Cantidad inicial:** 45.

Modelar una masa de piedra con cara superior plana, lateral grueso y cara inferior sencilla. El pivote representa la altura de aterrizaje; el volumen se extiende hacia abajo. Material de suelo sobre cara superior, piedra sobre laterales. Collider Box o collider continuo del ensamblaje; no MeshCollider cóncavo. Las grietas son dibujo o relieve pequeño sin desnivel de colisión. Reutilizar exactamente la misma pieza en terrazas grandes y apoyos de 06; las cuatro esquinas rotan la decoración, manteniendo el suelo.

#### T02 — Anexo / isla pequeña

**Tamaño:** 4 × 1,2 × 4 m. **Pivote:** Centro de cara superior. **Cantidad inicial:** 3.

Una losa de 4 × 4 con borde y cara inferior integrados. Se usa en la base y salida de escalada y en la isla de Yopo 2. No requiere bandas T06 adicionales. Piso plano, collider sólido sencillo y marcador de llegada interior. El anexo de 02 se une por su lado oeste al suelo libre; el de 04 continúa al salir de pared. La isla opcional no tiene escalones ocultos ni un agujero central.

#### T03 — Cuerpo de pilar

**Tamaño:** 4 × 8 × 4 m. **Pivote:** Centro de cara superior. **Cantidad inicial:** 25.

Bloque vertical de pilar con planos amplios, juntas grandes y detalle moderado. Pivote en su remate superior; cuerpo de 8 m hacia abajo. Se encadena verticalmente sin añadir espacio entre módulos. Las seis instancias de 04 forman tres columnas de dos bloques y producen la franja posterior de la torre. En otras salas son soporte visual y cierre de paso por debajo; no se consideran plataformas escalables por defecto. Collider Box donde el jugador pueda tocarlo; desactivar colisión en soportes exclusivamente de fondo.

#### T04 — Remate corto de pilar

**Tamaño:** 4 × 4 × 4 m. **Pivote:** Centro de cara superior. **Cantidad inicial:** 11.

Versión corta compatible en sección de 4 × 4 con T03, usada para ajustar altura y cerrar soportes. Pivote superior; altura 4 m hacia abajo. En 04 remata cada columna desde Y 20 hasta 24. Los remates de 06 y 08 ajustan silueta de soportes, no añaden apoyos transitables encima de otro piso. Conservar escala positiva y sección exacta al unir piezas; el borde superior puede quedar embebido bajo la losa.

#### T05 — Escalera de diez peldaños

**Tamaño:** 3,5 × 2 × 3,5 m. **Pivote:** Centro del borde bajo. **Cantidad inicial:** 5.

Módulo de diez peldaños: 0,2 m de contrahuella y 0,35 m de huella, ancho 3,5 m. Separar malla de escalones y collider en rampa. Pivote en centro del borde de entrada, a la altura del suelo bajo; termina a +2 m Y y +3,5 m Z. Dos unidades enlazan 01–02 y tres enlazan 07–08. Laterales simples de piedra; no añadir barandas que reduzcan ancho útil. Comprobar descenso sin saltos falsos del grounded.

#### T06 — Banda de borde

**Tamaño:** 6 × 1,2 × 0,3 m. **Pivote:** Centro superior del tramo. **Cantidad inicial:** 84.

Banda lateral de 6 m que cubre el grosor del borde de una terraza T01. No lleva suelo propio ni amplía el alcance de un salto. Pivote superior a mitad del tramo, frente decorado hacia el exterior. Usar en perímetros, no entre losas. Las 84 unidades son segmentos de 6 m antes de recortar remates en puertas y escaleras; las piezas cortas proceden del mismo modelo/variante de corte, no de una referencia funcional nueva. Collider desactivado si la losa ya resuelve el borde.

#### T07 — Remate inferior roto

**Tamaño:** 4 × 2 × 4 m. **Pivote:** Centro de cara superior. **Cantidad inicial:** 20.

Remate roto bajo la base de un pilar o de una isla, con puntas amplias y sombra clara. Cara superior de 4 × 4 compatible con T03/T04. Las irregularidades se orientan hacia abajo y no se convierten en trampas. Uso visual; sin collider salvo que entre en un recorrido accesible. No usar la silueta como promesa de pared escalable. Se colocan veinte remates en soportes visibles; otros soportes pueden quedar ocultos entre nubes.

#### T08 — Panel de muro

**Tamaño:** 4 × 4 × 0,6 m. **Pivote:** Centro inferior. **Cantidad inicial:** 19.

Panel de muro de 4 m de ancho, 4 m de altura y 0,6 de grosor. Pivote en suelo al centro. Frontal con ornamentación moderada y posterior sencillo. Delimita altar, salida y fondo, sin crear un laberinto. En la torre es revestimiento parcial, no un segundo bloque que tape los apoyos. BoxCollider si cierra un paso; sin collider si cubre otra masa sólida. No colocar paneles delante de P1, P2, P3 o P4.

#### T09 — Roca decorativa

**Tamaño:** 4 × 3 × 4 m. **Pivote:** Centro inferior. **Cantidad inicial:** 12.

Roca grande de fondo y borde, con base plana y masa simple. Variar rotación y escala uniforme entre 0,7 y 1,2 sin usarla como peldaño obligatorio. En arena se coloca fuera del corredor de combate; en otras salas no invade aterrizajes. Collider solo cuando puede tocársela, y bloquear su parte superior como punto seguro si es estrecha. Las doce instancias son decoración de la versión completa, no requisitos de progreso.

#### A01 — Marco de portal

**Tamaño:** 3 × 3,8 × 0,6 m. **Pivote:** Centro inferior del paso. **Cantidad inicial:** 6.

Marco ovalado de piedra/metal, altura 3,8 m y ancho exterior 3 m, con hueco aproximado de 2 × 3 m. Submallas: marco, bandas decorativas y anclajes del efecto. Pivote en centro inferior del paso; +Z es frente antes de rotar. Seis usos: retorno inicial, P1, P2, P3, P4 y salida. El interior permanece vacío en colisión; construir laterales y remate con cajas, nunca una caja completa tapando el paso. Las muescas de pareja y color se resuelven por variante.

#### A02 — Base de portal

**Tamaño:** 4 × 0,3 × 3 m. **Pivote:** Centro superior. **Cantidad inicial:** 6.

Plinto del portal de 4 × 3 m, altura 0,3 m. Pivote en centro superior. Colocar su cara superior a suelo +0,3; base llega a suelo. Borde de acceso biselado o rampa corta compatible con stepOffset real. Marco A01 empieza sobre esta base. La zona de interacción y Spawn pertenecen al suelo cercano, no obligan a subir a un plinto estrecho. Las seis bases comparten geometría.

#### A03 — Marco del cierre de llave

**Tamaño:** 4,4 × 4 × 0,8 m. **Pivote:** Centro inferior del paso. **Cantidad inicial:** 1.

Marco de piedra del cierre final, ancho exterior 4,4 m, altura 4 m, grosor 0,8. Hueco útil de al menos 3,2 × 3 m. Pivote en centro inferior del paso, plano de la puerta XY y eje de avance +Z. Separar columnas, dintel y socket de receptáculo. La abertura tiene collider vacío; usar cajas por lateral. El marco se sitúa en la salida norte de 07, no en mitad del descanso.

#### A04 — Hoja del cierre

**Tamaño:** 3,2 × 3 × 0,4 m. **Pivote:** Bisagra izquierda inferior. **Cantidad inicial:** 1.

Hoja de piedra de 3,2 × 3 m y grosor 0,4. Pivote en bisagra inferior izquierda, aproximadamente X local -1,6 respecto de la raíz del cierre. Cerrada cubre el hueco; abierta gira 95° hacia el nicho. El modelo no incluye el marco ni O10. Collider acompaña la hoja o se desactiva tras completar apertura, conservando una política única. Permitir interrupción/reconstrucción por guardado sin encerrar al jugador.

#### A05 — Pretil

**Tamaño:** 3 × 1,1 × 0,4 m. **Pivote:** Centro inferior. **Cantidad inicial:** 54.

Pretil de piedra de 3 m, altura 1,1 y grosor 0,4. Pivote inferior. Cara interior sin puntas; protege áreas de inspección y arena. Las 54 instancias no rodean cada isla: los lados de vuelo/embarque deben quedar abiertos. Rotar el módulo 90° según borde. Collider sencillo; no permitir que su canto sea un apoyo de recuperación. Los tramos de esquina pueden solaparse ligeramente en volumen sólido, sin invadir corredor útil.

#### A06 — Apoyo de escalada

**Tamaño:** 1,2 × 0,3 × 0,6 m. **Pivote:** Centro posterior contra muro. **Cantidad inicial:** 20.

Apoyo de mano/pie de 1,2 m de ancho, 0,3 de altura y 0,6 de saliente. Pivote posterior contra el muro; colocación cada metro en Y sobre el carril de la torre. Submalla clara en cara superior y piedra en laterales. El controlador de escalada gobierna contacto. No añadir tag Ladder a cada apoyo ni permitir que veinte triggers acumulen el contador del controlador antiguo. Si tienen collider físico, excluirlos de detección de grounded/recarga.

#### A07 — Muelle

**Tamaño:** 4 × 0,8 × 4 m. **Pivote:** Centro superior. **Cantidad inicial:** 2.

Muelle fijo de 4 × 4 m con grosor 0,8 hacia abajo. Pivote superior, suelo liso y borde de embarque reconocible. Un muelle pertenece a 06 y otro a 07. En cada extremo, su borde coincide con el del transporte a altura adecuada. Barandas se montan solo en lados no usados para abordar. El collider es estático, sin animación. La zona segura y la zona de soporte del transporte son independientes.

#### A08 — Plataforma móvil

**Tamaño:** 4 × 0,6 × 6 m. **Pivote:** Centro superior. **Cantidad inicial:** 1.

Transporte rectangular de 4 × 6 m, grosor 0,6, pivote central superior. Subpartes: piso sólido, bordes bajos, remates/metales y anclaje de efecto. Mantener el piso vacío; no lleva llave, pedestal o portal encima. El movimiento es un desplazamiento de toda la raíz, sin deformar el suelo. Collider Box del piso y superficies laterales suficientes para embarque; seguimiento del personaje se programa aparte. No lleva cadenas suspendidas que se extiendan por el recorrido salvo revisión adicional de arte.

#### O01 — Runa de portales

**Tamaño:** Ø 0,35; grosor 0,06 m. **Pivote:** Centro del objeto. **Cantidad inicial:** 1.

Medallón ficticio de Runa 1, diámetro 35 cm. Relieve simple con silueta que recuerde un paso/portal; no copiar escritura histórica sin fuente. Objeto independiente del soporte y del halo. Pivote central para inspección; superficie trasera también terminada. Al confirmar se oculta este hijo y permanece el altar. La misma textura puede usarse como icono de la runa con una variante de UI.

#### O02 — Runa de escalada

**Tamaño:** Ø 0,35; grosor 0,06 m. **Pivote:** Centro del objeto. **Cantidad inicial:** 1.

Medallón ficticio de Runa 2, mismo tamaño general pero relieve distinto de escalones/apoyos. Diferenciar por silueta y contenido, no solo tono. Objeto independiente, pivote central y reverso modelado. Un único premio ms_runa_escalada. No usar una copia de O01 con idéntico icono, porque el jugador debe reconocer qué adquirió al consultar el diario.

#### O03 — Par de alas

**Tamaño:** 0,65 × 0,4 × 0,2 en altar m. **Pivote:** Centro entre las dos alas. **Cantidad inicial:** 1.

Par de alas estilizadas como pieza de hallazgo de 65 cm de ancho. Separar ala izquierda, derecha y unión central, manteniendo una raíz lógica única. En altar es pequeña; en personaje se usa una instancia/variante equipada de 1,6–2 m de envergadura, ajustada al rig sin escalar colliders del jugador. Pivotes de giro en raíz de cada ala. La expansión visual no modifica la cápsula ni permite atravesar obstáculos que el cuerpo no puede pasar.

#### O04 — Yopo 1 — marcador de hallazgo

**Tamaño:** 0,25 × 0,35 × 0,25 m. **Pivote:** Centro inferior. **Cantidad inicial:** 1.

Marcador de Yopo 1: vasija pequeña de 25 × 35 cm con tapa/soporte integrados. La propuesta visual no valida la forma cultural del recipiente. Separar la parte recogible de una base simple de suelo si se desea conservar el punto tras recoger. Pivote inferior para colocar y anclaje central de inspección. Usar ID ms_yopo_1, ficha de ficción y efecto +5; no mostrar una animación de consumo.

#### O05 — Yopo 2 — marcador de hallazgo

**Tamaño:** 0,25 × 0,35 × 0,25 m. **Pivote:** Centro inferior. **Cantidad inicial:** 1.

Marcador de Yopo 2 con las mismas dimensiones de O04 y variante visual leve que permita reconocer el segundo hallazgo. Puede reutilizar geometría y cambiar banda o tapa. Se ubica en la isla opcional y tiene ID ms_yopo_2; no es otra carga del primer objeto. Un modelo compartido puede producir dos referencias de prefab de producción sin necesitar dos esculturas nuevas.

#### O06 — Llave medallón

**Tamaño:** Ø 0,28; grosor 0,05 m. **Pivote:** Centro del objeto. **Cantidad inicial:** 1.

Medallón de llave ficticia de 28 cm, grosor 5 cm, con contorno circular y muesca de orientación. Separado de soporte y receptáculo. Pivote central, detalle frontal y posterior apto para inspección. La muesca tiene correspondencia visual en O10, sin pedir un puzzle de rotación. La copia insertada usa el mismo recurso; no se recoge por segunda vez.

#### O07 — Pedestal de hallazgo

**Tamaño:** 1,5 × 0,9 × 1,2 m. **Pivote:** Centro inferior. **Cantidad inicial:** 4.

Mesa/pedestal de 1,5 × 1,2 m, altura 0,9. Pivote inferior; base y cara superior estables. Cuatro usos: ambas runas, alas y llave. Anclaje Socket_Item en (0; 0,9; 0). Collider sólido del cuerpo y trigger de interacción independiente. No bloquea su propio punto de inspección; dejar una aproximación de al menos 2 m. Material de piedra con ornamento cálido pequeño.

#### O08 — Soporte intercambiable

**Tamaño:** 0,4 × 0,2 × 0,4 m. **Pivote:** Centro inferior. **Cantidad inicial:** 4.

Soporte intercambiable de 40 × 20 cm, pivote inferior y Socket_Item por encima. Cuatro instancias montadas sobre O07. Producir variantes disco vertical, alas y medallón dentro de esta referencia, usando cavidades sencillas. No hornear el objeto dentro de la malla del pedestal. La recompensa debe poder ocultarse sin desaparecer la superficie que la sostenía.

#### O09 — Disco de descanso

**Tamaño:** Ø 2; altura 0,15 m. **Pivote:** Centro inferior. **Cantidad inicial:** 3.

Disco de descanso de 2 m de diámetro y 15 cm de alto, pivote inferior sobre suelo. Tres instancias D01/D04/D07. Superficie de piedra clara, borde circular y núcleo visual distinto de medallones de hallazgo. Collider fino o rampa que no atrape la cápsula. Interacción y efecto están separados. No montarlo encima de O07 ni confundirlo con otro altar recogible.

#### O10 — Receptáculo de llave

**Tamaño:** 0,45 × 0,7 × 0,2 m. **Pivote:** Centro posterior. **Cantidad inicial:** 1.

Receptáculo de 45 × 70 cm sobre lateral derecho del marco de 07. Pivote posterior; cavidad frontal para O06 y socket de medallón insertado. Su parte frontal mira hacia la aproximación en 07. Un trigger en la zona de suelo cercana gestiona E; no pedir que el personaje alcance con la mano 1,2 m exactos. Estados visuales vacío/ocupado; el collider del marco sigue siendo el que delimita el paso.

#### E01 — Vasija decorativa

**Tamaño:** 0,35 × 0,5 × 0,35 m. **Pivote:** Centro inferior. **Cantidad inicial:** 18.

Vasija decorativa de 35 × 50 cm, base inferior, tapa opcional como submalla. Dieciocho instancias. Mantener diseño diferente al marcador de yopo o retirar el halo y toda interacción. Colocar a los costados de altar/refugio, no en llegada de portal ni dentro de raycast de inspección. Una variante rota puede reemplazar una unidad decorativa, sin crear un premio o peligro.

#### E02 — Paño tejido

**Tamaño:** 0,9 × 2 × 0,03 m. **Pivote:** Centro superior de suspensión. **Cantidad inicial:** 14.

Paño tejido de 0,9 × 2 m con pivote en borde superior. Catorce unidades sujetas a muro o poste, con bandas geométricas basadas en dirección artística y fuentes verificadas cuando se atribuya un patrón. Animación leve de viento por shader o huesos; no colisión. No usar simulación de tela compleja para el bloqueo inicial. No tapar la silueta de puerta o portal.

#### E03 — Poste de abrigo circular

**Tamaño:** 0,2 × 3 × 0,2 m. **Pivote:** Centro inferior. **Cantidad inicial:** 8.

Poste de madera de 20 cm de sección y 3 m de altura, pivote inferior. Ocho unidades sobre un círculo de radio aproximado 3,6 m en 08. Se reutiliza un mismo modelo con rotaciones; desactivar colisión de postes que solo sean fondo. Los accesibles usan caja simple. Dejar el acceso del abrigo libre entre los dos postes del frente.

#### E04 — Cubierta vegetal circular

**Tamaño:** Ø 8; altura 3 m. **Pivote:** Centro inferior. **Cantidad inicial:** 1.

Cubierta vegetal circular, diámetro 8 m y altura 3 m, pivote en centro del borde inferior. Va a Y local 3 sobre el abrigo, sosteniéndose en postes. Submallas: faldón, cumbrera y fibras grandes; no modelar cada hebra. El interior es sencillo y oscuro. Evitar tapar arena o núcleo del jefe en cámara; si la vista lo cruza, ocultar suavemente su malla como oclusor.

#### E05 — Panel curvo del abrigo

**Tamaño:** Arco 60°; radio 3,6; altura 2,4 m. **Pivote:** Centro inferior del arco. **Cantidad inicial:** 6.

Panel de cerramiento del abrigo, arco de 60°, radio 3,6 m, altura 2,4 m y espesor aproximado 0,15. Seis unidades de referencia cubren el perímetro si se colocan completas; para dejar entrada se recorta/omite la sección frontal de una unidad dentro de su variante. Pivote inferior de arco. Los seis módulos son una asignación máxima inicial: el panel de entrada tiene hueco, no una pared que impida entrar. No añadir un interior jugable obligatorio.

#### E06 — Grupo de vegetación baja

**Tamaño:** 1 × 0,6 × 1 m. **Pivote:** Centro inferior. **Cantidad inicial:** 24.

Grupo de vegetación baja de 1 × 1 m y 0,6 m de alto, pivote en suelo. Veinticuatro unidades. Hojas simples y masa legible; sin collider. Evitar el centro de plataformas pequeñas y el contorno de aterrizaje. Variante de tono y rotación con atlas compartido. Mantener poca presencia vegetal para que la piedra y el cielo dominen.

#### E07 — Enredadera colgante

**Tamaño:** 0,4 × 4 × 0,3 m. **Pivote:** Anclaje superior. **Cantidad inicial:** 28.

Enredadera de unos 4 m colgando desde anclaje superior. Veintiocho unidades debajo de bordes y pilares. Viento ligero; sin collider. No ocultar A06 ni crear una ruta de escalada visualmente falsa. Si su silueta parece una cuerda alcanzable, apartarla de la pared escalable o reducirla. La raíz se monta en borde inferior de la losa, sin hojas atravesando el piso.

#### E08 — Isla lejana

**Tamaño:** 8 × 20 × 8 m. **Pivote:** Centro superior. **Cantidad inicial:** 6.

Isla distante de 8 × 8 m y 20 m de alto, pivote en parte superior. Seis unidades fuera del volumen jugable. Malla simplificada de pilar/roca y losa, sin colliders, hallazgos, marcadores ni luces de premio. Posiciones sugeridas: (-55; 10; 65), (90; 15; -30), (190; 35; 70), (50; 55; 110), (170; 70; 150), (-25; 40; 120). Ajustar encuadre antes de fijarlas; nunca hacerlas parecer el próximo apoyo obligatorio.

#### C01 — Torso del guardián

**Tamaño:** 3,5 × 3 × 2 m. **Pivote:** Centro de pelvis. **Cantidad inicial:** 1.

Torso del guardián, ancho 3,5 m, alto 3 y fondo 2. Pivote de producción en pelvis. Bloques de piedra y placas cálidas con juntas grandes; espacio frontal para C05. Rig independiente de prefabs ya existentes del jugador. El torso no contiene las mallas de brazos/cabeza si esas partes se van a animar separadas. Un collider de selección/bounds no aplica daño durante turnos.

#### C02 — Cabeza del guardián

**Tamaño:** 1,3 × 1,2 × 1,2 m. **Pivote:** Base del cuello. **Cantidad inicial:** 1.

Cabeza de 1,3 × 1,2 × 1,2, pivote en cuello. Rasgos abstractos para orientar mirada sin representar una deidad. Se coloca sobre el torso; giro breve para avisos y caída/derrota. Un socket une la pieza al rig. No copiar tocado o máscara histórica con significado atribuido sin ficha de referencia.

#### C03 — Brazo del guardián

**Tamaño:** 1 × 2,6 × 1 m. **Pivote:** Centro del hombro. **Cantidad inicial:** 2.

Brazo simétrico de 1 × 2,6 × 1, pivote en hombro, dos instancias con bindings izquierda/derecha. Subpartes antebrazo/mano si el rig requiere flexión; reutilizar geometría base y conservar escalas positivas. Hombros aproximadamente a ±1,75 m en X del torso y Y 4,6 desde pies. Sus barridos son visuales; la respuesta de combate la decide el modelo.

#### C04 — Pierna del guardián

**Tamaño:** 1 × 2,2 × 1 m. **Pivote:** Centro de cadera. **Cantidad inicial:** 2.

Pierna simétrica de 1 × 2,2 × 1, pivote de cadera, dos instancias. Pies anchos y planos, sin necesidad de locomoción por toda la arena en la primera versión. Mantener raíces del rig estables; desplazamientos de ataques no deben mover el encuentro fuera de las marcas. Collider corporal sencillo; no NavMeshAgent ni navegación de perseguidor.

#### C05 — Núcleo del guardián

**Tamaño:** Ø 0,65 m. **Pivote:** Centro del núcleo. **Cantidad inicial:** 1.

Núcleo circular de 65 cm, pivote central sobre pecho. Material emisivo controlado; estados inactivo/aviso/impacto/derrota. Una única instancia. Visible desde marca de jugador durante todos los ataques. No requiere collider de daño preciso para aceptar E en el turno; la lógica aplica daño por acción válida.

#### C06 — Fragmento de piedra

**Tamaño:** Entre 0,5 y 0,9 por lado m. **Pivote:** Centro de masa. **Cantidad inicial:** 6.

Fragmento de roca de 0,5–0,9 m por lado con pivote en centro de masa. Seis instancias en reserva reutilizable para lluvia visual y restos de derrota. Máximo dos activas en cada lluvia, con sombras previas. Al finalizar volver a reserva o quedar fuera del corredor. No registrar cada piedra como enemigo y no activar daño físico adicional al daño de turno.

## 10. Inventario y cantidades por zona

### 10.1. Distribución de instancias

La columna R/F contiene la isla de Yopo 2 y las seis islas lejanas; no pertenece a una novena sala principal. Los soportes de la torre se asignan a 04, aunque se vean desde 02. Las escaleras de 01→02 se asignan a 02; las de 07→08, a 08. Cada pieza se cuenta una vez.

| ID | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | R/F | Total |
|---|---|---|---|---|---|---|---|---|---|---|
| T01 | 4 | 4 | 4 | 4 | 1 | 8 | 4 | 16 | 0 | 45 |
| T02 | 0 | 1 | 0 | 1 | 0 | 0 | 0 | 0 | 1 | 3 |
| T03 | 2 | 2 | 2 | 6 | 1 | 6 | 2 | 4 | 0 | 25 |
| T04 | 0 | 0 | 0 | 3 | 0 | 4 | 0 | 4 | 0 | 11 |
| T05 | 0 | 2 | 0 | 0 | 0 | 0 | 0 | 3 | 0 | 5 |
| T06 | 8 | 8 | 8 | 8 | 4 | 24 | 8 | 16 | 0 | 84 |
| T07 | 2 | 2 | 2 | 2 | 1 | 5 | 2 | 4 | 0 | 20 |
| T08 | 0 | 3 | 2 | 3 | 0 | 3 | 2 | 6 | 0 | 19 |
| T09 | 0 | 2 | 1 | 2 | 0 | 2 | 2 | 3 | 0 | 12 |
| A01 | 1 | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 0 | 6 |
| A02 | 1 | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 0 | 6 |
| A03 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 1 |
| A04 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 1 |
| A05 | 6 | 6 | 4 | 4 | 0 | 4 | 6 | 24 | 0 | 54 |
| A06 | 0 | 0 | 0 | 20 | 0 | 0 | 0 | 0 | 0 | 20 |
| A07 | 0 | 0 | 0 | 0 | 0 | 1 | 1 | 0 | 0 | 2 |
| A08 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 1 |
| O01 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 |
| O02 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 1 |
| O03 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 1 |
| O04 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 |
| O05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 1 |
| O06 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 1 |
| O07 | 0 | 1 | 1 | 1 | 0 | 1 | 0 | 0 | 0 | 4 |
| O08 | 0 | 1 | 1 | 1 | 0 | 1 | 0 | 0 | 0 | 4 |
| O09 | 1 | 0 | 0 | 1 | 0 | 0 | 1 | 0 | 0 | 3 |
| O10 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 1 |
| E01 | 2 | 2 | 2 | 2 | 1 | 3 | 2 | 4 | 0 | 18 |
| E02 | 1 | 2 | 1 | 2 | 0 | 2 | 2 | 4 | 0 | 14 |
| E03 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 8 | 0 | 8 |
| E04 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 1 |
| E05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 6 | 0 | 6 |
| E06 | 2 | 3 | 2 | 3 | 1 | 6 | 2 | 5 | 0 | 24 |
| E07 | 2 | 4 | 2 | 4 | 2 | 6 | 2 | 6 | 0 | 28 |
| E08 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 6 | 6 |
| C01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 1 |
| C02 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 1 |
| C03 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 0 | 2 |
| C04 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 0 | 2 |
| C05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 1 |
| C06 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 6 | 0 | 6 |
| **Total** | 32 | 47 | 35 | 68 | 13 | 80 | 39 | 129 | 8 | **451** |

### 10.2. Cómo interpretar esas cantidades

| Grupo | Referencias base | Instancias asignadas | Qué incluye |
|---|---|---|---|
| T — Terreno y estructura | 9 | 224 | Losas, anexos, pilares, escaleras, bordes y rocas |
| A — Arquitectura y mecanismos físicos | 8 | 91 | Marcos, bases, cierre, pretiles, apoyos, muelles y transporte |
| O — Hallazgos y soportes | 10 | 18 | Seis hallazgos, cuatro pedestales, cuatro soportes, tres discos y receptáculo |
| E — Entorno | 8 | 105 | Cerámica decorativa, paños, abrigo, vegetación e islas lejanas |
| C — Guardián y fragmentos | 6 | 13 | Cuerpo por partes y reserva visual de piedras |
| **Total de referencias de catálogo** | **41** | **451** | Sin componentes, VFX ni raíces lógicas |

Las 451 son instancias del catálogo de montaje. No equivalen a 451 prefabs raíz ni 451 draw calls; algunos elementos se combinan, se instancian o permanecen desactivados en reserva.

Además se necesitan dos instancias visuales derivadas: un O03 equipado en el personaje tras recoger las alas y un O06 insertado en O10 al abrir el cierre. No son referencias nuevas, no son premios adicionales y pueden empezar desactivadas. Al incluir esas dos representaciones, la asignación visual máxima pasa a 453 instancias de referencia. El jugador, sus mallas existentes, UI, luces y partículas se contabilizan fuera de ese número.

Las bandas T06 se cortan según entradas; las variantes de corte no cambian el total de segmentos asignados. Si se combina un borde con una losa en la exportación, conservar esta lista como necesidades visuales y actualizar el inventario técnico para no afirmar que quedan dos objetos separados.

### 10.3. Archivo de inventario

[Inventario-piezas.json](Mundo-superior/Inventario-piezas.json) contiene ID, nombre, medidas, pivote, cantidad total y reparto por zona de las 41 referencias. Permite importar o convertir la lista a una hoja de producción. Su campo status indica que es una propuesta, no un recuento de una escena terminada.

### 10.4. Lo que también debe producirse y no es una malla nueva

- 6 prefabs de hallazgo con IDs y fichas propios.
- 4 ensamblajes de altar y 2 de hallazgo sobre suelo.
- 6 ensamblajes de portal, 2 parejas internas y 2 retornos de escena.
- 3 ensamblajes de descanso; 1 pared escalable; 1 cierre de llave.
- 1 transporte, 2 muelles y 2 extremos de ruta.
- 1 actor de jefe, 1 modelo de turnos reactivos y 1 reserva de fragmentos.
- Materiales, luces, cielo, nubes, audio, animaciones e iconos indicados en capítulos 14–17.
- Marcadores y volúmenes invisibles indicados en capítulo 12.
- Datos de progreso, integración de viaje y modo de movimiento propios del mundo superior.

## 11. Ensamblajes, prefabs y jerarquía
### 11.1. Estructura propuesta de archivos

Rutas de producción previstas, **todavía no creadas por esta entrega**:

```text
Assets/_Game/Art/Environments/MundoSuperior/
  Scenes/MundoSuperior_Blockout.unity
  Models/{Terreno,Arquitectura,Objetos,Entorno,Guardian}/
  Prefabs/{Zonas,Hallazgos,Portales,Mecanismos,Entorno,Guardian}/
  Materials/
  Textures/
  Audio/
  Animations/
  Data/
Assets/_Game/Scripts/Controller/MundoSuperior/
Assets/_Game/Scripts/Model/MundoSuperior/
Assets/_Game/Scripts/View/MundoSuperior/
```

Conservar archivos .meta al mover recursos. Reutilizar los recursos compartidos del jugador y UI por referencia; no duplicar todo el mundo inferior para cambiar nombres.

### 11.2. Jerarquía de escena

```text
MundoSuperior
  Systems
    MSLevelDirector
    MSProgressStore
    MSInteractionRouter
    MSRecoveryDirector
    MSCombatDirector
    MSAudio
  Player
    CharacterController y Health existentes
    Motor con perfil de movimiento del mundo superior
    Visual_Alas [desactivado hasta ms_alas]
  Cameras
    MainCamera / ExplorationOrbitCamera
    CombatPresentation [configuración, no segunda cámara activa permanente]
  Zonas
    MS01_Umbral
    MS02_RunaPortales
    MS03_RunaEscalada
    MS04_Alas
    MS05_PortalAzul
    MS06_CaminoLlave
      InicioP4
      ApoyoA
      ApoyoB
      ApoyoC
      TerrazaLlave
    MS07_Antesala
    MS08_Cima
  RamalYopo2
  Mechanisms
    Pared02_04
    Transporte06_07
    CierreLlave07
  Guardian
    Rig y visuales
    EncounterMarkers
    FragmentPool
  Environment
    Sol
    Cielo
    Nubes
    IslasLejanas
  Markers
    ArrivalSpawns
    Recoveries
    Checkpoints
  UI [sistema compartido y presentadores MS]
```

No crear un segundo EventSystem si el sistema de UI ya lo proporciona. No ejecutar el director del mundo inferior en esta escena. Los hijos de una zona se mueven con su raíz; los mecanismos que conectan dos zonas tienen coordenadas globales documentadas.

### 11.3. Ensamblajes principales

| Prefab / conjunto | Piezas visibles | Componentes / referencias necesarias |
|---|---|---|
| Terraza12 | 4 T01, borde T06, soporte elegido | Collider continuo, marcador seguro, volumen de zona |
| Terraza24 | 16 T01, borde T06, soporte | Collider de arena, bounds de cámara |
| AltarRuna1 | O07+O08+O01 | ID ms_runa_portales, ficha, trigger, halo |
| AltarRuna2 | O07+O08+O02 | ID ms_runa_escalada, ficha, trigger, halo |
| AltarAlas | O07+O08+O03 | ID ms_alas, visual equipado, ayuda de vuelo |
| AltarLlave | O07+O08+O06 | ID ms_llave, ficha, trigger, halo |
| HallazgoYopo1/2 | O04/O05 | ID propio, ficha, trigger y recompensa |
| Portal | A02+A01 | Plano VFX, condición, destino, spawn, luz, sonido |
| Descanso | O09 | ID D01/D04/D07, spawn persistente, efecto y audio |
| Pared | Torre T03/T04+A06 | Motor de carril, base/top, trigger de enganche |
| Cierre | A03+A04+O10 | Bisagra, flag de apertura, visual O06 ocupado |
| Transporte | A08 | Ruta y extremos, collider, soporte al personaje, audio |
| Muelles | A07 ×2 | Punto seguro y borde de embarque |
| Guardián | C01+C02+C03×2+C04×2+C05 | Rig, modelo de turnos, animador, vida y markers |
| Fragmentos | C06 ×6 | Reserva reutilizable, sombras y limpieza por intento |

El prefab de pared no duplica los pilares de 04 ya contados; referencia sus superficies o agrupa las mismas instancias bajo un contenedor lógico. El ensamblaje Guardian usa siete instancias corporales, no siete enemigos.

### 11.4. Anclajes de producción

Cada prefab de hallazgo necesita Root_Placement, Visual_Item, InspectionPivot y SafeApproach. El altar conserva Socket_Support en Y 0,9 y Socket_Item ajustado aproximadamente a Y 1,1. El hallazgo de suelo tiene un pivot de inspección en su centro, separado del punto de colocación inferior.

Cada portal necesita FrameRoot, EffectPlane, InteractionVolume, ArrivalFeet, Facing y CameraPreset. Cada muelle necesita DockEdge y SafeFeet. El transporte necesita SurfaceCenter y extremos de ruta. Cada descanso referencia un SpawnFeet persistente.

El rig del guardián puede usar 20 huesos iniciales: root, pelvis, spine, neck, head, core; shoulder/upperArm/foreArm/hand en cada lado; thigh/calf/foot en cada lado. Esa lista suma 20. Ajustar binding a la malla final, sin modificar el rig existente del jugador.

## 12. Colisiones, volúmenes y marcadores invisibles

### 12.1. Capas y responsabilidades

| Grupo lógico | Colisión con jugador | Raycast de interacción | Oclusión de cámara | Puede ser apoyo seguro |
|---|---|---|---|---|
| Suelo estático | Sí | Sí | Sí, según pieza | Sí si tiene marker válido |
| Pilar/muro | Sí | Bloquea visión | Sí | No por tocar costado |
| Pretil | Sí | Bloquea si procede | Sí | No |
| Escalada | Controlada por motor | Solo al enganche | Cámara específica | No recarga vuelo |
| Transporte | Sí, soporte móvil | No usa E para moverse | Sí | Temporal, no checkpoint |
| Hallazgo | Trigger separado | Sí, si disponible | No prioritario | No |
| Portal | Marco sólido y trigger | Sí | Evitar que el efecto cierre encuadre | Solo su piso cercano |
| Decoración | Según tamaño | Nunca ofrece interacción | Según oclusión | No |
| Nube / isla lejana | No | No | No collider de cámara | No |
| Jefe y VFX | Bounds de presentación | Inicio explícito | Presentación propia | No |

Los nombres de capas son conceptuales; comprobar capas disponibles antes de añadir índices a ProjectSettings. Conservar una configuración compartida coherente. Los triggers no deben participar en SphereCast de obstrucción de cámara.

### 12.2. Volúmenes funcionales mínimos

| Tipo | Cantidad lógica inicial | Dimensión / colocación inicial |
|---|---|---|
| Descubrimiento y encuadre | 12 | Una por 01/02/03/04/05/07/08 y cinco subzonas de 06 |
| Rama opcional | 1 | 4 × 5 × 4 m sobre la isla Yopo 2 |
| Hallazgo | 6 | Radio/alcance 2,2 m; validar altura y línea de visión |
| Portal | 6 | Ancho 3 m, alto 3,8 y fondo de aproximación 3 m |
| Descanso | 3 | Radio 2 m sobre disco; E desde suelo |
| Enganche de escalada | 1 | (1,4; 20,5; 3) junto a X 6,6, Y 14, Z 22 |
| Límite superior/inferior de pared | 2 | Planos o umbrales del motor, no dos hallazgos |
| Receptáculo | 1 | Zona de aproximación a O10, alcance 2,2 m |
| Soporte de transporte | 1 | Superficie A08 + detección de contacto superior |
| Inicio de jefe | 1 | Radio 2 m en marca previa de 08 |
| Perfiles de recuperación | 13 | Uno por superficie/ramal descritos debajo |
| Límite global de respaldo | 1 | Y de pies < -12, controlado por única autoridad |

Un volumen de zona tiene altura aproximada 6 m: desde 0,5 m bajo el piso hasta 5,5 m sobre él. En 06, cada subzona actualiza el encuadre pero descubre el mismo ID de sala. La torre usa un preset de cámara prioritario mientras se escala, no registra una novena zona.

Los 13 perfiles corresponden a 01, 02, 03, 04, Yopo 2, 05, P4, A, B, C, Llave, 07 y 08. Cada uno agrupa uno o más colliders de vacío según los huecos que rodean su superficie. La cantidad final de colliders puede aumentar sin cambiar el catálogo de modelado.

### 12.3. Perfiles de caída

Definir corredores de vacío por tramo, evitando pisos válidos, muelles y carril de escalada. Para una isla a altura H, un plano de captura inicial puede estar en H-6 m. Usar caja de 2 m de grosor bajo el hueco, no un enorme trigger de toda la sala que capture al subir por la pared.

Para vuelos de 06, el plano de captura de cada hueco se toma de la menor altura de sus dos apoyos menos 6 m. Ejemplo P4→A: Y = 36; A→B: Y = 40; B→C: Y = 44; C→Llave: Y = 48. La región X/Z sigue el corredor del hueco y no tapa el suelo del apoyo.

Para la pared, detectar abandono/caída desde su motor y recuperar en base. Si hay un trigger bajo 04 que cruza el carril, ignorarlo únicamente mientras el personaje está en escalada autorizada; no ignorarlo para caída libre.

Para transporte, vigilar separación vertical respecto del trayecto y pérdida real de soporte. Si el jugador cae 6 m por debajo de la ruta sin aterrizar en suelo válido, recuperar al muelle de origen del último embarque. La lógica no debe necesitar un collider gigante que atraviese otras salas.

### 12.4. Marcadores requeridos

- **13 SafeFeet temporales:** uno por cada perfil de superficie anterior. Los centros de apoyos pequeños se mantienen despejados.
- **3 CheckpointFeet:** D01, D04 y D07; pueden referenciar SafeFeet existentes si su posición es adecuada.
- **4 ArrivalFeet internos:** P1/P2/P3/P4, con orientación y ajuste de cápsula.
- **1 EntryFeet desde plaza:** reutiliza el Spawn de 01.
- **2 referencias de viaje externo:** destino superior de plaza al regresar y entrada de este mundo al viajar.
- **3 referencias de pared:** BaseSafe, Engage y TopExit; Base/Top pueden reutilizar los SafeFeet.
- **2 extremos de transporte y 2 DockSafeFeet:** muelles de 06 y 07; los DockSafe pueden reutilizar un marker de suelo si están cerca.
- **3 marcas de combate:** PlayerMark, GuardianHome e IntroMark.
- **1 marker de salida tras victoria:** suelo de arena libre, anterior al portal.

No sumar todos estos nombres como objetos únicos: varias referencias pueden apuntar al mismo Transform. Lo obligatorio es que cada sistema tenga un destino válido y que nunca use el marco o una coordenada en aire como Spawn.

### 12.5. Colisiones críticas de montaje

El CharacterController no usa Rigidbody como locomoción principal. Para el transporte, elegir una política de movimiento/collider compatible; no mezclar physics-driven y desplazamiento directo sin orden.

Escaleras con rampa de colisión; puertas con collider de hoja; portales con hueco vacío; pedestales con collider del cuerpo; objetos recogibles sin muro invisible; vegetación y nubes sin collider. No reutilizar collider de árbol o de un modelo importado para una plataforma cuya cara superior no coincida.

Aterrizajes con superficie libre mínima de 3 × 3 m alrededor del marker. Muelles de 4 m permiten anchura útil de al menos 3 m después de los bordes. Si la cápsula real o alas visuales son mayores, aumentar piso/encuadre y revisar inventario.

## 13. Cámara, encuadre y orientación

### 13.1. Presentación general

Reutilizar ExplorationOrbitCamera como base: ya tiene seguimiento, detección de obstrucción, SetYaw, SetFraming y SnapAfterTeleport. Los presets siguientes son iniciales. FOV de partida 55–60°. Mantener una única cámara de render activa.

| Zona / estado | Yaw inicial | Pitch | Distancia | Objetivo del encuadre |
|---|---|---|---|---|
| 01 | 0° | 30° | 9 m | Salida a escaleras y primer hito |
| 02 | 25° | 35° | 10 m | Altar, P1 y base de pared |
| 03 | 0° | 32° | 10 m | Altar de Runa 2 y P2 |
| Escalada | 270° | 22° | 9 m | Cara este vista desde +X y próximos apoyos |
| 04 | 90° | 30° | 11 m | Anexo de salida y dirección de 05 |
| Yopo 2 | 270° | 35° | 9 m | Isla y retorno a 04 |
| 05 | 90° | 35° | 10 m | Piso y portal |
| 06 P4/A/B/C | 90° | 30° | 12 m | Siguiente isla y borde de aterrizaje |
| 06 Llave | 0° | 35° | 11 m | Altar y muelle |
| Transporte | 312° aprox. | 30° | 12 m | Muelle de destino arriba y al fondo |
| 07 | 0° | 30° | 10 m | Disco, puerta y escalera |
| 08 Exploración | 0° | 25° | 14 m | Guardián, arena y portal de salida |

Estos ángulos parten de la convención del controlador: yaw 0 mira hacia +Z, yaw 90 hacia +X. Validar después de asignar el target y movementReference. La entrada de movimiento normal puede seguir la cámara; la escalada usa su propia base local.

### 13.2. Transiciones

Cambiar yaw/pitch en suelo seguro con mezcla de 0,3–0,5 s. En un vuelo, mantener el preset del tramo; no girar 90° al cruzar un trigger pequeño. En teletransporte, aplicar el preset del destino y SnapAfterTeleport antes del fundido de vuelta.

La cámara no se pega al borde del piso ni oculta el apoyo siguiente cuando pasa detrás de un pilar. Ajustar capas de obstrucción y, si hace falta, ocultar temporalmente mallas decorativas concretas. No quitar un collider de navegación para resolver un problema de cámara.

### 13.3. Cámara de combate

Posición inicial de estudio: global (132; 75; 82), mirando hacia (132; 73; 97). Ajustar para mostrar jugador, núcleo y ambos brazos sin la cubierta del abrigo cruzando el encuadre. El portal queda en un lateral al volver a exploración.

Guardar preset de exploración al iniciar; restaurar al abandonar/ganar. Inspección no comparte cámara de combate. El fundido y la UI deben caber en 720p, 1080p y proporción 16:10.

### 13.4. Cima como orientación

La distancia global del diseño impide ver todos los detalles desde 01. Usar una silueta grande de la cima y el sol en la misma dirección general, sin falsas flechas luminosas. El sol no es un destino interactivo. Desde 04 se ve 05; desde cada apoyo de 06 se ve el siguiente; desde la llave se ve el transporte.

## 14. Dirección artística y ambientación muisca

### 14.1. Paleta y lectura de superficies

| Uso | Color inicial | Tratamiento |
|---|---|---|
| Cara navegable | Arena clara #E8C98B | Planos limpios, juntas sencillas |
| Laterales de piedra | Ocre #C49A55 | Sombra y volumen |
| Grietas / contorno | Marrón #392E23 | Contraste sin ensuciar el piso |
| Cielo | Turquesa claro #B5E6DF | Fondo suave |
| Nubes | Crema #FFF3D9 | Bordes amplios, sin cubrir aterrizajes |
| Metal | Dorado mate #B78D35 | Detalle pequeño y focal |
| Madera | Marrón #70543B | Postes y estructura del abrigo |
| Cerámica | Terracota #AA6849 | Vasijas y hallazgos provisionales |
| Vegetación | Verde apagado #6A8051 | Poca densidad |
| Portal P1/P2 | Rosado #D948C5 | Color + dos muescas + nombre |
| Portal P3/P4 | Azul #357BFF | Color + tres muescas + nombre |

La ilustración guía el contorno y los bloques de color. Para llevarlo a 3D, preferir mallas sencillas, normales limpias, ambientación suave y contorno moderado. El piso debe distinguirse del vacío incluso en escala de grises. No llenar todas las superficies de símbolos o luces.

### 14.2. Referencias culturales y ficción

La identidad muisca se construye con referencias identificadas de orfebrería, cerámica, tejidos y ofrendas; no mediante una mezcla genérica de todas las culturas precolombinas. El Banco de la República y el Museo del Oro documentan tunjos, recipientes y contextos de ofrenda; los enlaces y límites están en capítulo 21.

Las ruinas suspendidas, los dos medallones de runa, su escritura decorativa, las alas, el transporte mágico, la llave y el guardián son ficción. No describir la arquitectura de bloques como una reconstrucción histórica de un templo muisca. El abrigo circular es una dirección conceptual que requiere una referencia arquitectónica específica antes del modelado final.

No atribuir significado a espirales, colores de portal, aves o bandas tejidas sin una fuente concreta. La asociación yopo→mayor daño es una mecánica propuesta, no una afirmación sobre un efecto histórico. La documentación de objetos deberá distinguir contexto cultural de función del juego.

### 14.3. Materiales necesarios

Doce familias iniciales: MS_SueloArena, MS_PiedraOcre, MS_MetalMate, MS_Madera, MS_Tejido, MS_Ceramica, MS_Vegetacion, MS_Portal, MS_Nubes, MS_Nucleo, MS_Polvo y MS_Contorno.

El portal comparte shader con parámetros por pareja, evitando una familia de material distinta por marco. Nubes, paños y hojas requieren translucencia/alpha apropiada al proyecto. El núcleo usa emisión moderada; no ilumina todo el piso.

Texturas: base color, normal donde aporte relieve y máscara de respuesta superficial compatible con los shaders URP existentes. Atlas compartidos para piedra, arquitectura y pequeños props. Punto de partida: 2048 para piedra/arquitectura compartida; 1024 para objetos/props; 512–1024 para efectos. Son presupuestos iniciales de producción, por medir en la escena, no requisitos mínimos de hardware.

### 14.4. Variantes y detalles

Variantes de T01: esquina intacta, borde roto y grieta superficial. Todas conservan el mismo área útil y collider. Variantes de E01: vasija alta, baja y rota, sin crear hallazgos nuevos. Variantes de paño: dos bandas geométricas artísticas diferenciadas, pendientes de referencia si se les da atribución cultural.

El desgaste se concentra en bordes y caras laterales. Superficies de altar tienen desgaste menor para reconocer interacción. Solo el hallazgo disponible lleva halo; decorar con metal no concede automáticamente estado recogible.

## 15. Animación, efectos y luz

### 15.1. Animaciones a entregar o adaptar

| Animación / estado | Duración inicial | Qué mueve |
|---|---|---|
| Enganche de pared | 0,25 s | Pose del personaje, sin salto de posición no validado |
| Agarre detenido | Bucle | Pose sostenida |
| Escalada | Bucle según velocidad | Brazos/piernas; locomoción del motor |
| Salida superior | 0,35 s | Transición controlada al anexo |
| Alas abrir / cerrar | 0,25 / 0,2 s | Submallas de O03 equipado |
| Vuelo sostenido | Bucle 1 s | Pose y batido suave |
| Giro de vuelo | Mezcla 0,2 s | Inclinación visual sin mover cápsula extra |
| Aterrizaje | 0,25 s | Pose y polvo |
| Cierre de llave | 0,8 s | Giro de A04 |
| Transporte partir / llegar | Rampas de 0,8 s | Velocidad de raíz, sin rotación del piso |
| Guardián reposo | Bucle 2 s | Núcleo y pose leve |
| Frontal / barrido | 1,2 s cada uno | Brazos, torso y efecto |
| Lluvia de fragmentos | 1 s de presentación | Fragmentos C06 y gesto |
| Pulso de núcleo | 1 s | Material y efecto radial |
| Reacción a golpe | 0,3 s | Pose del guardián |
| Derrota del guardián | 2,5 s | Descenso de pose y apagado de núcleo |
| Ataque del jugador | 0,45 s | Clip existente o adaptado |
| Bloqueo del jugador | 0,5 s | Clip/pose de defensa |
| Esquiva reactiva | 0,4 s | Movimiento lateral seguro y vuelta a marca |

Los tiempos de ataque visual no acortan las ventanas del modelo. Separar root motion visual y posición autorizada: la esquiva no puede sacar al jugador del perímetro. Reutilizar animaciones existentes cuando sirvan; no alterar rigs del personaje por este documento.

### 15.2. Efectos necesarios

| ID | Efecto | Instancias activas previstas |
|---|---|---|
| FX01 | Energía y borde de portal | 6, con estados apagado/activo |
| FX02 | Halo de hallazgo disponible | 6; se apaga al recoger |
| FX03 | Descanso: pulso suave | 3 |
| FX04 | Estela de alas | 1 equipada |
| FX05 | Polvo de escalada/aterrizaje | Reserva pequeña, compartida |
| FX06 | Energía del transporte | 1 |
| FX07 | Aro de pulso del guardián | 1 reutilizable |
| FX08 | Impacto/bloqueo y sombras de fragmentos | Hasta 2 por ataque |
| FX09 | Nubes en capas | 12 conjuntos visuales iniciales |
| FX10 | Victoria y encendido de salida | 1 secuencia |
| FX11 | Fundido de viaje/recuperación | 1 efecto de UI compartido |

Nubes: cuatro grupos bajos, cuatro medios y cuatro altos, distribuidos después del encuadre. No son plataformas ni tienen collider. Se puede usar planos orientados/volúmenes sencillos sin crear una malla única por nube. FX09 se cuenta aparte del catálogo de 41 modelos.

### 15.3. Iluminación

Una luz direccional principal cálida para el sol; ambientación del cielo suave. Sombras de pilares necesarias para volumen, con detalle moderado. Las nubes no deben crear una sombra móvil que oculte continuamente el borde de aterrizaje.

Luces puntuales locales: seis de portal, cuatro de altar principal y tres de descanso; Yopo 1/2 pueden usar solo emisión de halo. El núcleo puede usar emisión sin luz puntual permanente. Reducir luces simultáneas según visibilidad; no asumir que todas necesitan sombras en tiempo real.

Respetar movimiento reducido: detener respiración intensa de portal, batidos decorativos excesivos y sacudidas, manteniendo trayectos y señales indispensables de transporte/ataque. El ajuste no puede eliminar un aviso que permite jugar.

## 16. Sonido y música

### 16.1. Lista de recursos sonoros

| ID | Recurso | Disparador y mezcla |
|---|---|---|
| S01 | Viento/ambiente aéreo | Bucle bajo; no tapa UI |
| S02 | Música de exploración | Bucle suave; aumenta densidad hacia cima |
| S03 | Música del jefe | Entra al iniciar; sale al vencer/abandonar |
| S04 | Pasos de piedra | Movimiento en suelo; variantes sin cambio de mecánica |
| S05 | Salto | Despegue normal |
| S06 | Aterrizaje | Contacto válido; intensidad según caída |
| S07 | Escalada | Ritmo de contacto de manos/pies |
| S08 | Alas desplegar | Inicio de F |
| S09 | Vuelo | Bucle mientras alas abiertas; se detiene en pausa/teleport |
| S10 | Alas cerrar | Cierre voluntario o por presupuesto |
| S11 | Portal activar/viajar | Al confirmar viaje |
| S12 | Llegada por portal | Después de colocar en destino |
| S13 | Inspección abrir | Hallazgo disponible |
| S14 | Hallazgo confirmar | Premio único |
| S15 | Descanso | Guardado/checkpoint confirmado |
| S16 | Cierre de piedra | Apertura y tope de hoja |
| S17 | Transporte | Bucle espacial y parada |
| S18 | Aviso del jefe | Inicio de telegraph |
| S19 | Impacto de ataque | Daño válido y golpe visual |
| S20 | Defensa | Variantes correcta/incorrecta |
| S21 | Victoria | Una vez por secuencia de victoria |
| S22 | Recuperación | Fundido breve de caída |
| S23 | Acción no disponible | Error suave, con limitación de repetición |

Son 23 referencias funcionales; algunas pueden reutilizar recursos existentes y otras agrupan variantes. No es obligatorio grabar 23 sonidos completamente nuevos. El ambiente de caverna del mundo inferior no corresponde a este nivel.

### 16.2. Audio de orientación y accesibilidad

El transporte tiene audio espacial que se aproxima al muelle, pero su llegada también es visible. El portal emite tono solo cuando está activo; mostrar destino en pantalla. El agotamiento de vuelo se comunica con barra y aviso, además del sonido.

Separar volúmenes de música, ambiente, efectos y UI según el sistema existente. Al pausar, detener sonidos continuos de acciones, manteniendo una mezcla de pausa si ya existe. El silencio del usuario no debe impedir reconocer un ataque o el borde de embarque.

La música puede incorporar timbres de percusión y aire dentro de la ficción del juego. No presentar una pista generada o compuesta para el juego como reconstrucción de música muisca histórica sin investigación específica.

## 17. Interfaz y textos

### 17.1. Qué se muestra durante exploración

Objetivo breve, nombre de zona al descubrirla, interacción contextual y presupuesto de vuelo cuando es relevante. Vida aparece en combate o si cambia; no llenar la pantalla de contadores permanentes de todos los props.

En diario: seis hallazgos únicos, habilidades, zonas descubiertas 0–8, cierre y victoria. Runa 1 y 2 tienen fichas diferentes. Los yopos se identifican como opcionales. No mostrar una vasija decorativa como objeto de inventario.

El mapa representa 02→03→02 y 02→04 con conexiones diferentes. P1/P2 y P3/P4 conservan sus nombres además del color. El ramal Yopo 2 se muestra aparte de las ocho zonas principales.

### 17.2. Objetivos en orden

| Estado | Texto de objetivo |
|---|---|
| Sin Runa 1 | «Sube al altar y examina la runa de portales» |
| Con Runa 1, sin Runa 2 | «Usa el portal rosado para encontrar la runa de escalada» |
| Con Runa 2, sin Alas | «Regresa por P2 y escala la pared hacia las alas» |
| Con Alas, antes de 06 | «Vuela a la isla del portal azul» |
| En 06, sin Llave | «Asciende por los apoyos y recoge la llave» |
| Con Llave, cierre cerrado | «Usa el transporte y coloca el medallón en la antesala» |
| Cierre abierto, sin victoria | «Sube a la cima y enfrenta al guardián» |
| Jefe vencido | «Regresa a la plaza por el portal de la cima» |

Yopo 1/2 produce notificación pequeña de mejora, sin sustituir el objetivo principal. Al volver a una zona, no reponer mensajes de tutorial ya aprendidos salvo consulta de ayuda.

### 17.3. Prompts

- Hallazgo: «E · Examinar runa de portales / runa de escalada / alas / llave / Yopo 1 / Yopo 2».
- Confirmación: «E o Enter · Recoger» y «Esc · Volver».
- Portal: «E · Viajar a [destino]» o «E · Regresar a Plaza Núñez».
- Descanso: «E · Descansar y guardar».
- Pared: «E · Escalar»; dentro, «W/S · Subir/bajar · Espacio · Soltarse».
- Vuelo: «F · Alas · Espacio/Ctrl · Altura» y saldo visible.
- Receptáculo con llave: «E · Colocar medallón».
- Jefe: «E · Iniciar encuentro», después controles específicos del turno.

Una sola autoridad elige el prompt más próximo y válido. Prioridad durante exploración: inspección activa > portal/receptáculo elegido > descanso > enganche; resolver por distancia y orientación, sin mostrar cuatro paneles superpuestos. No enviar una misma E a dos sistemas.

### 17.4. Textos de fichas provisionales

**Runa de portales:** «Medallón ficticio que despierta los pasos entre islas. Activa los portales de este mundo.»

**Runa de escalada:** «Su marca permite reconocer y utilizar los apoyos de las paredes señaladas. Regresa a la torre que viste junto al primer altar.»

**Alas:** «Abren un vuelo breve entre terrazas. Aterriza en suelo firme para recuperar su capacidad.»

**Yopo 1 / Yopo 2:** «Hallazgo opcional. En la ficción del juego aumenta la potencia de tus ataques. Su representación y contexto cultural requieren una ficha de referencia antes del arte final.»

**Llave:** «Medallón del cierre de la cima. Colócalo en el receptáculo de la antesala.»

Son textos de diseño; no constituyen fichas museales definitivas. Evitar mezclar poderes ficticios con una atribución arqueológica.

## 18. Integración con el proyecto

### 18.1. Qué existe y qué debe desarrollarse

| Área | Evidencia en el repositorio revisado | Trabajo del mundo superior |
|---|---|---|
| Locomoción | PlayerController, PlayerAbilityModel | Perfil local, estados y motor de vuelo |
| Escalada | Contacto con tag Ladder; sin validación de Runa 2 | Carril, condición de habilidad, enganche/salida y limpieza |
| Cámara | ExplorationOrbitCamera con presets y snap | Zonas MS y presets de navegación/transporte/combate |
| Inspección | MIFind y MIInteractable, ligados a MIProgress y director inferior | Componente compartido o equivalente MS con flags propios |
| Guardado | MIProgress por ranura y WorldTravel.SaveSlot | MSProgressStore con namespace separado y seis hallazgos |
| Portales | MIPortal retorna a la plaza con mundo -1 | Portales internos y retorno superior con mundo +1 |
| Viaje de escena | WorldTravel.SceneFor resuelve solo mundo inferior | Añadir ruta de escena superior y conservar retorno de plaza |
| Combate reactivo | PlazaCombatModel, tutorial de tres golpes | Modelo de jefe con vida, daño, patrón, fase y derrota |
| Recuperación | PlayerRespawn y director inferior | Una autoridad MS; desactivar respuestas heredadas en esta escena |
| UI | Sistemas Nemequene/UI y UIKit | Presentadores/objetivos MS e iconos de alas/runas/llave |
| Transporte | No se identificó un transporte superior funcional en los archivos revisados | Movimiento y soporte al CharacterController |
| Nivel superior | Umbral de plaza; sin escena superior completa enlazada en WorldTravel | Graybox, recursos y escena final |

No afirmar que las alas ya funcionan porque el arte las muestre. No afirmar que copiar MIPortal produce P1/P2: el componente revisado llama ReturnToPlaza(-1).

### 18.2. Componentes propuestos

Nombres de trabajo, no archivos implementados por esta entrega:

| Componente | Responsabilidad |
|---|---|
| MSLevelDirector | Inicialización, referencias de zonas y coordinación de modos |
| MSProgressStore | Estado por ranura, flags idempotentes, máscara y checkpoint |
| MSMovementMode | Autorización de suelo, escalada, vuelo y bloqueo por interacción |
| MSClimbSurface | Carril, condición de Runa 2, límites y salida |
| MSFlightMotor | Presupuesto, desplazamiento y recarga válida |
| MSInteractionRouter | Elegir y ejecutar una sola acción contextual |
| MSFind | Inspección, ficha y confirmación de seis hallazgos |
| MSPortal | Tipo interno/externo, condición, destino y fundido |
| MSRest | Guardar checkpoint y restaurar vida/capacidad |
| MSKeySocket | Registrar apertura y sincronizar representación |
| MSMovingPlatform | Trayecto, pausa, espera y delta del soporte |
| MSRecoveryDirector | Caídas, validación de apoyo y derrotas |
| MSRoomZone | Descubrimiento y encuadre sin premios |
| MSGuardianCombatModel | Turnos, vida, daño, fase y victoria |
| MSGuardianPresenter | Animaciones, sombras, núcleo y fragmentos |
| MSHudPresenter | Objetivos, prompts, vuelo y combate |

Los componentes de presentación no escriben progreso por su cuenta. Los motores no manipulan PlayerPrefs directamente. Los hallazgos solicitan premio a una autoridad; el store calcula capacidades desde flags. Ese reparto permite probar reglas sin requerir un modelo final.

### 18.3. Viaje desde y hacia la plaza

Agregar una constante de escena superior prevista: Assets/_Game/Scenes/MundoSuperior.unity. SceneFor debe distinguir world < 0 para inferior y world > 0 para superior. La escena tiene que existir y estar en Build Settings antes de devolver su ruta válida.

Al salir de la plaza, conservar lecciones, resultado del entrenamiento, máscara de visitas, tiempo y ranura con WorldTravel. Al volver, ReturnToPlaza(+1) identifica el portal superior. No usar -1 por copiar el retorno del mundo inferior.

Nueva visita aparece en 01 por la entrada de mundo; la carga directa desde un guardado puede usar D01/D04/D07 según la política del menú. Diferenciar esos dos motivos de entrada en un parámetro de viaje, evitando que visitar desde plaza teletransporte por sorpresa a 07.

### 18.4. Riesgos concretos de reutilización

- Ladder activa escalada sin runa en PlayerController: no basta cambiar el color del apoyo.
- PlayerController.Teleport limpia velocidad y dash, pero el modo nuevo debe limpiar además pared, alas y soporte móvil.
- El contador de ladders existente puede quedar desfasado al teletransportar con controller desactivado: la nueva autoridad debe reconstruir/cancelar contacto explícitamente.
- MIFind y MIPortal están vinculados a MIProgress, MIAudio y director inferior. Copiarlos sin separar referencias puede otorgar premios del mundo incorrecto.
- PlazaCombatModel gana al tercer ataque y no maneja la vida propuesta del jefe. Reutilizar su patrón de estados, no sus constantes como si fueran el jefe completo.
- PlayerRespawn usa un umbral global -10 y daño de caída. Debe desactivarse para este mundo si MSRecoveryDirector toma control.
- La transformación del transporte cambia Y: soporte solo horizontal hace que el personaje caiga o se atraviese durante ascenso.
- Abrir inspección en aire, en pared o sobre un borde no es válido. Todos los objetos tienen suelo estable asignado.
- Compartir instancias de materiales y cambiar emisión globalmente puede encender todos los portales/hallazgos a la vez. Usar parámetros por renderer o instancias controladas.
- Pausa e inspección deben resolver Esc en un solo contexto; no cancelar objeto y abrir pausa en la misma pulsación.

## 19. Orden de construcción

### 19.1. Fase A — Plano funcional y geometría gris

Entregables: raíces de ocho zonas, cinco apoyos de 06, ramal Yopo 2, escaleras, pared, muelles y marcadores. Usar cubos con medidas del documento; colocar texto temporal de ID para el equipo, ocultable al jugar.

Criterio de cierre: todas las superficies principales existen, alturas y conexiones coinciden con tablas, y ningún pilar invade el suelo útil. Los números del boceto no sustituyen geometría física.

### 19.2. Fase B — Progreso y hallazgos

Implementar MSProgressStore, MSFind y objetivos. Probar confirmar/cancelar/recargar cada uno de los seis hallazgos. El jugador puede activar Runa 1 y Runa 2 sin la habilidad que entregan. Los yopos no afectan bloqueos de navegación.

Criterio: flags correctos e idempotentes, sin escribir MIProgress, y pedestal vacío al recargar un hallazgo obtenido.

### 19.3. Fase C — Portales y escalada

Implementar parejas, retornos, viaje de escena y carril. Recorrer 01→02→03→02→04 solo con esas habilidades, usando teclas y sin cámara/micrófono.

Criterio: no rebotes de portal, no enganche sin runa, salida superior segura y descenso de regreso viable.

### 19.4. Fase D — Vuelo y apoyos

Implementar perfil local, presupuesto, recarga y recuperación. Ensayar 04→05, ramal Yopo 2 y P4→A→B→C→Llave. Medir tiempos de vuelo y margen de aterrizaje con la cápsula real.

Criterio: ningún tramo exige precisión de un solo frame; cerrar/reabrir no repone carga; plataformas intermedias no se pueden omitir con el perfil previsto. Ajustar coordenadas antes del arte.

### 19.5. Fase E — Transporte, llave y antesala

Implementar delta de soporte, dos extremos, pausa, cierre y checkpoint 07. Validar ida y vuelta tanto con llave como sin ella.

Criterio: personaje acompaña ascenso, puede desembarcar caminando, no queda encerrado y la llave persiste tras caída.

### 19.6. Fase F — Jefe y salida

Implementar modelo de turnos, presenter, derrota, victoria y retorno superior. Usar rig/silueta provisionales primero; comprobar combate sin yopos y con ambos.

Criterio: daño por acción única, ventanas visibles, reintento en 07 y salida solo tras victoria.

### 19.7. Fase G — Arte, audio y pulido

Sustituir geometría gris por kit, revisar pivotes/colliders después de cada sustitución y añadir nubes/vegetación al final. Integrar UI y audio por eventos reales. Conservar una versión de graybox para comparar distancias y depurar.

Criterio: el arte no tapa destinos ni cambia de forma inadvertida el collider. Los materiales y luces mantienen piso, hueco y hallazgo distinguibles.

### 19.8. Entrega de cada pieza

Para cada ID del catálogo entregar modelo importable, prefab, material o asignación compartida, pivote, collider si corresponde, variantes necesarias y captura de comprobación de escala. Para elementos móviles, incluir clip/control y límites. Para un hallazgo, incluir ficha, icono, ID y premio.

Para la escena entregar referencias conectadas, ruta en Build Settings, modo de prueba desde editor, estados de guardado comprobados y lista de problemas pendientes. No declarar el mundo listo solo porque la lámina o los modelos existan.

## 20. Validación y aceptación

### 20.1. Matriz mínima de pruebas jugables futuras

| Caso | Preparación | Resultado esperado |
|---|---|---|
| Visita nueva | Flags vacíos, sin poderes inferiores | Recorrido completo hasta victoria |
| Runa 1 cancelada | Esc en inspección | Portal sigue apagado; objeto sigue presente |
| Runa 2 cancelada | Llegada por P1 | Pared no engancha |
| Portal inverso | Usar P2 después de Runa 2 | Vuelta segura a 02; no loop |
| Pared sin runa | Intentar E en base | Mensaje; no subir |
| Salida y descenso de pared | Runa 2 obtenida | Llegar a 04 y regresar por carril |
| Alas canceladas | Esc antes de confirmar | No activar F; objeto permanece |
| Cerrar/reabrir alas | En un mismo despegue | Usar saldo restante, sin recarga |
| Aterrizaje lateral falso | Rozar muro/pretil | Sin recarga ni marker seguro nuevo |
| Ramal opcional | Volar desde 04 | Obtener Yopo 2 y regresar con carga base |
| Ruta sin yopos | Omitir ambos | Obtener llave y derrotar jefe |
| Portal azul | P3 activo y Alas | P4 aparece al principio de 06 |
| Saltos de apoyo | Intentar P4→B o A→C | No omitir secuencia con vuelo base |
| Regreso por 06 | Alas completas | Cada vuelo inverso viable |
| Caída tras llave | ms_llave registrado | Llave conservada; no nuevo premio |
| Transporte ascendente | Personaje apoyado | Acompaña delta XYZ sin deslizarse |
| Transporte en pausa | Pausar a mitad | Fase y soporte congelados; continuidad al volver |
| Antesala sin llave | Viajar sin recogerla | Cierre cerrado; transporte permite regresar |
| Apertura y recarga | Guardar tras usar O10 | Puerta abierta y medallón insertado |
| Combate sin yopos | Daño 20 | 12 ataques válidos para 240 de vida |
| Combate con un Yopo | Daño 25 | 10 ataques válidos |
| Combate con ambos | Daño 30 | 8 ataques válidos |
| Defensa incorrecta | Respuesta opuesta | 20 daño una sola vez |
| Derrota | Vida llega a 0 | Recuperar en 07; jefe reiniciado |
| Victoria y recarga | ms_jefe_vencido | Jefe inactivo y salida activa |
| Regreso a plaza | Portal de cima | Volver frente al portal superior con progreso |
| Entrada desde plaza | Progreso avanzado | Llegada en 01; capacidades conservadas |
| Carga de ranura | D04 o D07 guardado | Aparición en descanso válido |
| Poderes inferiores | Doble salto y dash adquiridos | Sin borrarlos; perfil local respeta bloqueos |
| Sin dispositivos | Sin webcam/micrófono | Completar interacción y combate con teclado/mouse |

Estas pruebas se ejecutarán durante implementación. La revisión de esta entrega comprueba el documento y su inventario; no certifica que las mecánicas estén funcionando en Unity.

### 20.2. Revisión de piezas y escena

Verificar 41 referencias de catálogo y reparto de 451 instancias iniciales; distinguir las dos representaciones derivadas de alas/llave. Comprobar seis portales físicos, seis hallazgos únicos, tres descansos, dos muelles, una plataforma, un cierre y un jefe.

Revisar cada modelo: escala, pivote, UV, material, reverso si se inspecciona y collider sin invadir huecos. Cada terraza de 12 debe tener cuatro T01; arena dieciséis; Inicio/A/B/C de 06 una cada uno; Llave cuatro.

### 20.3. Lectura visual y cámara

Probar 720p, 1080p, 1440p y 16:10 con texto estándar/grande y contraste alto. Mostrar destino antes de despegar; no tapar piso con nubes. Probar portal y escalada con movimiento reducido, audio silenciado y ayudas de reacción ampliadas.

Desde la marca de combate se ve núcleo y aviso. La salida se reconoce después de victoria. Al cerrar inspección se restaura la cámara previa, no la de una sala distinta.

### 20.4. Rendimiento y estabilidad

Medir con Profiler en el equipo objetivo cuando exista la escena. Presupuesto objetivo inicial: 60 fps en configuración de referencia definida por el proyecto; no inventar resultado ni hardware mínimo. Revisar transparencias de nubes, luces activas, materiales instanciados, reserva de efectos y geometría fuera de vista.

No actualizar todos los portales, hallazgos y cámaras con búsquedas globales cada frame. Resolver referencias en inicialización y eventos de estado. La lista de 451 instancias es un presupuesto de montaje, no una prueba de rendimiento.

### 20.5. Condición de nivel terminado

El mundo está listo para entrega jugable cuando una partida nueva completa toda la ruta sin atajos accidentales ni bloqueos; una partida cargada reconstruye premios y accesos; todas las caídas tienen destino; el transporte conserva soporte; el jefe se puede reintentar y la victoria vuelve correctamente a plaza. El arte y audio deben apoyar esas condiciones.

## 21. Fuentes, archivos y límites

### 21.1. Referencias del diseño

- [Boceto y especificación inicial](Mundo-superior.md).
- [Lámina seleccionada](Mundo-superior/Boceto-ascenso-v1.png).
- [Inventario de piezas en JSON](Mundo-superior/Inventario-piezas.json).
- [Prompt inicial](Mundo-superior/Prompt-boceto.txt) y [ajustes de la lámina](Mundo-superior/Prompts-ajustes.txt).
- [Guía completa del mundo inferior](Guia-completa-mundo-inferior.md), como referencia de estructura documental.
- [PRODUCT.md](../../PRODUCT.md), restricciones y dirección del producto.

### 21.2. Referencias culturales

- [Banco de la República — Muisca](https://enciclopedia.banrepcultural.org/index.php?title=Muisca): contexto de orfebrería, ofrendas y prácticas rituales.
- [Museo del Oro — Ofrendatario con tapa](https://colecciones.banrepcultural.org/en/document/ofrendatario-con-tapa/63a069045d96b8790f28278e): ejemplo de recipiente cerámico con un conjunto de figuras de metal.
- [Historias de ofrendas muiscas](https://babel.banrepcultural.org/digital/collection/p17054coll18/id/400/): investigación de objetos y contextos de ofrenda.

Son las referencias de la propuesta inicial. El documento desarrolla un mundo ficticio; no agrega una reconstrucción arqueológica ni valida por analogía cada diseño de pieza. Arquitectura circular, recipientes de yopo, vestuario y ornamentación atribuida necesitan fichas específicas antes de convertirse en arte culturalmente identificado.

### 21.3. Evidencia técnica revisada

Rutas del repositorio revisadas para distinguir reutilización de desarrollo pendiente:

- Assets/_Game/Scripts/Controller/PlayerController.cs.
- Assets/_Game/Scripts/Model/PlayerAbilityModel.cs.
- Assets/_Game/Scripts/Controller/PlayerRespawn.cs.
- Assets/_Game/Scripts/Controller/PlazaNunez/ExplorationOrbitCamera.cs.
- Assets/_Game/Scripts/Controller/PlazaNunez/WorldTravel.cs.
- Assets/_Game/Scripts/Model/PlazaNunez/PlazaCombatModel.cs.
- Assets/_Game/Scripts/Controller/PlazaNunez/PlazaCombatController.cs.
- Assets/_Game/Scripts/Controller/MundoInferior/MIProgress.cs.
- Assets/_Game/Scripts/Controller/MundoInferior/MIFind.cs.
- Assets/_Game/Scripts/Controller/MundoInferior/MIInteractable.cs.
- Assets/_Game/Scripts/Controller/MundoInferior/MIPortal.cs.
- Assets/_Game/Scripts/Controller/MundoInferior/MundoInferiorBlockout.cs.

### 21.4. Qué falta verificar jugando

Las cifras de movimiento y coordenadas son una base específica para graybox. Falta medir alcance real, márgenes de vuelo, colisión de escalada, fase del transporte, vista de cámara y ritmo del jefe. Si cambia un tamaño o una regla, actualizar ruta, tabla de montaje e inventario conjuntamente.

La imagen contiene perspectiva artística. Algunas plataformas se dibujan más cercanas o a alturas comprimidas; los muelles y trayectos usan las coordenadas de esta guía. Las seis zonas de premio, ocho salas y mecanismos son decisiones claras; sus resultados jugables todavía requieren implementación y pruebas.

