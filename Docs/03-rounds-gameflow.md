# 03 — Rounds & Game Flow

**Key files:** `GameFlow/RoundSystem.cs`, `EnemyCounter.cs`, `EnemyCounter_PerUnitComponent.cs`, `GameFlow/PauseTimerOnEnter.cs`, `GameFlow/PlayTimerOnlyHere.cs`, `GameFlow/WinGateKeySpawner.cs`, `Items/WinGateKeyItem.cs`, `GameFlow/WinGate.cs`, `GameFlow/EscapeTimer.cs`

## RoundSystem

Singleton on a HUD object (needs a `CanvasGroup` — the timer UI dims to 0.4 alpha while paused). An `Update()`-driven countdown:

- `timer` counts down from `roundDuration` at `Time.deltaTime · speedMultiplier`, driving the `roundTimerGUI` slider.
- At zero → `AdvanceRound()`: fires `RoundEvent[CurrentRound]` (or `LastRoundEvent` when past the array), shows the "Round N begins" banner (`NewRoundDisplayText.Animate()`), increments `CurrentRound`, resets the timer and `speedMultiplier` to 1.

### Controls other systems use

| API | Effect | Used by |
|---|---|---|
| `isCounting = false/true` | Pause/resume the countdown (UI dims). Damage to enemies is also blocked while paused (`AiHealth` checks it). | `PauseTimerOnEnter` / `PlayTimerOnlyHere` trigger zones (e.g. safe rooms) |

**Pausing and resuming are one-way each, by design.** `PauseTimerOnEnter` only ever stops the clock; `PlayTimerOnlyHere` (on the church/arena volume) is the only thing that starts it again. So an angel room — whose `Angel.prefab` carries a `TimerPauseTrigger` — stays paused for the *whole* visit, healing and wish menu included, until the player walks back out into the arena. `PauseTimerOnEnter` used to also resume in `OnDisable`, which restarted the round the instant healing tore the angel's triggers down, with the player still standing in the room.

| `speedMultiplier` | Countdown speed | `EnemyCounter` fast-forwards when arena is cleared |
| `enabled = false` | Stops rounds entirely | `WinGateKeyItem` after the key is picked up |
| `CurrentRound` | Read-only round index | anything |

## Setting up rounds (Inspector workflow)

Rounds are **data, not code** — each round is one entry in `RoundSystem.RoundEvent` (a `UnityEvent[]`):

1. Select the RoundSystem object in `Chruch_Arena`, set **Round Duration** (seconds per round).
2. Size **Round Event** to the number of rounds. Entry *i* fires at the **start of round i+1**.
3. Into each entry, drag scene objects and pick methods. The standard calls:
   - `EnemiesSpawner.SpawnEnemiesEasy(int)` / `SpawnEnemiesWhite(int)` — spawn a wave (staggered 0.2 s apart, golden-spiral scattered).
   - `PickupableBonusesSpawner.SpawnBonuses(int)` — scatter potions.
   - `SpellSpawner.SpawnSpellsLo(int)` — place pickup-able spell unlocks.
   - `WinGateKeySpawner.SpawnWinGateKey()` — place the escape key (typically a late round).
   - `AngelSpawner` activation, one-off `UnityEvent` receivers, FMOD emitters — anything public works.
4. **Last Round Event** fires when the timer expires *after* the final array entry ("You reached the end").

Escalation = bigger ints in later entries. No code changes are ever needed to re-balance the campaign.

## Enemy counting & cleared-arena fast-forward

- Every enemy prefab carries `EnemyCounter_PerUnitComponent`: registers with `EnemyCounter` on enable, deregisters via the death event (`deCountMe`) or on disable (pooled release). Counting is idempotent — double-fires are impossible.
- `EnemyCounter.Deregister()` checks for zero: when the arena is cleared, `RoundSystem.speedMultiplier = clearedArenaTimerSpeed` (5 by default, Inspector-tunable) so the next round arrives quickly. `AdvanceRound` resets it to 1.

## Win condition chain

1. A late round fires `WinGateKeySpawner.SpawnWinGateKey()` — the (single, reused) key object teleports to a random spawn point, activates, and the "key has spawned" tip animates.
2. Player picks up `WinGateKeyItem` → sets `InventoryItemsBank.WinGateKey = true` and **disables RoundSystem** (no more waves).
3. `WinGate` checks the bank; with the key, passing through triggers the win flow (`Misc/Win.cs`, final screens showing EXP/level via `ExpGatheredDisplayFinalScreen` / `PlayerLevelDisplayFinalScreen`).
4. `EscapeTimer` (where used) adds an urgency countdown for the escape phase.

## Related flow pieces

- **Portals** — `OpenPortalParticles`, `PortalAfterClosing`, `Portal_ShrinkInnerRing`: round-wired visual/portal beats.
- **Death/respawn** — `Misc/Death.cs`, `Reborn.cs`, `ReloadCurrentLevel.cs`.
- **Pause** — `Menus/Pause.cs` (timescale + FMOD), `ResetTimeScale` safety on scene load.

## Adding a new round-driven system

Write any component with a public method (parameterless or single-arg), place it in the scene, and wire it into a `RoundEvent` entry. If it spawns repeatedly, pool it (doc 05). If it must react to *every* round instead of one, prefer wiring it into each entry over polling `CurrentRound`.
