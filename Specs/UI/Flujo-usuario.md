# Flujo de usuario — El asedio de Bacatá

Todo el recorrido está en un único diagrama Mermaid: inicio, tutorial obligatorio, exploración de ambos umbrales y pausa.

- [Vista previa del diagrama completo](Flujo-usuario.html).
- [Archivo Mermaid para importar](Flujo-usuario.mmd).
- [SVG](Flujo-usuario.svg) · [PNG](Flujo-usuario.png).

El archivo `.mmd` empieza directamente con `flowchart TB` y se importa completo en una sola operación. No incluye delimitadores Markdown.

## Lectura de los conectores

Los cuatro bloques forman parte del mismo diagrama. Un conector con letra continúa en el bloque con esa misma letra. Permiten mantener el recorrido completo sin flechas que atraviesen todo el lienzo.

| Letra | Destino |
| --- | --- |
| M | Menú principal. |
| T | Tutorial del lobby, con los requisitos pendientes. |
| L | Lobby libre, con los portales habilitados. |
| P | Pausa durante la exploración de cualquier zona. |

Reanudar vuelve al punto exacto donde se pausó. Salir de una inspección o cancelar un duelo lleva a T. Una defensa fallida se reintenta durante el duelo. Las tres lecciones y el duelo pueden completarse en cualquier orden.

El tutorial es obligatorio hasta completarlo por primera vez. El amarillo identifica la persistencia del tutorial entre visitas, todavía pendiente de implementación.

## Diagrama completo

```mermaid
flowchart TB
    subgraph FILA_SUPERIOR[" "]
        direction LR
        subgraph INICIO["M · INICIO"]
            direction TB
            ABRIR([Abrir el juego]) --> MENU["Menú principal"]
            MENU -->|Nueva partida| CARGA["Cargar Plaza Núñez"]
            MENU -->|Configuración| AJUSTES_INICIO["Ajustes"]
            MENU -->|Salir| SALIR_INICIO{"¿Salir?"}
            AJUSTES_INICIO -->|Volver| MENU_RETORNO(["M · Menú principal"])
            SALIR_INICIO -->|Cancelar| MENU_RETORNO
            SALIR_INICIO -->|Confirmar| FIN_INICIO([Cerrar juego])
            CARGA --> ESTADO{"¿Tutorial<br/>completado antes?"}
            ESTADO -->|No| IR_TUTORIAL(("T"))
            ESTADO -->|Sí| IR_LOBBY(("L"))
        end
        subgraph TUTORIAL["T · TUTORIAL"]
            direction TB
            T_ENTRADA(("T")) --> PRACTICA["Lobby · Tutorial obligatorio<br/>Portales bloqueados"]
            PRACTICA -->|Inspeccionar| PIEZAS["Vasija: giro horizontal<br/>Disco: giro vertical<br/>Guardián: giros y congelación"]
            PRACTICA -->|Entrenar| DUELO["Duelo<br/>Atacar, esquivar y bloquear"]
            PRACTICA -->|Esc al explorar| T_PAUSA(("P"))
            PIEZAS -->|Completar lección| REQUISITOS{"¿Tres lecciones<br/>y duelo ganado?"}
            DUELO -->|Ganar| REQUISITOS
            PIEZAS -->|Salir| T_RETORNO(["T · Continuar lo pendiente"])
            DUELO -->|Cancelar| T_RETORNO
            REQUISITOS -->|No| T_RETORNO
            REQUISITOS -->|Sí| GUARDAR["Recordar tutorial completado<br/>Por implementar"]
            GUARDAR --> T_LIBRE(("L"))
        end
        INICIO ~~~ TUTORIAL
    end

    subgraph FILA_INFERIOR[" "]
        direction LR
        subgraph EXPLORACION["L · EXPLORACIÓN"]
            direction TB
            L_ENTRADA(("L")) --> LOBBY["Lobby libre<br/>Portales habilitados"]
            LOBBY -->|Portal inferior| INFERIOR["Umbral inferior<br/>Esc: Pausa P"]
            LOBBY -->|Portal superior| SUPERIOR["Umbral superior<br/>Esc: Pausa P"]
            LOBBY -->|Esc| L_PAUSA(("P"))
            INFERIOR -->|Portal de retorno| L_RETORNO(["L · Volver al lobby"])
            SUPERIOR -->|Portal de retorno| L_RETORNO
        end
        subgraph CONSULTAS["P · PAUSA"]
            direction TB
            P_ENTRADA(("P")) --> PAUSA["Menú de pausa"]
            PAUSA -->|Reanudar| RETOMAR(["Retomar el punto anterior"])
            PAUSA -->|Diario| DIARIO["Mapa, objetivos<br/>y archivo"]
            PAUSA -->|Configuración| AJUSTES["Ajustes y controles<br/>Cámara y voz opcionales"]
            PAUSA -->|Menú principal o Salir| CONFIRMAR{"¿Confirmar<br/>acción elegida?"}
            DIARIO -->|Volver| P_RETORNO(["P · Volver a Pausa"])
            AJUSTES -->|Volver| P_RETORNO
            CONFIRMAR -->|Cancelar| P_RETORNO
            CONFIRMAR -->|Confirmar| DESTINO(["M · Menú principal<br/>o cerrar el juego"])
        end
        EXPLORACION ~~~ CONSULTAS
    end

    FILA_SUPERIOR ~~~ FILA_INFERIOR

    classDef default fill:#f6f8f5,stroke:#52665b,color:#182c23;
    classDef referencia fill:#eaf1fa,stroke:#42648a,color:#203b5b;
    classDef pendiente fill:#fff2cc,stroke:#9c6500,color:#342600;
    class IR_TUTORIAL,IR_LOBBY,T_ENTRADA,T_PAUSA,T_LIBRE,L_ENTRADA,L_PAUSA,P_ENTRADA,MENU_RETORNO,T_RETORNO,L_RETORNO,P_RETORNO,RETOMAR,DESTINO referencia;
    class ESTADO,GUARDAR pendiente;
    style FILA_SUPERIOR fill:transparent,stroke:transparent
    style FILA_INFERIOR fill:transparent,stroke:transparent
    style INICIO fill:#ffffff,stroke:#cbd5ce
    style TUTORIAL fill:#ffffff,stroke:#cbd5ce
    style EXPLORACION fill:#ffffff,stroke:#cbd5ce
    style CONSULTAS fill:#ffffff,stroke:#cbd5ce
```

