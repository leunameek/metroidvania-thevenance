# Mundo inferior — propuesta de nivel y referencias de modelado

Propuesta del 29 de septiembre de 2026 para **El asedio de Bacatá**, basada en el boceto de recorrido y la lámina visual entregados por el usuario. Los nombres nuevos, habitantes y usos de los objetos son decisiones de ficción para el juego. Las imágenes son referencias conceptuales; las dimensiones y conexiones descritas aquí guían el bloqueo del nivel y deben comprobarse jugando.

## Idea del mundo

Una gran caverna de raíces antiguas y piedra azulada, organizada en terrazas de tres alturas. El jugador desciende, gana movilidad, abre conexiones de retorno y llega al guardián del fondo. La silueta dominante es la raíz vertical; la horizontal corresponde a los caminos. Los arcos y rejas señalan cambios de sala. La cámara mantiene la lectura del siguiente apoyo y del suelo de llegada.

Conservar del boceto: entrada arriba a la derecha; brazaletes arriba a la izquierda; pruebas centrales de péndulos, saltos y pinchos; sección de derrumbe; cuerno abajo a la izquierda; antesala y jefe abajo a la derecha; conexiones de regreso. El dibujo permite varias lecturas: el orden siguiente es una propuesta explícita, no una transcripción exacta de sus notas.

## Recorrido

**01 → 02 → 03 → 04 → 05 → 06 → 07 → 08 → 09.**

| Sala | Organización y objetivo | Habitantes / riesgo | Salida y retorno |
|---|---|---|---|
| 01 Umbral | Terraza superior derecha, arco de llegada, vista hacia la raíz central. Aprender la orientación del mundo. | Espacio seguro. | Camino normal a 02 y portal de vuelta a la plaza siempre disponible. |
| 02 Santuario de raíces | Isla central ancha conectada a roca; altar con semilla que concede doble salto. Primer ensayo sin pinchos. | Custodio de raíces, personaje de orientación opcional. | 01↔02 abierto; escalones hacia 03 después de obtener la mejora. |
| 03 Galería de brazaletes | Terraza superior izquierda, apoyos escalonados, altar al final. Obtener el impulso. | Un centinela cuerpo a cuerpo en suelo ancho, después del ensayo. | Bajada a 04; se abre desde aquí un atajo de regreso a 01. |
| 04 Patio de centinelas | Superficie de combate amplia con dos pilares y entradas visibles. Ensayar el impulso y enfrentarse al escudo. | Cuerpo a cuerpo y arquero en encuentros separados; guardián con escudo al final. | Puente a 05 y camino de regreso a 03. |
| 05 Paso de péndulos | Apoyos sobre un pozo con pinchos, dos péndulos desfasados y descanso entre ambos. | Solo peligro de navegación en la primera pasada. | Rampa a 06; regresar por el mismo paso es posible. |
| 06 Galería del derrumbe | Puente agrietado con refugios laterales. Primero un módulo aislado, luego secuencia de tres. | Plataformas que ceden y estalactitas señalizadas. | Escalones a 07; corredor lateral seguro de retorno tras superar el tramo. |
| 07 Cámara del cuerno | Ramal inferior izquierdo sobre el último pozo. Pedestal estable para inspeccionar el objeto. | Un centinela antes de la prueba, nunca en el punto de aterrizaje. | El cuerno activa el cierre de 08; escalera de raíces regresa a 06. |
| 08 Antesala | Terraza inferior central-derecha; descanso, vista parcial del jefe y soporte del cuerno junto a la reja. | Custodio reaparece solo si se desea una indicación breve. | Acceso a 09 tras colocar/activar el cuerno; regreso libre a 06. |
| 09 Cámara del guardián | Recinto inferior derecho con suelo amplio, puerta a la espalda y dos apoyos laterales. | Jefe de piedra y raíces. | Victoria abre la salida y el portal de retorno. Derrota permite reintentar desde 08. |

Los enlaces son recorridos físicos. El plano conceptual puede simplificarlos: al hacer el graybox comprobar las entradas de cada sala, los cambios de altura y que ninguna flecha atraviese una pared.

## Mejoras y bloqueos

