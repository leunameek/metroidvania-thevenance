# Sistema de Diseño UI — El Asedio de Bacatá

**Versión:** 1.0  
**Proyecto:** El Asedio de Bacatá  
**Objetivo:** definir un estándar visual, funcional y de accesibilidad para **todos los botones, menús, HUD, ventanas, indicadores y pantallas** del juego.

---

## 1. Dirección visual

La interfaz debe sentirse como una extensión del mundo de **El Asedio de Bacatá**, no como una capa moderna puesta encima del juego.

Debe transmitir:

- mundo antiguo;
- territorio sagrado;
- guerra y asedio;
- misterio;
- naturaleza;
- espiritualidad;
- tensión;
- aventura;
- oscuridad con alta legibilidad.

La estética general será **cinematográfica, artesanal, robusta, terrosa y ligeramente mística**.

Evitar:

- futurismo;
- neón;
- cyberpunk;
- glassmorphism;
- botones de aplicación móvil;
- minimalismo excesivamente limpio;
- fantasía medieval europea genérica;
- decoraciones que reduzcan la legibilidad.

---

## 2. Materiales y formas

### Materiales visuales

La UI puede inspirarse en:

- piedra oscura;
- madera envejecida;
- arcilla;
- cuero;
- fibras tejidas;
- obsidiana;
- metal cobrizo o dorado envejecido;
- pigmentos naturales.

Las texturas deben ser sutiles. No deben dificultar la lectura.

### Formas

Usar principalmente:

- rectángulos anchos;
- marcos gruesos;
- esquinas pequeñas o ligeramente biseladas;
- marcos internos;
- simetría;
- líneas geométricas;
- rombos, triángulos, grecas y círculos como motivos secundarios.

Evitar botones completamente redondos o con forma de píldora.

Los motivos culturales deben mantenerse como **inspiración estilizada**. No presentar símbolos como reproducciones arqueológicas exactas salvo que exista una referencia documentada.

---

## 3. Paleta oficial

| Uso | Nombre | HEX |
|---|---|---|
| Fondo profundo | Obsidiana | `#16130F` |
| Fondo secundario | Tierra oscura | `#241B14` |
| Panel | Marrón cacao | `#312219` |
| Panel elevado | Arcilla oscura | `#473225` |
| Borde oscuro | Carbón | `#0E0C09` |
| Acento principal | Oro envejecido | `#B88A47` |
| Acento destacado | Oro solar | `#D0A45B` |
| Texto principal | Hueso | `#E8D8B8` |
| Texto secundario | Pergamino | `#CBB894` |
| Texto desactivado | Ceniza | `#84796A` |
| Acento natural | Musgo | `#596147` |
| Éxito | Verde mineral | `#667653` |
| Advertencia | Ocre | `#C28A3E` |
| Error / peligro | Terracota | `#9B463A` |
| Peligro intenso | Sangre seca | `#713128` |

### Reglas de color

- El fondo nunca debe ser blanco.
- El dorado sirve para foco, jerarquía y selección.
- El rojo sirve solo para peligro, pérdida o acciones destructivas.
- El verde sirve para confirmaciones y éxito.
- Evitar más de 3–4 colores dominantes en una misma pantalla.
- Nunca depender únicamente del color para transmitir un estado.

---

## 4. Tipografía

### Títulos

Usar una serif monumental, por ejemplo:

- Cinzel;
- Marcellus;
- Cormorant SC;
- equivalente visual.

Aplicar en:

- logo;
- títulos de pantalla;
- capítulos;
- nombres de jefes.

### Interfaz

Usar una serif altamente legible, por ejemplo:

- Source Serif 4;
- Spectral;
- Merriweather;
- equivalente.

Aplicar en:

- botones;
- configuraciones;
- inventario;
- objetivos;
- subtítulos;
- descripciones.

### Modo de alta legibilidad

Permitir una alternativa como:

- Atkinson Hyperlegible;
- Noto Sans;
- equivalente.

### Reglas

