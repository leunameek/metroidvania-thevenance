"""Builds the shared sound bank of El asedio de Bacatá (Assets/_Game/Resources/GameAudio) and
the in-place replacements of the earlier cues that did not fit (see Specs/Audio/Revision-de-audio.md).

Original procedural material only: no samples, no third-party recordings, no claim to reproduce
Muisca instruments or music. Deterministic (fixed seeds per sound).

Usage:
    python tools/audio/generate_game_audio.py            # whole bank + replacements + fixes
    python tools/audio/generate_game_audio.py ui foley   # only some groups
Originals of every replaced or repaired file are copied once to Audio_Originales/ (repo root).
"""
import os
import shutil
import sys
import zlib

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import dsp  # noqa: E402
import sources as src  # noqa: E402
from dsp import (SR, bandpass, body, cloth, decay, decorrelate, env, fade_head, fade_tail, highpass,  # noqa: E402
                 knock, layer, loop_crossfade, loop_fold, lowpass, mix_at, n_of, noise, norm_peak, norm_rms,
                 pan, phase, pink, rattle, read, space, whoosh, wobble, write)

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
BANK = os.path.join(ROOT, "Assets", "_Game", "Resources", "GameAudio")
BACKUP = os.path.join(ROOT, "Audio_Originales")
MI = os.path.join(ROOT, "Assets", "_Game", "Resources", "MIAudio")
MS = os.path.join(ROOT, "Assets", "_Game", "Resources", "MSAudio")
PLAZA = os.path.join(ROOT, "Assets", "_Game", "Audio", "PlazaNunez")
MENU = os.path.join(ROOT, "Assets", "_Game", "UI", "Resources", "Nemequene")

# Peak targets per family (dBFS). The runtime mix sets the rest per channel.
PEAK = {"ui": -12, "step": -6, "foley": -6, "combat": -3, "voice": -3, "amb": -9, "music": -3, "sting": -3}
WRITTEN = []


def seeded(name):
    dsp.seed(zlib.crc32(name.encode()))


def out(folder, name, x, family, rms=None):
    path = os.path.join(BANK, folder, name + ".wav")
    x = np.asarray(x, float)
    looping = name.endswith(("_bucle", "_presencia")) or name == "quimue_zumbido"
    if x.ndim == 1 and not looping:  # one-shots always end in silence, without a step
        x = fade_tail(fade_head(x), min(.12, len(x) / SR * .15))
    if rms is not None:
        x = norm_rms(x, rms, PEAK[family])
    else:
        x = norm_peak(x, PEAK[family])
    write(path, x)
    WRITTEN.append(path)


def replace(path, x, peak, rms=None):
    """Overwrites an existing clip (same GUID, so scene and code references stay) after backing it up."""
    backup(path)
    x = np.asarray(x, float)
    write(path, norm_rms(x, rms, -3) if rms is not None else norm_peak(x, peak))
    WRITTEN.append(path)


def backup(path):
    """Copies the original once; returns the copy so repairs always start from the original."""
    rel = os.path.relpath(path, ROOT)
    target = os.path.join(BACKUP, rel)
    if not os.path.exists(target):
        os.makedirs(os.path.dirname(target), exist_ok=True)
        shutil.copy2(path, target)
    return target


def variants(folder, base, count, family, make):
    for i in range(1, count + 1):
        seeded(f"{base}_{i}")
        out(folder, f"{base}_{i}", make(i), family)


# ======================================================================== interface

def ui_focus(i):
    return knock("hollow_wood", 1150 + 70 * i, .09, .35, 2.2)


def ui_confirm():
    return space(layer(knock("clay", 520, .35, .7), (src.seed_shaker(.1, 5000), .18, .01)), "small_room", .12)


def ui_back():
    return space(knock("hollow_wood", 330, .25, .45, 1.3), "small_room", .12)


def ui_open():
    unfurl = cloth(.32, 3200, .5) * np.linspace(.4, 1, n_of(.32))
    return space(layer((unfurl, .6), (knock("hollow_wood", 440, .2, .3, 1.4), .7, .22)), "small_room", .12)


def ui_close():
    fold = cloth(.26, 2600, .5) * np.linspace(1, .3, n_of(.26))
    return space(layer((fold, .55), (knock("hollow_wood", 300, .2, .3, 1.6), .6, .16)), "small_room", .1)


def ui_blocked():
    a = knock("clay", 190, .14, .3, 3.0)
    return layer((a, 1), (a * .8, 1, .09))


def ui_tick():
    return knock("seed", 3200, .03, .5, 1.0) * .6


def ui_tab():
    return layer((cloth(.12, 4500, .3), .7), (knock("hollow_wood", 900, .08, .3, 2.0), .4, .03))


def ui_objective():
    a = body("clay", 587.3, 1.2, .5, .5)
    b = body("clay", 880, 1.2, .5, .5)
    return space(layer((a, .8), (b, .7, .16), (src.seed_shaker(.3, 4200), .12, .02)), "hall", .25)


def ui_dialogue():
    return cloth(.07, 3000, .2) * .5


def build_ui():
    variants("UI", "ui_foco", 3, "ui", ui_focus)
    for name, make in [("ui_confirmar", ui_confirm), ("ui_atras", ui_back), ("ui_abrir", ui_open), ("ui_cerrar", ui_close),
                       ("ui_bloqueado", ui_blocked), ("ui_ajuste", ui_tick), ("ui_pestana", ui_tab),
                       ("ui_objetivo", ui_objective), ("ui_dialogo", ui_dialogue)]:
        seeded(name)
        out("UI", name, make(), "ui")


# ======================================================================== movement and handling

