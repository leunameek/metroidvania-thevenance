# Technical Summary — Prototype

High-level, current-state overview of `Assets/_Game`: what mechanics exist and what
tech/libraries they're built on. This is a rollup, not a replacement — for implementation
detail, gotchas, and balance history, see `Movement-Pickup-Inspection.md` and
`Combat-Damage-Enemies.md`. This doc adds the one system neither of those covers yet: the
boss encounter (`BossFightController`/`BossFightTrigger`) and voice-command input
(`VoiceCommandRecognizer`/`CommandFeedbackUI`), added in the most recent commit.

Scene: `Assets/_Game/Scenes/Dev/Movement.unity` · Scripts: `Assets/_Game/Scripts/{Model,
View,Controller}/` · Prefabs: `Assets/_Game/Prefabs/` · Materials: `Assets/_Game/Materials/`

## Architecture (MVC)

The course requires an MVC architecture. `Assets/_Game/Scripts/` is split into three
folders; every existing MonoBehaviour kept its filename and `.meta` GUID (scene/prefab
references are keyed by GUID, not path — moving files between folders is safe, deleting and
recreating one is not), so this was a structural refactor, not a rewrite.

- **`Model/`** — plain C# classes, zero `MonoBehaviour`/`GameObject`/`Transform` dependency
  (`Health.cs` is the one exception — it was already a MonoBehaviour before this refactor and
  is kept as the template for the event-driven pattern below, but nothing else in Model depends
  on it). Owns game-rule state and state machines, exposes C# events for state changes (the
  pattern `Health.HealthChanged`/`Died` already used, now applied everywhere). Takes `float
  deltaTime` as a parameter instead of reading `Time.deltaTime` globally, so it's callable from
  outside Unity's frame loop — see the EditMode tests below. Compiled as its own assembly
  (`Prototype.Model.asmdef`, `autoReferenced: true` so `Controller/`/`View/` see it
  automatically) — this is a hard boundary, not just a folder convention: nothing in `Model/`
  may reference a `Controller/`/`View/` type, since that would be a circular assembly
  reference. Contents: `Health.cs`, `PlayerAbilityModel.cs` (dash tier/chain gating, boss-dodge
  cooldown, double jump), `InspectionModel.cs` (the pickup World/EnteringInspect/Inspecting FSM
  + yaw/pitch math), `HandGestureModel.cs` (open/closed-hand heuristic +
  delta-accumulate/freeze-on-close), `ArcherModel.cs`/`MeleeModel.cs` (detection/cooldown/attack
  decisions), `ShieldEnemyModel.cs` (the Idle/Windup/Charging/Recovering charge FSM +
  patrol/charge-hit rules), `BossFightModel.cs` (the Telegraph/PlayerReact/Resolve turn FSM +
  raised-hand-direction judging).
- **`View/`** — presentation-only MonoBehaviours: `HealthBarBuilder.cs`, `PlayerHealthBarUI.cs`,
  `EnemyHealthBarUI.cs`, `PulsingOrb.cs`, `HandOverlayUI.cs`, `CommandFeedbackUI.cs`, and the
  new `BossFightUI.cs` (the boss encounter's alert icon + turn label, extracted verbatim out of
  `BossFightController`; self-attaches via `AddComponent` so it needed no scene wiring).
- **`Controller/`** — MonoBehaviours that read input/physics/collisions and wire Model ↔ View:
  `PlayerController.cs`, `InspectablePickup.cs`, `HandGestureTracker.cs`, `ArcherEnemy.cs`,
  `MeleeEnemy.cs`, `ShieldEnemy.cs`, `BossFightController.cs`, and the smaller glue scripts
  (`DashHurtbox`, `DestroyOnDeath`, `PlayerRespawn`, `CameraFollow`, `ArrowProjectile`,
  `HandTrackingBootstrapper`, `VoiceCommandRecognizer`, `BossFightTrigger`). Also
  `IPickupReward.cs`/`DashPickupReward.cs`/`DoubleJumpPickupReward.cs` — these were first
  scaffolded into `Model/` but actually belong here: their `Grant(PlayerController player)`
  signature references `PlayerController` directly, which the Model assembly can't see, so
  they're really orchestration glue between two Controllers (`InspectablePickup` →
  `PlayerController`), not pure state. Every Controller kept its exact public API
  (method/property names), so nothing that calls e.g. `PlayerController.DashTier` or
  `ShieldEnemy.IsShielded` needed to change.