- Máximo 2 familias tipográficas principales.
- No usar manuscritas para información funcional.
- Las mayúsculas se reservan para títulos, etiquetas breves y estados.
- Los textos largos deben usar mayúsculas y minúsculas normales.

---

## 5. Escala y espaciado

### Resolución de referencia

`1920 × 1080`

La interfaz debe escalar correctamente a otras resoluciones.

### Tamaños recomendados

| Elemento | Tamaño aproximado |
|---|---:|
| Título principal | 72–110 px |
| Título de pantalla | 48–64 px |
| Subtítulo | 32–40 px |
| Botón principal | 30–36 px |
| Botón secundario | 26–32 px |
| Texto normal | 24–28 px |
| Texto auxiliar | 20–24 px |
| Subtítulos | 28–34 px |

### Área segura

Mantener la información crítica dentro del 90 % central de la pantalla.

Márgenes recomendados:

- 64 px horizontal;
- 48 px vertical.

### Escala de espaciado

Usar múltiplos de 8:

`8 / 16 / 24 / 32 / 40 / 48 / 64 / 80 / 96`

---

# 6. Sistema de botones

Todos los botones deben derivar del mismo componente base.

## 6.1 Primary Button

Usos:

- Nueva partida;
- Continuar;
- Confirmar;
- Guardar;
- Aplicar;
- Iniciar.

### Apariencia

- fondo marrón oscuro;
- borde exterior carbón;
- borde interior dorado;
- textura ligera;
- texto hueso;
- ornamentos geométricos pequeños en los extremos.

### Tamaño recomendado

- estándar: `520 × 96 px`;
- mínimo: `320 × 72 px`.

### Estados

**Default**  
Fondo `#312219`, borde `#B88A47`, texto `#E8D8B8`.

**Hover**  
Borde más luminoso, brillo dorado sutil y escala `1.02`.

**Pressed**  
Escala `0.98`, fondo ligeramente más oscuro y desplazamiento vertical de 2–3 px.

**Focus**  
Doble borde o marcador geométrico claramente visible. Nunca depender solo del color.

**Selected**  
Borde dorado permanente y pequeño símbolo lateral.

**Disabled**  
55 % de opacidad aproximada, texto ceniza y sin animación.

**Loading**  
Bloquear doble activación y mostrar indicador simple.

---

## 6.2 Secondary Button

Usos:

- Atrás;
- Cancelar;
- Cerrar;
- acciones secundarias.

Debe usar menos contraste y un borde más discreto que el botón primario.

---

## 6.3 Danger Button

Usos:

- eliminar partida;
- abandonar progreso;
- restablecer;
- salir sin guardar.

Mantener el mismo componente, sustituyendo parte del dorado por terracota.

Toda acción irreversible necesita confirmación.

---

## 6.4 Icon Button

Ejemplos:

- cerrar;
- regresar;
- audio;
- mapa;
- inventario;
- diario.

Tamaño interactivo mínimo recomendado: `56 × 56 px`.

El icono puede medir `24–32 px`.

---

# 7. Iconografía

Los iconos deben:

- tener siluetas claras;
- usar grosor consistente;
- funcionar en tamaños pequeños;
- evitar detalle excesivo;
- evitar metáforas tecnológicas cuando haya una alternativa coherente.

Ejemplos:

- inventario → bolsa;
- mapa → mapa/pergamino;
- objetivo → símbolo geométrico;
- peligro → símbolo triangular estilizado;
- voz → micrófono reinterpretado o símbolo de voz acompañado por ondas;
- guardado → símbolo propio del sistema, no necesariamente un disquete.

---

# 8. Paneles y contenedores

## Panel normal

Para:

- inventario;
- configuración;
- diario;
- mapa.

## Modal

Para:

- confirmaciones;
- errores;
- advertencias.

## Panel flotante

Para:

- tooltips;
- detalles de objetos;
- mensajes breves.

### Apariencia común

