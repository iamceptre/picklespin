# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Picklespin is a first-person arena spell-caster game built in **Unity 6000.3.20f1** on the **Built-in Render Pipeline** (forward). The repo root contains two projects:

- `Picklespin/` — the Unity project (open this folder in Unity Editor)
- `Picklespin-FMOD/` — the FMOD Studio audio project (`Picklespin.fspro`); built banks land in `Picklespin/Assets/FMODBanks/`

There are no CLI build scripts, tests, or linters — building and playing happens through the Unity Editor. Scenes: `Assets/Scenes/Menu_Main.unity` and `Assets/Scenes/Chruch_Arena.unity` (the "Chruch" misspelling is canonical; don't rename assets to fix it).

**Don't compile-test after changing code.** No `dotnet build`, no CLI compile check — finish the edit and hand it back. The Editor is open and compiles on focus; the user checks it there and will say if something needs fixing.

## Architecture

All game code lives in `Assets/Scripts/`, organized by category and then by feature:

- `AI/` — `Enemies/` (health, vision, references, death, eyes), `Allies/` (converted enemies, helper spirit), `Angel/`, `States/` (the FSM)
- `Player/` — `Movement/` (with `HandFeedback/`), `Attack/` (casting and pooling) with `Attack/Spells/` (the components bolted onto a `Bullet` prefab), `Vitals/` (HP, stamina, magicka and their bars), `Progression/` (exp, unlocks, classes, wishes), `Angel/` (healing and the two choice menus)
- `UI/` — `HUD/`, `Damage/`, `Tips/`, `EndScreen/`, `Animation/` (generic fades, pulses, tweens), `Interaction/` (hover)
- `GameFlow/` — `Rounds/` (round system, spawners, counters, timer triggers), `Portal/` (win gate, escape timer)
- `Environment/` — `Torches/`, `Atmosphere/` (fog, dust, stars, light fluctuation)
- `VFX/` — `Dissolve/`, `Particles/`, `Decals/`
- `Misc/` — the shared statics at the root (`PhiMath`, `InputCompat`, `DevLog`, `PerlinFloat`), plus `Transform/` (look-at, follow, rotate) and `Utility/`
- flat categories: `Items/`, `Audio/`, `Camera/`, `MapInteractions/`, `Menus/`, `Settings/` (with `Sliders/`), `Test/` (with `Cheats/`)

Keep new scripts in the matching folder — no loose scripts at the top level, and none outside `Assets/Scripts/` (`Assets/Editor/` and the generated `Assets/InputSystem/PlayerInputActions.cs` are the exceptions Unity requires). One MonoBehaviour per file, and **the file name must match the class name** or Unity cannot bind the component. Third-party code is in `Assets/Plugins/`: A* Pathfinding Project (enemy navigation), FMOD, DOTween (Demigiant), BeautifulDissolves, VolumetricFog, Camera Shake, Magic Light Probes.

**Rendering: Built-in RP, not URP.** No SRP asset is assigned (`m_CustomRenderPipeline: {fileID: 0}` in both `GraphicsSettings` and `QualitySettings`) and the project contains no `UniversalRenderPipelineAsset`. The URP package (17.3.0) *is* in `manifest.json` but is entirely unused — treat its presence as a trap, not as evidence. Write shaders Built-in style: `CGPROGRAM` + `UnityCG.cginc`, no `"RenderPipeline"="UniversalPipeline"` SubShader tag, no `Packages/com.unity.render-pipelines.*` includes, and Built-in fallbacks (`Legacy Shaders/...`). A URP-tagged SubShader is silently skipped here — the shader compiles with no errors and the material just renders magenta, which is very slow to diagnose. Custom shaders live in `Assets/Shaders/`; `Trail.shader` and `GhostTrail.shader` are the reference examples. Post-processing is Post Processing Stack v2 (`UnityEngine.Rendering.PostProcessing`), not URP Volumes.

**φ-based tuning, where it earns its place.** `Assets/Scripts/Misc/PhiMath.cs` holds the golden-ratio constants and two samplers. Use it only where φ's irrationality does real work: the golden-angle Vogel spiral for spawn scattering and `GoldenSequence` for low-discrepancy "random" picks (both beat `Random` at not clumping/streaking), the non-repeating dual-sine torch flicker, and the player-facing feel constants — `PlayerMovement.cs` is Quake-style velocity physics fully tuned in φ, plus `RecoilMultiplier` and the crit multiplier. **Everywhere else, write plain readable numbers.** An invisible timing, fade duration, AI distance or UI tween is not improved by being a power of φ — it just hides the value. Don't re-φ constants that were deliberately converted to round numbers.

**Singleton pattern everywhere.** ~50 manager/UI scripts expose `public static X instance` (or `Instance`), assigned in `Awake()`. Cross-references to other singletons are grabbed in `Start()` — keep that ordering (never read another singleton's `instance` in `Awake()`).

**Enemy AI** (`Scripts/AI/States/`): a component-based FSM. `State` is the base; concrete states (`WaypointsForSpawner`, `AttackPlayer`, `LoosingPlayer`) are MonoBehaviours on the enemy. `StateManager` ticks the machine via `InvokeRepeating` at ~0.2s with a random per-enemy offset (deliberate perf staggering — updates are not per-frame) and calls `AiVision.PerceptionCheck()` each tick. A static `StateManager.AllManagers` list supports global queries like `IsAnyAIInAttackOrLoosing()`. Each enemy prefab has an `AiReferences` hub component aggregating its parts (health, vision, states, FMOD emitters) with `ResetAll()` used when the pooled enemy respawns.

**Object pooling, not Instantiate/Destroy**: enemies (`GameFlow/EnemiesSpawner`) and spell projectiles (`Items/PoolSpawnableObject`, `Player/Attack/SpellProjectileSpawner`) are pooled and reset via `OnEnable`/explicit Reset methods. New spawned things should follow this pattern.

**Game flow**: `RoundSystem` (singleton) runs a timer and fires a serialized `UnityEvent[]` — one entry per round — plus `LastRoundEvent`. Much of the game's wiring in general goes through Inspector-assigned UnityEvents, so behavior is often not visible in code; check the scene/prefab serialized fields before concluding something is unused.

**Player** (`Scripts/Player/`): CharacterController-based FPS movement in `Movement/` (`PlayerMovement`, `Bhop`, `Dash`, `DynamicFOV`, `CameraBob`, `FootstepSystem`). Combat in `Attack/`: `Attack` (singleton) implements hold-to-cast with a casting-progress slider, spells are `Bullet` prefabs picked by `selectedSpell`, mana is `Ammo`. Every spell identity — the pickup, the bullet, the pools, the cast VFX, the inventory slot — is the shared `SpellId` enum (`Scripts/Player/Attack/SpellId.cs`); the per-spell arrays stay index-based and are subscripted with a single `(int)spell` cast at the boundary, so enum order and array order must match. Vitals are `PlayerHP`, `PlayerStamina`, `Ammo`; progression is `PlayerEXP`, `UnlockedSpells`, `SpellSelector`. A separate "angel" healing mechanic lives in `AngelMind`/`AngelHeal`/`AngelHealingMinigame`. Healing an angel costs 7 mana/s (then 10 HP/s once dry, floored at 10 HP) and ends with `AngelWishMenu` — three randomized upgrade wishes picked with 1/2/3 while movement, attacking and the inventory are disabled. Healing no longer refills the three bars automatically; a full restore is one of the wishes. Buffs that would otherwise have to be written into a prefab (spell damage, cast time, recoil, rocket-jump force, enemy speed) live in the static `WishUpgrades` and are read at the point of use — **never write an upgrade into a prefab field at runtime**, the Editor persists that between sessions.

**Player classes.** The *first* healed angel opens `PlayerClassMenu` before the wish menu — three of six classes drawn at random, picked with 1/2/3, or 4 to refuse; either way it only ever asks once per run, then hands straight over to `AngelWishMenu`. The choice lives in the static `PlayerClasses` (same split as `WishUpgrades`: one-off stat changes go onto the scene singletons when the class is taken, everything prefab-shaped is read at the point of use). The classes are Vesper (no health bar — magicka is doubled and *is* the health pool), Lightfoot (the only class with speed-damage, dash ×3, half health, angels cost double), Umbral (one shared black bar — health, magicka *and* stamina, which spends it only down to the 20% low-magicka line and is winded below that, at half fatigability — no spell inventory, a single spell whose splash needs the bar over half), Blastfool (rocket jumps ×4 force / ×2 self-damage, point-blank only; damage ×0.2 grounded / ×0.7 airborne / ×2 mid rocket-jump, recoil ×0.1), Bastion (double health, piercing shots that hit ×2, ×1.5 spell cooldown, −20% speed, −20% jump) and Sanctus (the light spell converts enemies into allies via `ConvertedAlly` and commands them where it lands; own spells at 25%). **Speed-damage is Lightfoot-only** — for everyone else, and before a class is taken, spells land flat and `SpeedIndicator` is hidden. All the HUD reshuffling is in one place, `PlayerClassHud` (bar roots plus the heart/boot/palm icons, each tinted the colour of the bar its resource currently lives in). Both menus are the same menu asking different questions and share the abstract `AngelChoiceMenu` — canvas fade, numbered lines, digit keys, control lock-out — so a subclass only supplies `SlotCount`, `RollOptions`, `BuildLine` and `OnChosen`.

**Input**: the new Input System via `InputActionReference` fields; subscribe to `.action.performed/.canceled` in `OnEnable` and unsubscribe in `OnDisable`. Active Input Handling is "Input System Package" only — never call legacy `UnityEngine.Input`; for one-off KeyCode-style polling (cheats, debug keys) use the `InputCompat` static bridge (`Scripts/Misc/InputCompat.cs`).

**Logging**: never call `Debug.Log*` directly — go through `DevLog.Info/Warn/Error` (`Scripts/Misc/DevLog.cs`). Its methods are `[Conditional("UNITY_EDITOR")]`/`[Conditional("DEVELOPMENT_BUILD")]`, so in a release build the call sites vanish entirely, arguments included. A shipped build is silent by design.

**Audio**: all sound goes through FMOD (`StudioEventEmitter`, `EventReference`) — do not use Unity `AudioSource` for new sounds. Changing audio content requires editing the FMOD Studio project and rebuilding banks.

## Code style

**No comments in code. None.** Not above a method, not at the end of a line, not a banner, not a `TODO`, not a summary block. Make the code say it instead: name things precisely, pull a confusing expression into a named local or a named constant, split a method that needs narrating. If you catch yourself about to explain something, that is the signal to rename or restructure it, not to write a sentence.

The one exception is `[Tooltip]` on a serialized field — that is Inspector text for whoever is wiring the prefab, not a comment.

**Resolve it once, at startup — cache everything.** Nothing that runs per frame or per FSM tick may allocate or go looking for something. `GetComponent`/`GetComponentInChildren`, any `Find*`, `Camera.main`, `new` on a list/array/string, LINQ, anything that boxes — all of it belongs in `Awake` (this object's own parts) or `Start` (other singletons, per the ordering rule above), stored in a field. Serialize the reference and let `Awake` fill it only if it's empty, the way `AiReferences` does.

In code that ticks: reuse a preallocated buffer instead of building one (`SpellAreaOfEffect`'s static `overlapResults`), compare `sqrMagnitude` when only the ordering matters, and hold `WaitForSeconds` and `Shader.PropertyToID` in `readonly` fields rather than making them fresh each call. When a reference genuinely cannot exist before runtime — a component added on conversion, a pooled instance's owner — resolve it the first time and never look it up again.

## Gameplay reference

Controls: LMB shoot, RMB heal, WASD move, C stealth, Shift sprint, Space jump, 1/2/3 select spell (and pick a wish while the angel's wish menu is open). Cheats: ↑+3 unlocks the third spell, ↑+M refills magicka, typing a class name (`vesper`, `lightfoot`, `umbral`, `blastfool`, `bastion`, `sanctus`) takes that class — `PlayerClassCheat` goes through `PlayerClassMenu.Take`, so it grants exactly what the angel would, and it stays enabled so you can keep switching. Cheat/debug scripts all live in `Test/` (`Test/Cheats/`, `Test/GameSpeedSlider`, `Test/DebugTestCameraShake`, plus `Test/CheatActivatedFeedback` for the on-screen toast) and are wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD` — their class shells stay so scene references survive, but the bodies compile only in Editor and development builds. Each one logs what it offers when it becomes usable, so the Console lists the available cheats on play.
