# El asedio de Bacatá

Propuesta 1.0 | 6 de octubre de 2026

# 07 / Guion 4: encuentros y combate por voz


## Contrato de combate

Todos los encuentros hostiles de campaña son turnos reactivos con selección por voz; locomoción libre se suspende. En exploración Q sirve para movilidad, no dañar al pasar sobre un enemigo. DashHurtbox, proyectiles físicos e IA de daño se desactivan al entrar en un duelo. Solo el modelo resuelve daño. Las animaciones representan lo decidido y no añaden un segundo impacto.


## Estado y entrada

Exploración -> Presentación -> Decisión del jugador -> Ejecución -> Aviso enemigo -> Defensa -> Resolución -> siguiente decisión. Victoria, derrota, abandono y pausa son salidas explícitas. E inicia una vez desde suelo seguro; ese E no ataca. Cada turno tiene un token único y solo admite una acción ofensiva o una defensa. Las órdenes tardías con token viejo se descartan.


## Decidir y hablar

La decisión ofensiva no tiene tiempo límite. Menú muestra solo acciones legales, su efecto y objetivo. Activar escucha únicamente en decisiones y defensa; suspender durante diálogos, sonidos de NPC y cinemáticas para evitar autoactivación. La voz recibe verbos breves: atacar, bloquear, esquivar, contraatacar y las opciones de ficha. Se muestra palabra reconocida y acción antes de resolver; entrada baja/confusa pide repetición sin gastar turno.


## Dos perfiles de reacción

Perfil teclado inicial: aviso 1,8 s y ventana 2,4 s. Perfil voz propuesto: aviso 1,8 s y ventana 4,8 s para enunciar y reconocer; multiplicador de ayuda 1-3 y modo sin vencimiento disponibles. Son cifras iniciales para medir. Nada exige acertar en 0,2 s con voz. Se conserva ritmo mediante aviso, decisión y respuesta, no penalizando diferencias de acento o volumen.


## Errores de voz

Sin coincidencia no dispara una acción aleatoria. Motor no disponible o micrófono desconectado pausa la defensa y ofrece teclas antes de daño. Una palabra válida que el jugador eligió equivocadamente puede fallar según ficha. Silencio con motor operativo y ventana vencida falla si el modo tiene tiempo; en modo sin vencimiento espera. Repeticiones se filtran por token; jamás dos golpes por una frase.


## Órdenes anticipadas

Durante el aviso no se ejecuta ninguna defensa. El HUD muestra PREPARA y luego RESPONDE, pero no nombra el golpe ni la defensa (ver Lenguaje de señales). Si el recognizer entrega una frase iniciada antes de la apertura y no ofrece marca de tiempo fiable, descartar con mensaje Repite al abrir la señal; no presumir latencia compensada. Para voz, calibrar y ofrecer modo sin tiempo evita castigo. No aceptar frases en pausa ni arrastrarlas al reanudar.


## Lenguaje de señales (revisión 7 oct 2026)

El jugador interpreta al enemigo; la pantalla no le dice qué hacer. Cada defensa tiene una sola señal, igual en todos los duelos y redundante en cuerpo, sonido y suelo, más un subtítulo de sonido para jugar sin audio:

| Defensa | Señal | Cuerpo / suelo | Sonido (subtítulo) |
|---|---|---|---|
| Bloquear | Golpe de frente | Se planta y echa el peso atrás; polvo de sus pies hacia Nemequene | Tambor grave doble (Tambor grave) |
| Esquivar | Golpe que barre o embiste | Recoge el golpe a un lado; estela de aire que cruza a Nemequene | Silbido que sube (Silbido de lado) |
| Cubrir | Algo cae desde arriba | Sombra que crece bajo los pies; arenilla que cae | Crujido arriba (Crujido arriba) |
| Parar (además) | Golpe físico | Destello dorado de tumbaga en el enemigo al abrirse la respuesta | Campanilla de oro (Destello dorado) |

La señal sale de la respuesta principal de cada ficha (`DuelMove.Signal`); el destello, de golpes físicos que aceptan Parar (`DuelMove.Glint`). Los efectos propios de cada escenario (piedras de MI09, cabezas, luna/sol) se suman, no la sustituyen.

- **Aviso:** solo dice quién ataca (cabeza A/B, luna/sol), que es información táctica. Parar se ofrece siempre que haya concentración; contra un golpe sin destello es una defensa equivocada, no un rechazo que delate el golpe.
- **Error:** en el entrenamiento el resultado explica la señal que había y se mantiene 2,8 s para leerlo; en los demás combates solo se ve con la opción de accesibilidad.
- **Sin tarjetas en los combates (playtest 8 oct):** ningún combate explica la señal en pantalla, ni la primera vez; el lenguaje completo (frente, barrido, arriba y destello) se enseña solo en el entrenamiento (E01). El cuadro de anuncio/resultado del duelo solo aparece con la opción de accesibilidad.
- **Entrenamiento (E01):** cinco ataques y cuatro defensas. Las dos primeras se explican (cuerpo, sonido y respuesta); las dos siguientes (frente y luego barrido) se leen sin ayuda. Al vencer, el panel resume el lenguaje completo.
- **Sin ritmo estricto:** la voz y las manos mantienen la ventana amplia; interpretar es obligatorio, acertar el instante no.
- **Accesibilidad:** «Mostrar la defensa correcta en combate» vuelve a nombrar la respuesta en duelos y entrenamiento.
- **Intensidad (playtest 7 oct):** cada señal dura todo el aviso y crece hasta el golpe: carril de chevrones naranjas del enemigo a Nemequene con polvo continuo (frente), franja de chevrones celestes que lo cruza con estelas de aire (barrido), sombra oscura con borde violeta y escombros que caen (arriba); luz del color de la señal, el cuerpo del enemigo se inclina (atrás, de lado o hacia delante) y el sonido se repite al abrir la respuesta. El destello es una llamarada dorada con luz.
- **Todos los combates:** además de duelos y entrenamiento, las criaturas del inframundo y el centinela de escudo usan las mismas señales: mordida, dardo, grito y pulso = frente; coletazo = barrido; picada = arriba; los anillos de alcance toman el color de la señal. En tiempo real la respuesta es moverse fuera del carril, la franja o la sombra; el dorado marca el momento de impulsarse (grito interrumpible, criatura en el suelo tras la picada). Los enemigos de la escena Dev/Movement no son alcanzables en el juego y quedan fuera.


## Recursos de combate propuestos

Vida 100; restaurar en descansos y al reintentar. Concentración 0-3, empieza en 1 por encuentro y suma 1 con defensa correcta hasta máximo. Ataque base D=20 +5 por cada Yopo opcional, hasta 30. Contraatacar consume 1 y hace D+10 solo si Contraventana está lista tras defensa correcta de un golpe físico. Jaguar usa afinidad, consume 1, hace D+10 y rompe un lazo habilitado. No hay consumibles obligatorios ni curación por farmear.


## Defensa y parry

