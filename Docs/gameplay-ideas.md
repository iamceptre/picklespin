# Gameplay Ideas — improvement backlog

Not documentation — a working list of design ideas, roughly ordered by value-for-effort within each section. Each idea names the existing systems it would hook into so it can be scoped quickly. Delete entries that get built or rejected.

The guiding thesis for all of it: **the game's identity is "move fast, hit hard" — the speed-damage multiplier, apex accuracy, and bhop already exist, but the player can't *see* any of it.** Legibility first, content second, systems third.

---

## 1. Make the core loop visible (highest value, lowest cost)

### 1.1 Crosshair / HUD reacts to the damage multiplier
`PlayerMovement.SpeedDamageMultiplier` already swings ×0.25–×2. Tint or scale the crosshair (or extend `SpeedIndicator`) with it so "I'm glowing = I hit harder" is learned in the first minute without a tutorial.

### 1.2 Damage numbers styled by multiplier tier
`DamageUI_V2` popups already exist. Dim/gray at low multiplier, big/gold at ×2, distinct style for φ-crits. Add an FMOD parameter to pitch hit sounds up with the multiplier (goes through the existing emitter setup, see doc 08).

### 1.3 Telegraph the apex window
`RecoilMultiplier` gives perfect accuracy near the jump apex — invisible today. A subtle crosshair contraction while inside the apex window teaches it for free.

### 1.4 Style/score meter
Kills-while-fast, airshots, apex kills feed a combo meter with an end-screen grade. Gives the movement system a scoreboard to perform for. Hooks: `GiveExpToPlayer` (already knows headshots), `PlayerMovement` speed, `RoundSystem` end events.

---

## 2. Teach the movement

### 2.1 Practice room (single highest-leverage content add)
A tiny scene: speed gauge, damage dummy showing live multiplier numbers, a "reach ×2" gate. Bhop is niche knowledge — right now it lives in `CONTROLS.txt` and nowhere else. Reuse `SpeedIndicator`, `DamageUI_V2`, a stripped `AiHealth` dummy that never dies.

### 2.2 Contextual tips
The tips UI (doc 07) can react: player has been below ×0.5 multiplier for a whole round → show a "speed = damage" hint once.

---

## 3. Combat variety

### 3.1 A zoner enemy that punishes standing still
Both current enemy types are chasers, so optimal play degenerates into circling. An artillery-type that area-denies your last position forces route improvisation — the thing bhop is *for*. Fits the existing FSM (`State` + `StateManager`, doc 04); its "attack" state targets a position, not the player.

### 3.2 An enemy that punishes predictable circling
Something that leads its shots or cuts corners. Cheap version: reuse `AiVision` and aim at `player position + velocity × leadTime` (velocity is already on `PlayerMovement`).

### 3.3 Per-spell identity via the multiplier toggle
`Bullet.useSpeedDamageMultiplier` now exists (fireball is flat). Use it deliberately: flat utility spells vs. speed-scaled "payoff" spells, so loadout choice = playstyle choice.

### 3.4 A movement spell
One spell that's *primarily* mobility (blink / bounce-pad / pull) with damage as a side effect. Rocket-jumping already blurs weapon/movement — lean in. Slots into `selectedBulletIndex` + `UnlockedSpells` like any spell (doc 02 "adding a new spell").

---

## 4. Pacing & meta

### 4.1 Between-round choice
`RoundSystem` fires hand-authored `UnityEvent[]` waves but never asks the player anything. Even a binary pick ("+max mana or +dash charge") between rounds adds run identity for very little code, reusing `PlayerEXP`/`UnlockedSpells`.

### 4.2 Resolve the healing/movement tension
If angel healing means stopping, every heal is a "stop playing the fun way" tax. Options: heal-over-time that ticks faster above a speed threshold, or keep `AngelHealingMinigame` active while airborne. Reward staying in flow, same as apex accuracy.

### 4.3 Intensity curve per round
Rounds are authored — use that: alternate pressure rounds and breather rounds, and drive an FMOD music-intensity parameter from round state or player speed (`AudioSnapshotManager` already manages snapshots).

---

## 5. Content

### 5.1 Second arena
One combat scene makes every run visually identical. Even a geometry remix of Church_Arena with different lighting/fog (VolumetricFog + torch systems already exist) buys perceived content cheaply. Design around closed circuits where bhop speed can be maintained; add verticality for apex play.

### 5.2 Arena mutators
Cheaper than new arenas: per-round or per-run modifiers (low gravity round, fog round, torch-out darkness round) wired through the same round `UnityEvent[]`.

---

## 6. Options & accessibility (cheap now, expected by players)

- **Key rebinding** — nearly free now that input is fully on the Input System (`PerformInteractiveRebinding` + the existing `PlayerInputActions`).
- **FOV slider** — feed `DynamicFOV`'s base value; a fast game at locked FOV makes people sick.
- **Camera bob / shake intensity toggles** — `CameraBob` and `CameraShakeManagerV2` are already centralized; expose 0–1 scalars in `Settings/` (PlayerPrefs pattern exists).

---

## 7. Structural (not urgent, notes to future self)

- Singleton `Start()` grabs fail silently when a scene is missing a manager (see the `MouselookXY_old` NPE). A null-check-and-log helper would turn those into readable errors.
- A CI job running the `dotnet build Assembly-CSharp.csproj` compile check would catch broken refactors before they land.
- Decide what persists between runs (currently PlayerPrefs settings only): arcade high-score table vs. light roguelite meta. §1.4's style score is the natural high-score currency.
