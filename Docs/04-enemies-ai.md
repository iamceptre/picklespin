# 04 — Enemies & AI

**Key files:** `Scripts/AI/*`, `Scripts/AI/State Stuff/*`, `AiReferences.cs`, `SpawnInEnemy.cs`, `GameFlow/EnemiesSpawner.cs`

## Anatomy of an enemy prefab

The root object carries most brains; hitboxes are child colliders tagged `NPC_Hitbox` / `Hitbox_Head`.

| Component | Role |
|---|---|
| `AiReferences` | Hub of references (health, vision, states, FMOD hit emitters, dissolver). `ResetAll()` restores everything for pooled reuse. `Bullet` resolves this on hit. |
| `AiHealth` | HP, body/eye damage multipliers, `deathEvent` (UnityEvent, wired in prefab), spawns damage numbers. |
| `AiVision` | Perception: FOV cone + hearing. Static `AllAIs` registry, `IsAnyEnemySeeingPlayer()` powers the awareness UI. |
| `StateManager` + states | The FSM (below). |
| `AIPath` + `AIDestinationSetter` (A*) | Locomotion — arena enemies use the A* Pathfinding Project, not Unity NavMesh. The death event disables `AIPath` via `StopNPCspeed.Stop()`; `AiReferences.ResetAll` re-enables it for pooled reuse, and the spawner calls `IAstarAI.Teleport` after placement so A*'s internal position syncs. |
| `EnemyCounter_PerUnitComponent` | Global alive-count registration (doc 03). |
| `SpawnInEnemy` | Spawn-in presentation: particles + material alpha fade, then `StateManager.StartAI()`. Re-runs on every pooled activation (`OnEnable`). |
| `EvilEntityDeath` | Death orchestration: screen flash, camera shake, health-bar detach/fade, fire cleanup, `Dissolver.StartDissolve()`, extra `deathEvent`. |
| `Dissolver` | Death dissolve shader animation + ash pile; when finished, returns the enemy to its pool (`EnemiesSpawner.TryDespawn`), `Destroy` only as fallback. |
| `MaterialFlashWhenHit` | White/red emission flash on hit/headshot. |
| `AiHealthUiBar` + `CanvasFader` | World-space HP bar; detaches on death, `ResetBar()` re-attaches on reuse. |

## The state machine

`State` is an abstract MonoBehaviour with `State RunCurrentState()` — concrete states are **components on the enemy** (`WaypointWander`, `AttackPlayer`, `LoosingPlayer`), returning either themselves or the next state.

`StateManager` ticks the machine with `InvokeRepeating` every ~0.2 s **plus a random per-enemy offset** — a deliberate optimization staggering all enemies' AI across frames instead of per-frame updates. Each tick also runs `AiVision.PerceptionCheck()`. `StartAI()` begins ticking (called by `SpawnInEnemy` after the fade-in); `ResetStateManager()` cancels it. The static `StateManager.AllManagers` list powers `IsAnyAIInAttackOrLoosing()` (used for combat music/awareness).

State flow: `WaypointsForSpawner` patrols randomized waypoints (`cachedPoint` injected by the spawner) → sees/hears player → `AttackPlayer` (chase + melee at `meleeAttackRange`) → loses sight → `LoosingPlayer` (4 s search) → back to patrol or attack. The prefab's serialized `currentState` is the starting state; `StateManager` caches it in `Awake` and `ResetStateManager` restores it (a pooled enemy must never come back brain-dead with a null state). (`WaypointWander` is a legacy NavMesh-based variant of the patrol state.)

## Perception rules (`AiVision`)

- **Sight:** angle cone (`angle`) + raycast against `obstructionMask` (no distance limit inside the cone besides the ray length).
- **Hearing:** driven by the player's `movementStateForFMOD` — sneak (0) is silent; walk ≤ 15 m; run ≤ 30 m; landing after a long fall ≤ 45 m for 1 s (all enemies get `EnableLandingHearing` via `JumpLandSignals`).
- **Getting hit** reveals the player for 6 s (`HitShowsMePlayer`), timestamp-based.

## Turned enemies (`ConvertedAlly`, Sanctus only)

Sanctus' light spell converts whatever it lands on. `ConvertedAlly` is **added at runtime** by `Bullet` — no prefab knows Sanctus exists, and `AiReferences` therefore cannot auto-find it in `Awake`.

- `Take` stops the FSM (`StateManager.StopAI`), clears `AIDestinationSetter.target` (it would otherwise overwrite the destination every frame from the old waypoint) and takes over `AIPath.destination` on its own `InvokeRepeating` tick, at the same 0.2 s cadence the FSM uses.
- It hunts the nearest entry in the new `AiReferences.AllEnemies` registry that is alive and not itself converted, and strikes for 25 every second inside 3.5 m. A light spell that hit nobody calls `CommandAll(point)`: every ally walks there first, then picks the hunt back up.
- Regular enemies still only hunt the player — they do not fight back.
- **Pooling:** `AiReferences.ResetAll` looks the component up explicitly and calls `Revert()`, so a reused enemy always comes back hostile. Everything `Take` touched is restored by `ResetAll`'s own chain.

## Death & pooled respawn lifecycle

```
AiHealth.hp ≤ 0 → deathEvent → EvilEntityDeath.Die()
  → bar detach/fade, screen flash, shake, SetOnFire cleanup
  → Dissolver.StartDissolve() (swap to dead material, animate, drop ash pile)
  → dissolve done → EnemiesSpawner.TryDespawn(gameObject) → pool.Release (SetActive false)

Next wave → pool.Get → position on golden spiral → inject waypoints
  → AiReferences.ResetAll() (bar reattach, health, vision, states, material flash, dissolve state)
  → SetActive(true) → SpawnInEnemy fade-in → StartAI()
```

If you add per-enemy state (new component with runtime mutation), **add a reset call to `AiReferences.ResetAll()`** or it will leak into the next life.

## Adding a new enemy type

1. Duplicate an existing enemy prefab (keeps hitbox tags, all lifecycle components, and the prefab-wired death events).
2. Adjust `AiHealth` (hp, multipliers), `AiVision` (cone, masks), `NavMeshAgent` (speed), state parameters (`AttackPlayer` damage/range, `WaypointWander.idleSpeed`), visuals + `Dissolver` materials, FMOD emitters on `AiReferences`.
3. New behavior = new `State` component: implement `RunCurrentState()`, add it to the prefab, and route to it from an existing state's transition logic. Give it a `Reset…State()` method and call it from `ResetAll` if it holds state.
4. Spawning: add a field + pool in `EnemiesSpawner` following the `easyPool`/`whitePool` pattern, and expose a `SpawnEnemiesX(int)` method to wire into round events.
5. The counter, awareness UI, damage pipeline, and pooling all work automatically once `AiReferences` and `EnemyCounter_PerUnitComponent` are present.

## The Angel (friendly NPC)

`AngelSpawner` activates one of several placed angels; `AngelMind` runs its logic (locked room door, torches, eye animations). The player heals it via the `AngelHeal` + `AngelHealingMinigame` interaction for EXP and rewards; `HelperSpirit` (A* `AIPath`) guides the player toward the active angel, and `Helper_Arrow` points at it.
