# Movement, Pickup, Inspection, Hand Tracking & Combat

Spec of everything built in the `Assets/Prototype` sandbox so far: 3D movement, camera,
ladders, ramps, the reusable pickup/inspection flow, the dash ability, MediaPipe-based
hand-gesture control of the inspection camera, and the health/combat system (dash damage,
3 enemy types, player/enemy health bars, respawn). Written for a future session to pick up
context quickly — check the scene/scripts against this if anything drifted.

Scene: `Assets/Prototype/Scenes/Movement.unity`
Scripts: `Assets/Prototype/Scripts/`
Prefabs: `Assets/Prototype/Prefabs/`
Materials: `Assets/Prototype/Materials/`
Setup/run instructions for teammates (Spanish): `README.md` at repo root.

Unity 6000.3.21f1, URP, new Input System only (`activeInputHandler: 1` in ProjectSettings —
scripts use `UnityEngine.InputSystem.Keyboard`/`Mouse`, not the legacy `Input` class).

## Player movement — `PlayerController.cs`

On the `Character` GameObject. Uses a `CharacterController` (not Rigidbody) — all motion goes
through `CharacterController.Move()`, called once per `Update()`.

- **A/D** → -X/+X, **W/S** → +Z/-Z, normalized on diagonals.
- **Space** → jump (`Mathf.Sqrt(jumpHeight * -2 * gravity)`), only while grounded.
- Character yaws to face its movement direction (`Quaternion.RotateTowards` toward
  `Quaternion.LookRotation(move)`, `rotationSpeed` = 540°/s). Only yaw — pitch/roll never
  touched, so there's no need for Rigidbody-style rotation constraints.
- Gravity is manual (`gravity = -20`), with a `groundStickSpeed` (15) grounded stick so
  `isGrounded` doesn't flicker. **Bug fixed after initial ship:** this used to be a flat `-2`,
  which was enough on flat ground but too weak to keep the capsule glued to a *descending*
  ramp at normal move speed — the controller would detach from the slope for a frame,
  `isGrounded` would read false right as the player pressed Space, and jump silently did
  nothing while going downhill. Bumped to `15` (comfortably ahead of both `moveSpeed` (5) and
  `dashSpeed` (12) on any walkable slope) and applied in both the normal-movement and
  `UpdateDashMotion()` code paths — dash was equally susceptible since it moves even faster.
- `OnControllerColliderHit` — new callback, added for the shield enemy (see Combat below): if
  `IsDashing` and the hit collider has a `ShieldEnemy`, reports the current dash chain count to
  it. `CharacterController` doesn't fire regular `OnCollisionEnter`, this is the correct
  substitute for "what did I just bump into while moving".

Serialized tunables: `moveSpeed` (5), `jumpHeight` (1.5), `gravity` (-20), `climbSpeed` (4),
`rotationSpeed` (540), `faceAnchor` (Transform), `dashSpeed` (12), `dashDuration` (0.2),
`dashChainWindow` (0.2), `dashDamage` (20), `groundStickSpeed` (15).

