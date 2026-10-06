"""Builds the runtime UI sprites of El Asedio de Bacatá from the Bacata_UI_Premium package.

Usage:  python tools/ui/build_bacata_ui_sprites.py <folder that contains Bacata_UI_Premium>

The package ships illustrated frames whose bird emblem and dark feather would stretch under
9-slice scaling. This script separates them (as the design document asks): it writes a clean
frame for slicing plus the emblem and the feather as independent overlays. Icons are turned
white so Unity can tint them gold or bone. Output goes to
Assets/_Game/UI/Resources/Nemequene/Bacata; the import settings live in
Assets/_Game/UI/Editor/BacataSpriteImporter.cs.
"""
import os
import shutil
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, "Assets", "Nemequene", "UI", "Resources", "Nemequene", "Bacata")


def load(path):
    return np.array(Image.open(path).convert("RGBA")).astype(np.float32)


def save(array, *parts):
    path = os.path.join(OUT, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    Image.fromarray(np.clip(array, 0, 255).astype(np.uint8), "RGBA").save(path, optimize=True)
    return path


def mirror_clean(src, patch_x, patch_strip, patch_height):
    """Left half mirrored onto the right (drops the feather), then the top-centre emblem area
    is covered with a plain stretch of the top border."""
    h, w, _ = src.shape
    half = w // 2
    clean = src.copy()
    clean[:, w - half:] = src[:, :half][:, ::-1]
    s0, s1 = patch_strip
    strip = clean[:patch_height, s0:s1].copy()
    x = patch_x[0]
    while x < patch_x[1]:
        n = min(s1 - s0, patch_x[1] - x)
        clean[:patch_height, x:x + n] = strip[:, :n]
        x += n
    return clean


def overlay(src, clean, box, threshold=10, ramp=45):
    """Pixels of src that differ from clean inside box, as a transparent overlay (cropped)."""
    x0, y0, x1, y1 = box
    a = src[y0:y1, x0:x1]
    b = clean[y0:y1, x0:x1]
    colour = np.abs(a[..., :3] - b[..., :3]).max(axis=2)
    alpha_gain = np.clip(a[..., 3] - b[..., 3], 0, 255) / 255.0
    mask = np.clip((colour - threshold) / ramp, 0, 1)
    mask = np.maximum(mask * (a[..., 3] / 255.0), alpha_gain)
    mask_img = Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(1.2))
    out = a.copy()
    out[..., 3] = np.array(mask_img, dtype=np.float32)
    ys, xs = np.nonzero(out[..., 3] > 8)
    cy0, cy1, cx0, cx1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    return out[cy0:cy1, cx0:cx1], (x0 + cx0, y0 + cy0, x0 + cx1, y0 + cy1)


def white(src):
    out = src.copy()
    out[..., :3] = 255
    return out


def resize(array, size):
    img = Image.fromarray(np.clip(array, 0, 255).astype(np.uint8), "RGBA").resize(size, Image.LANCZOS)
    return np.array(img).astype(np.float32)