Bloquear, Esquivar y Cubrir resuelven patrones según ficha, sin gasto de concentración. Parar es parry estratégico opcional: disponible con concentración >=1 frente a golpe físico que lo permita; consume 1, da cero daño y crea Contraventana reforzada (+20 al siguiente Contraatacar en vez de +10). Usa la misma ventana accesible, no una subventana de milisegundos. Si no aplica al patrón, HUD lo indica y no lo ofrece. El siguiente turno consume o pierde esa oportunidad al elegir otra acción.


## Alternativa de entrada

Teclas en combate: 1 Atacar, 2 Contraatacar si legal, F Bloquear, Espacio Esquivar, R Parar si legal; opciones contextuales en botones numerados 3-6 visibles. La plaza puede conservar E para ataque de entrenamiento como alias. Los comandos y botones llaman al mismo modelo. El mouse y las teclas pueden completar campaña; registrar entrada real, no atribuir uso de voz/manos a la alternativa.


## Objetivos y equilibrio

Núcleos, cabezas y ataduras son objetivos del modelo seleccionados por nombre/icono. No puntería física por voz. Si se necesita objetivo, la primera palabra abre selección y no gasta acción hasta objetivo confirmado. Todos los jefes admiten victoria con D=20, sin Yopos. Afinidades esenciales se obtienen antes del requisito y no se pierden al reintentar.


## Derrota y retirada

Enemigo lleva vida a 0 -> derrota jugable -> descanso MI02/MI08/MS07 o entrada de círculo según ficha, salud 100. Reiniciar vida, fases y concentración del encuentro; conservar objetos, máscaras ya registradas, atajos y jefes vencidos. Pausa ofrece Abandonar. Homenaje y victoria no se anulan por salir del juego. La muerte de C19-20 usa estado narrativo distinto y nunca pasa por este respawn.


## Variedad y duración

Encuentros comunes enseñan una decisión contextual y un patrón, 2-4 acciones ofensivas previstas. Jefes combinan lo aprendido, no agregan palabras durante un ataque sin presentar. Menos repeticiones a cambio de cambios de postura, cubiertas, lectura de origen, interrupción y uso de máscaras. No reaparecen enemigos obligatorios vencidos en retornos. El superior añade dos encuentros de canon en anexos seguros y los separa de navegación.


## Comandos
- Atacar / Ataca / atacar / Decisión ofensiva: D al objetivo vulnerable; sin recurso.
- Bloquear / Bloquea / bloquear / Defensa: 0 daño contra patrón bloqueable; +1 concentración.
- Esquivar / Esquiva / esquivar / Defensa: 0 daño contra patrón evadible; +1 concentración.
- Cubrir / Cubrir / cobertura / Defensa a proyectil: Pilar o ala guardiana protege; 0 daño, +1 concentración.
- Parar / Parar / parry / Defensa física autorizada: Consume 1, 0 daño, contraventana reforzada.
- Contraatacar / Contraataca / contraatacar / Turno tras defensa física: Consume 1, D+10; D+20 si reforzada.
- Flanquear / Flanquear / Turno contra caimán guardado: Sin daño; quita guardia hasta próximo ataque y deja D+10 al próximo Atacar.
- Acercar / Acercar / Turno contra murciélago distante: Sin daño; elimina penalización de distancia hasta fin del encuentro.
- Impulso / Impulso / Turno frente escudo intacto: Subestado: Uno, Dos, Tres en orden, sin tiempo; cadena visual rompe escudo.
- Jaguar / Jaguar / Exposición de lazo / final: Consume 1; D+10 y rompe atadura habilitada; afinidad permanente.
- Cuerno / Cuerno / Turno de jefe inferior: Sin daño; abre núcleo del siguiente turno y evita desorientación; objetivo automático.
- Anclar / Anclar / Turno de cóndor/águila: Sin daño; usa suelo estable y anula viento de esa ronda; no altera vuelo fuera del duelo.
- Interrumpir / Interrumpir / Turno frente carga de ala: D/2 e impide próximo ataque Carga; no disponible para patrones físicos ya ejecutándose.
- Luna / Sol / Luna / Chía; Sol / Sué / Selección de objetivo del final: Elige vínculo lunar/solar; acción se confirma por un verbo posterior.
- Cabeza A / B / Cabeza A / izquierda; Cabeza B / derecha / Selección ofensiva de serpiente: Elige la cabeza señalada por el HUD; no gasta turno hasta confirmar Atacar u otra acción legal. La cámara conserva izquierda y derecha.
- Vincular / Vincular / Turno final, ambas máscaras libres: Consume 1; reduce el vínculo elegido y la vida del antagonista en D. El vínculo se apaga al llegar a 0; tras ambos, Jaguar/Atacar terminan.

## E01 / Guardián de entrenamiento

Zona: Plaza / círculo

Motivo: Enseña atención, no está corrompido.

Entrada: E en marca de círculo tras llegar; puede repetirse sin borrar progreso.

Reglas: Tres ataques del jugador y dos defensas correctas; vida de práctica no llega a derrota. Error: -10 energía y repetir exactamente la defensa. Atacar espera sin límite; luego directo y onda.

Patrón: Directo -> Esquivar; onda -> Bloquear. No introducir Parar antes de dominar ambas.

Resultado: training_complete; vida restaurada; apertura inferior si tres lecciones listas.

GUARDIÁN: Puedes esperar tu decisión.

GUARDIÁN: Directo: esquiva al abrir la señal.

GUARDIÁN: Onda: bloquea al abrir la señal.

GUARDIÁN: Una voz clara basta.

1. PG: círculo y dos marcas, sin obstáculos.
2. PM: E fija cámara y ofrece entrada por voz.
3. PM: Atacar aplica primer golpe único.
4. PG: directo anunciado, Esquivar desplaza lateral.
5. PG: segunda ronda, onda y Bloquear.
6. PM: tercer ataque completa; guardián baja arma.

**Prompt:** Producir E01 Guardián de entrenamiento en Plaza / círculo. Motivo: Enseña atención, no está corrompido. Iniciar: E en marca de círculo tras llegar; puede repetirse sin borrar progreso. Implementar reglas: Tres ataques del jugador y dos defensas correctas; vida de práctica no llega a derrota. Error: -10 energía y repetir exactamente la defensa. Atacar espera sin límite; luego directo y onda. Anunciar patrón: Directo -> Esquivar; onda -> Bloquear. No introducir Parar antes de dominar ambas. Resolver: training_complete; vida restaurada; apertura inferior si tres lecciones listas. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E02 / Custodio de Raíces

Zona: MI02; eco en MI08

Motivo: Quiere conservar un apoyo del camino, no combatir.

Entrada: E cerca o conversación opcional; cuerpo anclado a MI02.

Reglas: Sin turno ni voz; elegir preguntas por UI. Dos líneas de primera visita, hasta tres niveles de pista. Su eco en 08 procede del hilo de raíz, visible al completar cuerno.

Patrón: Presentar problema -> señalar altar -> devolver control. Si semilla ya recogida, explica galería; si cuerno en mano, explica máscara sellada.

Resultado: No premio, no misión obligatoria; mejora orientación y prepara vínculo del jefe.

