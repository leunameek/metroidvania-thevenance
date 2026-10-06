# Guía completa del mundo inferior

El asedio de Bacatá

Guía de montaje de objetos interacciones y recorrido del nivel

30 de septiembre de 2026

## Propósito de la guía

El mundo inferior se construye como una caverna de piedra azulada y raíces antiguas, dividida en nueve zonas conectadas. El jugador entra arriba a la derecha, obtiene doble salto, encuentra tres mejoras de impulso, supera combate y trampas, recoge el cuerno, abre la antesala y derrota al guardián del fondo. Los retornos permiten recuperar el camino y volver a Plaza Núñez sin perder los hallazgos.

Esta guía sirve al diseñador del nivel, al artista que modela las piezas y a quien integra las mecánicas en Unity. Explica dónde colocar los objetos, cómo ensamblarlos, qué debe ver y hacer el jugador, qué desbloquea cada acción y cómo comprobar que el recorrido funciona. Las instrucciones de montaje definen el nivel que se va a construir.

![Mapa conceptual del mundo inferior](../../ArtSource/Worlds/MundoInferior/01-estructura-nivel.png)

El mapa comunica la distribución en tres alturas. Los números identifican zonas, las flechas doradas indican el recorrido principal y las líneas turquesa muestran conexiones opcionales. Las siluetas blancas sirven de referencia de escala; no son personajes adicionales que deban colocarse. Las flechas y números pertenecen a la documentación y no necesitan aparecer como objetos dentro del juego.

## 1 Cómo leer y montar este documento

### 1.1 Decisiones que se conservan

Se conserva la organización del chat Diseña el mundo inferior y de la especificación Mundo inferior: 01 Umbral, 02 Santuario, 03 Brazaletes, 04 Centinelas, 05 Péndulos, 06 Derrumbe, 07 Cuerno, 08 Antesala y 09 Guardián. Se conserva también el kit de 33 tipos base y sus 58 referencias individuales vigentes. Las mejoras anteceden a los obstáculos que las necesitan; el cuerno abre la reja de 08 y la victoria permite usar la salida de 09.

Los personajes de raíces, el cuerno y el guardián son ficción del juego. Su forma y sus funciones no constituyen afirmaciones históricas. El mundo mantiene piedra, hueso, metal gastado y raíces, con motivos ornamentales ficticios. La interfaz continúa el lenguaje de piedra tallada, pergamino y oro envejecido ya definido para el producto.

### 1.2 Medidas y coordenadas de montaje

Las dimensiones y coordenadas de esta guía son una propuesta inicial de bloqueo en Unity. No son posiciones extraídas de una escena terminada. Un metro equivale a una unidad de Unity. El personaje de referencia mide aproximadamente 1,75 m; la cápsula real del controlador tiene prioridad al comprobar huecos y aterrizajes.

Cada sala tiene su propio origen. El punto (0; 0; 0) está en el centro del borde de entrada y en la superficie del suelo principal. X positivo va a la derecha del plano local; Z positivo entra en la sala; Y positivo sube. Una coordenada escrita (X; Y; Z) identifica el pivote indicado para una pieza individual o la raíz de un ensamblaje. En A01 con A02, la raíz está en el centro del paso: la hoja tiene su bisagra como hijo desplazado aproximadamente 1,1 m hacia la izquierda, no otro pivote central. En O04 con soporte, la raíz está en la base del pedestal y el soporte ocupa el socket superior. Los diagramas locales se leen desde la entrada hacia el fondo. Al orientar la sala en el mundo, se gira su raíz completa para ajustarla al recorrido del mapa.

Las posiciones de Spawn y de personajes indican su punto de contacto con el suelo. Se convierten a posición de raíz según el pivote del modelo y la cápsula. En la cápsula actual de la plaza, de altura 2 m y centro local 0, colocar la raíz aproximadamente 1 m encima del marcador de pies; Teleport copia la posición y no realiza esa compensación automáticamente. En un rig con pivote a los pies y centro de cápsula elevado, se utiliza el marcador directamente tras verificar sus ajustes.

Las terrazas son superiores, medias e inferiores. Como referencia inicial, 01 está a altura global 0; 02 a -2; 03 a +2; 04 y 05 a -6; 06 a -10; 07, 08 y 09 a -14. Las alturas se ajustan cuando se midan los saltos. Estos valores no autorizan saltos verticales directos de 4 u 8 m: las conexiones usan rampas, peldaños y plataformas intermedias.

Estas cotas exigen reemplazar o desactivar la recuperación heredada de PlayerRespawn, cuyo umbral global inicial es -10, y la de TechnicalDemoController, inicialmente -8. Dejar esos gestores activos haría reaparecer al jugador mientras camina por las terrazas inferiores. El mundo inferior usa una sola autoridad de recuperación, preferiblemente volúmenes de caída por pozo; cualquier límite global de respaldo debe quedar por debajo del pozo más bajo, con margen suficiente. No cambiar las cotas sin revisar también ese límite.

Se conecta el socket Exit de una sala al socket Entry de la siguiente con geometría jugable real. El suelo debe ser continuo salvo los huecos indicados. No se unen salas solamente por una flecha ni se deja una caída como única conexión cuando luego se exige regresar. Las pendientes y los tramos intermedios se ajustan al controlador.

### 1.3 Qué representa cada posición

Las tablas de cada sala identifican las instancias funcionales que deben existir: objetos únicos, actores, puertas, trampas, apoyos y puntos seguros. Las cantidades decorativas son un mínimo inicial, ampliable sin invadir las superficies transitables. Una terraza de 12 por 10 m se compone con T02 y extensiones; no supone que exista una malla nueva de ese tamaño.

Un objeto coleccionable se coloca en el anclaje superior del pedestal, no en Y igual a cero. O04a tiene 0,9 m de alto: su base queda en el suelo y el anclaje del objeto queda aproximadamente a 1,05 m, ajustado al soporte. O01 representa un par de brazaletes: las mallas izquierda y derecha forman una sola recompensa. Tres hallazgos de impulso requieren tres conjuntos de ese par, no seis premios distintos.

Las coordenadas de salas con plataformas identifican centros de apoyo. Antes de fijarlas, medir el alcance real de salto y de impulso con el controlador del mundo inferior. Cambiar la separación de los apoyos cuando sea necesario, conservando el orden de aprendizaje y los descansos. La cámara y las colisiones se verifican con el personaje real, no solo mirando la ilustración.

### 1.4 Orientación y ensamblajes de los objetos

Dentro de una sala sin girar, entrada y salida están sobre el eje Z. Una puerta cruza el carril con su hoja en el plano XY; el personaje avanza por Z a través del hueco. Los altares orientan su frente hacia el carril cercano. Enemigos y custodio miran hacia la entrada al empezar, aproximadamente una rotación de 180 grados en Y si el frente del modelo apunta a Z positivo. Ajustar esa rotación si el artista exportó otra dirección frontal.

| Ensamblaje | Raíz y piezas fijas | Pieza móvil y anclaje |
|---|---|---|
| Altar semilla | O04a en suelo y O04d en socket superior | O03 en anclaje cercano a Y 1,05; giro propio al inspeccionar |
| Altar brazaletes | O04a en suelo y O04b encima | O01a y O01b hijos del mismo hallazgo; un solo trigger y premio |
| Altar cuerno | O04a en suelo y O04c encima | O02 apoyado en curva de soporte; eje de inspección propio |
| Descanso | O04e v2 sobre suelo sólido | VFX aparte; no desplazar disco al guardar |
| Arco y reja | A01 centrado en paso; marco A02 fijo | Bisagra aproximadamente X -1,1 en raíz de puerta; hoja gira 95 grados hacia nicho |
| Portal | A04 marco fijo de 2,6 por 3,4 m | Plano VFX y volumen de interacción independientes del marco |
| Rejilla | T03 y aro A03 centrados en mismo hueco | Tapa con eje tangente horizontal; primera versión cerrada |
| Péndulo | Soporte en techo y eje paralelo a Z local | Suspensión y peso bajo el eje; rotación alrededor de Z produce barrido en X |
| Losa | Apoyos fijos y trigger sobre cara superior | H03a sustituida por estado H03b y fragmentos H03c al romper |
| Piedra que cae | Anclaje fijo en techo | H04 desprendible y collider de daño móvil; sombra en punto anunciado |
| Enemigo con accesorios | Rig del cuerpo y collider según punto de pies | Arco, bastón, escudo o núcleo unidos al socket correspondiente del rig |

Después de girar una sala completa, sus componentes conservan estas orientaciones locales. Las coordenadas de bisagra, soporte y núcleo son offsets de ensamblaje, no posiciones globales nuevas. Aplicar escala antes de configurar pivotes; revisar colliders después de cambiar la escala visual de un prefab.

## 2 Recorrido y condiciones de avance

### 2.1 Ruta principal

Plaza Núñez → 01 Umbral → 02 Semilla → 03 Brazaletes nivel 1 → 04 Brazaletes niveles 2 y 3 y guardián de escudo → 05 Péndulos → 06 Derrumbe → 07 Cuerno → 08 Activación de la reja → 09 Guardián → portal de regreso a la plaza.

La secuencia enseña orientación, salto, doble salto, impulso, cadenas de impulso, lectura de combate, sincronización de obstáculos y aplicación combinada de habilidades. El nivel no exige una webcam ni un micrófono. La interacción principal debe poder completarse con teclado y mouse.

| Zona | Entrada y tarea principal | Condición para avanzar | Resultado persistente |
|---|---|---|---|
| 01 | Llegar y localizar el santuario | Seguir el suelo seguro hasta 02 | Zona descubierta |
| 02 | Examinar y confirmar la semilla | Doble salto obtenido y ensayo superado | Semilla y habilidad |
| 03 | Alcanzar el altar y recoger los brazaletes | Impulso nivel 1 obtenido | Hallazgo 1 y atajo 03 a 01 |
| 04 | Recoger dos mejoras y vencer al escudo | Cadena de tres disponible y escudo derrotado | Hallazgos 2 y 3 y salida abierta |
| 05 | Cruzar dos péndulos con descanso | Alcanzar la plataforma de salida | Zona descubierta |
| 06 | Cruzar derrumbe y caída de piedras | Activar retorno seguro al final | Atajo de 06 abierto |
| 07 | Superar prueba final y recoger cuerno | Confirmar el objeto | Cuerno obtenido |
| 08 | Descansar y activar soporte del cuerno | Cuerno presente y reja activada | Reja del jefe abierta |
| 09 | Evitar ataques y dañar núcleo expuesto | Vida del jefe llega a cero | Guardián derrotado y salida activa |

### 2.2 Conexiones y retornos

| Conexión | Cuándo funciona | Montaje y efecto |
|---|---|---|
| Plaza y 01 | Tras cumplir el tutorial de plaza; retorno libre | Portal de llegada y punto de regreso sobre suelo estable |
| 01 y 02 | Desde el inicio | Camino ancho con rampas; sin reja de progreso |
| 02 y 03 | Ascenso principal tras semilla | Prueba de doble salto; retorno mediante apoyos y camino seguro |
| 03 y 01 | Palanca accesible desde 03 | Corredor alto continuo; al abrirse funciona en ambos sentidos |
| 03 y 04 | Tras brazaletes 1 | Bajada escalonada con regreso; evitar un salto ciego |
| 04 y 05 | Escudo derrotado | Reja de combate o barrera asociada abre permanentemente |
| 05 y 06 | Desde que se alcanza el tramo | Rampa desde plataforma final; el paso de péndulos admite regreso |
| 06 y 07 | Después del derrumbe | Descenso y ruta paralela de vuelta bajo raíces |
| 06 y 08 | Al abrir palanca al final de 06 | Corredor lateral seguro que evita repetir derrumbe |
| 07 y 08 | Tras recoger cuerno | Apoyos inferiores y suelo continuo al final; retorno libre |
| 08 y 09 | Cuerno activado | Reja de 08 abre; cierre de arena independiente durante combate |
| 09 y plaza | Tras victoria | Portal de regreso activo y visible desde el centro de arena |

El corredor de 06 llega a un lado de 08, antes de la reja. Si el jugador lo recorre sin cuerno, puede descansar, ver el bloqueo y regresar a 07. No se introduce una segunda llave ni una palanca oculta para el acceso al jefe. El cuerno nunca se consume al abrir la puerta.

### 2.3 Bloqueos que deben entenderse

El doble salto resuelve una diferencia de altura o una separación claramente imposible con salto simple. El impulso resuelve un paso horizontal de práctica antes del combate. El escudo presenta una defensa visible que requiere la tercera cadena. La reja de 08 tiene un soporte de cuerno visible junto a ella. El portal final muestra que su energía aumenta al terminar el combate.

Una mejora nunca se sitúa detrás de su propio bloqueo. La semilla se alcanza caminando o con salto simple. El primer par de brazaletes se alcanza con doble salto, sin necesitar impulso. Los hallazgos 2 y 3 se alcanzan con las capacidades obtenidas anteriormente y antes de activar al escudo. El cuerno se alcanza sin tener el cuerno.

## 3 Controles e interacción del jugador

### 3.1 Perfil de controles para este nivel

Se propone conservar el perfil de exploración de Plaza Núñez para el mundo inferior: WASD para moverse respecto a la cámara, Espacio para saltar, Shift izquierdo sostenido para correr, Q para impulso y E para interactuar. Al pulsar Espacio de nuevo en el aire, se usa el segundo salto si la semilla ya se obtuvo. Cada pulsación de Q genera un impulso; los niveles 2 y 3 permiten encadenar más pulsaciones en una misma secuencia.

| Acción | Control principal propuesto | Qué debe suceder |
|---|---|---|
| Moverse | W A S D | Desplazamiento horizontal según orientación de cámara |
| Correr | Shift izquierdo sostenido | Velocidad de carrera; no concede la habilidad de impulso |
| Salto y segundo salto | Espacio y otra pulsación en el aire | Segundo salto solo después de confirmar semilla |
| Impulso | Q | Desplazamiento en dirección elegida o frontal si no hay entrada |
| Cadena de impulso | Q durante la secuencia activa | Hasta el nivel obtenido; tercera cadena rompe escudo |
| Examinar o activar | E cerca del objeto | Abre inspección o activa palanca, descanso, soporte o portal |
| Confirmar hallazgo | E dentro de inspección | Concede recompensa una sola vez y vuelve a exploración |
| Cancelar inspección | Escape | Restaura objeto y control sin recogerlo |
| Rotar objeto | Mouse en inspección | Rotación del modelo; alternativa disponible sin cámara |
| Usar manos | Cámara opcional configurada | Mano derecha abierta controla un eje, izquierda el otro; puño congela el eje |
| Pausa | Escape durante exploración | Detiene juego y devuelve el foco al reanudar |
| Mapa y ayuda | Tab abre mapa y H abre Controles | Muestra información del nivel sin inventar recursos nuevos |

