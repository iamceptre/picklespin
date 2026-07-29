# 08 — Audio (FMOD)

**All game audio goes through FMOD.** Do not add Unity `AudioSource`s. The FMOD Studio project lives in `Picklespin-FMOD/Picklespin.fspro`; building banks there outputs to `Picklespin/Assets/FMODBanks/` (`Master.bank`, `Master.strings.bank`, `Debug`).

## Playback patterns used in code

| Pattern | When | Example |
|---|---|---|
| `StudioEventEmitter` component + `emitter.Play()` | Positional, reusable, per-object sounds | enemy hit sounds on `AiReferences`, door open/close, torch loops |
| `RuntimeManager.PlayOneShot(EventReference)` | Fire-and-forget | spell shoot sound, spell-locked click |
| `EventInstance` held in code | Sounds needing parameter control / manual stop | casting hold loop in `Attack` |
| `[SerializeField] EventReference` | Preferred way to point at an event (Inspector-assigned) | most scripts |

## Global parameters

`FMODUnity.RuntimeManager.StudioSystem.setParameterByName(...)`:

- **`MovementState`** (0 = sneak, 1 = walk, 2 = run) — set by `PlayerMovement.SetFmodMovementState` on state change. Drives footstep mix **and doubles as the AI hearing input** (`AiVision` reads `movementStateForFMOD`) — if you touch this, you're touching stealth gameplay.

## Snapshots & mix zones

- `AudioSnapshotManager` — keyed snapshot registry (`Enable/Disable/SwitchToExclusive`); logs warnings on unknown keys.
- `AudioSnapshotTrigger` — trigger-volume snapshot control (reverb zones, interiors).
- `TriggerBoxAudioBledner` — blends a target emitter parameter by player position inside a box (distance-based ambience blending).
- `PauseMyEmitters`, `FMODResetManager`, `ResetFMODonEnable`, `IgnoreFirstOnEnableEventFMOD` — pause/scene-reload housekeeping helpers.

## Footsteps

`CameraBob` fires an `OnFootstep` C# event at a sine-phase threshold (bob and footsteps can't desync). `FootstepSystem` plays the emitter (+ every-2nd-step layer), gated on `PlayerMovement.IsGroundedStable`, and `FloorTypeDetector.Check()` sets the surface-type parameter. Jumps/landings: `JumpLandSignals` (soft/hard landing by fall speed) — landings also ping AI hearing.

## Adding a sound — checklist

1. Author the event in FMOD Studio (`Picklespin-FMOD`), assign to a bank, **Build** (F7).
2. Unity: add a `[SerializeField] EventReference` (one-shots) or a `StudioEventEmitter` (loops/positional) and assign the event in the Inspector.
3. Positional sounds on pooled objects: emitters restart via `Play()` on reuse — check behavior across pool reuse (`IgnoreFirstOnEnableEventFMOD` exists for OnEnable-autoplay quirks).
4. Volume routing is handled by bus settings + `VolumeSettingLoader`/`PlayerPrefsSliderManager` — new sounds should live under an existing bus (SFX/Music/…) to inherit the settings sliders.
