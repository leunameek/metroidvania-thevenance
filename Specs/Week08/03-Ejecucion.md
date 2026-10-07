# Ejecutar Plaza Núñez

1. Abrir el proyecto con Unity 6000.3.21f1 y esperar la importación.
2. Abrir `Assets/_Game/Scenes/PlazaNunez.unity` y pulsar Play.
3. WASD camina, Shift corre, Espacio salta, Q hace dash y botón derecho + mouse gira la cámara.
4. C activa la cámara. Acercarse a la estación 01 y pulsar E.
5. Mano izquierda abierta: mover horizontalmente. Derecha abierta: mover verticalmente.
   Cerrar los puños detiene los ejes. Seguir las casillas de progreso de cada estación.
6. M permite usar mouse explícitamente: arrastrar con clic izquierdo y soltar para congelar.
7. E sale al completar una estación; Esc permite salir antes y conservar su progreso parcial.
8. Completar las tres estaciones. Se pueden visitar en cualquier orden y volver a practicar.
9. Acercarse a la entrada del círculo de entrenamiento y pulsar E.
10. E ataca en tu turno. Esperar el aviso «¡AHORA!» y usar Espacio para esquivar el golpe
    directo o F para bloquear la onda amplia. Cada error repite la defensa; Esc permite salir.
11. Con tres estaciones y el duelo completados se abren los dos portales. E viaja al acercarse.
12. Explorar el umbral de cada mundo y volver por su portal de retorno. El avance se conserva.
13. H muestra la guía y los volúmenes de efectos/ambiente; V silencia; R reinicia el recorrido.

La plaza es la primera escena de Build Settings. Los destinos son umbrales de introducción,
no los mundos completos. El modelo de Nemequene ya está integrado; su animación disponible
es la del prefab existente. Esta actualización no añade un conjunto nuevo de animaciones.

La cámara solo se activa al pedirlo con C y su vista previa es local. La alternativa con mouse
permite presentar el recorrido sin cámara. La práctica de combate usa teclado, no micrófono.
Probar la sensibilidad y la detección con la cámara de la presentación antes de usar manos
frente a público. No interpretar las pruebas automatizadas como calibración de cámara.

Las instrucciones dentro del juego son la referencia inmediata. El diseño y los criterios de
verificación están en [05-Plaza-Lobby-Tutorial.md](05-Plaza-Lobby-Tutorial.md).

Pruebas de modelos: Window > General > Test Runner > EditMode > Run All.
Prueba de recorrido: Unity en batchmode, `-executeMethod PlazaValidation.Run`.
No añadir `-quit`: el propio comprobador finaliza al completar o fallar. Con gráficos activos
genera capturas en Evidence/PlazaNunez; con `-nographics` omite las capturas.

La escena Movement es el prototipo anterior y conserva sus controles y su carga automática
de MediaPipe. No confundir ese flujo con la activación mediante C en el lobby.
