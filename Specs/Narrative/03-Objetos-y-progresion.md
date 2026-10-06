# El asedio de Bacatá

Propuesta 1.0 | 6 de octubre de 2026

# 05 / Guion 3: objetos e inspección


## Ciclo principal

Pista en entorno, acción de aproximación, entrada E, manipulación, descubrimiento, estabilización, confirmación, recompensa persistente y aplicación inmediata. Un hallazgo necesita interpretar una relación visible, no girar sin propósito hasta completar una barra. Las lecciones de plaza conservan su umbral cuantitativo; los hallazgos usan marcas y vistas.


## Control de manos

Modo manos explícito con cámara habilitada. Izquierda abierta: yaw; derecha abierta: pitch; cada puño congela su eje. Reabrir establece nueva referencia y no salta la rotación. Una mano fuera de cámara congela ese eje y avisa; jamás equivale a puño válido. Al reaparecer descartar deltas anteriores. Modo una mano permite elegir eje; mouse/teclas conserva el mismo puzzle.


## Señalar y seleccionar

Nueva propuesta: rayo de índice estable 0,4 s resalta un hotspot grande; pinza corta selecciona ese hotspot. Solo funciona en estado Selección, no mientras rota. Seleccionar no concede automáticamente el objeto. E o botón Confirmar cierra cuando todos los requisitos están listos. No confundir pinza con cierre de puño ni permitir dos interacciones simultáneas.


## Geometría de validación

Cada marca lleva una normal local que se compara con dirección de cámara. Cono inicial 18 grados, estabilidad 0,65 s y guía visible; ajustar en prueba real. Para dos vistas, registrar haber mostrado ambas, sin exigir un único Euler ni una posición absoluta de manos. El puño estabiliza; lo que resuelve el puzzle es la orientación y la correspondencia, no una pose biométrica.


## Seguridad de la escena

Durante inspección se suspenden locomoción, IA hostil del entorno, trampas del tramo y cámara normal. No basta SetInputLocked para proteger al jugador. Sobre suelo estable; nunca vuelo, muelle móvil o caída. Guardar pose de cámara/objeto, restaurar al cancelar y soltar autoridad de interacción en todas las salidas.


## Tiempo y recompensa

Sin límite para observar. Intento incorrecto muestra una pista de la relación fallida, nunca quita vida. Confirmar hace una transacción por findId: completar puzzle, conceder habilidad, guardar y ocultar pieza. Si ya existe ID, abrir archivo sin volver a premiar. Cerrar antes de confirmar no concede recompensa; descubrimientos parciales pueden mantenerse solo durante sesión, con regla visible.


## Ritmo

Meta inicial de 30-75 s por hallazgo funcional, 15-35 s por recuerdo opcional y hasta 120 s para urna; valores de diseño por medir. Alternar comparación, búsqueda de reverso, lectura de perfil y selección, con un ensayo jugable después. Tras tres intentos ofrecer pista. Revisitas permiten consultar información y saltar presentación ya vista.


## O-P01 / Vasija del eco

Zona: Plaza / estación 01

**Acceso:** Disponible desde llegada; pedestal estable a la izquierda.

**Tarea:** Ver una banda que desaparece detrás de la vasija y seguirla al girar.

**Gestos:** Mano izquierda abierta rota horizontal; derecha cerrada fija inclinación.

**Validación:** Acumular 28 grados de giro horizontal; no sirve entrar y pulsar E.

**Premio:** plaza_lesson_1; pieza permanece para practicar.

**Aplicación:** En objetos posteriores enseña a buscar reverso.

**Fallo:** Salir conserva avance de la sesión; regresar no concede lección duplicada.

BACHUÉ: El frente no contiene toda la pieza.

1. PG: jugador ve banda cortada en altar.
2. PM: E reserva pieza y cámara muestra borde oculto.
3. PD: palma izquierda desplaza y banda continúa por reverso.
4. PD: indicador llega a 28 grados; no cronómetro.
5. PM: E confirma lección, no retira vasija.
6. PG: objetivo indica disco o figura pendientes.

**Prompt:** Producir O-P01 Vasija del eco en Plaza / estación 01. Entrada: Disponible desde llegada; pedestal estable a la izquierda. Mostrar la pista: Ver una banda que desaparece detrás de la vasija y seguirla al girar. Integrar manos: Mano izquierda abierta rota horizontal; derecha cerrada fija inclinación. Validar: Acumular 28 grados de giro horizontal; no sirve entrar y pulsar E. Confirmar una sola vez: plaza_lesson_1; pieza permanece para practicar. Enseñar inmediatamente: En objetos posteriores enseña a buscar reverso. Fallo: Salir conserva avance de la sesión; regresar no concede lección duplicada. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-P02 / Disco del alba

Zona: Plaza / estación 02

**Acceso:** Accesible sin completar estación 01.

**Tarea:** Descubrir la cara inferior de un disco sobre apoyo.

**Gestos:** Derecha abierta mueve inclinación; izquierda cerrada fija horizontal.

**Validación:** 28 grados acumulados verticalmente.

**Premio:** plaza_lesson_2; conserva disco.

**Aplicación:** Prepara lectura inferior de runas, máscara y llave.

**Fallo:** No pedir giro de 360 grados; reflejos no sustituyen marca de progreso.

BACHUÉ: Inclina; también existe un camino debajo.

1. PG: disco frontal sobre pedestal.
2. PM: vista de inspección con cara inferior oculta.
3. PD: mano derecha sube/baja y disco inclina.
4. PD: forma inferior se hace visible; 28 grados.
5. PM: E termina y registra método de entrada.
6. PG: personaje vuelve a su suelo, cámara restaurada.

**Prompt:** Producir O-P02 Disco del alba en Plaza / estación 02. Entrada: Accesible sin completar estación 01. Mostrar la pista: Descubrir la cara inferior de un disco sobre apoyo. Integrar manos: Derecha abierta mueve inclinación; izquierda cerrada fija horizontal. Validar: 28 grados acumulados verticalmente. Confirmar una sola vez: plaza_lesson_2; conserva disco. Enseñar inmediatamente: Prepara lectura inferior de runas, máscara y llave. Fallo: No pedir giro de 360 grados; reflejos no sustituyen marca de progreso. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-P03 / Guardián de jade

Zona: Plaza / estación 03

**Acceso:** Tercera pieza de práctica; puede hacerse primero.

**Tarea:** Rotar dos ejes y mantener una postura estable.

**Gestos:** Izquierda horizontal, derecha vertical; cerrar ambos puños congela.

**Validación:** Ambos movimientos y congelación 0,65 s; mano ausente no cuenta como puño.

**Premio:** plaza_lesson_3; conserva figura.

**Aplicación:** Prepara estabilización de hallazgos reales.

**Fallo:** Seguimiento perdido congela y avisa; mouse puede completar con soltar, registrado como mouse.

BACHUÉ: Detener también es una forma de mirar.

1. PG: figura y dos marcas visibles.
2. PM: entrar sin recibir recompensa.
3. PD: dos manos abiertas rotan ejes.
4. PD: puños cerrados, contador estable 0,65 s.
5. PM: E confirma cuando requisitos se iluminan con texto.
6. PG: siguiente objetivo es práctica si las tres listas.

**Prompt:** Producir O-P03 Guardián de jade en Plaza / estación 03. Entrada: Tercera pieza de práctica; puede hacerse primero. Mostrar la pista: Rotar dos ejes y mantener una postura estable. Integrar manos: Izquierda horizontal, derecha vertical; cerrar ambos puños congela. Validar: Ambos movimientos y congelación 0,65 s; mano ausente no cuenta como puño. Confirmar una sola vez: plaza_lesson_3; conserva figura. Enseñar inmediatamente: Prepara estabilización de hallazgos reales. Fallo: Seguimiento perdido congela y avisa; mouse puede completar con soltar, registrado como mouse. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-I01 / Semilla de raíces

Zona: MI02 / O03

**Acceso:** Caminar desde MI01; altar alcanzable con salto simple.

**Tarea:** Leer brote y hendidura; encontrar dos mitades de una misma unión.

**Gestos:** Girar horizontal hasta reverso, inclinar al brote y cerrar ejes.

**Validación:** Normal de marca dirigida a cámara dentro de cono de 18 grados y estabilidad 0,65 s.

**Premio:** mi_seed; HasDoubleJump.

**Aplicación:** Ensayo: primer salto sobre apoyo ancho y segundo en el aire para alcanzar escalón; ruta MI03.

**Fallo:** Error deja pieza; caídas de ensayo recuperan base sin pinchos; no exigir doble salto para adquirirla.

CUSTODIO: El segundo apoyo estaba en la unión, no fuera de ella.

1. PG: entrada y altar sobre suelo fijo.
2. PM: señal de brote apunta a hendidura.
3. PD: giro revela unión del reverso.
4. PD: inclinación alinea dos mitades; puños fijan.
5. PM: E guarda semilla y pequeño brote de luz.
6. PG: dos saltos visibles hacia plataforma de ensayo.