Public accessors added for the combat system to read without over-exposing internals:
`IsDashing`, `DashChainCount`, `DashInstanceId` (increments once per dash press/chain-link —
lets a hurtbox tell "a new dash link just happened" apart from "still overlapping the same
one"), `DashDamage`. Plus `Teleport(Vector3)` — disables `CharacterController`, sets
`transform.position`, zeroes vertical velocity and dash state, re-enables — the safe way to
reposition a `CharacterController` (a direct `transform.position` write fights with its
internal collision resolution otherwise). Used by `PlayerRespawn`.

### Ladder climbing

- Ladders are identified by the **`Ladder` tag** (registered in `ProjectSettings/
  TagManager.asset`), checked via `other.CompareTag("Ladder")`. **Changed from an earlier
  `Ladder.cs` empty marker component + `GetComponent<Ladder>()`** — a ponytail-audit pass
  flagged the marker component as reinventing Unity's own tag system for a plain existence
  check; the component/script/`.meta` were deleted and the `Ladder.prefab`'s GameObject tagged
  directly instead. The prefab's `BoxCollider` is still a **trigger** (was solid originally —
  swapped so it doesn't block the player like a wall).
- `PlayerController` tracks `_laddersTouching` (int, not bool — safe under overlapping
  colliders) via `OnTriggerEnter`/`OnTriggerExit`.
- While `IsOnLadder`: gravity is suspended, **W/S climb up/down** (mapped onto world Y at
  `climbSpeed`), **A/D** still shift sideways. Stepping off the sides or climbing past the
  top/bottom of the trigger volume exits the state and normal gravity/jump resumes.
- Entering a ladder trigger mid-dash cancels the dash (`_dashTimeRemaining = 0`) so the two
  systems never fight.

### Ramps

No dedicated ramp prefab — reused the `Platform` prefab (solid `BoxCollider`, non-trigger)
just rotated on its local Z axis. `CharacterController`'s default slope limit is 45°; the two
ramps in the scene use ±27.38°, comfortably inside that, so they're walkable with zero extra
code. Since movement is kinematic (`CharacterController.Move`, not Rigidbody physics), there's
no friction/sliding concern a physics material would normally solve. Only cosmetic caveat: a
tilted box leaves slanted, non-flush ends at the top/bottom of the ramp — a wedge mesh would
fix that if it ever becomes visible, not needed for the prototype.

### Dash ability

Three-tier dash, unlocked progressively by the pickups (see below). Trigger key: **Left Shift**.

- `DashTier` (0–3) on `PlayerController`, `HasDash => DashTier > 0`. `GrantDash(tier)` only
  ever raises it (`if (tier > DashTier) DashTier = tier`) — highest tier collected wins,
  order doesn't matter.
- A dash locks in a direction once, at the moment it starts: current WASD input if any is
  held, else `transform.forward`. The character snaps to face that direction immediately.
  For the dash's duration, this direction fully overrides normal horizontal movement (gravity
  still applies vertically underneath it via `_verticalVelocity`).
- **Chaining** ("accumulate the acceleration of two dashes to go faster"): pressing Shift again
  within `dashChainWindow` (0.2s) of the *previous press* — classic double-tap, independent of
  whether the current dash has finished animating — adds another full `dashSpeed` onto the
  existing dash velocity (additive stacking: a 2-chain peaks at 2× `dashSpeed`, a 3-chain at
  3×) and refreshes the dash timer. Capped at `_dashChainCount < DashTier`.
- A Shift press that arrives outside the chain window, or once the tier's chain cap is already
  hit, is **ignored** while a dash is in progress — you can't interrupt a dash early, only
  chain it (or wait for `_dashTimeRemaining` to hit 0, which resets `_dashChainCount` to 0 and
  makes the next press start a fresh dash).
- Tier meaning, decided with the user up front:
  - **Tier 1** — single dash, no chaining. Once it finishes you can immediately dash again.
  - **Tier 2** — up to 2 chained dashes (one double-tap extends it).
  - **Tier 3** — up to 3 chained dashes (one more chain link than tier 2, same 0.2s rule).
- Disabled entirely on ladders (`IsOnLadder` guard in `HandleDashPress`) and while
  `SetInputLocked(true)` (pickup inspection — see below), which also zeroes any in-flight dash.

## Camera — `CameraFollow.cs`

On `Main Camera`. Fixed lateral 3-D "side view": camera position tracks the `Character`
transform on X, Y, and Z, but **rotation is never touched by this script** — it stays exactly
as placed in the Editor (currently pitched 3.8° down). That's an important asymmetry to
remember: anything that moves the camera for a cutscene/inspection has to restore rotation
manually on the way out, because `CameraFollow` won't do it (see the pickup bug below).

- `_offset` is computed once in `Start()` from the camera's initial position relative to the
  target, then held constant — so whatever relative framing you set up in the Editor is what
  gets preserved as the player moves.
- `followHeight` (bool, default **true**) — when true, follows Y too (needed since there are
  platforms/ladders on the Z *and* Y axes); when false, height is pinned to wherever the
  camera currently sits.
- Movement is `Vector3.SmoothDamp` at `smoothTime` (0.15s), not a hard lock.

## Reusable pickup/inspection system — `InspectablePickup.cs` + `IPickupReward.cs`

Originally a single `DashPickup.cs` script; refactored to be reusable for any future pickup
type (health, key items, lore, etc.), not just the dash ability.

