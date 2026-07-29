# 06 — Items & Pickups

**Key files:** `Assets/Scripts/Items/*`, `Player/InventoryItemsBank.cs`

## Pickup types

| Item | Script | Effect |
|---|---|---|
| HP orb | `Pickupable_HP` | `PlayerHP.ModifyHP(+n)` |
| Mana orb | `Pickupable_Mana` | `Ammo.GiveManaToPlayer` |
| Stamina orb | `Pickupable_Stamina` | `PlayerMovement.GiveStaminaToPlayer` |
| Potion (typed) | `Pickupable_Potion` | HP/Mana/Stamina by enum; refuses when the stat is full (`TryGiveResource`) |
| Spell unlock | `SpellPickupable` | `UnlockedSpells.UnlockASpell(spellID)`; duplicate → +50 mana |
| Win-gate key | `WinGateKeyItem` | Sets `InventoryItemsBank.WinGateKey`, disables `RoundSystem` |

Shared presentation: `Pickupable_Item` (float/bob animation via `StartFloating`), `GrowOnEnable` (pop-in scale), `ItemAfterPickingUp` (pickup feedback + release/destroy), `HandBopAfterItemPickup` (hand animation).

## Spawner architecture

`PickupableBonusesSpawner` (potions/orbs) and `SpellSpawner` (spell unlocks) share a pattern:

- `ObjectPool<T>` pre-warmed to the spawn-point count (see doc 05).
- **Spawn-point occupancy:** `isSpawnPointTaken[]` per point plus `avaliableSpawnPointsCount`. Spawning re-rolls until it lands on a free point; the spawned item is told its point index (`SetOccupiedWaypoint`) and frees it when collected.
- Round events call `SpawnBonuses(int)` / `SpawnSpellsLo(int)` (doc 03); spawns scatter over time (0.05–0.1 s stagger).
- Counts self-clamp to available points; both `Debug.LogWarning("No available spawn points.")` when exhausted — if you see that, add spawn points or lower the round's numbers.

## Adding a new pickup type

1. Duplicate the closest prefab (potion for consumables). Its root needs: `PoolSpawnableObject` (pool plumbing + occupancy), `Pickupable_Item` (float), your new effect script with the trigger/pickup logic.
2. Effect scripts follow the pattern: grab singletons in `Start` (`PlayerHP.Instance`, `Ammo.instance`, `PlayerMovement.Instance`…), apply the effect on player trigger, then release to pool.
3. Add the prefab to `PickupableBonusesSpawner.bonuses` — it enters the random rotation automatically (pool `CreateItem` picks a random prefab from the array).
4. For a *guaranteed* (non-random) drop, give it its own spawner following `WinGateKeySpawner` (single reused instance) instead.

## InventoryItemsBank

Deliberately minimal singleton: boolean flags for story items (currently only `WinGateKey`). Quest-item state lives here so `WinGate` and UI can query one place. Add new flags as public bools — don't build a general inventory unless the game actually needs one.