**Prompt:** Producir O-I01 Semilla de raíces en MI02 / O03. Entrada: Caminar desde MI01; altar alcanzable con salto simple. Mostrar la pista: Leer brote y hendidura; encontrar dos mitades de una misma unión. Integrar manos: Girar horizontal hasta reverso, inclinar al brote y cerrar ejes. Validar: Normal de marca dirigida a cámara dentro de cono de 18 grados y estabilidad 0,65 s. Confirmar una sola vez: mi_seed; HasDoubleJump. Enseñar inmediatamente: Ensayo: primer salto sobre apoyo ancho y segundo en el aire para alcanzar escalón; ruta MI03. Fallo: Error deja pieza; caídas de ensayo recuperan base sin pinchos; no exigir doble salto para adquirirla. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-I02 / Brazaletes de impulso 1

Zona: MI03 / O01 par

**Acceso:** Llegar usando doble salto; antes de primer caimán.

**Tarea:** Reconocer que dos mallas son un único par, no dos premios.

**Gestos:** Girar raíz común del par hasta que sus extremos se continúen visualmente.

**Validación:** Marcas alineadas con guía de perfil, tolerancia 18 grados; detener 0,65 s.

**Premio:** mi_bracelets_1; impulso nivel mínimo 1.

**Aplicación:** Ensayo terrestre Q entre dos marcas de suelo; luego palanca del atajo 03-01.

**Fallo:** No combate mientras inspecciona; no pedir impulso para llegar a este par.

NEMEQUENE: Separado solo empuja. Unido conserva una dirección.

1. PG: altar al terminar doble salto.
2. PM: par completo flotante, dos mallas en misma raíz.
3. PD: giro alinea marcas interiores.
4. PD: detener revela trazo continuo.
5. PM: E y ajuste breve en muñecas.
6. PG: impulso corto hacia marca segura; atajo visible.

**Prompt:** Producir O-I02 Brazaletes de impulso 1 en MI03 / O01 par. Entrada: Llegar usando doble salto; antes de primer caimán. Mostrar la pista: Reconocer que dos mallas son un único par, no dos premios. Integrar manos: Girar raíz común del par hasta que sus extremos se continúen visualmente. Validar: Marcas alineadas con guía de perfil, tolerancia 18 grados; detener 0,65 s. Confirmar una sola vez: mi_bracelets_1; impulso nivel mínimo 1. Enseñar inmediatamente: Ensayo terrestre Q entre dos marcas de suelo; luego palanca del atajo 03-01. Fallo: No combate mientras inspecciona; no pedir impulso para llegar a este par. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-I03 / Brazaletes de impulso 2

Zona: MI04 / primer nicho

**Acceso:** Llegar con impulso 1; nicho fuera de círculo hostil.

**Tarea:** Seguir segunda banda que conecta principio y continuación.

**Gestos:** Horizontal revela segmento; vertical compara su dirección con banda del primer par en archivo.

**Validación:** Dos vistas reconocidas en cualquier orden y estabilización final.

**Premio:** mi_bracelets_2; nivel mínimo 2; tercer análisis principal si semilla/B1 listos.

**Aplicación:** Ensayo de dos impulsos en corredor con final ancho; habilita reserva de coca.

**Fallo:** Archivo ofrece silueta anterior, no prueba de memoria textual.

NEMEQUENE: El siguiente impulso no cambia lo que empezó el primero.

1. PG: nicho y corredor de ensayo, enemigos fuera.
2. PM: segunda banda parece incompleta.
3. PD: reverso conserva comienzo.
4. PD: inclinación muestra continuación en guía.
5. PM: E confirma; diario suma análisis.
6. PG: dos estelas en mismo carril, apoyo amplio al final.

**Prompt:** Producir O-I03 Brazaletes de impulso 2 en MI04 / primer nicho. Entrada: Llegar con impulso 1; nicho fuera de círculo hostil. Mostrar la pista: Seguir segunda banda que conecta principio y continuación. Integrar manos: Horizontal revela segmento; vertical compara su dirección con banda del primer par en archivo. Validar: Dos vistas reconocidas en cualquier orden y estabilización final. Confirmar una sola vez: mi_bracelets_2; nivel mínimo 2; tercer análisis principal si semilla/B1 listos. Enseñar inmediatamente: Ensayo de dos impulsos en corredor con final ancho; habilita reserva de coca. Fallo: Archivo ofrece silueta anterior, no prueba de memoria textual. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-I04 / Brazaletes de impulso 3

Zona: MI04 / nicho previo al escudo

**Acceso:** Alcanzar con nivel 2; antes de trigger de duelo.

**Tarea:** Ordenar tres marcas de una banda y terminarla sin inversión.

**Gestos:** Rotar para leer puntos uno-dos-tres; seleccionar inicio con señalamiento/pinza o tecla; congelar.

**Validación:** Orientación correcta y selección de extremo de inicio, sin ventana de 0,2 s para manos.

**Premio:** mi_bracelets_3; nivel mínimo 3, máximo lógico 3.

**Aplicación:** Ensayo Q en exploración; en duelo Impulso habilita secuencia verbal Uno, Dos, Tres con espera.

**Fallo:** No añadir un cuarto nivel; inspeccionar recogido abre archivo sin premio.

NEMEQUENE: Comenzar, mantener, terminar.

1. PG: último par visible antes del círculo de escudo.
2. PM: tres marcas en banda.
3. PD: giro revela flecha de inicio.
4. PD: selección estable de inicio y congelación.
5. PM: E registra nivel 3; tutorial de cadena.
6. PG: primer plano escudo con tres uniones vulnerables.

**Prompt:** Producir O-I04 Brazaletes de impulso 3 en MI04 / nicho previo al escudo. Entrada: Alcanzar con nivel 2; antes de trigger de duelo. Mostrar la pista: Ordenar tres marcas de una banda y terminarla sin inversión. Integrar manos: Rotar para leer puntos uno-dos-tres; seleccionar inicio con señalamiento/pinza o tecla; congelar. Validar: Orientación correcta y selección de extremo de inicio, sin ventana de 0,2 s para manos. Confirmar una sola vez: mi_bracelets_3; nivel mínimo 3, máximo lógico 3. Enseñar inmediatamente: Ensayo Q en exploración; en duelo Impulso habilita secuencia verbal Uno, Dos, Tres con espera. Fallo: No añadir un cuarto nivel; inspeccionar recogido abre archivo sin premio. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-I05 / Cuerno de respuesta

Zona: MI07 / O02

**Acceso:** Después de péndulos, derrumbe y duelo E06 en aproximación.

**Tarea:** Encontrar canal de salida y respuesta del cuerno sin usar micrófono.

**Gestos:** Giro horizontal sigue curva, vertical descubre canal inferior; congelar en perfil legible.

**Validación:** Dos puntos de observación detectados y 0,65 s estable sobre canal.

**Premio:** mi_horn permanente; se usa en MI08 y después en urna de plaza.

**Aplicación:** E en soporte de 08 reproduce nota y abre reja/nicho lunar.

**Fallo:** No consumir ni obligar a soplar; al cancelar no desaparece.

NEMEQUENE: El sonido vuelve al que lo envía.

1. PG: aterrizaje libre y altar al fondo.
2. PM: curva del cuerno frente a jugador.
3. PD: giro hasta boca pequeña.
4. PD: inclinación muestra canal que regresa.
5. PM: E registra; objeto pasa a archivo.
6. PG: soporte 08 responde y reja abre hacia nicho.

**Prompt:** Producir O-I05 Cuerno de respuesta en MI07 / O02. Entrada: Después de péndulos, derrumbe y duelo E06 en aproximación. Mostrar la pista: Encontrar canal de salida y respuesta del cuerno sin usar micrófono. Integrar manos: Giro horizontal sigue curva, vertical descubre canal inferior; congelar en perfil legible. Validar: Dos puntos de observación detectados y 0,65 s estable sobre canal. Confirmar una sola vez: mi_horn permanente; se usa en MI08 y después en urna de plaza. Enseñar inmediatamente: E en soporte de 08 reproduce nota y abre reja/nicho lunar. Fallo: No consumir ni obligar a soplar; al cancelar no desaparece. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S01 / Runa 1 / hebilla de armadura

Zona: MS02 / O01

**Acceso:** Escaleras desde MS01, sin vuelo ni escalada.

**Tarea:** Comparar anverso/reverso del fragmento de hebilla para reconocer P1 y P2.

**Gestos:** Dos giros para revelar marcas; señalar cada destino y congelar.

**Validación:** Ambas caras examinadas; elegir par 1-2, no 3-4; 0,65 s.

**Premio:** ms_rune 1; armor_memory_1; portales internos habilitados.

**Aplicación:** E P1 lleva P2 en MS03; pared posterior visible pero bloqueada.

**Fallo:** No teletransportar al tocar; etiquetas de destino con forma además de color.