- fondo oscuro;
- textura leve;
- borde robusto;
- borde interior dorado;
- sombra suave;
- ornamentos pequeños.

---

# 9. Pantallas principales

## 9.1 Pantalla de inicio

Debe soportar:

1. Logo **El Asedio de Bacatá**.
2. Nueva partida.
3. Continuar.
4. Cargar partida.
5. Configuración.
6. Créditos.
7. Salir.

Opcional:

- accesibilidad rápida;
- idioma;
- perfil.

El logo domina la composición. Los botones forman un bloque claro y ordenado.

---

## 9.2 Nueva partida

Si no existe progreso, iniciar o mostrar opciones iniciales.

Si ya existe una partida:

**EXISTE UNA PARTIDA EN PROGRESO**

Opciones:

- Continuar.
- Crear nueva partida.
- Cancelar.

---

## 9.3 Cargar / guardar

Cada ranura debe poder mostrar:

- capítulo;
- zona;
- tiempo jugado;
- fecha;
- porcentaje aproximado;
- miniatura opcional.

Estados:

- vacía;
- ocupada;
- seleccionada;
- guardando;
- corrupta.

Eliminar partida siempre requiere confirmación.

---

## 9.4 Pausa

Opciones recomendadas:

- Continuar.
- Objetivos.
- Inventario.
- Mapa.
- Configuración.
- Reiniciar desde último punto.
- Volver al menú principal.

La escena sigue visible pero oscurecida.

---

# 10. Configuración

Dividir en pestañas:

1. Video.
2. Audio.
3. Controles.
4. Jugabilidad.
5. Accesibilidad.
6. Idioma.

## Video

- resolución;
- modo de pantalla;
- VSync;
- límite de FPS;
- calidad general;
- sombras;
- texturas;
- antialiasing;
- efectos;
- brillo;
- gamma;
- motion blur;
- temblor de cámara.

## Audio

- general;
- música;
- efectos;
- voces;
- ambiente;
- interfaz;
- micrófono.

Cada slider debe mostrar también un valor numérico.

## Controles

Mostrar acción + tecla/botón actual.

Ejemplo:

`Interactuar        [ E ]`

Al reasignar:

**PULSA UNA NUEVA TECLA**

## Jugabilidad

- sensibilidad;
- inversión de cámara;
- vibración;
- ayudas;
- tutoriales;
- velocidad de texto;
- pausa automática;
- indicadores de interacción.

---

# 11. Micrófono y mecánicas de voz

Esta sección es crítica para **El Asedio de Bacatá**.

Debe incluir:

- dispositivo de entrada;
- nivel detectado;
- sensibilidad;
- prueba de micrófono;
- calibración;
- ruido de fondo;
- iniciar/detener prueba.

Estados visibles:

- ESCUCHANDO;
- VOZ DETECTADA;
- VOZ DEMASIADO BAJA;
- SATURACIÓN;
- MICRÓFONO NO ENCONTRADO;
- PROCESANDO;
- NO SE ENTENDIÓ.

Nunca depender solo de un cambio de color.

**Toda mecánica basada en voz debe tener una alternativa accesible mediante selección o botón.**

---

# 12. Accesibilidad

Opciones mínimas:

- tamaño de texto;
- subtítulos;
- fondo de subtítulos;
- nombre del hablante;
- tipografía de alta legibilidad;
- alto contraste;
- reducción de movimiento;
- reducción de flashes;
- reducción de temblor de cámara;
- mantener pulsado / alternar;
- intensidad de vibración;
- indicadores visuales de sonido;
- navegación completa con teclado;
- navegación completa con mando;
- remapeo;
- sensibilidad de voz;
- alternativa a comandos hablados.

Contraste recomendado:

- texto normal: mínimo 4.5:1;
- texto grande: mínimo 3:1.

No transmitir información exclusivamente mediante color, sonido o animación.

---

# 13. Subtítulos

- centrados;
- máximo 2 líneas;
- fondo oscuro semitransparente;
- tamaño configurable;
- posibilidad de mostrar hablante.