**What stayed in Controller on purpose**: `CharacterController.Move()`/gravity, `Instantiate`,
collider/transform mutation, and MediaPipe's thread-safety lock/buffer plumbing in
`HandGestureTracker` — these are inherently Unity-engine-coupled, not game rules, so pulling
them into a Model would just relocate the coupling instead of removing it.

**Verification**: `Assets/_Game/Scripts/Tests/EditMode/` has NUnit tests for the branchiest
extracted Models (dash tier/chain-window gating, the shield enemy's `minChargeRange` safe
window, boss-fight miss-vs-dodge timing, hand-gesture freeze-on-close). These were written and
never run — no Unity Editor access this session — so treat them as unverified until run once
via `Window → General → Test Runner → EditMode → Run All`.

## Tech stack

- **Engine**: Unity 6000.3.21f1, Universal Render Pipeline (URP).
- **Input**: new Input System exclusively (`activeInputHandler: 1`) —
  `UnityEngine.InputSystem.Keyboard`/`Mouse`, never the legacy `Input` class.
- **Player physics**: `CharacterController` (kinematic, not Rigidbody) — all motion goes
  through `CharacterController.Move()`, gravity/dash/dodge are hand-rolled vertical velocity
  accumulators, not PhysX gravity.
- **Hand tracking**: `MediaPipeUnityPlugin` (homuler, MIT), installed from a pre-built release
  tarball (not a bare git URL — that skips the compiled native binaries). Bundles native libs
  for Windows/macOS/Linux; runs CPU-delegate only on both Windows and macOS. Loaded via a
  second scene (`Hand Landmark Detection.unity`) opened additively alongside `Movement.unity`.
- **Voice input**: `UnityEngine.Windows.Speech.KeywordRecognizer` — a **Windows-only** built-in
  API (UWP/Windows Standalone). Unlike hand tracking, the boss fight's voice-command dodge has
  no cross-platform fallback; it will fail to construct on macOS/Linux. Not yet guarded with a
  platform check or `try/catch` in `VoiceCommandRecognizer.StartListening()`.
- **UI**: no hand-authored Canvas prefabs anywhere in gameplay systems — every HUD element
  (health bars, hand overlay, charge alert, boss turn label/alert, command feedback text) is
  built at runtime in C# (`new GameObject(...)`, `AddComponent<Canvas>()`, etc.). Text uses
  Unity's legacy `UI.Text` + `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`,
  specifically to avoid TMP asset references and runtime font-lookup fragility.
- No NavMesh/pathfinding anywhere in the project — every enemy (and the boss's would-be
  movement) uses flat polling + direct `transform.position`/`CharacterController.Move` math.
  No inventory, save system, or scene-transition framework exists yet — this is a single-scene
  mechanics sandbox.

## Core mechanics

**Movement** (`PlayerController.cs`) — WASD + Space jump on a `CharacterController`, yaw-only
rotation toward move direction, manual gravity with a grounded "stick" force tuned to survive
descending ramps.

**Camera** (`CameraFollow.cs`) — side-view follow cam (position-only, rotation fixed by the
Editor) by default; can be switched into a **third-person mode** behind the player
(`EnterThirdPerson()`/`ExitThirdPerson()`, distance/height/smoothing all separately tunable),
used for the boss fight.