NEMEQUENE: La ruta reconoce los dos extremos.

1. PG: altar, P1 y pared en mismo encuadre.
2. PM: hebilla lleva runa central.
3. PD: anverso muestra una muesca de P1.
4. PD: reverso muestra pareja de P2; selección.
5. PM: E guarda recuerdo y activa aro.
6. PG: P1 muestra destino MS03 antes de cruzar.

**Prompt:** Producir O-S01 Runa 1 / hebilla de armadura en MS02 / O01. Entrada: Escaleras desde MS01, sin vuelo ni escalada. Mostrar la pista: Comparar anverso/reverso del fragmento de hebilla para reconocer P1 y P2. Integrar manos: Dos giros para revelar marcas; señalar cada destino y congelar. Validar: Ambas caras examinadas; elegir par 1-2, no 3-4; 0,65 s. Confirmar una sola vez: ms_rune 1; armor_memory_1; portales internos habilitados. Enseñar inmediatamente: E P1 lleva P2 en MS03; pared posterior visible pero bloqueada. Fallo: No teletransportar al tocar; etiquetas de destino con forma además de color. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S02 / Runa 2 / brazal de armadura

Zona: MS03 / O02

**Acceso:** Solo P1-P2 al inicio; suelo fijo sin escalada necesaria.

**Tarea:** Comparar relieve del brazal con apoyos de pared vista en 02.

**Gestos:** Inclinar reverso, señalar relieve escalonado y congelar.

**Validación:** Relieve inspeccionado y selección pared marcada, no pilar liso.

**Premio:** ms_rune 2; armor_memory_2; escalada en superficies marcadas.

**Aplicación:** Resolver cóndor en anexo, volver P2-P1, escalar 02-04.

**Fallo:** No habilitar escalada universal; no crear ruta 03-05 que omita alas.

NEMEQUENE: Ya vi esta señal en la pared del primer altar.

1. PG: P2 llega a suelo ancho junto al altar.
2. PM: brazal y runa, apoyo fijo.
3. PD: reverso presenta escalones.
4. PD: guía muestra pared ya visitada; seleccionar.
5. PM: E; objetivo volver P2-P1 después del encuentro.
6. PG: pared 02 reconoce apoyos y ruta a 04.

**Prompt:** Producir O-S02 Runa 2 / brazal de armadura en MS03 / O02. Entrada: Solo P1-P2 al inicio; suelo fijo sin escalada necesaria. Mostrar la pista: Comparar relieve del brazal con apoyos de pared vista en 02. Integrar manos: Inclinar reverso, señalar relieve escalonado y congelar. Validar: Relieve inspeccionado y selección pared marcada, no pilar liso. Confirmar una sola vez: ms_rune 2; armor_memory_2; escalada en superficies marcadas. Enseñar inmediatamente: Resolver cóndor en anexo, volver P2-P1, escalar 02-04. Fallo: No habilitar escalada universal; no crear ruta 03-05 que omita alas. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S03 / Alas de dirección

Zona: MS04 / O03

**Acceso:** Escalar desde 02; altar sobre suelo firme junto a descanso.

**Tarea:** Leer bisagra y borde de ataque del par plegado.

**Gestos:** Horizontal muestra eje, vertical muestra dirección; seleccionar borde delantero.

**Validación:** Dos puntos identificados, dirección adelante válida y congelación.

**Premio:** ms_wings; vuelo jugable renovable al aterrizar.

**Aplicación:** Ensayo corto con recuperación; primer vuelo 04-05, luego secuencia P4 a llave.

**Fallo:** No gasto de yopo para cada vuelo; no nuevo premio por malla equipada.

NEMEQUENE: La forma me lleva. Estas alas me enseñan a llegar.

1. PG: salida de pared, descanso y alas.
2. PM: par plegado y bisagra.
3. PD: giro a perfil y flecha ornamental ficticia.
4. PD: seleccionar borde de avance, detener.
5. PM: E y despliegue breve de alas equipables.
6. PG: aterrizaje objetivo de 05 visible antes de volar.

**Prompt:** Producir O-S03 Alas de dirección en MS04 / O03. Entrada: Escalar desde 02; altar sobre suelo firme junto a descanso. Mostrar la pista: Leer bisagra y borde de ataque del par plegado. Integrar manos: Horizontal muestra eje, vertical muestra dirección; seleccionar borde delantero. Validar: Dos puntos identificados, dirección adelante válida y congelación. Confirmar una sola vez: ms_wings; vuelo jugable renovable al aterrizar. Enseñar inmediatamente: Ensayo corto con recuperación; primer vuelo 04-05, luego secuencia P4 a llave. Fallo: No gasto de yopo para cada vuelo; no nuevo premio por malla equipada. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S04 / Yopo 1 / recuerdo de potencia

Zona: MS02 / ramal corto

**Acceso:** Opcional al lado de Runa 1; suelo con ida y vuelta.

**Tarea:** Reconocer cavidad de memoria compatible con poporo.

**Gestos:** Rotar recipiente ficticio hasta base; señalar encaje del poporo, congelar.

**Validación:** Base leída y correspondencia seleccionada.

**Premio:** ms_yopo 1; +5 daño base en combates posteriores; no movilidad.

**Aplicación:** Potencia permanente al devolver recuerdo al poporo, nunca combustible obligatorio.

**Fallo:** Omitir no bloquea portales, jefe ni final; no instrucciones reales de sustancias.

NEMEQUENE: Este recuerdo afina el golpe. No decide por mí.

1. PG: altar opcional junto a ruta principal.
2. PM: recipiente de memoria, sin escena de consumo.
3. PD: giro muestra cavidad en base.
4. PD: guía de encaje del poporo; selección.
5. PM: E: Potencia +5, mejora permanente.
6. PG: volver a Runa 1 sin otro combate.

**Prompt:** Producir O-S04 Yopo 1 / recuerdo de potencia en MS02 / ramal corto. Entrada: Opcional al lado de Runa 1; suelo con ida y vuelta. Mostrar la pista: Reconocer cavidad de memoria compatible con poporo. Integrar manos: Rotar recipiente ficticio hasta base; señalar encaje del poporo, congelar. Validar: Base leída y correspondencia seleccionada. Confirmar una sola vez: ms_yopo 1; +5 daño base en combates posteriores; no movilidad. Enseñar inmediatamente: Potencia permanente al devolver recuerdo al poporo, nunca combustible obligatorio. Fallo: Omitir no bloquea portales, jefe ni final; no instrucciones reales de sustancias. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S05 / Yopo 2 / recuerdo de potencia

Zona: Isla lateral de MS04

**Acceso:** Alas obtenidas; vuelo de ida con aterrizaje estable y retorno viable.

**Tarea:** Comparar segunda cavidad con primera para completar recuerdo.

**Gestos:** Giro y detención muestran marcas distintas; seleccionar segunda cavidad del poporo.

**Validación:** Vista base y elección correcta; no exigir pose compleja de dedos.

**Premio:** ms_yopo 2; +5 daño adicional, máximo +10 entre ambos.

**Aplicación:** Vuelo se renueva en isla; regreso a 04 con capacidad base.

**Fallo:** Caer conserva premio confirmado; no volver a sumarlo; primer Yopo no requerido.

NEMEQUENE: Esta ruta también tiene regreso.

1. PG: isla lateral y 04 visibles; sin enemigos.
2. PG: vuelo de ida, aterrizaje amplio.
3. PD: inspección de base en suelo, no en aire.
4. PD: segunda marca y cavidad seleccionada.
5. PM: E registra; vuelo recargado por aterrizar.
6. PG: regreso visible a santuario 04.

**Prompt:** Producir O-S05 Yopo 2 / recuerdo de potencia en Isla lateral de MS04. Entrada: Alas obtenidas; vuelo de ida con aterrizaje estable y retorno viable. Mostrar la pista: Comparar segunda cavidad con primera para completar recuerdo. Integrar manos: Giro y detención muestran marcas distintas; seleccionar segunda cavidad del poporo. Validar: Vista base y elección correcta; no exigir pose compleja de dedos. Confirmar una sola vez: ms_yopo 2; +5 daño adicional, máximo +10 entre ambos. Enseñar inmediatamente: Vuelo se renueva en isla; regreso a 04 con capacidad base. Fallo: Caer conserva premio confirmado; no volver a sumarlo; primer Yopo no requerido. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-S06 / Llave / placa de armadura

Zona: MS06 / altar fijo

**Acceso:** P4, cuatro vuelos, duelo águila y suelo estable.

**Tarea:** Reconocer perfil del medallón y vínculo forzado en reverso del tercer fragmento.

**Gestos:** Inclinar borde para ver perfil; girar reverso y señalar encaje del cierre.

**Validación:** Perfil y marca observados; encaje correcto y estabilidad.

**Premio:** ms_key; armor_memory_3; llave persistente.

