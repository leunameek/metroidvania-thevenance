"""Instruments, nature and creature voices built on dsp.py.

The instruments are fictional and generic (breathy flute, clay whistle, slit log, frame drum,
seed rattle, struck metal). They do not reproduce documented Muisca instruments or music, and the
creature calls are stylised designs, not recordings of the species.
"""
import numpy as np

from dsp import (SR, wobble, body, bandpass, brown, cloth, decay, env, fade_head, fade_tail, filt, fit,
                 formants, glottal, highpass, lowpass, mix_at, moving_band, n_of, noise, phase,
                 pink, rattle, sine, t, whoosh)
import dsp


def rnd():
    return dsp.rng


# ------------------------------------------------------------------ music instruments

def flute(freq, seconds, breath=.18, vibrato=5.0, depth=.006, chiff=.5):
    n = n_of(seconds)
    tt = np.arange(n) / SR
    vib = 1 + depth * np.sin(2 * np.pi * vibrato * tt) * np.clip((tt - .25) / .4, 0, 1)
    f = freq * vib * (1 + .002 * wobble(n, 3))
    ph = phase(f, n)
    tone = np.sin(ph) + .22 * np.sin(2 * ph + .5) + .06 * np.sin(3 * ph + 1.1)
    air = bandpass(noise(seconds), freq * .8, freq * 4) * breath
    attack = bandpass(noise(seconds), freq * 2, min(SR / 2.3, freq * 8)) * decay(n, 30) * chiff
    e = env(n, min(.09, seconds * .3), min(.25, seconds * .4), 1.6)
    return (tone + air + attack) * e


def ocarina(freq, seconds, breath=.08):
    n = n_of(seconds)
    tt = np.arange(n) / SR
    f = freq * (1 + .004 * np.sin(2 * np.pi * 4.5 * tt) * np.clip((tt - .3) / .3, 0, 1))
    ph = phase(f, n)
    tone = np.sin(ph) + .05 * np.sin(2 * ph)
    air = bandpass(noise(seconds), freq * .9, freq * 2.5) * breath
    return (tone + air) * env(n, .05, min(.3, seconds * .5), 1.4)


def slit_log(freq, seconds=1.2, strike=.6):
    return body("hollow_wood", freq, seconds, strike, damp=.55)


def frame_drum(freq=95, seconds=1.0, slap=.4, damp=1.0):
    n = n_of(seconds)
    out = np.zeros(n)
    for i, (r, a) in enumerate([(1, 1), (1.59, .5), (2.14, .35), (2.3, .25), (2.65, .2), (2.92, .12)]):
        fr = freq * r * (1 + .5 * decay(n, 40) * (i == 0) * .08)
        out += a * np.sin(phase(fr, n)) * decay(n, (7 + 6 * i) * damp)
    hit = lowpass(noise(seconds), 2500) * decay(n, 60) * slap
    return fade_head(out + hit)


def seed_shaker(seconds=.25, bright=4000):
    n = n_of(seconds)
    return rattle(seconds, density=900, center=bright, spread=.35, shape=env(n, .02, seconds * .7, 1.2))


def metal(material, freq, seconds=3.0, strike=.4):
    return body(material, freq, seconds, strike, damp=1.0)


def wind_pad(freq, seconds, width=.03, air=.25):
    """A tuned breath (wind through a pipe): narrow noise bands on the first partials."""
    x = noise(seconds)
    out = bandpass(x, freq * (1 - width), freq * (1 + width), 3)
    out += bandpass(noise(seconds), freq * 2 * (1 - width), freq * 2 * (1 + width), 3) * .45
    out = out / (np.std(out) or 1)
    out += lowpass(pink(seconds), freq * 3) * air
    n = n_of(seconds)
    return out * env(n, seconds * .3, seconds * .4, 1.2)


# ------------------------------------------------------------------ nature

def wind(seconds, low=250, high=900, gusts=.5, speed=.08):
    n = n_of(seconds)
    x = pink(seconds)
    slow = (wobble(n, speed * 3) + 1) / 2
    centers = low + (high - low) * slow
    y = moving_band(x, centers, centers * 1.1, frame=2048)
    g = (wobble(n, speed * 2) + 1) / 2
    return y * (1 - gusts + gusts * g ** 1.5)


def leaves(seconds, density=.5):
    n = n_of(seconds)
    x = bandpass(noise(seconds), 1800, 9000)
    am = lowpass(np.abs(rnd().standard_normal(n)) ** 3, 25)
    gust = np.clip((wobble(n, .15) + 1) / 2 * 1.6 - .5 + density * .5, 0, 1)
    return x * am / (am.max() or 1) * gust


