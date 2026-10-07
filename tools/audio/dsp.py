"""Small numpy-only DSP kit for the procedural sound bank of El asedio de Bacatá.

Everything is synthesised (no samples, no third-party recordings): modal bodies for wood, clay,
stone and gold; filtered noise for air, fabric, fire and water; a source-filter voice for the
creatures; convolution with generated rooms. Deterministic when the caller seeds `rng`.
"""
import os
import wave

import numpy as np

SR = 44100
rng = np.random.default_rng(2026)


def seed(value):
    global rng
    rng = np.random.default_rng(value)


def n_of(seconds):
    return max(1, int(round(SR * seconds)))


def t(seconds):
    return np.arange(n_of(seconds)) / SR


def noise(seconds):
    return rng.standard_normal(n_of(seconds))


def wobble(n, hz):
    """Slow random control signal normalised to -1..1 (modulations, gusts, surges)."""
    size = 1 << int(np.ceil(np.log2(n + 1)))
    X = np.fft.rfft(rng.standard_normal(size))
    f = np.fft.rfftfreq(size, 1 / SR)
    X[f > hz] = 0; X[0] = 0
    y = np.fft.irfft(X, size)[:n]
    return y / (np.max(np.abs(y)) or 1)


def fit(x, n):
    """Pads or trims to n samples."""
    if len(x) >= n:
        return x[:n]
    return np.r_[x, np.zeros(n - len(x))]


# ------------------------------------------------------------------ envelopes

def env(n, attack=.005, release=.1, curve=2.0):
    e = np.ones(n)
    a = min(n, max(1, n_of(attack)))
    r = min(n - a, max(1, n_of(release))) if n > a else 0
    e[:a] = np.linspace(0, 1, a) ** 1.5
    if r > 0:
        e[n - r:] *= np.linspace(1, 0, r) ** curve
    return e


def decay(n, rate):
    return np.exp(-np.arange(n) / SR * rate)


def fade_tail(x, seconds=.05):
    """Guarantees a silent, click-free end."""
    x = x.copy(); k = min(len(x), n_of(seconds))
    x[-k:] *= np.cos(np.linspace(0, np.pi / 2, k)) ** 2
    return x


def fade_head(x, seconds=.002):
    x = x.copy(); k = min(len(x), n_of(seconds))
    x[:k] *= np.sin(np.linspace(0, np.pi / 2, k)) ** 2
    return x


# ------------------------------------------------------------------ filters (zero phase, FFT)

def _response(f, kind, lo, hi, order):
    with np.errstate(divide="ignore", invalid="ignore"):
        if kind == "low":
            return 1 / np.sqrt(1 + (f / lo) ** (2 * order))
        if kind == "high":
            r = 1 / np.sqrt(1 + (lo / np.maximum(f, 1e-6)) ** (2 * order)); r[f == 0] = 0; return r
        if kind == "band":
            return _response(f, "high", lo, None, order) * _response(f, "low", hi, None, order)
        if kind == "notch":
            return 1 - _response(f, "band", lo, hi, order) * .9
    raise ValueError(kind)


def filt(x, kind, lo, hi=None, order=2):
    n = len(x)
    size = 1 << int(np.ceil(np.log2(n + 1)))
    X = np.fft.rfft(x, size)
    f = np.fft.rfftfreq(size, 1 / SR)
    return np.fft.irfft(X * _response(f, kind, lo, hi, order), size)[:n]


def lowpass(x, cutoff, order=2):
    return filt(x, "low", cutoff, None, order)


def highpass(x, cutoff, order=2):
    return filt(x, "high", cutoff, None, order)


def bandpass(x, lo, hi, order=2):
    return filt(x, "band", lo, hi, order)


def peak_eq(x, freq, q, gain_db):
    n = len(x); size = 1 << int(np.ceil(np.log2(n + 1)))
    X = np.fft.rfft(x, size); f = np.fft.rfftfreq(size, 1 / SR)
    bw = freq / q
    g = 10 ** (gain_db / 20)
    shape = np.exp(-.5 * ((f - freq) / (bw / 2)) ** 2)
    return np.fft.irfft(X * (1 + (g - 1) * shape), size)[:n]


def pink(seconds):
    n = n_of(seconds); size = 1 << int(np.ceil(np.log2(n + 1)))
    X = np.fft.rfft(rng.standard_normal(size))
    f = np.fft.rfftfreq(size, 1 / SR); f[0] = 1
    y = np.fft.irfft(X / np.sqrt(f), size)[:n]
    y = highpass(y, 20)
    return y / (np.std(y) or 1)