**Aplicación:** Esperar transporte a MS07; E en receptáculo abre última puerta.

**Fallo:** No inspección sobre plataforma móvil; no pérdida al caer; cierre no consume llave.

NEMEQUENE: Esta placa conserva permiso. La marca forzada viene de otro poder.

1. PG: águila retira alas, altar en terraza amplia.
2. PM: placa lleva medallón central.
3. PD: canto reproduce silueta de cierre.
4. PD: reverso con lazo de dos tonos, eco de Quimue.
5. PM: E registra antes del transporte.
6. PG: muelle y plataforma, destino 07 visible.

**Prompt:** Producir O-S06 Llave / placa de armadura en MS06 / altar fijo. Entrada: P4, cuatro vuelos, duelo águila y suelo estable. Mostrar la pista: Reconocer perfil del medallón y vínculo forzado en reverso del tercer fragmento. Integrar manos: Inclinar borde para ver perfil; girar reverso y señalar encaje del cierre. Validar: Perfil y marca observados; encaje correcto y estabilidad. Confirmar una sola vez: ms_key; armor_memory_3; llave persistente. Enseñar inmediatamente: Esperar transporte a MS07; E en receptáculo abre última puerta. Fallo: No inspección sobre plataforma móvil; no pérdida al caer; cierre no consume llave. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N01 / Poporo de afinidades

Zona: Meditación / entrega de Bachué

**Acceso:** C03, automático tras escuchar misión; confirmación manual antes de viaje.

**Tarea:** Inspeccionar dos cavidades de memoria y encaje del bastón.

**Gestos:** Horizontal muestra cavidades; vertical revela base; señalar una para recibir explicación.

**Validación:** Dos zonas observadas; E acepta custodia, sin contenido que se agote.

**Premio:** poporo_owned; futura coca/Yopo base; inventario de afinidades.

**Aplicación:** Coca habilita Jaguar; yopo base habilita Guacamaya.

**Fallo:** Salir de inspección no pierde la entrega; cámara solo opcional; no gesto de ingestión.

BACHUÉ: Guardará lo aprendido, no decidirá por ti.

1. PM: Bachué sostiene recipiente y lo ofrece.
2. PD: dos cavidades aún sin luz.
3. PD: giro muestra encaje del bastón.
4. PD: selección de una cavidad explica recurso.
5. PM: E acepta y poporo queda al cinto.
6. PG: mapa marca camino hacia plaza.

**Prompt:** Producir O-N01 Poporo de afinidades en Meditación / entrega de Bachué. Entrada: C03, automático tras escuchar misión; confirmación manual antes de viaje. Mostrar la pista: Inspeccionar dos cavidades de memoria y encaje del bastón. Integrar manos: Horizontal muestra cavidades; vertical revela base; señalar una para recibir explicación. Validar: Dos zonas observadas; E acepta custodia, sin contenido que se agote. Confirmar una sola vez: poporo_owned; futura coca/Yopo base; inventario de afinidades. Enseñar inmediatamente: Coca habilita Jaguar; yopo base habilita Guacamaya. Fallo: Salir de inspección no pierde la entrega; cámara solo opcional; no gesto de ingestión. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N02 / Mapa del umbral

Zona: Meditación / junto al poporo

**Acceso:** Tras O-N01; no necesita nuevo camino fuera del atlas.

**Tarea:** Seguir trazado desde Bacatá a Plaza Núñez y reconocer retorno.

**Gestos:** Inclinar mapa para ver relieve; señalar el nodo de la plaza.

**Validación:** Nodo correcto seleccionado; E confirma, abre diario.

**Premio:** map_owned; objetivo llegar a plaza; mapa no consumible.

**Aplicación:** C04 elide viaje terrestre sin añadir campaña de Tunja jugable.

**Fallo:** No interpretar todos los símbolos como datos históricos; ayuda muestra nombre del nodo.

BACHUÉ: Este lugar une caminos que tu tierra no puede mostrar.

1. PM: mapa desplegado entre ambos personajes.
2. PD: origen Bacatá y trazo continuo.
3. PD: inclinar revela nodo de tránsito.
4. PD: señalar Plaza Núñez estable 0,4 s.
5. PM: E registra mapa en diario.
6. PG: corte de viaje llega a medallón de plaza.

**Prompt:** Producir O-N02 Mapa del umbral en Meditación / junto al poporo. Entrada: Tras O-N01; no necesita nuevo camino fuera del atlas. Mostrar la pista: Seguir trazado desde Bacatá a Plaza Núñez y reconocer retorno. Integrar manos: Inclinar mapa para ver relieve; señalar el nodo de la plaza. Validar: Nodo correcto seleccionado; E confirma, abre diario. Confirmar una sola vez: map_owned; objetivo llegar a plaza; mapa no consumible. Enseñar inmediatamente: C04 elide viaje terrestre sin añadir campaña de Tunja jugable. Fallo: No interpretar todos los símbolos como datos históricos; ayuda muestra nombre del nodo. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N03 / Coca / afinidad de Jaguar

Zona: MI04 / nicho del tercer par

**Acceso:** Semilla y brazaletes 1-2 analizados; reserva antes del escudo y jefe.

**Tarea:** Reconocer correspondencia de hoja ficcional con extremo jaguar del bastón.

**Gestos:** Girar conjunto simbólico, señalar silueta del jaguar; depositar por E en poporo.

**Validación:** Tres análisis registrados y correspondencia identificada.

**Premio:** coca_affinity permanente; Jaguar habilitado en duelos apropiados.

**Aplicación:** Tutorial opcional en descanso; primer uso obligatorio en E07 se anuncia antes.

**Fallo:** No unidades que se agoten; no preparación ni consumo real; si omitido, puerta MI09 avisa y ruta de retorno queda abierta.

NEMEQUENE: El jaguar del bastón responde al poporo.

1. PG: reserva visible en nicho seguro antes de escudo.
2. PM: aviso Tres análisis completados.
3. PD: símbolo ficcional en hojas; no receta.
4. PD: señalar extremo jaguar con correspondencia.
5. PM: E lleva luz al poporo sin ingestión realista.
6. PG: ficha Jaguar lista en diario de combate.

**Prompt:** Producir O-N03 Coca / afinidad de Jaguar en MI04 / nicho del tercer par. Entrada: Semilla y brazaletes 1-2 analizados; reserva antes del escudo y jefe. Mostrar la pista: Reconocer correspondencia de hoja ficcional con extremo jaguar del bastón. Integrar manos: Girar conjunto simbólico, señalar silueta del jaguar; depositar por E en poporo. Validar: Tres análisis registrados y correspondencia identificada. Confirmar una sola vez: coca_affinity permanente; Jaguar habilitado en duelos apropiados. Enseñar inmediatamente: Tutorial opcional en descanso; primer uso obligatorio en E07 se anuncia antes. Fallo: No unidades que se agoten; no preparación ni consumo real; si omitido, puerta MI09 avisa y ruta de retorno queda abierta. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N04 / Máscara de Chía

Zona: MI08 / nicho tras cuerno

**Acceso:** Cuerno abre reja y nicho antes de MI09.

**Tarea:** Descubrir lazo oscuro en reverso y distinguir tomar de liberar.

**Gestos:** Giro completo entre dos vistas guiadas, inclinar reverso y congelar sobre lazo.

**Validación:** Anverso y reverso observados; E registra sellada, no habilita entrega.

**Premio:** chia_sealed; al vencer E07 cambia a chia_released.

**Aplicación:** C10 entrega en custodia; reflejo lunar habilita mundo superior y acciones finales.

**Fallo:** Salir no borra sello; no recibir liberación por confirmar antes del jefe.

NEMEQUENE: Puedo tomar su forma. El lazo aún retiene su respuesta.

1. PG: cuerno abre nicho lateral fuera de arena.
2. PM: máscara lunar en soporte fijo.
3. PD: anverso muestra rostro ceremonial ficticio.
4. PD: reverso revela lazo del jefe.
5. PM: E registra Máscara sellada; no mensaje de victoria.
6. PG: hilo conduce a MI09 y descanso disponible.

**Prompt:** Producir O-N04 Máscara de Chía en MI08 / nicho tras cuerno. Entrada: Cuerno abre reja y nicho antes de MI09. Mostrar la pista: Descubrir lazo oscuro en reverso y distinguir tomar de liberar. Integrar manos: Giro completo entre dos vistas guiadas, inclinar reverso y congelar sobre lazo. Validar: Anverso y reverso observados; E registra sellada, no habilita entrega. Confirmar una sola vez: chia_sealed; al vencer E07 cambia a chia_released. Enseñar inmediatamente: C10 entrega en custodia; reflejo lunar habilita mundo superior y acciones finales. Fallo: Salir no borra sello; no recibir liberación por confirmar antes del jefe. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N05 / Yopo base / afinidad de Guacamaya

Zona: Plaza / Bachué tras Chía libre

**Acceso:** E07 y C10; nunca antes de liberación lunar.

