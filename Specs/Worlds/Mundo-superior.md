# Mundo superior — boceto de ascenso
Propuesta del 5 de octubre de 2026 para **El asedio de Bacatá**. Primer diseño conceptual basado en las dos referencias del usuario. Entrega: distribución de plataformas, objetos, portales y progresión; las mecánicas nuevas se proponen para construcción posterior en Unity.

La [guía completa del mundo superior](Guia-completa-mundo-superior.md) desarrolla este boceto: fija medidas iniciales, montaje por zona, reglas propuestas de movimiento y combate, 41 referencias de modelado, cantidades, componentes, sonido, efectos y validación. Para construir el nivel, usar esa guía y su inventario como especificación detallada vigente.

![Boceto de ascenso](Mundo-superior/Boceto-ascenso-v1.png)

## Idea y lectura
Un ascenso por terrazas de piedra ocre entre nubes, con el sol y la arena de la cima como referencia de orientación. La ruta cambia de forma al adquirir habilidades: caminar y saltar al principio, activar portales, escalar una pared reconocible y volar entre islas. La cámara del juego sigue siendo 3D; la vista de la lámina sirve para explicar la distribución, no fija una cámara isométrica.

Interpreto las notas «ruina 1» y «ruina 2» del dibujo como dos **runas de habilidad**, alojadas en ruinas. Sus medallones, poderes, símbolos, alas y guardián son ficción del juego.

**Recorrido obligatorio:** 01 → 02 → P1→P2 → 03 → P2→P1 → 02 → escalar → 04 → volar → 05 → P3→P4 → 06 → plataforma móvil → 07 → 08.

El retorno de 03 a 02 es breve y deliberado: desde 02 se ve una pared que todavía no se puede escalar; la runa de 03 permite volver y usarla. Así la habilidad abre una ruta conocida y conduce a las alas. El ascenso general culmina en la arena superior.

## Estaciones y objetos
| Zona | Posición en la lámina | Qué contiene | Progresión y acceso |
|---|---|---|---|
| 01 Umbral | Abajo a la izquierda | Llegada, retorno a la plaza y descanso | Suelo ancho y seguro; escaleras normales a 02. |
| 02 Terraza de la primera runa | Sobre 01, a la izquierda | Runa 1, Yopo 1 opcional y portal rosado P1 | La runa activa los portales del nivel. P1 lleva a P2 en 03. La pared detrás de 02 queda bloqueada hasta conseguir Runa 2. |
| 03 Ruina de escalada | Isla inferior derecha | P2 y Runa 2 sobre suelo fijo | Se alcanza usando P1; recoger Runa 2 sin escalar. Volver por P2 a P1 y subir la pared marcada de 02 hacia 04. |
| 04 Santuario de las alas | Terraza central izquierda, encima de 02 | Alas y descanso; Yopo 2 en una isla lateral | La escalada lleva al suelo del altar. Primer ensayo de vuelo a 05. Yopo 2 se alcanza y se abandona con las alas. |
| 05 Isla del portal azul | Centro, encima de 04 | P3 sobre una plataforma estable | El hueco desde 04 exige las alas. P3 lleva a P4, el portal azul en el primer apoyo del tramo de 06. |
| 06 Camino de la llave | Parte alta derecha | P4 abajo, apoyos ascendentes, llave en altar fijo y transporte al final | Volar entre apoyos desde P4 hasta la llave. La plataforma móvil conecta la terraza de la llave con 07. |
| 07 Antesala | Parte alta izquierda | Descanso y soporte de la llave junto al umbral | Registrar la llave abre el último acceso. Escalera continua hacia 08; espacio seguro para reintentar. |
| 08 Cima | Arriba | Arena ancha, guardián provisional, refugio circular y salida | El jefe cierra el ascenso. La victoria activa el retorno a la plaza. |

**Código de la lámina:** flechas doradas = movimiento físico; trazos rosados = P1↔P2; trazos azules = P3↔P4; punteado gris = ramal opcional; discos claros = descanso. Color, forma y nombres identifican cada enlace. Los portales grises de 01 y 08 son retornos a la plaza, distintos de los dos pares internos.

## Función de las recompensas y bloqueos
- **Runa 1:** activación permanente de los portales internos, con destinos fijos. Enseñar primero P1/P2 en un entorno seguro; P3 permanece fuera de alcance hasta obtener las alas.
- **Runa 2:** escalada permanente, limitada a paredes con apoyos visibles. La pared bajo 04 no se puede resolver con salto doble ni impulso. Es necesario medirlo con el controlador real.
- **Alas:** vuelo limitado por trayecto, recuperable al tocar suelo. Ensayo corto y seguro antes de P3; secuencia más larga después de P4. El tiempo de vuelo y la recuperación son propuestas nuevas, por ajustar; no usar un consumible obligatorio.
- **Yopo 1 y 2:** dos hallazgos opcionales de mejora de poder, respetando la nota del usuario. Propuesta inicial: incremento permanente de potencia de combate, sin desbloquear movilidad ni exigir reposición. Sus valores quedan pendientes. La jarra de la lámina es un marcador conceptual; el recipiente y su descripción cultural requieren una referencia específica antes del arte final.
- **Llave:** medallón ficticio de acceso al jefe. Registrar su adquisición de forma permanente; no requiere transportarla físicamente ni mantenerla en las manos. Se inspecciona sobre suelo fijo y se usa en el soporte de 07. Su colocación no puede perderse al morir.
- **Plataforma móvil:** recorrido de ida y vuelta entre dos bordes seguros, visible desde ambos. Tras registrar la llave puede servir como atajo de regreso; ningún jugador debe quedar encerrado por haberla dejado en el otro extremo.

