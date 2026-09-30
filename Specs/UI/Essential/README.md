# Interfaz esencial — 15 de septiembre de 2026

## Resultado

- Exploración sin leyenda permanente de controles ni etiquetas flotantes. La vida aparece con daño o durante combate; el objetivo se muestra brevemente al cambiar. Los avisos de interacción dependen de la proximidad.
- Pausa con cinco acciones: continuar, diario, configuración, menú principal y salir. Mapa, objetivos y archivo comparten el diario. Reiniciar está en Jugabilidad; el inventario vacío no tiene acceso visible.
- Misma tipografía, fondos verdes oscuros, marfil y foco dorado del inicio en botones, pausa, configuración, diario, inspección, combate, calibración y confirmaciones.
- Inspección con instrucción de la lección y entrada activa, progreso y salida. Las descripciones y la clasificación cultural se conservan en el archivo.
- Combate con acciones por fase: atacar, ambas defensas o volver. Las reglas de reacción, fallos y progresión se conservan. La voz aparece únicamente cuando se habilita.
- Se retiran avisos repetidos, explicaciones permanentes del menú y botones inoperantes durante la carga. H y Configuración permiten consultar los controles completos.

## Comprobaciones

Resultado final: **130 comprobaciones aprobadas** (72 de interfaz y 58 del menú principal), **0 errores de ejecución**. Compilación Windows completada y recorrido del ejecutable aprobado. Las capturas del inicio están en `Title/`.

`validation.txt` registra el recorrido funcional y `title-validation.txt` el inicio independiente. `windows-smoke.txt` comprueba la compilación Windows: configuración, entrada a la plaza, pausa y regreso al inicio.

Se ejercitan teclado, mouse, pausa real, cancelación de confirmaciones, retorno del foco, accesibilidad, inspección de las tres estaciones, defensas, portales y reinicio. Capturas a 1280×720, 1920×1080, 1920×1200 y 2560×1440; muestras con texto al 150 %. La cámara auxiliar de las pruebas compone la UI sobre la escena sin permitir que la geometría la oculte.

Las capturas `menu-*` recorren pantallas de forma programática; algunas muestran estados de prueba aún sin progreso. `menu-Poporo` solo comprueba el componente de extensión, que no tiene acceso en la pausa. `dialogue-validation-fixture` usa texto de ayuda como contenido de prueba; no es una conversación narrativa añadida al juego.

Los diagnósticos internos del buscador del editor se registran aparte de los errores de ejecución. La prueba automatizada verifica la alternativa con teclado/mouse y el estado de voz no disponible; no acredita reconocimiento humano con cámara o micrófono.

## Ejecución

Abre `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity` en Unity, o ejecuta `Builds/Nemequene/Nemequene.exe` con su carpeta completa.
