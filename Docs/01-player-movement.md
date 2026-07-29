# 01 — Player Movement

**File:** `Assets/Scripts/Player/Movement/PlayerMovement.cs` (singleton `PlayerMovement.Instance`)

Quake-style velocity physics on a `CharacterController`. `moveDirection` is a persistent world-space **velocity** (m/s), not an input vector. Each frame: probe the ground, apply friction + acceleration (grounded) or air-accelerate + gravity (airborne), then `characterController.Move(velocity * dt)`.

## The physics core

Grounded, every frame:
1. **Friction** — `speed -= max(speed, frictionStopSpeed) · groundFriction · dt`. The `frictionStopSpeed` floor (φ) makes the final bit of sliding stop cleanly instead of trailing off forever. Lower `groundFriction` = icier.
2. **Accelerate** (the actual Quake formula) — `vel += wishdir · min(accel · wishspeed · dt, wishspeed − vel·wishdir)`. The dot-product term is what makes strafing physics emerge naturally.

Airborne: same `Accelerate` but `wishspeed` is capped at `airSpeedCap` (φ m/s) with high `airAcceleration` — this tiny-cap/big-accel combination is exactly what enables **air-strafing** speed gains. Gravity accumulates on `moveDirection.y`; horizontal air speed is clamped to `MaxHorizontalSpeed = runSpeed · φ`.

### φ-tuned parameters (Inspector, "Quake Physics")

| Field | Default | Meaning |
|---|---|---|
| `groundAcceleration` | φ⁵ ≈ 11.09 | How fast you reach wish speed |
| `groundFriction` | φ√φ ≈ 2.058 | Ground drag (lower = icier) |
| `frictionStopSpeed` | φ | Below this, friction bites fully |
| `airAcceleration` | φ⁴ ≈ 6.854 | Air-strafe responsiveness |
| `airSpeedCap` | φ | Per-wish air gain cap |

## Ground & slope handling

- `ProbeGround()` SphereCasts under the capsule every frame → stable grounded flag + surface normal. Never trust raw `characterController.isGrounded`; it flickers on slopes. Other systems read **`IsGroundedStable`**.
- On walkable ground, velocity is **projected onto the surface plane** — downhill movement hugs the slope instead of stair-stepping through air.
- A **stick force** (`groundStickForce`, 3) presses along the surface normal at Move time — keeps contact without slowing tangential movement. `StairGravity()` / `NormalGravity()` (called by `StairGravity` trigger volumes) switch it to `stairStickForce` (12) and back.
- **`SnapToGround()`** after the move welds the controller over stair edges and slope crests (up to `groundSnapDistance`, 0.35 m).
- Ground steeper than the controller's Slope Limit → slide branch (`steepSlopeSlideSpeed`), no jumping.
- `airborneByImpulse` marks jumps/explosions so uphill motion (positive projected y) is not mistaken for being airborne, and so stick/snap can't eat a jump on its launch frame.

## Jumping & bhop

- Jump buffer: pressing jump up to φ/10 s early still fires on landing.
- **Auto-hop:** holding jump hops on the exact landing frame.
- A hop executed on the landing frame runs **before friction** → momentum fully preserved (the real Quake bhop mechanic).
- If the jump lands within `bhopTimingThreshold` (1/φ³ ≈ 0.236 s) of touchdown **and** movement keys are held, horizontal velocity is multiplied by `1 + bhopSpeedBonus`, clamped to `runSpeed · φ`. Bhop jumps cost 1/φ⁴ of normal jump stamina.
- `AddExplosionJump(force, center, radius)` — rocket jumps. Called by `Bullet.ApplyRocketJumpForce` with `rocketJumpForce · φ²`.

## Speed-based damage

`SpeedDamageMultiplier` is computed every frame: lerp from `minDamageMultiplier` (0.25 at/below `walkSpeed`) to `maxDamageMultiplier` (2.5 at `runSpeed · φ`). `Bullet` samples it **at impact**. `HorizontalSpeed` and `MeasuredVelocity` are the public speed readouts — `MeasuredVelocity` is sampled *before* the ground-snap move because `CharacterController.velocity` only reflects the last `Move()` call.

## Consumers of movement state

| Consumer | Reads |
|---|---|
| `CameraBob` (drives handbob **and** fires footstep events) | `IsGroundedStable` |
| `FootstepSystem` | `IsGroundedStable`, `anyMovementKeysPressed` |
| `CharacterControllerVelocity` (speedometer) | `MeasuredVelocity` |
| `AiVision` hearing, `HearingRange` | `movementStateForFMOD` (0 sneak / 1 walk / 2 run) |
| `SpeedIndicator` UI | `HorizontalSpeed`, `MaxHorizontalSpeed`, `SpeedDamageMultiplier` |
| `Dash`, `CameraSkewController` | `moveDirection`, `speedMultiplier` |

## Movement states

`HandleMovementState()`: Sneak (crouch held) / Run (sprint + grounded + stamina) / Walk. State changes set capsule height, the FMOD `MovementState` global parameter, and drive stamina drain/recovery. Speeds are **never mutated** — current wish speed is derived from the state (`crouchSpeed`/`runSpeed`/`walkSpeed`).

## Tuning tips

- Momentum feel: `groundFriction` (glide) and `bhopTimingThreshold` (chain forgiveness).
- Bhop growth per hop: `bhopSpeedBonus` (scene currently 0.06 — very subtle; φ−1 ≈ 0.618 is aggressive).
- Remember: **Inspector values override code defaults** (see doc 10). The scene's serialized values on the Player are the live tuning.