- **`IPickupReward`** — one-method interface: `void Grant(PlayerController player)`.
- **`InspectablePickup.cs`** — kept the *same script GUID* `DashPickup.cs` had (renamed the
  file, not the GUID) so existing prefab references didn't break. Owns 100% of the generic
  flow: trigger detection, the E-to-inspect/confirm state machine, camera blend via
  `FaceAnchor`, mouse/hand-driven object rotation, and the camera snap-back fix. In `Awake()`
  it does `_reward = GetComponent<IPickupReward>()`; `Claim()` calls `_reward?.Grant(_player)`.
  It never references dash-specific anything.
- **`DashPickupReward.cs`** — the only dash-specific piece left: `[Range(1,3)] dashTier` and
  `Grant()` → `player.GrantDash(dashTier)`.

**Pattern for a new pickup type:** write a small `SomethingReward : MonoBehaviour,
IPickupReward`, drop it on a new prefab alongside `InspectablePickup` (and whatever visual you
want instead of `PulsingOrb`, which `InspectablePickup` only reaches for if present) — none of
the inspect/camera/rotate/claim machinery needs to change.

### Visual — `PulsingOrb.cs`

Small sphere (local scale 0.5) with a `Point` `Light` (cyan) and the `DashPowerUp.mat`
material (URP/Lit, dark base color + bright cyan HDR `_EmissionColor` for bloom). Each frame:
bobs `transform.localPosition.y` on a sine wave (`floatAmplitude`/`floatSpeed`), and pulses
both the light's `intensity` and the material's emission color together via
`MaterialPropertyBlock` (no per-instance material duplication). `SetFloating(bool)` pauses
just the bob — used during inspection so the object holds still while it's being rotated,
without stopping the light pulse.

A child GameObject **`TopMarker`** (flattened sphere, local pos `(0, 0.5, 0)`, scale
`(0.55, 0.15, 0.55)`, plain-red non-emissive `DashPowerUpTopMarker.mat`) sits on the sphere's
north pole, purely as a debug aid — it makes object rotation during inspection visually
obvious (no functional role, no script, no collider).

### Interaction range

`SphereCollider` on the pickup is a **trigger**, `m_Radius = 6` with the object's `0.5` scale →
**3-unit world-space interaction radius**. (Started at the default `0.5`/`0.25` world radius,
which required standing almost inside the tiny orb — bumped up so the prompt zone is generous.)

### State machine (`InspectablePickup.Update`)

`State.World → EnteringInspect → Inspecting → (World)`, driven by `Keyboard.current`:

1. **World** — `OnTriggerEnter`/`Exit` (checking `other.GetComponent<PlayerController>()`) just
   track `_playerInRange`; nothing happens until the player presses **E** while in range.
2. **E pressed → `BeginInspect()`**: disables the `Camera.main`'s `CameraFollow` component,
   pauses the orb's float bob, looks up (and caches) `HandGestureTracker`/`HandOverlayUI` if
   not already found, shows the hand overlay, calls `_player.SetInputLocked(true)` (freezes
   `PlayerController.Update` entirely — no movement, no dash, no gravity accumulation), then
   captures the camera's current pose (`_transitionStartPos/Rot`) before moving it anywhere.
3. **EnteringInspect → `BlendIntoInspect()`**: `Lerp`/`Slerp`s the camera from that captured
   pose to a fixed inspect pose over `transitionDuration` (0.4s).
   - **Inspect camera position** = `_player.FaceAnchor.position` — a **child Transform of the
     Character called `Face`** (currently a visible placeholder sphere at local
     `(0, 0.366, 0.221)`; the plan is to swap it for a bare empty GameObject later — nothing in
     the script depends on it having a mesh/collider, only `.position`, so that swap is a
     drop-in change). Falls back to `pickup.position + Vector3.back * inspectDistance` only if
     no anchor is assigned.
   - **Inspect camera rotation** = `Quaternion.LookRotation(pickup.position - cameraPos)` —
     always looks straight at the pickup.
4. **Inspecting**: the camera **holds still** for the rest of this state — it does not orbit.
   Instead, rotation input (mouse, or hands — see below) drives `_yaw`/`_pitch` applied straight
   to `transform.rotation` **on the pickup object itself** (`Quaternion.Euler(_pitch, _yaw, 0)`,
   unclamped). So the player spins the object in place in front of a static, POV-framed camera
   — deliberately not an orbiting third-person camera. From here:
   - **E again → `Claim()`**: `_reward.Grant(_player)`, `EndInspect()`, then
     `gameObject.SetActive(false)`.
   - **Escape → `EndInspect()`** directly: backs out without claiming, pickup stays in the world.