CUSTODIO: No puedo dejar este apoyo.

NEMEQUENE: Entonces conservaré el regreso.

CUSTODIO: La fuerza del guardián está atada, no perdida.

1. PG: custodio a un lado del altar, manos abiertas.
2. PM: E acerca cámara sin bloquear camino.
3. PM: pregunta contextual según progreso.
4. PD: mano/bastón apunta a marca real del objeto.
5. PM: respuesta breve con subtítulos.
6. PG: cámara vuelve, raíz conecta visualmente su eco futuro.

**Prompt:** Producir E02 Custodio de Raíces en MI02; eco en MI08. Motivo: Quiere conservar un apoyo del camino, no combatir. Iniciar: E cerca o conversación opcional; cuerpo anclado a MI02. Implementar reglas: Sin turno ni voz; elegir preguntas por UI. Dos líneas de primera visita, hasta tres niveles de pista. Su eco en 08 procede del hilo de raíz, visible al completar cuerno. Anunciar patrón: Presentar problema -> señalar altar -> devolver control. Si semilla ya recogida, explica galería; si cuerno en mano, explica máscara sellada. Resolver: No premio, no misión obligatoria; mejora orientación y prepara vínculo del jefe. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E03 / Hombre-caimán / guardia terrestre

Zona: MI03 / patio tras primer impulso

Motivo: Protege una tumba; acepta prueba del visitante antes de atribuirle saqueo.

Entrada: E en borde del patio estable 10x 10 m; primer par adquirido.

Reglas: Vida 60; guardia frontal reduce Atacar a D/2. Flanquear consume turno sin daño, quita guardia hasta próximo golpe y añade +10 a ese Atacar. Alternativa lenta: seis ataques de 10; no bloqueo absoluto.

Patrón: Alterna mordida directa -> Esquivar/Parar y cola frontal -> Bloquear. Fallo 15. Defender golpe físico prepara contraataque.

Resultado: Cede paso, no muerte; resolved_caiman 03; atajo accesible sin otro combate en retorno.

CAIMÁN: Tu bastón no abre una tumba.

NEMEQUENE: Vengo a restituir un paso.

CAIMÁN, FINAL: Entonces no te quedes con lo que no te pertenece.

1. PG: caimán entre dos pilares, altar queda atrás.
2. PM: baja hocico y explica guardia frontal.
3. PD: menú muestra Flanquear y daño reducido.
4. PG: decisión desplaza a marca lateral; no WASD libre.
5. PM: defensa de cola, contraventana visible.
6. PG: criatura aparta cuerpo y permite palanca.

**Prompt:** Producir E03 Hombre-caimán / guardia terrestre en MI03 / patio tras primer impulso. Motivo: Protege una tumba; acepta prueba del visitante antes de atribuirle saqueo. Iniciar: E en borde del patio estable 10x 10 m; primer par adquirido. Implementar reglas: Vida 60; guardia frontal reduce Atacar a D/2. Flanquear consume turno sin daño, quita guardia hasta próximo golpe y añade +10 a ese Atacar. Alternativa lenta: seis ataques de 10; no bloqueo absoluto. Anunciar patrón: Alterna mordida directa -> Esquivar/Parar y cola frontal -> Bloquear. Fallo 15. Defender golpe físico prepara contraataque. Resolver: Cede paso, no muerte; resolved_caiman 03; atajo accesible sin otro combate en retorno. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E04 / Hombre-murciélago / vigía

Zona: MI04 / patio separado

Motivo: Orden forzada lo mantiene disparando contra quienes avanzan.

Entrada: E antes del segundo patio; sin proyectiles desde fuera de cámara.

Reglas: Vida 40. Distante: Atacar D/2; Acercar consume turno, deja distancia corta y Atacar D. Cubrir responde a flecha/onda de proyectiles con pilar visible; Esquivar responde a descenso físico. Atacar repetido sigue siendo viable.

Patrón: Proyectil -> Cubrir o Bloquear; descenso -> Esquivar/Parar. Fallo 15. No destruir cobertura al azar.

Resultado: Lazo menor se rompe, cae de rodillas sin desintegrarse; abre nicho de B3.

MURCIÉLAGO: La orden me arrastra.

NEMEQUENE: Puedo cortar su fuerza sin destruir la tuya.

MURCIÉLAGO: El tercer par está antes del escudo.

1. PG: vigía, pilar y suelo de aproximación.
2. PM: dos distancias señaladas por marcas en piso.
3. PD: Acercar fija marca más próxima o Atacar a distancia.
4. PG: aviso de proyectil, Cubrir lleva detrás del pilar.
5. PM: descenso conocido y defensa.
6. PG: victoria libera lazo y muestra último nicho.

**Prompt:** Producir E04 Hombre-murciélago / vigía en MI04 / patio separado. Motivo: Orden forzada lo mantiene disparando contra quienes avanzan. Iniciar: E antes del segundo patio; sin proyectiles desde fuera de cámara. Implementar reglas: Vida 40. Distante: Atacar D/2; Acercar consume turno, deja distancia corta y Atacar D. Cubrir responde a flecha/onda de proyectiles con pilar visible; Esquivar responde a descenso físico. Atacar repetido sigue siendo viable. Anunciar patrón: Proyectil -> Cubrir o Bloquear; descenso -> Esquivar/Parar. Fallo 15. No destruir cobertura al azar. Resolver: Lazo menor se rompe, cae de rodillas sin desintegrarse; abre nicho de B3. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E05 / Hombre-caimán de escudo

Zona: MI04 / cierre final del patio

Motivo: Vínculo fuerza el cierre, no escucha final de su orden.

Entrada: E solo cuando nivel 3 y tres pares registrados; aviso orienta al par pendiente.

Reglas: Escudo tiene tres uniones, vida corporal 40. Impulso abre Uno, Dos, Tres sin límite temporal; orden incorrecto vuelve al primer paso sin daño ni gastar turno ofensivo. Orden correcto ejecuta una cadena visual, rompe escudo y consume el turno. Ya sin escudo, Atacar D; dos golpes base terminan.

Patrón: Escudo intacto: carga -> Esquivar; roto: golpe -> Bloquear/Parar. Fallo 20. La cadena no depende de Q en 0,2 s ni de colisión física.

Resultado: mi_shield 04 abre reja 04-05; solo una victoria persistente.

ESCUDO: Ningún paso. Ningún...

NEMEQUENE: Escucha el final.

ESCUDO: Orden incompleta. Paso reconocido.

1. PG: escudo con tres juntas y puerta detrás.
2. PD: menú Impulso, nivel 3 confirmado.
3. PD: Uno, Dos, Tres se seleccionan en orden sin cuenta atrás.
4. PG: tres estelas en misma dirección rompen defensa.
5. PM: carga anuncia esquiva; luego núcleo vulnerable.
6. PG: dos golpes base, baja escudo y abre puerta.

