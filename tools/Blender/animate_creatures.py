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


def one_head_per_piece(chain_a, chain_b, largest=1000):
    """Each small loose piece of the mesh (a tongue, a fang) follows one head only: weights it had
    on the other head's bones are dropped and the rest renormalised. Tripo had tied part of head B's
    tongue to head A's jaw, so it stretched across to the other head when they parted (2026-10-07)."""
    import bmesh
    for ob in [o for o in bpy.data.objects if o.type == "MESH"]:
        names = {g.index: g.name for g in ob.vertex_groups}
        bm = bmesh.new(); bm.from_mesh(ob.data); bm.verts.ensure_lookup_table()
        seen, islands = set(), []
        for v in bm.verts:
            if v.index in seen: continue
            stack, comp = [v], []
            seen.add(v.index)
            while stack:
                a = stack.pop(); comp.append(a.index)
                for e in a.link_edges:
                    o = e.other_vert(a)
                    if o.index not in seen: seen.add(o.index); stack.append(o)
            islands.append(comp)
        bm.free()
        verts = ob.data.vertices
        fixed = 0
        for comp in islands:
            if len(comp) > largest: continue
            wa = sum(g.weight for i in comp for g in verts[i].groups if names[g.group] in chain_a)
            wb = sum(g.weight for i in comp for g in verts[i].groups if names[g.group] in chain_b)
            if wa == wb: continue
            own, other = (chain_a, chain_b) if wa > wb else (chain_b, chain_a)
            # The bone of its own head that carries most of the piece takes the parts tied only
            # to the other head.
            totals = {}
            for i in comp:
                for g in verts[i].groups:
                    if names[g.group] in own: totals[names[g.group]] = totals.get(names[g.group], 0) + g.weight
            carrier = max(totals, key=totals.get)
            for i in comp:
                groups = [(names[g.group], g.weight) for g in verts[i].groups if g.weight > 0]
                keep = [(n, w) for n, w in groups if n not in other]
                if len(keep) == len(groups): continue
                if not keep: keep = [(carrier, 1.0)]
                total = sum(w for _, w in keep)
                for n, _ in groups: ob.vertex_groups[n].remove([i])
                for n, w in keep: ob.vertex_groups[n].add([i], w / total, "REPLACE")
                fixed += 1
        print("ONE_HEAD", ob.name, "islands", len(islands), "fixed verts", fixed)