def step(surface, i):
    r = dsp.rng
    heel = .55 + .25 * r.random()
    if surface == "piedra":       # leather sole on dressed stone: thud, scuff, a little grit
        x = layer((body("stone", 140 + 30 * r.random(), .12, .3, 2.5), .5),
                  (bandpass(noise(.09), 900, 5000) * decay(n_of(.09), 55), .5),
                  (rattle(.07, 500, 6000, .4) * .25, 1, .01),
                  (lowpass(noise(.06), 300) * decay(n_of(.06), 40), heel))
        return space(x, "open_air", .08, .12)
    if surface == "cueva":        # wet stone in a cave: softer scuff, a squelch, a short echo
        x = layer((body("stone", 120 + 25 * r.random(), .12, .25, 2.8), .45),
                  (bandpass(noise(.08), 700, 3500) * decay(n_of(.08), 45), .45),
                  (src.bubble(700 + 500 * r.random(), .04, .4), .18 * r.random(), .03),
                  (lowpass(noise(.05), 250) * decay(n_of(.05), 40), heel))
        return space(x, "cave", .18, .5)
    if surface == "tierra":       # packed earth and grass: crunch and swish
        crunch = rattle(.12, 900, 2500, .5) * .7
        x = layer((lowpass(noise(.08), 500) * decay(n_of(.08), 35), heel),
                  (crunch, .6, .005), (bandpass(noise(.14), 2500, 8000) * env(n_of(.14), .02, .1), .18))
        return space(x, "open_air", .05, .1)
    if surface == "madera":
        x = layer((body("hollow_wood", 110 + 25 * r.random(), .22, .35, 1.4), .8),
                  (bandpass(noise(.06), 1200, 5000) * decay(n_of(.06), 60), .3))
        return space(x, "small_room", .1, .15)
    if surface == "agua":
        splash = bandpass(noise(.25), 600, 6000) * env(n_of(.25), .005, .2)
        x = layer((splash, .6), (lowpass(noise(.08), 300) * decay(n_of(.08), 30), .5),
                  *[(src.bubble(400 + 1500 * r.random(), .05, .3), .25, .02 + .1 * r.random()) for _ in range(5)])
        return space(x, "open_air", .05, .15)
    raise ValueError(surface)


def land(surface, heavy):
    k = 1.8 if heavy else 1.0
    x = layer((step(surface, 1), 1.0), (step(surface, 2), .8, .03),
              (lowpass(noise(.6), 160) * decay(n_of(.6), 14 / k), .9 * k), (cloth(.2, 2500), .25))
    if heavy:
        x = layer((x, 1), (rattle(.4, 300, 3000, .6), .2, .02))
    return x


def jump(i):
    return layer((step("piedra", i), .5), (cloth(.22, 3000, .4), .5, .01), (whoosh(.25, 300, 900, .9, .3), .35, .02))


def dash(i):
    return layer((whoosh(.38, 250 + 40 * i, 1800, 1.0, .35), 1), (cloth(.3, 3600, .5), .35))


def build_foley():
    for surface, count in [("piedra", 6), ("cueva", 6), ("tierra", 6), ("madera", 4), ("agua", 4)]:
        variants("Foley", "paso_" + surface, count, "step", lambda i, s=surface: step(s, i))
        seeded("aterrizaje_" + surface)
        out("Foley", "aterrizaje_" + surface, land(surface, False), "foley")
    seeded("aterrizaje_fuerte")
    out("Foley", "aterrizaje_fuerte", land("piedra", True), "foley")
    variants("Foley", "salto", 3, "foley", jump)
    variants("Foley", "impulso", 3, "foley", dash)
    # Handling an object: lift (fabric + clay leaving the stone), turn, set down.
    seeded("objeto_tomar")
    out("Foley", "objeto_tomar", layer((cloth(.3, 3000), .5), (body("clay", 300, .25, .2, 2), .6, .05),
                                        (bandpass(noise(.12), 1500, 6000) * decay(n_of(.12), 30), .3, .02)), "foley")
    variants("Foley", "objeto_girar", 3, "foley",
             lambda i: bandpass(noise(.22), 900 + 150 * i, 4500) * env(n_of(.22), .05, .12) * (1 + .5 * np.sin(phase(18, n_of(.22)))))
    seeded("objeto_soltar")
    out("Foley", "objeto_soltar", space(layer((body("clay", 220, .4, .5, 1.4), 1), (body("stone", 160, .2, .3, 2.5), .4),
                                              (cloth(.15, 2500), .2)), "small_room", .1), "foley")
    # Inspection opens/closes: air and the piece brought close.
    seeded("examinar_abrir")
    out("Foley", "examinar_abrir", layer((whoosh(.45, 300, 1200, 1.0, .5), .5), (body("clay", 420, .4, .3, 1.2), .5, .3)), "foley")
    seeded("examinar_cerrar")
    out("Foley", "examinar_cerrar", layer((whoosh(.35, 900, 300, 1.0, .4), .45), (body("clay", 300, .3, .3, 1.6), .5, .2)), "foley")


# ======================================================================== combat

def staff_swing(i, length=.32, low=350, high=2200):
    return layer((whoosh(length, low + 30 * i, high, .8, .45), 1), (body("wood", 180, .05, .2, 4), .15))


def impact(material, i):
    r = dsp.rng
    thud = lowpass(noise(.12), 400) * decay(n_of(.12), 30)
    if material == "piedra":
        x = layer((body("stone", 160 + 30 * i, .5, .7, .9), 1), (body("wood", 240, .2, .6, 2), .6), (thud, .6),
                  (rattle(.3, 400, 4000, .5), .25, .01))
        return space(x, "stone_room", .15, .6)
    if material == "criatura":   # thick hide: dull body, a slap, the staff's wood
        x = layer((lowpass(noise(.2), 900) * decay(n_of(.2), 22), 1), (thud, .8),
                  (bandpass(noise(.05), 1500, 4000) * decay(n_of(.05), 70), .5), (body("wood", 230, .15, .5, 2.2), .5))
        return space(x, "open_air", .1, .3)
    if material == "escamas":    # dry scales: rasp and thud
        x = layer((thud, 1), (bandpass(noise(.18), 2000, 8000) * decay(n_of(.18), 25), .5),
                  (rattle(.15, 600, 3000, .4), .35), (body("wood", 230, .15, .5, 2.2), .5))
        return space(x, "open_air", .1, .3)
    if material == "plumas":     # feathers: muffled thud and a flutter
        x = layer((thud, .9), (cloth(.3, 5000, .6), .6, .01), (src.wings(.35, 1, 300, 1400), .35, .03),
                  (body("wood", 230, .12, .4, 2.4), .4))
        return space(x, "sky", .12, .4)
    if material == "espiritu":   # Quimue: gold and silver struck, air
        metal = "gold" if i % 2 else "silver"
        x = layer((body(metal, 330 + 40 * r.random(), 1.6, .6), .7), (thud, .5), (whoosh(.4, 1500, 400, 1.0, .2), .4))
        return space(x, "hall", .2, .8)
    if material == "entrenamiento":   # the practice guardian: carved wood on a stone core
        x = layer((body("wood", 190 + 20 * i, .35, .7, 1.2), 1), (body("stone", 130, .3, .4, 1.5), .5), (thud, .6))
        return space(x, "open_air", .1, .4)
    raise ValueError(material)