def bubble(freq, seconds=.08, rise=.15):
    n = n_of(seconds)
    f = freq * (1 + rise * np.linspace(0, 1, n) ** 2)
    return np.sin(phase(f, n)) * decay(n, freq / 25) * env(n, .001, seconds * .3)


def water_lap(seconds, period=3.2, bright=900):
    n = n_of(seconds)
    tt = np.arange(n) / SR
    wave_shape = np.clip(np.sin(2 * np.pi * tt / period + .7 * np.sin(2 * np.pi * tt / (period * 2.7))), 0, 1) ** 2
    x = lowpass(pink(seconds), bright) * (.25 + wave_shape)
    out = x
    for _ in range(int(seconds * 6)):
        at = rnd().random() * seconds
        if wave_shape[min(n - 1, n_of(at))] > .3:
            mix_at(out, bubble(300 + 900 * rnd().random(), .06), at, .25 * rnd().random())
    return out


def stream(seconds, bright=3000, density=60):
    out = bandpass(pink(seconds), 400, bright) * .6
    n = n_of(seconds)
    am = lowpass(np.abs(rnd().standard_normal(n)), 8)
    out *= .6 + .8 * am / (am.max() or 1)
    for _ in range(int(seconds * density)):
        mix_at(out, bubble(500 + 2200 * rnd().random(), .05, .3), rnd().random() * seconds, .12 * rnd().random())
    return out


def fountain(seconds):
    n = n_of(seconds)
    splash = bandpass(noise(seconds), 900, 7000)
    am = lowpass(np.abs(rnd().standard_normal(n)) ** 2, 60)
    splash *= .4 + am / (am.max() or 1)
    low = lowpass(brown(seconds), 400) * .3
    out = splash * .5 + low
    for _ in range(int(seconds * 90)):
        mix_at(out, bubble(400 + 1800 * rnd().random(), .05, .25), rnd().random() * seconds, .15 * rnd().random())
    return out


def fire(seconds, crackle=14, roar=.5):
    n = n_of(seconds)
    base = bandpass(brown(seconds), 45, 260) * roar * .6
    hiss = bandpass(noise(seconds), 2500, 8000) * .05 * (.5 + lowpass(np.abs(rnd().standard_normal(n)), 3))
    out = base + hiss
    count = int(seconds * crackle)
    for _ in range(count):
        at = rnd().random() * seconds
        k = n_of(.002 + .01 * rnd().random())
        c = rnd().standard_normal(k) * decay(k, 600 + 900 * rnd().random())
        c = bandpass(c, 1200 + 2000 * rnd().random(), 9000)
        mix_at(out, c, at, .6 * rnd().random() ** 2)
        if rnd().random() < .15:  # a pop with a small burst of sparks
            mix_at(out, rattle(.15, 400, 5000, .5), at + .01, .25 * rnd().random())
    return out


def bird_call(kind="trino", base=None):
    """Generic songbird figures (not a specific species)."""
    r = rnd()
    out = np.zeros(n_of(1.6))
    at = 0.0
    if kind == "trino":
        f0 = base or (2600 + 1200 * r.random())
        for i in range(r.integers(4, 9)):
            d = .045 + .03 * r.random()
            n = n_of(d)
            f = f0 * (1 + .25 * np.sin(np.linspace(0, np.pi, n))) * (1 - .1 * i / 8)
            mix_at(out, np.sin(phase(f, n)) * env(n, .004, d * .6), at, .6 + .4 * r.random())
            at += d + .02 + .02 * r.random()
    elif kind == "silbo":
        f0 = base or (1800 + 900 * r.random())
        for f_from, f_to, d in [(f0, f0 * 1.35, .22), (f0 * 1.2, f0 * .9, .3)]:
            n = n_of(d)
            f = np.linspace(f_from, f_to, n)
            mix_at(out, np.sin(phase(f, n)) * env(n, .03, d * .5) * (1 + .3 * np.sin(phase(28, n))), at, .7)
            at += d + .12
    else:  # "chip": short repeated notes
        f0 = base or (3500 + 1500 * r.random())
        for i in range(r.integers(2, 5)):
            d = .03
            n = n_of(d)
            f = f0 * np.linspace(1.15, .9, n)
            mix_at(out, np.sin(phase(f, n)) * env(n, .002, .02), at, .8)
            at += .11 + .04 * r.random()
    end = min(len(out), n_of(at + .1))
    return fade_tail(out[:end], .02)


def raptor_whistle(f0=2400, seconds=1.1):
    n = n_of(seconds)
    tt = np.linspace(0, 1, n)
    f = f0 * (1.1 - .35 * tt) * (1 + .03 * np.sin(phase(24, n)))
    ph = phase(f, n)
    x = np.sin(ph) + .25 * np.sin(2 * ph) + bandpass(noise(seconds), f0 * .8, f0 * 1.6) * .25
    return x * env(n, .04, seconds * .6, 1.5) * (1 + .4 * np.sin(phase(22, n)))