Ejemplo:

**TISQUESUSA**  
“El camino hacia Bacatá está abierto.”

---

# 14. HUD de exploración

Debe ser mínimo y contextual.

Componentes posibles:

- objetivo actual;
- interacción;
- objeto equipado;
- estado del personaje;
- sigilo;
- peligro;
- objeto obtenido;
- accesos rápidos.

La información aparece cuando es necesaria y luego desaparece.

## Prompt de interacción

Ejemplos:

`[ E ] INTERACTUAR`

`[ X ] EXAMINAR`

Debe mantener una posición estable o estar anclado claramente al objeto.

---

# 15. Objetivos y diario

Al recibir una misión:

**NUEVO OBJETIVO**  
“Encuentra la entrada al santuario.”

Mostrar durante 3–5 segundos y almacenar en el diario.

### Diario

Columna izquierda:

- lista de misiones.

Centro:

- descripción.

Zona secundaria:

- datos adicionales.

Estados:

- activa;
- completada;
- bloqueada.

La misión activa usa una marca dorada.

---

# 16. Mapa

Mantener la paleta del resto de UI.

- fondo oscuro;
- terreno en tonos tierra;
- rutas claras;
- objetivo dorado;
- peligro terracota;
- jugador en hueso.

Controles:

- centrar jugador;
- zoom;
- filtros;
- leyenda;
- objetivo activo.

---

# 17. Inventario

Usar cuadrícula.

Cada objeto muestra:

- icono;
- cantidad;
- estado.

Al seleccionarlo:

- nombre;
- imagen;
- descripción;
- uso;
- información narrativa o funcional.

Estados de ranura:

- normal;
- seleccionada;
- vacía;
- bloqueada;
- nueva.

---

# 18. Inspección de objetos

- fondo oscurecido;
- objeto grande al centro;
- rotación;
- zoom;
- descripción;
- controles visibles.

Botones:

- Girar.
- Acercar.
- Volver.

---

# 19. Combate

Componentes posibles:

- nombre del enemigo;
- estado/vida;
- turno actual;
- acción disponible;
- indicador de voz;
- resultado del comando;
- estados alterados.

## Secuencia de combate por voz

**PREPÁRATE**  
↓  
**HABLA AHORA**  
↓  
**INTERPRETANDO**  
↓  
**ATAQUE RECONOCIDO**

Error:

**NO SE ENTENDIÓ**

Botón:

**REPETIR**

Un fallo técnico de reconocimiento no debe castigar directamente al jugador.

---

# 20. Jefes

Entrada de jefe:

- pausa breve;
- nombre;
- descriptor;
- ornamento;
- transición corta.

Ejemplo:

**QUIMUINCHATECA**  
*Zaque de Tunja*

La barra del jefe debe ser visualmente más importante que la de un enemigo normal.

---

# 21. Sigilo

Estados:

- OCULTO;
- SOSPECHA;
- DETECTADO.

Evitar un radar futurista.

Usar un ojo o símbolo de vigilancia estilizado y una progresión alrededor del icono.

---

# 22. Diálogos

Diseño:

- caja inferior;
- nombre del personaje;
- texto;
- indicador de avance.

Opcional:

- retrato;
- elecciones.

Las elecciones deben usar el mismo sistema de botones.

Ejemplo:

1. Entrar al santuario.
2. Esperar.
3. Preguntar por Bacatá.

La opción seleccionada recibe borde dorado y marcador lateral.

---

# 23. Tutoriales, tooltips y notificaciones

## Tutorial

Panel pequeño + icono + instrucción corta.

Ejemplo:

**INTERACTUAR**  
`E`

No enseñar varias mecánicas importantes al mismo tiempo.

## Tooltip

Máximo recomendado: 2–4 líneas.

## Notificaciones

Tipos:

- información → dorado;
- éxito → verde mineral;
- advertencia → ocre;
- error → terracota.

Toda notificación importante debe tener icono y texto.