def player_hurt(i):
    return layer((lowpass(noise(.25), 500) * decay(n_of(.25), 16), 1), (cloth(.25, 3000, .6), .5),
                 (bandpass(noise(.06), 800, 3000) * decay(n_of(.06), 50), .4), (src.frame_drum(55 + 5 * i, .5, .1, 1.6), .5))


def build_combat():
    variants("Combate", "baston_aire", 3, "combat", lambda i: staff_swing(i))
    for material, count in [("piedra", 3), ("criatura", 3), ("escamas", 2), ("plumas", 2), ("espiritu", 2), ("entrenamiento", 3)]:
        variants("Combate", "impacto_" + material, count, "combat", lambda i, m=material: impact(m, i))
    variants("Combate", "bloqueo", 2, "combat", lambda i: space(layer((body("wood", 200 + 20 * i, .3, .8, 1.2), 1),
                                                                         (bandpass(noise(.15), 1800, 6000) * decay(n_of(.15), 25), .4, .02),
                                                                         (lowpass(noise(.1), 300) * decay(n_of(.1), 30), .5)), "open_air", .1, .3))
    variants("Combate", "esquiva", 2, "combat", lambda i: layer((whoosh(.35, 500, 2400, 1.1, .4), 1), (step("tierra", i), .4, .25)))
    seeded("cubrirse")
    out("Combate", "cubrirse", layer((cloth(.3, 2800, .5), .7), (lowpass(noise(.15), 300) * decay(n_of(.15), 20), .7, .18),
                                     (body("wood", 180, .2, .5, 2), .4, .2)), "combat")
    seeded("parada")
    out("Combate", "parada", space(layer((body("wood", 420, .4, 1.0, .7), 1), (body("bone", 900, .3, .6, 1), .4),
                                         (whoosh(.15, 1500, 3000, .8, .2), .3)), "open_air", .12, .4), "combat")
    variants("Combate", "dano_recibido", 3, "combat", player_hurt)
    seeded("contraataque")
    out("Combate", "contraataque", layer((staff_swing(1, .22, 500, 2600), 1), (staff_swing(2, .25, 400, 2400), .9, .2)), "combat")
    seeded("interrumpir")
    out("Combate", "interrumpir", layer((whoosh(.6, 300, 2000, 1.0, .6), 1), (whoosh(.4, 2000, 500, 1.0, .3), .6, .3),
                                        (body("wood", 260, .2, .7, 2), .5, .55)), "combat")
    seeded("anclar")
    n = n_of(1.4)
    thrum = lowpass(noise(1.4), 120) * env(n, .05, .8) * 1.5 + np.sin(phase(55, n)) * env(n, .02, 1.0) * .5
    out("Combate", "anclar", space(layer((body("wood", 150, .4, .9, 1), 1), (thrum, .8, .02)), "stone_room", .2, .6), "combat")
    seeded("vincular")
    n = n_of(1.2)
    creak = bandpass(noise(1.2), 600, 2500) * (np.abs(np.sin(phase(9, n))) ** 4) * env(n, .2, .5)
    out("Combate", "vincular", space(layer((creak, .6), (body("silver", 760, 1.2, .2), .25, .3), (cloth(.6, 3000), .3)), "hall", .2, .8), "combat")
    seeded("turno_jugador")
    out("Combate", "turno_jugador", space(layer((src.frame_drum(98, .6, .2, 1.4), .8), (knock("hollow_wood", 392, .2, .5), .5, .12),
                                                (knock("hollow_wood", 392, .2, .4), .4, .24)), "open_air", .12, .4), "combat")
    seeded("lazo_roto")
    n = n_of(1.8)
    snap = layer((body("silver", 620, 1.8, .9, .7), .6), (body("gold", 415, 1.8, .9, .7), .6), (highpass(noise(.05), 3000) * decay(n_of(.05), 80), .7))
    fall = np.sin(phase(np.linspace(830, 210, n), n)) * env(n, .01, 1.2) * .25
    out("Combate", "lazo_roto", space(layer((snap, 1), (fall, 1, .02)), "hall", .3, 1.4), "combat")
    # The jaguar affinity (C08 stand-in): warm pulse, wood motif and breath; the call; the lunge.
    seeded("jaguar_pulso")
    n = n_of(1.3)
    pulse = np.sin(phase(73.4, n)) * env(n, .15, .8) * 1.2 + lowpass(noise(1.3), 400) * env(n, .3, .6) * .6
    out("Combate", "jaguar_pulso", space(layer((pulse, 1), (src.slit_log(220, .8), .6, .1), (src.slit_log(294, .8), .5, .32),
                                                (src.seed_shaker(.6, 3800), .25, .15)), "hall", .25, .8), "combat")
    seeded("jaguar_llamada")
    out("Criaturas", "jaguar_llamada", space(src.jaguar_saw(6, .4), "open_air", .2, .8), "voice")
    seeded("jaguar_embestida")
    paws = np.zeros(n_of(.8))
    for k in range(5):
        mix_at(paws, lowpass(noise(.08), 400) * decay(n_of(.08), 45), .1 + k * .12, .7 + .3 * dsp.rng.random())
    out("Combate", "jaguar_embestida", layer((paws, 1), (whoosh(.7, 300, 1500, 1.0, .7), .8), (src.creature(.35, 110, 80, "felino", 1.2, .6, 28, .4), .5, .55)), "combat")


# ======================================================================== creatures

