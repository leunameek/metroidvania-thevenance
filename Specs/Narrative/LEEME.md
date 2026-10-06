# Guion de El asedio de Bacatá (propuesta 1.0, 2026-10-06)

Cuatro guiones editables del usuario: `01` historia y diálogos (H01-H21, D01-D12), `02` cinemáticas
(C01-C21) y animaciones (A01-A70), `03` objetos (O-*) y progresión por niveles (T-*, M*), `04` encuentros
(E01-E11), producción y guardado. El PDF con storyboards y las fuentes (Narrativa.pdf, GDD) se quedan
fuera del repositorio.

Decisiones tomadas con el usuario el 2026-10-06:
- Canon de diálogos: manda H (historia). Las fichas C y E siguen su texto cuando difieren.
- Combate: E01 y E07-E11 por turnos con voz (español e inglés); E03-E06 del inframundo con el dash.
- Vida del jugador: 100.
- "Revisitar recuerdos" (D13, postgame) queda fuera de alcance.
- Lista de modelos y clips pendientes: `Personajes-y-animaciones.md`.

El juego lee `Assets/_Game/Resources/Narrative/historia.json`, generado desde `01-Historia-y-dialogos.md`:

    python tools/narrative/build_story_json.py Specs/Narrative/01-Historia-y-dialogos.md Assets/_Game/Resources/Narrative/historia.json
