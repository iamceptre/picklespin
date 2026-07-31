using System.Collections.Generic;
using FMODUnity;
using Pathfinding;
using UnityEngine;

// Every part is optional - nothing here may assume one exists.
public class AiReferences : MonoBehaviour
{
    public static List<AiReferences> AllEnemies { get; } = new();

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

    // death events deactivate arbitrary children through the Inspector; pooled reuse
    // restores these snapshots without knowing which ones
    private Transform[] allChildren;
    private bool[] childInitialActive;

    // not all start enabled (the white enemy's eye hitbox), so the prefab state matters
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

        // FMOD's TriggerOnce is once per *object* lifetime, which for a pooled enemy
        // means the death scream never plays again after its first death
        var emitters = GetComponentsInChildren<StudioEventEmitter>(true);
        for (int i = 0; i < emitters.Length; i++) emitters[i].TriggerOnce = false;

        allColliders = GetComponentsInChildren<Collider>(true);
        colliderInitialEnabled = new bool[allColliders.Length];
        for (int i = 0; i < allColliders.Length; i++)
        {
            colliderInitialEnabled[i] = allColliders[i].enabled;
        }
    }

    private void OnEnable() => AllEnemies.Add(this);
    private void OnDisable() => AllEnemies.Remove(this);

    // CharacterController derives from Collider, so the body-block goes with them
    public void DisableAllColliders()
    {
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i]) allColliders[i].enabled = false;
        }
    }

    // called while the enemy is still inactive, so OnEnable sees a clean prefab state
    public void ResetAll()
    {
        // added at runtime by Sanctus' light spell: a reused enemy must come back hostile
        if (TryGetComponent(out ConvertedAlly ally)) ally.Revert();

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
