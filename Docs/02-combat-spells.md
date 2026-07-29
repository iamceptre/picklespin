# 02 — Combat & Spells

**Key files:** `Player/Attack/Attack.cs`, `Player/Attack/Bullet.cs`, `Player/Attack/Ammo.cs`, `SpellProjectileSpawner.cs`, `Player/UnlockedSpells.cs`, `Player/SpellSelector.cs`

## Cast flow

1. **Input** — `Attack` (singleton) subscribes to `primaryAction`/`secondaryAction` (`InputActionReference`) in `OnEnable`/`OnDisable`. Primary = cast selected spell, secondary = heal.
2. **Casting** — spells with `castDuration > 0` are hold-to-cast: a coroutine fills `castingProgress`/`castingSlider`; releasing early fires `CancelCasting`. Casting calls `PlayerMovement.SlowMeDown()` (half speed, sneak audio state) and `SpeedMeBackUp()` when done.
3. **Mana** — `Ammo` (`ammo`/`maxAmmo`, "magicka") is spent per `Bullet.magickaCost`. Not enough mana → `NoManaLightAnimation` + hand-shake feedback.
4. **Spawn** — `SpellProjectileSpawner` (singleton) `Get()`s a pooled `Bullet` for the selected index, positions it at the hand casting point, and `Bullet.OnShoot()` plays particles/sound; `ApplyProjectileForce` launches it.
5. **Cooldown** — per-spell `myCooldown` drives the `SpellCooldown` radial UI.

## Bullet lifecycle (pooled)

`Bullet` lives in an `ObjectPool<Bullet>` (per spell type, created in `SpellProjectileSpawner`). `OnEnable` → `ResetBulletState()` + auto-kill timer (`timeBeforeOff`). On hit:

- **Trigger hits** on `Hitbox_Head` / `NPC_Hitbox` tags → `GeneralAfterHit` (head = eyeshot).
- **Collision** with world/enemy body → explosion, decal (`SpellDecalManager`, static geometry on `decalLayerMask` only), rocket-jump check.
- `HitRegistered(AiReferences refs, bool weakPoint)` is the single damage entry point: plays hit sounds, rolls **crit** (10% base, 50% when mana < 20%; crit damage = `originalDamage · φ`), flashes the enemy (`MaterialFlashWhenHit`), applies damage + special effects (`SetOnFire` for the fire spell), and reveals the player to the AI (`AiVision.HitShowsMePlayer`).
- `isRanged` spells also do an `OverlapSphereNonAlloc` area hit (`rangeRadius`, `detectionLayer`).
- Return to pool via `ReturnToPool()` (auto-kill, or `ReturnSpellToPoolAfterExplosion` after the explosion FX finishes). The light spell instead hands control to `LightSpell`, which fades and calls `bullet.ReturnToPool()` when done.

### Damage pipeline

```
Bullet.damage
  × crit (φ if rolled)                       — RandomizeCritical
  × PlayerMovement.SpeedDamageMultiplier     — 0.25 (standing) → 2.5 (runSpeed·φ), sampled at impact
  → AiHealth.TakeDamage(dmg, eyeshot, crit)
      × bodyDamageMultiplier or eyeDamageMultiplier (per enemy)
      → hp; spawns pooled damage number (DamageUI_Spawner); death via deathEvent
```

`AiHealth.TakeDamage` ignores damage while `RoundSystem.isCounting` is false.

### Rocket jumping

`Bullet.ApplyRocketJumpForce` overlap-checks the explosion; rigidbodies get `AddExplosionForce`, the player gets `PlayerMovement.AddExplosionJump(rocketJumpForce · φ², …)` plus self-damage proportional to proximity (self-damage is **not** scaled by the φ² push).

## Adding a new spell — checklist

Spells are indexed by `spellID` = position in every parallel array. Current spells: 0 = purple, 1 = fireball (ranged, sets on fire), 2 = light.

1. **Bullet prefab** — duplicate an existing spell's Bullet prefab. Set on the `Bullet` component: `spellID` (next free index), `spellName`, `damage`, `magickaCost`, `speed`, `myCooldown`, `castDuration` (0 = instant), `timeBeforeOff`, `isRanged`/`rangeRadius` if area, `doesThisSpellSetOnFire`, FMOD `shootSound`/`pullupSound`/explosion emitters, `rocketJumpForce`, explosion FX + light.
2. **`Attack.bulletPrefab`** — add the prefab to the array on the Attack component (index = spellID).
3. **`SpellProjectileSpawner`** — add a pool for the new index. The pools are per-spell (see the existing `bulletPrefab[0..2]` create functions) — extend the same pattern for index 3 and pre-warm it in `PreInstantiate`.
4. **`UnlockedSpells`** — extend each per-spell array by one: `spellUnlocked` (start locked = false), `invSlotRect`, `spellIcon`, `lockedSpellTint`. An editor `OnValidate` warns if lengths mismatch. Add the new slot visuals to the inventory bar canvas and hook them into `InventoryBarSelectedSpell`'s `invSlot`/`invSlotSpellIcon`/`invNumbersRect` arrays.
5. **Selection is automatic** — `SpellSelector` derives spell count from the arrays: digit key 4 and scroll-cycling work with no code changes. Scroll skips locked spells; unlocking adds the spell to the rotation.
6. **Unlock route** — either a `SpellPickupable` item in the world calling `UnlockedSpells.UnlockASpell(spellID)` (duplicate pickup = +50 mana refund), or a cheat (`Test/Cheats/UnlockSpellCheat`).
7. **FMOD** — author the cast/hit events in `Picklespin-FMOD`, build banks.
8. **Special behavior** — hit effects go through `HitRegistered → ApplySpecialEffect`; follow the `SetOnFire` pattern (component on the enemy, enabled by the bullet flag) rather than branching in `Bullet`.

## Related feedback systems

Crosshair (`CrosshairManager`, `CrosshairRecoilUI`), camera shake (`CameraShakeManagerV2.ShakeSelected(index)` — indices are scene-configured shake presets), hand animator (`PublicPlayerHandAnimator`), screen tint (`ScreenFlashTint`), cast blast light (`PlayCastBlast`).
