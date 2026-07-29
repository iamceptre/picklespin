# 05 — Spawning & Pooling

**Rule: nothing that spawns repeatedly during gameplay uses `Instantiate`/`Destroy` per use.** Everything below runs on `UnityEngine.Pool.ObjectPool<T>` or single-object reuse.

## Pool inventory

| What | Pool owner | Release path |
|---|---|---|
| Spell projectiles (`Bullet`) | `SpellProjectileSpawner` — one pool per spell index, pre-warmed | `Bullet.ReturnToPool()` (auto-kill timer, post-explosion, or `LightSpell.Die`) |
| Enemies (easy + white) | `EnemiesSpawner` — one pool per prefab + instance→pool registry | `Dissolver` → `EnemiesSpawner.TryDespawn(go)` |
| Damage numbers (`DamageUI_V2`) | `DamageUI_Spawner` (16 warm / 64 max) | Releases itself after fade |
| Potions/bonuses (`PoolSpawnableObject`) | `PickupableBonusesSpawner`, pre-warmed to spawn-point count | `SetPool`-injected pool on pickup/timeout |
| Pickup-able spells (`SpellPickupable`) | `SpellSpawner`, pre-warmed | same pattern |
| Win-gate key | Single object, teleported + `SetActive` | deactivate |
| Spell decals | `SpellDecalManager` | internal |

## The house pattern

```csharp
// owner
pool = new ObjectPool<Thing>(CreateItem, OnGet, OnRelease, OnDestroyItem, false, warmCount, maxCount);

Thing CreateItem() {
    Thing t = Instantiate(prefab);
    t.SetPool(pool);          // pooled object stores its own way home
    return t;
}
void OnGet(Thing t)     => t.gameObject.SetActive(true);
void OnRelease(Thing t) => t.gameObject.SetActive(false);
void OnDestroyItem(Thing t) => Destroy(t.gameObject);
```

The pooled object calls `pool.Release(this)` when done (with a `Destroy` fallback if no pool was injected, so prefabs still work standalone).

**Enemy variant:** enemies need positioning + state reset *before* activation, so `EnemiesSpawner` uses `actionOnGet: null` and does `Get → position → inject waypoints → ResetAll() → SetActive(true) → IAstarAI.Teleport()` manually (the Teleport syncs A*'s internal position, which otherwise remembers where the enemy died). Because `Dissolver` (not the spawner) decides when the enemy is truly gone, the spawner keeps a `Dictionary<GameObject, ObjectPool<GameObject>>` registry and exposes static `TryDespawn`.

**Hard-won ordering rule:** `ResetAll` runs while the object is still *inactive*, before its components' `Start` has ever run on first spawn. Therefore any value a `Reset…()` method depends on (like `AiHealth.defaultHP` or `StateManager.initialState`) **must be captured in `Awake`, never `Start`** — and anything a death event disables (like `StopNPCspeed` turning off `AIPath`) must be explicitly re-enabled in `ResetAll`.

## Pooling checklist for new spawnables

Reuse is where pooling bugs live — everything must behave correctly on the *second* activation:

1. **One-shot init goes in `Awake`** (runs once); **per-use init goes in `OnEnable`** or an explicit `Reset…()` method. Never in `Start` (runs once, and *after* the first `OnEnable`).
2. Kill tweens/coroutines on release (`DOKill`, `StopAllCoroutines`) — DOTween keeps running on disabled objects.
3. Restore anything mutated during use: swapped materials (`Dissolver.ResetDissolveState`), detached children (health bar `ResetBar`, `QuickLightFadeOut` re-attach), scale/color/alpha, counters.
4. Never `Destroy` a pooled object or its children — detach/hide and restore instead.
5. Capture "original" values **once in `Awake`**, not in `OnEnable` (or reuse re-captures mutated values — the damage-number font-size bug).
6. If other systems hold references to the instance (registries, `AllAIs`-style lists), pair add/remove in `OnEnable`/`OnDisable` and make counting idempotent (see `EnemyCounter_PerUnitComponent`).

## Spawn placement math (φ)

- **Golden-spiral scatter** (`PhiMath.GoldenSpiralPoint(i, n, radius)`): Vogel/sunflower packing — wave members mathematically cannot clump, unlike `Random.insideUnitSphere`.
- **Golden-sequence selection** (`PhiMath.GoldenSequence(i)`): low-discrepancy 0..1 sequence for picking spawn points — feels random, never streaks the same point.
- Potion/spell spawners instead use an **occupancy system**: `isSpawnPointTaken[]` + `avaliableSpawnPointsCount`, re-rolling until a free point is found; items free their point on pickup (`SetOccupiedWaypoint`).

## Known non-pooled one-shots (accepted)

Menu/level-transition objects, `ExplosionScaleTween` (self-destroying one-shot), UI `TextFadeOutAndDestoryOnAwake`, and `SpelledLightSpawner` (legacy path; the live light-spell flow goes through the pooled bullet). If any of these start firing frequently, pool them using the checklist above.
