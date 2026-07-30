using FMODUnity;
using Pathfinding;
using UnityEngine;

// The one reference hub per enemy: damage sources and the spawner ask this
// instead of walking the hierarchy.
//
// Every part is optional. Structural components are auto-resolved in Awake, so
// building a new enemy is a matter of adding or leaving out components — an
// enemy with no SetOnFire simply cannot be set on fire, one with no Dissolver
// does not dissolve. Nothing here may assume a part exists.
//
// The hand-picked assets (the headshot particle, the four FMOD emitters) stay
// manual: there are several of each on an enemy and only the Inspector knows
// which is which.
public class AiReferences : MonoBehaviour
{
    [Header("Auto-found on this object or its children if left empty")]
    public AiHealth Health;
    public AiVision Vision;
    public AiHealthUiBar HpUiBar;
    [Tooltip("when dying")] public GiveExpToPlayer GiveExp;
    public MaterialFlashWhenHit MaterialFlash;
    [Tooltip("optional — an enemy without it cannot be set on fire")]
    public SetOnFire setOnFire;
    public StateManager stateManager;
    public WaypointsForSpawner WaypointsForSpawner;
    public AttackPlayer AttackPlayer;
    public LoosingPlayer LoosingPlayer;
    public Dissolver Dissolver;
    public AIPath aiPath;
    [Tooltip("disabled by the death event, so pooled reuse has to switch it back on")]
    public BarEase HpBarEase;

    [Header("Hand-picked assets")]
    public ParticleSystem HeadshotParticle;
    public StudioEventEmitter damageTakenBig;
    public StudioEventEmitter damageTakenSmall;
    public StudioEventEmitter damageTakenEyeshot;
    public StudioEventEmitter damageTakenCritical;

    // snapshot of every child's initial active state: death events deactivate
    // arbitrary children (eye, eye light, ...) via the Inspector, and pooled
    // reuse must restore all of them without knowing which ones
    private Transform[] allChildren;
    private bool[] childInitialActive;

    // same idea for colliders: a dying enemy switches every one of them off so
    // the player can walk through the corpse, and respawning has to put each
    // back the way the prefab had it (some start disabled, e.g. the white
    // enemy's eye hitbox)
    private Collider[] allColliders;
    private bool[] colliderInitialEnabled;

    private void Awake()
    {
        if (!Health) Health = GetComponentInChildren<AiHealth>(true);
        if (!Vision) Vision = GetComponentInChildren<AiVision>(true);
        if (!HpUiBar) HpUiBar = GetComponentInChildren<AiHealthUiBar>(true);
        if (!GiveExp) GiveExp = GetComponentInChildren<GiveExpToPlayer>(true);
        if (!MaterialFlash) MaterialFlash = GetComponentInChildren<MaterialFlashWhenHit>(true);
        if (!setOnFire) setOnFire = GetComponentInChildren<SetOnFire>(true);
        if (!stateManager) stateManager = GetComponentInChildren<StateManager>(true);
        if (!WaypointsForSpawner) WaypointsForSpawner = GetComponentInChildren<WaypointsForSpawner>(true);
        if (!AttackPlayer) AttackPlayer = GetComponentInChildren<AttackPlayer>(true);
        if (!LoosingPlayer) LoosingPlayer = GetComponentInChildren<LoosingPlayer>(true);
        if (!Dissolver) TryGetComponent(out Dissolver);
        if (!aiPath) TryGetComponent(out aiPath);
        if (!HpBarEase) HpBarEase = GetComponentInChildren<BarEase>(true);

        allChildren = GetComponentsInChildren<Transform>(true);
        childInitialActive = new bool[allChildren.Length];
        for (int i = 0; i < allChildren.Length; i++)
        {
            childInitialActive[i] = allChildren[i].gameObject.activeSelf;
        }

        // FMOD's TriggerOnce means "once per object lifetime", which for a
        // pooled enemy is once per session — the death scream played on an
        // enemy's first death and never again on any reuse. The death chain
        // already guarantees exactly one Play per life (AiHealth.isDead), so
        // the emitter-level latch is pure breakage here.
        var emitters = GetComponentsInChildren<StudioEventEmitter>(true);
        for (int i = 0; i < emitters.Length; i++) emitters[i].TriggerOnce = false;

        allColliders = GetComponentsInChildren<Collider>(true);
        colliderInitialEnabled = new bool[allColliders.Length];
        for (int i = 0; i < allColliders.Length; i++)
        {
            colliderInitialEnabled[i] = allColliders[i].enabled;
        }
    }

    // Called on the first frame of death: the corpse stops blocking the player
    // and stops registering hits while it dissolves. CharacterController derives
    // from Collider, so the thing that actually body-blocks is covered too.
    public void DisableAllColliders()
    {
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i]) allColliders[i].enabled = false;
        }
    }

    // Called by EnemiesSpawner while the enemy is still inactive, so every part
    // is back at its prefab state before OnEnable runs anywhere.
    public void ResetAll()
    {
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i] && allChildren[i].gameObject.activeSelf != childInitialActive[i])
            {
                allChildren[i].gameObject.SetActive(childInitialActive[i]);
            }
        }
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i]) allColliders[i].enabled = colliderInitialEnabled[i];
        }
        if (GiveExp) GiveExp.ResetExpParticles();
        // undo the death event's StopNPCspeed shutdown before anything else
        if (aiPath)
        {
            aiPath.enabled = true;
            aiPath.isStopped = false;
            aiPath.canMove = true;
        }
        if (setOnFire) setOnFire.ResetFireState();
        if (HpUiBar) HpUiBar.ResetBar();
        if (Vision) Vision.ResetVisionState();
        if (Health) Health.ResetHealth();
        if (HpBarEase) HpBarEase.ResetEase(); // after ResetHealth, so it syncs to full HP
        if (MaterialFlash) MaterialFlash.ResetFlashState();
        if (stateManager) stateManager.ResetStateManager();
        if (WaypointsForSpawner) WaypointsForSpawner.ResetWaypointState();
        if (AttackPlayer) AttackPlayer.ResetAttackState();
        if (LoosingPlayer) LoosingPlayer.ResetLoosingState();
        if (Dissolver) Dissolver.ResetDissolveState();
    }
}
