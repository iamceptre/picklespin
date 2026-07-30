using System.Collections.Generic;
using UnityEngine;

// Ticks the enemy FSM on a coarse InvokeRepeating instead of per-frame, with a
// small random per-enemy offset so a wave never re-evaluates on the same frame.
public class StateManager : MonoBehaviour
{
    public State currentState;
    public AiVision aiVision;
    public static List<StateManager> AllManagers { get; } = new();
    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;
    float actualRefreshRate;
    State initialState;

    void Awake()
    {
        initialState = currentState;
        if (!aiVision) aiVision = GetComponentInParent<AiVision>(true);
    }

    void OnEnable() => AllManagers.Add(this);

    void OnDisable()
    {
        AllManagers.Remove(this);
        CancelInvoke(); // a pooled deactivation must never leave a tick scheduled
    }

    public void StartAI()
    {
        CancelInvoke(); // idempotent: calling twice must not double the tick rate
        float randomTimeOffset = Random.Range(0f, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
        InvokeRepeating(nameof(RunStateMachine), randomTimeOffset, actualRefreshRate);
    }

    // Death: stop thinking straight away. The corpse lives on until it has
    // dissolved, and a ticking corpse keeps steering, keeps swinging at the
    // player and keeps answering the awareness queries below.
    public void StopAI()
    {
        CancelInvoke();
        currentState = null;
    }

    void RunStateMachine()
    {
        if (aiVision) aiVision.PerceptionCheck();
        State next = currentState ? currentState.RunCurrentState() : null;
        if (next != null) currentState = next;
    }

    public void ResetStateManager()
    {
        CancelInvoke();
        currentState = initialState; // restore the prefab's starting state for pooled reuse
    }

    public static bool IsAnyAIInAttackOrLoosing()
    {
        foreach (StateManager m in AllManagers)
        {
            if (m.currentState is AttackPlayer || m.currentState is LoosingPlayer) return true;
        }
        return false;
    }
}