5. **`EndInspect()`**: snaps the camera's position *and rotation* back to the
   `_transitionStartPos`/`_transitionStartRot` captured in step 2, **then** re-enables
   `CameraFollow`, un-pauses the orb float, hides the hand overlay, and calls
   `_player.SetInputLocked(false)`.

   **Why the explicit snap-back exists:** `CameraFollow` only ever writes `transform.position`
   (see above) — it never touches rotation. Originally `EndInspect()` just re-enabled
   `CameraFollow` and let it take over; position recovered fine via `SmoothDamp`, but rotation
   stayed stuck wherever the inspect view last pointed, so gameplay resumed looking the wrong
   way. Since the player is frozen for the whole inspection, the pre-inspect pose is still
   valid when it ends, so a direct snap (no re-blend needed) fixes it cleanly on both the
   `Claim` and `Escape` exit paths.

### Rotation input: mouse vs. hands

`UpdateInspectRotation()` checks `_handTracker != null && _handTracker.IsConnected` (a result
arrived within the last second) — if so, uses hand deltas; otherwise falls back to
`Mouse.current.delta`. Mapping (mirrors the mouse-drag convention: hand-down ≡ mouse-down,
hand-right ≡ mouse-right):

- Right hand (open only) vertical movement → pitch, via `ConsumeRightHandDeltaY()`.
- Left hand (open only) horizontal movement → yaw, via `ConsumeLeftHandDeltaX()`.
- `handRotationSensitivity` (400, much larger than mouse's `rotationSensitivity` 0.3 since hand
  deltas are normalized `[0,1]` image coordinates, not raw pixels).
- `invertVertical`/`invertHorizontal` checkboxes — **added as a safety net because the actual
  sign/mirroring was never verified live in this session** (no webcam access while building
  it). If gestures rotate the object backwards, flip these before touching code.

## Hand tracking — MediaPipe integration

Uses `MediaPipeUnityPlugin` (homuler, MIT license) for real hand landmark detection, feeding
the pickup-inspection rotation above. Full gesture spec from the user: right hand controls
vertical rotation, left hand controls horizontal; closing a hand into a fist freezes that
axis exactly where it is; opening it again resumes with no jump.

### Package install (matters for every teammate, not just this machine)

- **Not installed via a bare git URL** — that only checks out the C# side, no compiled
  binaries (confirmed by inspecting the resulting `Library/PackageCache`: empty `Plugins`
  folders, no installer tool). The correct install is the **pre-built release tarball**:
  `com.github.homuler.mediapipe-0.16.3.tgz` from
  `https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3`, added via
  `Package Manager → + → Add package from tarball...`.
- That tarball bundles native binaries for **Windows (`mediapipe_c.dll`), macOS
  (`libmediapipe_c.dylib`), and Linux (`libmediapipe_c.so`) together** — confirmed by listing
  `Runtime/Plugins/` directly. Unity picks the right one per platform automatically, so **this
  runs on Mac** (Intel and Apple Silicon both listed as supported Editor platforms in the
  plugin's own docs) with zero extra setup — CPU delegate only there (GPU mode isn't supported
  on macOS or Windows; already the default in `HandLandmarkDetectionConfig.cs`). Only Mac-side
  gotcha: Gatekeeper may block the unsigned `.dylib` on first run, needs manual allow.
- Because the install is a local tarball, `Packages/manifest.json` ends up with a
  machine-specific absolute `file:` path — **every teammate has to redo the install step
  themselves**; the resulting manifest diff on their machine is expected, not a bug. Full
  install steps are in `README.md` (Spanish).
- Sample used: `Official Solutions` → `Hand Landmark Detection`, imported via the package's
  Samples tab.