**Ladders & ramps** — tag-based (`Ladder` tag, trigger collider) climb state that suspends
gravity; ramps are just rotated solid boxes inside the `CharacterController`'s 45° slope limit,
no dedicated ramp logic.

**Dash** — 3-tier progressive ability (unlocked via pickups), chainable within a 0.2s
double-tap window, additive speed stacking per chain link, doubles as the melee weapon (see
Combat below).

**Reactive dodge** (`PlayerController.PerformDodge`) — new, boss-fight-only. A short fixed-
distance lateral burst independent of the platformer dash (no chaining, no hurtbox, no tier
gate), so it still works while normal input is locked during the encounter.

**Pickup & inspection** (`InspectablePickup.cs` + `IPickupReward.cs`) — generic trigger →
E-to-inspect → camera blend to a fixed POV pose → mouse- or hand-driven object rotation →
E-to-claim/Escape-to-cancel flow. Reward behavior is pluggable via `IPickupReward` (`Grant`);
current implementations are dash-tier and double-jump pickups.

**Hand tracking** (`HandGestureTracker.cs` + `HandOverlayUI.cs`) — MediaPipe hand landmarks
drive: (1) pickup-inspection rotation (right hand = pitch, left hand = yaw, closed fist
freezes that axis), and (2) the boss fight's dodge-direction gesture (raised hand — either
side — picks dodge direction). A 21-dot-per-hand debug overlay renders bottom-right; it used
to be toggled visible only during inspection, but the toggle calls were removed in the latest
commit (see Known gaps) so it now default-renders whenever a hand is tracked.

**Combat** (`Health.cs`, `DashHurtbox.cs`, 3 enemy types) — dash passes through and damages
every enemy it overlaps; enemies have generic `Health` + screen/world-space bar UI; three
enemy archetypes (stationary archer, chasing melee, shielded charger requiring a 3-hit dash
combo to break); player has fall damage and last-grounded-position respawn plus death respawn.
Full detail in `Combat-Damage-Enemies.md`.

## Boss fight system (new, undocumented elsewhere)

Turn-based encounter layered on top of the free-roam movement/combat systems, entered/exited
by walking through a trigger volume.

- **`BossFightTrigger.cs`** — a `BoxCollider` (trigger) in the scene ("Boss Fight Trigger",
  size `6×4×10`); `OnTriggerEnter`/`Exit` from the player call `BossFightController.
  StartFight()`/`EndFight()`. Its `bossFight` reference is unset in the Inspector
  (`fileID: 0`) — resolved at runtime via `FindFirstObjectByType` in `Awake()`, same
  self-wiring pattern used throughout the project.
- **`BossFightController.cs`** — a 4-state machine: `Inactive → Telegraph → PlayerReact →
  Resolve → (loops to Telegraph)`.
  - `StartFight()`: switches the camera to third-person (`CameraFollow.EnterThirdPerson`),
    locks player movement input, starts voice listening.
  - **Telegraph** (1.2s): red screen-space alert icon shown, "Turno del Enemigo" label.
  - **PlayerReact** (1.5s window): player must say a dodge keyword *and* have a hand raised in
    the direction to dodge; matching both fires `PlayerController.PerformDodge(direction)` and
    skips straight to Resolve. Missing the window applies flat `missDamage` (15) to the
    player's `Health` via `GetComponent<Health>()`.
  - **Resolve** (0.8s): brief pause, then loops back to Telegraph — this is an unbounded loop
    with no boss HP/attack pattern/win condition, i.e. a mechanic testbed, not an encounter
    with an ending.
  - `EndFight()` (on trigger exit) restores free camera/input and stops voice listening
    regardless of which state the player left mid-encounter.
- **Dodge direction** comes from `GetRaisedHandDirection()`: compares landmark 9 (palm center)
  Y of each hand against `raisedYThreshold` (0.4, image-space, smaller = higher in frame) —
  right hand raised → direction `-1`, left → `+1`, both/neither → no dodge.