**Tarea:** Distinguir recurso base de los dos refinamientos opcionales de MS.

**Gestos:** Examinar recipiente de memoria y señalar extremo guacamaya del bastón.

**Validación:** Chía libre y entrega recibida; correspondencia ave reconocida.

**Premio:** guacamaya_affinity; usos esenciales renovables.

**Aplicación:** Primer vuelo C11 es automático cinematográfico; Alas controlan después navegación.

**Fallo:** Cancelar conserva entrega pendiente; no gastar afinidad; no exigir Yopo 1/2 para portal.

BACHUÉ: La afinidad permanece. Ningún intento te dejará sin camino.

1. PM: Bachué muestra cavidad iluminada por Chía.
2. PD: recipiente de memoria sin instrucciones de consumo.
3. PD: giro revela marca del ave.
4. PD: selección del extremo guacamaya.
5. PM: E; poporo muestra dos afinidades distintas.
6. PG: objetivo señala urna vacía junto al portal superior.

**Prompt:** Producir O-N05 Yopo base / afinidad de Guacamaya en Plaza / Bachué tras Chía libre. Entrada: E07 y C10; nunca antes de liberación lunar. Mostrar la pista: Distinguir recurso base de los dos refinamientos opcionales de MS. Integrar manos: Examinar recipiente de memoria y señalar extremo guacamaya del bastón. Validar: Chía libre y entrega recibida; correspondencia ave reconocida. Confirmar una sola vez: guacamaya_affinity; usos esenciales renovables. Enseñar inmediatamente: Primer vuelo C11 es automático cinematográfico; Alas controlan después navegación. Fallo: Cancelar conserva entrega pendiente; no gastar afinidad; no exigir Yopo 1/2 para portal. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N06 / Urna vacía y recuerdo del cuerno

Zona: Plaza / tres urnas del portal superior

**Acceso:** Después de yopo base; ramal corto de suelo estable.

**Tarea:** Inspeccionar interiores: dos tienen memoria/restos intactos, una está vacía y tiene soporte de cuerno.

**Gestos:** Seleccionar urna con señalamiento estable y pinza de confirmación; izquierda gira, derecha inclina interior.

**Validación:** Interior vacío observado y soporte reconocido; E coloca proyección de cuerno adquirido.

**Premio:** urn_solved; ms_unlocked; no nuevo cuerno ni premio duplicado.

**Aplicación:** Nota automática y C11 de guacamaya a MS01.

**Fallo:** Urna incorrecta: Aquí permanece una memoria; devolver intacta. No daño, profanación ni azar; puede comparar las tres.

NEMEQUENE: Está vacía, pero conserva un lugar para el sonido.

1. PG: tres urnas separadas, mismas luces de importancia.
2. PM: seleccionar una; muestra interior inclinado.
3. PD: dos poseen memoria, la correcta muestra cavidad vacía.
4. PD: perfil de cuerno coincide; señalamiento confirma urna.
5. PM: E; proyección del poporo suena en soporte.
6. PG: nubes abren paso; inicio C11 desde suelo seguro.

**Prompt:** Producir O-N06 Urna vacía y recuerdo del cuerno en Plaza / tres urnas del portal superior. Entrada: Después de yopo base; ramal corto de suelo estable. Mostrar la pista: Inspeccionar interiores: dos tienen memoria/restos intactos, una está vacía y tiene soporte de cuerno. Integrar manos: Seleccionar urna con señalamiento estable y pinza de confirmación; izquierda gira, derecha inclina interior. Validar: Interior vacío observado y soporte reconocido; E coloca proyección de cuerno adquirido. Confirmar una sola vez: urn_solved; ms_unlocked; no nuevo cuerno ni premio duplicado. Enseñar inmediatamente: Nota automática y C11 de guacamaya a MS01. Fallo: Urna incorrecta: Aquí permanece una memoria; devolver intacta. No daño, profanación ni azar; puede comparar las tres. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N07 / Máscara de Sué

Zona: MS08 / altar posterior a serpiente

**Acceso:** Solo después de victoria E08; no durante turno enemigo.

**Tarea:** Reconocer marcas complementarias de Chía y cerrar último vínculo.

**Gestos:** Girar reverso, comparar reflejo lunar del poporo y estabilizar ambas señales.

**Validación:** Serpiente liberada, dos caras vistas y correspondencia lunar-solar.

**Premio:** sue_released; final_return_ready.

**Aplicación:** C15 anticipa grieta; al regresar Quimue activa E11.

**Fallo:** No consumir Yopo ni exigir daño perfecto; victoria guardada aunque cancele inspección.

NEMEQUENE: Son dos respuestas, no dos órdenes.

1. PG: serpiente libera altar en arena silenciosa.
2. PM: Sué estable sobre piedra clara.
3. PD: anverso y reverso con dos puntos de respuesta.
4. PD: reflejo de Chía se alinea sin mover otra malla física.
5. PM: E registra máscara, vínculo pierde tensión.
6. PG: portal y sombra de Quimue; C15 advierte retorno.

**Prompt:** Producir O-N07 Máscara de Sué en MS08 / altar posterior a serpiente. Entrada: Solo después de victoria E08; no durante turno enemigo. Mostrar la pista: Reconocer marcas complementarias de Chía y cerrar último vínculo. Integrar manos: Girar reverso, comparar reflejo lunar del poporo y estabilizar ambas señales. Validar: Serpiente liberada, dos caras vistas y correspondencia lunar-solar. Confirmar una sola vez: sue_released; final_return_ready. Enseñar inmediatamente: C15 anticipa grieta; al regresar Quimue activa E11. Fallo: No consumir Yopo ni exigir daño perfecto; victoria guardada aunque cancele inspección. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-N08 / Bastón bicéfalo / legado

Zona: Prólogo, combate y epílogo

**Acceso:** Entrega de Saguanmachica; no escondido en un nivel.

**Tarea:** Primera entrega demuestra dos extremos; final transmite misma pieza.

**Gestos:** Inspección opcional de ambas puntas tras C01, sin bloquear prólogo.

**Validación:** No premio de habilidad por girar; su custodia narrativa cambia en C20-21.

**Premio:** staff_owned, después tisquesusa_staff; mismo objeto persistente.

**Aplicación:** Ataques humanos y acciones de vínculo; guía formas del poporo.

**Fallo:** No duplicar bastón cuando Furachogua lo entrega: Tisquesusa deposita el original y lo recibe investido.

SAGUANMACHICA: Si solo escuchas un extremo, deja de servir.

1. PD: tallado de ambas figuras en C01.
2. PM: entrega al adulto, punta segura hacia suelo.
3. PG: combate usa bastón sin arma nueva inexplicada.
4. PD: C20 mano de Nemequene deja bastón al sobrino.
5. PM: C21 sobrino deposita original junto al agua.
6. PM: Furachogua entrega máscaras; él retoma el mismo bastón.

**Prompt:** Producir O-N08 Bastón bicéfalo / legado en Prólogo, combate y epílogo. Entrada: Entrega de Saguanmachica; no escondido en un nivel. Mostrar la pista: Primera entrega demuestra dos extremos; final transmite misma pieza. Integrar manos: Inspección opcional de ambas puntas tras C01, sin bloquear prólogo. Validar: No premio de habilidad por girar; su custodia narrativa cambia en C20-21. Confirmar una sola vez: staff_owned, después tisquesusa_staff; mismo objeto persistente. Enseñar inmediatamente: Ataques humanos y acciones de vínculo; guía formas del poporo. Fallo: No duplicar bastón cuando Furachogua lo entrega: Tisquesusa deposita el original y lo recibe investido. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L01 / Fragmento tallado

Zona: MI01 / nicho lateral derecho

**Acceso:** Opcional, en ubicación de su ID ofrenda_01; nunca en apoyo inestable.

**Tarea:** Seguir una incisión que gira hacia el portal de regreso.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_01; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: Alguien marcó el regreso antes de avanzar.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Seguir una incisión que gira hacia el portal de regreso.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Fragmento tallado como ofrenda_01.
6. PG: Alguien marcó el regreso antes de avanzar. La ruta principal sigue abierta.

**Prompt:** Producir O-L01 Fragmento tallado en MI01 / nicho lateral derecho. Entrada: Opcional, en ubicación de su ID ofrenda_01; nunca en apoyo inestable. Mostrar la pista: Seguir una incisión que gira hacia el portal de regreso. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_01; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L02 / Disco agrietado

Zona: MI02 / lateral del santuario

**Acceso:** Opcional, en ubicación de su ID ofrenda_02; nunca en apoyo inestable.

**Tarea:** Distinguir grieta física de trazo completo: inclinar reverso para ver que el camino continúa.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_02; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: Una fractura no borra todo lo que una pieza conserva.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Distinguir grieta física de trazo completo: inclinar reverso para ver que el camino continúa.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Disco agrietado como ofrenda_02.
6. PG: Una fractura no borra todo lo que una pieza conserva. La ruta principal sigue abierta.