La escena Movement usa otro perfil: Shift izquierdo para impulso, sin el comportamiento de carrera de la plaza. Su configuración tampoco coincide con la plaza. Al integrar el mundo inferior se debe elegir un perfil único y hacer que todos los prompts reflejen ese perfil. Esta guía utiliza Q; no mostrar Shift como impulso mientras Shift está asignado a correr.

La cadena mantiene la dirección del primer impulso; no se gira cada eslabón con una nueva dirección. Durante un impulso activo, el controlador ignora Espacio. Para combinar salto e impulso, iniciar primero el salto y después impulsar, o esperar a que termine el impulso antes de volver a saltar. El tutorial debe enseñar el comportamiento real del perfil que se integre.

### 3.2 Secuencia completa de un hallazgo

1. El jugador se aproxima al pedestal desde suelo firme. Aparece E Examinar semilla, E Examinar brazaletes o E Examinar cuerno, según el objeto.
2. Al pulsar E, el sistema reserva esa interacción. Bloquea movimiento, impulso y acciones del mundo que puedan interferir con la inspección.
3. La cámara encuadra el modelo. El controlador normal de cámara queda suspendido durante la inspección.
4. El jugador rota el objeto con mouse o manos. Los objetos del mundo inferior no añaden las tres lecciones obligatorias de la plaza: inspeccionar y confirmar es suficiente para recogerlos.
5. E confirma. El sistema comprueba el ID del hallazgo, registra la recompensa y actualiza la capacidad o el inventario en una única operación.
6. El modelo recogido desaparece del pedestal; el pedestal y su soporte permanecen. Se reduce su luz para indicar que ya se utilizó.
7. Se muestra un aviso breve y el objetivo siguiente. El control y la cámara vuelven a exploración; el jugador queda sobre el mismo suelo seguro.
8. Escape antes de confirmar devuelve el objeto a su posición y orientación inicial. No concede habilidad, no guarda adquisición y no borra un hallazgo ya confirmado.

La inspección del prototipo mueve temporalmente el mismo modelo. Por ello, el objeto recogible debe ser hijo independiente del pedestal. Nunca asignar como objeto de inspección toda la terraza, el soporte, el enemigo o un conjunto de colliders del nivel.

### 3.3 Progresión de impulso sin duplicar premios

En una partida nueva del mundo inferior, la progresión propia comienza sin semilla y en nivel 0 de impulso. El impulso que la plaza presta para enseñar sus controles se trata como capacidad del entrenamiento. Al entrar al mundo inferior se aplica el estado persistente del nivel; al volver no se borran capacidades permanentes ya obtenidas. Esta separación es una regla de integración propuesta y requiere distinguir entrenamiento de progreso real.

Los tres hallazgos tienen IDs diferentes y nivel objetivo 1, 2 y 3. Confirmar el hallazgo 1 garantiza al menos nivel 1; confirmar el 2 garantiza al menos nivel 2; confirmar el 3 garantiza al menos nivel 3. Volver a un pedestal recogido no suma otro nivel. Una partida avanzada conserva su nivel al revisitar salas.

El código actual incrementa el nivel al conceder una mejora y no impone un máximo de tres. Reutilizarlo sin control de IDs podría producir nivel 4 si se hereda el nivel prestado en la plaza, o permitir duplicados al cargar. Antes de integrar los premios se requiere recompensa idempotente y restauración explícita de estado. El diseño usa un máximo lógico de tres cadenas, no afirma que el modelo actual ya lo limite.

### 3.4 Cómo se pelea durante la exploración

Los enemigos de exploración se atacan con el impacto del impulso mediante DashHurtbox. La guía no presupone un ataque normal con espada ni un botón de ataque que todavía no exista. El jugador aprende a provocar el ataque enemigo, apartarse y utilizar el impulso durante la recuperación.

El duelo de práctica de Plaza Núñez y el jefe de la escena Movement son sistemas diferentes. Sus botones o comandos de voz no se convierten automáticamente en controles del guardián de este nivel. Para 09 se propone combate físico en tiempo real, con impulso contra el núcleo expuesto. Si se decide otro sistema más adelante, deberá cambiarse la guía de combate y la interfaz correspondiente.

## 4 Distribución detallada por zona

### Zona 01 Umbral

**Lugar y función.** Terraza superior derecha del mapa. Primera sala de aproximadamente 12 m de ancho por 10 m de fondo. Presenta la caverna, el árbol central y la dirección del santuario. Todo su suelo principal es estable; no contiene enemigos ni trampas activas.

**Montaje.** Construir el suelo T02 desde X -6 a +6 y Z 0 a 10. Apoyarlo visualmente con T05 y pared T06 posterior. Colocar el marco completo A04 sobre el borde de entrada con su efecto y trigger dentro. El arco de la imagen se interpreta como referencia conceptual: A04 tiene hueco 2,6 por 3,4 m y no cabe dentro del arco A01 de hueco 2,4 por 3,2 m. Usar A04 completo, sin superponer los dos marcos. Dejar un área libre de al menos 3 por 3 m al salir del portal. Los bordes laterales usan A06 donde una caída no sea parte de la navegación. El acceso a 02 queda al fondo; la conexión de 03 entra por un lateral y permanece cerrada hasta abrirla desde 03.

| Instancia | Posición local inicial | Orientación y montaje | Uso |
|---|---|---|---|
| A04 portal de llegada y retorno | (0; 0; 0,7) | Marco propio, efecto y trigger separados | E permite volver a plaza |
| Spawn 01 | (0; 0; 3) | Mira hacia Z positivo | Llegada sobre suelo firme |
| Salida a 02 | (0; 0; 9,5) | Rampa con apoyos hacia santuario | Ruta principal siempre abierta |
| A02 puerta del atajo | (-5; 0; 6) | Abre fuera del espacio de llegada | Cerrada hasta palanca de 03 |
| Dos N05 piedras de luz | (-3; 0; 2) y (3; 0; 7) | Luz tenue en bordes | Orientación sin parecer premio |
| T08 rocas y N02 raíces | Laterales y fondo | Separación mínima de 1 m del carril | Ambientación |

**Qué hace el jugador.** Sale del arco, identifica el santuario por su luz cálida y sigue el camino al fondo. Puede girarse y usar E para retornar a la plaza. No necesita examinar el portal ni resolver un puzle en esta sala. El aviso inicial es Encuentra el santuario de raíces.

**Interacciones y regreso.** El portal informa su destino antes de viajar. La puerta lateral se ve como un acceso futuro, sin exigir al jugador buscar una palanca aquí. Después de activar el atajo en 03, esta misma puerta queda abierta y permite regresar en ambos sentidos. Volver desde la plaza restaura hallazgos y atajos, y coloca al jugador en Spawn 01, fuera del trigger del portal.

**Comprobación.** Llegar y soltar todos los controles no debe causar caída. La cámara muestra piso, salida y un hito del mundo. A04 no teletransporta repetidamente al entrar; requiere E o una protección equivalente contra rebote entre destinos.

### Zona 02 Santuario de raíces

**Lugar y función.** Isla ancha del centro superior. Aproximadamente 14 por 12 m. Contiene semilla, custodio opcional y primer descanso. El ensayo del doble salto ocurre después de recoger la semilla y sobre un espacio de recuperación sin pinchos.

**Montaje.** Crear una terraza principal T02 en los primeros 7 m de fondo, un altar lateral que se ve desde la entrada y apoyos de prueba hacia el fondo. El altar se alcanza por un pasillo de al menos 2 m. El descenso de 01 a 02 usa T04, sin saltos obligatorios. La cámara debe mostrar el primer apoyo alto y su aterrizaje.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| Altar O04a y O04d | (-3; 0; 4) | Base fija; soporte de semilla encima | Hallazgo obligatorio |
| O03 semilla | (-3; 1,05; 4) | Centro separado y pequeño halo cálido | Concede doble salto al confirmar |
| C04a custodio y C04b bastón | (3; 0; 4) | Mira a entrada; bastón en mano | Orientación opcional |
| Descanso O04e v2 | (3; 0; 1,8) | Disco sólido horizontal sobre suelo | Cura y fija checkpoint 02 |
| Apoyo de salida T01 | (0; 1,8; 8) | Cara superior plana | Ensayo de segundo salto |
| Plataforma de llegada | (0; 2,6; 11) | Ancha y sin enemigos | Conduce a 03 |
| Suelo de recuperación | Bajo ensayo, Y -1 | Terreno firme y acceso de vuelta | Recupera intentos fallidos |
| N04 y N05 | Alrededor del altar, fuera del paso | Vegetación de 0,8 m y luz tenue | Diferencia el santuario |

**Qué hace el jugador.** Primero puede descansar o hablar con el custodio. Después se acerca al altar, pulsa E, rota la semilla y confirma con E. Aparece Doble salto obtenido y la indicación Pulsa Espacio de nuevo en el aire. Se prueba el apoyo alto y luego la llegada hacia 03. El ensayo se ajusta para exigir el segundo salto con el perfil real; 1,8 y 2,6 m son alturas iniciales de bloqueo.

**Diálogo propuesto.** El custodio puede decir La semilla te permite dar un segundo salto antes de tocar suelo. Mirar al altar mientras habla ayuda a ubicarlo. No entrega una misión paralela, no exige conversación para recoger la semilla y no repite instrucciones mientras se inspecciona.

**Estado y fallo.** Cancelar inspección conserva semilla en altar. Confirmarla la retira y registra la habilidad. Una caída en el ensayo devuelve a un suelo cercano estable; no borra la semilla. Si el jugador muere más adelante, el checkpoint 02 permite continuar con los premios obtenidos. El descanso está separado del altar: recoger semilla y guardar no son la misma interacción.

### Zona 03 Galería de brazaletes

**Lugar y función.** Terraza superior izquierda del mapa, conectada por el ascenso desde 02. Sala inicial de 12 por 18 m. Contiene el primer par de brazaletes, un ensayo de impulso, un centinela y la palanca que abre el retorno alto hacia 01.

**Montaje.** Los primeros 8 m son seguros y conducen al altar. El ensayo se sitúa detrás del altar; el centinela espera al final, sobre una plataforma ancha. Una salida lateral permite abrir el atajo antes del combate. El camino hacia 04 desciende con escalones visibles y recuperables.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| Altar O04a y O04b | (-3; 0; 5) | Frente hacia centro de sala | Hallazgo de impulso 1 |
| Par O01a y O01b | Anclaje (-3; 1,05; 5) | Separación aproximada 0,2 m entre piezas | Una recompensa nivel 1 |
| H05 palanca | (-4; 0; 8) | Mango a altura alcanzable; E visible | Abre atajo 03 a 01 |
| A02 y A05 acceso de atajo | (-5; 0; 9) | Hoja gira a un nicho libre | Corredor hacia 01 |
| Apoyo de lanzamiento | (0; 0; 9) | Suelo ancho con margen | Ensayo de impulso |
| Plataforma de recepción | (0; 0; 13) | Inicial 4 por 4 m; sin enemigo en borde | Aprender alcance del impulso |
| C01 centinela | (0; 0; 16) | Patrulla acotada al fondo | Primer enemigo con nueva habilidad |
| Salida a 04 | (0; 0; 17,5) | Bajada con descansos de suelo | Continuación principal |

**Qué hace el jugador.** Al alcanzar el altar, confirma los brazaletes y recibe Impulso nivel 1 obtenido. La instrucción indica Q y la dirección del movimiento. Puede activar la palanca, ver abrirse la puerta y recorrer el atajo para comprobar que regresa a 01. Vuelve a 03, ensaya un impulso sobre la separación de práctica y se aproxima al centinela. Espera su preparación, esquiva y usa el impulso para golpearlo.

La separación del ensayo debe permitir aterrizar con margen aun si el impulso comienza un poco tarde. Si el salto simple o la carrera permiten saltarlo sin usar impulso, ajustar distancia y altura sin retirar la plataforma segura. Para exigir impulso durante aprendizaje se necesita un bloqueo validado, no confiar en la apariencia de un pozo.

**Regreso.** El centinela no persigue al jugador hasta el altar, la palanca o 01. El atajo permanece abierto al morir, recargar y volver desde la plaza. Las dos hojas de sus extremos, en 01 y 03, leen el mismo flag de apertura y se abren con la palanca de 03. No se vuelve a cobrar una recompensa de brazaletes en esta sala. El pasaje alto se representa con raíces y piedra, con apoyos continuos; la línea turquesa del mapa no es un salto directo entre extremos.

### Zona 04 Patio de centinelas

**Lugar y función.** Gran terraza media izquierda. Los dos primeros módulos tienen aproximadamente 12 por 10 m; el módulo final se ensancha a 18 m y ocupa unos 22 m de fondo. La profundidad total inicial queda en torno a 44 m. Enseña cadenas de impulso, introduce al arquero y termina con el guardián de escudo. Las mejoras 2 y 3 quedan en descansos intermedios.

**Montaje.** Cada módulo tiene entrada segura, espacio de combate y salida visible. Dos pilares de piedra actúan como hitos y cobertura del arquero. El arquero no dispara durante la inspección de un premio ni hacia la siguiente sala. La arena del escudo queda al final, con un descanso anterior donde se recoge nivel 3. Su reja de salida se coloca lejos de la trayectoria de la carga.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| C01 centinela A | (-2; 0; 4) | Patrulla en módulo 1 | Repetición de combate |
| C01 centinela B | (2; 0; 8) | Se activa después del primero | Evitar dos amenazas nuevas juntas |
| Hallazgo O01 nivel 2 con O04 | (-4; 0; 10,5) | Nicho de suelo firme fuera de agro | Permite segunda cadena |
| Dos T05 pilares | (-2,5; 0; 15) y (2,5; 0; 17) | Cubierta sólida; paso central libre | Cobertura y lectura del arquero |
| C02a vigía con C02b arco | (0; 0; 19) | Suelo ancho, dentro de cámara | Encuentro de proyectil |
| C02c flecha | Pool de proyectiles | Spawn desde mano/arco, no pieza fija en suelo | Proyectil funcional |
| Hallazgo O01 nivel 3 con O04 | (4; 0; 21,5) | Nicho protegido antes de activar escudo | Permite tercera cadena |
| Zona de ensayo de cadena | X -8 a +8, alrededor de Z 24 | Carril lateral recto de 16 m y recepción protegida | Probar tres Q sin dirigirse al escudo |
| C03a escudo y C03b defensa | (0; 0; 35) | Arena acotada Z 28 a 43; escudo separado | Requiere tercera cadena |
| A01 y A02 salida de combate | (0; 0; 43,5) | Fuera de alcance de carga | Abre al derrotar escudo |

