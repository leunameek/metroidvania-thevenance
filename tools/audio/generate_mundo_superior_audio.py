"""Synthesises the sound bank of the Mundo Superior (original material, no samples).

Usage: python tools/audio/generate_mundo_superior_audio.py [output dir]
Writes 16-bit mono WAV files to Assets/Worlds/MundoSuperior/Resources/MSAudio, read at runtime by
MSAudio (Prototype.Runtime). Re-running regenerates every clip deterministically (fixed seed).

Palette of the guide (section 16): open air instead of the lower world's cavern, light stone,
warm wood and metal, breathy flutes and hand drums. The music is written for the game and is not
a reconstruction of historical Muisca music.
"""
import os
import sys
import wave

import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "Assets", "Worlds", "MundoSuperior", "Resources", "MSAudio")
SR = 44100
rng = np.random.default_rng(2611)


def t(seconds):
    return np.arange(int(round(SR * seconds))) / SR


def n_of(seconds):
    return int(round(SR * seconds))


def env(n, attack=0.01, release=0.2, curve=3.0):
    e = np.ones(n)
    a = max(1, n_of(attack)); r = max(1, n_of(release))
    e[:min(a, n)] = np.linspace(0, 1, min(a, n))
    if r < n:
        e[-r:] *= np.linspace(1, 0, r) ** curve
    return e


def decay(n, rate):
    return np.exp(-np.arange(n) / SR * rate)


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.empty_like(x); acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc; y[i] = acc
    acc = 0.0
    for i in range(len(y) - 1, -1, -1):
        acc = (1 - a) * y[i] + a * acc; y[i] = acc
    return y


def highpass(x, cutoff):
    return x - lowpass(x, cutoff)


def bandpass(x, low, high):
    return lowpass(highpass(x, low), high)


def noise(seconds):
    return rng.standard_normal(n_of(seconds))


def air_reverb(x, seconds=1.2, mix=0.25, damp=5000):
    # Open-air space: few, far reflections and a long soft tail (no stone chamber).
    n = len(x) + n_of(seconds)
    y = np.zeros(n); y[:len(x)] = x
    tail = np.zeros(n)
    for delay, gain in [(0.061, .32), (0.113, .26), (0.171, .2), (0.263, .15), (0.397, .1)]:
        d = n_of(delay)
        tail[d:d + len(x)] += x * gain
    tail = lowpass(tail, damp) * decay(n, 3.0 / seconds)
    return y * (1 - mix) + tail * mix * 1.6


def norm(x, peak=0.85):
    m = np.max(np.abs(x)) or 1
    return x / m * peak


def tone(freq, seconds, kind="sine"):
    count = n_of(seconds)
    f = np.full(count, freq) if np.isscalar(freq) else freq[:count]
    ph = 2 * np.pi * np.cumsum(f) / SR
    if kind == "tri":
        return 2 / np.pi * np.arcsin(np.sin(ph))
    return np.sin(ph)


def flute(freq, seconds, vibrato=5.2, breath=0.12):
    # Breathy wooden flute: fundamental + soft odd harmonics, slow vibrato, air noise.
    n = n_of(seconds)
    f = freq * (1 + 0.006 * np.sin(2 * np.pi * vibrato * t(seconds)) * np.minimum(t(seconds) * 3, 1))
    body = tone(f, seconds) + tone(f * 2, seconds) * .18 + tone(f * 3, seconds) * .08
    air = bandpass(noise(seconds), freq * .8, freq * 4) * breath
    return (body + air) * env(n, .08, min(.35, seconds * .5))


def hand_drum(seconds=0.5, pitch=110, bright=900, rate=9):
    n = n_of(seconds)
    skin = tone(pitch * (1 + 0.8 * decay(n, 40)), seconds) * decay(n, rate)
    slap = lowpass(noise(seconds), bright) * decay(n, rate * 3)
    return skin + slap * .45


def stone_tap(seconds=0.25, low=180, bright=3200, rate=30):
    n = n_of(seconds)
    body = lowpass(noise(seconds), bright) * decay(n, rate)
    thump = tone(low * (1 + .4 * decay(n, 60)), seconds) * decay(n, rate * .8)
    return body * .7 + thump * .6