**Prompt:** Producir O-L02 Disco agrietado en MI02 / lateral del santuario. Entrada: Opcional, en ubicación de su ID ofrenda_02; nunca en apoyo inestable. Mostrar la pista: Distinguir grieta física de trazo completo: inclinar reverso para ver que el camino continúa. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_02; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L03 / Medallón calado

Zona: MI03 / lateral tras ensayo

**Acceso:** Opcional, en ubicación de su ID ofrenda_03; nunca en apoyo inestable.

**Tarea:** Mirar a través del calado y alinearlo con silueta de la puerta del atajo.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_03; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: El hueco deja ver una salida que el frente ocultaba.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Mirar a través del calado y alinearlo con silueta de la puerta del atajo.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Medallón calado como ofrenda_03.
6. PG: El hueco deja ver una salida que el frente ocultaba. La ruta principal sigue abierta.

**Prompt:** Producir O-L03 Medallón calado en MI03 / lateral tras ensayo. Entrada: Opcional, en ubicación de su ID ofrenda_03; nunca en apoyo inestable. Mostrar la pista: Mirar a través del calado y alinearlo con silueta de la puerta del atajo. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_03; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L04 / Fragmento del muro

Zona: MI04 / nicho fuera del patio hostil

**Acceso:** Opcional, en ubicación de su ID ofrenda_04; nunca en apoyo inestable.

**Tarea:** Girar hasta leer una instrucción cortada: Conservar el paso; no Cerrar todo paso.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_04; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: La orden perdió su final.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Girar hasta leer una instrucción cortada: Conservar el paso; no Cerrar todo paso.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Fragmento del muro como ofrenda_04.
6. PG: La orden perdió su final. La ruta principal sigue abierta.

**Prompt:** Producir O-L04 Fragmento del muro en MI04 / nicho fuera del patio hostil. Entrada: Opcional, en ubicación de su ID ofrenda_04; nunca en apoyo inestable. Mostrar la pista: Girar hasta leer una instrucción cortada: Conservar el paso; no Cerrar todo paso. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_04; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L05 / Raíz petrificada

Zona: MI06 / refugio lateral estable

**Acceso:** Opcional, en ubicación de su ID ofrenda_05; nunca en apoyo inestable.

**Tarea:** Comparar dos ramificaciones e identificar la que conduce a corredor de retorno.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_05; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: Este apoyo fue dejado para quien regresara.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Comparar dos ramificaciones e identificar la que conduce a corredor de retorno.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Raíz petrificada como ofrenda_05.
6. PG: Este apoyo fue dejado para quien regresara. La ruta principal sigue abierta.

**Prompt:** Producir O-L05 Raíz petrificada en MI06 / refugio lateral estable. Entrada: Opcional, en ubicación de su ID ofrenda_05; nunca en apoyo inestable. Mostrar la pista: Comparar dos ramificaciones e identificar la que conduce a corredor de retorno. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_05; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


## O-L06 / Sello de piedra

Zona: MI08 / lateral de descanso

**Acceso:** Opcional, en ubicación de su ID ofrenda_06; nunca en apoyo inestable.

**Tarea:** Inclinar sello y localizar dos huellas juntas, anticipando el respeto al guardián.

**Gestos:** Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa.

**Validación:** Vista del recuerdo válida; E archiva una sola vez.

**Premio:** ofrenda_06; archivo cultural marcado Ficción del juego.

**Aplicación:** Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo.

**Fallo:** Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador.

NEMEQUENE: Dos presencias pueden compartir un mismo umbral.

1. PG: nicho lateral señalado por forma, separado del carril.
2. PM: entrar E en suelo estable, nunca bajo ataque.
3. PD: Inclinar sello y localizar dos huellas juntas, anticipando el respeto al guardián.
4. PD: manos detienen la cara que conserva el recuerdo.
5. PM: E guarda Sello de piedra como ofrenda_06.
6. PG: Dos presencias pueden compartir un mismo umbral. La ruta principal sigue abierta.

**Prompt:** Producir O-L06 Sello de piedra en MI08 / lateral de descanso. Entrada: Opcional, en ubicación de su ID ofrenda_06; nunca en apoyo inestable. Mostrar la pista: Inclinar sello y localizar dos huellas juntas, anticipando el respeto al guardián. Integrar manos: Rotar e inclinar con el mismo sistema de manos; congelar la cara significativa. Validar: Vista del recuerdo válida; E archiva una sola vez. Confirmar una sola vez: ofrenda_06; archivo cultural marcado Ficción del juego. Enseñar inmediatamente: Aporta una línea de interpretación de Nemequene; no llave, combustible ni final alternativo. Fallo: Omitir no bloquea; al cancelar queda en nicho; revisitar archivo no suma contador. Respetar los seis paneles, cámara estable y arte del mundo; sin premio al acercarse.


# 06 / Guion 3: progresión por niveles


## T-P00 / Plaza / tutorial

Tarea: Fuente, tres estaciones a izquierda, círculo a derecha, portal inferior al norte.

Bloqueo: Lecciones y duelo; superior bloqueado hasta Chía libre + urna.

Salida: MI01; retorno temprano siempre.

1. PG: medallón sur y fuente al fondo.
2. PG: tres altares y círculo en laterales separados.
3. PD: manos realizan tres aprendizajes.
4. PG: círculo resuelve dos defensas.
5. PG: arco inferior activo con destino.
6. PG: superior muestra pista de Chía, sin interacción engañosa.

**Prompt:** Montar T-P00 Plaza / tutorial: Fuente, tres estaciones a izquierda, círculo a derecha, portal inferior al norte. Bloqueos: Lecciones y duelo; superior bloqueado hasta Chía libre + urna. Salida: MI01; retorno temprano siempre. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I01 / MI01 Umbral

Tarea: Observar raíz central, seguir suelo hacia santuario.

Bloqueo: Ninguna habilidad requerida.

Salida: MI02; portal a plaza disponible.

1. PG: salida del arco en suelo ancho.
2. PM: leer hito raíz y luz de altar.
3. PG: rampa conecta físicamente MI02.
4. PG: puerta lateral futura del atajo.
5. PD: diario objetivo Semilla.
6. PG: retorno detrás, sin rebote de portal.

**Prompt:** Montar T-I01 MI01 Umbral: Observar raíz central, seguir suelo hacia santuario. Bloqueos: Ninguna habilidad requerida. Salida: MI02; portal a plaza disponible. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I02 / MI02 Santuario

Tarea: Adquirir semilla, conversar opcional y ensayar doble salto.

Bloqueo: Semilla para apoyos altos; altar no la requiere.

Salida: MI03; descanso MI02.

1. PG: altar y custodio junto a suelo firme.
2. PD: O-I01 resuelve unión.
3. PM: confirmación de habilidad.
4. PG: primer salto sobre base ancha.
5. PG: segundo impulso en aire y aterrizaje visible.
6. PG: ruta a 03 y descanso atrás.

**Prompt:** Montar T-I02 MI02 Santuario: Adquirir semilla, conversar opcional y ensayar doble salto. Bloqueos: Semilla para apoyos altos; altar no la requiere. Salida: MI03; descanso MI02. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I03 / MI03 Galería

Tarea: Brazaletes 1, ensayo, caimán y palanca de retorno.

Bloqueo: Doble salto para aproximación; impulso aprendido antes del duelo.

Salida: MI04; atajo 03-01 abierto.

1. PG: apoyos al altar sin impulso.
2. PD: O-I02 alinea par.
3. PG: impulso en corredor seguro.
4. PM: E03 en patio ancho.
5. PD: palanca abre puerta vista en 01.
6. PG: bajada continua a 04, regreso viable.

**Prompt:** Montar T-I03 MI03 Galería: Brazaletes 1, ensayo, caimán y palanca de retorno. Bloqueos: Doble salto para aproximación; impulso aprendido antes del duelo. Salida: MI04; atajo 03-01 abierto. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I04 / MI04 Centinelas

Tarea: Brazaletes 2, murciélago, brazaletes 3, coca y escudo.

Bloqueo: Niveles y tres análisis antes del escudo; salida solo tras E05.

Salida: MI05; calle de vuelta a 03.

1. PG: nicho B2 fuera de amenaza.
2. PM: E04 separado de prueba de movilidad.
3. PD: tercer par y coca en nicho seguro.
4. PG: ensayo de cadena hacia apoyo ancho.
5. PM: E05 usa combinación por voz.
6. PG: reja 04-05 abre y baja arma.

**Prompt:** Montar T-I04 MI04 Centinelas: Brazaletes 2, murciélago, brazaletes 3, coca y escudo. Bloqueos: Niveles y tres análisis antes del escudo; salida solo tras E05. Salida: MI05; calle de vuelta a 03. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I05 / MI05 Péndulos

Tarea: Leer dos ritmos desfasados, esperar entre ambos.

Bloqueo: Movilidad obtenida; sin enemigo en primera pasada.