**Prompt:** Producir E05 Hombre-caimán de escudo en MI04 / cierre final del patio. Motivo: Vínculo fuerza el cierre, no escucha final de su orden. Iniciar: E solo cuando nivel 3 y tres pares registrados; aviso orienta al par pendiente. Implementar reglas: Escudo tiene tres uniones, vida corporal 40. Impulso abre Uno, Dos, Tres sin límite temporal; orden incorrecto vuelve al primer paso sin daño ni gastar turno ofensivo. Orden correcto ejecuta una cadena visual, rompe escudo y consume el turno. Ya sin escudo, Atacar D; dos golpes base terminan. Anunciar patrón: Escudo intacto: carga -> Esquivar; roto: golpe -> Bloquear/Parar. Fallo 20. La cadena no depende de Q en 0,2 s ni de colisión física. Resolver: mi_shield 04 abre reja 04-05; solo una victoria persistente. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E06 / Vigía del cuerno

Zona: MI07 / aproximación

Motivo: Su orden prohíbe que alguien lleve la llamada a la máscara.

Entrada: E antes de último tramo, a suficiente distancia del aterrizaje y pedestal.

Reglas: Vida 60; mismo murciélago con variante: carga sonora visible. Atacar D; Interrumpir D/2 cancela próximo grito y evita ceguera visual, pero nunca oculta información de HUD. Si grito ocurre, siguiente ataque recibe -5 daño, mínimo 5, solo esa ronda.

Patrón: Grito cargado -> Bloquear; descenso -> Esquivar. Interrumpir cancela grito antes del aviso y no admite interrupción retroactiva. Fallo 15.

Resultado: resolved_horn_guard; camino al altar libre; no exige habilidad nueva.

VIGÍA: La llamada debe quedar aquí.

NEMEQUENE: Debe volver a quien pueda responder.

1. PG: vigía sobre patio previo al salto.
2. PM: garganta/ala prepara carga reconocible.
3. PD: Interrumpir o Atacar explican costo y beneficio.
4. PM: acción interrumpe, o grito pide Bloquear.
5. PG: descenso y Esquivar repiten patrón aprendido.
6. PG: criatura baja alas y deja ruta al cuerno.

**Prompt:** Producir E06 Vigía del cuerno en MI07 / aproximación. Motivo: Su orden prohíbe que alguien lleve la llamada a la máscara. Iniciar: E antes de último tramo, a suficiente distancia del aterrizaje y pedestal. Implementar reglas: Vida 60; mismo murciélago con variante: carga sonora visible. Atacar D; Interrumpir D/2 cancela próximo grito y evita ceguera visual, pero nunca oculta información de HUD. Si grito ocurre, siguiente ataque recibe -5 daño, mínimo 5, solo esa ronda. Anunciar patrón: Grito cargado -> Bloquear; descenso -> Esquivar. Interrumpir cancela grito antes del aviso y no admite interrupción retroactiva. Fallo 15. Resolver: resolved_horn_guard; camino al altar libre; no exige habilidad nueva. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E07 / Jefe caimán-murciélago

Zona: MI09 / arena 18x 16 m inicial

Motivo: Guardián enlazado a Chía; protege y retiene contra su voluntad.

Entrada: E con coca y Chía sellada; guarda checkpoint MI08 antes de C07.

Reglas: Vida 160; dos ataduras. Fase 1 hasta 80: Atacar D con núcleo abierto; Cuerno consume turno sin daño, abre núcleo siguiente turno aunque no haya defensa correcta. Defensa correcta también abre núcleo. Si está cerrado, Atacar hace D/2. Primer Jaguar legal con concentración rompe atadura 1 y hace D+10; segunda a vida <=80 rompe atadura 2. Para victoria requiere ambas: a vida 1 con atadura pendiente avisa y ofrece ventana, no softlock. Afinidad nunca se gasta.

Patrón: Ciclo barrido -> Esquivar/Parar; onda -> Bloquear; piedras -> Cubrir. Fallo 20. Fase 2 mantiene ventanas y alterna dos patrones en rondas sucesivas, nunca dos respuestas simultáneas. Si concentración=0, defensa básica correcta restaura 1.

Resultado: mi_guardian, chia_released y portal; C09 honra. Objetivo deja claro liberar, no recolectar cadáver.

JEFE: No puedo soltar el paso.

NEMEQUENE: Cortaré lo que te obliga.

JEFE: Chía vuelve a responder.

1. PG: alas/hocico, máscara ligada a núcleo.
2. PM: barrido anunciado, elección de defensa.
3. PD: núcleo abierto y menú Cuerno/Jaguar.
4. PM: C08 transforma y rompe primera atadura.
5. PG: fase 2 usa piedras/onda aprendidas; segunda atadura.
6. PM: arrodillarse y homenaje, lazo lunar se apaga.

**Prompt:** Producir E07 Jefe caimán-murciélago en MI09 / arena 18x 16 m inicial. Motivo: Guardián enlazado a Chía; protege y retiene contra su voluntad. Iniciar: E con coca y Chía sellada; guarda checkpoint MI08 antes de C07. Implementar reglas: Vida 160; dos ataduras. Fase 1 hasta 80: Atacar D con núcleo abierto; Cuerno consume turno sin daño, abre núcleo siguiente turno aunque no haya defensa correcta. Defensa correcta también abre núcleo. Si está cerrado, Atacar hace D/2. Primer Jaguar legal con concentración rompe atadura 1 y hace D+10; segunda a vida <=80 rompe atadura 2. Para victoria requiere ambas: a vida 1 con atadura pendiente avisa y ofrece ventana, no softlock. Afinidad nunca se gasta. Anunciar patrón: Ciclo barrido -> Esquivar/Parar; onda -> Bloquear; piedras -> Cubrir. Fallo 20. Fase 2 mantiene ventanas y alterna dos patrones en rondas sucesivas, nunca dos respuestas simultáneas. Si concentración=0, defensa básica correcta restaura 1. Resolver: mi_guardian, chia_released y portal; C09 honra. Objetivo deja claro liberar, no recolectar cadáver. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E08 / Serpiente bicéfala

Zona: MS08 / arena 24x 24 m

Motivo: Dos voluntades atrapadas por un vínculo que retiene Sué.

Entrada: E en marca con llave; checkpoint MS07; Sué aún no recogible.

Reglas: Vida 180; dos vínculos de 40 cada uno incluidos en vida total. Al comenzar A está vulnerable; una defensa correcta cambia la vulnerabilidad a la cabeza que anunció. Una defensa fallida conserva el objetivo vulnerable anterior, mostrado en el HUD. Turno ofensivo primero selecciona cabeza A/B con botones y voz Izquierda/Derecha, luego Atacar o Contraatacar. A y B no significan izquierda física si cámara gira: mantener marcas y nombre. Elegir cabeza anunciada como vulnerable hace D y reduce su vínculo; otra hace D/2 solo a cuerpo. Ambos vínculos deben llegar a 0 antes de vida corporal 0; si falta, vida queda 1 y mantiene vulnerabilidad hasta resolver. D base 20 basta.