Los dos pares de portales funcionan en ambos sentidos al activarse. La salida de 01 sigue disponible desde el inicio; la de 08 se activa con la victoria. Los objetos y las nuevas habilidades no dependen de completar primero el mundo inferior. Si el jugador trae doble salto o impulso, ajustar las paredes y huecos para preservar esta progresión.

## Construcción y recuperación
1. Construir primero las ocho terrazas, el retorno 03→02 y los dos enlaces internos. Comprobar que ningún salto normal evita Runa 2 o Alas.
2. La escalada sale directamente a 04; 05 tiene que permanecer separado de esa pared para evitar obtener P3 antes que las alas.
3. Cada tramo de vuelo tiene aterrizaje visible y suelo donde recuperar la habilidad. Colocar P4 antes de la secuencia de apoyos, nunca junto a la llave.
4. Los descansos de 01, 04 y 07 registran objetos, habilidades, llave y victoria. Caer devuelve al último apoyo seguro del tramo; no obliga a repetir el mundo completo. Distinguir esta recuperación de la derrota contra el jefe, que devuelve a 07.
5. Si el vuelo permite regresar de 05 a 04, mantener ese retorno viable. La rama de Yopo 2 necesita ida y vuelta posibles con la misma capacidad base.
6. La cámara muestra el siguiente apoyo y la salida de cada portal; no coloca el horizonte, las nubes o un pilar delante del aterrizaje.
7. El transporte está a la altura de ambos muelles; la composición de la ilustración comprime alturas y no determina sus medidas. Ajustar ancho, velocidad y trayecto con el personaje.
8. El jefe permanece en una superficie amplia. No colocar ataques de enemigos en los primeros ensayos de escalada o vuelo. Su aspecto y combate se diseñarán después.

La lámina **no está a escala**. Las distancias, paredes escalables, duración del vuelo, colliders y cámara aún necesitan pruebas en Unity. El archivo de diseño define las conexiones cuando la perspectiva artística resulte ambigua.

## Ambientación muisca
La referencia del usuario guía el contorno oscuro, la piedra arenosa dorada, los volúmenes sencillos, el cielo turquesa y las nubes crema. Para desarrollar la identidad muisca, incorporar detalles de orfebrería, recipientes cerámicos, tejidos y espacios de ofrenda con referencias identificables. El abrigo circular de madera y cubierta vegetal es una dirección conceptual que requiere una referencia arquitectónica concreta para el modelado final.

El Museo del Oro documenta tunjos, orfebrería y ofrendas muiscas; su colección incluye recipientes de cerámica con figuras de metal. Estas referencias apoyan los objetos y espacios ceremoniales, pero no prueban la existencia de las ruinas flotantes, los símbolos de las runas, las alas ni la magia del juego.

- [Banco de la República — Muisca](https://enciclopedia.banrepcultural.org/index.php?title=Muisca): contexto de ofrendas, orfebrería y prácticas rituales.
- [Museo del Oro — Ofrendatario con tapa](https://colecciones.banrepcultural.org/en/document/ofrendatario-con-tapa/63a069045d96b8790f28278e): recipiente cerámico y conjunto de figuras de metal procedente de Fontibón.
- [Banco de la República — Historias de ofrendas muiscas](https://babel.banrepcultural.org/digital/collection/p17054coll18/id/400/): investigación de objetos y contextos de ofrenda.

Consulta: 5 de octubre de 2026. La asociación **yopo = mayor poder**, el guardián y los mecanismos de este nivel son decisiones de ficción. No asignar significado histórico a las espirales o bandas ornamentales generadas.

## Archivos y procedencia
- Lámina seleccionada: [Boceto-ascenso-v1.png](Mundo-superior/Boceto-ascenso-v1.png).
- Prompt inicial y dos ajustes: [Prompt-boceto.txt](Mundo-superior/Prompt-boceto.txt) y [Prompts-ajustes.txt](Mundo-superior/Prompts-ajustes.txt).
- Herramienta: **image_gen integrada**, mediante la habilidad imagegen; tres generaciones contando los ajustes. Referencias: las dos imágenes aportadas por el usuario.
- Revisión visual: ocho estaciones numeradas, dos portales rosados, dos azules, dos runas, alas, Y1/Y2, llave sobre altar fijo, transporte, descanso y arena final. La conectividad de la lámina se revisó visualmente; no se ejecutaron pruebas jugables porque esta entrega es conceptual.
