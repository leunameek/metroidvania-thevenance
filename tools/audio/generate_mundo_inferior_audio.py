"""Synthesises the sound bank of the Mundo Inferior (original material, no samples).

Usage: python tools/audio/generate_mundo_inferior_audio.py
Writes 16-bit mono WAV files to Assets/Worlds/MundoInferior/Resources/MIAudio, read at runtime by
MIAudio (Prototype.Runtime). Re-running regenerates every clip deterministically (fixed seed).
"""
import os
import wave

import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "Worlds", "MundoInferior", "Resources", "MIAudio")
SR = 44100
rng = np.random.default_rng(1977)


def t(seconds):
    return np.arange(int(round(SR * seconds))) / SR


def env(n, attack=0.01, release=0.2, curve=3.0):
    e = np.ones(n)
    a = max(1, int(round(SR * attack))); r = max(1, int(round(SR * release)))
    e[:a] = np.linspace(0, 1, a)
    if r < n:
        e[-r:] *= np.linspace(1, 0, r) ** curve
    return e


def decay(n, rate):
    return np.exp(-np.arange(n) / SR * rate)


def lowpass(x, cutoff):
    # One-pole filter, applied forward and backward for zero phase.
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


def noise(seconds):
    return rng.standard_normal(int(round(SR * seconds)))


def reverb(x, seconds=1.6, mix=0.35, damp=2400):
    # Sparse decaying echoes approximating a stone chamber.
    n = len(x) + int(round(SR * seconds))
    y = np.zeros(n); y[:len(x)] = x
    tail = np.zeros(n)
    for delay, gain in [(0.029, .55), (0.041, .5), (0.067, .42), (0.089, .36), (0.13, .3), (0.19, .24), (0.27, .18), (0.38, .12)]:
        d = int(round(SR * delay))
        tail[d:d + len(x)] += x * gain
    tail = lowpass(tail, damp) * decay(n, 3.0 / seconds)
    return y * (1 - mix) + tail * mix * 1.6


def norm(x, peak=0.85):
    m = np.max(np.abs(x)) or 1
    return x / m * peak


def tone(freq, seconds, kind="sine"):
    ph = 2 * np.pi * np.cumsum(np.full(int(round(SR * seconds)), freq) if np.isscalar(freq) else freq) / SR
    if kind == "tri":
        return 2 / np.pi * np.arcsin(np.sin(ph))
    return np.sin(ph)


def write(name, x, loop=False):
    os.makedirs(OUT, exist_ok=True)
    x = np.clip(x, -1, 1)
    if loop:  # crossfade the ends so the loop has no click
        f = int(round(SR * 0.25))
        x = x.copy(); x[:f] = x[:f] * np.linspace(0, 1, f) + x[-f:] * np.linspace(1, 0, f); x = x[:-f]
    data = (x * 32767).astype("<i2").tobytes()
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data)


def stone_hit(seconds=0.6, low=120, bright=1800, rate=9):
    n = int(round(SR * seconds))
    body = lowpass(noise(seconds), bright) * decay(n, rate)
    thump = tone(low * (1 + 0.6 * decay(n, 30)), seconds) * decay(n, rate * 0.8)
    return body * 0.7 + thump * 0.8


