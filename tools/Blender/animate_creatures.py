# Procedural clips for the three non-humanoid Tripo rigs of the campaign (serpiente bicéfala,
# jaguar and guacamaya) and a lighter copy of the 2M-triangle mujer-cóndor.
# Writes next to each original:  <folder>/Animations/<Name>_Animado.fbx  (mesh + armature + takes)
# Run: blender -b --python tools/Blender/animate_creatures.py -- "<...>/Assets/_Game/Art/Characters"
#
# Rigs (Blender space: Z up, the creatures look toward -Y, X is their side):
#  Serpiente: Root; neck A = tripo::Head_0..6 (+bone_6 jaw); neck B = bone_17..20; coils = tripo::Tail_0..7.
#  Jaguar: pelvis = tripo::0_Left_Limb_0/1; spine tripo::Spine_0..2 -> Head_0/1; legs: front R = Spine_3,
#          Spine_4, bone_10..13; front L = bone_14..19; rear R = bone_21..24; rear L = 0_Left_Limb_2..6;
#          tail = bone_30..34.
#  Guacamaya: body Root/Spine_0/Head_0, tail bone_3/Tail_0, legs 0_Left/Right_Limb_0..3,
#          wings bone_13 -> right bone_14..17 (+18..22), left bone_23..26 (+27, 28).
import bpy, math, os, sys
from mathutils import Quaternion, Vector

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
ROOT = args[0] if args else os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "_Game", "Art", "Characters")
FPS = 30
X, Y, Z = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))


def load(folder):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path = next(os.path.join(ROOT, folder, f) for f in os.listdir(os.path.join(ROOT, folder)) if f.lower().endswith(".fbx"))
    bpy.ops.import_scene.fbx(filepath=path)
    arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    bpy.context.scene.render.fps = FPS
    return arm


def pb(arm, name):
    return arm.pose.bones[name]


class Clip:
    """One action: frames 0..length; keys(frame, {bone: rotation quaternion}, {bone: location})."""

    def __init__(self, arm, name, length):
        self.arm, self.name, self.length = arm, name, length
        self.action = bpy.data.actions.new(name)
        self.action.use_fake_user = True
        arm.animation_data_create()
        arm.animation_data.action = self.action
        for b in arm.pose.bones:
            b.rotation_mode = "QUATERNION"

    def key(self, frame, pose, loc=None):
        # Every bone is keyed on every key frame (rest when not named) so takes never leak into each other.
        for b in self.arm.pose.bones:
            b.rotation_quaternion = pose.get(b.name, Quaternion())
            b.location = (loc or {}).get(b.name, Vector())
            b.keyframe_insert("rotation_quaternion", frame=frame)
            b.keyframe_insert("location", frame=frame)

    def sample(self, fn, step=2):
        for f in range(0, self.length + 1, step):
            pose, loc = fn(f / self.length)
            self.key(f, pose, loc)
        if self.length % step: self.key(self.length, *fn(1.0))


def turn(arm, bone, axis, degrees):
    """Rotation of `bone` around a world (armature) axis, expressed in the bone's own rest frame."""
    local = arm.data.bones[bone].matrix_local.to_3x3().inverted() @ axis
    return Quaternion(local.normalized(), math.radians(degrees))


def combine(arm, items):
    pose = {}
    for bone, axis, deg in items:
        if bone not in arm.pose.bones or abs(deg) < 1e-4: continue
        q = turn(arm, bone, axis, deg)
        pose[bone] = q @ pose[bone] if bone in pose else q
    return pose


def ease(t): return t * t * (3 - 2 * t)
def pulse(t, a, b):  # 0 -> 1 -> 0 between a and b
    if t <= a or t >= b: return 0.0
    u = (t - a) / (b - a); return math.sin(u * math.pi)
def ramp(t, a, b):  # 0 before a, 1 after b
    return ease(min(1, max(0, (t - a) / (b - a))))


def export(arm, folder, name):
    out_dir = os.path.join(ROOT, folder, "Animations")
    os.makedirs(out_dir, exist_ok=True)
    out = os.path.join(out_dir, name + "_Animado.fbx")
    arm.animation_data.action = None
    for b in arm.pose.bones:
        b.rotation_quaternion = Quaternion(); b.location = Vector()
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={"ARMATURE", "MESH"},
                             apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
                             add_leaf_bones=False, use_armature_deform_only=False, bake_anim=True,
                             bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                             bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.5,
                             path_mode="AUTO", embed_textures=False, mesh_smooth_type="FACE")
    print("CREATURE_OK", out, [a.name for a in bpy.data.actions])