---

# 24. Muerte, victoria y carga

## Muerte

**HAS CAÍDO**

Botones:

- Reintentar.
- Volver al menú.

## Capítulo completado

Mostrar:

- nombre;
- símbolo;
- resultado;
- desbloqueos.

Botón principal:

**CONTINUAR**

## Carga

Puede incluir:

- ilustración;
- logo pequeño;
- consejo;
- indicador basado en motivo solar o geometría circular.

Evitar barras de carga demasiado tecnológicas.

---

# 25. Modales y errores

Estructura estándar:

1. Título.
2. Mensaje.
3. Acción principal.
4. Acción secundaria.

Ejemplo:

**VOLVER AL MENÚ**  
“Se perderá el progreso no guardado.”

**VOLVER**  
**CANCELAR**

Una acción destructiva no debe quedar seleccionada por defecto.

Los errores deben explicar qué ocurrió y qué puede hacer el jugador.

Incorrecto:

`Error 0x00917`

Correcto:

**NO SE PUDO GUARDAR LA PARTIDA**  
“Comprueba el espacio disponible e inténtalo nuevamente.”

---

# 26. Animación

Duraciones recomendadas:

| Acción | Duración |
|---|---:|
| Hover | 100–150 ms |
| Pressed | 80–120 ms |
| Panel | 180–250 ms |
| Modal | 150–220 ms |
| Cambio de pantalla | 250–450 ms |

Usar:

- fade;
- desplazamiento leve;
- escala de 2–3 %;
- brillo controlado.

Evitar:

- rebotes;
- elasticidad exagerada;
- rotaciones fuertes;
- partículas constantes.

---

# 27. Sonido UI

El feedback debe sentirse físico.

Materiales sonoros:

- madera;
- piedra;
- metal suave;
- percusión corta;
- fibras.

Eventos:

- hover;
- seleccionar;
- confirmar;
- cancelar;
- error;
- abrir panel;
- cerrar panel.

No usar sonidos digitales o sci-fi.

---

# 28. Navegación

Todas las pantallas deben funcionar con:

- mouse;
- teclado;
- mando.

El foco siempre debe ser visible.

No dejar componentes sin navegación.

La acción principal debe ser fácil de identificar, pero las acciones destructivas nunca deben recibir foco inicial automáticamente.

---

# 29. Componentes obligatorios

Crear componentes reutilizables para:

### Buttons

- `PrimaryButton`
- `SecondaryButton`
- `DangerButton`
- `IconButton`
- `BackButton`

### Inputs

- `Toggle`
- `Checkbox`
- `Slider`
- `Dropdown`
- `InputField`
- `KeybindField`

### Navigation

- `Tab`
- `Breadcrumb`
- `Pagination`
- `Scrollbar`

### Containers

- `Panel`
- `Modal`
- `Tooltip`
- `Card`
- `Slot`

### Feedback

- `Toast`
- `Alert`
- `Loading`
- `ProgressBar`
- `VoiceMeter`

### HUD

- `ObjectiveIndicator`
- `InteractionPrompt`
- `HealthBar`
- `BossBar`
- `StealthIndicator`
- `VoiceIndicator`

Cada componente interactivo debe contemplar:

- default;
- hover;
- pressed;
- focused;
- selected;
- disabled;
- loading;
- error;
- success, cuando aplique.

---

# 30. Estructura recomendada en Unity

```text
UI/
├── Core/
│   ├── Canvas_Global
│   ├── EventSystem
│   └── UIManager
│
├── Buttons/
│   ├── BTN_Primary
│   ├── BTN_Secondary
│   ├── BTN_Danger
│   ├── BTN_Icon
│   └── BTN_Back
│
├── Inputs/
│   ├── UI_Toggle
│   ├── UI_Slider
│   ├── UI_Dropdown
│   └── UI_Keybind
│
├── Panels/
│   ├── UI_Panel
│   ├── UI_Modal
│   ├── UI_Tooltip
│   └── UI_Card
│
├── HUD/
│   ├── HUD_Objective
│   ├── HUD_Interaction
│   ├── HUD_Combat
│   ├── HUD_Boss
│   ├── HUD_Stealth
│   └── HUD_Voice
│
└── Screens/
    ├── SCR_MainMenu
    ├── SCR_SaveLoad
    ├── SCR_Pause
    ├── SCR_Settings
    ├── SCR_Inventory
    ├── SCR_Map
    ├── SCR_Journal
    ├── SCR_Death
    ├── SCR_Loading
    └── SCR_Credits
```