- **Semilla de raíces:** envoltura visual propuesta para el objeto de doble salto existente. La mejora se recoge en suelo alcanzable con salto simple; los primeros apoyos enseñan su uso sin daño por caída.
- **Brazaletes:** envoltura visual propuesta del impulso existente. Si la progresión conserva tres niveles de impulso, colocar tres hallazgos en 03, 04 y un nicho seguro anterior al escudo; confirmar que el jugador llega al escudo con el nivel que permite romperlo. No cerrar una salida que exija una mejora situada detrás de ella.
- **Cuerno:** nuevo objeto de progresión, no consumible. Al adquirirlo permite activar la reja de 08. Se conserva al morir. Es una propuesta de mecánica pendiente de implementación.
- **Atajos:** 01↔02 permite volver pronto a la plaza; 03→01 se abre desde 03 y después funciona en ambos sentidos. El retorno lateral de 06 evita repetir el derrumbe. Ningún acceso depende de un consumible agotable.
- **Descanso:** puntos seguros propuestos en 02 y 08; guardar estado de mejoras, cuerno y atajos. El guardado existente de la plaza no equivale a tener estos puntos implementados.

## Lenguaje visual

| Elemento | Tratamiento |
|---|---|
| Piedra principal | Basalto gris azulado `#666A7D`; planos grandes, pocas grietas, bordes rotos. |
| Caminos transitables | Cara superior azul verdosa `#6B97A4`; borde sólido y despejado. |
| Profundidad | Gris casi negro `#252630`, niebla azul `#8CBCCC` en capas alejadas. |
| Raíces | Marrón apagado `#655943`, tronco nudoso y ramificaciones grandes. |
| Hallazgos | Hueso `#DDD6B8`, metal mate `#A68C50`, foco cálido pequeño. |
| Amenazas | Silueta afilada, base oscura y aviso visible de movimiento; el color complementa la forma. |

Estilo cercano a la segunda referencia: contorno legible, formas sencillas y superficies de color amplio. No saturar con símbolos, cristales, decoraciones o luces de igual intensidad. El suelo navegable debe destacar del fondo aun en escala de grises. Los motivos geométricos nuevos son ornamentación ficticia.

Tres capas: primer plano con raíces escasas que no tapan los saltos; plano jugable con terrazas y puertas; fondo con pared de cueva, árbol antiguo, bruma y puntos de luz. Las rejas oscuras y los arcos de dovelas del dibujo son la referencia formal, sin añadir torres, castillos ni armaduras europeas.

## Kit de modelado

Unidades iniciales: 1 unidad de Unity = 1 metro; referencia humana de 1,75 m; retícula horizontal de 2 m. Estas medidas son de producción, no distancias finales de salto. Las láminas incluyen vistas de estudio; resolver cualquier diferencia entre vistas manteniendo el volumen más simple y la superficie superior indicada aquí.

