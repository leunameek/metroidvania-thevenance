# Movement, Pickup & Inspection

Spec of everything built in the `Assets/Prototype` sandbox so far: 3D movement, camera,
ladders, ramps, and the dash power-up pickup/inspection flow. Written for a future session
to pick up context quickly — check the scene/scripts against this if anything drifted.

Scene: `Assets/Prototype/Scenes/Movement.unity`
Scripts: `Assets/Prototype/Scripts/`
Prefabs: `Assets/Prototype/Prefabs/`
Materials: `Assets/Prototype/Materials/`

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
- Gravity is manual (`gravity = -20`), with the usual `-2` grounded stick so `isGrounded`
  doesn't flicker.

Serialized tunables: `moveSpeed` (5), `jumpHeight` (1.5), `gravity` (-20), `climbSpeed` (4),
`rotationSpeed` (540), `faceAnchor` (Transform), `dashSpeed` (12), `dashDuration` (0.2),
`dashChainWindow` (0.2).

### Ladder climbing

- `Ladder.cs` is an empty marker component on the `Ladder` prefab. The prefab's `BoxCollider`
  is a **trigger** (was solid originally — swapped so it doesn't block the player like a wall).
- `PlayerController` tracks `_laddersTouching` (int, not bool — safe under overlapping
  colliders) via `OnTriggerEnter`/`OnTriggerExit`, checking `other.GetComponent<Ladder>()`.
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

## Dash pickups & first-person inspection — `DashPickup.cs` + `PulsingOrb.cs`

One prefab (`DashPickup.prefab`), reused for all three tiers via a `[Range(1,3)] dashTier`
field — no per-tier prefab variants.

### Visual — `PulsingOrb.cs`

Small sphere (local scale 0.5) with a `Point` `Light` (cyan) and the `DashPowerUp.mat`
material (URP/Lit, dark base color + bright cyan HDR `_EmissionColor` for bloom). Each frame:
bobs `transform.localPosition.y` on a sine wave (`floatAmplitude`/`floatSpeed`), and pulses
both the light's `intensity` and the material's emission color together via
`MaterialPropertyBlock` (no per-instance material duplication). `SetFloating(bool)` pauses
just the bob — used during inspection so the object holds still while it's being rotated by
the player, without stopping the light pulse.

### Interaction range

`SphereCollider` on the pickup is a **trigger**, `m_Radius = 6` with the object's `0.5` scale →
**3-unit world-space interaction radius**. (Started at the default `0.5`/`0.25` world radius,
which required standing almost inside the tiny orb — bumped up so the prompt zone is generous.)

### State machine (`DashPickup.Update`)

`State.World → EnteringInspect → Inspecting → (World)`, driven by `Keyboard.current`:

1. **World** — `OnTriggerEnter`/`Exit` (checking `other.GetComponent<PlayerController>()`) just
   track `_playerInRange`; nothing happens until the player presses **E** while in range.
2. **E pressed → `BeginInspect()`**: disables the `Camera.main`'s `CameraFollow` component,
   pauses the orb's float bob, calls `_player.SetInputLocked(true)` (freezes
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
   Instead, mouse delta (`Mouse.current.delta`) drives `_yaw`/`_pitch` that get applied straight
   to `transform.rotation` **on the pickup object itself** (`Quaternion.Euler(_pitch, _yaw, 0)`,
   unclamped). So the player spins the object in place in front of a static, POV-framed camera
   — deliberately not an orbiting third-person camera. From here:
   - **E again → `Claim()`**: `_player.GrantDash(dashTier)`, `EndInspect()`, then
     `gameObject.SetActive(false)`.
   - **Escape → `EndInspect()`** directly: backs out without claiming, pickup stays in the world.
5. **`EndInspect()`**: snaps the camera's position *and rotation* back to the
   `_transitionStartPos`/`_transitionStartRot` captured in step 2, **then** re-enables
   `CameraFollow`, un-pauses the orb float, and calls `_player.SetInputLocked(false)`.

   **Why the explicit snap-back exists:** `CameraFollow` only ever writes `transform.position`
   (see above) — it never touches rotation. Originally `EndInspect()` just re-enabled
   `CameraFollow` and let it take over; position recovered fine via `SmoothDamp`, but rotation
   stayed stuck wherever the inspect view last pointed, so gameplay resumed looking the wrong
   way. Since the player is frozen for the whole inspection, the pre-inspect pose is still
   valid when it ends, so a direct snap (no re-blend needed) fixes it cleanly on both the
   `Claim` and `Escape` exit paths.

### Granting the ability

`Claim()` → `_player.GrantDash(dashTier)`. Nothing here implements damage or breaking
things yet — this pickup is purely the visual + inspection interaction + ability-tier grant.
`HasDash`/`DashTier` on `PlayerController` are the hook point for that future work.

## Scene placement

Three pickup instances of the one `DashPickup` prefab, tier set via a `dashTier` override
per `PrefabInstance` (target `fileID: 107` = the `DashPickup` MonoBehaviour):

| Tier | Name in Hierarchy | Position |
|---|---|---|
| 1 | Dash Pickup (Tier 1) | `(1.69, 2.51, -6.65)` — near spawn |
| 2 | Dash Pickup (Tier 2) | `(16.52, 6.3, 7.29)` — above the platform reached by the ladder |
| 3 | Dash Pickup (Tier 3) | `(51.57, 2.59, 7.27)` — far platform, end of the current layout |

`PlayerController.faceAnchor` on the `Character` is wired to the `Face` child transform.

## Known follow-ups / not done yet

- `Face` is still a visible sphere (has `MeshRenderer` + `SphereCollider`) — swap for an empty
  GameObject once the real character model exists. Script-side, nothing needs to change.
- Damage-dealing and breakable objects (the other half of "dash capable of making damage, and
  break things power up") are **not implemented** — deferred on purpose per the user.
- Dash feel (`dashSpeed`/`dashDuration`/`dashChainWindow`) hasn't been playtested/tuned yet,
  just set to reasonable starting values.
- No UI/prompt currently shows "Press E to inspect" or dash-tier feedback — everything is
  silent/keyboard-only right now.