- **UI**: `BuildAlertIcon()`/`BuildTurnLabel()` construct two more runtime `ScreenSpaceOverlay`
  canvases (sorting order 950/955, above the shield-charge alert's 950... actually tied with
  it — see Known gaps) directly on the `BossFightController` GameObject.
- Scene placement: "Boss Fight Controller" GameObject at the far end of the level, alongside a
  static "Boss Placeholder" mesh (no AI/animation — a stand-in) and the trigger volume in
  front of it.

## Voice commands

- **`VoiceCommandRecognizer.cs`** — thin wrapper over `KeywordRecognizer` (Windows Speech).
  One command currently defined: `CombatCommand.Dodge`, recognized from either `"esquiva"`
  (Spanish) or `"dodge"` (English), `ConfidenceLevel.Low`. `StartListening()`/`StopListening()`
  are called by `BossFightController` on fight enter/exit — recognition is not active outside
  a boss encounter.
- **`CommandFeedbackUI.cs`** — separate component (also on the boss controller's GameObject,
  `RequireComponent(VoiceCommandRecognizer)`), independently subscribes to the same
  `CommandRecognized` event and pops a "heard phrase → mapped effect" text toast
  (bottom-center, sorting order 960) for 1.5s. Purely diagnostic/UX feedback — does not gate
  or affect the dodge logic itself, which BossFightController reads independently.

## Known gaps worth flagging (not covered in the other Specs docs)

- **Voice commands are Windows-only.** `UnityEngine.Windows.Speech` has no macOS/Linux
  implementation; `README.md` currently only calls out MediaPipe as having per-OS
  considerations. If this ships to the Mac teammate, boss-fight voice dodge will need a
  platform guard or an alternative input path.
- **Hand overlay visibility regressed to "always on".** `HandOverlayUI.SetVisible(false)` was
  removed from `Awake()`, and `InspectablePickup` no longer calls `SetVisible(true/false)`
  around inspection either (both removed in the latest commit, no replacement caller added).
  Net effect: the 21-dot hand overlay now renders continuously whenever a hand is tracked,
  not just during inspection/boss fights as originally designed. Likely an oversight from
  wiring the boss fight's hand-raise detection, not an intentional UX change — revisit before
  next session.
- **Boss fight has no failure/win state** — it's an infinite Telegraph/React/Resolve loop with
  flat miss damage; nothing stops it at 0 HP, and there's no boss attack variety yet.
  `BossFightController` fields (`player`, `handTracker`, `cameraFollow`) are all unwired in the
  Inspector — depends entirely on the `FindFirstObjectByType` fallback in `Awake()`.
- The alert/turn-label canvases (`sortingOrder` 950/955) and the shield enemy's charge alert
  (also 950, see `Combat-Damage-Enemies.md`) can overlap in draw order if both are ever visible
  at once — not currently possible (different game states) but worth noting if either system
  changes.
- Boss fight balance (`telegraphDuration`/`reactDuration`/`resolveDuration`/`missDamage`/
  `raisedYThreshold`) is unplaytested first-pass, same caveat as every other tunable in this
  project.

## File map (scripts added/changed since the last spec pass)

| File | Role |
|---|---|
| `BossFightController.cs` | Boss encounter state machine, telegraph/turn-label UI |
| `BossFightTrigger.cs` | Trigger volume that starts/ends the boss fight |
| `VoiceCommandRecognizer.cs` | Windows Speech keyword recognition → `CombatCommand` events |
| `CommandFeedbackUI.cs` | On-screen "heard X → did Y" toast for recognized voice commands |
| `CameraFollow.cs` | +third-person mode for the boss fight |
| `PlayerController.cs` | +`PerformDodge`, boss-dodge tunables/state |
| `InspectablePickup.cs`, `HandOverlayUI.cs` | Hand-overlay visibility toggle removed (see gaps) |