**Qué hace el jugador.** Entra, identifica a cada centinela y los resuelve por separado. Recoge nivel 2 en el nicho, practica Q y una segunda Q durante la secuencia activa y se acerca al arquero usando la cobertura. Cuando la flecha pasa o el vigía termina su recuperación, se aproxima y golpea con impulso. Recoge nivel 3, lee que la tercera cadena rompe defensas y prueba la secuencia en suelo libre.

**Resolución del escudo.** Las primeras cadenas hacen contacto sin quebrar la defensa. La tercera cadena de una misma secuencia produce grieta y ruptura, según el feedback que se integre. No equivalen a tres impulsos independientes separados por segundos. Después de romper la defensa, los impactos válidos reducen vida hasta derrotar al enemigo. Su muerte registra el encuentro y abre la salida a 05.

El nivel no introduce una tecla de ataque que no exista en el prototipo. El daño viene del hurtbox del impulso. La barra o reacción del enemigo informa si el impacto fue bloqueado, rompió escudo o hizo daño. El escudo agrietado C03c es un estado visual, y C03d son fragmentos temporales; no se colocan cuatro guardianes por tener cuatro referencias.

El modelo actual acumula velocidad durante la cadena, aproximadamente 12, 24 y 36 unidades por segundo. La distancia total depende del instante de cada pulsación y puede aproximarse a 7,2 a 14,4 m. Por eso la práctica se hace en un carril lateral amplio, con salida y recepción seguras, y el enemigo está separado del ensayo. Medir la cadena completa y ampliar el suelo si hace falta; si se cambia a impulsos de velocidad constante, documentar ese cambio antes de ajustar distancias.

**Seguridad.** Una cámara por módulo mantiene amenaza y refugio visibles. El enemigo no activa ataques por proximidad XZ si el jugador está arriba o abajo en otra terraza. Se requiere filtro de altura o activación por sala y línea de visión. La patrulla y la carga deben respetar muros y precipicios. Si no se derrota al escudo, la reja permanece cerrada pero el camino de vuelta a 03 sigue disponible.

### Zona 05 Paso de péndulos

**Lugar y función.** Zona media central derecha. Pasillo de aproximadamente 10 por 24 m. Presenta dos péndulos desfasados sobre pinchos. La primera pasada no añade enemigos. Hay suelo de preparación al inicio, un descanso central grande y recepción estable al final.

**Montaje.** El carril de salto está alrededor de X 0. El soporte de cada péndulo se ancla al techo; el peso oscila transversalmente al avance, de izquierda a derecha. El jugador reconoce cuándo el peso se aleja del carril. Debajo, H01 indica el peligro visible; su trigger queda separado de la geometría de puntas.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| Terraza de preparación | X -5 a +5; Z 0 a 4 | Suelo continuo | Observar primer péndulo |
| T01 apoyo previo | (0; 0; 6) | Cara 2 por 2 m | Esperar antes de cruzar |
| H02 péndulo 1 | Pivote (0; 6; 8,5) | Suspensión 4 m; eje paralelo a Z | Cruza primer hueco |
| Descanso central | Centro (0; 0; 12) | Piso 4 por 4 m; Z 10 a 14 | Pausa entre mecanismos |
| H02 péndulo 2 | Pivote (0; 6; 15,5) | Mismo mecanismo, fase distinta | Segunda prueba |
| T01 recepción | (0; 0; 18) | 2 por 2 m; ampliable por calibración | Llegada del segundo salto |
| Terraza final | X -5 a +5; Z 20 a 24 | Suelo continuo; rampa al fondo | Salida a 06 |
| H01 módulos de pinchos | Debajo de huecos, Y -3 | Extender base hasta cubrir zona de caída | Riesgo legible |
| Anclas seguras 05A y 05B | Entrada y descanso central | Fuera de trayectoria del peso | Recuperación por caída |

**Qué hace el jugador.** Observa el primer péndulo desde la entrada. Avanza al apoyo previo, espera a que el peso abandone el centro y cruza usando salto, segundo salto o impulso según la calibración. Aterriza en el descanso, suelta controles si lo necesita y observa el segundo. Repite la lectura y alcanza recepción y salida.

No deben sincronizarse ambos péndulos en la misma fase. Como ajuste inicial, usar amplitud de unos 35 grados, período de 3 s y segundo péndulo desplazado media oscilación. El peso y su peligro siguen el mismo transform; su collider no permanece invisible en el centro mientras la malla se mueve. Los valores son de ajuste, no parámetros ya implementados.

**Fallo y vuelta.** Una caída o contacto con pinchos aplica el daño definido y recupera un ancla válida. El descanso central cuenta como ancla de caída al pisarlo; no es un checkpoint permanente de guardado. En el regreso, el mismo descanso permite leer los péndulos desde el otro lado. El arco no oculta el aterrizaje. Evitar colocar luces coleccionables falsas entre los pinchos.

### Zona 06 Galería del derrumbe

**Lugar y función.** Franja media inferior del mapa. Aproximadamente 12 por 26 m. Enseña una losa que cede, luego una secuencia de tres, y después dos estalactitas señalizadas. Separa las presentaciones para que el jugador entienda cada amenaza.

**Montaje.** Aislar una primera H03 junto a un refugio. Más adelante, formar una cadena de tres H03 con apoyos estables antes y después. Las piedras que caen se anclan sobre un tramo posterior estable, para no exigir aprenderlas mientras se descubre el derrumbe. Crear un corredor lateral con A05 y T02 que conecte entrada y salida y continúe hacia 08; se abre al llegar al final.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| Preparación | Z 0 a 4 | Piso firme | Identificar grietas |
| H03 módulo aislado | (0; 0; 5,5) | Losa horizontal con apoyos aparte | Primera demostración |
| Refugio A | (-3; 0; 7) | Piso 4 por 4 m | Descanso tras primera losa |
| H03 secuencia 1 a 3 | (0; 0; 10), (0; 0; 13), (0; 0; 16) | Distancia inicial 3 m entre centros | Travesía combinada |
| Refugio B | (3; 0; 18) | Piso firme de al menos 4 por 4 m | Aterrizaje sin enemigos |
| H04 caída A | Anclaje (-1; 5; 20) | Piedra separada y sombra en suelo | Aviso antes del impacto |
| H04 caída B | Anclaje (1; 5; 23) | Activación posterior a la primera | Repetición de la amenaza |
| H05 palanca de retorno | (4; 0; 24) | Suelo firme fuera de sombras | Abre corredor seguro |
| A02 y A05 corredor | Lateral X 5, Z 24 | Hoja gira fuera del carril | Retorno hacia inicio y acceso a 08 |
| Salida a 07 | (0; 0; 25,5) | Descenso con recuperación | Ruta principal al cuerno |

**Qué hace el jugador.** Pisa la primera losa, ve aparecer grietas y escucha el aviso; se mueve al refugio. Desde allí ve las tres losas siguientes y la recepción. Cruza sin detenerse sobre una H03. Al llegar al refugio B, observa una sombra en el tramo siguiente, espera la caída y pasa durante la recuperación. Repite con la segunda piedra y activa la palanca.

**Estados de la losa.** Intacta → ocupada y avisando → desprendida → rota → restaurada cuando se reinicia el intento. Como valor inicial, 1,1 s desde primer contacto hasta desprenderse y 0,25 s de aviso visual intenso al final. Las grietas no deben ser un peligro distinto sin explicación. H03a y H03b son el mismo objeto en estados diferentes; H03c son restos, sin recompensa.

**Estados de la estalactita.** Anclada → detecta al jugador en tramo autorizado → aviso de sombra y sonido durante unos 0,9 s → caída → impacto → restos → rearme en nuevo intento. Si el jugador sale de la sombra, la piedra cae donde se anunció; no lo persigue. N03 decorativa permanece quieta y no usa la misma señal activa.

**Reintento y retorno.** El ancla de caída siempre está en preparación o refugio estable. Nunca guardar una coordenada sobre una H03 que desaparece. Después de una caída no letal, antes de devolver el control, restaurar las losas y piedras del tramo que se va a repetir. La muerte o la recarga también restauran el intento. Después de abrir el corredor, la palanca permanece activada y el regreso evita la secuencia; no obliga a esperar que se reconstruyan las losas.

### Zona 07 Cámara del cuerno

**Lugar y función.** Ramal inferior izquierdo. Aproximadamente 14 por 18 m. El cuerno queda al fondo sobre un pedestal estable. Un centinela protege el acceso antes del último pozo; la inspección ocurre en una isla segura y sin persecución.

**Montaje.** Preparar un suelo de combate al entrar, apoyos T01 sobre H01 y una plataforma ancha para el cuerno. El arco del fondo es una abertura de la caverna, no otro portal obligatorio. Una ruta de regreso de raíces conecta con 06 mediante rampas y peldaños funcionales.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| C01 centinela | (0; 0; 3,5) | Patrulla solo Z 2 a 5 | Encuentro previo al pozo |
| Preparación de salto | (0; 0; 6) | Suelo firme con vista al altar | Planear la travesía |
| T01 apoyos A y B | (-1; 0; 8,5) y (1; 0; 11,5) | Cara despejada; ajustar distancias | Último ensayo de movilidad |
| Isla del cuerno | Centro (0; 0; 15) | Piso 6 por 5 m | Inspección segura |
| O04a y O04c | (0; 0; 15,5) | Base fija y soporte de cuerno | Pedestal obligatorio |
| O02 cuerno | (0; 1,05; 15,5) | Boquilla y curva visibles desde entrada | Llave persistente de 08 |
| A01a arco de fondo | (0; 0; 17) | Sin teletransporte asociado | Composición de cámara |
| H01 pinchos | Bajo tramo Z 7 a 13, Y -3 | Puntas visibles desde preparación | Peligro del pozo |
| N02d ruta de vuelta | Lateral izquierdo, conectada con 06 | Raíz sobre rampa o peldaños reales | Regreso sin escalada nueva |
| Salida hacia 08 | Lateral derecho de isla | Apoyos y terraza final estable | Llevar cuerno a reja |

**Qué hace el jugador.** Resuelve al centinela en el suelo inicial. Se detiene en preparación, recorre apoyos con las habilidades aprendidas y aterriza en la isla. Examina el cuerno, confirma y recibe Cuerno obtenido. El objetivo cambia a Activa la reja de la antesala. Puede regresar a 06 o continuar hacia 08.

**Interacción del cuerno.** Es un objeto de progresión, no un consumible ni un arma. Su adquisición añade un registro al inventario. No permite saltarse el jefe, no añade una habilidad de movimiento y no exige tocar melodías. La activación del soporte en 08 usa E y puede reproducir un sonido breve del cuerno.

**Estado y fallo.** Morir después de confirmarlo conserva el cuerno. La isla debe permitir girar y cancelar la inspección sin caer. El centinela no puede avanzar al pedestal ni golpear desde otra altura. La salida hacia 08 muestra apoyos con margen de llegada; no añadir un enemigo en el último aterrizaje.

### Zona 08 Antesala

**Lugar y función.** Terraza inferior central derecha. Aproximadamente 14 por 12 m. Es un punto de descanso antes del jefe, con soporte del cuerno junto a la reja, vista parcial de la cámara 09 y conexión segura de regreso a 06.

**Montaje.** El suelo es continuo. El descanso se coloca en un lateral cercano a entrada; el soporte del cuerno se coloca junto a la reja final. El acceso lateral desde 06 desemboca antes de la puerta. La rejilla dibujada en el mapa puede montarse como detalle cerrado T03 con A03 y protección; no debe convertirse en un salto obligatorio ni en un checkpoint encima de un hueco.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| Descanso O04e v2 | (-3; 0; 3) | Disco sólido sobre suelo estable | Cura y checkpoint 08 |
| Spawn de reintento | (0; 0; 3) | Mira al fondo, fuera de trigger | Reintentar guardián |
| Soporte O04a y O04c | (-2,5; 0; 8,5) | Vacío hasta activar; mira al jugador | Receptor del cuerno |
| A01a y A02 reja ritual | (0; 0; 10,5) | Bisagra izquierda; giro en nicho | Bloqueo por cuerno |
| Dos luces de borde | (-3; 0; 10) y (3; 0; 10) | Reutilizar N05 con efecto cálido | Enmarcar acceso sin nuevo kit |
| A05 entrada lateral | (-6; 0; 5) | Conecta corredor 06 | Retorno seguro |
| C04 custodio opcional | (4; 0; 6) | Fuera del carril y del descanso | Recordatorio breve |
| Rejilla T03 y A03 opcional | (4; 0; 3) | Cerrada, bisagra visible, collider sólido | Detalle cerrado sin descenso en primera versión |

**Qué hace el jugador.** Entra, usa E en descanso y prepara el intento. Se aproxima al soporte. Si no tiene cuerno, aparece Necesitas el cuerno de la cámara inferior y el objetivo indica la zona 07. Si lo tiene, E activa el receptor: se reproduce sonido, aparece el cuerno o una marca equivalente en el soporte y la reja abre. El estado de apertura se guarda. El cuerno conserva su registro de inventario.

**Apertura y cámara.** La hoja A02 gira unos 95 grados alrededor de su bisagra vertical izquierda y queda alojada en un espacio lateral libre. El marco A01 permanece quieto. Si se prefiere una reja que suba, hay que cambiar montaje, pivote y animación juntos; no mezclar la bisagra de la referencia con una traslación vertical sin reconstruir el mecanismo.

**Acceso al combate.** Después de abrir, el jugador todavía puede regresar, guardar o revisar controles. El jefe no comienza mientras se inspecciona el soporte. Al atravesar el corredor completo y pisar el trigger interior de 09, empieza el encuentro. La reja por cuerno permanece desbloqueada; el cierre temporal de arena pertenece a otra instancia.

### Zona 09 Cámara del guardián

**Lugar y función.** Recinto inferior derecho. Arena inicial de aproximadamente 18 por 16 m, con piso amplio y dos apoyos laterales. El guardián de 4,2 m ocupa el fondo. El portal de regreso queda visible a la derecha, fuera del carril de ataques.

**Montaje.** Usar T02 y extensiones como piso continuo. T06 delimita arena; T08 y N02 componen bordes y raíces sin obstáculos pequeños en el carril. Dos T01 laterales permiten apoyos de lectura y movilidad, pero no refugios desde los que el jugador sea invulnerable durante todo el combate. El cierre de entrada no atraviesa al jugador al iniciar.