Salida: MI06; vuelta por mismos apoyos.

1. PG: primer peso y apoyo posterior visibles.
2. PD: pivote anuncia dirección de oscilación.
3. PG: cruzar después del peso, no perseguirlo.
4. PM: pausa en descanso intermedio.
5. PG: segundo ritmo diferente y llegada.
6. PG: rampa de salida y retorno visible.

**Prompt:** Montar T-I05 MI05 Péndulos: Leer dos ritmos desfasados, esperar entre ambos. Bloqueos: Movilidad obtenida; sin enemigo en primera pasada. Salida: MI06; vuelta por mismos apoyos. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I06 / MI06 Derrumbe

Tarea: Losa aislada, refugio, tres losas y sombras de piedras; abrir retorno.

Bloqueo: No objeto nuevo; aprendizaje de aviso.

Salida: MI07 o antesala por corredor lateral.

1. PG: una grieta aislada sobre pozo.
2. PD: vibración y polvo antes de ceder.
3. PG: jugador salta al refugio estable.
4. PG: secuencia tres losas con llegadas visibles.
5. PD: sombras anuncian estalactitas fuera del refugio.
6. PG: palanca abre corredor continuo a MI08.

**Prompt:** Montar T-I06 MI06 Derrumbe: Losa aislada, refugio, tres losas y sombras de piedras; abrir retorno. Bloqueos: No objeto nuevo; aprendizaje de aviso. Salida: MI07 o antesala por corredor lateral. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I07 / MI07 Cuerno

Tarea: E06 en aproximación, navegación conocida, inspeccionar sobre altar fijo.

Bloqueo: No cuerno necesario para adquirir cuerno.

Salida: MI08; escalera de raíces vuelve 06.

1. PG: enemigo está antes, no en aterrizaje.
2. PM: E06 resuelve silencio/presión.
3. PG: salto conocido llega a suelo fijo.
4. PD: O-I05 canal de respuesta.
5. PM: E registra cuerno.
6. PG: salida amplia hacia antesala.

**Prompt:** Montar T-I07 MI07 Cuerno: E06 en aproximación, navegación conocida, inspeccionar sobre altar fijo. Bloqueos: No cuerno necesario para adquirir cuerno. Salida: MI08; escalera de raíces vuelve 06. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I08 / MI08 Antesala

Tarea: Descanso, soporte de cuerno, nicho de Chía sellada.

Bloqueo: Cuerno abre; coca requerida para iniciar jefe con tutorial claro.

Salida: MI09; regreso 06 o plaza sin perder hallazgos.

1. PG: descanso, reja y soporte visibles.
2. PD: E en cuerno produce pulso.
3. PG: reja y nicho abren.
4. PD: O-N04 identifica lazo.
5. PM: eco Custodio y aviso Jaguar disponible.
6. PG: E inicia arena solo con requisitos listos.

**Prompt:** Montar T-I08 MI08 Antesala: Descanso, soporte de cuerno, nicho de Chía sellada. Bloqueos: Cuerno abre; coca requerida para iniciar jefe con tutorial claro. Salida: MI09; regreso 06 o plaza sin perder hallazgos. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-I09 / MI09 Cámara híbrida

Tarea: Duelo E07, Jaguar en exposición, homenaje y liberación lunar.

Bloqueo: Máscara sellada y coca; sin desgaste consumible.

Salida: Plaza tras victoria; derrota a MI08.

1. PG: jefe bloquea salida, espacio amplio.
2. PM: C07 abre combate por turnos.
3. PD: jugador aprende barrido/onda/piedras.
4. PM: C08 Jaguar rompe atadura.
5. PM: homenaje C09, Chía libre.
6. PG: retorno activo, antesala no cerrada permanentemente.

**Prompt:** Montar T-I09 MI09 Cámara híbrida: Duelo E07, Jaguar en exposición, homenaje y liberación lunar. Bloqueos: Máscara sellada y coca; sin desgaste consumible. Salida: Plaza tras victoria; derrota a MI08. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-P01 / Plaza / tránsito superior

Tarea: Entregar Chía, yopo base, comparar urnas y sonar recuerdo de cuerno.

Bloqueo: Luna libre y O-N06 resuelto.

Salida: C11 a MS01; inferior revisitable.

1. PM: Bachué recibe máscara.
2. PD: O-N05 afinidad del ave.
3. PG: tres urnas y portal alto anticipado.
4. PD: O-N06 interior vacío y soporte.
5. PM: nota abre nubes, forma guacamaya.
6. PG: vuelo de cinemática aterriza MS01.

**Prompt:** Montar T-P01 Plaza / tránsito superior: Entregar Chía, yopo base, comparar urnas y sonar recuerdo de cuerno. Bloqueos: Luna libre y O-N06 resuelto. Salida: C11 a MS01; inferior revisitable. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S01 / MS01 Umbral

Tarea: Ver cima, retorno a plaza y escalera a 02.

Bloqueo: Ninguna habilidad superior previa.

Salida: MS02; descanso 01.

1. PG: llegada y suelo claro.
2. PG: cima orienta al fondo.
3. PM: retorno señala plaza.
4. PG: escaleras continuas suben 4 m.
5. PD: objetivo primera runa.
6. PG: llegada segura 02.

**Prompt:** Montar T-S01 MS01 Umbral: Ver cima, retorno a plaza y escalera a 02. Bloqueos: Ninguna habilidad superior previa. Salida: MS02; descanso 01. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S02 / MS02 Primera runa

Tarea: Runa 1, Yopo 1 opcional, P1 y pared futura.

Bloqueo: Runa 1 activa pares; pared necesita Runa 2.

Salida: P1-P2 a 03; después pared a 04.

1. PG: altar, P1 y pared visibles juntos.
2. PD: O-S01 dos marcas del par.
3. PG: ramal corto O-S04 opcional.
4. PM: P1 muestra P2 y destino 03.
5. PG: cruzar a suelo amplio de 03.
6. PG: regreso deja pared al mismo lado visto antes.

**Prompt:** Montar T-S02 MS02 Primera runa: Runa 1, Yopo 1 opcional, P1 y pared futura. Bloqueos: Runa 1 activa pares; pared necesita Runa 2. Salida: P1-P2 a 03; después pared a 04. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S03 / MS03 Escalada

Tarea: Runa 2 y cóndor E09 en anexo estable nuevo.

Bloqueo: Runa obtenida sin escalada; duelo antes de retorno de campaña.

Salida: P2-P1, pared 02-04.

1. PG: altar sobre tierra firme junto a P2.
2. PD: O-S02 relieve de apoyos.
3. PM: cóndor en anexo ancho, fuera de salida de portal.
4. PM: E09, promesa y regreso libre.
5. PG: P2 lleva 02.
6. PG: escalada utiliza solo apoyos hasta alas 04.

**Prompt:** Montar T-S03 MS03 Escalada: Runa 2 y cóndor E09 en anexo estable nuevo. Bloqueos: Runa obtenida sin escalada; duelo antes de retorno de campaña. Salida: P2-P1, pared 02-04. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S04 / MS04 Alas

Tarea: Adquirir alas, ensayo, descanso y ramal Yopo 2.

Bloqueo: Escalada para llegar; alas para salir volando.

Salida: MS05; regreso viable.

1. PG: salida de pared junto a altar y descanso.
2. PD: O-S03 bisagra y dirección.
3. PG: ensayo corto, recuperación bajo ruta.
4. PG: ramal opcional con ida/vuelta al santuario.
5. PG: aterrizaje 05 mostrado antes del hueco.
6. PG: vuelo obligatorio 04-05 sin enemigo.

**Prompt:** Montar T-S04 MS04 Alas: Adquirir alas, ensayo, descanso y ramal Yopo 2. Bloqueos: Escalada para llegar; alas para salir volando. Salida: MS05; regreso viable. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S05 / MS05 Portal azul

Tarea: Aterrizar en isla y activar P3.

Bloqueo: Alas para llegar; Runa 1 ya activa.

Salida: P4 en apoyo inicial de 06.

1. PG: alas desaceleran sobre suelo de 05.
2. PM: apoyo renueva presupuesto de vuelo.
3. PD: P3 muestra P4 en base, no junto a llave.
4. PM: E confirma destino.
5. PG: transición deja jugador fuera del trigger.
6. PG: P4 y primer apoyo 06 visibles.

**Prompt:** Montar T-S05 MS05 Portal azul: Aterrizar en isla y activar P3. Bloqueos: Alas para llegar; Runa 1 ya activa. Salida: P4 en apoyo inicial de 06. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S06 / MS06 Camino de llave

Tarea: Cuatro vuelos con apoyos A-B-C-Llave, águila y altar; transporte después.

Bloqueo: Cada aterrizaje renueva alas; águila solo en terraza final ancha.

Salida: Muelles 06-07; P4-P3 de vuelta.