Patrón: Fase 1 hasta 90: A presión frontal -> Bloquear/Parar; B barrido -> Esquivar. Fase 2 añade B pulso -> Bloquear y A fragmentos -> Cubrir. Ventanas iguales. Cada aviso muestra cabeza, silueta y verbo. Fallo 20.

Resultado: ms_guardian; altar Sué; C15 muestra resonancia y perseguidor. Yopos opcionales solo acortan acciones.

SERPIENTE A: Camino en el horizonte.

SERPIENTE B: Orden que lo retiene.

NEMEQUENE: Responderán sin obedecer a Quimue.

1. PG: ambas cabezas separadas con señales A/B.
2. PD: aviso señala cabeza origen y verbo de respuesta.
3. PM: defensa correcta vuelve una cabeza vulnerable.
4. PD: selección A/B y Atacar, daño único.
5. PG: dos vínculos se cortan en rondas separadas.
6. PG: serpiente se retira, Sué aparece sobre altar fijo.

**Prompt:** Producir E08 Serpiente bicéfala en MS08 / arena 24x 24 m. Motivo: Dos voluntades atrapadas por un vínculo que retiene Sué. Iniciar: E en marca con llave; checkpoint MS07; Sué aún no recogible. Implementar reglas: Vida 180; dos vínculos de 40 cada uno incluidos en vida total. Al comenzar A está vulnerable; una defensa correcta cambia la vulnerabilidad a la cabeza que anunció. Una defensa fallida conserva el objetivo vulnerable anterior, mostrado en el HUD. Turno ofensivo primero selecciona cabeza A/B con botones y voz Izquierda/Derecha, luego Atacar o Contraatacar. A y B no significan izquierda física si cámara gira: mantener marcas y nombre. Elegir cabeza anunciada como vulnerable hace D y reduce su vínculo; otra hace D/2 solo a cuerpo. Ambos vínculos deben llegar a 0 antes de vida corporal 0; si falta, vida queda 1 y mantiene vulnerabilidad hasta resolver. D base 20 basta. Anunciar patrón: Fase 1 hasta 90: A presión frontal -> Bloquear/Parar; B barrido -> Esquivar. Fase 2 añade B pulso -> Bloquear y A fragmentos -> Cubrir. Ventanas iguales. Cada aviso muestra cabeza, silueta y verbo. Fallo 20. Resolver: ms_guardian; altar Sué; C15 muestra resonancia y perseguidor. Yopos opcionales solo acortan acciones. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E09 / Mujer-cóndor

Zona: MS03 / anexo seguro nuevo

Motivo: Protege fragmento de antiguo portador y prueba compromiso de regreso.

Entrada: Tras Runa 2; E en anexo 10x 10 m junto a terraza, nunca sobre salida P2.

Reglas: Vida de prueba 60; Atacar D. Viento anuncia penalización: Anclar consume turno sin daño y anula la siguiente ronda de viento; alternativas defensivas siguen viables. No caída de arena ni pérdida de vuelo. Una victoria significa reconocimiento, no matar.

Patrón: Ala frontal -> Bloquear/Parar; viento lateral -> Esquivar. Anclar evita viento y recupera 1 concentración en esa ronda sin defensa. Fallo 15.

Resultado: condor_resolved; vuelta P2 libre. Amplía alcance del superior: actor y anexo nuevos respecto a guía, justificados por canon.

CÓNDOR: Alcanzar altura no basta. ¿Cómo volverás?

NEMEQUENE: Conservaré el mismo camino.

CÓNDOR: Llévate la señal de la pared.

1. PG: cóndor en anexo plano, portal fuera de combate.
2. PM: explica prueba sin hostilidad gratuita.
3. PD: Anclar o Atacar con efecto de viento visible.
4. PG: viento/ala solicitan respuesta diferente.
5. PM: resolución, criatura pliega alas.
6. PG: señala P2 y recuerdo de pared 02.

**Prompt:** Producir E09 Mujer-cóndor en MS03 / anexo seguro nuevo. Motivo: Protege fragmento de antiguo portador y prueba compromiso de regreso. Iniciar: Tras Runa 2; E en anexo 10x 10 m junto a terraza, nunca sobre salida P2. Implementar reglas: Vida de prueba 60; Atacar D. Viento anuncia penalización: Anclar consume turno sin daño y anula la siguiente ronda de viento; alternativas defensivas siguen viables. No caída de arena ni pérdida de vuelo. Una victoria significa reconocimiento, no matar. Anunciar patrón: Ala frontal -> Bloquear/Parar; viento lateral -> Esquivar. Anclar evita viento y recupera 1 concentración en esa ronda sin defensa. Fallo 15. Resolver: condor_resolved; vuelta P2 libre. Amplía alcance del superior: actor y anexo nuevos respecto a guía, justificados por canon. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E10 / Mujer-águila

Zona: MS06 / terraza final de llave

Motivo: Reconoce fuerza del visitante, cuestiona su ambición por la cima.

Entrada: Cuatro vuelos terminados; E en terraza de 12x 12, sin inspección en plataforma móvil.

Reglas: Vida 60. Dos posturas: elevada recibe Atacar D/2; Interrumpir D/2 la lleva al suelo y cancela próxima carga, luego Atacar D. Anclar anula viento, no hace daño. Estrategias alternativas de seis ataques reducidos siguen posibles.

Patrón: Carga aérea -> Esquivar; descarga de plumas/proyectil -> Cubrir o Bloquear. Fallo 15. Contraventana tras carga evitada permite Contraatacar.

Resultado: eagle_resolved; llave accesible. Diálogo prepara lectura de dos voces; reencuentro pacífico.

ÁGUILA: ¿Has visto el suelo de quienes vienen detrás?

NEMEQUENE: Dejé un camino de vuelta.

ÁGUILA: Arriba, mira cuál cabeza anuncia.

1. PG: llegada al último apoyo, descanso antes del duelo.
2. PM: águila abre alas sobre marca fija.
3. PD: Interrumpir contra carga elevada.
4. PG: defensa y descenso al suelo.
5. PM: último golpe completa prueba, no muerte.
6. PG: altar de llave liberado, transporte aún esperando.

**Prompt:** Producir E10 Mujer-águila en MS06 / terraza final de llave. Motivo: Reconoce fuerza del visitante, cuestiona su ambición por la cima. Iniciar: Cuatro vuelos terminados; E en terraza de 12x 12, sin inspección en plataforma móvil. Implementar reglas: Vida 60. Dos posturas: elevada recibe Atacar D/2; Interrumpir D/2 la lleva al suelo y cancela próxima carga, luego Atacar D. Anclar anula viento, no hace daño. Estrategias alternativas de seis ataques reducidos siguen posibles. Anunciar patrón: Carga aérea -> Esquivar; descarga de plumas/proyectil -> Cubrir o Bloquear. Fallo 15. Contraventana tras carga evitada permite Contraatacar. Resolver: eagle_resolved; llave accesible. Diálogo prepara lectura de dos voces; reencuentro pacífico. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## E11 / Quimue / dueño de dos vínculos

Zona: Plaza / círculo de 16x 16 m propuesto

Motivo: Quiere las máscaras para conservar dominio de los tres planos.

