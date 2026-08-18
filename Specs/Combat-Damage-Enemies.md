# Combat, Damage & Enemies

Everything built **after** the original combat section in `Movement-Pickup-Inspection.md` —
health bar bug fixes, the shield enemy's full charge/patrol/alert AI, fall damage, and double
jump. That file's "Combat system" section describes the *first pass* (still accurate for
`Health.cs`, `DashHurtbox.cs`, the archer, and the melee enemy); this doc is the current source
of truth for everything that changed or was added on top of it. Read both — this one doesn't
repeat what's still correct there.

Scene: `Assets/Prototype/Scenes/Movement.unity`
Scripts: `Assets/Prototype/Scripts/`
Prefabs: `Assets/Prototype/Prefabs/`
Materials: `Assets/Prototype/Materials/`

## Health bar bug: `Image.Type.Filled` silently ignored without a sprite

Real Unity gotcha, cost a few rounds of live debugging to isolate (see git history / prior
session for the diagnostic `Debug.Log` trail — since removed). Both `PlayerHealthBarUI` and
`EnemyHealthBarUI` build their fill bar via `HealthBarBuilder.Build()`, which creates an
`Image` with no `sprite` assigned. Unity's `Image.OnPopulateMesh()` has a fast path: **if
`overrideSprite == null`, it falls back to `Graphic.OnPopulateMesh()` and completely ignores
`type`/`fillMethod`/`fillAmount`**, just drawing a plain full-size rect. So `fillAmount` was
being set correctly every hit (confirmed via logging), `Health.CurrentHealth` was dropping
correctly, but the bar visually never moved — always full.

**Fix**, in `HealthBarBuilder.cs`: a shared 1×1 white `Sprite` (built once from
`Texture2D.whiteTexture`, cached in a static field, reused by every bar instance) assigned to
the fill `Image` before setting `type = Filled`. That's the only thing that was wrong — the
fill math itself, anchors, and event wiring were all correct the whole time.

## `EnemyHealthBarUI` — bar didn't disappear when the enemy died

`EnemyHealthBarUI.BuildUI()` deliberately does **not** parent its world-space `Canvas`
GameObject under the enemy (it's billboarded manually in `LateUpdate` instead, to avoid
inheriting the enemy's scale/rotation — e.g. `ShieldEnemy`'s non-uniform `1.2/1.8/1` scale
would otherwise stretch the bar). Because it's unparented, destroying the enemy (via
`DestroyOnDeath`) never cascaded to the bar — it kept floating in place after the enemy was
gone.

**Fix**: `EnemyHealthBarUI` now also subscribes to `Health.Died` and calls
`Destroy(_barTransform.gameObject)` directly. Same unparented-billboard pattern was reused
later for `ShieldEnemy`'s charge alert (see below) and given the same treatment up front.

## Shield enemy — full AI (supersedes the "starts shielded, breaks on Tier-3" description)

`ShieldEnemy.cs` now runs a 4-state machine (`Idle → Windup → Charging → Recovering → Idle`)
layered on top of the original shield/break mechanic, which is unchanged. All of this runs
**regardless of shield state** — a broken (unshielded) shield enemy still patrols and charges,
it's just also killable by then.

### Patrol (Idle state)

While `Idle` and the player is outside `detectionRange`, it wanders: picks a random point
within `patrolRadius` (4) of its **spawn position** (captured once in `Awake`), walks to it at
`patrolSpeed` (1.5), pauses `patrolPauseDuration` (1.5s), picks a new point, repeats. Flat XZ
movement via `transform.position +=`, no gravity/pathfinding — same simplification already used
by `MeleeEnemy`/`ArcherEnemy`. No ledge/obstacle awareness — a platform smaller than the patrol
radius can walk it toward an edge.

### Windup — the telegraph

Every `Idle` frame also checks distance to the player; entering `detectionRange` (12) **and**
being farther than `minChargeRange` (2.5, see balance note below) locks in the current direction
to the player and starts `Windup`. For `windupDuration` (0.9s) the enemy just rotates in place
to face that locked direction (`turnSpeed` 360°/s) — no movement. This is the player's reaction
window.