# ---------------------------------------------------------------- serpiente bicéfala
def serpent():
    folder = "Serpiente+Bicéfala"
    arm = load(folder)
    A = ["tripo::Head_0", "tripo::Head_1", "tripo::Head_2", "tripo::Head_3"]
    B = ["bone_17", "bone_18", "bone_19", "bone_20"]
    C = ["tripo::Tail_%d" % i for i in range(8)]
    jaw = "bone_6"

    def neck(chain, fwd, side, weights=(.5, .8, 1, .7)):
        return [(b, X, fwd * w) for b, w in zip(chain, weights)] + [(b, Y, side * w) for b, w in zip(chain, weights)]

    def idle(t):  # two necks breathing on different rhythms (2 and 3 cycles), coils swelling
        a = math.sin(t * 2 * math.pi * 2); b = math.sin(t * 2 * math.pi * 3 + 1)
        items = neck(A, 5 * a, 4 * math.cos(t * 2 * math.pi * 2)) + neck(B, 6 * b, -5 * math.cos(t * 2 * math.pi * 3))
        items += [(c, X, 1.5 * math.sin(t * 2 * math.pi * 2 + i * .7)) for i, c in enumerate(C)]
        items += [(jaw, X, 4 + 3 * max(0, a))]
        return combine(arm, items), None

    def attack_a(t):  # head A rears back (aviso), then strikes forward and recovers
        f = -28 * pulse(t, 0, .45) + 46 * pulse(t, .4, .8)
        items = neck(A, f, 0) + neck(B, -6 * pulse(t, .3, .9), 4) + [(jaw, X, 25 * pulse(t, .35, .75))]
        return combine(arm, items), None

    def shake_a(t):  # head A shakes off fragments
        items = neck(A, 10 * pulse(t, 0, 1), 18 * math.sin(t * 2 * math.pi * 4) * pulse(t, 0, 1))
        return combine(arm, items), None

    def attack_b(t):  # head B: long lateral sweep
        s = 30 * pulse(t, 0, .4) - 55 * pulse(t, .35, .85)
        items = neck(B, 18 * pulse(t, .3, .9), s, (.6, .9, 1, .8))
        return combine(arm, items), None

    def pulse_b(t):  # head B throat pulse: rises, holds, pushes
        items = neck(B, -20 * pulse(t, 0, .5) + 30 * pulse(t, .5, .9), 0)
        return combine(arm, items), None

    def hit(chain):
        return lambda t: (combine(arm, neck(chain, -30 * pulse(t, 0, 1), 12 * pulse(t, 0, .6))), None)

    def released(t):  # both necks lower away from the altar and come to rest
        k = ramp(t, 0, .8)
        items = neck(A, 32 * k, 10 * k) + neck(B, 32 * k, -10 * k) + [(c, X, -3 * k) for c in C]
        return combine(arm, items), None

    def rest(t):  # looping calm after the release
        k = 1 + .05 * math.sin(t * 2 * math.pi)
        items = neck(A, 32 * k, 10) + neck(B, 32 * k, -10) + [(c, X, -3) for c in C]
        return combine(arm, items), None

    def emerge(t):  # coiled down -> both heads rise
        k = 1 - ramp(t, 0, .9)
        items = neck(A, 70 * k, 0) + neck(B, 70 * k, 0) + [(c, X, -6 * k) for c in C]
        return combine(arm, items), None

    for name, length, fn in [("Idle", 90, idle), ("AtaqueA", 42, attack_a), ("SacudidaA", 36, shake_a),
                             ("AtaqueB", 42, attack_b), ("PulsoB", 40, pulse_b), ("GolpeA", 18, hit(A)),
                             ("GolpeB", 18, hit(B)), ("Liberada", 60, released), ("Reposo", 90, rest),
                             ("Emerger", 50, emerge)]:
        Clip(arm, name, length).sample(fn)
    export(arm, folder, "Serpiente")


