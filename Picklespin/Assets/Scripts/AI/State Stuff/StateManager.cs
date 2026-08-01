using System.Collections.Generic;
using UnityEngine;

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
        CancelInvoke();
    }

    public void StartAI()
    {
        CancelInvoke();
        float randomTimeOffset = Random.Range(0f, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
        InvokeRepeating(nameof(RunStateMachine), randomTimeOffset, actualRefreshRate);
    }

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
        currentState = initialState;
    }

    public static void AllLosePlayer()
    {
        foreach (StateManager m in AllManagers)
        {
            m.LosePlayer();
        }
    }

    public void LosePlayer()
    {
        CancelInvoke();
        if (aiVision) aiVision.ResetVisionState();
        if (currentState) currentState = initialState;
    }

    public static bool IsAnyAIInAttackOrLoosing()
    {
        foreach (StateManager m in AllManagers)
        {
            switch (m.currentState)
            {
                case AttackPlayer attack when !attack.HasGrudge: return true;
                case LoosingPlayer loosing when !loosing.HasGrudge: return true;
            }
        }
        return false;
    }
}
