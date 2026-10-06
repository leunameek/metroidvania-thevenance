"""Creates Unity .meta files (fresh GUIDs) for new untracked assets and their folders, so the
GUIDs are fixed in git instead of being generated differently by each editor that opens them."""
import os, subprocess, uuid
out = subprocess.run(["git", "status", "--porcelain", "--untracked-files=all"], capture_output=True, text=True).stdout
paths = [l[3:].strip().strip('"') for l in out.splitlines() if l.startswith("??") and l[3:].startswith("Assets/")]
paths = [p for p in paths if not p.endswith(".meta")]
dirs = set()
for p in paths:
    d = os.path.dirname(p)
    while d and d != "Assets":
        dirs.add(d); d = os.path.dirname(d)
tail = "  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
for d in sorted(dirs):
    if not os.path.exists(d + ".meta"):
        open(d + ".meta", "w", newline="\n").write(f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n" + tail); print(d + ".meta")
for p in paths:
    if os.path.exists(p + ".meta"): continue
    g = uuid.uuid4().hex
    if p.endswith(".cs"):
        body = f"fileFormatVersion: 2\nguid: {g}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {{instanceID: 0}}\n" + tail
    else:
        body = f"fileFormatVersion: 2\nguid: {g}\nTextScriptImporter:\n  externalObjects: {{}}\n" + tail
    open(p + ".meta", "w", newline="\n").write(body); print(p + ".meta")
