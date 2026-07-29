using FMODUnity;
using Pathfinding;
using UnityEngine;

public class AiReferences : MonoBehaviour
{

    public AiHealth Health;
    public AiVision Vision;
    public AiHealthUiBar HpUiBar;
    [Tooltip("when dying")]public GiveExpToPlayer GiveExp;
    public MaterialFlashWhenHit MaterialFlash;
    public ParticleSystem HeadshotParticle;
    public SetOnFire setOnFire;
    public StateManager stateManager;
    public WaypointsForSpawner WaypointsForSpawner;
    public AttackPlayer AttackPlayer;
    public LoosingPlayer LoosingPlayer;

    public StudioEventEmitter damageTakenBig;
    public StudioEventEmitter damageTakenSmall;
    public StudioEventEmitter damageTakenEyeshot;
    public StudioEventEmitter damageTakenCritical;

    [Tooltip("auto-found on this object if left empty")]
    public Dissolver Dissolver;
    [Tooltip("auto-found on this object if left empty")]
    public AIPath aiPath;

    // snapshot of every child's initial active state: death events deactivate
    // arbitrary children (eye, eye light, ...) via the Inspector, and pooled
    // reuse must restore all of them without knowing which ones
    private Transform[] allChildren;
    private bool[] childInitialActive;

    private void Awake()
    {
        if (!Dissolver) TryGetComponent(out Dissolver);
        if (!aiPath) TryGetComponent(out aiPath);

        allChildren = GetComponentsInChildren<Transform>(true);
        childInitialActive = new bool[allChildren.Length];
        for (int i = 0; i < allChildren.Length; i++)
        {
            childInitialActive[i] = allChildren[i].gameObject.activeSelf;
        }
    }

    public void ResetAll()
    {
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i] && allChildren[i].gameObject.activeSelf != childInitialActive[i])
            {
                allChildren[i].gameObject.SetActive(childInitialActive[i]);
            }
        }
        if (GiveExp) GiveExp.ResetExpParticles();
        // undo the death event's StopNPCspeed shutdown before anything else
        if (aiPath)
        {
            aiPath.enabled = true;
            aiPath.isStopped = false;
            aiPath.canMove = true;
        }
        if (HpUiBar) HpUiBar.ResetBar();
        Vision.ResetVisionState();
        Health.ResetHealth();
        MaterialFlash.ResetFlashState();
        stateManager.ResetStateManager();
        WaypointsForSpawner.ResetWaypointState();
        AttackPlayer.ResetAttackState();
        LoosingPlayer.ResetLoosingState();
        if (Dissolver) Dissolver.ResetDissolveState();
    }
}