# ---------------------------------------------------------------- jaguar
def jaguar():
    folder = "Jaguar+(Transformación)"
    arm = load(folder)
    spine = ["tripo::Spine_0", "tripo::Spine_1", "tripo::Spine_2"]
    head = ["tripo::Head_0", "tripo::Head_1"]
    fr = ["tripo::Spine_3", "tripo::Spine_4", "bone_10", "bone_11", "bone_12"]
    fl = ["bone_14", "bone_15", "bone_16", "bone_17", "bone_18"]
    rr = ["bone_21", "bone_22", "bone_23"]
    rl = ["tripo::0_Left_Limb_2", "tripo::0_Left_Limb_3", "tripo::0_Left_Limb_4"]
    tail = ["bone_30", "bone_31", "bone_32", "bone_33", "bone_34"]

    def leg(chain, swing, bend):
        # swing: + moves the paw backward; bend folds the lower joints alternately (knee/ankle)
        out = [(chain[0], X, swing)]
        for i, b in enumerate(chain[1:]):
            out.append((b, X, bend * (1 if i % 2 == 0 else -.6)))
        return out

    def tail_wave(t, amp, cycles=1):
        return [(b, Z, amp * math.sin(t * 2 * math.pi * cycles - i * .8)) for i, b in enumerate(tail)]

    def gait(t, amp, bend, cycles=1):
        p = t * 2 * math.pi * cycles
        items = leg(fl, amp * math.sin(p), bend * max(0, math.cos(p))) + leg(rr, amp * math.sin(p), bend * max(0, math.cos(p)))
        items += leg(fr, amp * math.sin(p + math.pi), bend * max(0, math.cos(p + math.pi)))
        items += leg(rl, amp * math.sin(p + math.pi), bend * max(0, math.cos(p + math.pi)))
        items += [(s, X, 1.5 * math.sin(2 * p)) for s in spine] + [(head[0], X, -2 * math.sin(2 * p))]
        return items

    def idle(t):  # agazapado: slow breath, head looks around, tail sways
        p = t * 2 * math.pi
        items = [(s, X, 1.2 * math.sin(p * 2)) for s in spine] + [(head[0], Z, 9 * math.sin(p)), (head[0], X, -3 * math.sin(p * 2))]
        items += tail_wave(t, 10, 1)
        items += leg(fl, -4, 6) + leg(fr, -4, 6) + leg(rl, 4, 8) + leg(rr, 4, 8)
        return combine(arm, items), None

    def walk(t): return combine(arm, gait(t, 18, 22) + tail_wave(t, 8, 1)), None
    def run(t): return combine(arm, gait(t, 30, 38) + tail_wave(t, 6, 2)), None

    def pounce(t):  # embestida: crouch (aviso) -> lunge forward -> recover
        c = pulse(t, 0, .45); l = pulse(t, .38, .78)
        items = leg(fl, 10 * c - 45 * l, 30 * c) + leg(fr, 10 * c - 45 * l, 30 * c)
        items += leg(rl, -15 * c + 35 * l, 40 * c) + leg(rr, -15 * c + 35 * l, 40 * c)
        items += [(s, X, 6 * c - 6 * l) for s in spine] + [(head[0], X, 10 * c - 18 * l)] + tail_wave(t, 15, 1)
        loc = {"tripo::0_Left_Limb_0": Vector((0, 0, -.06 * c + .08 * l))}
        return combine(arm, items), loc

    def roar(t):  # aparición / rugido: head up, chest out
        r = pulse(t, .1, .95)
        items = [(head[0], X, -25 * r), (head[1], X, -10 * r)] + [(s, X, -5 * r) for s in spine]
        items += leg(fl, -8 * r, 0) + leg(fr, -8 * r, 0) + tail_wave(t, 20 * r + 4, 2)
        return combine(arm, items), None

    def hit(t):
        h = pulse(t, 0, 1)
        items = [(s, X, -8 * h) for s in spine] + [(head[0], X, -15 * h), (head[0], Z, 12 * h)] + tail_wave(t, 10 * h, 1)
        return combine(arm, items), None

    for name, length, fn in [("Idle", 90, idle), ("Caminar", 30, walk), ("Correr", 18, run), ("Embestida", 36, pounce),
                             ("Rugido", 45, roar), ("Golpe", 18, hit)]:
        Clip(arm, name, length).sample(fn)
    export(arm, folder, "Jaguar")


