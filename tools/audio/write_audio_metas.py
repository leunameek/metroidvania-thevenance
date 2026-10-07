"""Import settings of every game sound (and .meta files for the new ones).

Short cues: decompressed on load and preloaded, so the first play is never late. Long loops
(music, ambience beds, longer than 8 s): streamed Vorbis. Existing .meta files keep their GUID;
only the AudioImporter settings are rewritten. New clips and folders get a fresh GUID.

Usage: python tools/audio/write_audio_metas.py
"""
import glob
import os
import uuid
import wave

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FOLDERS = ["Assets/_Game/Resources/GameAudio", "Assets/_Game/Resources/MIAudio", "Assets/_Game/Resources/MSAudio",
           "Assets/_Game/Audio/PlazaNunez", "Assets/_Game/UI/Resources/Nemequene"]
TAIL = "  userData: \n  assetBundleName: \n  assetBundleVariant: \n"


def importer(guid, streaming):
    load, quality, preload = (2, .55, 0) if streaming else (0, .7, 1)
    return (f"fileFormatVersion: 2\nguid: {guid}\nAudioImporter:\n  externalObjects: {{}}\n  serializedVersion: 8\n"
            f"  defaultSettings:\n    serializedVersion: 2\n    loadType: {load}\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n"
            f"    compressionFormat: 1\n    quality: {quality}\n    conversionMode: 0\n    preloadAudioData: {preload}\n"
            f"  platformSettingOverrides: {{}}\n  forceToMono: 0\n  normalize: 0\n  loadInBackground: {1 if streaming else 0}\n"
            f"  ambisonic: 0\n  3D: 1\n" + TAIL)


def guid_of(meta):
    for line in open(meta, encoding="utf-8"):
        if line.startswith("guid:"):
            return line.split()[1]
    return None


def main():
    os.chdir(ROOT)
    made = changed = 0
    for folder in FOLDERS:
        d = folder
        while d != "Assets":
            if not os.path.exists(d + ".meta"):
                open(d + ".meta", "w", newline="\n").write(
                    f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n" + TAIL)
                made += 1
            d = os.path.dirname(d)
        for sub in glob.glob(folder + "/**/", recursive=True):
            sub = sub.rstrip("/\\")
            if sub != folder and not os.path.exists(sub + ".meta"):
                open(sub + ".meta", "w", newline="\n").write(
                    f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n" + TAIL)
                made += 1
        for wav in glob.glob(folder + "/**/*.wav", recursive=True):
            with wave.open(wav) as w:
                seconds = w.getnframes() / w.getframerate()
            meta = wav + ".meta"
            guid = guid_of(meta) if os.path.exists(meta) else None
            if guid is None:
                guid = uuid.uuid4().hex; made += 1
            else:
                changed += 1
            open(meta, "w", newline="\n").write(importer(guid, seconds > 8.0))
    print("metas new:", made, "rewritten:", changed)


if __name__ == "__main__":
    main()