| ID | Pieza base | Tamaño inicial / variantes | Uso y separación |
|---|---|---|---|
| T01 | Plataforma pequeña | 2×2×1 m; borde recto o roto. | Apoyo de salto; cara superior plana. |
| T02 | Terraza larga | 8×4×2 m; extensiones de 2 m. | Camino y salas; soportada por pared o pilar. |
| T03 | Plataforma con hueco | 4×4×1,5 m; hueco Ø1,6 m. | Descenso; aro y tapa independientes. |
| T04 | Rampa de roca | 4×4 m, desnivel 2 m inicial. | Conectar terrazas; ajustar pendiente al controlador. |
| T05 | Pilar / apoyo de roca | 2×2×4 m; alturas 2/4/6 m. | Sostener plataformas y marcar profundidad. |
| T06 | Pared de caverna | 4×6×1 m; recta/esquina/remate. | Cerrar recorrido; cara jugable limpia. |
| T07 | Puente de losas | 2×4×0,6 m por tramo. | Unión estrecha con apoyos y juntas claras. |
| T08 | Estalagmita / roca suelta | Roca 0,5–1,5 m; formación 1–3 m. | Fondo y borde; variante peligrosa en H01. |
| A01 | Arco simple de dovelas | Hueco 2,4×3,2 m. | Entrada de sala; duplicar para arco doble. |
| A02 | Reja vertical | 2,2×3 m; marco separado. | Puerta móvil; pivote abajo/centro según apertura. |
| A03 | Rejilla circular | Ø1,6 m. | Tapa de T03; pivote en bisagra. |
| A04 | Portal rectangular | Hueco 2,6×3,4 m. | Paso entre mundos; plano de efecto independiente. |
| A05 | Abertura natural | Exterior 5×5 m; hueco 2,5×3 m. | Transición rocosa y corredor de retorno. |
| A06 | Murete modular | 2×0,6×1 m. | Límite visible en zonas seguras, remate independiente. |
| N01 | Árbol seco central | 9 m alto; versión pequeña de 4 m. | Hito principal; tronco y ramas gruesas. |
| N02 | Raíz / escalera de raíces | Tramos 2–4 m. | Guiar al jugador y recuperar desniveles. |
| N03 | Racimo de estalactitas | 1–3 m alto. | Techo; nunca confundir con H04 activo. |
| N04 | Vegetación del altar | 0,8 m; hojas rígidas y brotes. | Señalar santuario y semilla. |
| N05 | Piedra de luz | 0,4–1 m, emisión tenue. | Puntos del fondo; no convertirla en coleccionable. |
| H01 | Pinchos minerales | Base de 2×2 m; puntas 0,7–1,2 m. | Pozo; collider simple separado del modelo. |
| H02 | Péndulo de piedra | Peso 1,2×1,8×0,6 m; cuerda/raíz 4 m. | Pivote en techo; peso, suspensión y soporte separados. |
| H03 | Losa que cede | 2×2×0,5 m. | Suelo agrietado, intacto/aviso/roto; fragmentos aparte. |
| H04 | Estalactita que cae | 1,5 m, base de techo 0,5 m. | Estado fijo, desprendimiento y restos; sombra de aviso. |
| H05 | Palanca y cierre | Base 0,8 m; mango 0,6 m. | Abre atajo; mango articulado y puerta reutilizada A02. |
| O01 | Brazaletes de impulso | Par de 12–15 cm, abiertos. | Izquierdo/derecho; mismo lenguaje de la referencia. |
| O02 | Cuerno curvo | 45 cm. | Hueso mate, boquilla y sujeción sencillas; sin motivos atribuidos. |
| O03 | Semilla de doble salto | 18 cm. | Núcleo cálido y envoltura de raíz; efecto aparte. |
| O04 | Pedestal de hallazgo | 1,5×1×0,9 m. | Malla base común; soportes intercambiables para O01–O03. |
| C01 | Centinela de fragmentos | 1,7 m. | Enemigo cuerpo a cuerpo; silueta delgada. |
| C02 | Vigía de raíces | 1,8 m. | Arquero; arco de raíz y proyectil separados. |
| C03 | Guardián de escudo | 2,1 m. | Enemigo robusto; escudo rompible como pieza aparte. |
| C04 | Custodio de raíces | 1,8 m. | Personaje pacífico; bastón, manos abiertas y postura tranquila. |
| C05 | Guardián del fondo | 4,2 m. | Jefe; núcleo, brazos y raíces dorsales separados para animación. |

Total: **33 piezas base**, con variantes por reutilización. El arco doble reutiliza A01; la plataforma con rejilla combina T03+A03; la puerta de jefe combina A01+A02; el altar de cada mejora combina O04 y su soporte. Niebla, halo de portal, brillos y sombra de aviso son efectos independientes y no necesitan malla detallada.

## Enemigos y personaje

**C01 Centinela de fragmentos:** cuerpo de roca con juntas de raíz; cabeza sencilla, brazos largos y torso estrecho. Se despierta al aproximarse, anuncia un golpe corto y deja recuperación. Usar la función del enemigo melee existente. Introducción individual en 03; pareja espaciada en 04; uno antes de 07.

**C02 Vigía de raíces:** figura alta con arco claramente visible, capucha hecha de corteza y núcleo tenue. Se queda en un apoyo ancho y visible, apunta antes del disparo y permite acercarse por una cobertura. Usar el arquero existente; colocación inicial solo en 04. Evitar disparos desde fuera de cámara durante el aprendizaje de péndulos.

**C03 Guardián de escudo:** masa frontal ancha y escudo de piedra estratificada. Patrulla, prepara una carga y queda expuesto después. Reutiliza la función existente de escudo/carga y el nivel de impulso que rompe su defensa. Arena de 04 despejada; restringir patrulla y corregir colisiones antes de usarlo junto a paredes o precipicios.

