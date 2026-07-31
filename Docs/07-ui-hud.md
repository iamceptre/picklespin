# 07 — UI & HUD

**Key files:** `Assets/Scripts/UI/*`, plus player-side display scripts in `Assets/Scripts/Player/`

## Stat bars

- `HpBarDisplay`, `StaminaBarDisplay`, `AmmoDisplay` (mana), `PlayerBarDisplay` — singleton sliders refreshed by their systems calling `Refresh(...)` (push, not per-frame polling).
- `BarEase` — the "damage ghost" trail: a second slider eases toward the real one (`MoveTowards`), instant on gain, eased on loss. **It only has a chunk to show if the real bar snapped**, and it must not share the fill's colour — two bars the same colour read as one bar lagging (`PlayerClassHud.umbralGhostColor`).

**Bars poll, they are not pushed.** `PlayerBarDisplay.Update` reads its pool's `DisplayFraction` every frame: **losses land instantly, gains slide in** (`gainEaseTime`), and `BarEase` supplies all the delayed motion. Pushing was fine while one system owned one bar, but a class can fold several pools into one — Umbral spends the same bar for magicka, health *and* breath — and the pushes then arrive in a single frame from step and continuous sources at once, each with its own idea of how the bar should move. `Refresh(bool)` and `SetContinuousValue(…)` survive for the call sites and Inspector events; they only decide whether the bar may slide.