def stone_hit(seconds=0.6, low=120, bright=1800, rate=9):
    n = n_of(seconds)
    body = lowpass(noise(seconds), bright) * decay(n, rate)
    thump = tone(low * (1 + 0.6 * decay(n, 30)), seconds) * decay(n, rate * 0.8)
    return body * 0.7 + thump * 0.8


def whoosh(seconds, low=300, high=2500, rise=True):
    # Band of air sweeping up (or down) with a bell-shaped swell.
    n = n_of(seconds)
    shape = np.sin(np.linspace(0, np.pi, n)) ** 2
    lo = lowpass(noise(seconds), high) - lowpass(noise(seconds), low) * .5
    sweep = np.linspace(.6, 1.4, n) if rise else np.linspace(1.4, .6, n)
    return lo * shape * sweep


def feathers(seconds, density=0.97):
    n = n_of(seconds)
    flutter = highpass(noise(seconds), 2500) * (rng.random(n) > density) * 3
    return lowpass(flutter, 8000) + bandpass(noise(seconds), 600, 3000) * .25


def add(*parts):
    # Sum of clips of different lengths (shorter ones are padded with silence).
    out = np.zeros(max(len(p) for p in parts))
    for p in parts: out[:len(p)] += p
    return out


def mix_at(dst, src, at, gain=1.0):
    a = n_of(at)
    if a >= len(dst): return
    m = min(len(src), len(dst) - a)
    dst[a:a + m] += src[:m] * gain


def write(name, x, loop=False):
    os.makedirs(OUT, exist_ok=True)
    x = np.clip(x, -1, 1)
    if loop:  # crossfade the ends so the loop has no click
        f = n_of(0.25)
        x = x.copy(); x[:f] = x[:f] * np.linspace(0, 1, f) + x[-f:] * np.linspace(1, 0, f); x = x[:-f]
    data = (x * 32767).astype("<i2").tobytes()
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data)


# Scale of the world's music: D minor pentatonic.
D = [146.83, 174.61, 196.0, 220.0, 261.63, 293.66, 349.23, 392.0, 440.0, 523.25]


