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
    """Quadruped clips made with leg IK and baked to the bones: the paws plant on the ground and
    the body carries the weight (2026-10-06 playtest: the first jaguar read stiff and unclear)."""
    folder = "Jaguar+(Transformación)"
    arm = load(folder)
    spine = ["tripo::Spine_0", "tripo::Spine_1", "tripo::Spine_2"]
    head = ["tripo::Head_0", "tripo::Head_1"]
    tail = ["bone_30", "bone_31", "bone_32", "bone_33", "bone_34"]
    root = "tripo::Root"
    # Leg: IK on the ankle bone (its tail touches the ground), chain of three.
    legs = {"FR": "bone_12", "FL": "bone_18", "RR": "bone_23", "RL": "tripo::0_Left_Limb_4"}
    front = ("FR", "FL")
    rig = QuadrupedRig(arm, legs, root)

    def tail_wave(t, amp, cycles=1, droop=0, lag=.9):
        out = [(b, X, droop * (i + 1) / len(tail)) for i, b in enumerate(tail)]
        out += [(b, Z, amp * (.5 + .5 * i / len(tail)) * math.sin(t * 2 * math.pi * cycles - i * lag)) for i, b in enumerate(tail)]
        return out

    def stride(u, duty, length, lift):
        """Paw offset along one step cycle (u 0..1): stance slides back under the body, swing arcs forward."""
        u %= 1.0
        if u < duty:
            k = u / duty
            return Vector((0, -length / 2 + length * k, 0))
        s = (u - duty) / (1 - duty)
        e = ease(s)
        return Vector((0, length / 2 - length * e, lift * math.sin(math.pi * s) ** .8))

    # ------------------------------------------------------------ gaits
    def gait(t, phases, duty, length, lift, bob, flex, sway, tail_amp, tail_cycles, head_nod):
        feet = {leg: stride(t + ph, duty, length * (1.0 if leg in front else .9), lift) for leg, ph in phases.items()}
        p = t * 2 * math.pi
        body = Vector((sway * .004 * math.sin(p), 0, bob * math.cos(2 * p)))
        pitch = flex * .25 * math.sin(2 * p)
        fk = [(s, X, flex * math.sin(2 * p + .6 * i)) for i, s in enumerate(spine)]
        fk += [(s, Z, sway * math.sin(p + .5 * i)) for i, s in enumerate(spine)]
        fk += [(head[0], X, -head_nod * math.sin(2 * p + 1.2) - flex * .8 * math.sin(2 * p)), (head[0], Z, -sway * .8 * math.sin(p))]
        fk += tail_wave(t, tail_amp, tail_cycles, droop=-6)
        return feet, body, pitch, 0, fk

    walk = lambda t: gait(t, {"RL": 0, "FL": .25, "RR": .5, "FR": .75}, .64, .26, .07, .010, 1.6, 3.0, 9, 1, 2.5)
    run = lambda t: gait(t, {"RL": 0, "RR": .1, "FL": .48, "FR": .58}, .38, .46, .12, .035, 7.0, 1.5, 6, 2, 5)

    # ------------------------------------------------------------ in place
    def idle(t):
        p = t * 2 * math.pi
        breath = math.sin(p * 3)
        # Weight shifts from one side to the other; the head looks around and pauses.
        shift = math.sin(p)
        look = math.sin(p) * ramp(abs(math.sin(p)), .2, .7)
        body = Vector((.008 * shift, 0, .004 * breath - .004))
        fk = [(s, X, 1.4 * breath * (i + 1) / 3) for i, s in enumerate(spine)]
        fk += [(head[0], Z, 22 * look), (head[1], Z, 8 * look), (head[0], X, -3 + 4 * math.sin(p * 2 + 1))]
        fk += tail_wave(t, 14, 1, droop=-10, lag=1.1) + [(tail[-1], X, 18 * pulse(t, .55, .7))]
        return {}, body, 0, 1.5 * shift, fk

    def roar(t):
        # Draws in (head low, chest down), then rears and roars (head high, chest up, tail lashing).
        a = pulse(t, 0, .38); r = ramp(t, .3, .45) * (1 - ramp(t, .78, 1.0)); shake = math.sin(t * 2 * math.pi * 6) * r
        body = Vector((0, .02 * a + .05 * r, -.03 * a + .025 * r))
        fk = [(s, X, 4 * a - 7 * r) for s in spine]
        fk += [(head[0], X, 16 * a - 34 * r + 3 * shake), (head[1], X, 8 * a - 14 * r), (head[0], Z, 4 * shake)]
        fk += tail_wave(t, 10 + 26 * r, 3, droop=-4 + 14 * r)
        feet = {"FR": Vector((0, .01 * r, 0)), "FL": Vector((0, .01 * r, 0))}
        return feet, body, -6 * r + 4 * a, 0, fk

    def pounce(t):
        # Crouch (the warning) -> spring -> flight with the forepaws reaching -> landing -> recover.
        c = ramp(t, 0, .3) * (1 - ramp(t, .33, .42))          # crouch
        fly = pulse(t, .35, .68)                                 # in the air
        land = pulse(t, .6, .86)                                 # impact
        go = ramp(t, .34, .62) * (1 - ramp(t, .82, 1.0))        # body carried forward
        body = Vector((0, .05 * c - .2 * go, -.075 * c + .13 * fly - .05 * land))
        pitch = 6 * c - 3 * fly + 4 * land
        reach = ramp(t, .36, .55) * (1 - ramp(t, .62, .7))
        feet = {}
        for leg in ("FR", "FL"):
            feet[leg] = Vector((0, -.24 * go - .12 * reach, .09 * fly))
        for leg in ("RR", "RL"):
            push = pulse(t, .32, .5)
            feet[leg] = Vector((0, .06 * push - .2 * ramp(t, .46, .7) * (1 - ramp(t, .82, 1.0)), .1 * pulse(t, .44, .72)))
        fk = [(s, X, 6 * c - 10 * fly + 8 * land) for s in spine]
        fk += [(head[0], X, 10 * c - 6 * fly + 10 * land), (head[1], X, 4 * c)]
        fk += tail_wave(t, 8, 1, droop=-14 * c + 18 * fly)
        return feet, body, pitch, 0, fk

    def hit(t):
        h = pulse(t, 0, .7); w = math.sin(t * 2 * math.pi * 3) * pulse(t, 0, 1)
        body = Vector((.025 * h, .05 * h, -.02 * h))
        fk = [(s, X, -6 * h) for s in spine] + [(s, Z, 8 * h) for s in spine]
        fk += [(head[0], X, -18 * h), (head[0], Z, 20 * h + 4 * w)] + tail_wave(t, 16 * h, 2, droop=10 * h)
        feet = {"FR": Vector((0, .04 * h, 0)), "FL": Vector((0, .05 * h, 0))}
        return feet, body, -5 * h, 9 * h, fk

    for name, length, fn, loop in [("Idle", 120, idle, True), ("Caminar", 32, walk, True), ("Correr", 16, run, True),
                                   ("Embestida", 40, pounce, False), ("Rugido", 44, roar, False), ("Golpe", 18, hit, False)]:
        rig.bake(name, length, fn, loop)
    rig.finish()
    export(arm, folder, "Jaguar")


