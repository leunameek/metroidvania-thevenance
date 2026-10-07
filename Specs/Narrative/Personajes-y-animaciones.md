# Personajes y animaciones — lista de producción

Fecha: 2026-10-06. Fuente: guion H01–H21 (canon), cinemáticas C01–C21, catálogo A01–A70 y encuentros E01–E11.
Esta lista dice **qué modelos riggeados y qué clips faltan** para cubrir toda la campaña (sin el modo "Revisitar recuerdos").

## Cómo entregarlos

- Carpeta por personaje: `Assets/_Game/Art/Characters/<Personaje>/` con el modelo (`<Personaje>.fbx`) y una subcarpeta `Animations/` con un `.fbx` por clip.
- Humanoides (personas y mujeres/hombres-animal bípedos): rig compatible con Mixamo → se importan como **Humanoid** y comparten clips. Descargar de Mixamo en **FBX for Unity**, el modelo **With Skin** y cada animación **Without Skin**, con **In Place** marcado cuando exista la opción.
- Criaturas no bípedas (jaguar, guacamaya, caimán-murciélago, serpiente): rig propio → se importan como **Generic**; los clips van dentro del mismo `.fbx` o en `Animations/` con el mismo esqueleto.
- Nombre del archivo = nombre del clip (por ejemplo `Animations/Sword And Shield Block.fbx`). Yo configuro el importador y el Animator de cada carpeta.
- Nombres de Mixamo entre comillas son **sugerencias de búsqueda**; cualquier clip equivalente sirve.
- Columna "Bucle": sí = se repite (idle, caminar, vuelo); no = se reproduce una vez.

## Resumen de personajes

| # | Personaje | Tipo de rig | Estado | Dónde aparece |
|---|---|---|---|---|
| 1 | Nemequene (protagonista) | Humanoid | **Existe**, faltan clips | Todo el juego |
| 2 | Tisquesusa (sobrino) | Humanoid | Modelo existe (antes llamado Nemequene), falta rig/clips | H02, H19–H21 |
| 3 | Saguanmachica (tío, anciano) | Humanoid | **Falta** | H01, H19 |
| 4 | Nemequene niño | Humanoid | **Falta** (opcional: C01 se puede resolver con planos de manos) | H01 |
| 5 | Bachué (chamana) | Humanoid | **Falta** | H03–H05, H11–H12, H16–H18 |
| 6 | Quimue / Quimuinchateca | Humanoid | **Falta** | H15 (eco), H16–H17 |
| 7 | Furachogua | Humanoid | **Falta** | H21 |
| 8 | Invasor de oro y plata (arquero) | Humanoid | **Falta** | H19 |
| 9 | Pueblo de Bacatá (2–3 extras) | Humanoid | **Falta** (opcional) | H01, H19, H21 |
| 10 | Custodio de Raíces | Humanoid | **Falta** | H06, H09 (eco) |
| 11 | Guardián de entrenamiento | Humanoid | **Existe** (plaza), revisar clips | H05 |
| 12 | Hombre-caimán (también versión con escudo) | Humanoid | Modelado según GDD, falta rig/clips | H07 |
| 13 | Hombre-murciélago (vigía y vigía del cuerno) | Humanoid | Modelado según GDD, falta rig/clips | H07, H09 |
| 14 | Mujer-cóndor | Humanoid | Modelado según GDD, falta rig/clips | H13 |
| 15 | Mujer-águila | Humanoid | **Falta** | H14 |
| 16 | Jefe caimán-murciélago | Generic | **Falta** | H10 |
| 17 | Serpiente bicéfala | Generic | **Falta** | H16 |
| 18 | Jaguar (transformación) | Generic (cuadrúpedo) | **Falta** | H10, H17 |
| 19 | Guacamaya (transformación) | Generic (ave) | **Falta** | H12 |

## Objetos y accesorios (sin rig, solo modelo)

Bastón bicéfalo (jaguar/guacamaya), poporo, mapa, máscara de Chía, máscara de Sué, cuerno, semilla de raíces, 3 pares de brazaletes, hoja de coca (ficcional), recipiente de yopo base y 2 de yopo opcional, 3 fragmentos de armadura (hebilla, brazal, placa-llave), 3 urnas funerarias (una vacía con soporte), escudo del hombre-caimán, arco del invasor y flecha, arco/proyectil del hombre-murciélago, 6 ofrendas (fragmento tallado, disco agrietado, medallón calado, fragmento de muro, raíz petrificada, sello de piedra). Las alas ya existen en `Art/Equipment/Alas`.

## 1. Nemequene (jugador) — Humanoid

Ya existen: Orc Idle, Walking, Running, Jump, Jumping Up, Front Flip, Falling Idle, Hard Landing, Climbing Ladder, Climbing To Top, Hanging Idle, Flying.

