using UnityEngine;

public class StateManager : MonoBehaviour
{

   public State currentState;
   [HideInInspector]public AiVision aiVision;

    [HideInInspector] public float RefreshEveryVarSeconds = 0.25f;
 
    void Start()
    {
      InvokeRepeating("RunStateMachine",0 , RefreshEveryVarSeconds);
        aiVision = GetComponent<AiVision>();
    }

    private void RunStateMachine()
    {

        State nextState = currentState?.RunCurrentState();
        aiVision.FieldOfViewCheck();

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
