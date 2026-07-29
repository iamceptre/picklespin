# 07 — UI & HUD

**Key files:** `Assets/Scripts/UI/*`, plus player-side display scripts in `Assets/Scripts/Player/`

## Stat bars

- `HpBarDisplay`, `StaminaBarDisplay`, `AmmoDisplay` (mana), `PlayerBarDisplay` — singleton sliders refreshed by their systems calling `Refresh(...)` (push, not per-frame polling).
- `BarEase` — the "damage ghost" trail: a second slider eases toward the real one (`MoveTowards`), instant on gain, eased on loss.
- `BarLightsAnimation`, `ManaLightAnimation`, `NoManaLightAnimation` — bar glow feedback on gain/spend/insufficient.

## Damage numbers

`DamageUI_Spawner.Spawn(worldPos, damage, isCritical)` — pooled `DamageUI_V2` instances (doc 05). Crits render gold at φ× font size. Numbers show **final** damage (after crit, speed multiplier, and body/eye multipliers). Spawned by `AiHealth.TakeDamage`.

## Speed indicator

`SpeedIndicator` — drives an optional TMP speed text, an optional ×-multiplier text, and an optional 0–100 `Slider` (forced range; smoothly eased with framerate-independent exponential smoothing, `sliderSmoothing`). Maps `walkSpeed → 0` and `runSpeed·φ → 100`; tints slow→fast colors. All references optional — wire any subset.

## Inventory bar & spells

- `InventoryBarSelectedSpell` — slot highlight (selected = white, others dimmed), number-key bump animation.
- `UnlockedSpells` — unlock light sweep, locked-icon shake, selected-aura badge (shared `PlayBadge` animation helper), locked-slot tints.
- `SpellCooldown` — per-spell cooldown radial. `SelectedSpellDisplay` — current spell readout.
- `CrosshairManager` / `CrosshairRecoilUI` — crosshair show/hide (also used by door interaction) and recoil kick.

## Awareness & guidance

- `EnemyAwarnessUI` — "you are seen" indicator via `AiVision.IsAnyEnemySeeingPlayer()`.
- `Helper_Arrow` + `HelperSpirit` — 3D arrow + pathfinding spirit guiding to the active angel.
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