Entrada: Regreso con Sué, Chía en custodia; C16 y E en marca explícita. Se puede prepararse/guardar antes de E.

Reglas: Vida 200 y dos anclajes luna/sol de 40 incluidos en vida. El origen activo empieza en Luna; al resolver cada aviso cambia al origen anunciado para la decisión siguiente. Tras las dos rondas defensivas de Unión, el origen activo queda en Sol; el ciclo conserva también los avisos lunares normales. El HUD identifica antes de elegir qué anclaje puede dañarse. Al anunciar origen, elegir Luna/Sol y Vincular consume 1, hace D a vida y al anclaje seleccionado. Si se elige anclaje opuesto no se corta y hace D/2 a vida: feedback antes de repetir. Atacar D/2 mientras existe algún anclaje. Con ambos cortados Atacar D o Jaguar D+10. Vida queda 1 si anclaje pendiente, mostrando objetivo; defender recupera concentración. Victoria requiere ambos cortados y vida 0. Máscaras jamás robadas físicamente del inventario.

Patrón: Fase 1 hasta 100: Lunar barrido -> Esquivar; Solar frontal -> Bloquear/Parar. Fase 2 añade unión: HUD muestra Luna -> Sol; dos rondas defensivas consecutivas cada una con aviso y su propia ventana, sin pedir dos palabras a la vez. Fallo 20 por ronda, máximo 40 en secuencia. Las dos máscaras cortan la unión permanentemente al completar anclajes.

Resultado: quimue_defeated; tránsito restituido; C17 y C18 antes de regreso trágico. No afirmar invasores derrotados.

QUIMUE: Reuniste lo que no pude tomar unido.

NEMEQUENE: No te daré su voluntad.

QUIMUE: Querías conquistar.

NEMEQUENE: Hoy elijo devolver.

1. PG: dos lazos conectan Quimue a soportes de máscara.
2. PD: menú origen Luna/Sol y Vincular explica concentración.
3. PM: ataque lunar/solar anticipado; defensa cambia el turno.
4. PG: primer lazo se apaga; no arrebato de inventario.
5. PG: segunda fase anuncia dos rondas completas, cortar segundo lazo.
6. PM: bastón/Jaguar final, Quimue pierde poder y queda vencido.

**Prompt:** Producir E11 Quimue / dueño de dos vínculos en Plaza / círculo de 16x 16 m propuesto. Motivo: Quiere las máscaras para conservar dominio de los tres planos. Iniciar: Regreso con Sué, Chía en custodia; C16 y E en marca explícita. Se puede prepararse/guardar antes de E. Implementar reglas: Vida 200 y dos anclajes luna/sol de 40 incluidos en vida. El origen activo empieza en Luna; al resolver cada aviso cambia al origen anunciado para la decisión siguiente. Tras las dos rondas defensivas de Unión, el origen activo queda en Sol; el ciclo conserva también los avisos lunares normales. El HUD identifica antes de elegir qué anclaje puede dañarse. Al anunciar origen, elegir Luna/Sol y Vincular consume 1, hace D a vida y al anclaje seleccionado. Si se elige anclaje opuesto no se corta y hace D/2 a vida: feedback antes de repetir. Atacar D/2 mientras existe algún anclaje. Con ambos cortados Atacar D o Jaguar D+10. Vida queda 1 si anclaje pendiente, mostrando objetivo; defender recupera concentración. Victoria requiere ambos cortados y vida 0. Máscaras jamás robadas físicamente del inventario. Anunciar patrón: Fase 1 hasta 100: Lunar barrido -> Esquivar; Solar frontal -> Bloquear/Parar. Fase 2 añade unión: HUD muestra Luna -> Sol; dos rondas defensivas consecutivas cada una con aviso y su propia ventana, sin pedir dos palabras a la vez. Fallo 20 por ronda, máximo 40 en secuencia. Las dos máscaras cortan la unión permanentemente al completar anclajes. Resolver: quimue_defeated; tránsito restituido; C17 y C18 antes de regreso trágico. No afirmar invasores derrotados. Voz y teclado llaman al mismo modelo con un token por acción. Respetar paneles, señales accesibles y diálogo; no añadir daño desde VFX.


## Escudo, intento correcto

Nivel 3 -> turno Impulso -> Uno -> Dos -> Tres, cada palabra con espera ilimitada -> cadena visual rompe escudo -> enemigo Carga -> Esquivar -> +1 concentración -> Atacar 20 -> golpe físico -> Bloquear -> Atacar 20 -> victoria. Orden equivocada en selección no causa daño; respuesta defensiva equivocada sí 20.


## Jaguar sin reservas agotables

Jefe abierto por defensa correcta, concentración 2 -> Jaguar consume 1, daño 30 y corta atadura 1 -> defensa bloquea onda y devuelve concentración -> Atacar en núcleo -> al llegar<=80, otro Jaguar corta atadura 2 -> ataques terminan. Si fallan defensas y concentración 0, Cuerno abre exposición pero Jaguar espera una defensa correcta. Modo sin vencimiento permite pensar.


## Serpiente, leer origen

Cabeza A anuncia presión -> Bloquear -> concentración 2 y A vulnerable -> decir Izquierda (A marcada) -> Atacar 20 al vínculo A. Próxima B barrido -> Esquivar -> Derecha -> Atacar 20 a B. Repetir para 40 en cada vínculo; cuerpo pierde esos mismos 80, no daño duplicado. Después Atacar al cuerpo termina vida restante.


## Quimue con daño mínimo

D20; vida 200. Defender y Vincular Luna dos veces corta 40; defender y Vincular Sol dos veces corta 40, restan 120 de vida. Seis Atacar 20 más terminan si ningún contraataque. Como cada acción tiene defensa, patrón puede alcanzar fase 2, siempre con aviso. Sin yopos ni parry es viable. Si concentration 0, esperar defensa correcta; afinidades no se pierden.


# 08 / Producción, guardado y validación


## Propuesta de montaje de campaña

Mantener las escenas/nombres internos existentes. Añadir un controlador de capítulo con condiciones explícitas: prólogo, plaza tutorial, inframundo, retorno lunar, puzzle de urna, superior, retorno solar, Quimue, regreso, muerte y legado. Separar progreso global del estado temporal de una escena. Las guías anteriores continúan vigentes para terreno, medidas y retornos; el guion reemplaza apertura de mundos, tipos de actores y reglas de combate donde hay conflicto.


## Qué se conserva y qué cambia

Se conservan nueve salas inferiores, ocho zonas superiores, semilla, tres brazaletes, cuerno, dos runas, alas, dos Yopos opcionales, llave, dos pares internos P1/P2 y P3/P4, transporte y checkpoints. Se añaden nicho lunar en MI08, recursos de afinidad, tres urnas del hub, máscara solar después de serpiente, Bachué y actores del canon. Se añaden dos encuentros superiores en suelo ancho. No agregar un tercer nivel para Quimue: reutilizar círculo de plaza.


## Presupuesto de geometría nuevo