| Clip | Mixamo sugerido | Bucle | Uso (ID) |
|---|---|---|---|
| Idle de combate (bastón alto) | "Sword And Shield Idle" / "Standing Melee Idle" | sí | A01, todos los duelos |
| Idle de diálogo | "Idle" / "Standing Idle" (relajado) | sí | A01, conversaciones |
| Aterrizaje suave | "Landing" | no | A04 |
| Dash / impulso | "Running Slide" o "Sprinting Forward Roll" (o "Dodging" corto) | no | A06 |
| Escalar pared | "Climbing" / "Climbing Up Wall" | sí | A08 (Runa 2) |
| Colgar y descansar en pared | "Hanging Idle" (ya existe) | sí | A08 |
| Planeo / vuelo con alas: ascender y descender | "Flying" (existe) + "Falling" | sí | A10 |
| Aterrizar desde vuelo | "Falling To Landing" | no | A11 |
| Inspeccionar: alcanzar objeto | "Picking Up Object" / "Reaching Out" | no | A12 |
| Inspeccionar: sostener | "Holding Idle" / "Looking At Object" | sí | A12 |
| Confirmar / guardar objeto | "Put Back Item" / "Taking Item" | no | A14 |
| Equipar (brazaletes, alas) | "Pulling Out" / "Wiping Hands" | no | A15 |
| Ataque básico de bastón (estocada) | "Stable Sword Inward Slash" / "Standing Melee Attack Horizontal" | no | A16 |
| Contraataque (golpe descendente) | "Standing Melee Attack Downward" | no | A17 |
| Esquiva izquierda | "Dodging Left" / "Standing Dodge Left" | no | A18 |
| Esquiva derecha | "Dodging Right" / "Standing Dodge Right" | no | A18 |
| Bloqueo: entrar / sostener / impacto | "Sword And Shield Block" + "Sword And Shield Block Idle" + "Standing Block React Large" | no / sí / no | A19 |
| Parar (parry) | "Sword And Shield Parry" / "Standing Melee Parry" | no | A20 |
| Cubrirse / salir de cobertura | "Crouch To Stand" + "Stand To Crouch" (o "Taking Cover") | no | A21 |
| Flanquear (paso lateral) | "Left Strafe" corto | no | A22 |
| Acercarse / anclar postura | "Walking" corto + "Standing Melee Taunt" / "Power Up" | no | A22 |
| Recibir golpe leve | "Hit Reaction" / "Standing React Small From Front" | no | A23 |
| Perder equilibrio (golpe fuerte) | "Standing React Large From Front" | no | A23 |
| Derrota jugable (rodilla al suelo) | "Kneeling" / "Standing To Kneel" | no | A24 |
| Levantarse en descanso | "Kneeling To Standing" | no | A24 |
| Usar cuerno en soporte | "Pushing Button" / "Reaching Out" | no | A29 |
| Palanca | "Lever Pull" / "Pulling Lever" | no | A30 |
| Llave en receptáculo | "Opening Door" / "Inserting Key" | no | A30 |
| Descansar / sentarse en descanso | "Sitting Idle" / "Kneeling Idle" | sí | A30, M01 |
| Honrar criatura (arrodillarse) | "Kneel" + "Kneeling Idle" | no + sí | A31 |
| Entregar / recibir objeto | "Giving Item" / "Receive" | no | A32 |
| Hablar | "Talking" (2 variantes) | sí | A33 |
| Escuchar | "Idle" / "Thinking" | sí | A33 |
| Señalar | "Pointing" | no | A33 |
| Meditar sentado | "Sitting Meditation" / "Praying" | sí | A60, C03 |
| Herida por flecha y caída | "Dying" / "Falling Back Death" (sin el último tramo) | no | A58, C19 |
| Herido hablando (acostado) | "Laying Idle" / "Injured Idle" | sí | A58, C20 |
| Muerte en reposo | "Dying" lento / último fragmento de "Laying" | no | A58, C20 |
| Liberar transformación (pose para Jaguar/Guacamaya) | "Power Up" / "Charge" | no | A25, A27 |

## 2. Tisquesusa — Humanoid (reutiliza el modelo que antes se llamaba Nemequene)

Comparte los clips de diálogo con Nemequene (A33, A60). Además:

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Idle | "Idle" | sí | general |
| Caminar | "Walking" | sí | A60 |
| Visión (se lleva la mano a la cabeza) | "Headache" / "Rubbing Head" | no | A57, C02 |
| Hablar / escuchar | "Talking", "Thinking" | sí | C02, C20 |
| Arrodillarse junto a herido | "Kneeling Down" + "Kneeling Idle" | no + sí | C19, C20 |
| Levantar / cargar a herido | "Carry" / "Picking Up" pesado | no | A57, C19 |
| Recibir bastón | "Receive" / "Taking Item" | no | A57, C20 |
| Cubrir cuerpo | "Kneeling" + "Put Down" | no | A60, C20 |
| Meditar junto al agua | "Sitting Meditation" | sí | C21 |
| Depositar bastón y recibirlo | "Put Down" + "Picking Up" | no | A32, C21 |
| Gritar ("¡Nemequene!") | "Yelling" | no | C19 |

