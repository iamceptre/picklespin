# 09 — Effects & Visuals

## Dissolve family (death/spawn shader effects)

All drive a `_DissolveAmount` shader property (IDs cached via `Shader.PropertyToID`).

- **`Dissolver`** — enemy death: swaps to `deadMaterial`, animates dissolve, spawns an ash pile on the ground (raycast placement, random yaw/scale), then returns the enemy to its pool. Captures the living material at death so `ResetDissolveState()` can fully restore for reuse. The ash pile detaches to stay behind and is reclaimed on reuse.
- **`SpawnInEnemy`** — spawn-in: transparent → opaque alpha fade + particles, then `StateManager.StartAI()`. Re-runs per pooled activation.
- **`UnDissolveAtEnable`** / **`DissolveBounce`** — generic materialize-on-enable and looping shimmer variants (tweens killed on disable — pooled-safe).

## Torch flicker (φ quasi-periodic)

`TorchFlickerManager` (singleton) updates all registered `TorchFlicker`s centrally: optional frame-skipping and squared-distance culling around the camera. Each torch sums **two sines with frequencies in ratio φ** for sway and glow — quasi-periodic, so the flame never visibly loops, at 4 `sin()` calls per torch. Per-torch random phase + ±20% rate offsets prevent synchronization. Related: `LightFluctuation`, `LightPulsing`, `FadeInLightOnEnable`, `AngelTorchManager`, `Torch`, `PlayAshSoloSoundWhenNotLit`.

## Camera feedback

- **`CameraShakeManagerV2`** (singleton) — indexed shake presets configured in scene (`ShakeSelected(index)`); known indices: 9 = jump, 2 = enemy hit, 6 = enemy death, 8 = big spell, 0/1 = soft/hard landing. `ShakeHand` for hand-only shake; Cinemachine impulse based (`CameraShakeImpulseCustom`).
- **`CameraBob`** — sine bob scaled by measured speed; drives hand bob and emits `OnFootstep`. **`DynamicFOV`** — FOV widens with speed. **`CameraSkewController`** — subtle velocity-based tilt. **`JumpLandSignals`** — landing dip tween + shake + sounds.
- **`ScreenFlashTint`** (singleton) — indexed full-screen tint flashes (damage, death, enemy kill = 6).

## Misc visual helpers

`ScrollTexture` (cached material + property ID, visibility-gated), `Rotate`, `LookAt`/`LookAtY`/`LookAtPlayer` (self-disables beyond range; re-enabled by `EnableLookAtOnEnter`), `FloatUpDown`, `PulsatingImage`, `ImageFadePulse`, `GrowOnEnable`, `ScaleUpOnAwake`, `ExplosionScaleTween` (one-shot, scales with player distance), `ShootingStarManager` (random-interval sky streaks), `FogManager`, `SetDustIntensity`, clouds shader (`Shaders/`).

## Conventions for new effects

1. Cache material instances **once** (`Awake`) and `Shader.PropertyToID` as `static readonly` — never `renderer.material.SetX("_Name", …)` per frame.
2. DOTween: `DOKill()` before retargeting; kill in `OnDisable` if the object can be pooled/deactivated mid-tween.
3. Prefer φ-ratio dual sines over `Random`/noise for organic looping motion (see `TorchFlicker`) — smooth, cheap, never repeats.
4. One-shot world effects that spawn frequently belong in a pool (doc 05).
5. Heavy per-frame effect groups should follow the `TorchFlickerManager` pattern: one manager `Update()`, distance culling, frame skipping — not N independent `Update()`s.