- `NumHands` defaults to `1` in the sample's own `HandLandmarkDetectionConfig.cs` — changed to
  `2` there directly (it's a plain C# property, not `[SerializeField]`, so it doesn't show in
  the Inspector; it's exposed instead via a runtime GUI modal the sample scene builds, which we
  don't otherwise touch).

### Real API (verified by reading the installed package, not guessed)

`Mediapipe.Tasks.Vision.HandLandmarker.HandLandmarkerResult` has three parallel lists:
`handedness: List<Classifications>` (categoryName `"Left"`/`"Right"`), `handLandmarks:
List<NormalizedLandmarks>` (21 points per hand, `x`/`y`/`z` normalized `[0,1]`, image-space —
`0,0` is top-left, Y increases *downward*), and `handWorldLandmarks` (unused here). Landmark
index 9 = middle-finger MCP, used as a stable "palm center" point (steadier than the wrist as
the hand rotates).

### `HandLandmarkerRunner.cs` (sample script, now edited)

Lives at `Assets/Samples/MediaPipe Unity Plugin/0.16.3/Official Solutions/Scenes/Hand Landmark
Detection/HandLandmarkerRunner.cs` — a project-local, editable copy of the sample, not a
read-only package file. Added one surgical hook: `public event Action<HandLandmarkerResult>
ResultUpdated`, invoked in `OnHandLandmarkDetectionOutput` right alongside the existing
`DrawLater` call. Nothing else in that file changed. **Important:** in `LIVE_STREAM` running
mode (what the sample uses) this event fires from a MediaPipe callback thread, not Unity's
main thread — confirmed by how the sample's own
`HandLandmarkerResultAnnotationController.DrawLater`/`SyncNow` handle it (lock + `CloneTo` into
a buffer in the callback; the *entire* read+consume step also happens under that same lock,
later, from `Update()`). `HandGestureTracker` copies this exact pattern.

### `HandGestureTracker.cs` (new)

The gesture logic, thread-safety pattern lifted directly from the sample as above:

- `OnResultUpdated` (possibly background thread): `lock { result.CloneTo(ref _latestResult);
  _isStale = true; }` — no Unity API calls here beyond that.
- `Update()` (main thread): holds the *same* lock for the entire `ProcessResult(_latestResult)`
  call, not just a snapshot-copy — releasing the lock early would let the writer thread mutate
  the same `List<T>` instances the reader is still iterating (`CloneTo` reuses lists via `??
  new List<>()` rather than reallocating). This was a deliberate correctness point, not
  boilerplate.
- **Open/closed heuristic**: for each of the 4 non-thumb fingers, compare wrist-to-tip distance
  vs. wrist-to-PIP distance; extended if tip is farther by `openFingerMargin` (1.2×). Open if
  ≥3 of 4 are extended. Radial-distance-from-wrist rather than a raw Y-coordinate check, so it
  tolerates the hand being rotated in-frame.
- **Delta/freeze logic**: the position reference (`_rightLastPosition`/`_leftLastPosition`)
  updates every frame a hand is visible, **open or closed** — but the delta only accumulates
  into the output (`_rightDeltaYAccum`/`_leftDeltaXAccum`) when open. That's the entire
  freeze/resume mechanism: closing stops rotation dead where it is (delta stops accumulating),
  reopening continues with zero jump (reference never went stale). If a hand disappears from
  frame entirely, its reference resets so reappearing doesn't cause a jump either.
- `ConsumeRightHandDeltaY()`/`ConsumeLeftHandDeltaX()` are consume-and-reset (read-then-zero),
  so they're safe to call once per frame regardless of script execution order relative to
  other components.
- Also exposes `RightHandPoints`/`LeftHandPoints` (`IReadOnlyList<Vector2>`, all 21 normalized
  landmarks per hand, cleared when that hand isn't visible) — used only by `HandOverlayUI`, not
  by the rotation logic.
- `TryConnectToRunner()` is called from `Update()` every frame until it succeeds (not just once
  in `OnEnable`) — deliberate, because the hand-tracking scene loads additively (see below) and
  its load order relative to this component's lifecycle isn't guaranteed.

### `HandTrackingBootstrapper.cs` + scene wiring (why two scenes)

Rebuilding the MediaPipe camera/graph bootstrap from scratch for the gameplay scene was judged
too risky to get right without being able to test it (lots of unexamined supporting
infrastructure: `Bootstrap` prefab, `ImageSourceProvider`, `GpuManager`, `AssetLoader`,
`VisionTaskApiRunner<T>`). Instead, the proven-working sample scene is kept completely intact
and loaded **additively** alongside `Movement.unity`:

- `HandTrackingBootstrapper` (on the `HandTracking` GameObject in `Movement.unity`, alongside
  `HandGestureTracker` and `HandOverlayUI`) calls `SceneManager.LoadScene("Hand Landmark
  Detection", LoadSceneMode.Additive)` in `Awake()` if not already loaded.
- Both `Movement.unity` and the sample scene are registered in `ProjectSettings/
  EditorBuildSettings.asset` — required for `SceneManager.LoadScene` by name to resolve, even
  in Editor Play mode.
- `HandGestureTracker` finds the runner via `FindFirstObjectByType<HandLandmarkerRunner>()`
  (works across loaded scenes, not just the active one), so no manual cross-scene Inspector
  drag is needed.

**Conflicts found and fixed in the sample scene** (`Hand Landmark Detection.unity`), each
scoped as narrowly as possible:
- Its own `Main Camera` was tagged `MainCamera`, colliding with the gameplay camera for
  `Camera.main` lookups (used by `InspectablePickup`/`CameraFollow`) — retagged to `Untagged`.
- It also carried a second `AudioListener`, spamming Unity's "2 audio listeners" warning —
  disabled (`m_Enabled: 0`).
- That same camera was then fully **disabled** (`m_Enabled: 0`) — MediaPipe reads the webcam
  texture directly via `imageSource.GetCurrentTexture()`, not through this camera, so disabling
  it only removes an on-screen debug render, not tracking.
- Its `EventSystem` uses the legacy `StandaloneInputModule` (reads `UnityEngine.Input`), which
  spams `InvalidOperationException` under this project's Input-System-only setting
  (`activeInputHandler: 1`). Not needed for anything (no UI clicks happen in that scene during
  gameplay) — the whole `EventSystem` GameObject is disabled (`m_IsActive: 0`).
- The sample's own debug visualization — a full-screen hand skeleton with connecting bone
  lines, part of the shared `Annotatable Screen` prefab (`Assets/Samples/.../UI/Objects/
  Annotatable Screen.prefab`) — kept rendering regardless of the camera being disabled, because
  its `Canvas` is `Screen Space - Overlay` (renders independent of any camera). Disabling the
  camera never touched it. Fixed with a **scene-only `PrefabInstance` override**
  (`m_IsActive: 0` on the prefab's root GameObject, target fileID
  `3259285889726014651`) rather than editing the shared prefab asset — so no other sample scene
  reusing that prefab is affected.

### `HandOverlayUI.cs` (new) — the "little hands" picture-in-picture

Purpose-built replacement for the sample's own (now-disabled) visualization, built **entirely
at runtime in code** (`Canvas`, panel, 42 dot `Image`s) rather than hand-authored scene UI —
judged too fragile to get right blind via YAML for something this fiddly.

- `RequireComponent(typeof(HandGestureTracker))`, lives on the same `HandTracking` GameObject.
- Bottom-right corner box, `260×200`, `24px` margin, semi-transparent dark background panel.
- 21 dots per hand, blue for left / orange for right (matching the colors already seen in the
  sample and liked by the user), positioned each frame from `HandGestureTracker`'s
  `RightHandPoints`/`LeftHandPoints` (normalized image coords mapped into the viewport's pixel
  rect, Y flipped since UI-space Y is up but image-space Y is down). No bone/connector lines —
  dots only, scope was kept deliberately small.
- Hidden by default (`SetVisible(false)` in `Awake()`); `InspectablePickup` toggles it on in
  `BeginInspect()` and off in `EndInspect()` — only ever visible while actually inspecting
  something, unlike the sample's original full-screen-always overlay.

## Scene placement

Three pickup instances of the one `DashPickup` prefab (source guid
`ac95da82b8c94d579b1d9b52b84156be`), tier set via a `dashTier` override per `PrefabInstance` —
**target `fileID: 109`**, the `DashPickupReward` component (not `107`, which is now
`InspectablePickup` and no longer has a `dashTier` field, after the reusability refactor):

| Tier | Name in Hierarchy | Position |
|---|---|---|
| 1 | Dash Pickup (Tier 1) | `(1.69, 2.51, -6.65)` — near spawn |
| 2 | Dash Pickup (Tier 2) | `(16.52, 6.3, 7.29)` — above the platform reached by the ladder |
| 3 | Dash Pickup (Tier 3) | `(51.57, 2.59, 7.27)` — far platform, end of the current layout |

`PlayerController.faceAnchor` on the `Character` is wired to the `Face` child transform.

The `HandTracking` GameObject (root of `Movement.unity`) carries `HandTrackingBootstrapper` +
`HandGestureTracker` + `HandOverlayUI`.

## Combat system — health, dash damage, 3 enemy types

Design locked in with the user before implementation (see `Health.cs`, `DashHurtbox.cs`,
`PlayerController.cs` additions above, `ArcherEnemy.cs`, `MeleeEnemy.cs`, `ShieldEnemy.cs`):
dash **passes through** enemies (one dash can hit several in its path, motion/duration
unaffected by connecting); pushable-object damage is **deferred**, not part of this pass;
the shield enemy requires a full **Tier-3 chain** (3 dashes combo'd back-to-back, not 3
unrelated dashes over time) to break; enemies are destroyed outright on death (no
drops/respawn), the player respawns at the scene's starting position with health refilled.

### `Health.cs` (reusable, generic)

Same composition philosophy as `IPickupReward` — one component, attached to both the player
and every enemy, nothing enemy/player-specific baked in. `maxHealth`, `TakeDamage(float)`,
`Heal(float)`, `Revive()` (full heal + clears dead flag), `IsDead`, `HealthChanged` event (for
bar UI), `Died` event (for death/respawn handlers). Has its own `invulnerabilityDuration`
(default 0.5s) so one stationary overlap can't multi-tick damage in a single frame.

**Enemy prefabs set `invulnerabilityDuration: 0`, not the 0.5s default** — deliberately.
`DashHurtbox` (below) already de-dupes hits per dash instance on its own; if `Health`'s i-frames
were also active at their default 0.5s, they'd silently eat the 2nd and 3rd hits of a Tier-3
chain combo (chain links land ~0.2s apart, well inside a 0.5s window), breaking the shield
enemy's core mechanic. The player keeps the real 0.5s i-frames, since melee/arrow damage has no
equivalent per-source dedup and actually needs them.

### Dash as a weapon — `DashHurtbox.cs`

Reusable trigger component (`RequireComponent(typeof(Health))`), used by the archer, the
melee enemy, and the shield enemy once broken. Enemy colliders are **triggers** specifically
so the dash physically passes through them (matches the "hit multiple enemies per dash"
decision). On `OnTriggerEnter`/`OnTriggerStay`: if the other collider's `PlayerController.
IsDashing` and its `DashInstanceId` differs from the last id this hurtbox reacted to, apply
`player.DashDamage` and record the id — naturally supports multi-enemy dashes and separate
chain-link hits without extra bookkeeping, since a fresh `DashInstanceId` is stamped every
time a new dash or chain-link starts.

### Archer — `ArcherEnemy.cs` + `ArrowProjectile.cs`

Stationary turret (no NavMesh/pathfinding in this project, kept consistent): polls distance to
a cached `FindFirstObjectByType<PlayerController>()`, faces the player once inside
`detectionRange` (10), fires on `fireCooldown` (2s) from a child `Muzzle` transform. `Arrow`
prefab is a small scaled cube (`ArrowProjectile.cs`: `speed` 10, `damage` 10, `lifetime` 5s) with
a trigger `BoxCollider` + kinematic `Rigidbody` (kinematic Rigidbody required for Unity to fire
trigger events against static level geometry, same as any other trigger-vs-static pairing) —
moves via `transform.Translate`-style forward motion, damages the player and destroys itself on
contact, or on hitting anything that isn't the player/another arrow/its own archer. Has
`Health` (30) + `DashHurtbox` + `EnemyHealthBarUI` + `DestroyOnDeath` — dies to the dash like
any light enemy.

### Melee — `MeleeEnemy.cs`

Simple `chaseRange`(8)/`attackRange`(1.5)/`attackCooldown`(1.2s) state machine, same
no-pathfinding polling approach as the archer: idle outside chase range, moves straight toward
the player on the XZ plane inside it (flat kinematic movement, no gravity/grounding — fine for
the current flat/ramp layout, would need a real grounded mover if enemies ever need to path
over more complex terrain), stops and damages the player (`attackDamage` 15) on a cooldown once
in attack range. `Health` (50) + `DashHurtbox` + `EnemyHealthBarUI` + `DestroyOnDeath`.

### Shield — `ShieldEnemy.cs`

Starts **shielded**: a solid (non-trigger) `BoxCollider` that physically blocks the player like
a wall, `DashHurtbox` present but disabled (`m_Enabled: 0` in the prefab **and** redundantly
disabled in `Awake()`), immune to damage. A translucent cyan sphere child (`ShieldVisual`,
`ShieldVisual.mat` with HDR emission, same glow technique as `PulsingOrb`) visualizes the
shield and is hidden once broken.

`RegisterDashChainHit(int chainCount)` is called from `PlayerController.
OnControllerColliderHit` — because the enemy is a solid wall, dashing into it blocks the
player's motion (a normal `CharacterController` collision), which fires that callback on every
chain-link press while the player stays planted against it. No extra contact-tracking needed:
the moment `chainCount >= 3` arrives (meaning the player's existing chain system already
verified 3 dashes landed within the chain window), the shield breaks. This only reads state
`PlayerController` already maintains — `ShieldEnemy` doesn't duplicate any chain-timing logic.

On break: flips its own collider to `isTrigger = true`, hides the shield visual, enables its
`DashHurtbox` — from that point on it behaves exactly like a light enemy (`Health` 40), killable
by ordinary passing-dashes ("then could be hit to dead", per the user's original request).

### Health bar UI

- `HealthBarBuilder.cs` — small static helper, builds a background + fill `Image` pair under a
  given `RectTransform`. Factors out boilerplate shared by both bar types below (two immediate
  call sites, not speculative reuse). Same runtime-Canvas-construction approach already proven
  by `HandOverlayUI.cs` — no hand-authored UI prefabs.
- `PlayerHealthBarUI.cs` — fixed screen-space bar, top-left, on `Character`.
- `EnemyHealthBarUI.cs` — world-space bar billboarded above the enemy's head every `LateUpdate`
  (faces `Camera.main`). Hidden at full HP, and hidden entirely while a `ShieldEnemy` is still
  shielded (checked via `GetComponent<ShieldEnemy>()?.IsShielded`) — appears once damaged or
  once the shield breaks.

### Player respawn — `PlayerRespawn.cs`

Captures `transform.position` in `Awake()` as the respawn point (no dedicated scene marker
needed — the `Character`'s own starting position *is* the spawn point). Subscribes to `Health.
Died`: calls `PlayerController.Teleport(...)` back there, then `Health.Revive()`.

### Enemy prefabs & scene placement

`Assets/Prototype/Prefabs/`: `ArcherEnemy.prefab`, `MeleeEnemy.prefab`, `ShieldEnemy.prefab`,
`Arrow.prefab` — hand-authored YAML, same fileID/GUID workflow used for `DashPickup.prefab`.
Materials: `ArcherEnemyBody.mat` (green), `MeleeEnemyBody.mat` (dark red), `ShieldEnemyBody.mat`
(steel grey), `ShieldVisual.mat` (glowing cyan), `Arrow.mat` (dark brown). One instance of each
enemy placed in `Movement.unity` near the player's spawn area for testing (Archer `(6, 2.012,
-6)`, Melee `(-8, 2.012, -6)`, Shield `(1.69, 1.912, -3)`, all root-level `PrefabInstance`s,
registered in the scene's `SceneRoots` manifest alongside the existing pickups/`HandTracking`).

## Known follow-ups / not done yet

- `Face` is still a visible sphere (has `MeshRenderer` + `SphereCollider`) — swap for an empty
  GameObject once the real character model exists. Script-side, nothing needs to change.
- Pushable-object damage (shoving a crate into an enemy) was explicitly **deferred** — not part
  of the combat pass above, follow-up feature.
- Combat balance (`dashDamage` 20, enemy HP values, attack damage/cooldowns, detection ranges)
  is all first-pass defaults, unplaytested.
- Dash feel (`dashSpeed`/`dashDuration`/`dashChainWindow`) hasn't been playtested/tuned yet,
  just set to reasonable starting values.
- No UI/prompt currently shows "Press E to inspect" or dash-tier feedback — everything is
  silent/keyboard-only right now.
- Hand-rotation sign/mirroring (`invertVertical`/`invertHorizontal` on `InspectablePickup`) was
  never verified live — check first if gestures feel backwards before assuming it's a logic bug.
- `openFingerMargin` (1.2×) is an untuned starting heuristic for open/closed detection; may need
  calibration per webcam/lighting.
- A gesture-based alternative to the **E** key for confirming a pickup was discussed but not
  built: a pinch (thumb tip near index tip, landmarks 4 and 8) was the recommendation — reads
  as "grabbing" the object, and is orthogonal to the open/closed states already driving
  rotation so it wouldn't false-trigger mid-rotate. Not implemented yet.
- `README.md` (repo root, Spanish, no emojis) covers install/run for teammates, including the
  per-machine MediaPipe tarball step and confirmation that Mac is supported.
