"""Compose the original title-loop 'Bruma del umbral'. No samples or third-party music.

Soft plucked partials, a slow harmonic bed and filtered air. Fictional ambient music;
no claim to reproduce Muisca instruments or a historical musical tradition.
Requires numpy. Deterministic, writes a 60-second stereo PCM loop.
"""
from pathlib import Path
import json
import wave
import numpy as np

RATE = 44100
SECONDS = 60
N = RATE * SECONDS
rng = np.random.default_rng(709)
mix = np.zeros((N, 2), dtype=np.float64)


def midi(note):
    return 440 * 2 ** ((note - 69) / 12)


def place(signal, start, gain, pan=0):
    """Wrap release tails across the loop seam, retaining continuity."""
    ids = (np.arange(len(signal)) + int(start * RATE)) % N
    mix[ids, 0] += signal * gain * np.sqrt((1 - pan) / 2)
    mix[ids, 1] += signal * gain * np.sqrt((1 + pan) / 2)


def pluck(note, length=6):
    t = np.arange(int(length * RATE)) / RATE
    f = midi(note)
    attack = 1 - np.exp(-t / 0.009)
    return attack * (
        np.sin(2 * np.pi * f * t) * np.exp(-t / 1.75)
        + .22 * np.sin(2 * np.pi * f * 2.001 * t) * np.exp(-t / .65)
        + .065 * np.sin(2 * np.pi * f * 3.003 * t) * np.exp(-t / .24)
    )


# Four long, voiced changes, all from the same restrained D-minor palette.
chords = [(50, 57, 65), (48, 55, 64), (46, 53, 62), (45, 52, 60)]
for bar, notes in enumerate(chords):
    t = np.arange(20 * RATE) / RATE
    envelope = np.minimum(t / 4, 1) * np.maximum(0, np.minimum((20 - t) / 5, 1))
    for i, note in enumerate(notes):
        f = midi(note)
        bed = (np.sin(2 * np.pi * f * t) + .18 * np.sin(2 * np.pi * 2 * f * t))
        bed *= envelope * (.86 + .14 * np.sin(2 * np.pi * .08 * t + i))
        place(bed, bar * 15 - 2.5, .043, (i - 1) * .45)

# Sparse call and response; silence and long tails carry most of the piece.
melody = [(0, 69), (2.8125, 74), (6.5625, 72), (10.3125, 65),
          (15.9375, 67), (19.6875, 72), (24.375, 64),
          (30, 65), (33.75, 69), (37.5, 74), (41.25, 77),
          (46.875, 76), (50.625, 72), (54.375, 69), (58.125, 62)]
for i, (start, note) in enumerate(melody):
    voice = pluck(note)
    pan = -.28 if i % 2 == 0 else .28
    place(voice, start, .13, pan)
    for delay, gain in [(.39, .026), (.81, .014), (1.47, .007)]:
        place(voice, start + delay, gain, -pan)

# Seamless filtered air, shaped slowly; no loud percussion or transient surprises.
noise = rng.normal(0, 1, N)
spectrum = np.fft.rfft(noise)
freq = np.fft.rfftfreq(N, 1 / RATE)
spectrum *= np.exp(-((freq - 700) / 650) ** 2)
air = np.fft.irfft(spectrum, n=N)
t = np.arange(N) / RATE
air *= .0025 * (.7 + .3 * np.cos(2 * np.pi * t / SECONDS))
mix[:, 0] += air
mix[:, 1] += np.roll(air, 971)

# Conservative headroom before the player's independently adjustable music gain.
peak = float(np.max(np.abs(mix)))
mix *= .42 / max(peak, 1e-9)
output = Path(__file__).resolve().parents[1] / "Assets/Nemequene/UI/Resources/Nemequene/Menu_Bruma.wav"
output.parent.mkdir(parents=True, exist_ok=True)
pcm = (np.clip(mix, -1, 1) * 32767).astype('<i2')
with wave.open(str(output), 'wb') as audio:
    audio.setnchannels(2)
    audio.setsampwidth(2)
    audio.setframerate(RATE)
    audio.writeframes(pcm.tobytes())
report = {"title": "Bruma del umbral", "seconds": SECONDS, "sample_rate": RATE,
          "channels": 2, "peak_dbfs": float(20 * np.log10(np.max(np.abs(mix)))),
          "rms_dbfs": float(20 * np.log10(np.sqrt(np.mean(mix ** 2)))),
          "clipped_samples": int(np.count_nonzero(np.abs(mix) >= 1)),
          "seam_sample_delta": float(np.max(np.abs(mix[-1] - mix[0]))),
          "source": "Original deterministic synthesis; no external samples"}
report_path = Path(__file__).resolve().parents[1] / "Specs/UI/TitleMenu/music-validation.json"
report_path.parent.mkdir(parents=True, exist_ok=True)
report_path.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report))
