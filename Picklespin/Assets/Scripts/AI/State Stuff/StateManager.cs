using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("AI States")]
    public State currentState;
    public AiVision aiVision;

    [Header("Settings")]
    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;

    float randomTimeOffset;
    float actualRefreshRate;

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
}