| Instancia | Posición local inicial | Montaje | Uso |
|---|---|---|---|
| A01 y A02 cierre de arena | (0; 0; 0,7) | Independiente de reja 08 | Evita salir durante encuentro |
| Trigger de inicio | Centro (0; 0; 3) | Volumen que confirma entrada completa | Activa jefe una vez |
| Spawn C05 | (0; 0; 11) | Mira a entrada; núcleo hacia jugador | Encuentro principal |
| Área del núcleo | Anclaje frontal del rig | Collider vulnerable separado | Solo recibe daño expuesto |
| Apoyos laterales T01 | (-6; 0,8; 8) y (6; 0,8; 8) | Cara ancha y aterrizaje visible | Opciones de movimiento |
| A04 portal de victoria | (7; 0; 13) | Trigger fuera de pared y ataque | Regreso tras victoria |
| Destino de salida | Plaza Núñez | Spawn seguro fuera del portal de plaza | Final de recorrido |
| Dos H04 de ataque | Anclajes con área de selección en arena | Aviso de suelo antes de caer | Tercer ataque del jefe |

**Inicio.** El jugador entra completo; la cámara encuadra jefe y suelo. Antes de activar el jefe, se registra automáticamente el checkpoint 08 y su spawn validado, aunque el jugador no haya usado el descanso. Esta excepción garantiza el reintento desde la antesala; no cura durante el combate. El cierre temporal de arena se activa detrás de él, se muestra nombre y vida del guardián y empieza una preparación visible. No infligir daño en el mismo instante de entrada. El portal final permanece inactivo hasta victoria.

**Ataque 1 Barrido frontal.** El guardián lleva un brazo atrás, marca el lado que barrerá y ejecuta un movimiento horizontal. El jugador sale del arco del golpe usando desplazamiento o impulso. Durante recuperación se abre el núcleo.

**Ataque 2 Golpe y onda.** El guardián levanta ambos brazos, golpea suelo y produce una onda con frente visible. El jugador salta sobre la onda. El doble salto permite corregir el tiempo, pero el suelo de llegada sigue siendo seguro. La onda se calcula como frente en expansión, sin aplicar daño a toda la arena de forma instantánea.

**Ataque 3 Dos piedras anunciadas.** Aparecen dos sombras en posiciones elegidas del suelo, con tiempo de aviso. Caen dos H04. El jugador se aparta de las sombras. Las posiciones se fijan al comenzar aviso; no siguen sus pies. Las piedras no caen sobre Spawn 08 ni dañan al otro lado de la puerta.

**Ventana de daño.** Después de cada secuencia, el núcleo cambia de protegido a expuesto, con color y postura legibles. El jugador se acerca y usa impulso contra el collider del núcleo. Un mismo impulso no debe registrar varios impactos por múltiples colliders. Al finalizar la ventana vuelve la defensa. La vida se muestra solo si está conectada a un modelo real.

**Segunda mitad.** A partir de la mitad de vida, el guardián combina dos ataques ya vistos y acorta un poco recuperación. No añade enemigos, nuevas teclas ni pinchos bajo el jugador. Como valores iniciales de ajuste: preparación 0,8 a 1,2 s, exposición 1,5 a 2 s, daño enemigo 20 con vida de jugador 100; ajustar daño y vida del jefe en pruebas para permitir varios ciclos y errores recuperables.

**Derrota del jugador.** Se detiene la secuencia, se limpian ondas, proyectiles y piedras activas, y se vuelve a 08 con vida restaurada. El cuerno y las mejoras permanecen. La reja ritual sigue abierta; el cierre de arena se reinicia. El jefe vuelve con vida completa, sin reanudar un ataque a mitad de animación. Se puede volver a entrar con E o atravesando el trigger según contrato elegido, sin repetir la inspección del cuerno.

**Victoria.** Al llegar la vida del jefe a cero, se anulan sus colliders de daño y ataques pendientes. El núcleo se apaga, el jefe cae o queda inerte y se registra la victoria antes de activar la salida. Abren el cierre de arena y el portal. El jugador puede mirar el recinto y luego usar E en el portal para volver a plaza. Al revisitar, el guardián permanece derrotado y no vuelve a bloquear la salida.

## 5 Reglas comunes para todos los objetos

### 5.1 Mallas materiales y pivotes

Modelar en escala real y aplicar transformaciones antes de exportar. La cara superior de T01, T02, T03 y T07 debe ser plana o tener cambios suaves compatibles con CharacterController. Reservar daño para H01 y otros peligros identificados. La piedra rota de un borde no necesita una colisión exacta con cada pequeño fragmento.

Cada puerta separa marco fijo, hoja y bisagra. Cada péndulo separa anclaje, suspensión y peso; el transform que anima cuelga del eje del techo. Cada losa separa superficie sólida y fragmentos. Cada personaje separa rig, accesorios y colliders funcionales. El cuerno, semilla y brazaletes conservan pivote propio para girar durante inspección.

Materiales compartidos: piedra principal gris azulado #666A7D; superficies transitables azul verdoso #6B97A4; profundidad #252630; bruma #8CBCCC; raíces #655943; hueso #DDD6B8; metal mate #A68C50. El halo cálido de hallazgos es un efecto aparte. Los cambios de estado no requieren duplicar una sala ni una malla completa.

### 5.2 Colisiones y capas

Separar capas o categorías de suelo navegable, pared, jugador, enemigo, interacción, daño y decoración. Los nombres son organizativos; usar las capas disponibles del proyecto al implementar. Los triggers no deben empujar al jugador. Los colliders sólidos no deben conceder premios solo por rozarlos.

T01 y T02 usan cajas o mallas simples para el suelo. T03 requiere un aro real: una caja que cubra todo el módulo taparía el hueco. A01 y A05 necesitan paredes laterales y techo separados para dejar el paso vacío. A02 usa collider en la hoja que sigue su movimiento. A03 usa collider de tapa sólida cuando está cerrada. H01 usa volumen de peligro, no un collider complejo de todas las puntas. El daño de H02 acompaña al peso. El daño de H04 solo está activo durante caída e impacto.

Las raíces decorativas no deben enganchar la cápsula al caminar. Las raíces que soportan un retorno necesitan superficie de suelo continua. Una malla con aspecto de escalera no concede escalada por sí sola: en este montaje N02d acompaña una rampa o peldaños navegables. Existe escalada de prototipo mediante el tag Ladder, pero su movimiento usa ejes mundiales y requiere adaptar alineación, entrada, salida y contador de contacto antes de utilizarla en una raíz orientada libremente. La primera versión conserva el retorno sobre suelo continuo.

### 5.3 Puertas palancas y rejillas

H05 solo necesita proximidad y E. Su primer uso cambia un flag, anima el mango y abre la puerta o grupo de puertas vinculado. Usos posteriores no invierten la apertura ni cierran al jugador dentro de una sala. Conectar la palanca de 03 únicamente con las dos hojas del atajo 03 a 01, y la de 06 con la hoja del corredor seguro de 06. No apuntar ambas palancas al mismo grupo por error.

A03 circular encaja en el hueco de T03. Si se desea un descenso opcional, antes de abrir la tapa debe existir suelo de llegada y un camino de regreso, y E debe mostrar el destino. Para la primera versión, mantener cerradas las rejillas decorativas de 04 y 08 y utilizar el corredor lateral de 06 para retorno. No inventar caídas necesarias a partir de las flechas verticales del mapa conceptual.

El mapa muestra algunas rejillas rectangulares; la referencia individual disponible es circular de diámetro 1,6 m. El montaje inicial usa T03 con A03 circular. Una rejilla rectangular sería una variante adicional que debe modelarse con sus medidas y colisión, no estirar el modelo circular hasta deformarlo.

### 5.4 Activación de actores

Los enemigos se activan por encuentro o sala. Añadir comprobación de altura y visibilidad para que un centinela bajo una terraza no ataque al jugador que está arriba. El código de algunos enemigos calcula distancia ignorando Y; la separación vertical del mapa necesita un filtro adicional al integrarlo.

Las patrullas se limitan a suelo propio y las cargas respetan paredes. No ubicar a un arquero donde pueda disparar a un aterrizaje oculto. No ubicar un enemigo nuevo sobre la misma plataforma donde se aprende un péndulo o se confirma un premio. El custodio no usa lógica de enemigo ni barra de vida.

### 5.5 Daño y recuperación propuestos

Perfil inicial de ajuste: vida máxima 100; centinela 10 por golpe; flecha 8; escudo 20 por carga; péndulo y piedra 15; caída o pinchos 10 con traslado a ancla segura; ataques de jefe 20. Estos valores requieren equilibrio y no sustituyen automáticamente los valores serializados del prototipo.

Una caída genera un solo evento de daño, cancela velocidad y lleva al jugador a suelo firme. Proponer una protección breve de 1 s al reaparecer para impedir daño inmediato; no permitir que distintos triggers de un mismo pozo resten vida varias veces durante el traslado. Si la vida llega a cero, ejecutar la derrota y el checkpoint permanente, sin hacer también una segunda recuperación de caída.

Un punto seguro de caída se actualiza solo al tocar suelo estable autorizado. El descanso O04e de 02 u 08 cura y establece un checkpoint persistente mediante E. Son dos sistemas diferentes: el apoyo de espera del péndulo protege un intento, pero no guarda una partida encima del mecanismo.

### 5.6 Perfil inicial para probar el montaje

El perfil siguiente concreta una primera sesión de ajuste. Movimiento y daño del impulso toman como referencia los valores del perfil de plaza; las vidas enemigas comunes toman los prefabs existentes. La vida del jefe y los avisos de ataque son propuestas nuevas. Conservar la cápsula del jugador al medir y ajustar el escenario antes de cambiar la física para salvar un hueco mal montado.

| Parámetro | Valor inicial para MI | Aplicación |
|---|---|---|
| Caminar y correr | 5 y 8 m por segundo | WASD y Shift con perfil de exploración |
| Altura de salto y gravedad | 1,5 m y -20 m por segundo al cuadrado | Segundo salto reinicia impulso vertical después de semilla |
| Impulso | 12 m por segundo durante 0,2 s | Nivel 2 acumula 24 y nivel 3 acumula 36 durante cadena |
| Daño de impulso | 20 por impacto válido | Una aplicación por eslabón y objetivo; núcleo debe filtrar duplicados |
| Vida C01 y C02 | 50 y 30 | Tres y dos impactos de 20 desde vida completa, respectivamente |
| Vida C03 | 40 después de romper defensa | Dos impactos válidos de 20; ruptura y daño pueden coincidir si se permite |
| Vida C05 | 200 propuestos | Diez impactos de 20; núcleo solo vulnerable durante exposición |
| Preparación C01 | 0,6 s propuestos | Señal visible antes del golpe; requiere añadir estado de aviso |
| Preparación C02 | 0,8 s y disparo cada 2 s propuestos | Primer disparo también anunciado; cobertura con colisión |
| Preparación y recuperación C03 | 0,9 s y 6 s como referencia | Ajustar recorrido de carga para no abandonar arena |
| Aviso H03 y H04 | 1,1 s y 0,9 s propuestos | Grietas antes de ceder y sombra antes de caída |
| Péndulo H02 | 35 grados y período 3 s propuestos | Segundo desfasado 1,5 s; revisar volumen barrido |

El impulso no concede invulnerabilidad en el código actual. Por tanto, golpear una carga puede dañar también al jugador; la solución enseñada es esquivar la carga y aprovechar recuperación. El jefe no exige cadena triple para recibir daño: la tercera cadena se enseña para romper al escudo de 04. Ajustar vida, daño y exposición del núcleo si una cadena permite terminar el combate demasiado rápido.

## 6 Persistencia guardado y estados

### 6.1 Datos mínimos del mundo inferior

| Dato propuesto | Valor inicial | Cuándo cambia | Regla al cargar |
|---|---|---|---|
| MI_SemillaObtenida | false | Confirmación O03 | Activa doble salto y oculta O03 |
| MI_Brazaletes01 | false | Hallazgo 03 confirmado | Garantiza nivel 1 |
| MI_Brazaletes02 | false | Primer hallazgo de 04 | Garantiza nivel 2 |
| MI_Brazaletes03 | false | Segundo hallazgo de 04 | Garantiza nivel 3 |
| MI_CuernoObtenido | false | Confirmación O02 | Inventario conserva cuerno |
| MI_Atajo03Abierto | false | Palanca de 03 | Puerta de atajo abierta |
| MI_Retorno06Abierto | false | Palanca de 06 | Corredor seguro abierto |
| MI_Escudo04Derrotado | false | Muerte del escudo | Reja 04 a 05 abierta |
| MI_Reja08Abierta | false | Activación del cuerno | Reja ritual abierta |
| MI_GuardianDerrotado | false | Vida del guardián a cero | Jefe inerte y portal activo |
| MI_Checkpoint | 01 hasta primer descanso | Descanso 02 u 08 o entrada validada al jefe | Spawn validado del punto seguro |
| MI_ZonasDescubiertas | Solo 01 al entrar | Entrada a cada sala | Mantiene conocimiento del mapa |

Los nombres anteriores son un contrato de datos propuesto, no campos que ya existan en el guardado. Registrar hallazgos, apertura de atajos y victoria inmediatamente en la ranura activa. Guardar el checkpoint en un descanso estable; al iniciar 09 registrar de forma automática el spawn estable de 08 como excepción de reintento. Si no hay sistema de escritura disponible durante integración, mantener explícito que la sesión solo conserva estado en memoria hasta añadirlo.

### 6.2 Qué persiste y qué se reinicia

Persiste: semilla, tres hallazgos de impulso, cuerno, puertas desbloqueadas, atajos, escudo de 04 derrotado, jefe derrotado y zonas descubiertas. Se reinician en un nuevo intento: ataques en ejecución, flechas, ondas, sombras, losas desprendidas y piedras que caen. La vida del jefe vuelve completa si el jugador muere antes de vencerlo.

Los enemigos comunes pueden volver al reintentar desde un descanso o al cargar la escena; los premios recogidos no reaparecen. El escudo derrotado queda registrado para mantener el acceso principal abierto. Si muere el jugador con escudo solo agrietado y todavía vivo, el encuentro vuelve al estado inicial y se conserva nivel 3.

Pausar no borra progreso ni reconstruye trampas. Salir a la plaza conserva el mundo inferior y vuelve a usar un spawn seguro al reentrar. Cargar una ranura reconstruye todos los visuales desde sus flags antes de habilitar movimiento. No cargar con el jugador atrapado en una puerta cerrada ni en un pozo.

### 6.3 Tabla de situaciones del jugador