## Las tres lecciones de inspección

| Pieza | Requisito |
| --- | --- |
| Vasija del eco | Giro horizontal de 28°. |
| Disco del alba | Giro vertical de 28°. |
| Guardián de jade | Ambos giros y congelación durante 0,65 s. |

## Reglas del recorrido

- Las tres inspecciones y el duelo pueden completarse en cualquier orden. Los cuatro requisitos deben cumplirse antes de habilitar los dos portales.
- El tutorial es obligatorio para acceder a los mundos; se permite pausar, consultar ayuda o abandonar. Salir de una inspección conserva su avance parcial durante la sesión. Cancelar un duelo permite intentarlo de nuevo, sin marcarlo como ganado.
- Teclado y mouse permiten completar todo el tutorial. Cámara, manos y voz son opcionales; sus ajustes y calibraciones se ofrecen desde el juego. Un fallo de dispositivo no debe impedir completar el recorrido con los controles alternativos.
- Diario también se abre con Tab y los controles con H durante la exploración. Esc en inspección sale a la plaza; en el duelo cancela el entrenamiento. Las flechas hacia Pausa corresponden al estado de exploración.
- En Configuración → Jugabilidad, Reiniciar pide confirmación y actualmente recarga la visita desde cero. Para el flujo propuesto, reiniciar una visita no debe borrar el registro de tutorial ya completado. Repetir voluntariamente las prácticas tampoco debe volver a bloquear los portales.
- Desactivar los consejos de tutorial en Configuración no equivale a completar las lecciones ni debe permitir saltarse los requisitos del primer ingreso.
- Los destinos existentes son umbrales visitables con retorno al lobby, no niveles completos. El diagrama representa ese alcance.

## Diferencia con la implementación actual

El menú, la carga de la plaza, las inspecciones, el duelo, el bloqueo de portales, los viajes de ida y vuelta y las pantallas de pausa y consulta ya existen. Actualmente el progreso del tutorial se reinicia al recargar la partida; solo los ajustes persisten.

Para que el requisito se aplique únicamente hasta la primera finalización, falta persistir el estado de tutorial completado y restaurar sus requisitos al comenzar otra visita. Los nodos amarillos representan esa ampliación propuesta. Este documento no modifica el comportamiento del juego ni añade una opción Continuar al menú.

## Referencias del proyecto

- [Diseño de la interfaz](../../DESIGN.md).
- [Lobby y tutorial](../Week08/05-Plaza-Lobby-Tutorial.md).
- [Menú de inicio](../../Assets/Nemequene/UI/Documentation/04-Menu-inicio.md).
- [Navegación de pausa y diario](../../Assets/Nemequene/UI/Scripts/MenuController.cs).
- [Requisitos y transición entre mundos](../../Assets/Prototype/Scripts/Controller/Week08/TechnicalDemoController.cs).