class QuadrupedRig:
    """IK targets for four paws plus a body pose on the root bone; each clip is sampled on the
    targets and baked to plain bone keys (visual keying), then the constraints go away."""

    def __init__(self, arm, legs, root):
        self.arm, self.legs, self.root = arm, legs, root
        bpy.context.view_layer.objects.active = arm
        bpy.ops.object.mode_set(mode="POSE")
        self.rest = {}
        self.targets = {}
        for key, bone in legs.items():
            tip = arm.matrix_world @ arm.data.bones[bone].tail_local
            empty = bpy.data.objects.new("IK_" + key, None)
            bpy.context.scene.collection.objects.link(empty)
            empty.location = tip
            c = arm.pose.bones[bone].constraints.new("IK")
            c.target = empty; c.chain_count = 3; c.use_tail = True; c.use_stretch = False
            self.rest[key] = tip.copy(); self.targets[key] = empty
        for b in arm.pose.bones: b.rotation_mode = "QUATERNION"
        self.root_rest = arm.data.bones[root].matrix_local.to_3x3()

    def pose(self, feet, body, pitch, roll, fk):
        arm = self.arm
        for b in arm.pose.bones:
            b.rotation_quaternion = Quaternion(); b.location = Vector()
        # Body: offset (world) and pitch/roll about the root.
        # The tilt turns about the middle of the body, not the root at the feet.
        pivot = arm.data.bones[self.root].head_local
        centre = Vector((0, .05, .48))
        world = Quaternion(X, math.radians(pitch)) @ Quaternion(Y, math.radians(roll))
        body = body - ((world @ (centre - pivot)) - (centre - pivot))
        pb = arm.pose.bones[self.root]
        pb.location = self.root_rest.inverted() @ body
        pb.rotation_quaternion = turn(arm, self.root, X, pitch) @ turn(arm, self.root, Y, roll)
        for bone, rot in combine(arm, fk).items():
            arm.pose.bones[bone].rotation_quaternion = rot @ arm.pose.bones[bone].rotation_quaternion
        for key, empty in self.targets.items():
            empty.location = self.rest[key] + feet.get(key, Vector())

    def bake(self, name, length, fn, loop):
        arm = self.arm
        # Sample the targets and the body into a scratch action, then bake what the IK makes of it.
        scratch = bpy.data.actions.new("_scratch_" + name)
        arm.animation_data_create(); arm.animation_data.action = scratch
        for empty in self.targets.values():
            empty.animation_data_create(); empty.animation_data.action = bpy.data.actions.new("_scratch_" + empty.name)
        for f in range(0, length + 1):
            t = (f % length) / length if loop else f / length
            self.pose(*fn(t))
            for b in arm.pose.bones:
                b.keyframe_insert("rotation_quaternion", frame=f); b.keyframe_insert("location", frame=f)
            for empty in self.targets.values(): empty.keyframe_insert("location", frame=f)
        bpy.context.scene.frame_start, bpy.context.scene.frame_end = 0, length
        for o in bpy.context.scene.objects: o.select_set(False)
        arm.select_set(True); bpy.context.view_layer.objects.active = arm
        if arm.mode != "POSE": bpy.ops.object.mode_set(mode="POSE")
        # only_selected=False: every bone is baked, whatever is selected.
        bpy.ops.nla.bake(frame_start=0, frame_end=length, step=1, only_selected=False, visual_keying=True,
                         clear_constraints=False, clear_parents=False, use_current_action=False, bake_types={"POSE"})
        baked = arm.animation_data.action
        baked.name = name; baked.use_fake_user = True
        for empty in self.targets.values():
            a = empty.animation_data.action; empty.animation_data.action = None; bpy.data.actions.remove(a)
        bpy.data.actions.remove(scratch)
        print("JAGUAR_BAKED", name, length)

    def finish(self):
        arm = self.arm
        for bone in self.legs.values():
            for c in list(arm.pose.bones[bone].constraints): arm.pose.bones[bone].constraints.remove(c)
        for empty in self.targets.values(): bpy.data.objects.remove(empty)
        bpy.ops.object.mode_set(mode="OBJECT")


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
