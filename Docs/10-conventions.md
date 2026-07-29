# 10 — Conventions & Gotchas

## φ / natural-math tuning (`PhiMath`)

`Assets/Scripts/Misc/PhiMath.cs` is the single source of golden-ratio constants:

- Constants: `PHI` (1.618), `INV_PHI` (0.618), `INV_PHI2` (0.382), `PHI2`–`PHI5`, `GoldenAngleDeg/Rad`.
- `GoldenSpiralPoint(i, n, r)` — Vogel/sunflower disc packing for scatter placement (provably even, no clumps).
- `GoldenSequence(i)` — low-discrepancy 0..1 sequence for repeated "random" picks (no streaks, no gaps).

**Rules:** new tuning numbers use the nearest φ expression (`PhiMath.PHI2`, `1f / PhiMath.PHI4`, …) instead of arbitrary magic values; repeated random picks prefer `GoldenSequence`; group scatter prefers `GoldenSpiralPoint`. Existing φ anchors: movement physics (all of it), crit = φ×, crouch height 1.618, bhop window 1/φ³, round-UI dim 1/φ², door timings φ/2 & φ⁴, torch flicker frequency ratio φ. Deliberately non-φ: `walkSpeed 5` / `runSpeed 13` (Fibonacci — their ratio ≈ φ²) and `gravity 9.81`.

## Singleton pattern

`public static X instance` (or `Instance`) assigned in `Awake` (destroy-duplicate guard), **consumed by other scripts in `Start`** — never read another singleton's `instance` from `Awake`; Awake order is undefined. ~50 managers follow this.

## Serialization gotchas (cost us real bugs — read this)

1. **Inspector values override code defaults.** Changing a `[SerializeField]` default in code does nothing for objects already serialized in a scene/prefab. Fix the value in the Inspector (or scene YAML). When adding a field, the code default applies only until the scene is next saved.
2. **Float colors are 0–1.** `new Color(255, 215, 0)` is 255× overbright, not gold.
3. **Renaming a serialized field loses its scene value** (use `[FormerlySerializedAs]` if you must). Renaming a public method breaks every UnityEvent wired to it in scenes/prefabs — grep before renaming, or don't.
4. **UnityEvents hide flow.** Round logic, death behavior, and pickups are wired in the Inspector. When something "isn't called from anywhere," check prefab/scene events first.

## Pooling lifecycle rules (doc 05 has the full checklist)

- `Awake` = once-ever init and capturing "original" values. `OnEnable` = per-activation init. **Never rely on `Start` for anything a pooled reset needs** — `ResetAll` runs before first `Start`.
- Whatever death/disable events mutate (disabled `AIPath`, swapped materials, detached children), the reset path must explicitly restore.
- Never `Destroy` pooled objects or their children.

## Update-loop hygiene

- No per-frame `GetComponent`, `Camera.main` (use `CachedCameraMain`), uncached `renderer.material`, or string shader/animator lookups (cache `Shader.PropertyToID` as `static readonly`).
- Throttle + change-detect UI text (string building allocates). Push (`Refresh()`) instead of polling.
- Repeated timers: timestamps (`Time.time`) or a single staggered manager (see `StateManager`, `TorchFlickerManager`) over per-instance `Update` countdowns.
- `Invoke(nameof(Method), t)` — never string literals. Prefer coroutines or timestamps for anything cancellable.
- Coroutines are **not** stopped by `enabled = false` — only by deactivating the GameObject, `StopCoroutine`, or destruction. If a system must be stoppable via `enabled`, drive it from `Update` (this bit RoundSystem once).

## Input

New Input System via `InputActionReference`: subscribe `performed`/`canceled` in `OnEnable`, unsubscribe in `OnDisable`. Polling: `action.IsPressed()` / `ReadValue<T>()`; keyboard: `Keyboard.current[Key.X].wasPressedThisFrame`. (Legacy `Input.*` survives only in old cheat scripts — don't add more.)

## FMOD

All audio through FMOD (doc 08). The `MovementState` global parameter doubles as the AI-hearing input — audio and stealth are coupled by design.

## Known cheats & debug

README cheats: ↑+3 unlock third spell, ↑+M refill magicka; typing `oioi` in-game activates the cheat panel (`Test/Cheats/`). `Test/` also holds joke/debug scripts (`PressFtoFart`, `GameSpeedSlider`, pickle activators).