def main(package):
    base = os.path.join(package, "Bacata_UI_Premium")
    comp = os.path.join(base, "componentes")
    if os.path.isdir(OUT):
        shutil.rmtree(OUT)

    # Panel frame: 1920 x 1300, emblem centred on the top edge, feather in the lower right.
    panel = load(os.path.join(comp, "PNL_standard_normal.png"))
    clean = mirror_clean(panel, (700, 1220), (590, 700), 150)
    # The feather tips reach the left half along the bottom edge: cover them with plain border.
    h, w, _ = panel.shape
    strip = clean[h - 130:, 240:480].copy()
    for x in range(480, w - 480, 240):
        n = min(240, w - 480 - x)
        clean[h - 130:, x:x + n] = strip[:, :n]
    save(clean, "Frames", "Frame_Panel.png")
    emblem, ebox = overlay(panel, clean, (680, 0, 1240, 150), threshold=28, ramp=40)
    save(emblem, "Frames", "Frame_Emblem.png")
    feather, fbox = overlay(panel, clean, (w // 2, 160, w - 90, h - 90), threshold=8, ramp=40)
    save(feather, "Frames", "Frame_Feather.png")
    print("panel", w, h, "emblem box", ebox, "feather box", fbox,
          "feather offset from right/bottom", (w - fbox[2], h - fbox[3]))

    key = load(os.path.join(comp, "PNL_key_normal.png"))
    save(mirror_clean(key, (52, 108), (36, 52), 36), "Frames", "Frame_Key.png")

    shutil.copy(os.path.join(comp, "Marco_HUD.png"), os.path.join(OUT, "Frames", "Frame_HUD.png"))

    # Ribbons and lines for buttons and tabs.
    for name, src in [
        ("Ribbon", "BTN_primary_normal"), ("Ribbon_Focus", "BTN_secondary_focused"),
        ("Ribbon_Disabled", "BTN_primary_disabled"), ("Line", "BTN_secondary_normal"),
        ("Line_Disabled", "BTN_secondary_disabled"),
    ]:
        os.makedirs(os.path.join(OUT, "Controls"), exist_ok=True)
        shutil.copy(os.path.join(comp, src + ".png"), os.path.join(OUT, "Controls", name + ".png"))

    # Bars: dark track plus one white fill that each role tints (health, progress, voice).
    os.makedirs(os.path.join(OUT, "Controls"), exist_ok=True)
    shutil.copy(os.path.join(comp, "BAR_track.png"), os.path.join(OUT, "Controls", "Bar_Track.png"))
    fill = load(os.path.join(comp, "BAR_fill_progress.png"))
    lum = fill[..., :3].mean(axis=2)
    peak = max(1.0, np.percentile(lum[fill[..., 3] > 128], 95))
    fill[..., :3] = np.clip(lum / peak, 0, 1)[..., None] * 255
    save(fill, "Controls", "Bar_Fill.png")

    # Toggle: the package draws the "on" state; "off" is its mirror with a dim knob.
    on = load(os.path.join(comp, "INPUT_toggle_normal.png"))
    save(on, "Controls", "Toggle_On.png")
    off = on[:, ::-1].copy()
    knob = (off[..., 0] > 150) & (off[..., 1] > 120)
    off[knob, :3] *= .45
    save(off, "Controls", "Toggle_Off.png")

    # Icons: white on alpha, 256 px, so the same sprite reads gold, bone or crimson.
    icons = os.path.join(base, "iconos")
    for f in sorted(os.listdir(icons)):
        if f.endswith("_hueso.png"):
            name = f[len("ICO_"):-len("_hueso.png")]
            save(resize(white(load(os.path.join(icons, f))), (256, 256)), "Icons", name + ".png")

    # Art.
    art = os.path.join(base, "arte")
    os.makedirs(os.path.join(OUT, "Art"), exist_ok=True)
    for f in ["Portada_Bacata", "Titulo_Bacata", "Plaza_Nunez", "Mundo_Inferior", "Mundo_Superior",
              "Vasija", "Disco", "Guardian", "Mapa_Bacata"]:
        shutil.copy(os.path.join(art, f + ".png"), os.path.join(OUT, "Art", f + ".png"))

    # HUD portrait: the face of Retrato_Jugador in a disc that fills the ring of Frame_HUD.
    portrait = Image.open(os.path.join(art, "Retrato_Jugador.png")).convert("RGBA").crop((385, 170, 1135, 920))
    disc = Image.new("RGBA", portrait.size, (21, 23, 25, 255))
    disc.alpha_composite(portrait)
    disc = disc.resize((256, 256), Image.LANCZOS)
    mask = Image.new("L", (256 * 4, 256 * 4), 0)
    ImageDraw.Draw(mask).ellipse((0, 0, 256 * 4 - 1, 256 * 4 - 1), fill=255)
    disc.putalpha(mask.resize((256, 256), Image.LANCZOS))
    disc.save(os.path.join(OUT, "Art", "Retrato_HUD.png"), optimize=True)
    write_metas()


# Unity import settings: (pixels per unit, 9-slice border left/bottom/right/top in source px,
# max size, compression). Components are drawn at twice their logical size, hence 200 ppu.
IMPORT = {
    "Frames/Frame_Panel.png": (200, (200, 180, 200, 180)),
    "Frames/Frame_Key.png": (200, (36, 32, 36, 32)),
    "Controls/Ribbon.png": (200, (140, 95, 140, 95)),
    "Controls/Ribbon_Focus.png": (200, (140, 95, 140, 95)),
    "Controls/Ribbon_Disabled.png": (200, (140, 95, 140, 95)),
    "Controls/Line.png": (200, (140, 95, 140, 95)),
    "Controls/Line_Disabled.png": (200, (140, 95, 140, 95)),
    "Controls/Bar_Track.png": (200, (30, 31, 30, 31)),
    "Controls/Bar_Fill.png": (200, (12, 11, 12, 11)),
}

META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: {l}, y: {b}, z: {r}, w: {t}}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {size}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: {compression}
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid(relative):
    import hashlib
    return hashlib.md5(("bacata-ui/" + relative).encode("utf8")).hexdigest()


def write_metas():
    for folder, dirs, files in os.walk(OUT):
        rel_folder = os.path.relpath(folder, OUT).replace(os.sep, "/")
        with open(folder + ".meta", "w", newline="\n") as meta:
            meta.write(FOLDER_META.format(guid=guid(rel_folder)))
        for f in files:
            if not f.endswith(".png"):
                continue
            rel = (f if rel_folder == "." else rel_folder + "/" + f)
            art = rel.startswith("Art/")
            ppu, border = IMPORT.get(rel, (100 if art else 200, (0, 0, 0, 0)))
            l, b, r, t = border
            with open(os.path.join(folder, f) + ".meta", "w", newline="\n") as meta:
                meta.write(META.format(guid=guid(rel), ppu=ppu, l=l, b=b, r=r, t=t,
                                       size=2048 if art else 4096, compression=1 if art else 2))


if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    main(sys.argv[1])
