# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Picklespin is a first-person arena spell-caster game built in **Unity 6000.0.32f1** with **URP 17**. The repo root contains two projects:

- `Picklespin/` — the Unity project (open this folder in Unity Editor)
- `Picklespin-FMOD/` — the FMOD Studio audio project (`Picklespin.fspro`); built banks land in `Picklespin/Assets/FMODBanks/`

There are no CLI build scripts, tests, or linters — building and playing happens through the Unity Editor. Scenes: `Assets/Scenes/Menu_Main.unity` and `Assets/Scenes/Chruch_Arena.unity` (the "Chruch" misspelling is canonical; don't rename assets to fix it).

## Architecture

All game code lives in `Assets/Scripts/`, organized by category: `AI/` (with `AI/Angel/` for the angel NPCs and `AI/State Stuff/` for the FSM), `Player/` (`Movement/`, `Attack/`), `GameFlow/`, `Items/`, `UI/`, `Audio/`, `Camera/`, `Environment/` (torches, fog, ambiance), `VFX/` (dissolves, particles), `Settings/` (options/PlayerPrefs), `Menus/`, `MapInteractions/`, `Misc/`, `SpecificSpells/`, `Test/`. Keep new scripts in the matching category folder — no loose scripts at the top level. Third-party code is in `Assets/Plugins/`: A* Pathfinding Project (enemy navigation), FMOD, DOTween (Demigiant), BeautifulDissolves, VolumetricFog, Camera Shake, Magic Light Probes.

**φ-based tuning.** Gameplay constants derive from the golden ratio via `Assets/Scripts/Misc/PhiMath.cs` (φ powers for feel constants, golden-angle Vogel spiral for spawn scattering, `GoldenSequence` for low-discrepancy "random" picks). When adding or changing tuning numbers, prefer the nearest φ expression (`PhiMath.PHI2`, `1/PhiMath.PHI4`, …) over arbitrary magic values, and prefer golden-sequence sampling over `Random` for repeated picks. Player movement (`PlayerMovement.cs`) is Quake-style velocity physics fully tuned in φ.

**Singleton pattern everywhere.** ~50 manager/UI scripts expose `public static X instance` (or `Instance`), assigned in `Awake()`. Cross-references to other singletons are grabbed in `Start()` — keep that ordering (never read another singleton's `instance` in `Awake()`).

**Enemy AI** (`Scripts/AI/State Stuff/`): a component-based FSM. `State` is the base; concrete states (`WaypointsForSpawner`, `AttackPlayer`, `LoosingPlayer`) are MonoBehaviours on the enemy. `StateManager` ticks the machine via `InvokeRepeating` at ~0.2s with a random per-enemy offset (deliberate perf staggering — updates are not per-frame) and calls `AiVision.PerceptionCheck()` each tick. A static `StateManager.AllManagers` list supports global queries like `IsAnyAIInAttackOrLoosing()`. Each enemy prefab has an `AiReferences` hub component aggregating its parts (health, vision, states, FMOD emitters) with `ResetAll()` used when the pooled enemy respawns.

**Object pooling, not Instantiate/Destroy**: enemies (`GameFlow/EnemiesSpawner`) and spell projectiles (`Items/PoolSpawnableObject`, `Player/Attack/SpellProjectileSpawner`) are pooled and reset via `OnEnable`/explicit Reset methods. New spawned things should follow this pattern.

**Game flow**: `RoundSystem` (singleton) runs a timer and fires a serialized `UnityEvent[]` — one entry per round — plus `LastRoundEvent`. Much of the game's wiring in general goes through Inspector-assigned UnityEvents, so behavior is often not visible in code; check the scene/prefab serialized fields before concluding something is unused.

**Player** (`Scripts/Player/`): CharacterController-based FPS movement in `Movement/` (`PlayerMovement`, `Bhop`, `Dash`, `DynamicFOV`, `CameraBob`, `FootstepSystem`). Combat in `Attack/`: `Attack` (singleton) implements hold-to-cast with a casting-progress slider, spells are `Bullet` prefabs indexed by `selectedBulletIndex`, mana is `Ammo`. Progression: `PlayerHP`, `PlayerStamina`, `PlayerEXP`, `UnlockedSpells`, `SpellSelector`. A separate "angel" healing mechanic lives in `AngelMind`/`AngelHeal`/`AngelHealingMinigame`.

**Input**: the new Input System via `InputActionReference` fields; subscribe to `.action.performed/.canceled` in `OnEnable` and unsubscribe in `OnDisable`. Active Input Handling is "Input System Package" only — never call legacy `UnityEngine.Input`; for one-off KeyCode-style polling (cheats, debug keys) use the `InputCompat` static bridge (`Scripts/Misc/InputCompat.cs`).

**Audio**: all sound goes through FMOD (`StudioEventEmitter`, `EventReference`) — do not use Unity `AudioSource` for new sounds. Changing audio content requires editing the FMOD Studio project and rebuilding banks.

## Gameplay reference

Controls: LMB shoot, RMB heal, WASD move, C stealth, Shift sprint, Space jump. Cheats: ↑+3 unlocks the third spell, ↑+M refills magicka. Cheat/debug scripts (`Test/Cheats/`, `Test/GameSpeedSlider`, `Camera/DebugTestCameraShake`) are wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` — their class shells stay so scene references survive, but the bodies compile only in Editor and development builds.