def build_creatures():
    c = src.creature
    # E07 caimán-murciélago: low reptile bellow and hiss, leathery wings, bat chitter.
    seeded("caiman_presencia")
    breath = np.zeros(n_of(12))
    for k in range(4):
        mix_at(breath, lowpass(noise(1.6), 500) * env(n_of(1.6), .6, .9) * (.6 + .4 * dsp.rng.random()), .4 + k * 3, 1)
        mix_at(breath, c(.9, 42, 38, "reptil", .5, .5, 24, .6) * .25, 1.6 + k * 3, 1, wrap=True)
    out("Criaturas", "caiman_presencia", loop_fold(breath, 12), "voice", rms=-24)
    seeded("caiman_despierta")
    out("Criaturas", "caiman_despierta", space(layer((c(2.4, 48, 34, "reptil", .7, .6, 22, .7), 1),
                                                     (lowpass(noise(2.4), 90) * env(n_of(2.4), .6, 1.2), .8),
                                                     (src.wings(.9, 2, 150, 700, 1.4), .6, 1.6)), "cave", .25, 1.5), "voice")
    variants("Criaturas", "caiman_aviso", 2, "voice", lambda i: space(layer((src.hiss(.9, 1800, 7000, .2, .3), .8),
                                                                             (c(.9, 60 + 8 * i, 85, "reptil", .8, .7, 30, .5), .8)), "cave", .2, .8))
    seeded("caiman_ataque")
    out("Criaturas", "caiman_ataque", space(layer((knock("bone", 280, .2, 1, 1.5), 1), (src.wings(.5, 1, 120, 900, 1.5), .9),
                                                  (c(.4, 90, 60, "reptil", 1, .7, 30, .4), .5)), "cave", .2, .8), "voice")
    variants("Criaturas", "caiman_herido", 2, "voice", lambda i: space(c(.45 + .1 * i, 85 + 10 * i, 55, "reptil", .9, .7, 34, .5), "cave", .2, .6))
    seeded("caiman_liberado")
    out("Criaturas", "caiman_liberado", space(layer((lowpass(noise(2.5), 600) * env(n_of(2.5), .3, 1.8), .7),
                                                    (c(1.8, 45, 32, "reptil", .5, .3, 20, .6), .4),
                                                    (cloth(1.0, 2200, .5), .4, 1.2)), "cave", .25, 1.5), "voice")
    variants("Criaturas", "murcielago_chillido", 2, "voice", lambda i: space(src.bat_chitter(.45, 8 + i), "cave", .25, .6))
    # E08 serpiente bicéfala: scales over stone, two hisses (low head A, high head B).
    seeded("serpiente_presencia")
    out("Criaturas", "serpiente_presencia", loop_crossfade(src.scales(13, .8), 1.0), "voice", rms=-28)
    seeded("serpiente_despierta")
    out("Criaturas", "serpiente_despierta", space(layer((src.hiss(2.2, 1500, 6000, .6, .8), .7), (src.hiss(2.0, 3500, 10000, .5, .8), .6, .4),
                                                        (src.scales(2.6, 1.5), .8)), "sky", .2, 1.2), "voice")
    seeded("serpiente_aviso_a")
    out("Criaturas", "serpiente_aviso_a", layer((src.hiss(1.0, 1300, 5000, .1, .4, 9), 1), (c(.8, 70, 90, "garganta", 1.2, .3, 20, .2), .3)), "voice")
    seeded("serpiente_aviso_b")
    out("Criaturas", "serpiente_aviso_b", layer((src.hiss(1.0, 3800, 11000, .1, .4, 13), 1), (src.scales(.8, 2), .4)), "voice")
    seeded("serpiente_ataque")
    out("Criaturas", "serpiente_ataque", layer((whoosh(.35, 600, 3000, 1.1, .7), 1), (knock("bone", 400, .2, 1, 1.4), .7, .3),
                                               (src.hiss(.4, 2000, 8000, .01, .3), .5, .3)), "voice")
    variants("Criaturas", "serpiente_herida", 2, "voice", lambda i: layer((src.hiss(.5, 1500 + 1500 * (i - 1), 9000, .01, .4, 20), 1),
                                                                         (src.scales(.5, 2.5), .5)))
    seeded("serpiente_liberada")
    out("Criaturas", "serpiente_liberada", space(layer((src.scales(3.0, .6) * np.linspace(1, .2, n_of(3.0)), 1),
                                                       (src.hiss(2.0, 1200, 5000, .5, 1.4), .3)), "sky", .2, 1.2), "voice")
    # E09 mujer-cóndor: heavy slow wings, a hissing wheeze (condors have no song).
    variants("Criaturas", "condor_aviso", 2, "voice", lambda i: layer((src.hiss(.8, 900, 4000, .1, .5, 7), .8),
                                                                     (c(.6, 140 + 20 * i, 110, "garganta", 1.5, .4, 18, .2), .4)))
    seeded("condor_alas")
    out("Criaturas", "condor_alas", space(src.wings(1.8, 2, 90, 600, 1.6), "sky", .15, .8), "voice")
    seeded("condor_ataque")
    out("Criaturas", "condor_ataque", space(layer((whoosh(1.1, 150, 1500, 1.2, .55), 1), (src.wings(.9, 1, 100, 700, 1.6), .7, .1)), "sky", .2, .8), "voice")
    seeded("condor_herido")
    out("Criaturas", "condor_herido", layer((src.hiss(.4, 1000, 5000, .01, .3), .8), (cloth(.4, 5000, .7), .6)), "voice")
    seeded("condor_liberado")
    out("Criaturas", "condor_liberado", space(layer((src.wings(2.0, 3, 100, 600, 1.4) * np.linspace(1, .2, n_of(2.0)), 1),
                                                    (cloth(.8, 2500, .3), .4, 1.4)), "sky", .2, 1.0), "voice")
    # E10 mujer-águila: piercing descending whistle, quick wings, a dive.
    variants("Criaturas", "aguila_grito", 2, "voice", lambda i: space(src.raptor_whistle(2500 + 300 * i, .9 + .2 * i), "sky", .2, .9))
    seeded("aguila_alas")
    out("Criaturas", "aguila_alas", space(src.wings(1.0, 3, 250, 1600, .8), "sky", .12, .6), "voice")
    seeded("aguila_ataque")
    out("Criaturas", "aguila_ataque", layer((whoosh(.8, 2500, 500, 1.0, .75), 1), (src.raptor_whistle(2900, .5), .35)), "voice")
    seeded("aguila_herida")
    out("Criaturas", "aguila_herida", layer((src.raptor_whistle(3100, .35), .6), (cloth(.4, 6000, .7), .6)), "voice")
    seeded("aguila_liberada")
    out("Criaturas", "aguila_liberada", space(layer((src.raptor_whistle(2300, 1.3) * .5, 1), (src.wings(1.5, 3, 250, 1200, .8) * np.linspace(1, .1, n_of(1.5)), .7, .6)), "sky", .3, 1.5), "voice")
    # E11 Quimue: the two-tone hum of C02/C13/C16 (gold and silver), lunar and solar warnings.
    seeded("quimue_zumbido")
    n = n_of(16)
    tt = np.arange(n) / SR
    hum = (np.sin(2 * np.pi * 146.8 * tt + .3 * np.sin(2 * np.pi * .25 * tt)) * .6
           + np.sin(2 * np.pi * 155.6 * tt) * .45 * (.6 + .4 * np.sin(2 * np.pi * .125 * tt))
           + np.sin(2 * np.pi * 440.4 * tt) * .08 + np.sin(2 * np.pi * 466.8 * tt) * .06)
    hum += bandpass(pink(16), 2000, 6000) * .03
    out("Criaturas", "quimue_zumbido", loop_fold(hum, 16), "voice", rms=-22)
    seeded("quimue_aviso_luna")
    out("Criaturas", "quimue_aviso_luna", space(layer((body("silver", 1046, 2.0, .4, .7), .6), (body("silver", 1568, 2.0, .4, .7), .4, .15),
                                                      (np.sin(phase(np.linspace(155.6, 196, n_of(1.2)), n_of(1.2))) * env(n_of(1.2), .4, .5), .4)), "hall", .35, 1.4), "voice")
    seeded("quimue_aviso_sol")
    out("Criaturas", "quimue_aviso_sol", space(layer((body("gold", 293.7, 2.2, .6, .6), .7), (body("gold", 440, 2.2, .5, .6), .4, .12),
                                                     (np.sin(phase(np.linspace(146.8, 110, n_of(1.2)), n_of(1.2))) * env(n_of(1.2), .4, .5), .5)), "hall", .35, 1.4), "voice")
    seeded("quimue_ataque")
    out("Criaturas", "quimue_ataque", space(layer((whoosh(.7, 200, 3000, 1.2, .8), .8), (body("gold", 220, .8, .9, 1.2), .6, .55),
                                                  (body("silver", 660, .8, .9, 1.2), .5, .55)), "hall", .3, 1.0), "voice")
    variants("Criaturas", "quimue_herido", 2, "voice", lambda i: space(layer((body("gold" if i == 1 else "silver", 350 + 120 * i, 1.0, .9, 1.4), .8),
                                                                             (src.hiss(.4, 3000, 9000, .01, .3), .3)), "hall", .3, 1.0))
    seeded("quimue_rotura")
    n = n_of(3.0)
    beat = np.sin(phase(np.linspace(146.8, 100, n), n)) * .5 + np.sin(phase(np.linspace(155.6, 98, n), n)) * .5
    out("Criaturas", "quimue_rotura", space(layer((beat * env(n, .01, 2.6), .7), (body("gold", 293.7, 2.5, 1.0, .6), .6),
                                                  (body("silver", 1046, 2.5, 1.0, .6), .4, .05), (highpass(noise(.08), 2500) * decay(n_of(.08), 60), .6)), "hall", .4, 2.0), "voice")
    # C11 the guacamaya: harsh squawks and bright wings.
    variants("Criaturas", "guacamaya_grito", 2, "voice", lambda i: space(c(.4 + .1 * i, 700 + 80 * i, 560, "ave", 1.4, .8, 75, 0), "sky", .15, .6))
    seeded("guacamaya_alas")
    out("Criaturas", "guacamaya_alas", src.wings(1.2, 4, 300, 2000, .8), "voice")
    # E01 the practice guardian: carved wood creaks before the blow.
    seeded("entrenamiento_aviso")
    n = n_of(.7)
    creak = bandpass(noise(.7), 500, 2200) * (np.abs(np.sin(phase(np.linspace(14, 22, n), n))) ** 6) * env(n, .1, .2)
    out("Criaturas", "entrenamiento_aviso", layer((creak, .8), (src.frame_drum(70, .7, .15, 1.2), .8), (body("stone", 90, .5, .3, 1.2), .3, .1)), "voice")