`DisplayFraction` is the pool's own view of itself, whole points **plus whatever a continuous drain has taken since the last one** (`Ammo` subtracts both its drain and stamina remainders, `PlayerHP` its drain remainder), so a continuous drain is smooth without anyone pushing sub-point values into the UI.
- `BarLightsAnimation`, `ManaLightAnimation`, `NoManaLightAnimation` — bar glow feedback on gain/spend/insufficient.
- **Low-bar pulsing is per bar, not per system.** `PlayerBarDisplay` already computes its own normalized value on every refresh, so it pushes that straight into a `PulsatingImage` (auto-found on the slider's `fillRect` if the reference is left empty) via `RefreshLowState(fraction, threshold)`. Health, stamina and magicka all warn the player the same way, from one implementation, and no owning system knows the pulse exists. `PulsatingImage.StartPulsating` no-ops while its object is inactive, so a bar the class HUD hides can never throw "coroutine couldn't be started".
- **The threshold is never a second copy in the Inspector** — each bar pulses at the value that already means something in play, read live from its own system, so a wish that raises a pool moves the warning with it:

  | bar | threshold | is exactly |
  |---|---|---|
  | health | `PlayerHP.LowHealthThreshold` | `regenThresholdPercentage` — where the tinnitus, desaturation and regen start |
  | stamina | `PlayerMovement.NoSprintThreshold` | `staminaRecoveryThreshold / maxStamina` — where sprinting and full-power jumps come back |
  | magicka | `Ammo.LowMagickaThreshold` | the 0.2 that `Bullet.RandomizeCritical` reads as "low magicka" (via `Ammo.IsLow`) |

  All three compare strictly below, matching the rules they mirror.

## Damage numbers

`DamageUI_Spawner.Spawn(worldPos, damage, isCritical)` — pooled `DamageUI_V2` instances (doc 05). Crits render gold at 1.6× font size. Numbers show **final** damage (after crit, speed multiplier, and body/eye multipliers). Spawned by `AiHealth.TakeDamage`.

## Player class HUD

`PlayerClassHud` (singleton) is the **only** place the HUD is rebuilt for the player's class — no bar or indicator knows a class exists. It subscribes to the static `PlayerClasses.Changed` in `OnEnable` (and unsubscribes in `OnDisable`; a static event outlives the scene) and applies once in `Start`. Every reference is optional:

- `healthBarRoot` — off for Vesper and Umbral, whose magicka bar *is* their health.
- `staminaBarRoot` — off for Umbral (`PlayerClasses.StaminaSharesMagicka`), who sprints off the magicka bar. That bar's own low pulse doubles as the winded warning: stamina spends stop at `Ammo.LowMagickaThreshold`, the same 20% line the pulse starts at.
- `magickaBarRoot` — never hidden. For Umbral every `Slider` fill under it (the `BarEase` damage ghost included, found once in `Awake`) is repainted with `umbralBarColor`, and the **background goes to the lighter `umbralBackgroundColor`** so a black fill still reads against the empty part. Backgrounds are auto-found from each slider's stock `"Background"` child unless `magickaBarBackgrounds` is wired by hand; the fill, the handle and the bar's frame art are never touched by the auto-find. Original alpha is kept throughout — transparency is part of the bar's look, not a class signal.
- `healthIcon` / `staminaIcon` / `magickaIcon` — heart, boot, palm. Each icon **slides to the row of, and takes the colour of, the bar its resource currently lives in** (authored position and colour = that bar's row and colour). Vesper's heart moves down onto the magicka bar; all three of Umbral's end up on it, black. A row keeps its own icon in place and newcomers stack beside it by `sharedRowOffset` — a bar whose icon left never gains one, so the two cases cannot collide.
- `speedIndicatorRoot` — **hidden for every class but Lightfoot**; nobody else has a speed-damage multiplier to read.
- `TryGetResourceColor(HudResource)` — the same row colour the icons take, handed out to anything else on the HUD that speaks for a resource. `BarLightsAnimation` passes the bar it was asked to flash to `ManaLightAnimation`, whose `+n` / `-n` text is painted that colour on every play (asked each time, never cached — the class is taken mid-run). It returns false when no icon is wired for that row and there is nothing to sample, and the text keeps the colour it was authored in; Umbral's black is handed out regardless, since the bar is black whether or not the palm icon exists.
- `spellInventoryBarRoot` — hidden while `PlayerClasses.LockedSpellIndex >= 0` (Umbral). Wire this to the *visual* bar, not to the object carrying `UnlockedSpells` / `InventoryBarSelectedSpell`.

The low-bar pulse is **not** the HUD's business, and no longer `PlayerHP`'s either — see below.

## Angel menus (`AngelChoiceMenu`)

`AngelWishMenu` and `PlayerClassMenu` are the same menu asking different questions, so everything shared lives in the abstract `AngelChoiceMenu`: the canvas-group fade, the numbered lines, the digit-key polling, the highlight-and-fade, and the movement/attack/inventory lock-out (the digits *are* the inventory keys).

**A line can also be clicked.** The lock-out disables `MouselookXY` and unlocks the cursor (`Confined` + visible) for as long as the menu is up, and `Update` hit-tests the pointer against the option lines with `RectTransformUtility.RectangleContainsScreenPoint` — no EventSystem, no `GraphicRaycaster`, no per-line component, and the lines do not have to be raycast targets. The line under the pointer takes a hover tint halfway to `highlightColor`, cleared before the chosen-line highlight so the two never read alike. Subclasses override four members — `SlotCount`, `RollOptions()`, `BuildLine(slot)`, `OnChosen(slot)` — plus two optional hooks: `AfterChoice()` (the wish menu hands the controls straight back; the class menu holds them until the wish menu has them) and `OnClosed()` (fires after the fade — where the class menu opens the wish menu).

Base-class serialized fields carry `[FormerlySerializedAs]` for the wish menu's old names (`wishCanvas`, `wishCanvasGroup`, `wishGrantedSound`), so the existing scene wiring survives the move.

### Wish class tags

Every wish carries `Limits` — a `(PlayerClassId, int)[]` naming the classes it may be offered to and **how many times each of them may take it per run** (`Unlimited` for refills and flat EXP). A class with no entry never sees that wish; `TimesTaken` is per-catalog-entry and the catalog is rebuilt in `Awake`, so it resets with the run. `PlayerClassId.None` (the player who refused a class) is a tag in its own right and is included everywhere except the single-class wishes. Build the tags with `All(n)`, `AllExcept(n, …)` or `Only(n, …)` rather than writing the array by hand, unless the cap differs per class (only "Lengthen my breath" does).

The tags encode what a class *has*: no stamina bar for Umbral, no health pool for Vesper or Umbral, no rocket-jump wishes for Blastfool (its kit already is one), no enemy-slowing for Lightfoot. "Fill my veins" and "Fill the void" are the same grant under two names — Umbral's bar *is* magicka, it just reads as dark energy.

## Speed indicator

`SpeedIndicator` — drives an optional TMP speed text, an optional ×-multiplier text, and an optional 0–100 `Slider` (forced range; smoothly eased with framerate-independent exponential smoothing, `sliderSmoothing`). Maps `walkSpeed → 0` and `runSpeed·φ → 100`; tints slow→fast colors. All references optional — wire any subset. Only shown for Lightfoot (see above) — every other class shoots for flat damage, so there is no multiplier to read.

## Inventory bar & spells

- `InventoryBarSelectedSpell` — slot highlight (selected = white, others dimmed), number-key bump animation.
- `UnlockedSpells` — unlock light sweep, locked-icon shake, selected-aura badge (shared `PlayBadge` animation helper), locked-slot tints.
- `SpellCooldown` — per-spell cooldown radial. `SelectedSpellDisplay` — current spell readout.
- `CrosshairManager` / `CrosshairRecoilUI` — crosshair show/hide (also used by door interaction) and recoil kick.

## Awareness & guidance

- `EnemyAwarnessUI` — "you are seen" indicator via `AiVision.IsAnyEnemySeeingPlayer()`.
- `Helper_Arrow` + `HelperSpirit` — 3D arrow + pathfinding spirit guiding to the active angel. `Helper_Arrow` owns the pairing, so nothing else talks to the spirit directly. The spirit goes out once the player is inside the angel's room (`isCloseToAngel` → `HideSpiritOnly`) while the arrow keeps pointing, and comes back on the way out if the arrow still has a target (`RestoreSpirit`).
- `TipManager` + `TipDisplay` / `ShowSelectedTip` — indexed contextual tips (`Show(i)`/`Hide(i)`; the door prompt is index 0). `KeyHasBeenSpawnedTip`, `DontKillMePrompt` — event tips.
- `NewRoundDisplayText` — round banner (doc 03). `ShowFPS` — throttled FPS counter (4 Hz, change-detected).

## Menus & settings

`Menus/` (pause, exit, scene load) + top-level settings scripts: `PlayerPrefsSliderManager`, `BulkSettingsManager`, `VolumeSettingLoader`, `SetFullscreen`, `ApplyFpsLimit`/`ShowSliderValueInTextFpsLimit`, `ResetPlayerPrefs`. Settings persist via `PlayerPrefs`; sliders self-describe with `ShowSliderValueInText`.

Final screens: `ExpGatheredDisplayFinalScreen`, `PlayerLevelDisplayFinalScreen`, `AddedEXP_Gui` (in-game EXP popups).

## Conventions for new HUD elements

1. Singleton (`Instance` in `Awake`) if gameplay code must reach it; plain component otherwise.
2. **Push updates** — expose `Refresh()`/`Show()` and call it from the owning system; avoid `Update()` polling. If per-frame is unavoidable, throttle + change-detect (see `ShowFPS`) — string building every frame allocates.
3. UI feedback animations use DOTween; `DOKill()` before starting a new tween on the same target, and kill on disable if the element can be pooled/hidden mid-animation.
4. Optional references (`if (x)` guards) keep components reusable across HUD variants (see `SpeedIndicator`).
5. World-space UI that follows enemies: see `AiHealthUiBar` (detach on death, `ResetBar` on pooled reuse).
