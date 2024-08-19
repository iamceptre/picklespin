using UnityEngine;

public class StateManager : MonoBehaviour
{

     public State currentState;
     public AiVision aiVision;

    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;

    private float randomTimeOffset;
    private float actualRefreshRate;


    public void StartAI()
    {
        randomTimeOffset = Random.Range(0, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
        InvokeRepeating("RunStateMachine", randomTimeOffset, actualRefreshRate);
    }






    private void RunStateMachine()
    {
        aiVision.FieldOfViewCheck();

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