## 3. Saguanmachica — Humanoid (anciano)

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Idle de anciano | "Old Man Idle" | sí | general |
| Tallar (sentado, manos trabajando) | "Sitting Working" / "Typing" sentado adaptado | sí | A56, C01 |
| Entregar bastón | "Giving Item" | no | A56, C01 |
| Hablar | "Talking" | sí | C01 |
| Herido sosteniendo el paso (de pie, apoyado) | "Injured Idle" / "Wounded Idle" | sí | C19 |
| Morir (caer, mano que cede) | "Dying" lento | no | A56, C19 |

## 4. Nemequene niño — Humanoid (opcional)

Idle, "Sitting" (mirando el tallado), "Talking" corto. Si no hay modelo, C01 se hace solo con planos de manos y el corte niño→adulto se resuelve con fundido.

## 5. Bachué — Humanoid

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Idle sereno | "Idle" femenino / "Standing Idle" | sí | junto a la fuente |
| Caminar (aparición por el sendero) | "Female Walk" | sí | A34, C03 |
| Aparición / ritual (brazos que se elevan) | "Praying" / "Spell Cast" lento | no | A34 |
| Custodia: colocar máscara en la fuente | "Put Back Item" / "Placing Item" | no | A34, C10, C16, C18 |
| Entregar objeto (poporo, mapa, yopo) | "Giving Item" | no | C03, C10 |
| Hablar (2 variantes) / escuchar / señalar | "Talking", "Thinking", "Pointing" | sí / sí / no | A33 |
| Sentada meditando (opcional) | "Sitting Meditation" | sí | C03 |

## 6. Quimue — Humanoid

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Entrar por la grieta (caminar amenazante) | "Standing Walk Forward" (paso lento y pesado) | sí | A53, C16 |
| Idle erguido / exigir máscara | "Standing Taunt" / "Idle" | sí | A53 |
| Idle de combate | "Magic Idle" / "Standing Melee Idle" | sí | A53 |
| Ataque lunar (barrido lateral) | "Standing 1H Magic Attack 01" / "Mutant Swiping" | no | A54 |
| Ataque solar (brazo arriba, golpe frontal) | "Standing 2H Magic Attack 01" / "Spell Cast" | no | A54 |
| Unión (dos signos, preparación larga) | "Standing 2H Cast Spell" | no | A54 |
| Recibir golpe / perder anclaje | "Hit Reaction" + "Standing React Large" | no | A55 |
| Vencido: caer de rodillas | "Standing To Kneel" / "Defeated" | no | A55, C17 |
| De rodillas exhausto | "Kneeling Idle" / "Defeated Idle" | sí | C17 |
| Hablar | "Talking" | sí | C16 |

## 7. Furachogua — Humanoid

"Female Idle"; emerger del agua ("Getting Up" / "Rising"); "Giving Item" (entregar máscaras); "Talking"; retirarse ("Walking Backwards" + fundido).

## 8. Invasor de oro y plata — Humanoid

"Idle" armado; "Standing Aim Walk Forward" / "Bow Idle Aim"; "Standing Draw Arrow"; "Standing Aim Recoil" (disparo). Solo C19, sin combate jugable.

## 9. Pueblo de Bacatá (opcional)

"Idle", "Walking", "Terrified" / "Running Scared" (C19), "Talking" (C21).

## 10. Custodio de Raíces — Humanoid

"Idle" lento (anclado, no camina); "Talking" / "Thinking"; "Pointing" (señala altar/cuerno); para el eco de MI08 se reutilizan los mismos clips con material translúcido.

## 11. Guardián de entrenamiento — Humanoid (existe)

Revisar que tenga: idle de combate, ataque directo ("Standing Melee Attack Horizontal"), onda ("Standing Melee Attack 360" / "Ground Slam"), recibir golpe, bajar arma ("Sword And Shield Idle" → "Idle"), señalar portal ("Pointing").

## 12. Hombre-caimán (y caimán de escudo) — Humanoid

