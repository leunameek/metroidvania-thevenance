"""Procedural, seamlessly tiling textures of the Mundo Inferior graybox (guide 5.1 palette).

Usage: python tools/textures/generate_mundo_inferior_textures.py
Writes albedo + normal PNGs to Assets/_Game/Art/Environments/MundoInferior/Materials/Textures:
  MI_Piedra  cliffs and walls, blue-grey stone #666A7D with strata and cracks (2 m tile)
  MI_Suelo   walkable tops, blue-green flagstones #6B97A4 with dark joints (2 m tile)
  MI_Fondo   pit floors and depth, #252630 rubble
Every noise field is periodic (FFT-filtered white noise, wrapped Voronoi), so tiles have no seams.
"""
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "Worlds", "MundoInferior", "Materials", "Textures")
N = 1024
rng = np.random.default_rng(42)


def periodic_noise(scale, power=2.0):
    # Fractal noise with a 1/f^power spectrum, band-limited around `scale` cycles per tile.
    white = rng.standard_normal((N, N))
    f = np.fft.fftfreq(N) * N
    fx, fy = np.meshgrid(f, f)
    r = np.sqrt(fx ** 2 + fy ** 2); r[0, 0] = 1
    spectrum = np.fft.fft2(white) / (r ** power) * (r >= scale * .5)
    x = np.real(np.fft.ifft2(spectrum))
    return (x - x.mean()) / (x.std() + 1e-9)


def voronoi(cells, jitter=0.85):
    # Wrapped Voronoi: distance to nearest and second nearest feature point, tile-periodic.
    g = cells
    pts = (np.stack(np.meshgrid(np.arange(g), np.arange(g)), -1) + .5 + (rng.random((g, g, 2)) - .5) * jitter) / g
    pts = pts.reshape(-1, 2)
    ys, xs = np.mgrid[0:N, 0:N] / N
    d1 = np.full((N, N), 9.0); d2 = np.full((N, N), 9.0); idx = np.zeros((N, N), int)
    for i, (px, py) in enumerate(pts):
        dx = np.abs(xs - px); dx = np.minimum(dx, 1 - dx)
        dy = np.abs(ys - py); dy = np.minimum(dy, 1 - dy)
        d = np.sqrt(dx * dx + dy * dy)
        closer = d < d1
        d2 = np.where(closer, d1, np.minimum(d2, d)); idx = np.where(closer, i, idx); d1 = np.where(closer, d, d1)
    return d1, d2, idx


def normal_from_height(h, strength):
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * strength
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * strength
    n = np.stack([-dx, dy, np.ones_like(h)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return ((n * .5 + .5) * 255).astype(np.uint8)


def hex_rgb(h):
    return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)], float) / 255


def save(name, albedo, height, strength):
    os.makedirs(OUT, exist_ok=True)
    Image.fromarray((np.clip(albedo, 0, 1) * 255).astype(np.uint8), "RGB").save(os.path.join(OUT, name + "_albedo.png"), optimize=True)
    Image.fromarray(normal_from_height(height, strength), "RGB").save(os.path.join(OUT, name + "_normal.png"), optimize=True)


def stone():
    base = hex_rgb("666A7D")
    large = periodic_noise(3, 2.2); mid = periodic_noise(12, 1.8); fine = periodic_noise(60, 1.2)
    ys = np.mgrid[0:N, 0:N][0] / N
    strata = np.sin((ys * 6 + large * .08) * 2 * np.pi) * .5 + .5  # horizontal bedding, 6 per tile
    d1, d2, _ = voronoi(9, 1.0)
    crack = np.clip(1 - (d2 - d1) * 110, 0, 1) ** 4
    height = large * .35 + mid * .35 + fine * .15 + strata * .35 - crack * 1.2
    shade = .78 + large * .07 + mid * .06 + fine * .04 + (strata - .5) * .18 - crack * .16
    tint = np.stack([shade] * 3, -1) * base / base.mean() * .52
    tint += (periodic_noise(5, 2)[..., None] * np.array([-.01, .005, .02]))  # cold/warm drift
    save("MI_Piedra", tint, height, 3.0)


def floor():
    base = hex_rgb("6B97A4"); joint = hex_rgb("2E4048")
    d1, d2, idx = voronoi(6, .55)  # flagstones of ~33 cm on a 2 m tile
    edge = d2 - d1
    mortar = np.clip(1 - edge * 55, 0, 1)
    per = rng.random(idx.max() + 1)[idx]  # per-slab value
    wear = periodic_noise(20, 1.4); fine = periodic_noise(80, 1.0); big = periodic_noise(3, 2.0)
    bevel = np.clip(edge * 22, 0, 1)
    height = bevel * 1.0 + wear * .12 + fine * .06 - mortar * .6
    shade = .70 + (per - .5) * .16 + wear * .05 + fine * .03 + big * .05
    slab = np.stack([shade] * 3, -1) * base / base.mean() * .62
    slab = slab * (1 - mortar[..., None]) + joint * .55 * mortar[..., None]
    # Lighter worn rims make the walkable top read at a glance (guide 7.2).
    slab += (bevel < .6)[..., None] * (1 - mortar[..., None]) * .04
    save("MI_Suelo", slab, height, 2.5)


def depth():
    base = hex_rgb("252630")
    d1, d2, _ = voronoi(14, 1.0)
    pebble = np.clip(1 - d1 * 16, 0, 1)
    n = periodic_noise(8, 1.8); fine = periodic_noise(70, 1.1)
    height = pebble * .8 + n * .3 + fine * .2
    shade = .9 + pebble * .25 + n * .08 + fine * .05
    save("MI_Fondo", np.stack([shade] * 3, -1) * base / base.mean() * .2, height, 2.0)


if __name__ == "__main__":
    stone(); floor(); depth()
    print("MI textures written to", OUT)