# ---------------------------------------------------------------- guacamaya
def macaw():
    folder = "Guacamaya+(Transformación)"
    arm = load(folder)
    right = ["bone_14", "bone_15", "bone_16", "bone_17"]
    left = ["bone_23", "bone_24", "bone_25", "bone_26"]
    legs = ["tripo::0_Left_Limb_0", "tripo::0_Right_Limb_0"]
    shins = ["tripo::0_Left_Limb_1", "tripo::0_Right_Limb_1"]
    tail = ["bone_3", "tripo::Tail_0"]
    head = "tripo::Head_0"

    def wings(down, sweep=0, lag=0, outer=1.0):
        # down: + lowers both wings (the right one turns about +Y, the left one mirrored); sweep folds them back.
        items = []
        for i, (r, l) in enumerate(zip(right, left)):
            d = down if i == 0 else lag * (outer if i > 1 else .6)
            items += [(r, Y, d), (l, Y, -d), (r, Z, sweep if i == 0 else 0), (l, Z, -sweep if i == 0 else 0)]
        return items

    folded = lambda: wings(78, 40, 25)
    tucked = lambda k: [(b, X, 60 * k) for b in legs] + [(b, X, -70 * k) for b in shins]

    def idle(t):  # on the ground: wings folded, head bobs and turns, tail sways
        p = t * 2 * math.pi
        items = folded() + [(head, X, 8 * max(0, math.sin(p * 3))), (head, Z, 14 * math.sin(p))]
        items += [(b, Z, 6 * math.sin(p * 2)) for b in tail]
        return combine(arm, items), None

    def flap(t, cycles, amp, bias):
        p = t * 2 * math.pi * cycles
        return wings(bias + amp * math.sin(p), 0, amp * .5 * math.sin(p - 1.0))

    def fly(t):
        items = flap(t, 2, 48, 5) + tucked(1) + [(b, X, -10) for b in tail] + [(head, X, -8)]
        return combine(arm, items), {"tripo::Root": Vector((0, 0, .03 * math.sin(t * 4 * math.pi + 1.5)))}

    def glide(t):
        p = t * 2 * math.pi
        items = wings(8 + 4 * math.sin(p), 0, 3 * math.sin(p)) + tucked(1) + [(b, X, -12) for b in tail] + [(head, X, -10)]
        return combine(arm, items), None

    def takeoff(t):  # folded -> wings up -> strong downbeats, legs tuck
        open_ = ramp(t, 0, .3)
        beat = flap(t, 2.5, 55 * ramp(t, .2, .4), 0)
        items = [(b, a, d * (1 - open_)) for b, a, d in folded()] + [(b, a, d * open_) for b, a, d in beat]
        items += tucked(ramp(t, .4, .9)) + [(head, X, -10 * open_)]
        return combine(arm, items), {"tripo::Root": Vector((0, 0, .25 * ramp(t, .3, 1)))}

    def land(t):  # beats slow down, legs reach, wings fold
        fold = ramp(t, .55, 1)
        beat = flap(t, 2, 50 * (1 - ramp(t, .3, .7)), 0)
        items = [(b, a, d * (1 - fold)) for b, a, d in beat] + [(b, a, d * fold) for b, a, d in folded()]
        items += tucked(1 - ramp(t, .2, .6)) + [(head, X, 10 * pulse(t, .6, 1))]
        return combine(arm, items), {"tripo::Root": Vector((0, 0, .25 * (1 - ramp(t, 0, .7))))}

    for name, length, fn in [("Idle", 90, idle), ("Despegue", 36, takeoff), ("Vuelo", 30, fly),
                             ("Planeo", 60, glide), ("Aterrizaje", 36, land)]:
        Clip(arm, name, length).sample(fn)
    export(arm, folder, "Guacamaya")


# ---------------------------------------------------------------- mujer-cóndor (decimated copy)
def condor():
    folder = "Mujer+condor"
    out = os.path.join(ROOT, folder, "Mujer_condor_Optimizado.fbx")
    if os.path.exists(out):
        print("CREATURE_SKIP", out); return
    arm = load(folder)
    for ob in [o for o in bpy.data.objects if o.type == "MESH"]:
        tris = sum(len(p.vertices) - 2 for p in ob.data.polygons)
        mod = ob.modifiers.new("Decimate", "DECIMATE")
        mod.ratio = min(1.0, 40000 / max(1, tris))
        bpy.context.view_layer.objects.active = ob
        bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={"ARMATURE", "MESH"},
                             apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
                             add_leaf_bones=False, use_armature_deform_only=False, bake_anim=False,
                             path_mode="AUTO", embed_textures=False)
    print("CREATURE_OK", out)


ONLY = set(args[1:])
for key, fn in [("serpiente", serpent), ("jaguar", jaguar), ("guacamaya", macaw), ("condor", condor)]:
    if not ONLY or key in ONLY:
        try:
            fn()
        except Exception as e:
            import traceback; traceback.print_exc(); print("CREATURE_FAILED", key, e)