# ======================================================================== ambience

def bed(name, seconds, parts, rms=-24, stereo=True):
    seeded(name)
    x = np.zeros(n_of(seconds + 2))
    for p in parts:
        sig = p()
        mix_at(x, sig, 0, 1)
    x = loop_crossfade(x, 2.0)
    if stereo:
        x = decorrelate(x, 13)
    out("Ambiente", name, x, "amb", rms=rms)


def build_ambience():
    S = 36
    bed("amb_plaza", S, [lambda: src.wind(S + 2, 220, 700, .5, .06) * .6, lambda: src.leaves(S + 2, .4) * .15,
                         lambda: lowpass(pink(S + 2), 120) * .08], rms=-21)
    bed("amb_caverna", S, [lambda: src.wind(S + 2, 90, 300, .6, .05) * .7, lambda: bandpass(pink(S + 2), 35, 90) * .25,
                           lambda: bandpass(src.stream(S + 2, 1800, 20), 200, 1800) * .05], rms=-21)
    bed("amb_alturas", S, [lambda: src.wind(S + 2, 300, 1300, .7, .1), lambda: highpass(src.wind(S + 2, 1500, 3500, .8, .2), 1200) * .15], rms=-20)
    bed("amb_bacata_manana", S, [lambda: src.wind(S + 2, 200, 600, .4, .05) * .5, lambda: src.leaves(S + 2, .5) * .2,
                                 lambda: lowpass(src.fire(S + 2, 3, .3), 1500) * .05], rms=-22)
    bed("amb_bacata_incendio", S, [lambda: src.fire(S + 2, 22, .8) * .8, lambda: src.wind(S + 2, 150, 450, .7, .1) * .6,
                                   lambda: src.crowd(S + 2, 8) * .12], rms=-18)
    bed("amb_colina", S, [lambda: src.wind(S + 2, 250, 800, .6, .07) * .7, lambda: src.leaves(S + 2, .6) * .2,
                          lambda: src.stream(S + 2, 2500, 25) * .06], rms=-21)
    bed("amb_refugio", S, [lambda: lowpass(src.wind(S + 2, 120, 380, .6, .05), 600) * .4, lambda: src.fire(S + 2, 10, .5) * .6,
                           lambda: lowpass(pink(S + 2), 200) * .06], rms=-20)
    bed("amb_laguna", S, [lambda: src.water_lap(S + 2, 3.4, 900) * .7, lambda: src.wind(S + 2, 250, 700, .5, .05) * .5,
                          lambda: src.leaves(S + 2, .5) * .25], rms=-20)
    # Point sources and scattered details (played around the listener at random by the runtime).
    seeded("fuente_bucle")
    out("Ambiente", "fuente_bucle", loop_crossfade(src.fountain(14), 1.0), "amb", rms=-17)
    seeded("hoguera_bucle")
    out("Ambiente", "hoguera_bucle", loop_crossfade(src.fire(12, 18, .6), 1.0), "amb", rms=-17)
    seeded("agua_corriente_bucle")
    out("Ambiente", "agua_corriente_bucle", loop_crossfade(src.stream(12, 3000, 70), 1.0), "amb", rms=-19)
    for i, kind in enumerate(["trino", "silbo", "chip", "trino", "silbo", "chip"], 1):
        seeded(f"ave_canto_{i}")
        out("Ambiente", f"ave_canto_{i}", space(src.bird_call(kind), "open_air", .2, .5), "amb")
    variants("Ambiente", "ave_lejana", 2, "amb", lambda i: space(lowpass(src.raptor_whistle(2200 + 200 * i, 1.0), 4000) * .6, "sky", .4, 1.5))
    variants("Ambiente", "hojas_rafaga", 2, "amb", lambda i: src.leaves(3.0, .9) * env(n_of(3.0), 1.0, 1.4))
    variants("Ambiente", "piedra_asienta", 2, "amb", lambda i: space(layer((body("stone", 70 + 20 * i, 1.0, .3, .8), .8),
                                                                             (rattle(.8, 120, 2000, .6), .4, .05)), "cave", .5, 2.0))
    variants("Ambiente", "fuego_chasquido", 3, "amb", lambda i: src.fire(.6, 25, .1))
    variants("Ambiente", "insecto_noche", 2, "amb", lambda i: bandpass(noise(2.0), 4000 + 600 * i, 6000 + 600 * i) * ((.5 + .5 * np.sin(phase(28 + 5 * i, n_of(2.0)))) ** 4) * env(n_of(2.0), .3, .6) * .5)