| Situación | Resultado esperado |
|---|---|
| Cancela semilla, brazaletes o cuerno | Vuelve a exploración sin recoger y con objeto restaurado |
| Muere tras recoger una mejora | Conserva capacidad y pedestal queda vacío |
| Muere tras recoger cuerno y antes de 08 | Conserva cuerno; puede ir al soporte sin repetir hallazgo |
| Cae sobre pinchos de 05 | Un daño y recuperación en última ancla estable de 05 |
| Falla derrumbe | Tramo se restaura al reiniciar; ancla fuera de H03 |
| Llega a 08 por corredor sin cuerno | Descanso disponible, puerta cerrada y ruta a 07 abierta |
| Muere durante jefe | Reaparece 08, reja por cuerno abierta, jefe reiniciado |
| Sale a plaza y vuelve | Hallazgos y atajos conservados; spawn fuera de portal |
| Carga después de victoria | Guardián derrotado; cierre y salida abiertos |
| Usa E repetidamente en objeto recogido | No aumenta nivel ni duplica inventario |
| Muere con ataque y caída simultáneos | Una sola derrota y un solo traslado |

## 7 Cámara señales iluminación y audio

### 7.1 Lectura de la navegación

La cámara del juego permite movimiento 3D en XZ. La perspectiva de la ilustración comunica volumen; no impone cambiar el controlador a 2D ni fijar una cámara isométrica. El perfil del nivel se deriva de la exploración existente y usa zonas de cámara para asegurar visión del siguiente apoyo.

En 01 y 02 se ve el santuario y el ascenso. En 03 se ve altar, palanca y recepción del impulso. En 04 se encuadra un encuentro por módulo. En 05 entran en cuadro peso, apoyo de espera y aterrizaje; el techo que oculte el péndulo se atenúa o retira. En 06 se ve refugio antes de pisar la losa. En 07 se ve el cuerno desde preparación. En 08 se reconoce soporte y reja. En 09 se muestran suelo completo, núcleo y avisos de ataque.

Raíces del primer plano pueden dar profundidad, pero no cruzar una plataforma de aterrizaje, el prompt de interacción o una sombra de peligro. Los pilares de 04 protegen de flechas sin bloquear la cámara. Antes de finalizar, revisar cada sala desde la entrada, la salida y durante el regreso.

### 7.2 Señales visuales por función

| Función | Señal principal | Señal complementaria |
|---|---|---|
| Suelo transitable | Cara superior azul verdosa continua | Borde de piedra sólido |
| Hallazgo disponible | Modelo sobre soporte y halo cálido | E Examinar y nombre |
| Hallazgo recogido | Soporte vacío | Luz disminuida y registro de inventario |
| Descanso | Disco O04e sólido y despejado | E Descansar con efecto de recuperación |
| Pinchos | Silueta afilada visible en pozo | Sonido o aviso al caer |
| Péndulo | Balanceo del peso real | Crujido acompasado |
| Losa inestable | Grietas y movimiento de aviso | Sonido antes de ceder |
| Piedra activa | Sombra de impacto | Polvo de techo y sonido de desprendimiento |
| Reja con cuerno | Soporte vacío reconocible | Texto Necesitas el cuerno |
| Núcleo vulnerable | Postura abierta y cambio de material | Sonido breve de apertura |
| Salida activa | Portal encendido | E Volver a Plaza Núñez |

No comunicar progreso solo mediante color. Añadir forma, texto o movimiento legible. La piedra de luz N05 no aparece en el inventario ni usa el mismo halo intenso que un premio. La estalactita decorativa no tiembla si nunca va a caer.

### 7.3 Audio mínimo por zona

Ambiente base de caverna y goteo en todo el mundo; aire de profundidad más evidente en pozos; resonancia suave en santuario; crujido de raíces cerca de retornos; sonido de metal o piedra en palancas y rejas; señal clara para hallazgo; preparación y paso de péndulo; grieta de losa; aviso y golpe de estalactita; apertura de núcleo y confirmación de victoria.

Los avisos esenciales también se ven. Los subtítulos de custodio no se superponen al prompt de interacción. No reproducir una explicación extensa encima de la primera lectura del péndulo. La música del jefe empieza al iniciar 09 y se detiene o cambia tras derrota o victoria, sin duplicarse en reintentos.

## 8 Integración en Unity y estado del proyecto

### 8.1 Organización de archivos y escena

Mantener las referencias en ArtSource/Worlds/MundoInferior. Crear los recursos jugables en Assets/Worlds/MundoInferior con subcarpetas Scenes, Prefabs, Meshes, Materials, Textures, Audio y VFX. Los archivos de referencia PNG no son todavía prefabs ni modelos 3D.

Jerarquía recomendada: MI_WorldRoot contiene Geometry con Room_01_Umbral a Room_09_Guardian; Traversal con rampas, pruebas y atajos; Interactables con mejoras, cuerno, puertas y descansos; Hazards con pinchos, péndulos, losas y piedras; Actors con enemigos, custodio y jefe; Atmosphere con fondos, niebla, luces y audio; CameraZones con límites de cámara; Runtime con estado de progreso, checkpoints y controlador de nivel.

Cada instancia funcional utiliza nombre estable, por ejemplo MI_R03_O01_Upgrade01, MI_R04_O01_Upgrade02, MI_R04_O01_Upgrade03, MI_R07_O02_Horn, MI_R08_A02_HornGate y MI_R09_A02_ArenaGate. El ID de recompensa es independiente del nombre de la malla. Una copia visual de un brazalete no debe convertirse en una cuarta mejora.

### 8.2 Qué se puede reutilizar

| Sistema local | Uso posible | Adaptación necesaria |
|---|---|---|
| PlayerController y PlayerAbilityModel | Movimiento, salto, impulso y cadenas | Perfil coherente, premios idempotentes y separación de entrenamiento |
| DoubleJumpPickupReward | Premio de semilla | Vincular O03 y guardar adquisición |
| DashPickupReward | Premio de brazaletes | Nivel objetivo y validación de ID para evitar duplicados |
| InspectablePickup e InspectionModel | Examinar, rotar, confirmar o cancelar | Modelo hijo separado, cámara orbital y UI de mundo inferior |
| DashHurtbox y Health | Impacto de impulso y vida | Capas, un golpe por objetivo y daño de núcleo |
| MeleeEnemy | Centinela C01 | Límites, altura, visibilidad y arte del enemigo |
| ArcherEnemy y ArrowProjectile | Vigía C02 y flecha | Cobertura, límites de sala y limpieza de proyectiles |
| ShieldEnemy | Enemigo C03 | Tercera cadena, colisiones y apertura de salida |
| PlayerRespawn | Referencia para recuperar posición | Checkpoints reales, anclas estables y limpieza de estado |
| CameraFollow y CameraZoomTrigger | Referencia de encuadre | Convivencia con ExplorationOrbitCamera |
| LevelTeleportTrigger y PlazaPortal | Referencia de transiciones | Viaje al nivel real, destino seguro y persistencia |
| HUD e interfaz existentes | Vida, objetivo, inspección y avisos | Conectar datos reales de MI; no simular recursos inexistentes |

### 8.3 Sistemas que hay que completar

H01 requiere daño y recuperación de caída; H02 oscilación y contacto móvil; H03 aviso, desprendimiento y reinicio; H04 aviso, caída e impacto; H05 apertura persistente de un atajo; O02 adquisición y activación de reja; O04e descanso y checkpoint; C04 interacción pacífica; C05 vida, ataques, exposición de núcleo, derrota y victoria. También se requiere controlador de estado del nivel, reconstrucción desde guardado y enlace de los portales con el mundo real.

El guardado actual conserva las lecciones de la plaza, entrenamiento, mundos visitados y tiempo, y vuelve al punto seguro de Plaza Núñez. Eso no equivale a guardar semilla, brazaletes, cuerno, trampas, puertas o checkpoints del mundo inferior. El portal actual de la plaza lleva a umbrales de demostración dentro de su escena; se debe conectar a este recorrido cuando exista.

### 8.4 Precauciones concretas al reutilizar el prototipo

InspectablePickup suspende CameraFollow, mientras la plaza emplea ExplorationOrbitCamera. Se necesita un adaptador que bloquee el controlador de cámara activo y lo restaure al confirmar o cancelar. También debe restaurarse el foco de UI y el bloqueo de movimiento. Reutilizar el componente sin esa conexión puede causar que ambas cámaras intenten escribir la posición de vista.

La UI de inspección de la plaza está vinculada a AnalyzableObject y a sus lecciones; los pickups de Movement utilizan otra ruta. Añadir un presentador que lea el hallazgo del mundo inferior y no obligue a completar los gestos del tutorial para recoger semilla, brazaletes y cuerno.

El escudo comprueba una cadena de tres. En Movement la ventana de cadena está serializada en 0,3 s; la duración de cada impulso es 0,2 s y el código exige estar impulsándose para encadenar. La plaza usa otros ajustes. Para el nivel propuesto, partir de su perfil de exploración y probar la secuencia real; no explicar que el jugador puede esperar 0,5 s después de acabar un impulso si la cadena solo se acepta mientras está activo.

Los enemigos cuerpo a cuerpo actuales no tienen una preparación visual de golpe implementada, y el arquero puede disparar inmediatamente al entrar en rango. Añadir sus avisos antes de usarlos como encuentros de aprendizaje. Bloquear el movimiento durante inspección no pausa la IA: desactivar agresión hacia la alcoba segura o suspender el encuentro correspondiente mientras se examina un premio. Evitar dos radios de interacción que respondan simultáneamente a E; un gestor central selecciona el objeto válido más cercano y visible.

Movement contiene valores de prueba, como vida 999 del jugador y daño 70 de carga del escudo, que no son el equilibrio propuesto aquí. El controlador BossFight existente tiene un bucle de esquiva y daño al jugador, pero no resuelve la vida y victoria del guardián descrito. Un cambio de malla no implementa ese combate.

## 9 Orden de construcción y validación

### 9.1 Montaje por etapas

1. Crear nueve raíces de sala y marcar Entry, Exit y sockets de retorno. Colocar suelo temporal, paredes y pendientes con materiales simples.
2. Jugar la ruta 01 a 09 y cada regreso sin decoración. Comprobar que todas las flechas tienen conexión física y que los descensos se pueden recuperar.
3. Medir salto simple, doble salto, impulso y cadenas con el personaje y la cámara elegidos. Documentar las distancias medidas antes de fijar los pozos.
4. Colocar semilla y los tres hallazgos de brazaletes. Verificar estado nuevo, cancelación, confirmación, muerte, salida a plaza y carga.
5. Añadir palancas y retornos. Probar ambos sentidos y reapertura desde guardado.
6. Integrar C01, después C02 y después C03. Comprobar altura, cobertura, muros y límites de cada encuentro.
7. Añadir H01, primer péndulo, segundo péndulo, primera losa, secuencia de losas y piedras en ese orden. Probar un riesgo aislado antes de combinarlos.
8. Integrar cuerno y reja de 08. Acceder a 08 con y sin cuerno, morir tras adquirirlo y recargar antes y después de abrir.
9. Integrar 09 con un ataque a la vez, núcleo y vida. Después añadir segunda mitad, derrota, victoria y portal final.
10. Sustituir bloqueos por kit modelado, colocar raíces, iluminación y sonido. Repetir recorrido con cámara final, texto grande y controles de teclado y mouse.

### 9.2 Criterios para ajustar saltos

Medir desde borde de lanzamiento hasta borde de aterrizaje, no entre centros de plataformas. Registrar altura relativa, tamaño de cápsula, velocidad inicial y si se usó carrera. Medir al menos salto simple desde reposo, salto corriendo, doble salto, un impulso, salto con impulso y cadena triple.

Un salto obligatorio debe tener margen: como punto de partida, reservar aproximadamente 20 por ciento del alcance medido para error humano y una recepción de al menos 2 m, ampliable. Para un bloqueo de habilidad, la prueba debe resultar imposible con el conjunto anterior y posible con margen con la habilidad nueva. Si la carrera permite saltarlo, aumentar diferencia o rediseñar el gate; no subir el daño para forzar la ruta.

No exigir activar una cadena exactamente al máximo de su ventana mientras se aprende el sistema. El patio de 04 tiene suelo suficiente para probar Q tres veces sin caer. Después del impulso, la cámara debe conservar el aterrizaje en cuadro y el controlador no debe mantener inercia que arroje fuera al jugador.

### 9.3 Pruebas de aceptación del nivel

| Prueba | Criterio de aprobación |
|---|---|
| Recorrido nuevo | Puede terminarse con teclado y mouse desde 01 hasta plaza |
| Semilla | Se recoge con capacidades iniciales y habilita segundo salto |
| Tres brazaletes | Cada ID concede su nivel una vez; nunca nivel 4 por duplicación |
| Escudo | Solo tercera cadena rompe defensa y victoria abre paso a 05 |
| Péndulos | Peso y collider coinciden; hay espera y recepción visibles |
| Derrumbe | Toda losa avisa y todo fallo tiene ancla estable |
| Piedras | Sombra precede caída y coincide con impacto |
| Cuerno | Se conserva al morir y activa reja sin consumirse |
| Atajos | Permiten ida y vuelta y persisten tras cargar |
| Inspección | Confirmar y cancelar restauran cámara, modelo y controles |
| Separación de alturas | Enemigos y trampas no afectan otra terraza por coincidencia XZ |
| Checkpoint 08 | Reintento empieza fuera de arena y sin ataque activo |
| Jefe | Vida real, exposición, daño, derrota y victoria comprobables |
| Portal final | Solo activa tras victoria y termina en suelo seguro de plaza |
| Carga y reentrada | Visuales y flags coinciden antes de devolver control |
| Cámara | Ningún aterrizaje, aviso o objeto esencial queda oculto |
| Accesibilidad | Señales esenciales visibles, texto legible y dispositivos opcionales |

### 9.4 Lista final antes de entregar una versión jugable

Comprobar que las nueve salas existen y se pueden recorrer; los tres hallazgos de impulso están antes del escudo; el cuerno está en 07; el soporte está en 08; el portal final está en 09; las palancas tienen destinatarios distintos; descansos están sobre suelo fijo; los colliders no rellenan arcos ni huecos; los enemigos respetan alturas y muros; todos los fallos tienen recuperación; el guardado reconstruye premios, puertas y victoria; la cámara de inspección se restaura y ningún comando obligatorio depende de manos o voz.

## 10 Referencias y trazabilidad