---

# 31. Convención de nombres

```text
BTN_   Button
PNL_   Panel
SCR_   Screen
TXT_   Text
IMG_   Image
ICO_   Icon
SLD_   Slider
TGL_   Toggle
DD_    Dropdown
HUD_   HUD
FX_    Visual Effect
SFX_   Audio UI
```

Ejemplos:

```text
BTN_MainMenu_NewGame
BTN_MainMenu_Settings
BTN_MainMenu_Quit
PNL_Settings_Audio
SLD_Audio_MasterVolume
HUD_Combat_VoiceMeter
```

---

# 32. TextMeshPro

Usar TextMeshPro en toda la interfaz.

Crear presets globales:

```text
TMP_Title
TMP_Subtitle
TMP_Button
TMP_Body
TMP_Caption
TMP_SubtitleDialogue
```

No ajustar manualmente una fuente distinta en cada prefab.

---

# 33. Reglas de consistencia

Antes de crear una pantalla nueva verificar:

- ¿usa la paleta oficial?
- ¿usa las fuentes oficiales?
- ¿usa el sistema de espaciado?
- ¿reutiliza componentes existentes?
- ¿tiene todos los estados necesarios?
- ¿funciona con mando?
- ¿funciona con teclado?
- ¿tiene suficiente contraste?
- ¿mantiene el tono histórico y terroso?
- ¿evita elementos tecnológicos innecesarios?
- ¿se lee bien sobre fondos complejos?

Si alguna respuesta es no, revisar antes de aprobar.

---

# 34. Qué NO hacer

No crear:

- botones completamente diferentes en cada menú;
- iconos de estilos distintos;
- bordes aleatorios;
- colores sin una función definida;
- neón;
- glassmorphism;
- blur excesivo;
- botones móviles genéricos;
- múltiples tipografías por pantalla;
- textos pegados al borde;
- animaciones exageradas;
- una UI plana sin relación con el mundo del juego.

---

# 35. Jerarquía visual

Prioridad:

1. Información crítica.
2. Acción principal.
3. Acción secundaria.
4. Información contextual.
5. Decoración.

La decoración siempre debe estar subordinada a la función.

---

# 36. Prompt maestro para agentes de diseño o desarrollo

