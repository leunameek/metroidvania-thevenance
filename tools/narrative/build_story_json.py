"""Builds Assets/_Game/Resources/Narrative/historia.json from the editable script
(01-Historia-y-dialogos.md, guion H01-H21 + D01-D13). Run: python build_story_json.py <md> <out>.
Flags, prerequisites and cue names are curated here; text, places and order come from the md."""
import json, re, sys

SPEAKERS = {
    "SAGUANMACHICA": ("Saguanmachica", ""), "NEMEQUENE NIÑO": ("Nemequene", "niño"),
    "NEMEQUENE ADULTO": ("Nemequene", ""), "NEMEQUENE": ("Nemequene", ""), "TISQUESUSA": ("Tisquesusa", ""),
    "BACHUÉ": ("Bachué", ""), "GUARDIÁN": ("Guardián de entrenamiento", ""), "CUSTODIO": ("Custodio de Raíces", ""),
    "HOMBRE-CAIMÁN": ("Hombre-caimán", ""), "HOMBRE-MURCIÉLAGO": ("Hombre-murciélago", ""),
    "JEFE": ("Guardián caimán-murciélago", ""), "JEFE LIBERADO": ("Guardián caimán-murciélago", "liberado"),
    "MUJER-CÓNDOR": ("Mujer-cóndor", ""), "MUJER-ÁGUILA": ("Mujer-águila", ""), "QUIMUE": ("Quimue", ""),
    "SERPIENTE A": ("Serpiente", "cabeza A"), "SERPIENTE B": ("Serpiente", "cabeza B"), "SERPIENTE": ("Serpiente", ""),
    "FURACHOGUA": ("Furachogua", ""),
}
# Flags granted when the whole sequence has been played (or skipped).
SETS = {
    "H01": ["staff_owned"], "H02": ["vision_seen"], "H03": ["poporo_owned", "map_owned"], "H04": ["hub_active"],
    "H11": ["chia_in_custody", "guacamaya_affinity"], "H12": ["urn_solved", "ms_unlocked", "first_guacamaya_seen"],
    "H15": ["quimue_echo_seen"], "H17": ["final_duel_available"], "H18": ["masks_in_custody", "return_home_seen"],
    "H19": ["canonical_wound"], "H20": ["nemequene_dead", "tisquesusa_staff"], "H21": ["campaign_complete"],
}
REQUIRES = {
    "H02": ["staff_owned"], "H03": ["vision_seen"], "H04": ["map_owned"], "H05": ["hub_active"],
    "H06": ["mi_unlocked"], "H07": ["mi_unlocked"], "H08": ["mi_unlocked"], "H09": ["mi_unlocked"],
    "H10": ["chia_sealed", "coca_affinity"], "H11": ["chia_released"], "H12": ["guacamaya_affinity"],
    "H13": ["ms_unlocked"], "H14": ["ms_rune_2"], "H15": ["eagle_resolved"], "H16": ["ms_lock_open"],
    "H17": ["sue_released"], "H18": ["quimue_defeated"], "H19": ["return_home_seen"], "H20": ["canonical_wound"],
    "H21": ["tisquesusa_staff"],
}
# Sequences whose lines are said at different moments of play: cue per line (same order as md).
CUES = {
    "H05": ["vasija", "disco", "figura", "duelo_inicio", "duelo_defensa", "portal"],
    "H06": ["custodio", "custodio", "custodio", "semilla"],
    "H07": ["caiman", "caiman", "murcielago", "tercer_par"],
    "H08": ["pendulos", "derrumbe", "atajo"],
    "H09": ["cuerno", "soporte", "soporte", "soporte"],
    "H10": ["intro", "intro", "jaguar", "liberado", "liberado"],
    "H13": ["hebilla", "condor", "condor", "pared"],
    "H14": ["alas", "aguila", "aguila", "aguila"],
    "H15": ["placa", "eco", "eco", "llave"],
    "H16": ["intro", "intro", "intro", "liberado", "eco"],
}
# D13 belongs to "Revisitar recuerdos", left out of scope on 2026-10-06.
SKIP = {"D13"}
def main(src, out):
    text = open(src, encoding="utf-8").read()
    story = text.split("# 02 / Guion 1: historia y diálogos", 1)[1]
    blocks = re.split(r"\n## (H\d\d) / ", story)
    sequences = []
    for i in range(1, len(blocks), 2):
        sid, body = blocks[i], blocks[i + 1].split("\n## Diálogos por estado")[0]
        title = body.splitlines()[0].strip()
        place = re.search(r"\*\*Lugar:\*\* (.+)", body).group(1).strip()
        objective = re.search(r"\*\*Objetivo:\*\* (.+)", body).group(1).strip()
        storyboard = re.search(r"\*\*Storyboard:\*\* (.+)", body).group(1).strip()
        paragraphs = [p.strip() for p in body.split("\n\n")]
        summary = paragraphs[2] if len(paragraphs) > 2 else ""
        lines = []
        for m in re.finditer(r"^([A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ\- ]+?)(?:, ([A-ZÁÉÍÓÚÑ ]+))?: (.+)$", body, re.M):
            raw, ctx, said = m.group(1).strip(), (m.group(2) or "").strip().lower(), m.group(3).strip()
            name, note = SPEAKERS[raw]
            lines.append({"speaker": name, "note": note or ctx, "text": said, "cue": ""})
        cues = CUES.get(sid)
        if cues:
            assert len(cues) == len(lines), (sid, len(cues), len(lines))
            for line, cue in zip(lines, cues): line["cue"] = cue
        sequences.append({"id": sid, "title": title, "place": place, "summary": summary, "objective": objective,
                          "storyboard": storyboard, "requires": REQUIRES.get(sid, []), "sets": SETS.get(sid, []),
                          "lines": lines})
    contextual = []
    part = story.split("## Diálogos por estado", 1)[1]
    for m in re.finditer(r"^- (D\d\d) / ([^:]+): (.+?) / (.+?) / (.+)$", part, re.M):
        if m.group(1) in SKIP: continue
        contextual.append({"id": m.group(1), "speaker": m.group(2).strip(), "question": m.group(3).strip(),
                           "answer": m.group(4).strip(), "rule": m.group(5).strip()})
    data = {"version": 1, "source": "Guion H01-H21 (canon), propuesta 1.0 del 2026-10-06",
            "sequences": sequences, "contextual": contextual}
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(len(sequences), "sequences,", sum(len(s["lines"]) for s in sequences), "lines,", len(contextual), "contextual")

if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
