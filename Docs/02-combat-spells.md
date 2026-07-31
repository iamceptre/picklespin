# 02 — Combat & Spells

**Key files:** `Player/Attack/Attack.cs`, `Player/Attack/Bullet.cs`, `Player/Attack/Ammo.cs`, `SpellProjectileSpawner.cs`, `Player/UnlockedSpells.cs`, `Player/SpellSelector.cs`

## Cast flow

1. **Input** — `Attack` (singleton) subscribes to `primaryAction`/`secondaryAction` (`InputActionReference`) in `OnEnable`/`OnDisable`. Primary = cast selected spell, secondary = heal.
2. **Casting** — spells with `castDuration > 0` are hold-to-cast: a coroutine fills `castingProgress`/`castingSlider`; releasing early fires `CancelCasting`. Casting calls `PlayerMovement.SlowMeDown()` (half speed, sneak audio state) and `SpeedMeBackUp()` when done.
3. **Mana** — `Ammo` (`ammo`/`maxAmmo`, "magicka") is spent per `Attack.CurrentMagickaCost` — `Bullet.magickaCost` × `WishUpgrades.MagickaCostMultiplier`, floored at 1 and read live so a wish applies to the spell already in hand. Not enough mana → `NoManaLightAnimation` + hand-shake feedback.
4. **Spawn** — `SpellProjectileSpawner` (singleton) `Get()`s a pooled `Bullet` for the selected index, positions it at the hand casting point, and `Bullet.OnShoot()` plays particles/sound; `ApplyProjectileForce` launches it.
5. **Cooldown** — per-spell `myCooldown` drives the `SpellCooldown` radial UI. `SpellCooldown.StartCooldown` scales every cooldown by `WishUpgrades.CooldownMultiplier`, so the wish shortens the dash's turn on the shared bar too.

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
  × WishUpgrades.SpellDamageMultiplier(spellName)
  × PlayerClasses.ProjectileDamageMultiplier — 2 for Bastion, 0.25 for Sanctus, 1 otherwise
  × Bullet.flightDamageMultiplier            — Blastfool only: 0.2 grounded / 0.7 airborne / 2 mid rocket jump, sampled at OnShoot
  × PlayerMovement.SpeedDamageMultiplier     — Lightfoot only: 0.25 (standing) → 2.5 (runSpeed·φ), sampled at impact
  → AiHealth.TakeDamage(dmg, eyeshot, crit)
      × bodyDamageMultiplier or eyeDamageMultiplier (per enemy)
      → hp; spawns pooled damage number (DamageUI_Spawner); death via deathEvent
```

`AiHealth.TakeDamage` ignores damage while `RoundSystem.isCounting` is false.

**Speed-damage is a class perk, not a base rule.** Only Lightfoot turns speed into damage; for every other class — and before the first angel offers a class at all — the multiplier is skipped entirely and `SpeedIndicator` is hidden. `Bullet.useSpeedDamageMultiplier` still opts individual spells *out*, it just no longer opts anything in by itself.

### Class hooks in `Bullet`

- **Bastion** — shots land for ×2 and `PlayerClasses.SpellCooldownMultiplier` (1.5) stretches the wait between them, applied in `Attack.SuccesfulShoot` where the spell's own `myCooldown` is read — the dash's turn on the shared bar is not slowed. `PlayerClasses.PiercingProjectiles` sends the shot *through* enemies: `Pierce` damages each enemy once (`piercedThisFlight`), restores the launch velocity so the collision cannot deflect it, and never explodes. The shot dies on world geometry or on the auto-kill timer.
- **Umbral** — `aoeRequiresChargedBar` (per prefab) gates `RangeHitDetection` on `PlayerClasses.ChargedBarReady`, i.e. the shared bar above 50%.
- **Blastfool** — `PlayerClasses.FlightDamageMultiplier` reads `PlayerMovement.IsRocketJumping` / `IsGroundedStable` and gives ×0.2 on foot, ×0.7 airborne, ×2 while still riding their own blast. `Bullet.OnShoot` caches it per shot, so landing before the projectile arrives cannot take the damage back. Their recoil is ×0.1 (`PlayerClasses.RecoilScale`, applied in `RecoilMultiplier`).
- **Sanctus** — `PlayerClasses.LightSpellConverts` turns the light spell's `HitRegistered` into `ConvertedAlly.Convert` instead of damage; a light spell that hit no enemy calls `ConvertedAlly.CommandAll(impactPoint)` and every ally walks there.

### Rocket jumping

`Bullet.ApplyRocketJumpForce` overlap-checks the explosion; rigidbodies get `AddExplosionForce`, the player gets `PlayerMovement.AddExplosionJump(rocketJumpForce · φ², …)` plus self-damage proportional to proximity (self-damage is **not** scaled by the φ² push, nor by `WishUpgrades.RocketJumpForceMultiplier`). Blastfool multiplies the push ×4 and the self-damage ×2, and `PlayerClasses.RocketJumpMinProximity` makes both apply only within the innermost quarter of the blast radius — a far-off explosion does nothing to them at all.

**Cast VFX is optional per spell.** Every array on `PlayCastBlast` (`castBlasts`, `castingParticles`, `castingParticleSizeScript`, `castingSound`, `castingStartSound`) is indexed by spell ID, but a spell does not have to appear in all of them — a missing or short entry is skipped instead of throwing `IndexOutOfRange` on the first shot. Adding a spell is a prefab plus a pool entry; the cast VFX can come later.

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