El diseño se apoya en el chat Diseña el mundo inferior, donde se definió la ruta y se generaron las referencias; en el chat Completar diseño y flujo de usuario y su diseño integral de interfaz; y en las conversaciones de referencias visuales para conservar el lenguaje de formas claras y piezas separadas. La organización específica del nivel se conserva de Specs/Worlds/Mundo-inferior.md.

Fuentes del proyecto: ArtSource/Worlds/MundoInferior/01-estructura-nivel.png; ArtSource/Worlds/MundoInferior/Recursos/prompts-recursos.json; ArtSource/Worlds/MundoInferior/ModelosIndividuales; Specs/Movement-Pickup-Inspection.md; Specs/Combat-Damage-Enemies.md; Specs/Level-Triggers.md; Specs/Week08/05-Plaza-Lobby-Tutorial.md; Specs/UI/Diseno-integral-2026-09-29.md; PRODUCT.md; scripts y escenas de Assets/Prototype.

Las dimensiones del kit provienen del inventario anterior. Las coordenadas locales, daños, tiempos y reglas detalladas de guardado de esta guía son parámetros propuestos para montar y comprobar el nivel. El catálogo siguiente conserva los IDs de las referencias para que el modelado, la escena y las interacciones utilicen el mismo nombre de pieza.

## 11 Recuento de instancias funcionales iniciales

Este recuento concreta el montaje propuesto. No cuenta cada extensión de terraza, pared, raíz o roca: esas piezas se repiten para alcanzar las dimensiones de cada sala y se registran al construir el bloqueo. Las cantidades de la tabla sí determinan los encuentros, las recompensas y los mecanismos de la primera versión.

| Conjunto | Cantidad inicial | Ubicación |
|---|---|---|
| Semilla O03 | 1 | Altar de 02 |
| Hallazgo de brazaletes O01 | 3 pares | Uno en 03 y dos en 04 |
| Cuerno O02 recogible | 1 | Altar de 07; réplica visual opcional en receptor 08 |
| Bases O04a | 6 | Semilla 02, brazaletes 03 y dos de 04, cuerno 07 y receptor 08 |
| Soportes O04b | 3 | Tres hallazgos de brazaletes |
| Soportes O04c | 2 | Hallazgo 07 y receptor 08 |
| Soporte O04d | 1 | Semilla 02 |
| Discos O04e v2 | 2 | Descansos de 02 y 08 sobre suelo, separados de altares |
| Centinelas C01 | 4 | Uno 03, dos 04 y uno 07 |
| Vigía C02 | 1 | Módulo central de 04; arco 1 y flechas reutilizables |
| Guardián de escudo C03 | 1 | Final de 04 con una defensa y sus estados |
| Custodio C04 | 1 más aparición opcional | 02 obligatorio como recurso; aparición 08 opcional |
| Guardián C05 | 1 | Arena 09 |
| Péndulos H02 | 2 | Paso 05 |
| Losas H03 | 4 | Una aislada y tres en secuencia en 06 |
| Piedras activas H04 | 2 de recorrido y 2 del jefe | 06 y ataque de 09; se reutilizan en cada intento |
| Palancas H05 | 2 | Atajo en 03 y corredor en 06 |
| Rejas A02 | 6 | Extremos atajo 01 y 03, salida 04, corredor 06, cuerno 08, cierre arena 09 |
| Portales A04 | 2 dentro del nivel | Retorno 01 y victoria 09; destino en plaza aparte |
| Rejillas A03 con T03 | 0 a 2 opcionales | Motivos cerrados de 04 y 08; sin gate principal |
| Árbol N01 principal | 1 | Fondo central visible desde alturas superiores y medias |

En cada actor, accesorios y estados pertenecen a la misma instancia lógica. La flecha C02c y los fragmentos H03c o C03d son objetos de ciclo temporal. Las dos H04 del jefe no suman dos estalactitas decorativas obligatorias sobre la antesala. Los arcos simples y dobles se distribuyen en entradas y salidas según longitud del corredor; A01b se construye por combinación y no añade otra mecánica.

## 12 Catálogo completo de piezas y componentes

Este catálogo desarrolla los 33 IDs base del kit de `Specs/Worlds/Mundo-inferior.md` y los campos de tamaño, partes y pivote de `ArtSource/Worlds/MundoInferior/Recursos/prompts-recursos.json`. Los nombres de archivos corresponden a las imágenes individuales presentes en `ArtSource/Worlds/MundoInferior/ModelosIndividuales/`. Son referencias de modelado: un PNG no equivale a una malla, un prefab, un collider, una animación ni una mecánica implementada. Las instrucciones siguientes son una propuesta de montaje para producir el nivel. Deben comprobarse con el controlador real, la cámara y los sistemas de juego antes de considerar final cualquier distancia o interacción.

Las dimensiones usan la escala inicial de 1 unidad de Unity = 1 metro. La retícula de 2 m sirve para unir módulos; no determina automáticamente el alcance de salto. Una pieza tiene un ID de familia aunque disponga de varias imágenes, partes o estados. Las variantes descritas como cambio de material, rotación o escala no obligan a producir una imagen adicional. Las zonas son: 01 Umbral; 02 Santuario; 03 Brazaletes; 04 Centinelas; 05 Péndulos; 06 Derrumbe; 07 Cuerno; 08 Antesala; 09 Guardián.

### T01 Plataforma pequeña

Tamaño inicial: 2 × 2 × 1 m. Es una única masa de roca con la superficie superior integrada, plana y despejada. El pivote se sitúa en el centro de esa cara: al colocar varias, la altura del transform representa la altura de aterrizaje. Montarla sobre una columna o sobre roca de fondo que explique su soporte; no añadir irregularidades de colisión a la cara transitable por las grietas del dibujo. Usar un collider sencillo de suelo, ajustado al volumen útil. Las variantes de borde recto y roto cambian la silueta exterior, conservando un área suficiente para la cápsula del jugador.

Uso principal: escalones 02→03, apoyos de espera y cruce de 05, descenso 06→07 y salto de salida de 07. También sirve como apoyo lateral en 09 si el diseño del jefe lo requiere. Se interactúa pisándola o saltando hacia ella: no tiene acción de recoger, activar ni romper. Cada apoyo que parezca alcanzable debe poder alcanzarse con la mejora que el jugador ya posee; cada llegada debe estar libre de daño oculto. Pendiente: medir separación, desnivel y margen de aterrizaje con salto simple, doble e impulso reales. Las plataformas que colapsan pertenecen a H03, no a T01.

- Archivo: `T01-plataforma-pequena.png`.

### T02 Terraza larga

Tamaño inicial: 8 × 4 × 2 m, con extensiones de 2 m. Separar núcleo, extensiones y remates; colocar el pivote en el centro superior y unir los extremos a la retícula. Un ensamblaje de terrazas forma un suelo continuo: sellar juntas visuales y de colisión para evitar que la cápsula tropiece. La superficie azul verdosa marca el plano transitable; el volumen de basalto puede descender por debajo sin afectar la navegación. Para el bloqueo utilizar cajas de suelo; en el modelo final conservar colisiones simples independientes de raíces o puntas decorativas.

Uso en 01–09 como base estable de salas, descansos, aproximaciones y aterrizajes. Ensancharla mediante módulos para el patio de 04 y la arena de 09; no intentar encajar un combate amplio sobre una única pieza de 8 × 4 m. Es suelo pasivo, sin interacción contextual. Las variantes de unión interior carecen de borde de acantilado donde conectan con otra pieza; los remates exteriores sí lo muestran. Pendiente: dimensionar las salas para la cámara, alcance de ataques y giro del jugador, y comprobar las transiciones con rampas, puentes y puertas.

- Archivo: `T02-terraza-larga.png`.

### T03 Plataforma con hueco

Tamaño inicial: 4 × 4 × 1,5 m con hueco circular de Ø1,6 m. Separar el anillo de roca y el revestimiento interior; A03 proporciona la tapa independiente. Situar el pivote en el centro superior, con el hueco centrado. El agujero debe atravesar el volumen completo: construir la colisión del suelo con varios sectores alrededor del hueco, nunca con una caja que lo tape. La combinación cerrada incorpora el collider de la tapa A03; la abierta deja libre el descenso. El borde interior debe permitir leer la caída y no enganchar al jugador.

Uso en la primera versión: motivo cerrado opcional en 04 y 08, sin descenso obligatorio. El retorno funcional usa el corredor de 06. El mapa ilustrado muestra rejillas rectangulares, mientras el kit aprobado T03+A03 es circular: para este ensamblaje adoptar el círculo; fabricar una tapa rectangular sería ampliar el kit. No colocar una caída obligatoria sin suelo seguro visible debajo ni una única salida que impida volver. La tapa permanece cerrada y sin prompt en este montaje. Un descenso funcional futuro exigiría definir apertura, llegada y regreso; una rejilla dibujada no establece por sí sola un mecanismo.

- Archivo: `T03-plataforma-hueco.png`.

### T04 Rampa de roca

Tamaño inicial: 4 × 4 m con desnivel de 2 m. Es una cuña maciza de roca; el cuerpo integra la cara inclinada continua y puede llevar faldones laterales separados. Pivote en el centro del borde inferior. Orientar su extremo alto hacia la terraza de llegada, sellando la unión; para invertir el recorrido reutilizar la misma pieza girada. La colisión debe reproducir una pendiente limpia, mediante un volumen simple o una malla de colisión simplificada, sin convertir las grietas de la textura en escalones.

Uso: enlaces estables de 01↔02, conexión de la salida de 05 con 06, aproximación al pedestal de 07 y corredor seguro hacia 08, allí donde el bloqueo confirme la pendiente. Se recorre caminando; no requiere una acción de trepar. La medida inicial equivale a una subida de 2 m en 4 m de avance y debe reducirse o alargarse si el controlador no la acepta. Pendiente: comprobar límite de pendiente, fricción, bajada y cámara; no sustituir una prueba de salto por una rampa que permita saltarse una mejora obligatoria.

- Archivo: `T04-rampa-roca.png`.

### T05 Pilar de apoyo

Tamaño principal: 2 × 2 × 4 m; variantes de altura 2 y 6 m. Separar cuerpo, base y remate superior reutilizable. Pivote en el centro de la base para ajustar la altura sin mover la cimentación. El remate puede sostener T01, T02 o T07; al unirlos, evitar caras superpuestas visibles y grietas de suelo. Utilizar una caja sencilla para la masa sólida cuando el jugador puede chocar con ella. La parte enterrada o exclusivamente situada en el fondo puede carecer de collider.

Uso: soporte bajo las terrazas de 01–03, dos referencias de cobertura y volumen en 04, pilares de los apoyos de 05–07 y volumen lateral en 09. La cara superior solo es transitable cuando está integrada explícitamente en el recorrido; un pilar de fondo no invita a un salto opcional por accidente. Es una estructura pasiva, sin activación. Las alturas se ajustan por módulos o escala controlada, revisando proporciones. Pendiente: comprobar que los pilares de 04 permiten acercarse al arquero sin bloquear la circulación y que los de 09 no ocultan al jefe o atrapan al jugador.

- Archivo: `T05-pilar-roca.png`.

### T06 Pared de caverna

Tamaño inicial: 4 × 6 × 1 m. El kit individual separa pared recta, esquina interior y remate. Pivote en la base central y conexiones cada 4 m. Colocar paneles con la cara rocosa hacia el espacio visible y conservar una cara jugable limpia; solapar solo el volumen exterior para ocultar costuras. El cierre de navegación usa cajas o volúmenes sencillos, evitando salientes pequeños que enganchen al jugador. Una pared situada entre cámara y jugador necesita una solución de visibilidad antes de decorar.

Uso en todas las zonas para delimitar salas, conducir corredores y dar profundidad; en 03 forma el límite del ramal de brazaletes, en 06 sostiene el corredor lateral y en 09 cierra la arena. No es una pared rompible ni escalable por defecto. Los pasos se abren combinándola con A01 o A05, dejando también vacío el collider. Pendiente: probar cámara, transparencia u ocultación del techo, y comprobar que las flechas del recorrido se traducen en pasos reales y no atraviesan muros sólidos.

- Archivo recto: `T06a-pared-recta.png`.
- Archivo esquina: `T06b-pared-esquina.png`.
- Archivo remate: `T06c-pared-remate.png`.

### T07 Puente de losas

Tamaño: 2 × 4 × 0,6 m por tramo. Montar tres losas alineadas y dos apoyos; las extensiones reutilizan el mismo patrón. Pivote en el centro superior del inicio del tramo, para encadenar el siguiente al final. La cara transitable permanece plana aunque el borde esté erosionado. Puede emplearse un collider continuo para cada tramo estable, sin huecos funcionales entre losas visualmente juntas. Añadir apoyos inferiores compatibles con T05 o la pared de roca y conservar un final claro de puente.

Uso principal: galería elevada que conecta la región superior con 03, salida 04→05 y uniones estables de 06. En 05 no debe invadir el espacio donde la intención es saltar entre apoyos; en 06 los segmentos que ceden se sustituyen expresamente por H03. Se cruza caminando o impulsándose: T07 por sí mismo no se rompe. El remate roto es una variante visual para bordes exteriores, no un mecanismo de caída. Pendiente: medir anchura útil para la cápsula, giro, cámara y posibilidad de retorno en ambas direcciones.

- Archivo: `T07-puente-losas.png`.

### T08 Rocas y estalagmitas

Escala de trabajo: rocas de 0,5–1,5 m; estalagmitas de 1–3 m. Hay tres rocas y dos formaciones individuales. Pivote de cada pieza en su base. Montarlas como grupos de borde, base de pilares o fondo, variando orientación y escala moderadamente para evitar repeticiones. Las rocas grandes próximas al jugador pueden usar cajas o colisiones simplificadas; las pequeñas y las estalagmitas de fondo no necesitan collider. No usar la geometría afilada como colisión de daño: esa función pertenece a H01.

Uso en 01–09, concentrado en los márgenes y huecos del escenario; las estalagmitas delimitan visualmente el fondo de 05–07 y las rocas enmarcan los altares de 02, 03 y 07. Son decoración o límites sólidos visibles, sin acción contextual ni recompensa. Mantener vacíos el aterrizaje, la aproximación a puertas y el perímetro inmediato de interacción. Pendiente: revisar que una estalagmita decorativa no se confunda con un pincho dañino y que ninguna roca cree un apoyo que permita omitir la ruta de progresión.

- Roca grande: `T08a-roca-grande.png`.
- Roca mediana: `T08b-roca-mediana.png`.
- Roca pequeña: `T08c-roca-pequena.png`.
- Estalagmita alta: `T08d-estalagmita-alta.png`.
- Estalagmita corta: `T08e-estalagmita-corta.png`.

### A01 Arco de piedra