Mismo modelo; el escudo es un prop en la mano.

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Idle / espera | "Mutant Idle" | sí | A36 |
| Amenaza (rugido) | "Mutant Roaring" | no | A36 |
| Guardia frontal | "Sword And Shield Block Idle" | sí | A36, E03 |
| Caminar | "Mutant Walking" | sí | patrulla (combate por dash) |
| Mordida (ataque directo) | "Mutant Punch" / "Zombie Attack" | no | A37 |
| Coletazo (giro) | "Mutant Swiping" / "Standing Melee Attack 360" | no | A37 |
| Carga con escudo | "Sword And Shield Run" + "Shield Bash" | no | A38, E05 |
| Escudo roto (retroceso) | "Standing React Large From Front" | no | A38 |
| Recibir golpe | "Hit Reaction" | no | general |
| Ceder / arrodillarse | "Standing To Kneel" + "Kneeling Idle" | no + sí | A39 |
| Apartarse | "Walking Backwards" / "Left Strafe Walk" | no | A39 |

## 13. Hombre-murciélago — Humanoid (+ alas como malla con huesos propios si se pueden animar)

| Clip | Mixamo sugerido | Bucle | Uso |
|---|---|---|---|
| Idle con alas entreabiertas | "Idle" encorvado / "Mutant Idle" | sí | A40 |
| Apuntar | "Standing Aim Idle" / "Bow Idle Aim" | sí | A40 |
| Disparar proyectil | "Standing Aim Recoil" / "Throw" | no | A40 |
| Elevarse / sobrevolar | "Flying" / "Jump" en el sitio | sí | A41 |
| Descenso en picada (ataque) | "Jump Attack" / "Falling To Landing" | no | A41 |
| Grito cargado (vigía del cuerno) | "Yelling" / "Mutant Roaring" | no | A41, E06 |
| Recibir golpe | "Hit Reaction" | no | general |
| Ceder / arrodillarse | "Standing To Kneel" + "Kneeling Idle" | no + sí | A39 |

## 14. Mujer-cóndor — Humanoid

"Idle" femenino de combate; golpe de ala frontal ("Standing Melee Attack Horizontal" / "Mutant Swiping"); batido de viento ("Spell Cast" con ambos brazos / "Standing 2H Magic Area Attack"); recibir golpe; plegar alas y ceder ("Standing To Kneel" o "Bowing"); hablar ("Talking").

## 15. Mujer-águila — Humanoid

"Idle" femenino; postura elevada ("Flying" / "Floating" en el sitio); carga aérea ("Jump Attack"); descarga de plumas ("Throw" / "Standing 1H Magic Attack"); interrumpida y cae al suelo ("Falling To Landing" + "Hit Reaction"); ceder ("Bowing"); hablar ("Talking").

## 16. Jefe caimán-murciélago — Generic (cuadrúpedo con alas)

No existe en Mixamo; necesita rig y clips propios: despertar (alas plegadas → abiertas), idle de arena, rugido ("No puedo soltar el paso"), barrido de ala (aviso + golpe), golpe al suelo/onda (aviso + golpe), señalar techo/caída de piedras (aviso + ejecución), recibir golpe, mostrar lazo expuesto (pecho abierto, en bucle), atadura rota (sacudida), liberado (alas bajan, mirada suave) e idle en paz.

## 17. Serpiente bicéfala — Generic

Rig con columna de muchos huesos y dos cabezas (A y B) controlables por separado: emerger, espera dual (bucle, ritmos distintos por cabeza), cabeza A presión frontal (aviso + golpe), cabeza A sacudida de fragmentos, cabeza B barrido largo, cabeza B pulso de garganta, recibir golpe en A y en B, vínculo roto en una cabeza, liberación final (retira ambas cabezas del altar), reposo.

## 18. Jaguar — Generic (cuadrúpedo)

Idle agazapado (bucle), embestida corta, recuperación y retroceso, pose de aparición (de la transformación). Si el modelo trae caminar/correr, se aprovecha para cinemáticas.

## 19. Guacamaya — Generic (ave)

Idle en suelo, despegue, vuelo con batido (bucle), planeo (bucle), aterrizaje con alas plegándose.

## Animaciones que NO necesitan clip

Péndulos, losas, estalactitas, rejas, palancas, portales, transporte, piezas flotando, fuente/lazos y HUD (A61–A70) los animo por código o con Timeline sobre los props; no hace falta descargar nada para ellos. La transformación (disolver humano → animal) es efecto visual más un corte entre los dos modelos.

## Prioridad sugerida

1. Nemequene (clips de combate e inspección), Bachué, Guardián de entrenamiento → tutorial, prólogo y primer duelo.
2. Saguanmachica, Tisquesusa → prólogo y epílogo.
3. Hombre-caimán, hombre-murciélago, jefe caimán-murciélago, jaguar, Custodio → mundo inferior.
4. Mujer-cóndor, mujer-águila, serpiente bicéfala, guacamaya → mundo superior.
5. Quimue, Furachogua, invasor, pueblo → final.
