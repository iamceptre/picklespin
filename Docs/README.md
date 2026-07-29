# Picklespin — System Documentation

First-person arena spell-caster. Unity 6 (6000.0.x), URP, FMOD, A* Pathfinding + Unity NavMesh, DOTween.

| Doc | Covers |
|---|---|
| [01 — Player Movement](01-player-movement.md) | Quake-style physics, bhop, slopes, speed-damage, tuning |
| [02 — Combat & Spells](02-combat-spells.md) | Attack flow, Bullet lifecycle, damage pipeline, **adding a new spell** |
| [03 — Rounds & Game Flow](03-rounds-gameflow.md) | RoundSystem, **setting up rounds**, enemy counting, win condition |
| [04 — Enemies & AI](04-enemies-ai.md) | State machine, vision/hearing, death & respawn, **adding a new enemy** |
| [05 — Spawning & Pooling](05-spawning-pooling.md) | Every pool in the game, **how to pool a new object** |
| [06 — Items & Pickups](06-items-pickups.md) | Potions, spell pickups, key item, spawn-point occupancy |
| [07 — UI & HUD](07-ui-hud.md) | Bars, damage numbers, speed indicator, inventory bar, tips |
| [08 — Audio (FMOD)](08-audio-fmod.md) | Banks, emitters, global parameters, snapshots |
| [09 — Effects & Visuals](09-effects-visuals.md) | Dissolve, torch flicker, camera shake, screen tint |
| [10 — Conventions & Gotchas](10-conventions.md) | φ-math rules, singleton pattern, serialization traps |

## Project layout

- `Picklespin/` — the Unity project. Open this folder in Unity Hub.
- `Picklespin-FMOD/` — FMOD Studio project (`Picklespin.fspro`). Built banks land in `Picklespin/Assets/FMODBanks/`.
- Scenes: `Assets/Scenes/Menu_Main.unity` and `Assets/Scenes/Chruch_Arena.unity` (the "Chruch" spelling is canonical — don't rename).
- All gameplay code: `Assets/Scripts/`. Third-party: `Assets/Plugins/`.

## The 60-second architecture

The **player** (CharacterController + `PlayerMovement`) casts **spells** (`Attack` → pooled `Bullet` projectiles) at **enemies** (NavMeshAgent + component-based state machine ticked by `StateManager`). The **`RoundSystem`** fires one scene-wired `UnityEvent` per round, which triggers **spawners** (enemies, potions, pickup-able spells, the win-gate key). Everything that spawns repeatedly is **object-pooled**. Cross-cutting rules: singletons everywhere (`X.instance` assigned in `Awake`, consumed in `Start`), FMOD for all audio, and all tuning constants derive from φ via `PhiMath` (see doc 10).

Much of the game's wiring lives in **Inspector-assigned UnityEvents** (round events, death events, pickup events) — when tracing behavior, check the scene/prefab serialized fields, not just code.