1. PG: P4 antes del primer vuelo.
2. PG: aterrizaje A con margen.
3. PG: vuelo lateral B y luego C, suelo visible.
4. PM: E10 en terraza de llave, no durante vuelo.
5. PD: O-S06 tercer fragmento y medallón.
6. PG: esperar transporte en muelle estable.

**Prompt:** Montar T-S06 MS06 Camino de llave: Cuatro vuelos con apoyos A-B-C-Llave, águila y altar; transporte después. Bloqueos: Cada aterrizaje renueva alas; águila solo en terraza final ancha. Salida: Muelles 06-07; P4-P3 de vuelta. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S07 / MS07 Antesala

Tarea: Descanso, usar llave, eco de Quimue y escaleras a cima.

Bloqueo: Llave persistente; no se consume.

Salida: MS08; derrota regresa 07.

1. PG: desembarque en muelle, no borde.
2. PM: descanso restaura salud.
3. PD: llave coincide con receptáculo.
4. PG: hoja abre hacia nicho, sin golpear personaje.
5. PM: eco Quimue advierte seguimiento.
6. PG: escalera continua alcanza marca del jefe.

**Prompt:** Montar T-S07 MS07 Antesala: Descanso, usar llave, eco de Quimue y escaleras a cima. Bloqueos: Llave persistente; no se consume. Salida: MS08; derrota regresa 07. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-S08 / MS08 Cima

Tarea: Serpiente E08, Sué en altar fijo y retorno anticipado.

Bloqueo: Llave abrió acceso; máscara solo tras victoria.

Salida: Plaza final; derrota 07.

1. PG: dos cabezas ocupan misma arena ancha.
2. PM: duelo por turnos identifica origen.
3. PD: cortar vínculos alternando sin daño por colliders.
4. PM: serpiente cede y abre altar.
5. PD: O-N07 registra Sué.
6. PG: C15 grieta y aviso antes de retorno a Quimue.

**Prompt:** Montar T-S08 MS08 Cima: Serpiente E08, Sué en altar fijo y retorno anticipado. Bloqueos: Llave abrió acceso; máscara solo tras victoria. Salida: Plaza final; derrota 07. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## T-P02 / Plaza / final y recuerdos

Tarea: Quimue, despedida, regreso cinematográfico, muerte y legado.

Bloqueo: Chía y Sué libres; no requiere yopos ni opcionales.

Salida: Créditos; copia previa para Revisitar recuerdos.

1. PG: plaza cambia solo zona del círculo.
2. PM: E11 usa máscaras y bastón.
3. PM: C18 custodia de Bachué.
4. PG: C19 regreso sin HUD y flecha fatal.
5. PM: C20 herida y bastón a sobrino.
6. PG: C21 Iguaque y créditos; memoria explícita.

**Prompt:** Montar T-P02 Plaza / final y recuerdos: Quimue, despedida, regreso cinematográfico, muerte y legado. Bloqueos: Chía y Sué libres; no requiere yopos ni opcionales. Salida: Créditos; copia previa para Revisitar recuerdos. Usar los seis paneles para mostrar pista, acción y llegada. Conservar conexiones de las guías, retornos, medidas pendientes de prueba y cámara 3D.


## M01 / Descanso

E en suelo fijo; confirmar partida guardada con ranura y objetivo. No premio repetible.

1. PG: disco seguro fuera de amenaza.
2. PM: personaje posa bastón y respira.
3. PD: sello de guardado aparece al concluir escritura.
4. PG: vuelve control con vida plena, cámara estable.


## M02 / Atajo 03-01

Palanca en MI03 abre puerta vista en MI01; ambos sentidos después.

1. PG: palanca y corredor de vuelta.
2. PM: mano acciona mango, 0,7 s.
3. PG: puerta abre sin cerrar otros pasos.
4. PG: cámara muestra entrada 01 reconocible.


## M03 / Retorno de derrumbe

Al final MI06 abrir corredor físico que conecta antesala por su lado exterior.

1. PG: refugio y palanca antes de descenso.
2. PM: activar desde suelo fijo.
3. PG: murete/reja lateral se retira.
4. PG: corredor continuo a MI08 sin repetir losas.


## M04 / Soporte de cuerno

E valida mi_horn; proyecta la pieza y reproduce nota por audio del juego.

1. PG: soporte junto a reja, fuera de arena.
2. PD: perfil de cuerno encaja.
3. PG: pulso avanza a puerta y nicho lunar.
4. PG: abertura segura; cuerno permanece en inventario.


## M05 / Portales

E muestra destino y confirma; transición sin rebote. Cada par bidireccional tras Runa 1.

1. PG: suelo ancho frente a arco.
2. PD: destino y estado activo textual.
3. PG: fundido breve, traslado único.
4. PG: llegada fuera de trigger con vista del siguiente suelo.


## M06 / Pared marcada

Runa 2 autoriza agarres visibles; subida de 02 a 04, no a 05.

1. PG: base y apoyos legibles.
2. PM: enganche desde suelo.
3. PG: escalada controla cuerpo, cámara muestra salida.
4. PG: desenganche solo con aterrizaje de 04 válido.


## M07 / Vuelo y alas

Presupuesto inicial 4 s renovable al aterrizar; no consumible; ajustar medidas jugando.

1. PG: destino visible antes de activar.
2. PM: alas despliegan, cuerpo se inclina.
3. PG: vuelo con UI de presupuesto sin ocultar apoyo.
4. PG: aterrizaje estable, recarga y control humano.


## M08 / Transporte

Ida/vuelta automática entre muelles; espera visible; E solo opcional para solicitar arribo.

1. PG: muelle y trayectoria hacia 07.
2. PG: plataforma se alinea y espera 2 s iniciales.
3. PM: personaje embarca con soporte, sin inspección.
4. PG: desembarque en suelo; caída recupera muelle del último embarque.


## M09 / Cierre de llave

E valida ms_key, abre hoja y conserva objeto.

1. PG: receptáculo fuera del barrido de puerta.
2. PD: medallón proyectado encaja.
3. PG: hoja gira hacia nicho, camino libre.
4. PG: escalera continua y objetivo jefe.


## M10 / Ofrendas y decoración

Ofrendas opcionales ya registradas por MIProgress se agrupan en archivo. Para cada ID existente, usar reverso con un recuerdo breve; no añadir llaves. Raíces, vasijas decorativas, muretes, brillos y piedras de fondo no se coleccionan.

1. PG: nicho lateral seguro separado de ruta.
2. PD: giro descubre una memoria del antiguo viajero.
3. PM: E archiva una vez, o Esc devuelve sin premio.
4. PG: ruta principal sigue disponible; ninguna puerta depende del contador.


## Ubicación de piezas
- MI01: O-L01 y arco. Nicho lateral del umbral, ruta normal hacia 02; portal siempre retorna.
- MI02: Semilla, Custodio y O-L02. Altar previo al ensayo; descanso y diálogo fuera de apoyos.
- MI03: Brazaletes 1, caimán, O-L03, palanca. Orden altar-ensayo-duelo-atajo; sin habilidad que exija obtenerse detrás.
- MI04: Brazaletes 2/3, coca, murciélago, escudo, O-L04. Nichos fuera de arenas; tercer par y coca antes del escudo. Mantener patios 10x 10m iniciales.
- MI05: Dos péndulos y descanso intermedio. Conservar medidas guía; llegada despejada; no duelo.
- MI06: Derrumbe, O-L05, palanca. Refugios estables; corredor lateral continuo a antesala antes de reja.
- MI07: Cuerno y vigía previo. Altar al final en suelo fijo; batalla antes de aproximación final.
- MI08: Soporte, Chía sellada, eco y O-L06. Nuevo nicho lateral visible al abrir reja; no cambiar acceso MI09; checkpoint 08.
- MI09: Híbrido y retorno. Arena 18x 16m inicial; mismo cierre, arte del canon y modelo de turnos.
- MS01: Umbral raíz(0;0;0). 12x 12m; retorno plaza y descanso.
- MS02: Runa 1/Yopo 1 raíz(0;4;19). 12x 12m, altar delante de pared, P1 a 03; pared no rellena terraza.
- MS03: Runa 2 raíz(38;6;19). 12x 12m y anexo de cóndor 10x 10m; P2 llega a apoyo despejado.
- MS04: Alas raíz(0;24;19). 12x 12m; salida de escalada y descanso; Yopo 2 en(-29;26;22).
- MS05: P3 raíz(34;28;19). 6x 6m; llegada libre, presupuesto renovado.
- MS06: P4(60;42;19), A(85;46;19), B(110;50;25), C(135;54;19). Cuatro vuelos hasta Llave(158;58;19); águila en terraza de llave 12x 12m.
- MS07: Antesala raíz(132;64;68). 12x 12m, transporte sube 6m, descanso y llave; tres escaleras llevan 08.
- MS08: Cima raíz(132;70;96,5). 24x 24m, serpiente bicéfala y altarSué; retorno solo tras victoria y máscara registrada.