La guía superior no tenía enemigos comunes; aquí se incorporan Cóndor en anexo MS03 de 10x 10 m y Águila en terraza final MS06 de 12x 12 m. Mantener salida deP2 libre, mínimo 3x 3m antes de trigger y ningún golpe desde fuera de cuadro. Ampliar lateral de MS03 hacia espacio que no conecte con MS05 ni cree atajo aéreo. Quimue usa círculo hub de 16x 16m iniciales. Ajustar con cámara y cápsula, no dar estas medidas por probadas.


## Orden de capacidades

El préstamo de dash en tutorial es temporal y no cuenta como brazalete permanente. MI comienza con progreso propio, nivel 0. Cada par tiene ID y nivel objetivo 1/2/3, máximo 3. Runa 1 no concede alas; Runa 2 no permite escalar muros lisos. Guacamaya cinematográfica de plaza no concede vuelo jugable antes de MS04. Alas no permiten escapar del duelo. En MS el dash queda terrestre para no omitir huecos; su efecto en combate es acción del modelo.


## Dirección de arte

Inferior: basalto azulado, humedad, raíces y luz de hueso; superior: piedra arena/ocre, cielo turquesa, nubes crema, metal mate y textiles escasos; plaza: piedra cálida, pergamino, verde y oro envejecido. Conservar formas amplias y suelo legible. Las prendas, máscaras y animales del canon requieren diseño aprobado; no fabricar una atribución cultural histórica al modelarlos. Las piezas de armadura son fragmentos ficticios, no armaduras europeas.


## Cámara y cuerpos

Cámara de exploración sigue 3D y muestra destino de salto/vuelo. Los esquemas de storyboard no fijan isometría. Al reservar inspección o combate, guardar pose y autoridad; al salir restaurar ambas. Cámara cinemática puede usar distancias dePG/PM/PD, sin lente extrema ni giro que desoriente. Animaciones in place fuera de secuencias de trayectoria. Cuerpo, ropa, arma y alas deben comprobarse juntos.


## Prompts de trabajo

Cada ficha contiene su prompt local derivado del guion y storyboard. El prompt maestro fija estilo, entrada, duración, salida y eventos; el local fija el objeto/actor, requisito, descubrimiento o patrón y recompensa. Un prompt no sustituye criterio de aceptación: entregar secuencia editable, poses, IDs, sockets, audio/subtítulos y prueba de estados. No pedir a un generador que invente lore o conecte automáticamente salas.


## Implementación propuesta

Usar datos de secuencia con sceneId, trigger, requiredFlags, watchedFlag, shotList y exitState. Datos de hallazgo con findId, visibleHotspots, requiredViews, stabilityTime, rewardType y persistKey. Datos de encuentro con encounterId, actions, telegraphs, allowedDefenses, phaseThresholds y victoryFlags. Un modelo acepta comandos semánticos; adaptadores de voz, manos y teclado no resuelven por sí solos daño o narrativa.


## Dispositivos y continuidad

MediaPipe local dirige objeto y selección cuando modo manos activo. Reutilizar calibración/detección existente, añadir hotspots como sistema nuevo. Reconocedor actual solo cubre Attack/Dodge/Guard; ampliar verbos y objetivos por estado y plataforma sin dar por garantizado soporte. Indicador de cámara/micrófono real. No grabar/transmitir contenido por defecto; guardar solo opción de entrada y progreso del juego. Alternativas disponibles sin red/cámara/micrófono.


## Audio y actuaciones

No exigir acentos supuestamente indígenas, gritos, cantos o fórmulas rituales al jugador. Voces de personaje según dirección dramática. Distinguir ruido que avisa de peligro, sonido de confirmación y ambiente. Mientras habla un NPC, silenciar escucha de comandos y reabrir con señal visible al terminar. Subtítulos nombran Cuerno responde, Viento lateral, Pulso lunar cuando aportan información.


## Orden de producción

Primero reconciliar estado global y perfiles de entrada; después probar un hallazgo y un duelo completos en plaza; luego MI con cinco habilidades/cuernos y máscaras; después MS con retornos, alas y anexos; luego Quimue y cadena de epílogo. Bloquear las 21 cinemáticas con poses antes de pedir animación final. Pulir manos, voz y ritmo mediante pruebas reales, y al final sustituir placeholders por modelos aprobados.


## Historia

Nueva partida recorre H01-H21 en orden de campaña. Cada parentesco, objeto y custodia coinciden. Ningún diálogo de capítulo futuro aparece temprano. La muerte final y la entrega de tres piezas permanecen al saltar o recargar.


## Objetos

Cada ficha exige acción observable antes de confirmar. No basta acercarse y pulsarE dos veces. Cancelar restaura pieza y cámara; confirmar dos veces no duplica premio. Las 28 fichas y seis ofrendas usan IDs distintos. Ningún requisito se oculta detrás del objeto que lo satisface.


## Manos reales

Probar dos manos, una mano, cierre/reapertura, salida de cuadro, luz cambiante, cámara apagada y cambio a mouse. Verificar signos yaw/pitch, descartar deltas antiguos y estabilidad. No certificar MediaPipe con una prueba que solo use mouse.


## Voz real

Probar verbos y objetivos con varias voces, ruido moderado y audio del propio juego; registrar fallos por estado, latencia desde decisión y percepción. No asegurar precisión universal. Una frase da una acción; palabra no disponible no gasta turno; desconexión pausa y ofrece alternativa.


## Combate

Resolver E03-E11 sin Yopos y sin Parar. Comprobar concentración 0 y recuperación mediante defensa. Escudo permite Uno/Dos/Tres sin tiempo. Serpiente y Quimue no finalizan con lazos pendientes ni quedan sin acciones legales. Animaciones/proyectiles no infligen doble daño.


## Navegación

Jugar rutasT-P00 aT-P02. P1-P2 vuelve a 02; pared lleva 04; 04-05 requierealas; P4 empieza 06 antes de cuatro vuelos. Islas opcionales admiten ida/vuelta. El transporte vuelve aunque se abandone. Ningún combate bloquea un aterrizaje de aprendizaje.


## Guardado

Guardar/cargar tras cada habilidad, máscara sellada/libre, atajo, jefe y retorno. Fallar en MI09 vuelve 08; MS08 vuelve 07; Quimue vuelvehub. Recuperación por caída usa apoyo estable, no borde/losa/aire. Reintentar no reinicia elementos persistentes.


## Cinemáticas

21 secuencias, todos los clips/familias A01-A70 y sockets. Probar terminar/saltar/pausar cada una. Con voz deNPC no se escucha comando. Custodia visual y estado coinciden. Subtítulos no se cortan al alargar una locución.


## Accesibilidad

Campaña completa con teclado/mouse sin cámara ni micrófono; manos/voz siguen opción principal solicitada. Ventana ampliada y sin vencimiento no cambian recompensas. Forma, nombre y verbo acompañan color. Movimiento reducido no quita señales.


## Entrega cultural

Separar siempre Descripción del juego de Contexto documentado. Las fichas de magia y objetos ficticios no publican atribuciones museales sin fuente. El guion evita recetas y afirma parentescos/cronología como adaptación, no como lección histórica.