**HUD alert**: a red `!` appears on screen for the whole windup (see next section) as the actual
visual cue — nothing changes on the enemy model itself.

### Charging

After the windup timer, it charges: moves in the **locked** direction (decided at windup, not
re-tracked) at `chargeSpeed` (18) for up to `chargeDuration` (0.5s — a short committed lunge,
not a sustained chase; was 1.2s originally and felt like it was "still following" the player
because the total travel distance was long, not because it re-tracked). Straight-line, ignores
world geometry (can charge through walls — same no-collision-avoidance simplification as
patrol). Each frame checks flat XZ distance to the player; closing to within `hitRadius` (1.2)
deals `chargeDamage` to the player's `Health` and ends the charge immediately; otherwise it ends
when the duration runs out. Either way, moves to `Recovering`.

### Recovering

Stationary for `cooldown` (6s, "generous" per explicit request — was 3s) before returning to
`Idle` and becoming chargeable again.

### Balance history on this attack

- `chargeDamage` started at `9999` (explicit instakill, per the original request: "charged
  attack could insta kill the player"). Later dialed back to **50** (half the player's 100 max
  HP) — same request that asked for it, just softened after playtesting felt it was too punishing
  combined with everything else. `Health.TakeDamage`'s clamp-to-max-of-0 means this is 50 flat
  damage, not a percentage — if `maxHealth` ever changes, revisit this value.
- `minChargeRange` (2.5) exists to fix a real interaction bug, not just tuning: the charge AI has
  **no minimum range**, so originally it could windup/charge even at the exact distance needed to
  land the shield-break Tier-3 dash combo — the enemy would either interrupt the player's combo
  attempt or outright kill them with the charge before 3 hits landed, making the shield
  effectively unbeatable ("it's bit OP"). Gating windup to `distance >= minChargeRange` means
  once the player closes to combo range, the enemy can't start a new charge (a charge can only
  begin from `Idle`), giving a guaranteed safe window — interrupted at most by slow patrol
  wandering, never by another charge.

## HUD charge alert — screen-space `!`, not a world marker

First built as a world-space billboard floating above the enemy (same pattern as the health
bar). Explicitly moved to a fixed **screen-space HUD element** instead, per direct feedback —
the ask was for a reaction-time cue the player notices immediately, not something that has to be
in-frame and pointed at the right 3D spot to be seen.

- Built once, lazily, by whichever `ShieldEnemy` instance's `Awake()` runs first
  (`static GameObject _alertGo`, guarded by `if (_alertGo == null)`) — shared across every
  shield enemy in the scene so simultaneous chargers don't stack duplicate HUD elements.
