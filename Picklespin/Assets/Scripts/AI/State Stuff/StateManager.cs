using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public State currentState;
    public AiVision aiVision;
    public static List<StateManager> AllManagers { get; } = new();
    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;
    float randomTimeOffset;
    float actualRefreshRate;

    void OnEnable() => AllManagers.Add(this);
    void OnDisable() => AllManagers.Remove(this);

    public void StartAI()
    {
        randomTimeOffset = Random.Range(0f, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
        InvokeRepeating(nameof(RunStateMachine), randomTimeOffset, actualRefreshRate);
    }

    void RunStateMachine()
    {
        aiVision.PerceptionCheck();
        State next = currentState ? currentState.RunCurrentState() : null;
        if (next != null) currentState = next;
    }

    public void ResetStateManager()
    {
        CancelInvoke();
        currentState = null;
    }

    public static bool IsAnyAIInAttackOrLoosing()
    {
        if (AllManagers.Count == 0) return false;
        foreach (StateManager m in AllManagers)
        {
            if (m.currentState is AttackPlayer || m.currentState is LoosingPlayer) return true;
        }
        return false;
    }
}
