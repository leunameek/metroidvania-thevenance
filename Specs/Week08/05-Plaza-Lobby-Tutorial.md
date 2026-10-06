# Plaza Núñez: lobby y tutorial

Escena: `Assets/_Game/Scenes/PlazaNunez.unity`.
Actualización de presentación del 12 de septiembre de 2026. Sustituye el alcance del graybox
descrito en los documentos iniciales de Week08; estos conservan su valor como diagnóstico histórico.

## Recorrido

El jugador aparece al sur, sobre un medallón de piedra y bronce. La avenida central conduce
a la fuente; las tres estaciones se encuentran a la izquierda y el círculo de entrenamiento
a la derecha. Los portales están al norte: violeta para el inferior, turquesa para el superior.
La plaza es un espacio seguro. Una caída devuelve al punto seguro del mundo actual sin perder
objetivos. No hay un límite de tiempo para examinar objetos ni para decidir el ataque.

| Estación | Pieza de práctica | Acción comprobada |
| --- | --- | --- |
| 01 | Vasija del eco | Acumular 28° de giro horizontal |
| 02 | Disco del alba | Acumular 28° de inclinación vertical |
| 03 | Guardián de jade | Ambos giros y mantener la congelación 0,65 s |

Las tres piezas están presentes desde el inicio, no desaparecen al completar y pueden revisitarse.
Entrar en inspección no otorga progreso. E termina una lección completada; Esc sale en cualquier
momento conservando el avance parcial de la sesión. Las tres estaciones y el duelo habilitan
ambos portales, independientemente del orden en que se completen.

## MediaPipe y accesibilidad

C activa la cámara local usando el plugin instalado y el modelo `hand_landmarker.bytes`.
La escena auxiliar se carga aditivamente sin su interfaz, cámaras, luces ni EventSystem.
No se graba ni se sube video. C también permite pausarla. La interfaz informa si la cámara
está apagada, no existe, espera permiso, recibe resultados o pierde una mano.

La mano izquierda abierta mueve el eje horizontal y la derecha abierta mueve el vertical.
Cerrar cada mano congela su eje. En la tercera estación hay que cerrar ambas después de
mover los dos ejes. Una mano perdida se considera ausente; se descartan desplazamientos
acumulados antiguos y se restablece la referencia al reaparecer o abrir un puño.

M selecciona explícitamente el mouse: arrastrar con botón izquierdo rota, soltar congela.
Esta alternativa puede completar el recorrido, pero no se registra como prueba de uso de manos.
Los parámetros iniciales usan sensibilidad 300° por unidad normalizada, una zona de ruido de
0,0015 y límite de desplazamiento por cuadro. Deben ajustarse tras probar cámaras reales.

## Duelo de entrenamiento

E en la entrada del círculo comienza o repite. La cámara encuadra al personaje y al guardián;
se bloquea la locomoción libre durante el duelo. Esc cancela y devuelve a la entrada con vida
restaurada. Ganar y salir también restaura la exploración.

1. Turno del jugador: E golpea. Puede esperar cuanto necesite.
2. El guardián anuncia un golpe directo durante 1,8 s. El círculo cambia a rojo.
3. Aparece «¡AHORA!» y el círculo se vuelve dorado: 2,4 s para pulsar Espacio y esquivar.
4. Defensa correcta: vuelve el turno del jugador. E golpea otra vez.
5. El guardián anuncia una onda amplia. Esperar «¡AHORA!» y pulsar F para bloquear.
6. Un tercer ataque con E gana la práctica. E devuelve a la plaza.

Una tecla anticipada no cuenta. Una defensa incorrecta o vencida reduce 10 puntos de energía
y repite exactamente esa defensa. La energía se recupera antes de poder morir. Ganar exige
los tres golpes y ambas defensas; repetir el entrenamiento no vuelve a bloquear los portales.
El combate no necesita micrófono ni reconocimiento de voz.

## Portales

Hay dos salidas y un retorno en cada destino. E cerca del arco produce sonido, fundido,
traslado y cambio de ambiente. Los destinos son dos zonas separadas de la misma escena:

- Inferior: umbral de basalto con cristales violetas, altar y ambiente grave.
- Superior: terraza clara elevada con columnas, bronce y ambiente aéreo.

Esto implementa el viaje de ida/vuelta y la primera dirección visual de ambos mundos;
no equivale a tener construidos los niveles completos del mundo inferior y superior.
El progreso se conserva al viajar y se reinicia con R o al volver a cargar el juego. No se
añade guardado persistente entre ejecuciones.

## Dirección artística y audio

Plaza de piedra cálida al atardecer, fachadas con pilastras y cornisas, jardines de laureles,
bancos de madera, faroles, fuente de dos alturas, detalles de bronce y pátina turquesa.
Se integra el prefab existente de Nemequene como visual del jugador. Los objetos usan
geometría propia (vasija torneada, disco con rayos e ídolo de jade), editable en la jerarquía.
Es una interpretación artística del lugar, no una reconstrucción histórica ni una atribución
arqueológica de las piezas ficticias.

16 WAV originales sintetizados dentro del proyecto: pasos, salto, aterrizaje, dash,
entrada de inspección, movimiento del objeto, estación completada, ataque, impacto,
bloqueo, aviso del enemigo, viaje por portal, victoria y tres ambientes en bucle.
No requieren descargas ni librerías de audio de terceros. Los pasos responden al
desplazamiento real en suelo y cambian de cadencia al correr; no suenan al estar inmóvil,
inspeccionando o teletransportándose. El sonido de giro tiene limitación de frecuencia.
H abre ayuda y controles independientes de efectos/ambiente. V silencia ambos.

Recursos en `Assets/_Game/Art/Environments/PlazaNunez` y `Assets/_Game/Audio/PlazaNunez`.
Generador explícito: `Nemequene > Plaza Núñez > Upgrade lobby and tutorial`.
Si la plaza ya está generada, no sobrescribe ediciones manuales. Antes de migrar se guarda
una copia estructural del graybox en `Scenes/Week08/Archive/PlazaNunez_Graybox.unity`.
Esa copia no es una escena de entrada ni una segunda versión del sistema del tutorial.

## Verificación

Modelos: `PlazaTutorialTests` y pruebas ampliadas de `HandGestureModelTests` en EditMode.
Recorrido integrado: `PlazaValidation.Run` en Unity batchmode. Comprueba movimiento,
rotación mediante el adaptador de mouse, cierre de lecciones, combate con error/reintento,
portales de ida/vuelta, caída en cada destino, cancelación, ayuda, audio y reinicio.
Con dispositivo gráfico también guarda capturas en `Evidence/PlazaNunez`.

La ejecución automatizada con mouse no certifica precisión ni latencia de la cámara física.
Para aceptar MediaPipe: probar buena iluminación y dos manos visibles, abrir/cerrar cada
mano, sacar una del cuadro, volver a introducirla, pausar/reanudar C y finalizar las tres
estaciones sin M. La escucha final y el ajuste de volumen también requieren una persona.