# ---------------------------------------------------------------- serpiente bicéfala
def serpent():
    """Two necks moved as chains: every motion travels from the base to the head a little later
    in each bone (overlap), the heads keep their gaze while the neck sways, and each head stays
    on its own side so they never cross (2026-10-07: the first clips turned the necks as rigid
    pieces and the heads passed through each other). Strikes cock the neck into an S, hold
    there (the duel's warning stops at 32 %) and whip out base-first."""
    folder = "Serpiente+Bicéfala"
    arm = load(folder)
    A = ["tripo::Head_0", "tripo::Head_1", "tripo::Head_2", "tripo::Head_3"]
    head_a = "tripo::Head_4"
    B = ["bone_17", "bone_18", "bone_19", "bone_20"]
    C = ["tripo::Tail_%d" % i for i in range(8)]
    jaw, root = "bone_6", "tripo::Root"
    root_rest = arm.data.bones[root].matrix_local.to_3x3().inverted()
    # Head A sits on the right (+X) and head B on the left (-X): each sways around its own side.
    SIDE_A, SIDE_B = 6, -7

    def chain(bones, fwd, side):
        """fwd(i), side(i) in degrees per bone; +fwd leans toward the front (-Y), +side toward +X."""
        out = []
        for i, b in enumerate(bones):
            out += [(b, X, fwd(i)), (b, Y, side(i))]
        return out

    def steady(bones, head, fwd, side, keep=.65):
        """The head turns back part of what the neck below it turned, so its gaze stays."""
        f = sum(fwd(i) for i in range(len(bones))); s = sum(side(i) for i in range(len(bones)))
        return [(head, X, -keep * f), (head, Y, -keep * s)] if head else []

    def lag(t, i, d=.035):
        return t - i * d

    def coils(t, tight=0.0, cycles=1, amp=1.5):
        return [(c, X, -tight * (1 - i / len(C)) + amp * math.sin(t * 2 * math.pi * cycles + i * .7)) for i, c in enumerate(C)]

    def body(turn=0.0, lean=0.0, lift=0.0, slide=0.0):
        items = [(root, Z, turn), (root, X, lean)]
        loc = {root: root_rest @ Vector((0, -slide, lift))} if lift or slide else None
        return items, loc

    def pose(items, loc=None):
        return combine(arm, items), loc

    W = (.55, .85, 1, .8)  # how much each neck bone takes of a sway

    # ------------------------------------------------------------ loops
    def idle(t):
        p = t * 2 * math.pi
        fa = lambda i: W[i] * 4 * math.sin(2 * p + 1 - i * .7)
        sa = lambda i: W[i] * (SIDE_A + 7 * math.sin(2 * p - i * .8))
        fb = lambda i: W[i] * 4.5 * math.sin(3 * p - i * .7)
        sb = lambda i: W[i] * (SIDE_B + 7 * math.sin(3 * p + 2 - i * .8))
        items = chain(A, fa, sa) + steady(A, head_a, fa, sa) + chain(B, fb, sb)
        # The tongue-flick of the jaw: two quick openings per loop.
        items += [(jaw, X, 3 + 9 * pulse(t, .38, .46) + 7 * pulse(t, .84, .9))] + coils(t)
        b, loc = body(turn=3 * math.sin(p), lift=.006 * math.sin(2 * p))
        return pose(items + b, loc)

    # ------------------------------------------------------------ head A
    def attack_a(t):
        cock = ramp(t, 0, .28) * (1 - ramp(t, .34, .44))
        rec = ramp(t, .62, 1)
        CP, SP = (-22, -16, 12, 26), (30, 26, 16, 6)
        def fa(i):
            s = ramp(lag(t, i, .03), .34, .48) * (1 - rec)
            return cock * CP[i] + s * SP[i] + 8 * pulse(lag(t, i, .04), .5, .78) * i / 3
        sa = lambda i: W[i] * SIDE_A * (1 - .5 * cock)
        away = pulse(t, .2, .95)
        fb = lambda i: -9 * away * W[i]
        sb = lambda i: W[i] * (SIDE_B - 10 * away)
        items = chain(A, fa, sa) + [(head_a, X, -8 * cock)] + chain(B, fb, sb)
        items += [(jaw, X, 10 * cock + 34 * pulse(t, .33, .64))] + coils(t, tight=5 * cock + 3 * pulse(t, .34, .7))
        strike = ramp(t, .34, .46) * (1 - rec)
        b, loc = body(lean=7 * strike - 4 * cock, slide=.05 * strike - .02 * cock)
        return pose(items + b, loc)

    def shake_a(t):
        rise = ramp(t, 0, .28)
        thrash = ramp(t, .3, .4) * (1 - ramp(t, .82, 1))
        def sa(i):
            w = math.sin(2 * math.pi * 4.5 * lag(t, i, .05)) * thrash * (8 + 7 * i)
            return W[i] * SIDE_A + w
        fa = lambda i: -14 * rise * W[i] * (1 - thrash) + 6 * thrash * W[i]
        away = pulse(t, .15, 1)
        items = chain(A, fa, sa) + [(head_a, X, 10 * rise)] + chain(B, lambda i: -7 * away * W[i], lambda i: W[i] * (SIDE_B - 12 * away))
        items += [(jaw, X, 4 + 20 * thrash)] + coils(t, tight=4 * thrash, cycles=3, amp=2.5 * thrash)
        b, loc = body(turn=5 * math.sin(2 * math.pi * 4.5 * t) * thrash)
        return pose(items + b, loc)

    # ------------------------------------------------------------ head B
    def attack_b(t):
        cock = ramp(t, 0, .28) * (1 - ramp(t, .34, .44))
        rec = ramp(t, .7, 1)
        def sb(i):
            sweep = ramp(lag(t, i, .04), .34, .58) * (1 - rec)
            return W[i] * SIDE_B - 34 * cock * W[i] + 52 * sweep * W[i] * (1 - .6 * pulse(t, .55, .8))
        def fb(i):
            return -10 * cock * W[i] + 32 * ramp(lag(t, i, .03), .34, .5) * (1 - rec) * W[i]
        duck = pulse(t, .3, .85)  # head A rears up and out of the sweep
        fa = lambda i: -20 * duck * W[i]
        sa = lambda i: W[i] * (SIDE_A + 14 * duck)
        items = chain(B, fb, sb) + chain(A, fa, sa) + steady(A, head_a, fa, sa, .4)
        items += [(jaw, X, 3 + 6 * duck)] + coils(t, tight=4 * cock)
        b, loc = body(turn=-6 * cock + 10 * pulse(t, .36, .8), lean=4 * pulse(t, .36, .7))
        return pose(items + b, loc)

    def pulse_b(t):
        rise = ramp(t, 0, .3) * (1 - ramp(t, .48, .58))
        push = pulse(t, .5, .9)
        tremble = math.sin(2 * math.pi * 9 * t) * pulse(t, .15, .5)
        fb = lambda i: (-24 if i < 2 else -8) * rise + 3 * tremble + 30 * W[i] * ramp(lag(t, i, .03), .5, .6) * (1 - ramp(t, .75, 1))
        sb = lambda i: W[i] * (SIDE_B - 4 * rise)
        away = pulse(t, .1, 1)
        fa = lambda i: -6 * away * W[i]
        sa = lambda i: W[i] * (SIDE_A + 8 * away)
        items = chain(B, fb, sb) + chain(A, fa, sa) + steady(A, head_a, fa, sa, .5)
        items += [(jaw, X, 3)] + coils(t, tight=6 * rise - 3 * push, cycles=4, amp=1.2 * rise)
        b, loc = body(lean=-4 * rise + 6 * push, lift=.02 * rise, slide=.03 * push)
        return pose(items + b, loc)

    # ------------------------------------------------------------ hits
    def hit(first):
        def fn(t):
            def recoil(i): return pulse(lag(t, i, .05), 0, .75)
            f = lambda i: -26 * recoil(i) * W[i]
            out = 16 if first else -16  # thrown outward, away from the other head
            s = lambda i: W[i] * ((SIDE_A if first else SIDE_B) + out * recoil(i))
            other_f = lambda i: -5 * pulse(t, .1, .8) * W[i]
            other_s = lambda i: W[i] * ((SIDE_B if first else SIDE_A) + (-6 if first else 6) * pulse(t, .1, .8))
            if first:
                items = chain(A, f, s) + [(head_a, X, 14 * recoil(4))] + chain(B, other_f, other_s) + [(jaw, X, 18 * pulse(t, 0, .6))]
            else:
                items = chain(B, f, s) + chain(A, other_f, other_s) + steady(A, head_a, other_f, other_s, .4) + [(jaw, X, 6)]
            items += coils(t, tight=4 * pulse(t, 0, .6))
            b, loc = body(turn=(-5 if first else 5) * pulse(t, 0, .7), lean=-4 * pulse(t, 0, .6), slide=-.025 * pulse(t, 0, .6))
            return pose(items + b, loc)
        return fn

    # ------------------------------------------------------------ release
    RF = (34, 30, 22, 10)

    def lowered(k, breath=0.0):
        fa = lambda i: RF[i] * k * (1 + breath)
        sa = lambda i: W[i] * (SIDE_A + 8 * k)
        fb = lambda i: RF[i] * k * (1 + breath)
        sb = lambda i: W[i] * (SIDE_B - 8 * k)
        return chain(A, fa, sa) + [(head_a, X, -12 * k)] + chain(B, fb, sb)

    def released(t):
        # The necks sink one bone after the other, sway once as they settle, and the jaw closes.
        k = sum(ramp(lag(t, i, .06), 0, .75) for i in range(4)) / 4
        settle = math.sin(2 * math.pi * 1.5 * t) * pulse(t, .3, 1) * .08
        items = lowered(k, settle) + [(jaw, X, 12 * pulse(t, 0, .6))] + [(c, X, -3 * k) for c in C]
        b, loc = body(lift=-.02 * k)
        return pose(items + b, loc)

    def rest(t):
        breath = .05 * math.sin(t * 2 * math.pi)
        items = lowered(1, breath) + [(c, X, -3 + .8 * math.sin(t * 2 * math.pi + i * .5)) for i, c in enumerate(C)]
        b, loc = body(lift=-.02)
        return pose(items + b, loc)

    def emerge(t):
        # From coiled low, the body unwinds and the necks rise base-first, swaying as they come up.
        def k(i): return 1 - ramp(lag(t, i, .07), .05, .85)
        sway = lambda i: 10 * math.sin(2 * math.pi * 1.5 * t - i * .9) * (1 - ramp(t, .6, 1))
        fa = lambda i: 70 * k(i) * W[i]
        sa = lambda i: W[i] * SIDE_A + sway(i)
        fb = lambda i: 70 * k(i) * W[i]
        sb = lambda i: W[i] * SIDE_B - sway(i)
        items = chain(A, fa, sa) + chain(B, fb, sb) + [(jaw, X, 22 * pulse(t, .7, .95))]
        items += [(c, X, -8 * (1 - ramp(t, 0, .7))) for c in C]
        b, loc = body(turn=-20 * (1 - ramp(t, 0, .8)), lift=-.04 * (1 - ramp(t, 0, .6)))
        return pose(items + b, loc)

    for name, length, fn in [("Idle", 90, idle), ("AtaqueA", 42, attack_a), ("SacudidaA", 36, shake_a),
                             ("AtaqueB", 42, attack_b), ("PulsoB", 40, pulse_b), ("GolpeA", 18, hit(True)),
                             ("GolpeB", 18, hit(False)), ("Liberada", 60, released), ("Reposo", 90, rest),
                             ("Emerger", 50, emerge)]:
        Clip(arm, name, length).sample(fn)
    one_head_per_piece(set(A + [head_a, jaw, "tripo::Head_5", "tripo::Head_6"]), set(B))
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
    """A real wingbeat (2026-10-07: the first one swung flat paddles): the downstroke drives with
    the wing open and the tip bending up under the air, the upstroke folds the wrist in and lifts
    it back; the body rises on each downstroke and leans forward to fly, the head stays level,
    the legs tuck. On the ground the wings lie folded along the back and the head moves in quick
    turns and holds, like a parrot's."""
    folder = "Guacamaya+(Transformación)"
    arm = load(folder)
    R = ["bone_14", "bone_15", "bone_16", "bone_17"]
    L = ["bone_23", "bone_24", "bone_25", "bone_26"]
    legs = ["tripo::0_Left_Limb_0", "tripo::0_Right_Limb_0"]
    shins = ["tripo::0_Left_Limb_1", "tripo::0_Right_Limb_1"]
    tail = ["bone_3", "tripo::Tail_0"]
    neck, head, root = "tripo::Spine_0", "tripo::Head_0", "tripo::Root"
    root_rest = arm.data.bones[root].matrix_local.to_3x3().inverted()
    pivot = arm.data.bones[root].head_local.copy()
    centre = Vector((0, -.03, .36))

    def wing(joint, axis, deg):
        """Both wings: the left one mirrors the right (turns about Y and Z change sign)."""
        return [(R[joint], axis, deg), (L[joint], axis, deg if axis == X else -deg)]

    def folded(k=1.0):
        return wing(0, Y, 80 * k) + wing(0, Z, 30 * k) + wing(1, Z, 20 * k)

    def tucked(k):
        return [(b, X, 60 * k) for b in legs] + [(b, X, -70 * k) for b in shins]

    def beat(ph, amp, bias=0.0):
        """One wingbeat at phase ph (0 top, .5 bottom): shoulder sweep, wrist fold on the way up,
        tip bent up by the air on the way down, the feathers twisting with the stroke."""
        s = math.sin(ph * 2 * math.pi)
        down = bias - amp * math.cos(ph * 2 * math.pi)
        fold = max(0.0, -s)  # upstroke
        push = max(0.0, s)   # downstroke
        items = wing(0, Y, down) + wing(0, X, 9 * s)
        items += wing(1, Z, 42 * fold) + wing(1, Y, -14 * fold) + wing(2, Z, -36 * fold)
        items += wing(3, Y, -14 * push + 6 * fold)
        return items, s

    def body(pitch=0.0, roll=0.0, lift=0.0, fwd=0.0):
        """Pitch/roll about the middle of the body (not the feet), plus a world offset."""
        q = Quaternion(X, math.radians(pitch)) @ Quaternion(Y, math.radians(roll))
        off = Vector((0, -fwd, lift)) - ((q @ (centre - pivot)) - (centre - pivot))
        return [(root, X, pitch), (root, Y, roll)], {root: root_rest @ off}

    def level(pitch):
        """The neck and head take back most of the body's lean, so the bird looks ahead."""
        return [(neck, X, -.45 * pitch), (head, X, -.35 * pitch)]

    def snap(t, keys):
        """Held values that change quickly: keys = [(time, value)], each change takes .05."""
        v = keys[0][1]
        for (t0, a), (t1, b) in zip(keys, keys[1:]):
            if t >= t1 - .05: v = b
            if t1 - .05 <= t < t1: v = a + (b - a) * ease((t - (t1 - .05)) / .05)
        return v

    FLY = 38  # forward lean in flight

    def idle(t):
        p = t * 2 * math.pi
        turn = snap(t, [(0, 0), (.12, 28), (.3, 28), (.34, -12), (.55, -12), (.6, 14), (.78, 14), (.82, 0), (1, 0)])
        tilt = snap(t, [(0, 0), (.12, 12), (.3, 12), (.34, -4), (.55, -4), (.6, 0), (1, 0)])
        ruffle = pulse(t, .4, .52)
        items = folded(1 - .35 * ruffle) + [(head, Z, turn), (head, Y, tilt), (neck, Z, .3 * turn)]
        items += [(neck, X, 3 * math.sin(p * 3)), (head, X, 10 * pulse(t, .64, .72))]
        items += [(b, X, -12 * pulse(t, .86, .94)) for b in tail] + [(b, Z, 4 * math.sin(p * 2)) for b in tail]
        b, loc = body(roll=2.5 * math.sin(p), lift=.003 * math.sin(p * 3))
        return combine(arm, items + b), loc

    def fly(t):
        ph = (t * 2) % 1.0
        w, s = beat(ph, 52, 2)
        items = w + tucked(1) + level(FLY) + [(b, X, -14) for b in tail] + [(b, Z, 3 * s) for b in tail]
        b, loc = body(pitch=FLY + 3 * s, lift=.03 * s)
        return combine(arm, items + b), loc

    def glide(t):
        p = t * 2 * math.pi
        items = wing(0, Y, -7 + 3 * math.sin(p)) + wing(3, Y, -6 + 2 * math.sin(p * 3))
        items += tucked(1) + level(FLY - 6) + [(b, X, -16) for b in tail] + [(b, Z, 5 * math.sin(p)) for b in tail]
        b, loc = body(pitch=FLY - 6, roll=6 * math.sin(p), lift=.01 * math.sin(p))
        return combine(arm, items + b), loc

    def takeoff(t):
        # Crouch and open (to .3), spring with strong beats as the body leans forward and leaves the ground.
        crouch = pulse(t, 0, .34)
        open_ = ramp(t, .08, .3)
        power = ramp(t, .22, .4)
        w, s = beat((t * 2.5 + .5) % 1.0, 58 * power, 0)
        items = [(bn, a, d * (1 - open_)) for bn, a, d in folded()] + [(bn, a, d * open_) for bn, a, d in w]
        items += tucked(ramp(t, .45, .9)) + level(FLY * ramp(t, .3, .8)) + [(b, X, -14 * open_) for b in tail]
        b, loc = body(pitch=FLY * ramp(t, .3, .8) + 6 * crouch, lift=-.03 * crouch + .3 * ramp(t, .3, 1) + .03 * s * power)
        return combine(arm, items + b), loc

    def land(t):
        # Beats slow and brake with the body rising upright (the flare), legs reach, wings fold.
        fold = ramp(t, .62, 1)
        w, s = beat((t * 2) % 1.0, 50 * (1 - ramp(t, .35, .65)), -8 * pulse(t, .3, .7))
        items = [(bn, a, d * (1 - fold)) for bn, a, d in w] + [(bn, a, d * fold) for bn, a, d in folded()]
        flare = FLY * (1 - ramp(t, 0, .45)) - 14 * pulse(t, .3, .75)
        items += tucked(1 - ramp(t, .15, .5)) + level(flare) + [(b, X, -18 * pulse(t, .3, .8)) for b in tail]
        b, loc = body(pitch=flare, lift=.3 * (1 - ramp(t, 0, .62)) - .025 * pulse(t, .6, .85))
        return combine(arm, items + b), loc

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
