# Menú centrado · referencia del usuario

## Composición

`reference.png` es la imagen aportada por el usuario. El menú usa su composición: montañas y sol en el título superior, tres acciones centradas sobre placas de piedra con adornos dorados, personaje con lanza y tela a la izquierda, lago y poblado al amanecer. La bruma y el agua conservan su movimiento suave; el título y los botones permanecen estables.

Los botones conservan texto real, foco de teclado, hover y acciones funcionales. El texto ampliado ensancha las placas sin mover su eje central. Alto contraste cambia la textura por una superficie oscura uniforme. Movimiento reducido, abrir otra vista y perder el foco detienen la animación ambiental.

El fondo completo ocupa la pantalla sin recortar sus extremos, también en 3:2 y 16:10. Las letras se centran verticalmente dentro de cada placa.

## Archivos integrados

- `Assets/Nemequene/UI/Resources/Nemequene/Menu_BacataValley.png`: fondo sin textos ni botones.
- `Assets/Nemequene/UI/Resources/Nemequene/Title_BacataMountains.png`: título con transparencia real.
- `Assets/Nemequene/UI/Resources/Nemequene/Menu_StoneButton.png`: placa sin texto, importada como sprite con bordes para adaptar su tamaño.

Las piezas se prepararon mediante la herramienta integrada `image_gen` a partir de la referencia. Los prompts completos están en `art-prompts.md`. El importador delimita el sprite dentro del PNG original sin modificar sus píxeles. El efecto ambiental se ejecuta localmente mediante un material de Unity.

## Comprobaciones

Las capturas de Unity incluyen las dimensiones de la referencia (1536 × 1024), 720p, 1080p, 1440p y 16:10; también texto al 150 % con alto contraste. El informe `validation.txt` comprueba composición, texto, contraste contra la textura real, navegación, configuración, animación, carga de partida y regreso al menú.

Resultado final del recorrido: **74 comprobaciones aprobadas, 0 errores de ejecución**. Contraste mínimo medido del texto enfocado sobre la textura de piedra: **5,45:1**.

Compilación Windows completada sin errores. El ejecutable entregado pasó configuración, entrada a la plaza, pausa y regreso al inicio; **0 errores de ejecución**. Informes: `windows-build.txt` y `windows-smoke.txt`. Captura real del ejecutable: `windows-main-menu.png`.

La revisión visual independiente señaló letras por encima del centro y recortes laterales en formatos estrechos. Ambos se corrigieron y se comprobaron en nuevas capturas. La revisión final y la documentación se completaron localmente porque el revisor auxiliar alcanzó su límite de uso.

| Hallazgo | Veredicto final |
|---|---|
| Letras desplazadas dentro de las placas | Resuelto: centradas horizontal y verticalmente |
| Recorte de tela y sol en 3:2 y 16:10 | Resuelto: composición completa adaptada al viewport |

## Ejecutar

Abre `Assets/Nemequene/UI/Scenes/Nemequene_MainMenu.unity`, o usa `Builds/Nemequene/Nemequene.exe` conservando toda la carpeta de la compilación.