Paso libre inicial: 2,4 × 3,2 m. Separar jambas, dovelas y piedra clave según convenga al modelado; los detalles pequeños pueden resolverse con normales. Pivote en la base central del paso. Colocar sobre un suelo continuo con los laterales conectados a pared o roca; para la variante doble combinar dos arcos compatibles. El collider se reparte por jambas y coronación, dejando completamente libre el paso. La variante doble es reutilización de A01, aunque exista una referencia individual del conjunto.

Uso: entradas visibles de salas, arco de fondo de 07, marco del umbral 08→09 y cierre de arena. En 01 se utiliza el marco propio de A04 sin superponer A01. El arco indica una transición espacial, pero no bloquea por sí mismo: A02 aporta la reja si la puerta está cerrada. Se atraviesa sin pulsar una acción. Comprobar ancho suficiente para el jugador y sus movimientos, y mostrar suelo más allá del marco antes de cruzar. No duplicar dos triggers de transición al construir un arco doble.

- Arco simple: `A01a-arco-simple.png`.
- Arco doble: `A01b-arco-doble.png`.

### A02 Reja vertical

Hoja de 2,2 × 3 m. Separar marco fijo, hoja móvil, bisagras y pestillo. El JSON individual concreta un pivote en el eje vertical de las bisagras izquierdas y apertura por giro; esta es la configuración propuesta de montaje. Unir el marco a A01 y reservar espacio para el barrido de la hoja sin golpear el altar ni al jugador. El collider de la hoja se mueve con ella; al terminar la apertura queda fuera del corredor útil. Los barrotes no requieren un collider individual por cada varilla.

Uso funcional en seis instancias: dos hojas del atajo 03↔01 controladas por H05 de 03; salida de 04 vinculada a la derrota del escudo; corredor seguro vinculado a H05 de 06; reja de 08 vinculada al cuerno; y cierre temporal de arena 09 independiente de la reja ritual. El jugador interactúa con la palanca o el soporte, no con todos los barrotes; cada hoja representa el estado de su mecanismo. Pendiente: integrar vínculos, persistencia y cierre temporal sin golpear al jugador. Este montaje usa giro; una alternativa por elevación vertical exigiría cambiar pivote y animación juntos.

- Archivo: `A02-reja-vertical.png`.

### A03 Rejilla circular

Diámetro: 1,6 m. Separar aro fijo, tapa y bisagra; pivote tangente al aro, con eje horizontal. Alinear el aro con el hueco T03, a ras del suelo, y reservar un volumen vacío de barrido para que la tapa pueda abatirse. El collider de la tapa sostiene al jugador cuando está cerrada y deja libre el hueco cuando está abierta. El aro conserva su colisión estática. No usar una tapa circular para cubrir un hueco rectangular sin rediseñar el ensamblaje.

Uso de la primera versión: motivo cerrado opcional de suelo en 04 y 08. No forma parte del recorrido obligatorio. Si no participa en un descenso, conservarla fija, sin mensaje de interacción ni falsa promesa de apertura. Una rejilla funcional necesita un mecanismo de apertura y un destino seguro que forme parte del recorrido; el arte no los implementa. Pendiente: definir activador, dirección de apertura, recuperación del jugador y posibilidad de retorno. El descenso no puede conducir a pinchos que el jugador no veía antes de activarlo.

- Archivo: `A03-rejilla-circular.png`.

### A04 Portal rectangular

Paso libre: 2,6 × 3,4 m. Separar dos postes, dintel, umbral y plano de efecto. Pivote en la base central. Montar el marco en una terraza amplia, con aproximación frontal y suelo estable en ambos lados de cualquier transición. Los postes llevan colisión sólida; el plano luminoso no bloquea. Usar un volumen de activación independiente dentro del paso, dimensionado para que el jugador entienda cuándo está entrando. Las variantes activa e inactiva se producen con VFX, luz y estado del trigger, conservando la malla.

Uso: portal de vuelta a la plaza en 01, disponible desde la llegada, y portal de regreso en 09 habilitado tras la victoria. El marco del dibujo puede tener forma de arco; el recurso canónico del kit es rectangular y el plano sirve para la disposición, no para obligar a sustituirlo. Interacción propuesta: activación contextual en el paso para impedir cambios involuntarios al acercarse; requiere conectar el destino real. El portal de plaza se usa con E; LevelTeleportTrigger trabaja por contacto y no concede por sí mismo guardado ni selección de destino. Integrar la acción E prevista y fijar una aparición segura al otro lado.

- Archivo: `A04-portal-rectangular.png`.

### A05 Abertura natural

Exterior de 5 × 5 m con paso de 2,5 × 3 m. Es un cuerpo rocoso con túnel interior continuo, pivote en la base central del paso y remates compatibles con T06. Colocar la entrada en el corredor y conectar el extremo posterior a suelo físico, aunque quede oculto desde la primera cámara. La colisión debe reproducir laterales y techo; una caja global que cierre el hueco no es válida. Evitar que la transición de suelo a túnel cree un escalón invisible.

Uso: paso del corredor seguro de retorno en 06, conexión lateral 07→06 y ramal de aproximación a 08. También puede enmarcar una salida rocosa de 03 hacia 04. Es un paso físico pasivo, sin cargar otra escena ni teletransportar por defecto. Los remates se usan para ocultar la unión de túnel y pared. Pendiente: comprobar cámara dentro y fuera del túnel, lectura de la salida y ancho de circulación; si se necesita cambio de escena, se añade un sistema independiente y una señal visual específica.

- Archivo: `A05-abertura-natural.png`.

### A06 Murete modular

Tramo principal: 2 × 0,6 × 1 m. Separar tramo recto, esquina, tapa y cierre; pivote en la base central. Unir con la retícula del suelo, empleando remates donde termina una baranda de piedra. Cada tramo puede llevar una caja simple que impida caídas involuntarias donde se busca un borde seguro. No extenderlo a través de puertas, rutas de salto o el barrido de una reja. Variantes individuales: recto, esquina y remate.

Uso: borde seguro de 01, descanso y altar de 02, pequeñas zonas de espera en 03, perímetro de descanso en 08 y límites secundarios de 09. En 05–07 usarlo con moderación para no ocultar la lectura de los pozos o impedir la maniobra diseñada. Es un límite pasivo: no se activa, rompe ni trepa automáticamente. Pendiente: decidir si su altura permite saltar por encima con el controlador real; donde debe cerrar una ruta se necesita un cierre de nivel coherente, sin confiar solamente en un murete de 1 m.

- Recto: `A06a-murete-recto.png`.
- Esquina: `A06b-murete-esquina.png`.
- Remate: `A06c-murete-remate.png`.

### N01 Árbol seco central

Altura principal: 9 m; variante secundaria de 4 m. Separar tronco, raíces grandes y ramas principales. Pivote en la base del tronco. Situarlo detrás de las terrazas centrales, con el tronco formando una referencia vertical reconocible desde varios sectores. Las ramas pueden conectar visualmente con N02 sin tapar el plano de juego. La variante pequeña reutiliza lenguaje y proporciones revisadas, no exige un segundo PNG. Collider solo para la parte de tronco con la que el jugador puede chocar; las ramas del fondo no necesitan colisión.

Uso principal como hito visual entre 02, 04, 05 y 06; un árbol secundario puede enmarcar 07 u 09 si no compite con el jefe. No es un enemigo, colectable, puerta ni ruta escalable por defecto. Su función es ayudar a leer alturas y recordar orientación durante el regreso. Pendiente: validar visibilidad desde la cámara, mantener despejados apoyos y ventanas de ataque, y evitar raíces que parezcan puentes jugables sin tener suelo continuo y acceso diseñado.

- Archivo: `N01-arbol-seco.png`.

### N02 Raíces y escalera

Tramos de raíz de 2–4 m; escalera inicial de 3 m. Las raíces recta, curva y bifurcada usan pivote en el extremo de unión; la escalera emplea base central y separa largueros y peldaños. Encadenar raíces decorativas hacia pared, árbol y pilares, sin cruzar la altura útil del jugador. Para una raíz que actúa como pasarela, añadir una superficie de navegación sencilla; para la escalera, el collider y el volumen de detección dependen de la mecánica elegida, no de cada peldaño dibujado.

Uso decorativo en 01–09; uso de retorno en 07→06 y conexión lateral estable de 06. La primera versión usa una rampa T04 o apoyos T01 bajo el arte de raíces y no exige trepar. Distinguir raíces decorativas sin collider, masas sólidas y suelo de navegación. El controlador dispone de escalada por tag Ladder, pero una alternativa futura necesita adaptar orientación, entrada, salida y cámara; no basta con colocar la malla N02d sobre un desnivel.

- Recta: `N02a-raiz-recta.png`.
- Curva: `N02b-raiz-curva.png`.
- Bifurcada: `N02c-raiz-bifurcada.png`.
- Escalera: `N02d-escalera-raices.png`.

### N03 Estalactitas decorativas

Altura: 1–3 m. Separar racimo de techo y pieza individual, con pivote en el centro superior del anclaje. Colocar sobre techo o en fondo alto, reservando espacio suficiente para que no entren en la cámara durante los saltos. Las variantes de racimo y pieza corta se construyen con el mismo recurso. En fondo no llevan collider ni volumen de daño; si una formación fija queda al alcance del jugador, utilizar una colisión estática sencilla y visible.

Uso en el techo de 01–09, especialmente en los grandes vacíos de 05 y 06 y el fondo de 09. Son decoración inmóvil, no peligros que se desprenden; H04 emplea una pieza y señal de actividad diferenciadas. No colocarlas directamente sobre el recorrido con el mismo lenguaje de aviso que las trampas. Pendiente: verificar que el jugador identifica cuáles pueden caer y que la densidad de techo no oculta péndulos, plataformas, jefe o sombras de advertencia.

- Archivo: `N03-estalactitas-decorativas.png`.

### N04 Vegetación de altar

Altura inicial: 0,8 m. Separar hojas grandes, brote y base de raíces; pivote en la base central. Montar un tufo detrás o a los lados del pedestal de 02, dejando despejadas la pieza recogible y la aproximación. Reutilizar hojas sueltas para unir el altar con el suelo. Normalmente no lleva collider: si las hojas son decorativas, el jugador no debe quedarse atrapado en ellas ni perder visibilidad del objeto.

Uso principal en 02, acompañando la semilla de doble salto; pequeñas repeticiones pueden señalar un santuario seguro en 08, sin copiar la intensidad visual de una mejora pendiente. Es decoración que orienta; no es una planta cosechable, una curación ni otra semilla. La versión completa y las hojas sueltas son variaciones del recurso. Pendiente: comprobar contraste con O03, mantener visible la superficie donde aterriza el jugador y no añadir una interacción por cada hoja.

- Archivo: `N04-vegetacion-altar.png`.

### N05 Piedra de luz

Tamaño: 0,4–1 m. Separar roca soporte, mineral y efecto de brillo; pivote en la base de roca. Montar en nichos, margen de suelo o fondo con emisión tenue; compartir material y limitar luces reales para evitar coste innecesario. La variante pequeña y mediana se obtiene por escala y composición del mismo recurso. No necesita collider en fondo; una roca sólida que invada navegación sí requiere una colisión simple.

Uso en 01–09 como referencia de profundidad, giro o aproximación, con más continuidad junto a corredores estables. Su luz no es tan intensa como la de una mejora, el soporte del cuerno o un portal activo. No se recoge ni se activa y no transmite por sí sola que un área sea un checkpoint. Pendiente: comprobar lectura en escala de grises y cámara, limitar destellos que compitan con H04 y evitar que el jugador confunda minerales de fondo con recompensas.

- Archivo: `N05-piedra-luz.png`.

### H01 Pinchos minerales

Módulo de base 2 × 2 m, con puntas de 0,7–1,2 m. Separar base y cinco pinchos; pivote en el centro superior de la base. Encadenar módulos sobre el fondo de los pozos, dejando coherentes las uniones. El volumen que causa daño es simple e independiente de las puntas visuales, colocado donde el contacto resulte comprensible. No usar decenas de colliders por cada detalle ni extender el trigger de daño más allá de la silueta visible.

Uso: pozo del paso 05, fondo del derrumbe 06 y último cruce hacia o desde 07. Puede aparecer en el fondo de otras terrazas como continuidad visual, pero solo los módulos de peligro activos causan daño. Se interactúa evitándolo mediante salto, doble salto e impulso; el contacto produce la consecuencia que se acuerde para caídas. Pendiente: fijar daño, reinicio y punto seguro de recuperación; probar invulnerabilidad temporal y evitar daño encadenado al reaparecer. No colocar pinchos invisibles en la llegada de plataformas o detrás de rejas.

- Archivo: `H01-pinchos-minerales.png`.

### H02 Péndulo de piedra

Peso de 1,2 × 1,8 × 0,6 m, suspensión de 4 m. Separar anclaje, eje, suspensión y peso; pivote en el eje horizontal del techo. El peso debe colgar del eje a la distancia prevista; girar el conjunto móvil, manteniendo el soporte fijo. Construir el collider de golpe alrededor del peso y mantener la suspensión como detalle visual salvo necesidad específica. La referencia de reposo y extremos de balanceo describe poses del mismo ensamblaje, no tres piedras distintas.

Uso obligatorio en 05: dos péndulos con fases desfasadas, un apoyo estable antes, otro entre ambos y uno después. La primera pasada no comparte este reto con un enemigo nuevo. El jugador observa el ritmo, espera fuera del barrido y cruza cuando el peso se aleja; no hay acción contextual. Pendiente: implementar movimiento, tiempos, daño y reinicio de fases; comprobar el volumen barrido y la visibilidad de ambos extremos. El apoyo de espera no puede quedar dentro del collider del peso ni obligar al jugador a saltar sin ver el siguiente suelo.

- Archivo: `H02-pendulo-piedra.png`.

### H03 Losa que cede

Tamaño: 2 × 2 × 0,5 m. Separar dos apoyos, losa intacta y fragmentos; el pivote principal queda en el centro de la losa y los fragmentos tienen pivotes locales. Montar la pieza sobre un vano de 06, enrasada con el suelo estable. Usar un collider de suelo limpio mientras la losa existe y un volumen de detección para identificar que el jugador la pisa; la transición a rotura retira la superficie funcional. Las piezas de escombros pueden ser visuales y no deben convertirse en obstáculos que atrapen al jugador.