## Estados persistentes
- Prólogo: C01-C03; staff_owned, vision_seen, poporo_owned, map_owned. No saltar a mundo sin objetos entregados; Skip aplica mismos flags.
- Tutorial: 3 lecciones y E01; lessons_complete, training_complete. mi_unlocked solo con ambas; préstamo dash no persiste comoB1.
- MI habilidades: O-I01-I04; mi_seed, mi_bracelets_1, mi_bracelets_2, mi_bracelets_3. Nivel máximo 3; objetos por ID. Tres análisis semilla+B1+B2 habilitan coca.
- MI acceso: O-I05 + O-N04; mi_horn, mi_horn_gate, chia_sealed. Cuerno no se consume; puerta persistente y máscara aún no libre.
- MI victoria: E07 y C09; mi_guardian, chia_released. Persistir antes de homenaje; retorno temprano sin victoria no habilita cielo.
- Retorno lunar: C10 y O-N05; chia_in_custody, guacamaya_affinity. Chía original en fuente; reflejo en poporo, afinidad no consumible.
- Urna: O-N06; urn_solved, ms_unlocked. Tres candidatas; un cuerno original y su proyección. Skip vuelo no omite puzzle.
- MS habilidades: O-S01-S03; ms_rune 1, ms_rune 2, ms_wings. Recuperar todas al cargar; no vuelo de forma plena antes de alas en gameplay.
- MS recuerdos: Runas y llave; armor_memory_1, armor_memory_2, armor_memory_3. Un recuerdo por altar; no tres recompensas funcionales adicionales.
- MS opcionales: O-S04/S05; ms_yopo 1, ms_yopo 2. Potencia por IDs; no cambia afinidad base ni bloquea final.
- MS acceso/victoria: Llave, E08 y O-N07; ms_key, ms_lock_open, ms_guardian, sue_released. Victoria persiste si máscara no se inspecciona todavía; portal final espera Sué.
- Quimue: C16 y E11; final_duel_available, quimue_defeated. Custodia máscaras segura; derrota vuelve a hub previo sin rehacer niveles.
- Epílogo: C18-C21; masks_in_custody, canonical_wound, nemequene_dead, tisquesusa_staff, campaign_complete. No entrar a respawn ni permitir batalla libre durante muerte canónica.
- Recuerdos: Después de créditos; memories_mode. Copia de estado previo a Quimue, no sobrescribe partida completada ni custodia final.

## Continuidad
- Parentesco contradictorio (Narrativa p 1-2): Saguanmachica tío; Tisquesusa sobrino. Eliminar tío/primo para Tisquesusa de todo texto.
- Tío vivo pese a herencia (Narrativa p 1 y 3): La entrega inicial es formación y reconocimiento como heredero, no funeral. Puede seguir vivo hasta regreso.
- Profecía no evitada (Narrativa p 1-3): Metales sin rostro; advertencia clara; el error es interpretación de Nemequene, no falta de una pista disponible.
- Poder de las máscaras (Narrativa p 2-3): Restauran tránsito, no victoria bélica. Bachué lo explica antes y después; invasión no contradice premio.
- Mundos libres frente a historia lineal (Guías MI/MS y Narrativa p 2): Campaña inferior primero; pruebas de editor pueden viajar libremente sin avanzar guion. Retornos ya abiertos siguen libres.
- Cuerno en dos ubicaciones (Narrativa p 2 y guíaMI): Original MI07; proyección de su recuerdo en urna vacía del hub. No segunda llave ni pérdida del primero.
- Coca antes del requisito (Narrativa p 2): Tras semilla+B1+B2, reserva visible en MI04; MI09 avisa si no recogida y permite retorno.
- Yopo antes de alas (Narrativa p 2, GDDp 8, guíaMS): Yopo base activa guacamaya de cinemática para llegar al umbral. Alas estabilizan vuelo controlado. Opcionales solo refinan daño.
- Guardián después de tomar Chía (Narrativa p 2): Máscara sellada en 08; liberación obligatoria al romper lazo 09. Retorno temprano no entrega máscara sellada.
- Tres fragmentos y objetos superiores (Narrativa p 2, guíaMS): Hebilla/Runa 1, brazal/Runa 2 y placa/Llave: tres memorias, sin cambiar bloqueo ni cantidad de premios de movilidad.
- Enemigos superiores antes ausentes (GDDp 10-11, guíaMS): Dos anexos/encuentros explícitos para cóndor y águila; no adversarios durante primer vuelo o salida del portal.
- Por qué Quimue alcanza plaza (Narrativa p 3): Rastrea resonancia solar por último lazo; anticipado en MS07/08. Restaurar pasos permite tránsito en ambos sentidos.
- Derrota final no injusta (Narrativa p 3): Flecha mortal en cinemática, escucha desactivada y sin HUD. El jugador no pierde un duelo que había ganado.
- Procedencia del legado (Narrativa p 3): Bachué custodia máscaras después de Quimue; Furachogua es emisaria. Bastón sigue en manos del sobrino, se deposita y recibe original.
- Muerte y postgame (Narrativa p 3): Muerte canónica persistente; Revisitar recuerdos usa copia previa, no resurrección ni nueva campaña oculta.

# 09 / Trazabilidad y lista de producción


## Fuentes
- S01: Narrativa.pdf, páginas físicas 1-3. Canon de doce etapas; contradicciones familiares resueltas y final conservado.
- S02: GDD - El ASEDIO DE BACATÁ.pdf, físicas 4, 7-12, 13-17. Sinopsis, voz, manos, recursos, fauna y avances; índice no acredita capítulos ausentes.
- S03: PRODUCT.md; Specs/Week 08/05-Plaza-Lobby-Tutorial.md. Tres piezas, giro 28 grados, congelación 0,65 s y práctica. Guardado posterior según PRODUCT.
- S04: Specs/Worlds/Guia-completa-mundo-inferior.md. MI01-09 y cinco hallazgos; se sustituye combate físico.
- S05: Specs/Worlds/Guia-completa-mundo-superior.md; Mundo-superior.md. MS01-08 y seis hallazgos; orden de campaña sustituye acceso libre.
- S06: Assets/_Game/Scripts/Controller/VoiceCommandRecognizer.cs. Attack/Dodge/Guard disponibles; nuevas acciones requieren integración.
- S07: Assets/_Game/Scripts/Controller/MundoInferior/MIFind.cs; MIGuardian.cs; MIShieldSentinel.cs. IDs y confirmación, combate físico existente; ampliar manos y turnos.
- S08: Assets/_Game/Scripts/Controller/MundoSuperior/MSGuardian.cs. Duelo de teclado; voz contextual y cabezas nuevas no implementadas.
- S09: ArtSource/Characters/Tisquesusa/README_v 02.md. Recurso estático de modelo; no define identidad del protagonista.
- S10: Specs/UI/Diseno-integral-2026-09-29.md; El_Asedio_de_Bacata_UI_Design_System.md. Piedra, pergamino, oro, legibilidad y foco.