- `ScreenSpaceOverlay` `Canvas`, `sortingOrder 950` (above the health bars' 900), anchored
  top-center. The `!` itself is two plain `Image` rects (a bar + a dot) — **not** a `Text`/TMP
  glyph, specifically to avoid depending on a runtime font lookup (`Resources.
  GetBuiltinResource<Font>(...)` names have changed across Unity versions and aren't something
  worth hand-authoring into prefab YAML blind).
- Shown at the start of `Windup`, hidden the instant `Charging` begins — it's a reaction-time
  cue, not a "danger nearby" indicator, so it disappears once the window it warns about has
  closed.

## Player fall damage & last-grounded respawn — `PlayerRespawn.cs`

Original `PlayerRespawn` only handled death (`Health.Died` → teleport to the scene's starting
position + full revive). Extended to also handle falling off a platform:

- `PlayerController.IsGrounded` (new, just exposes `_controller.isGrounded`) is polled every
  `Update()`; whenever true, `_lastGroundedPosition` is recorded.
- If `transform.position.y` drops below `fallThresholdY` (-10), `HandleFall()` fires: applies
  `fallDamage` (30) via `Health.TakeDamage`, then teleports back to `_lastGroundedPosition`.
- **Falling cannot itself cause a "wrong" respawn.** If the fall damage would/does kill the
  player (`Health.Died` fires), the existing death handler (`HandleDied`) already teleports to
  the fixed scene-start position and revives — that should win, not the last-grounded position
  (which would likely be right at the edge of the pit they just died in). Ordering hazard: by
  the time `TakeDamage()` returns, a death handler that revives has already reset
  `Health.IsDead` back to `false`, so checking `IsDead` *after* the call can't distinguish "died
  and got revived" from "never died." Fixed with an explicit `_justDied` flag, set synchronously
  inside `HandleDied` and consumed immediately after the `TakeDamage()` call in `HandleFall()` —
  if it's set, skip the last-grounded teleport, the death path already handled it.
- Earlier version of this clamped fall damage so it could never drop health below 1 (i.e. "falls
  can only hurt, never kill"). **Removed** — the user explicitly wants falling with no health
  left to be lethal, not an infinite bounce-at-1-HP loop. `fallDamage` is now applied unclamped.

`fallThresholdY`/`fallDamage` are Inspector fields on `PlayerRespawn` — untested against the
actual level's pit depths, tune per-level once real geometry exists.

## Double jump

Second traversal upgrade, deliberately built to mirror the dash pickup's pattern exactly (no new
pickup infrastructure needed — `InspectablePickup`/`IPickupReward` are already fully generic).

- **`PlayerController.cs`**: `HasDoubleJump` (bool, granted via `GrantDoubleJump()` — one-shot,
  no tiers, unlike dash) and `_airJumpAvailable`, which refills to `HasDoubleJump`'s value every
  frame the controller is grounded and gets spent on one Space press while airborne. Reuses the
  exact same `Mathf.Sqrt(jumpHeight * -2 * gravity)` jump-impulse formula as the ground jump, so
  the second jump feels identical in height/arc. Cleared (set `false`) on `Teleport()` and
  `SetInputLocked(true)`, matching how dash state is already reset in both places.
- **`DoubleJumpPickupReward.cs`**: the only new script — one line, `player.GrantDoubleJump()`.
  No tier field (unlike `DashPickupReward`), since this is "just one orb."
- **`DoubleJumpPickup.prefab`**: structurally identical to `DashPickup.prefab` (orb + `TopMarker`
  child, trigger `SphereCollider` radius 6, `Point` `Light`, `PulsingOrb`, `InspectablePickup`),
  just swapped to its own materials (`DoubleJumpPowerUp.mat` / `DoubleJumpPowerUpTopMarker.mat`,
  gold/amber instead of dash's cyan/red) so the two pickups read as different things at a
  glance. One instance placed in `Movement.unity` near spawn, close to the Tier-1 dash pickup —
  **not verified against actual platform geometry**, reposition in the Editor if it doesn't land
  on solid ground.

## Known follow-ups specific to this doc's systems

- `ShieldEnemy`'s patrol and charge both ignore world geometry entirely — no ledge checks, no
  wall collision during a charge. Fine for the current flat/simple layout; add raycasts if levels
  grow more complex.
- The charge attack's `!` alert is built from two plain rectangles, not a real glyph — good
  enough to read at a glance, but revisit with an actual icon/sprite if the HUD gets a real art
  pass.
- `fallThresholdY`/`fallDamage`, and all of the shield enemy's charge tunables
  (`detectionRange`/`minChargeRange`/`chargeSpeed`/`windupDuration`/`chargeDuration`/`cooldown`/
  `chargeDamage`), are first-pass numbers adjusted once each via direct feedback in this session
  — not a finished balance pass across the whole kit.
- Double jump has had zero playtesting against the current level geometry (no Editor access this
  session) — verify it doesn't trivialize any platforming that was tuned around single-jump-only.
- Enemy scene placement (`ArcherEnemy`/`MeleeEnemy`/`ShieldEnemy` positions in `Movement.unity`)
  has been adjusted directly in the Editor since these were first placed — don't trust older
  recorded coordinates from earlier spec revisions; check the scene file directly for current
  values.