Uso en 06: un módulo aislado que enseña el aviso, seguido por una secuencia de tres módulos con refugios visibles. Los PNG intacto, agrietado y fragmento representan estados o piezas del mismo mecanismo. Flujo propuesto: estable → pisada → aviso de grietas/sonido → caída → hueco abierto; la recuperación depende del sistema acordado. Pendiente: implementar retraso, reaparición, consecuencias de caída y persistencia de la secuencia. El retorno posterior usa un corredor estable; ningún checkpoint puede registrarse sobre una losa que desaparece.

- Intacta: `H03a-losa-intacta.png`.
- Agrietada: `H03b-losa-agrietada.png`.
- Fragmento: `H03c-fragmento-losa.png`.

### H04 Estalactita que cae

Piedra de 1,5 m y anclaje de techo de 0,5 m. Separar soporte fijo, piedra desprendible, restos y VFX de aviso; pivote de la piedra en su centro superior. Colocar el soporte en techo real y el punto de impacto sobre el suelo del recorrido. El collider dañino acompaña a la piedra durante la caída; el aviso de sombra ocupa la superficie de impacto antes de que se active el daño. Los restos finales usan colisión reducida o ninguna si bloquearían la circulación.

Uso: peligro anunciado en 06, separado de la primera enseñanza de la losa, y dos impactos convocados por un ataque propuesto del jefe en 09. Estados: anclada → aviso → desprendimiento → caída → restos. En 09 el aviso debe permitir abandonar la zona sin empujar al jugador contra una puerta cerrada. Pendiente: integrar detonación, tiempo de aviso, caída, alcance del daño y limpieza de restos; el ataque del jefe todavía exige su propia lógica. No convertir los racimos decorativos N03 en trampas indistinguibles.

- Archivo: `H04-estalactita-caida.png`.

### H05 Palanca de atajo

Base de 0,8 m y mango de 0,6 m. Separar base fija, eje, mango y agarre; pivote del mango en el eje horizontal. Montarla sobre suelo estable en el tramo seguro de 03, junto a la vista de la reja A02 que controla, con margen para acercarse y ver la apertura. El collider de la base es simple; un trigger independiente establece la distancia de interacción. El mango gira entre inactivo y activado sin desplazar la base ni el punto de lectura.

Uso funcional principal: abrir desde 03 el retorno 03↔01; tras activarse debe conservar el estado y permitir recorrerlo en ambos sentidos. Una segunda instancia abre el corredor seguro de 06 al finalizar el tramo; utiliza un flag distinto al del atajo alto. El jugador activa la palanca una vez, recibe sonido y animación y ve qué acceso cambió. Pendiente: integrar input, vínculo a la puerta, persistencia y condiciones de acceso. No ubicar el activador del atajo en el lado al que todavía no se puede llegar.

- Archivo: `H05-palanca-atajo.png`.

### O01 Brazaletes de impulso

Tamaño: 12–15 cm por brazalete. Separar izquierdo y derecho; el borde metálico puede ser otra submalla si facilita materiales. El pivote se sitúa en el centro de la abertura de muñeca. Para exhibirlos, usar O04a con O04b, dejando los dos visibles y sin fusionarlos con el soporte. Si se equipan en las muñecas, necesitan anclajes del rig; si solamente se representan en el altar, su función puede conservarse como visual del objeto de mejora existente. El trigger de recogida va en el conjunto de hallazgo, no en cada brazalete pequeño.

Uso principal en 03 para adquirir el impulso. Este montaje conserva tres niveles: usar otros dos hallazgos en 04, situando el último en un nicho seguro anterior al guardián de escudo; todos deben ser alcanzables con las habilidades ya obtenidas. Las variantes de nivel usan marcas sencillas o material, no nuevas siluetas incompatibles. Pendiente: verificar el sistema real de impulso y el nivel que rompe la defensa; nunca ubicar esa mejora detrás del enemigo que requiere dicho nivel. La recogida debe ser única y persistente.

- Izquierdo: `O01a-brazalete-izquierdo.png`.
- Derecho: `O01b-brazalete-derecho.png`.

### O02 Cuerno curvo

Longitud: 45 cm. Separar cuerpo hueco, boquilla y banda. Pivote principal en el centro de masa y anclaje específico en la boquilla para animación o interacción. Presentarlo sobre O04a+O04c en suelo estable de 07, con la curva reconocible desde la llegada. El volumen de recogida pertenece al altar y debe abarcar una aproximación cómoda; no exigir tocar exactamente la boquilla. El modelo es de hueso mate, con sujeción sencilla y sin añadir motivos culturales no documentados.

Uso de progresión: recogerlo en 07 y activar el soporte de la reja en 08; no se consume y se conserva al morir. La guía propone que el inventario conserve la posesión aunque una representación visual aparezca en el soporte, evitando duplicar o perder el objeto. Interacción sugerida: recoger con la acción contextual y, en 08, activar el soporte al poseerlo, con sonido y apertura visibles. Pendiente: implementar estado de posesión, input, vínculo a reja, feedback y guardado; es una mecánica nueva, no una función garantizada por el PNG.

- Archivo: `O02-cuerno-curvo.png`.

### O03 Semilla de doble salto

Tamaño: 18 cm. Separar semilla, tres valvas y VFX de brillo; pivote en el centro del objeto. Colocarla en O04a+O04d, rodeada por N04 en el altar lateral del santuario 02. El objeto permanece claramente visible por encima del soporte; las raíces no tapan la aproximación. El collider funcional es un volumen de recogida en el conjunto, no una simulación rígida del pequeño núcleo. Cerrada y abierta son estados visuales de la misma semilla.

Uso en 02 como envoltura artística del objeto de doble salto existente. Debe alcanzarse con salto simple o caminando; el primer ensayo posterior se hace sobre suelo seguro y sin pinchos. Después de obtenerla, el altar queda vacío o cambia de estado para no prometer una segunda mejora. Pendiente: conectar el visual al sistema real de recogida, conectar la activación por E indicada en esta guía y mostrar el control correcto del segundo salto; no crear una segunda mecánica que compita con el pickup existente.

- Archivo: `O03-semilla-doble-salto.png`.

### O04 Pedestal de hallazgos y soportes

Base común de 1,5 × 1 × 0,9 m. Separar mesa y tres soportes intercambiables de hallazgo; el disco de descanso es una pieza independiente. Pivote de la base en el centro inferior y anclaje superior de soporte bien definido. Ensamblajes: O04a+O04b+O01 para brazaletes; O04a+O04c+O02 para cuerno; O04a+O04d+O03 para semilla; O04e-v2 directamente sobre suelo estable para descanso. El disco no se monta encima de la mesa de hallazgos. El collider de la mesa es simple y sólido; el trigger funcional de cada ensamblaje es otro componente, sin vincular automáticamente todos los soportes a la misma acción.

Uso: semilla y descanso diferenciados en 02; brazaletes en 03 y los dos niveles siguientes en 04; cuerno en 07; soporte receptor del cuerno y descanso diferenciados en 08. Mantener espacio frontal libre, el objeto sobre el soporte apropiado y una distancia clara entre altar de progreso y punto seguro. El soporte de descanso representa una propuesta de guardado o recuperación que aún requiere integrar estado y reglas; no tratar el modelo como un checkpoint funcional. Referencia canónica de descanso: `O04e-soporte-descanso-v2.png`; el archivo anterior sin `-v2` es histórico y se excluye del montaje.

- Base: `O04a-pedestal-base.png`.
- Soporte de brazaletes: `O04b-soporte-brazaletes.png`.
- Soporte de cuerno: `O04c-soporte-cuerno.png`.
- Soporte de semilla: `O04d-soporte-semilla.png`.
- Soporte de descanso vigente: `O04e-soporte-descanso-v2.png`.

### C01 Centinela de fragmentos

Altura: 1,7 m. Separar cuerpo articulado, placas y juntas de raíz, con pivote en el suelo entre los pies. Preparar rig y puntos de ataque compatibles con el enemigo cuerpo a cuerpo existente; las placas pueden cambiar material o estado visual de daño sin alterar la navegación. Utilizar collider de cuerpo simple y volúmenes de impacto separados, ajustados al golpe anunciado. El modelo conceptual no incluye animaciones, IA ni configuración de combate.

Uso: un encuentro individual en suelo ancho de 03, después del ensayo de impulso; pareja espaciada en 04; uno antes de la prueba final de 07, separado del aterrizaje y del pedestal. Flujo esperado: reposo → detección → preparación visible del golpe → ataque → recuperación → vuelta al seguimiento o derrota. El jugador puede observar, esquivar o usar su combate existente durante la recuperación. Pendiente: ajustar detección, patrulla, daño y límites de suelo; no colocarlo donde pueda atacar desde fuera de cámara durante el primer péndulo o empujar al jugador al aparecer.

- Archivo: `C01-centinela-fragmentos.png`.

### C02 Vigía de raíces

Altura: 1,8 m. Separar cuerpo, arco y flecha. Pivote principal en el suelo entre los pies, anclaje del arco en mano y punto de salida del proyectil independiente. El cuerpo utiliza una colisión simple compatible con el arquero existente; la flecha necesita su propio volumen de impacto y ciclo de vida, mientras el arco visual no causa daño por contacto. La postura de reposo del recurso no sustituye apuntado, disparo y recuperación.

Uso inicial solo en 04, sobre apoyo ancho y visible, con un pilar o masa de roca que permita acercarse usando cobertura. Colocarlo después de presentar al centinela cuerpo a cuerpo, sin mezclar aprendizaje de disparos con primer salto de péndulo. Flujo: detecta → apunta de forma legible → dispara → recupera. El jugador evita el proyectil o se protege y alcanza su posición. Pendiente: línea de visión, alcance, velocidad y daño reales; las flechas no deben atravesar cobertura ni llegar desde fuera de cámara cuando el jugador está aprendiendo otro mecanismo.

- Cuerpo: `C02a-vigia-cuerpo.png`.
- Arco: `C02b-arco-vigia.png`.
- Flecha: `C02c-flecha-vigia.png`.

### C03 Guardián de escudo

Altura: 2,1 m. Separar cuerpo, escudo, agarre y fragmentos; pivote del cuerpo en suelo entre los pies y del escudo en el agarre. El escudo intacto y agrietado son estados de una pieza intercambiable; los fragmentos sirven para la rotura, sin duplicar una segunda defensa invisible. Configurar collider de cuerpo, defensa y ataque como funciones distintas. La masa frontal debe ser legible, con ambos brazos articulados y margen para la animación de carga.

Uso al final de 04 en superficie amplia y despejada, después de los hallazgos de impulso que el sistema necesita. Flujo propuesto sobre la función existente: patrulla → prepara carga → carga → recuperación; la tercera cadena de una misma secuencia rompe la defensa y expone el cuerpo. Los tres hallazgos previos garantizan esa capacidad. Pendiente: restringir patrulla, comprobar colisiones contra paredes y precipicios y conectar estado del escudo y salida; garantizar un espacio donde leer y esquivar la carga.

- Cuerpo: `C03a-guardian-escudo-cuerpo.png`.
- Escudo intacto: `C03b-escudo-intacto.png`.
- Escudo agrietado: `C03c-escudo-agrietado.png`.
- Fragmento de escudo: `C03d-fragmento-escudo.png`.

### C04 Custodio de raíces

Altura: 1,8 m. Separar cuerpo, rostro y bastón; pivote del personaje en el suelo entre los pies y anclaje del bastón en el agarre. La postura tranquila y las manos abiertas deben distinguirlo de los enemigos. Utilizar collider corporal simple si se quiere que ocupe espacio; el trigger de conversación va separado y sin colisión ofensiva. El bastón es accesorio, no arma ni activador de combate. Reposo y conversación son estados de animación propuestos.

Uso en 02 junto a la aproximación al altar, apartado del punto de recogida, y reaparición opcional en 08 para recordar la función del cuerno. La conversación es breve y opcional: orienta hacia el doble salto o el soporte, sin convertirlo en llave obligatoria ni crear un sistema de misiones adicional. Pendiente: integrar diálogo y input si se decide conservarlo; si ese sistema no está disponible, una indicación ambiental debe permitir completar el nivel. Su presencia no bloquea puertas ni cambia la persistencia del cuerno.

- Cuerpo: `C04a-custodio-cuerpo.png`.
- Bastón: `C04b-baston-custodio.png`.

### C05 Guardián del fondo

Altura: 4,2 m. Separar cuerpo, núcleo, hombros, antebrazos y raíces dorsales; pivote principal en suelo entre los pies y anclaje propio del núcleo. Preparar articulación robusta y colisiones simples: masa corporal, volúmenes de ataque y zona vulnerable del núcleo son componentes distintos. Las raíces de espalda no deben tapar el rostro ni extender daño más allá de un ataque anunciado. Núcleo protegido/expuesto, daño y derrota son estados visuales de un mismo jefe, sin que el PNG proporcione su lógica.

Uso exclusivo en 09, sobre arena estable alrededor de 18 × 16 m como punto de partida, con dos apoyos laterales y espacio suficiente para ver toda la amenaza. Ataques propuestos: barrido anunciado, golpe de suelo con onda que se salta y dos estalactitas H04 con sombra previa; después hay ventana de núcleo expuesto. En la segunda mitad combina esos ataques conocidos y comienza sin invocaciones. Pendiente: implementar jefe, ataques, ventana de daño, transición de fases, victoria, apertura y portal; la derrota recupera al jugador en 08 y no deja peligros activos en el punto de reintento.

- Archivo: `C05-guardian-fondo.png`.

### Recuento y criterio de uso

Hay 33 IDs base y 58 referencias PNG canónicas: 14 de terreno, 9 de arquitectura, 8 de naturaleza, 7 de peligros, 9 de objetos/soportes y 11 de personajes/partes. El directorio contiene 59 PNG porque conserva además `O04e-soporte-descanso.png`, reemplazado por `O04e-soporte-descanso-v2.png`. Esta diferencia no significa que existan 58 familias distintas: muchas imágenes representan partes separadas de un mismo ID.

El total no cuenta instancias de escena, estados de materiales, VFX, animaciones ni prefabs compuestos. Por ejemplo, dos péndulos usan el mismo H02; un centinela repetido no añade un recurso nuevo; el pedestal de semilla une tres IDs; y un punto seguro necesita una mecánica además de su soporte. Para producción, nombrar instancias y prefabs con prefijo `MI_` y su ID, conservando estos archivos fuente fuera de `Assets` hasta que se elijan y modelen las piezas.


## 13 Atlas de las referencias individuales

Las páginas siguientes muestran las 58 referencias vigentes, agrupadas por familia. Los rótulos están fuera de las imágenes originales. Cada miniatura corresponde al archivo enumerado en el catálogo. Las imágenes representan forma y material; las medidas, pivotes y funciones se consultan en el texto.

<!-- ATLAS_CONTENT -->