# ======================================================================== music

D_DORIAN = [146.83, 164.81, 174.61, 196.0, 220.0, 246.94, 261.63, 293.66, 329.63, 349.23, 392.0, 440.0]


def note(degree, octave=0):
    return D_DORIAN[degree % 7] * (2 ** (degree // 7 + octave))


def music(name, seconds, compose, rms=-21, room="hall", wet=.3):
    seeded(name)
    left = np.zeros(n_of(seconds + 6)); right = np.zeros(n_of(seconds + 6))
    for sig, at, gain, position in compose():
        st = pan(sig, position)
        mix_at(left, st[:, 0], at, gain); mix_at(right, st[:, 1], at, gain)
    ir = dsp.room_ir(2.4, 3500, [(.019, .4), (.033, .3), (.051, .25)], stereo=True)
    wl = dsp.convolve(left, ir[0])[:len(left)] * wet * 2; wr = dsp.convolve(right, ir[1])[:len(right)] * wet * 2
    x = np.stack([left + wl, right + wr], axis=1)
    x = loop_fold(x, seconds)
    out("Musica", name, x, "music", rms=rms)


def phrase(instrument, degrees, start, beat, octave=0, gain=1.0, position=0.0, lengths=None):
    events = []
    at = start
    for k, d in enumerate(degrees):
        length = (lengths[k] if lengths else 1) * beat
        if d is not None:
            events.append((instrument(note(d, octave), length + beat * .3), at, gain, position))
        at += length
    return events


def compose_plaza():
    beat = 60 / 64; bars = 24
    ev = []
    for k in range(bars):
        ev.append((src.frame_drum(82, 1.4, .15, 1.2), k * beat * 2, .35 if k % 2 == 0 else .2, -.2))
        if k % 4 == 3:
            ev.append((src.seed_shaker(.5, 3800), k * beat * 2 + beat, .12, .4))
    ev.append((src.wind_pad(73.4, bars * beat * 2, .02, .15), 0, .25, 0))
    melody = [(4, 2), (None, 2), (3, 1), (2, 1), (0, 4), (None, 4), (2, 2), (3, 1), (4, 1), (2, 4), (None, 6),
              (5, 1), (4, 1), (2, 2), (0, 4), (None, 8)]
    at = beat * 4
    for d, ln in melody:
        if d is not None:
            ev.append((src.flute(note(d, 1), ln * beat + .2, .2, 4.5), at, .45, .15))
        at += ln * beat
    for k in range(0, bars, 6):
        ev.append((body("clay", note(0, 1), 2.0, .4, .5), k * beat * 2 + beat * .5, .25, -.4))
        ev.append((body("clay", note(4, 1), 2.0, .4, .5), k * beat * 2 + beat * 3.5, .2, .4))
    return ev


def compose_duel_quimue():
    beat = .6; bars = 32
    ev = []
    for k in range(bars * 2):
        ev.append((src.frame_drum(62, .9, .3, 1.0), k * beat, .8 if k % 4 == 0 else .45, 0))
        if k % 2 == 1:
            ev.append((src.slit_log(196, .5, .5), k * beat + beat / 2, .25, .5))
        if k % 8 == 6:
            ev.append((src.seed_shaker(.3, 4200), k * beat, .15, -.5))
    for k in range(0, bars * 2, 8):  # gold (left) and silver (right) answer each other
        ev.append((body("gold", 293.66, 3.0, .5, .5), k * beat, .25, -.6))
        ev.append((body("silver", 311.13, 3.0, .5, .5), k * beat + beat * 4, .2, .6))
    ev.append((src.wind_pad(73.4, bars * 2 * beat, .02, .2), 0, .35, 0))
    ev += phrase(lambda f, s: src.ocarina(f, s), [0, None, 2, 3, 2, 0, None, None, 4, 3, 2, 3, 0, None, None, None], beat * 8, beat * 2, 0, .4, .2)
    return ev


def compose_prologue():
    beat = 60 / 72; bars = 20
    ev = []
    for k in range(bars):
        ev.append((src.slit_log(146.8, 1.2, .6), k * beat * 2, .45, -.3))
        ev.append((src.slit_log(220, 1.0, .5), k * beat * 2 + beat * 1.5, .3, .3))
    staff = [(0, 1), (2, 1), (4, 2), (3, 1), (2, 1), (0, 2), (None, 2), (4, 1), (5, 1), (4, 2), (2, 2), (0, 4), (None, 6)]
    at = beat * 2
    for rep in range(2):
        for d, ln in staff:
            if d is not None:
                ev.append((src.flute(note(d, 1), ln * beat + .2, .22, 5), at, .5, .1))
            at += ln * beat
    ev.append((src.wind_pad(73.4, bars * beat * 2, .02, .1), 0, .2, 0))
    return ev


def compose_epilogue():
    beat = .75; bars = 16
    ev = []
    for k in range(bars * 2):
        ev.append((src.frame_drum(52, 1.3, .35, .9), k * beat * 2, .7 if k % 4 == 0 else .35, 0))
    ev.append((src.wind_pad(73.4, bars * beat * 4, .03, .4), 0, .5, 0))
    ev.append((src.wind_pad(77.8, bars * beat * 4, .03, .2), 0, .25, .3))  # the minor second keeps it uneasy
    ev += phrase(lambda f, s: src.flute(f, s, .35, 3.5, .01), [4, None, 3, None, 2, None, None, None], beat * 8, beat * 4, 0, .3, -.2)
    return ev


def compose_legacy():
    beat = 60 / 66; bars = 22
    ev = []
    ev.append((src.wind_pad(73.4, bars * beat * 2, .02, .1), 0, .25, 0))
    ev.append((src.wind_pad(110, bars * beat * 2, .02, .05), 0, .12, .2))
    staff = [(0, 1), (2, 1), (4, 2), (3, 1), (2, 1), (4, 2), (7, 4), (None, 2), (4, 1), (5, 1), (4, 2), (2, 2), (0, 6), (None, 6)]
    at = beat * 2
    for d, ln in staff:
        if d is not None:
            ev.append((src.flute(note(d, 1), ln * beat + .3, .2, 5), at, .5, -.1))
            ev.append((src.ocarina(note(d, 0), ln * beat + .3), at + .02, .2, .3))
        at += ln * beat
    for k in range(0, bars, 2):
        ev.append((src.slit_log(146.8, 1.4, .4), k * beat * 2, .3, -.4))
    return ev


def sting(name, compose, room_wet=.35):
    seeded(name)
    x = np.zeros(n_of(8))
    for sig, at, gain in compose():
        mix_at(x, sig, at, gain)
    x = space(x, "hall", room_wet)
    end = np.nonzero(np.abs(x) > 1e-4)[0]
    x = x[:end[-1] + 1] if len(end) else x
    out("Musica", name, decorrelate(fade_tail(x, .3), 9), "sting")


def build_music():
    music("musica_plaza", 60 / 64 * 2 * 24, compose_plaza, rms=-18)
    music("musica_duelo_quimue", .6 * 64, compose_duel_quimue, rms=-18)
    music("musica_bacata_prologo", 60 / 72 * 2 * 20, compose_prologue, rms=-18)
    music("musica_bacata_epilogo", .75 * 2 * 16, compose_epilogue, rms=-18)
    music("musica_legado", 60 / 66 * 2 * 22, compose_legacy, rms=-18)
    sting("estinger_descubrimiento", lambda: [(body("clay", 587.3, 2, .5, .4), 0, .6), (body("clay", 880, 2, .5, .4), .18, .5),
                                              (src.flute(note(4, 1), 1.4), .3, .4), (src.seed_shaker(.5, 4000), .05, .15)])
    sting("estinger_objetivo", lambda: [(src.slit_log(220, 1.0), 0, .6), (src.slit_log(293.7, 1.0), .2, .6), (src.flute(note(0, 1), 1.2), .35, .4)])
    sting("estinger_liberacion", lambda: [(src.wind_pad(146.8, 4.0, .02, .1), 0, .4), (src.flute(note(4, 1), 1.2), .4, .5),
                                          (src.flute(note(3, 1), 1.0), 1.4, .45), (src.flute(note(0, 1), 2.2), 2.3, .5),
                                          (src.seed_shaker(1.2, 3600), .2, .12), (src.frame_drum(73, 2, .1, .6), 0, .4)])
    sting("estinger_derrota", lambda: [(src.frame_drum(49, 2.5, .3, .6), 0, .9), (src.wind_pad(73.4, 3.5, .03, .3), .1, .5),
                                       (src.flute(note(4, 0), 1.2, .4, 3, .012), .6, .35), (src.flute(note(2, 0), 2.0, .4, 3, .012), 1.6, .3)])
    sting("estinger_portal", lambda: [(whoosh(2.0, 120, 900, 1.2, .7), 0, .5), (src.wind_pad(110, 2.5, .02, .3), 0, .5),
                                      (body("clay", 293.7, 2.0, .4, .5), 1.3, .4), (src.frame_drum(62, 1.5, .2, .8), 1.4, .5)])


# ======================================================================== replacements and repairs

def gather(folder, name):
    sr, x = read(os.path.join(BANK, folder, name + ".wav"))
    return x if x.ndim == 1 else x.mean(axis=1)


def build_replacements():
    ui = lambda n: gather("UI", n)  # noqa: E731
    for bank in (MI, MS):
        replace(os.path.join(bank, "ui_foco.wav"), ui("ui_foco_1"), -16)
        replace(os.path.join(bank, "ui_confirmar.wav"), ui("ui_confirmar"), -12)
        replace(os.path.join(bank, "ui_abrir.wav"), ui("ui_abrir"), -12)
        replace(os.path.join(bank, "hallazgo_abrir.wav"), gather("Foley", "examinar_abrir"), -8)
        replace(os.path.join(bank, "hallazgo_confirmar.wav"), gather("Musica", "estinger_descubrimiento"), -4)
        replace(os.path.join(bank, "victoria.wav"), gather("Musica", "estinger_liberacion"), -3)
        replace(os.path.join(bank, "derrota.wav"), gather("Musica", "estinger_derrota"), -4)
        replace(os.path.join(bank, "dano.wav"), gather("Combate", "dano_recibido_1"), -4)
    replace(os.path.join(MI, "ui_error.wav"), ui("ui_bloqueado"), -12)
    replace(os.path.join(MS, "no_disponible.wav"), ui("ui_bloqueado"), -12)
    # Rests: a breath of wind and two clay notes instead of a sine chord.
    seeded("descanso")
    rest = space(layer((src.wind_pad(146.8, 3.0, .02, .2), .5), (body("clay", 440, 2.5, .4, .4), .5, .2),
                       (body("clay", 587.3, 2.5, .4, .4), .45, .6), (src.seed_shaker(.8, 3500), .1, .1)), "hall", .3, 1.5)
    replace(os.path.join(MI, "descanso.wav"), rest, -5)
    replace(os.path.join(MS, "descanso.wav"), rest, -5)
    seeded("ofrenda")
    replace(os.path.join(MI, "ofrenda.wav"), space(layer((body("gold", 659.3, 2.5, .5, .5), .5), (body("clay", 440, 1.5, .5, .5), .6, .1),
                                                        (src.seed_shaker(.6, 4500), .15, .05)), "cave", .3, 1.5), -5)
    seeded("nucleo_abre")
    replace(os.path.join(MI, "nucleo_abre.wav"), space(layer((body("stone", 110, 1.5, .6, .6), .8), (body("silver", 880, 1.5, .3, .6), .3, .2),
                                                            (whoosh(1.0, 200, 1200, 1.0, .5), .4)), "cave", .3, 1.5), -5)
    seeded("cierre_insertar")
    replace(os.path.join(MS, "cierre_insertar.wav"), space(layer((body("stone", 220, .4, .8, 1.5), .8), (body("gold", 523, 1.2, .4, .6), .4, .05),
                                                                (rattle(.2, 600, 3500, .5), .25)), "stone_room", .2, .8), -6)
    seeded("alas_aviso")
    replace(os.path.join(MS, "alas_aviso.wav"), layer((knock("hollow_wood", 523, .2, .6), .8), (knock("hollow_wood", 440, .2, .6), .8, .14),
                                                      (cloth(.3, 4000, .5), .3)), -8)
    # Plaza (scene-referenced by GUID through PlazaAudio): organic versions of every cue.
    replace(os.path.join(PLAZA, "Step.wav"), gather("Foley", "paso_piedra_1"), -10)
    replace(os.path.join(PLAZA, "Jump.wav"), gather("Foley", "salto_1"), -8)
    replace(os.path.join(PLAZA, "Land.wav"), gather("Foley", "aterrizaje_piedra"), -7)
    replace(os.path.join(PLAZA, "Dash.wav"), gather("Foley", "impulso_1"), -7)
    replace(os.path.join(PLAZA, "Inspect.wav"), gather("Foley", "examinar_abrir"), -9)
    replace(os.path.join(PLAZA, "Rotate.wav"), gather("Foley", "objeto_girar_1"), -14)
    replace(os.path.join(PLAZA, "Complete.wav"), gather("Musica", "estinger_descubrimiento"), -5)
    replace(os.path.join(PLAZA, "Attack.wav"), gather("Combate", "baston_aire_1"), -6)
    replace(os.path.join(PLAZA, "Impact.wav"), gather("Combate", "impacto_entrenamiento_1"), -4)
    replace(os.path.join(PLAZA, "Guard.wav"), gather("Combate", "bloqueo_1"), -5)
    replace(os.path.join(PLAZA, "Warning.wav"), gather("Criaturas", "entrenamiento_aviso"), -5)
    replace(os.path.join(PLAZA, "Portal.wav"), gather("Musica", "estinger_portal"), -5)
    replace(os.path.join(PLAZA, "Victory.wav"), gather("Musica", "estinger_objetivo"), -4)
    for name in ("Ambience_Plaza", "Ambience_Inferior", "Ambience_Superior"):
        bed_name = {"Ambience_Plaza": "amb_plaza", "Ambience_Inferior": "amb_caverna", "Ambience_Superior": "amb_alturas"}[name]
        replace(os.path.join(PLAZA, name + ".wav"), gather("Ambiente", bed_name), 0, rms=-21)
    replace(os.path.join(MENU, "UI_Select.wav"), ui("ui_confirmar"), -12)
    # The cave bed keeps its name (MundoInferiorBlockout) but gains air and water instead of a hum.
    replace(os.path.join(MI, "ambiente_caverna.wav"), gather("Ambiente", "amb_caverna"), 0, rms=-18)


def repair(path, cut_at=None, fade=.15, loop=None):
    """Keeps the sound; removes the defect: a hard cut (fade before it) or a loop with a silent gap."""
    sr, x = read(backup(path))
    if loop is not None:
        x = loop_fold(x, loop)
    if cut_at is not None:
        k = n_of(cut_at); f = n_of(fade)
        x = x[:k].copy()
        x[-f:] *= np.cos(np.linspace(0, np.pi / 2, f)) ** 2
    write(path, x)
    WRITTEN.append(path)


def build_repairs():
    # Abrupt ends found by the audit (the noise of the strike stops 20-48 dB above the floor).
    for bank, name, at in [(MI, "caida", .72), (MI, "estalactita_golpe", 1.12), (MI, "guardian_cae", 2.22), (MI, "guardian_golpe", 1.42),
                           (MI, "losa_grieta", .72), (MI, "palanca", .92), (MI, "escudo_rompe", .92), (MS, "cierre_puerta", 1.42),
                           (MS, "jefe_golpe", 1.22), (MS, "aterrizaje_fuerte", .82), (MS, "portal_llegada", 1.22)]:
        repair(os.path.join(bank, name + ".wav"), cut_at=at, fade=.35 if name == "cierre_puerta" else .2)
    # Music loops: the files ran past their grid with a silent tail; fold the tail onto the start.
    repair(os.path.join(MS, "musica_exploracion.wav"), loop=32.0)
    repair(os.path.join(MS, "musica_percusion.wav"), loop=32.0)
    repair(os.path.join(MS, "musica_jefe.wav"), loop=19.2)
    repair(os.path.join(MI, "musica_guardian.wav"), loop=16.0)
    # Wind loops with a small seam step: an equal-power crossfade.
    for name in ("vuelo_viento", "viento_alturas", "portal_zumbido"):
        path = os.path.join(MS, name + ".wav")
        sr, x = read(backup(path))
        write(path, loop_crossfade(x, .5 if name != "viento_alturas" else 1.5)); WRITTEN.append(path)


GROUPS = {"ui": build_ui, "foley": build_foley, "combat": build_combat, "creatures": build_creatures,
          "ambience": build_ambience, "music": build_music, "replace": build_replacements, "repair": build_repairs}

if __name__ == "__main__":
    chosen = sys.argv[1:] or list(GROUPS)
    for g in chosen:
        GROUPS[g]()
        print(g, "done")
    print(len(WRITTEN), "files written")
