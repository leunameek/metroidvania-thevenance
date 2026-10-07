# Decimates the Tripo models of "Assets/_Game/Art/Environments/MundoSuperior/Models" (~2M tris each) into
# <folder>/MS_<ID>[_<variant>]_Optimizado.fbx next to the originals, as done for the lower world.
# The Mundo Superior builder picks these files up by name (MS_<ID>...).
# Run: blender -b --python tools/Blender/optimize_mundo_superior.py [-- <models dir>] [IDs...]
# Already optimized folders are skipped; delete an *_Optimizado.fbx to redo it.
# Exports under 2,000 triangles are reported as BROKEN and skipped (empty Tripo exports, 38 KB).
import bpy, os, glob, sys

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
ROOT = args.pop(0) if args and os.path.isdir(args[0]) else \
    os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "Models", "Mundo Superior")
ONLY = set(args)

# Folder -> (output name, target triangles). Budgets follow the instance counts of the guide
# (45 slabs, 84 bands, 54 railings...): many copies get fewer triangles.
KIT = {
    "T01": ("MS_T01", 8000), "T01+BORDE+ROTO": ("MS_T01_BordeRoto", 8000), "T01-+GRIETA": ("MS_T01_Grieta", 8000),
    "T02-+V2": ("MS_T02", 10000), "T03": ("MS_T03", 6000), "T04": ("MS_T04", 6000), "T05": ("MS_T05", 15000),
    "T06": ("MS_T06", 3000), "T06+CORTE+ESCALERA": ("MS_T06_CorteEscalera", 3000), "T06+CORTE+PUERTA": ("MS_T06_CortePuerta", 3000),
    "T07": ("MS_T07", 5000), "T08": ("MS_T08", 5000), "T09": ("MS_T09", 6000),
    "A01": ("MS_A01", 20000), "A01-+TRESMUESCAS": ("MS_A01_TresMuescas", 20000), "A02": ("MS_A02", 10000),
    "A03": ("MS_A03", 25000), "A04": ("MS_A04", 15000), "A05": ("MS_A05", 4000), "A06": ("MS_A06", 2000),
    "A07": ("MS_A07", 10000), "A08": ("MS_A08", 15000),
    "O01": ("MS_O01", 15000), "O02": ("MS_O02", 15000), "O03": ("MS_O03", 15000), "O03-+ALAS+EQUIPABLES": ("MS_O03_Equipables", 15000),
    "O04": ("MS_O04", 12000), "O05": ("MS_O05", 12000), "O06": ("MS_O06", 15000), "O07": ("MS_O07", 15000),
    "O08": ("MS_O08", 8000), "O08+ALAS": ("MS_O08_Alas", 8000), "O08+MEDALLON": ("MS_O08_Medallon", 8000),
    "O09": ("MS_O09", 10000), "O10": ("MS_O10", 12000),
    "E01": ("MS_E01", 3000), "E01-BAJA": ("MS_E01_Baja", 3000), "E01-ROTA": ("MS_E01_Rota", 3000),
    "E03": ("MS_E03", 2000), "E04": ("MS_E04", 25000), "E04 (1)": ("MS_E04_B", 25000),
    "E05": ("MS_E05", 8000), "E05-ENTRADA": ("MS_E05_Entrada", 8000), "E06": ("MS_E06", 2000),
    "E07": ("MS_E07", 2000), "E08": ("MS_E08", 10000),
}

for folder, (name, target) in KIT.items():
    kit_id = name[3:6]
    if ONLY and kit_id not in ONLY:
        continue
    full = os.path.join(ROOT, folder)
    out = os.path.join(full, name + "_Optimizado.fbx")
    if os.path.exists(out):
        continue
    src = glob.glob(os.path.join(full, "tripo_convert*.fbx"))
    if not src:
        print("OPT_MISSING", folder, flush=True)
        continue
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=src[0])
    meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    if not meshes:
        print("OPT_BROKEN", folder, "no mesh", flush=True)
        continue
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    obj.name = name
    tris = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    dims = tuple(round(d, 3) for d in obj.dimensions)
    if tris < 2000:
        print("OPT_BROKEN", folder, tris, "tris", dims, flush=True)
        continue
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.decimate_type = 'COLLAPSE'
    mod.ratio = min(1.0, target / max(tris, 1))
    mod.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier=mod.name)
    after = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    # Textures stay in the .fbm folder: the Unity builder makes URP materials from them.
    bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'MESH'},
                             path_mode='STRIP', embed_textures=False, mesh_smooth_type='FACE', add_leaf_bones=False)
    print("OPT", folder, name, tris, "->", after, "dims", dims, flush=True)
print("OPT_ALL_DONE", flush=True)