**C04 Custodio de raíces:** personaje ficticio sin atribución a una deidad. Aspecto de corteza y fibras, rostro de piedra claro, sin arma ofensiva. En 02 orienta hacia la mejora; en 08 puede recordar la función del cuerno. Conversación opcional de una o dos líneas, sin sistema de misiones nuevo obligatorio.

**C05 Guardián del fondo:** jefe ficticio surgido del mismo kit de roca y raíces. Cabeza pequeña, hombros grandes y núcleo frontal de color hueso: masa distinta a los enemigos normales. Propuesta de tres ataques: barrido frontal anunciado, golpe al suelo con onda que se salta y llamada de dos estalactitas con sombra previa. Tras cada secuencia, ventana de exposición del núcleo. Segunda mitad combina ataques conocidos; no añade pinchos al lugar donde reaparece el jugador. Comenzar sin invocaciones para que el combate sea legible.

## Reglas de pruebas y de cámara

- Presentar un peligro aislado, repetirlo y después combinarlo. No introducir un enemigo nuevo en el mismo salto que un mecanismo nuevo.
- Cada péndulo tiene un apoyo de espera antes y después. Cada derrumbe tiene un refugio visible. Los puntos de llegada no contienen enemigos ni pinchos ocultos.
- Los saltos se ajustan con la cápsula y las variables reales del controlador. Medir salto simple, doble salto e impulso por nivel; usar márgenes de llegada, sin asumir distancias por la imagen.
- La cámara del prototipo permite movimiento 3D en XZ. La vista isométrica de las referencias es para comunicar volumen; no supone cambiar los controles ni fijar una cámara nueva.
- Usar corredores de unos 4 m y salas de combate de unos 10×10 m como inicio. Arena del jefe alrededor de 18×16 m, revisada según cámara y alcance de ataques.
- Evitar raíces entre cámara y apoyos; atenuar u ocultar el techo si obstruye. Mostrar el suelo al otro lado de una puerta antes de atravesarla.
- Las caídas recuperan un punto seguro sobre suelo estable, nunca una losa que ya desapareció. Acordar daño y persistencia al integrar; no copiar automáticamente los valores del prototipo.

## Organización sugerida para producción

Referencias y prompts: `ArtSource/Worlds/MundoInferior/`. Mantenerlos fuera de `Assets` hasta elegir y modelar las piezas.

Al implementar: `Assets/_Game/Art/Environments/MundoInferior/{Scenes,Prefabs,Meshes,Materials,Textures,Audio,VFX}`. Prefijos de objetos: `MI_T01_`, `MI_A01_`, etc., conservando el ID de esta tabla.

Jerarquía de escena propuesta:

```text
MI_WorldRoot
  Geometry
    Room_01_Umbral ... Room_09_Guardian
  Traversal
    JumpTrials / Ramps / Shortcuts
  Interactables
    Upgrades / Horn / Gates / SafePoints
  Hazards
    Pendulums / Collapse / Spikes / FallingStones
  Actors
    Melee / Archers / Shield / Custodian / Boss
  Atmosphere
    Background / Fog / Lighting / Audio
  CameraZones
```

Pivotes: plataformas en centro de cara superior; paredes en base; puertas según movimiento; péndulos en su eje de techo; losas en centro; personajes en suelo entre los pies. Colisiones de navegación con formas sencillas, separadas de raíces y puntas decorativas. Materiales compartidos de piedra, suelo, corteza, hueso y metal; emisión en material separado. Las superficies visibles en el mapa deben existir como suelo continuo en la escena salvo los huecos expresamente diseñados.

Orden: 1) bloquear las nueve salas y jugar recorrido y retornos; 2) medir y ajustar mejoras y saltos; 3) montar kit de terreno y puertas; 4) añadir trampas y señales; 5) colocar enemigos; 6) crear jefe; 7) decorar y comprobar cámara. La prioridad es validar el recorrido antes de detallar modelos.

## Entrega y límites

Las láminas agrupan todos los IDs del kit en celdas separadas, con vistas de volumen y estudio para modelar. Las vistas generadas requieren interpretación del artista y no sustituyen un plano técnico ni garantizan coherencia geométrica exacta entre ángulos. El plano muestra organización conceptual; el recorrido descrito en la tabla prevalece si hay diferencias gráficas.

Esta entrega define y representa el nivel. No altera escenas, controles, personajes existentes ni implementa los nuevos mecanismos en Unity. La integración y la validación del nivel jugable corresponden a una etapa posterior.