def main():
    # S01 Wind of the heights: two bands of air with slow gusts and a faint whistle (loop 24 s).
    s = 24.25; n = n_of(s)
    gust = .55 + .45 * np.sin(2 * np.pi * t(s) / 9.0) * np.sin(2 * np.pi * t(s) / 5.3 + 1)
    wind = bandpass(noise(s), 120, 900) * gust + bandpass(noise(s), 900, 2600) * gust ** 2 * .35
    whistle = tone(880 + 60 * np.sin(2 * np.pi * t(s) / 7), s) * np.maximum(0, gust - .7) * .08
    write("viento_alturas", norm(wind + whistle, .55), loop=True)

    # S02 Exploration music in two layers of the same length (loop 32 s, 75 bpm):
    # a flute melody over a warm pad, and a hand-drum layer that grows towards the summit.
    s = 32.25; n = n_of(s); beat = 60 / 75
    pad = np.zeros(n)
    for k, root in enumerate([146.83, 116.54, 130.81, 146.83]):  # Dm - Bb - C - Dm, 8 s each
        seg = (tone(root, 8.25, "tri") * .4 + tone(root * 1.5, 8.25) * .25 + tone(root * 2, 8.25) * .15) * env(n_of(8.25), 1.5, 2.0)
        mix_at(pad, lowpass(seg, 900), k * 8)
    melody = np.zeros(n)
    phrase = [(5, 1.5), (7, .75), (8, .75), (7, 1.5), (5, 1.5), (4, 3), (3, 1.5), (5, 1.5), (4, .75), (3, .75), (2, 3),
              (5, 1.5), (7, 1.5), (9, 1.5), (8, 1.5), (7, 3), (5, 1.5), (4, 1.5), (3, 1.5), (5, 4.5)]
    at = 0.0
    for idx, dur in phrase:
        mix_at(melody, flute(D[idx], dur * beat + .1), at, .55)
        at += dur * beat
    write("musica_exploracion", norm(air_reverb(pad * .8 + melody, 1.8, .35), .6), loop=True)
    drums = np.zeros(n)
    for k in range(int(s / beat)):
        a = k * beat
        mix_at(drums, hand_drum(.5, 98 if k % 4 == 0 else 147, 900, 9), a, 1 if k % 4 == 0 else .55)
        if k % 2 == 1: mix_at(drums, hand_drum(.25, 220, 2500, 22), a + beat / 2, .3)
        mix_at(drums, highpass(noise(.08), 4000) * decay(n_of(.08), 60), a + beat * .75, .12)  # shaker
    write("musica_percusion", norm(air_reverb(drums, 1.0, .25), .6), loop=True)

    # S03 Guardian music: deeper drums, a low ocarina call and stone hits (loop 19.2 s, 100 bpm).
    beat = .6; s = beat * 32 + .25; n = n_of(s)
    boss = np.zeros(n)
    for k in range(32):
        mix_at(boss, hand_drum(.6, 65, 700, 7), k * beat, 1 if k % 2 == 0 else .5)
        if k % 4 == 3: mix_at(boss, stone_tap(.3, 120, 2000, 20), k * beat + beat / 2, .5)
    for k, idx in enumerate([0, 2, 3, 2, 0, 1, 0, 0]):
        mix_at(boss, flute(D[idx] / 2, beat * 3.6, vibrato=4, breath=.2), k * beat * 4, .5)
    drone = (tone(73.4, s, "tri") * .25 + tone(110, s) * .12) * (.7 + .3 * np.sin(2 * np.pi * t(s) / (beat * 8)))
    write("musica_jefe", norm(air_reverb(boss + lowpass(drone, 500), 1.2, .25), .72), loop=True)

    # S04 Stone footsteps (4 variants), S05 jump, S06 landings.
    for i in range(4):
        x = stone_tap(.22, 150 + 25 * i, 2600 + 400 * i, 34 + 3 * i) + bandpass(noise(.22), 800, 4000) * decay(n_of(.22), 45) * .3
        write("paso_piedra_%d" % (i + 1), norm(x, .5))
    write("salto", norm(add(whoosh(.35, 400, 3000), stone_tap(.15, 200, 3000, 50) * .5), .5))
    write("aterrizaje", norm(stone_hit(.45, 110, 2200, 14) + bandpass(noise(.45), 400, 2500) * decay(n_of(.45), 18) * .5, .6))
    write("aterrizaje_fuerte", norm(air_reverb(stone_hit(.8, 70, 1600, 7) * 1.2 + lowpass(noise(.8), 1800) * decay(n_of(.8), 6) * .6, .8, .2), .8))

    # S07 Climbing: hand grips on stone (3 variants) and the effort over the top.
    for i in range(3):
        s = .3; n = n_of(s)
        scrape = bandpass(noise(s), 1200 + 300 * i, 5000) * env(n, .01, .15) * .6
        write("escalada_agarre_%d" % (i + 1), norm(scrape + stone_tap(s, 220 + 30 * i, 3500, 30) * .7, .45))
    s = 1.2; n = n_of(s)
    write("escalada_salida", norm(add(whoosh(s, 300, 2000) * .7 + bandpass(noise(s), 700, 3500) * env(n, .05, .4) * .3, stone_tap(.3, 140, 2500, 25)), .5))

    # S08-S10 Wings: unfurl, flight wind (loop), one beat per down-stroke, fold, low-budget warning.
    s = .6; n = n_of(s)
    shimmer = sum(tone(fr, s) * decay(n, 5 + i * 2) for i, fr in enumerate([587.3, 880, 1174.7])) * .25
    write("alas_abrir", norm(whoosh(s, 500, 4000) * 1.2 + feathers(s, .96) * env(n, .02, .3) * .6 + shimmer, .6))
    s = 4.25
    rush = bandpass(noise(s), 200, 1500) * (.8 + .2 * np.sin(2 * np.pi * t(s) / 1.4)) + bandpass(noise(s), 1500, 5000) * .25
    write("vuelo_viento", norm(rush, .5), loop=True)
    s = .45; n = n_of(s)
    write("aleteo", norm(whoosh(s, 250, 1800, rise=False) * 1.3 + feathers(s, .975) * env(n, .01, .25) * .5, .55))
    s = .5; n = n_of(s)
    write("alas_cerrar", norm(whoosh(s, 400, 3000, rise=False) + feathers(s, .97) * env(n, .01, .2) * .5, .55))
    s = .6; n = n_of(s)
    write("alas_aviso", norm((tone(659.3, s) * decay(n, 6) + tone(523.3, s) * decay(n, 6) * .6) * env(n, .01, .3), .4))

    # S11-S12 Portals: active hum (loop), travel and arrival.
    s = 4.25
    hum = (tone(196 * (1 + .004 * np.sin(2 * np.pi * .5 * t(s))), s) * .5 + tone(293.7, s) * .3 + tone(392, s) * .15) * (.75 + .25 * np.sin(2 * np.pi * t(s) / 2.125))
    write("portal_zumbido", norm(hum + bandpass(noise(s), 2000, 6000) * .05, .4), loop=True)
    s = 1.6; n = n_of(s)
    rise_chord = sum(tone(fr * (1 + .5 * t(s) / s), s) for fr in [293.7, 440, 587.3]) * env(n, .2, .6) * .3
    write("portal_viajar", norm(air_reverb(rise_chord + whoosh(s, 600, 5000) * .8, 1.4, .35), .65))
    s = 1.2; n = n_of(s)
    fall_chord = sum(tone(fr * (1.3 - .3 * t(s) / s), s) * decay(n, 3 + i) for i, fr in enumerate([587.3, 880, 1174.7])) * .35
    write("portal_llegada", norm(air_reverb(fall_chord + whoosh(s, 500, 3000, rise=False) * .5, 1.2, .35), .55))

    # S13-S15 Finds and rest: an opening shimmer, a warm wooden-chime chord, a breath and bells.
    s = 1.0; n = n_of(s)
    write("hallazgo_abrir", norm(air_reverb(sum(tone(fr, s) * decay(n, 4 + i) for i, fr in enumerate([1174.7, 1568, 2093])) * env(n, .06, .6), 1.4, .4), .45))
    s = 2.6; n = n_of(s)
    chord = sum(tone(fr, s) * decay(n, 1.4 + i * .6) * g for i, (fr, g) in enumerate([(293.7, 1), (440, .7), (587.3, .6), (698.5, .4), (880, .3)]))
    final = chord * env(n, .005, 1.2)
    mix_at(final, flute(880, .9), .35, .25)
    write("hallazgo_confirmar", norm(air_reverb(final, 2.2, .4), .75))
    s = 2.2; n = n_of(s)
    rest = sum(tone(fr * (1 + .002 * np.sin(2 * np.pi * 4 * t(s))), s) * g for fr, g in [(440, 1), (523.3, .6), (659.3, .5), (880, .25)]) * env(n, .35, 1.3)
    write("descanso", norm(air_reverb(rest + bandpass(noise(s), 300, 1500) * env(n, .5, 1.0) * .15, 2.0, .4), .55))

    # S16 Lock: the medallion seats with a click and a ring; the stone leaf grinds and stops.
    s = 1.2; n = n_of(s)
    ring = sum(tone(fr, s) * decay(n, 3 + i * 1.5) for i, fr in enumerate([523.3, 784, 1318.5])) * .4
    write("cierre_insertar", norm(add(stone_tap(.3, 300, 5000, 40), ring), .6))
    s = 1.4; n = n_of(s)
    grind = lowpass(noise(s), 700) * (.5 + .5 * np.abs(np.sin(2 * np.pi * t(s) * 6))) * env(n, .08, .3)
    stop = np.zeros(n); mix_at(stop, stone_hit(.5, 70, 1200, 8), 1.0, .9)
    write("cierre_puerta", norm(air_reverb(grind + stop, .9, .2), .75))

    # S17 Transport: low wooden-metal hum (loop), start and stop knocks.
    s = 3.25
    drive = (tone(55, s, "tri") * .4 + tone(110.5, s) * .2) * (.8 + .2 * np.sin(2 * np.pi * t(s) * 2)) + bandpass(noise(s), 150, 600) * .3
    creak = np.zeros(n_of(s)); mix_at(creak, tone(310 + 40 * t(.4), .4, "tri") * env(n_of(.4), .05, .2) * .15, 1.4)
    write("transporte_bucle", norm(drive + creak, .45), loop=True)
    write("transporte_arranque", norm(stone_hit(.6, 85, 1400, 9) + whoosh(.6, 200, 900) * .4, .6))
    write("transporte_parada", norm(add(stone_hit(.7, 70, 1300, 8), stone_tap(.2, 160, 2500, 30) * .4), .65))

    # S18-S21 Guardian: wake, warning, blow, core hit, defences, damage, victory and defeat.
    s = 2.4; n = n_of(s)
    write("jefe_despierta", norm(air_reverb(lowpass(noise(s), 180) * env(n, .8, .8) * 1.4 + tone(55 * (1 + .1 * t(s)), s) * env(n, 1.0, .8), 1.8, .3), .8))
    s = 1.0; n = n_of(s)
    write("jefe_aviso", norm(lowpass(noise(s), 300) * env(n, .6, .2) * 1.2 + tone(110 * (1 + t(s)), s) * env(n, .6, .2) * .4 + whoosh(s, 300, 1500) * .3, .65))
    write("jefe_golpe", norm(air_reverb(stone_hit(1.2, 45, 900, 3.4) * 1.2, 1.4, .25), .9))
    s = .7; n = n_of(s)
    write("golpe_nucleo", norm(air_reverb(sum(tone(fr, s) * decay(n, 7 + i * 2) for i, fr in enumerate([698.5, 1046.5, 1568])) + stone_hit(.7, 100, 1800, 10) * .4, 1.0, .3), .75))
    s = .5; n = n_of(s)
    write("defensa_bloqueo", norm(sum(tone(fr, s) * decay(n, 9 + i * 3) for i, fr in enumerate([523.3, 1311, 2190])) * .6 + stone_tap(s, 150, 2200, 14) * .5, .65))
    write("defensa_esquiva", norm(whoosh(.45, 600, 4500) * 1.2, .55))
    s = .45; n = n_of(s)
    write("defensa_fallida", norm(lowpass(noise(s), 1200) * decay(n, 12) + tone(150 * (1 - .4 * t(s) / s), s) * decay(n, 9) * .8, .7))
    s = .35; n = n_of(s)
    write("dano", norm(lowpass(noise(s), 1500) * decay(n, 14) + tone(170 * (1 - .4 * t(s) / s), s) * decay(n, 10) * .7, .65))
    s = 3.6; n = n_of(s)
    fanfare = np.zeros(n)
    for k, idx in enumerate([0, 2, 3, 5, 7]):
        mix_at(fanfare, flute(D[idx] * 2, 3.6 - k * .3, breath=.08) * decay(n_of(3.6 - k * .3), 1.0), k * .28, .5)
    for k in range(5): mix_at(fanfare, hand_drum(.5, 98, 900, 8), k * .28, .5)
    write("victoria", norm(air_reverb(fanfare, 2.4, .4), .8))
    s = 2.4; n = n_of(s)
    write("derrota", norm(air_reverb((tone(146.8 * (1 - .25 * t(s) / s), s, "tri") * .6 + tone(220 * (1 - .25 * t(s) / s), s) * .4) * env(n, .05, 1.6), 2.0, .4), .6))

    # S22 Recovery after a fall: a soft breath of air that fades in and out.
    s = .9; n = n_of(s)
    write("recuperacion", norm(air_reverb(whoosh(s, 300, 2500, rise=False) * .8 + tone(440, s) * env(n, .2, .5) * .1, 1.0, .3), .45))

    # S23 and interface: soft "not available", focus, confirm, open.
    s = .35; n = n_of(s)
    write("no_disponible", norm(tone(330, s, "tri") * decay(n, 10) + tone(311, s, "tri") * decay(n, 10) * .8, .35))
    s = .12; n = n_of(s)
    write("ui_foco", norm(tone(1318.5, s) * decay(n, 40), .3))
    s = .25; n = n_of(s)
    write("ui_confirmar", norm(tone(880, s) * decay(n, 18) + tone(1318.5, s) * decay(n, 22) * .5, .4))
    write("ui_abrir", norm(bandpass(noise(s), 400, 2500) * env(n, .05, .15) + tone(587.3, s) * decay(n, 12) * .4, .4))
    print("MS audio written to", OUT, len([f for f in os.listdir(OUT) if f.endswith(".wav")]), "files")


if __name__ == "__main__":
    main()