def wings(seconds, beats=3, low=180, high=900, heavy=1.0):
    out = np.zeros(n_of(seconds))
    period = seconds / beats
    for i in range(beats):
        d = period * .9
        w = whoosh(d, low * heavy, high, 1.4, .35)
        w += cloth(d, 3500, .4) * .35
        mix_at(out, w, i * period, 1.0)
    return out


# ------------------------------------------------------------------ creatures (source-filter)

VOWELS = {
    "reptil": [(320, 260, 1), (900, 400, .55), (2300, 900, .25)],
    "garganta": [(250, 200, 1), (700, 300, .5), (1900, 700, .2)],
    "ave": [(1400, 600, 1), (2900, 900, .6), (4200, 1200, .3)],
    "felino": [(420, 300, 1), (1150, 450, .6), (2600, 900, .25)],
}


def creature(seconds, f0_from, f0_to, vowel="reptil", breath=.6, growl=.4, growl_rate=38, sub=.3, curve=1.0):
    n = n_of(seconds)
    u = np.linspace(0, 1, n) ** curve
    f0 = f0_from + (f0_to - f0_from) * u
    src = glottal(f0, seconds, .5, .03)
    if sub > 0:
        src += glottal(f0 / 2, seconds, .7, .03) * sub
    src = src / (np.std(src) or 1)
    rough = 1 + growl * np.sin(phase(growl_rate * (1 + .15 * wobble(n, 5)), n))
    src = src * rough + noise(seconds) * breath
    table = VOWELS[vowel]
    y = formants(src, f0, lambda s: table)
    return y * env(n, min(.08, seconds * .2), min(.35, seconds * .5), 1.5)


def hiss(seconds, lo=2500, hi=9000, attack=.05, release=.3, flutter=0.0):
    n = n_of(seconds)
    x = bandpass(noise(seconds), lo, hi)
    if flutter:
        x *= 1 + .5 * np.sin(phase(flutter, n))
    return x * env(n, attack, release, 1.4)


def scales(seconds, speed=1.0):
    """Dry scales over stone: grainy rasp with slow surges."""
    n = n_of(seconds)
    rasp = bandpass(noise(seconds), 1500, 7000)
    grain = np.abs(rnd().standard_normal(n)) ** 3
    grain = lowpass(grain, 120 * speed)
    surge = np.clip((wobble(n, .4 * speed) + 1) / 2 * 1.4 - .2, 0, 1)
    return rasp * grain / (grain.max() or 1) * surge


def bat_chitter(seconds=.5, count=9):
    out = np.zeros(n_of(seconds))
    for i in range(count):
        d = .012
        n = n_of(d)
        f = (7000 + 1500 * rnd().random()) * np.linspace(1.2, .8, n)
        mix_at(out, np.sin(phase(f, n)) * env(n, .001, .008), i * seconds / count * (1 + .2 * rnd().random()), .7)
    return out


def jaguar_saw(grunts=6, spacing=.42):
    """The jaguar's hoarse "sawing" call: a series of grunts on in- and out-breath."""
    out = np.zeros(n_of(grunts * spacing + .8))
    for i in range(grunts):
        d = .3 if i % 2 == 0 else .2
        g = creature(d, 95 - 4 * i, 62, "felino", breath=1.1, growl=.5, growl_rate=31, sub=.4)
        mix_at(out, g, i * spacing * (1 - .03 * i), 1.0 if i % 2 == 0 else .55)
    return out


def crowd(seconds, voices=10):
    """Distant, unintelligible voices (no words): breathy vowel-shifting sources, far away."""
    out = np.zeros(n_of(seconds))
    keys = [[(700, 220, 1), (1200, 320, .6)], [(400, 160, 1), (2000, 400, .4)], [(320, 160, 1), (850, 300, .6)], [(520, 200, 1), (1700, 320, .45)]]
    for v in range(voices):
        n = n_of(seconds)
        f0 = 110 + 140 * rnd().random()
        contour = f0 * (1 + .1 * wobble(n, 3))
        src = glottal(contour, seconds, .9, .02) * .6 + noise(seconds) * .5
        src *= np.clip(wobble(n, 1.5) * 1.4 + .2, 0, 1)
        step = .16 + .1 * rnd().random()
        order = rnd().integers(0, len(keys), int(seconds / step) + 2)
        y = formants(src, contour, lambda s_: keys[order[int(s_ / step)]])
        mix_at(out, y, 0, .5 + .5 * rnd().random())
    return lowpass(highpass(out, 150), 1400)
