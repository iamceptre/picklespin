using UnityEngine;

public class StateManager : MonoBehaviour
{

     public State currentState;
    [HideInInspector] public AiVision aiVision;

    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;

    private float randomTimeOffset;
    private float actualRefreshRate;

    private void Awake()
    {
        aiVision = GetComponent<AiVision>();
    }


    public void StartAI()
    {
        randomTimeOffset = Random.Range(0, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
        InvokeRepeating("Do", randomTimeOffset, actualRefreshRate);
    }


    private void Do()
    {
        aiVision.FieldOfViewCheck();
        RunStateMachine();
        //Debug.Log("AI Refresh, every " + actualRefreshRate + " secs");
    }



    private void RunStateMachine()
    {
            State nextState = currentState?.RunCurrentState();


            if (nextState != null)
            {
                SwitchToTheNextState(nextState);
            }
        
    }

    private void SwitchToTheNextState(State nextState)
    {
        currentState = nextState;    
    }

}
