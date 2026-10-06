# Decimates the Tripo models of Assets/_Game/Art/Environments/MundoInferior/Models (~2M tris each) into
# <folder>/<ID>_Optimizado.fbx next to the originals, as done for the Plaza Núñez props.
# Run: blender -b --python tools/Blender/optimize_mundo_inferior.py [-- <models dir>] [IDs...]
# Already optimized folders are skipped; delete an *_Optimizado.fbx to redo it.
import bpy, os, glob, sys

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
ROOT = args.pop(0) if args and os.path.isdir(args[0]) else \
    os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Models", "Mundo_Inferior")
ONLY = set(args)

# Folder -> (kit ID of the guide, target triangles). Walkable and architectural pieces keep more detail.
# Not listed: "stone+arch+3d+model" (A01a, broken 464-tri export) and "stone+table+3d+model (1)" (copy of H03a).
KIT = {
    "stone+platform+3d+model": ("T01", 30000),
    "stone+platform+3d+model (1)": ("T02", 40000),
    "stone+circular+platform+3d+model": ("T03", 30000),
    "stone+pedestal+3d+model (1)": ("T05", 30000),
    "fantasy+cave+doorway+3d+model": ("T06a", 50000),
    "rocky+cavern+3d+model": ("T06b", 50000),
    "stone+ruin+wall+3d+model": ("T06c", 40000),
    "stone+bench+3d+model (1)": ("T07", 30000),
    "stone+rock+3d+model": ("T08a", 20000),
    "rock+block+3d+model": ("T08b", 15000),
    "rock+formation+3d+model": ("T08d", 20000),
    "stone+arch+3d+model (1)": ("A01b", 40000),
    "medieval+metal+gate+3d+model": ("A02", 30000),
    "stone+well+cover+3d+model": ("A03", 25000),
    "stone+archway+3d+model": ("A04", 40000),
    "stone+rock+arch+3d+model": ("A05", 50000),
    "stone+wall+3d+model": ("A06a", 15000),
    "stone+brick+wall+3d+model": ("A06b", 15000),
    "stone+wall+3d+model (1)": ("A06c", 15000),
    "gnarled+tree+3d+model": ("N01", 60000),
    "fallen+log+with+roots+3d+model": ("N02a", 25000),
    "twisted+wood+3d+model": ("N02b", 25000),
    "tree+stump+3d+model": ("N02c", 25000),
    "wooden+ladder+3d+model": ("N02d", 25000),
    "rock+platform+3d+model": ("N03", 25000),
    "succulent+plant+3d+model": ("N04", 25000),
    "crystal+rock+3d+model": ("N05", 15000),
    "rocky+terrain+3d+model": ("H01", 30000),
    "vine-wrapped+pillar+3d+model": ("H02", 30000),
    "stone+table+3d+model": ("H03a", 20000),
    "stone+block+3d+model": ("H03b", 20000),
    "stone+stalactite+3d+model": ("H04", 15000),
    "stone+catapult+3d+model": ("H05", 20000),
    "marble+bracelet+3d+model": ("O01a", 15000),
    "ornate+horn+bracelet+3d+model": ("O01b", 15000),
    "drinking+horn+3d+model": ("O02", 15000),
    "golden+dragon+egg+3d+model": ("O03", 20000),
    "stone+table+3d+model (2)": ("O04a", 25000),
    "stone+bench+3d+model": ("O04b", 20000),
    "stone+curved+bridge+3d+model": ("O04c", 20000),
    "stone+pedestal+3d+model": ("O04d", 25000),
    "stone+well+3d+model": ("O04e", 25000),
    "cracked+stone+disk+3d+model": ("C03c", 20000),
    "stone+ruin+shard+3d+model": ("C03d", 15000),
}

for folder, (kit_id, target) in KIT.items():
    if ONLY and kit_id not in ONLY:
        continue
    full = os.path.join(ROOT, folder)
    out = os.path.join(full, kit_id + "_Optimizado.fbx")
    if os.path.exists(out):
        continue
    src = glob.glob(os.path.join(full, "tripo_convert*.fbx"))
    if not src:
        print("OPT_MISSING", folder, flush=True)
        continue
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=src[0])
    meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    obj.name = kit_id
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.decimate_type = 'COLLAPSE'
    mod.ratio = min(1.0, target / max(tris, 1))
    mod.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier=mod.name)
    after = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    # Textures stay in the .fbm folder: the Unity setup builds URP materials from them.
    bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'MESH'},
                             path_mode='STRIP', embed_textures=False, mesh_smooth_type='FACE', add_leaf_bones=False)
    print("OPT", kit_id, tris, "->", after, flush=True)
print("OPT_ALL_DONE", flush=True)