def main():
    # Ambience: deep air, distant rumble and a soft drone in the fifth (loop, 24 s).
    s = 24.25
    air = lowpass(noise(s), 380) * 0.9 + lowpass(noise(s), 90) * 1.6
    slow = 0.6 + 0.4 * np.sin(2 * np.pi * t(s) / 12.0)
    drone = (tone(55, s) * 0.5 + tone(82.4, s) * 0.3 + tone(110.3, s) * 0.12) * (0.55 + 0.45 * np.sin(2 * np.pi * t(s) / 8.0))
    write("ambiente_caverna", norm(air * slow + drone * 0.35, 0.6), loop=True)

    # Water drop: pitched blip with a fast upward chirp and a small chamber tail.
    s = 0.5; n = int(round(SR * s))
    f = 900 + 1700 * np.exp(-t(s) * 60)
    write("gota", norm(reverb(tone(f, s) * decay(n, 28), 1.2, .45), .5))

    write("caida", norm(reverb(stone_hit(0.7, 70, 900, 7), 1.0, .3), .7))
    s = 0.35; n = int(round(SR * s))
    write("dano", norm(lowpass(noise(s), 1400) * decay(n, 14) + tone(160 * (1 - .4 * t(s) / s), s) * decay(n, 10) * .7, .7))
    s = 2.4; n = int(round(SR * s))
    write("derrota", norm(reverb((tone(110 * (1 - .25 * t(s) / s), s, "tri") * .6 + tone(165 * (1 - .25 * t(s) / s), s) * .4) * env(n, .05, 1.6), 2.2, .5), .6))

    # Finds: soft opening shimmer and a warm bell chord on confirmation.
    s = 1.2; n = int(round(SR * s))
    shimmer = sum(tone(fr, s) * decay(n, 4 + i) for i, fr in enumerate([880, 1320, 1760, 2200])) / 4
    write("hallazgo_abrir", norm(reverb(shimmer * env(n, .08, .8), 1.4, .5), .5))
    s = 2.6; n = int(round(SR * s))
    bell = sum(tone(fr, s) * decay(n, 1.6 + i * .7) * g for i, (fr, g) in enumerate([(392, 1), (493.9, .7), (587.3, .6), (784, .4), (1174.7, .2)]))
    write("hallazgo_confirmar", norm(reverb(bell * env(n, .005, 1.2), 2.4, .45), .75))
    s = 1.8; n = int(round(SR * s))
    chime = sum(tone(fr, s) * decay(n, 2.5 + i) for i, fr in enumerate([659.3, 987.8, 1318.5]))
    write("ofrenda", norm(reverb(chime * env(n, .005, 1.0), 2.0, .5), .6))

    # Mechanisms.
    s = 0.9; n = int(round(SR * s))
    clank = stone_hit(.9, 90, 2600, 6) + lowpass(noise(s), 5000) * decay(n, 40) * .4
    write("palanca", norm(reverb(clank, 1.2, .3), .75))
    s = 1.6; n = int(round(SR * s))
    grind = lowpass(noise(s), 600) * (0.5 + 0.5 * np.abs(np.sin(2 * np.pi * t(s) * 7))) * env(n, .1, .4)
    write("reja_abre", norm(reverb(grind + stone_hit(1.6, 60, 800, 4) * .5 * np.r_[np.zeros(int(round(SR * 1.2))), np.ones(n - int(round(SR * 1.2)))], 1.4, .35), .7))
    write("reja_cierra", norm(reverb(lowpass(noise(.8), 700) * env(int(round(SR * .8)), .02, .3) + stone_hit(.8, 55, 900, 6), 1.2, .35), .75))
    s = 2.0; n = int(round(SR * s))
    heal = sum(tone(fr * (1 + .002 * np.sin(2 * np.pi * 5 * t(s))), s) * g for fr, g in [(523.3, 1), (659.3, .7), (784, .6), (1046.5, .3)])
    write("descanso", norm(reverb(heal * env(n, .3, 1.2), 2.0, .5), .55))
    # Horn: breathy low call with a rising swell.
    s = 2.8; n = int(round(SR * s))
    f0 = 146.8 * (1 + 0.03 * np.minimum(t(s) * 2, 1))
    horn = (tone(f0, s) + tone(f0 * 2, s) * .5 + tone(f0 * 3, s) * .25 + tone(f0 * 4, s) * .12) * env(n, .35, .9)
    horn += lowpass(noise(s), 1200) * env(n, .2, .8) * .15
    write("cuerno", norm(reverb(horn, 2.6, .5), .8))

    # Hazards.
    s = 0.7; n = int(round(SR * s))
    crack = highpass(noise(s), 1200) * (rng.random(n) > 0.985) * 8 * decay(n, 4)
    write("losa_grieta", norm(reverb(lowpass(crack, 6000) + stone_hit(.7, 200, 3000, 12) * .3, .8, .25), .65))
    write("losa_cae", norm(reverb(stone_hit(1.0, 65, 1100, 4) + lowpass(noise(1.0), 2500) * decay(SR, 3) * .5, 1.4, .35), .8))
    s = 0.9; n = int(round(SR * s))
    trickle = highpass(noise(s), 2000) * (rng.random(n) > 0.97) * 4 * env(n, .3, .2)
    write("estalactita_aviso", norm(reverb(lowpass(trickle, 7000) + lowpass(noise(s), 300) * env(n, .5, .2) * .5, 1.0, .35), .55))
    write("estalactita_golpe", norm(reverb(stone_hit(1.1, 55, 1500, 4.5), 1.6, .35), .85))
    # One rush of air per crossing (MIPendulum plays it when the weight passes the lane).
    s = 1.0; n = int(round(SR * s))
    swish = lowpass(noise(s), 900) * np.sin(np.linspace(0, np.pi, n)) ** 2
    creak = tone(72 + 10 * t(s), s, "tri") * env(n, .02, .3) * decay(n, 6) * .25
    write("pendulo", norm(swish + lowpass(creak, 500), .6))

    # Guardians.
    s = 1.0; n = int(round(SR * s))
    write("guardian_carga", norm(reverb(lowpass(noise(s), 200) * env(n, .6, .2) * 1.5 + tone(55 * (1 + t(s)), s) * env(n, .6, .2) * .6, 1.0, .35), .7))
    write("guardian_golpe", norm(reverb(stone_hit(1.4, 45, 900, 3.2) * 1.2, 2.0, .4), .9))
    s = 0.8; n = int(round(SR * s))
    write("guardian_barrido", norm(reverb(lowpass(noise(s), 1500) * np.sin(np.linspace(0, np.pi, n)) ** 2 + stone_hit(.8, 80, 1200, 6) * .4, 1.2, .3), .75))
    s = 2.4; n = int(round(SR * s))
    write("guardian_despierta", norm(reverb(lowpass(noise(s), 160) * env(n, .8, .8) * 1.5 + tone(41.2, s) * env(n, 1.0, .8), 2.4, .45), .8))
    write("guardian_cae", norm(reverb(stone_hit(2.2, 40, 700, 1.6) * 1.3 + lowpass(noise(2.2), 1500) * decay(int(round(SR * 2.2)), 1.4) * .6, 2.6, .45), .9))
    s = 0.9; n = int(round(SR * s))
    write("escudo_rompe", norm(reverb(highpass(noise(s), 900) * decay(n, 6) + stone_hit(.9, 110, 3500, 5), 1.4, .35), .85))
    s = 0.5; n = int(round(SR * s))
    ring = sum(tone(fr, s) * decay(n, 9 + i * 3) for i, fr in enumerate([523, 1311, 2190]))
    write("escudo_bloquea", norm(reverb(ring * .6 + stone_hit(.5, 150, 2000, 14) * .5, .9, .3), .7))
    write("golpe_piedra", norm(reverb(stone_hit(.5, 130, 2200, 12), .8, .3), .75))
    s = 0.7; n = int(round(SR * s))
    write("golpe_nucleo", norm(reverb(sum(tone(fr, s) * decay(n, 7 + i * 2) for i, fr in enumerate([698, 1047, 1568])) + stone_hit(.7, 100, 1800, 10) * .4, 1.2, .4), .75))
    s = 1.0; n = int(round(SR * s))
    write("nucleo_abre", norm(reverb(tone(440 * (1 + .5 * t(s)), s) * env(n, .05, .5) * .6 + tone(660 * (1 + .5 * t(s)), s) * env(n, .05, .5) * .3, 1.4, .45), .55))

    # Guardian music: low drums and a modal drone pulse (loop, 16 s, 90 bpm).
    s = 16.25; n = int(round(SR * s))
    music = np.zeros(n)
    beat = 60 / 90
    for k in range(int(s / beat)):
        a = int(round(SR * k * beat)); m = int(round(SR * .5))
        if a + m > n: break
        drum = tone(55 * (1 + 1.2 * decay(m, 30)), .5) * decay(m, 7) + lowpass(noise(.5), 300) * decay(m, 20) * .5
        music[a:a + m] += drum * (1 if k % 4 == 0 else .55)
        if k % 2 == 1:
            m2 = int(round(SR * .2)); music[a:a + m2] += highpass(noise(.2), 2500) * decay(m2, 35) * .2
    melody = [146.8, 174.6, 196, 174.6, 146.8, 130.8, 146.8, 110]
    for k, fr in enumerate(melody):
        a = int(round(SR * k * 2)); m = int(round(SR * 2))
        if a + m > n: break
        music[a:a + m] += (tone(fr, 2, "tri") * .25 + tone(fr * 1.5, 2) * .1) * env(m, .3, .8)
    write("musica_guardian", norm(reverb(music, 1.2, .25), .7), loop=True)
    s = 3.5; n = int(round(SR * s))
    fanfare = np.zeros(n)
    for k, fr in enumerate([293.7, 370, 440, 587.3]):
        a = int(round(SR * k * .35)); m = n - a
        fanfare[a:] += (tone(fr, m / SR) + tone(fr * 2, m / SR) * .3) * decay(m, 1.2) * env(m, .02, 1.5)
    write("victoria", norm(reverb(fanfare, 2.6, .45), .8))
    s = 2.0; n = int(round(SR * s))
    write("portal", norm(reverb(sum(tone(fr * (1 + t(s)), s) * env(n, .2, 1.2) for fr in [220, 330, 440]) * .4 + highpass(noise(s), 3000) * env(n, .5, 1.0) * .2, 2.0, .5), .7))

    # Interface.
    s = 0.12; n = int(round(SR * s))
    write("ui_foco", norm(tone(1200, s) * decay(n, 40), .35))
    s = 0.25; n = int(round(SR * s))
    write("ui_confirmar", norm(tone(880, s) * decay(n, 18) + tone(1320, s) * decay(n, 22) * .5, .45))
    write("ui_abrir", norm(lowpass(noise(s), 2000) * env(n, .05, .15) + tone(440, s) * decay(n, 12) * .4, .45))
    s = 0.4; n = int(round(SR * s))
    write("ui_error", norm(tone(220, s, "tri") * decay(n, 8) + tone(207, s, "tri") * decay(n, 8), .45))
    print("MI audio written to", OUT, len(os.listdir(OUT)), "files")


if __name__ == "__main__":
    main()