def brown(seconds):
    n = n_of(seconds); size = 1 << int(np.ceil(np.log2(n + 1)))
    X = np.fft.rfft(rng.standard_normal(size))
    f = np.fft.rfftfreq(size, 1 / SR); f[0] = 1
    y = np.fft.irfft(X / f, size)[:n]
    y = highpass(y, 25)
    return y / (np.std(y) or 1)


def moving_band(x, centers, widths, frame=1024):
    """Time-varying band-pass: centers/widths are arrays (Hz) sampled per output sample."""
    hop = frame // 4
    win = np.hanning(frame)
    out = np.zeros(len(x) + frame); norm = np.zeros(len(x) + frame)
    f = np.fft.rfftfreq(frame, 1 / SR)
    xp = np.r_[x, np.zeros(frame)]
    for start in range(0, len(x), hop):
        seg = xp[start:start + frame] * win
        i = min(len(centers) - 1, start + frame // 2)
        c = max(20.0, float(centers[i])); w = max(10.0, float(widths[i]))
        resp = np.exp(-.5 * ((f - c) / (w / 2)) ** 2)
        out[start:start + frame] += np.fft.irfft(np.fft.rfft(seg) * resp, frame) * win
        norm[start:start + frame] += win ** 2
    norm[norm < 1e-6] = 1
    return (out / norm)[:len(x)]


def formants(x, f0_track, table, frame=1024):
    """Shapes a voice source with formant bands (Hz, bandwidth, gain) varying per sample via `table(i)`."""
    hop = frame // 4
    win = np.hanning(frame)
    out = np.zeros(len(x) + frame); norm = np.zeros(len(x) + frame)
    f = np.fft.rfftfreq(frame, 1 / SR)
    xp = np.r_[x, np.zeros(frame)]
    for start in range(0, len(x), hop):
        seg = xp[start:start + frame] * win
        i = min(len(x) - 1, start + frame // 2)
        resp = np.zeros_like(f) + .02
        for fc, bw, g in table(i / SR):
            resp += g * np.exp(-.5 * ((f - fc) / (bw / 2)) ** 2)
        out[start:start + frame] += np.fft.irfft(np.fft.rfft(seg) * resp, frame) * win
        norm[start:start + frame] += win ** 2
    norm[norm < 1e-6] = 1
    return (out / norm)[:len(x)]


# ------------------------------------------------------------------ oscillators

def phase(freq, n):
    freq = np.full(n, float(freq)) if np.isscalar(freq) else fit(np.asarray(freq, float), n)
    return 2 * np.pi * np.cumsum(freq) / SR


def sine(freq, seconds):
    n = n_of(seconds)
    return np.sin(phase(freq, n))


def glottal(f0, seconds, open_q=.6, jitter=.01):
    """Band-limited-ish pulse train (creature voices): sum of harmonics with a soft spectral tilt."""
    n = n_of(seconds)
    f = np.full(n, float(f0)) if np.isscalar(f0) else fit(np.asarray(f0, float), n)
    f = f * (1 + jitter * lowpass(rng.standard_normal(n), 30) * 3)
    ph = 2 * np.pi * np.cumsum(f) / SR
    out = np.zeros(n)
    for h in range(1, 40):
        alive = (f * h) < SR / 2.2
        if not alive.any():
            break
        out += alive * np.sin(h * ph) / h ** (1.0 + open_q)
    return out


# ------------------------------------------------------------------ bodies (modal synthesis)

MATERIALS = {
    # ratios, decay scale, brightness
    "wood": ([1, 2.57, 4.21, 5.93, 8.1], 26, .35),
    "hollow_wood": ([1, 1.98, 3.12, 4.3, 5.9], 14, .45),
    "clay": ([1, 2.31, 3.89, 5.3], 22, .4),
    "stone": ([1, 1.71, 2.89, 3.92, 5.37, 7.1], 34, .55),
    "gold": ([1, 2.76, 5.40, 8.93, 13.34], 3.2, .5),
    "silver": ([1, 2.92, 5.81, 9.44, 14.2], 2.4, .55),
    "bone": ([1, 2.4, 4.1, 6.6], 30, .45),
    "seed": ([1, 1.6, 2.3], 90, .6),
}


def body(material, f0, seconds, strike=.5, damp=1.0, detune=.004):
    ratios, rate, bright = MATERIALS[material]
    n = n_of(seconds)
    out = np.zeros(n)
    for i, r in enumerate(ratios):
        fr = f0 * r * (1 + detune * rng.standard_normal())
        if fr > SR / 2.3:
            continue
        a = (bright ** i) * (1 + .3 * rng.standard_normal()) * (1 if i == 0 else strike + .2)
        out += a * np.sin(phase(fr, n) + rng.random() * 6.28) * decay(n, rate * damp * (1 + .45 * i))
    # contact noise: the excitation itself (short, filtered by the body's range)
    k = n_of(.006 + .01 * (1 - strike))
    click = np.zeros(n); click[:k] = rng.standard_normal(k) * np.linspace(1, 0, k) ** 2
    click = bandpass(click, f0 * .8, min(SR / 2.2, f0 * 12)) * (.8 + strike)
    return fade_head(out + click)


def knock(material, f0, seconds=.25, strike=.6, damp=1.0):
    return fade_tail(body(material, f0, seconds, strike, damp), .03)


def rattle(seconds, density=180, center=3500, spread=.4, shape=None):
    """Seeds in a gourd/rattle: many tiny clicks under an envelope."""
    n = n_of(seconds)
    out = np.zeros(n)
    e = shape if shape is not None else env(n, .01, seconds * .6)
    count = int(density * seconds)
    for _ in range(count):
        at = int(rng.random() * n)
        if rng.random() > e[at]:
            continue
        k = n_of(.004 + .004 * rng.random())
        if at + k >= n:
            continue
        fr = center * np.exp(spread * rng.standard_normal())
        g = rng.random() ** 2
        out[at:at + k] += g * np.sin(phase(fr, k)) * decay(k, 900)
    return highpass(out, 1200)


def whoosh(seconds, f_from=400, f_to=1600, width=.9, peak_at=.5):
    n = n_of(seconds)
    x = noise(seconds)
    u = np.linspace(0, 1, n)
    centers = f_from * (f_to / f_from) ** u
    e = np.where(u < peak_at, (u / peak_at) ** 2, ((1 - u) / (1 - peak_at)) ** 1.6)
    return moving_band(x, centers, centers * width) * e


def cloth(seconds, bright=2500, grain=.6):
    """Fabric movement: crackly band noise with irregular amplitude."""
    n = n_of(seconds)
    x = bandpass(noise(seconds), 400, bright)
    am = lowpass(np.abs(rng.standard_normal(n)) ** (1 + grain * 2), 40)
    am /= am.max() or 1
    return x * am * env(n, .02, seconds * .5)


# ------------------------------------------------------------------ spaces

def room_ir(seconds=1.2, damp=3500, early=(), predelay=.008, stereo=False):
    n = n_of(seconds)
    chans = []
    for c in range(2 if stereo else 1):
        tail = noise(seconds) * decay(n, 6.9 / seconds)
        tail = lowpass(tail, damp) * .6 + lowpass(tail, damp * .3) * .4
        ir = np.zeros(n)
        p = n_of(predelay)
        ir[p:] += tail[:n - p] * .5
        for d, g in early:
            k = n_of(d * (1 + .03 * c))
            if k < n:
                ir[k] += g
        ir[0] = 0
        chans.append(ir / (np.sqrt(np.sum(ir ** 2)) or 1))
    return chans if stereo else chans[0]


def convolve(x, ir):
    n = len(x) + len(ir) - 1
    size = 1 << int(np.ceil(np.log2(n)))
    return np.fft.irfft(np.fft.rfft(x, size) * np.fft.rfft(ir, size), size)[:n]


ROOMS = {}


def space(x, name="stone_room", wet=.25, tail=None):
    """Adds a generated room (dry signal kept), extending the sound by the room's tail."""
    spec = {
        "small_room": (.5, 5000, [(.007, .5), (.011, .4), (.017, .3)]),
        "stone_room": (1.3, 3200, [(.011, .5), (.019, .45), (.029, .35), (.043, .3)]),
        "cave": (2.8, 2200, [(.031, .45), (.052, .4), (.083, .35), (.12, .25), (.19, .2)]),
        "open_air": (.9, 6000, [(.06, .25), (.13, .15)]),
        "sky": (2.2, 5000, [(.09, .2), (.21, .15)]),
        "hall": (2.0, 3800, [(.017, .45), (.031, .35), (.053, .3)]),
    }[name]
    if name not in ROOMS:
        state = rng.bit_generator.state
        ROOMS[name] = room_ir(spec[0], spec[1], spec[2])
        rng.bit_generator.state = state
    ir = ROOMS[name]
    y = convolve(x, ir)
    dry = np.r_[x, np.zeros(len(y) - len(x))]
    out = dry + wet * y * 2.2
    if tail is not None:
        out = out[:len(x) + n_of(tail)]
    return fade_tail(out, min(.25, len(out) / SR * .2))


# ------------------------------------------------------------------ assembly

def mix_at(dst, src, at_seconds, gain=1.0, wrap=False):
    a = n_of(at_seconds) if at_seconds > 0 else 0
    if wrap:
        idx = (np.arange(len(src)) + a) % len(dst)
        np.add.at(dst, idx, src * gain)
        return dst
    end = min(len(dst), a + len(src))
    if end > a:
        dst[a:end] += src[:end - a] * gain
    return dst


def norm_peak(x, db=-1.0):
    m = np.max(np.abs(x)) or 1
    return x / m * 10 ** (db / 20)


def norm_rms(x, db=-20.0, ceiling=-1.0):
    r = np.sqrt(np.mean(x ** 2)) or 1
    y = x / r * 10 ** (db / 20)
    m = np.max(np.abs(y))
    lim = 10 ** (ceiling / 20)
    if m > lim:  # soft limiter instead of clipping
        y = np.tanh(y / lim) * lim
    return y


def loop_fold(x, seconds):
    """Wraps everything past the loop length onto the start, so release tails and reverb
    continue across the seam instead of leaving a silent gap or a cut."""
    n = n_of(seconds)
    y = np.zeros((n,) + x.shape[1:])
    for s in range(0, len(x), n):
        seg = x[s:s + n]; y[:len(seg)] += seg
    return y


def loop_crossfade(x, seconds=1.0):
    """For noise beds without a grid: the last `seconds` fade into the first ones (equal power)."""
    k = n_of(seconds)
    w = np.linspace(0, 1, k)
    if x.ndim > 1:
        w = w[:, None]
    y = x[:-k].copy()
    y[:k] = x[:k] * np.sqrt(w) + x[-k:] * np.sqrt(1 - w)
    return y


def stereo(left, right=None, width=.0):
    if right is None:
        right = left
    return np.stack([left, right], axis=1)


def pan(x, position):
    """Mono → stereo, constant power, position -1..1."""
    a = (position + 1) * np.pi / 4
    return np.stack([x * np.cos(a), x * np.sin(a)], axis=1)


def decorrelate(x, ms=11):
    """Cheap stereo width for beds: a slightly delayed, filtered copy on the right."""
    d = n_of(ms / 1000)
    r = np.roll(x, d) * .7 + x * .3  # circular: a loop stays seamless
    return np.stack([x, r], axis=1)


def write(path, x, peak_db=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    x = np.asarray(x, float)
    if peak_db is not None:
        x = norm_peak(x, peak_db)
    if np.max(np.abs(x)) > .999:
        x = np.tanh(x)
    ch = 1 if x.ndim == 1 else x.shape[1]
    tpdf = (rng.random(x.shape) - rng.random(x.shape)) / 32768
    data = np.clip(np.round((x + tpdf) * 32767), -32768, 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(ch); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(data.tobytes())


def read(path):
    with wave.open(path, "rb") as w:
        ch = w.getnchannels(); sr = w.getframerate()
        x = np.frombuffer(w.readframes(w.getnframes()), "<i2").astype(float) / 32768
    x = x.reshape(-1, ch)
    return sr, (x[:, 0] if ch == 1 else x)


def layer(*parts):
    """Sums signals of different lengths. Each part: array, or (array, gain), or (array, gain, at_seconds)."""
    items = []
    for p in parts:
        if isinstance(p, tuple):
            x, g, at = (p + (1.0, 0.0))[:3] if len(p) == 1 else (p[0], p[1], p[2] if len(p) > 2 else 0.0)
        else:
            x, g, at = p, 1.0, 0.0
        items.append((np.asarray(x, float), g, at))
    n = max(n_of(at) + len(x) for x, g, at in items)
    out = np.zeros(n)
    for x, g, at in items:
        mix_at(out, x, at, g)
    return out