```text
Estás trabajando en la interfaz del videojuego EL ASEDIO DE BACATÁ.

Debes mantener de manera estricta la identidad visual existente del proyecto. No diseñes una UI genérica.

El juego tiene una estética histórica-fantástica inspirada en el mundo muisca, con una presentación cinematográfica, oscura, terrosa, espiritual, robusta y artesanal.

OBJETIVO
Diseña o implementa la interfaz solicitada como parte de un sistema de UI reutilizable. Cualquier nueva pantalla debe parecer creada por el mismo equipo y pertenecer claramente al mismo videojuego.

IDENTIDAD VISUAL
Utiliza principalmente piedra oscura, madera envejecida, arcilla, cuero, fibras, obsidiana y metal dorado/cobrizo envejecido.

Paleta:
#16130F Obsidiana
#241B14 Tierra oscura
#312219 Marrón cacao
#473225 Arcilla oscura
#B88A47 Oro envejecido
#D0A45B Oro solar
#E8D8B8 Hueso
#CBB894 Pergamino
#596147 Musgo
#9B463A Terracota

No utilizar estética futurista, neón, glassmorphism, colores eléctricos, botones modernos de aplicación móvil ni fantasía medieval europea genérica.

FORMAS
Usa rectángulos anchos, marcos robustos, bordes internos dorados y motivos geométricos simples como rombos, triángulos, grecas y círculos. Los ornamentos deben ser pequeños y funcionales.

TIPOGRAFÍA
Usa una serif monumental para títulos y una serif altamente legible para interfaz. Referencias: Cinzel o Marcellus para títulos; Source Serif 4 o Spectral para interfaz. No uses más de dos familias principales.

BOTONES
Todos los botones derivan de un mismo componente base:
- fondo marrón oscuro;
- borde exterior oscuro;
- borde interior dorado;
- textura artesanal leve;
- texto color hueso;
- ornamentos geométricos pequeños.

Crear siempre estados: default, hover, pressed, focus, selected y disabled. Añadir loading/error/success cuando aplique.

Hover: más brillo dorado y escala aproximada 1.02.
Pressed: escala aproximada 0.98.
Focus: indicador claramente visible para teclado o mando, no solo un cambio de color.

PANELES
Los paneles deben parecer superficies físicas del mundo: oscuros, robustos, con marco dorado, textura ligera y sombra suave.

ICONOS
Usa siluetas claras, simples y con grosor consistente. Evita símbolos tecnológicos si existe una metáfora compatible con el mundo del juego.

ACCESIBILIDAD
Todas las pantallas deben funcionar con mouse, teclado y mando. Mantén foco visible, alto contraste, escalado de texto, subtítulos, reducción de movimiento y remapeo. No transmitas información únicamente mediante color.

Toda mecánica basada en voz debe tener una alternativa accesible mediante botón o selección.

ANIMACIÓN
Usa solo fade, desplazamientos pequeños, brillo y escalado de 2–3 %. Evita rebotes, elasticidad y partículas excesivas.

SONIDO
El feedback de interfaz debe sonar físico: madera, piedra, metal suave, percusión corta o fibras. No utilices sonidos digitales o sci-fi.

COMPONENTES
Antes de crear algo nuevo comprueba si puedes reutilizar:
PrimaryButton
SecondaryButton
DangerButton
IconButton
Panel
Modal
Tooltip
Card
Slot
Slider
Toggle
Dropdown
Tab
Toast
VoiceMeter
InteractionPrompt

PRINCIPIOS
coherencia > decoración
legibilidad > detalle
función > ornamento
mundo del juego > tendencias modernas de UI

Cuando generes una pantalla, entrega también:
1. jerarquía de elementos;
2. componentes reutilizados;
3. tamaños y espaciados;
4. estados interactivos;
5. navegación con teclado/mando;
6. comportamiento responsive;
7. animaciones;
8. sonidos UI;
9. consideraciones de accesibilidad;
10. nombres sugeridos de prefabs/assets para Unity.
```

---

# 37. Checklist final

- [ ] Utiliza la paleta correcta.
- [ ] Utiliza las fuentes correctas.
- [ ] Reutiliza componentes.
- [ ] Los botones tienen todos los estados.
- [ ] Existe foco para mando y teclado.
- [ ] El texto es legible.
- [ ] Funciona en 16:9.
- [ ] Escala a otras resoluciones.
- [ ] No depende exclusivamente del color.
- [ ] Tiene sonidos coherentes.
- [ ] Las animaciones son sutiles.
- [ ] La decoración no compite con la información.
- [ ] Los iconos mantienen el mismo estilo.
- [ ] Las acciones destructivas piden confirmación.
- [ ] Las mecánicas de voz tienen alternativa accesible.
- [ ] La interfaz parece parte del universo de El Asedio de Bacatá.

---

# Regla principal

Si una interfaz nueva podría pertenecer visualmente a otro videojuego sin hacer cambios, todavía no tiene suficiente identidad de **El Asedio de Bacatá**.

Toda pantalla debe combinar:

**tierra + oro envejecido + geometría + materiales artesanales + legibilidad + tensión narrativa.**
