using System.Collections;
using UnityEngine;

public class StateManager : MonoBehaviour
{

   public State currentState;
   [HideInInspector] public AiVision aiVision;
    public AiHealth aiHealth;

    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;

    private void Awake()
    {
        aiVision = GetComponent<AiVision>();
    }
    void Start()
    {
        StartCoroutine(RefreshAI());
    }


    private IEnumerator RefreshAI()
    {
        while (true)
        {
            yield return new WaitForSeconds(RefreshEveryVarSeconds);
            RunStateMachine();
        }
    }

    private void RunStateMachine()
    {
        if (aiHealth.hp > 0)
        {
            State nextState = currentState?.RunCurrentState();
            aiVision.FieldOfViewCheck();


            if (nextState != null)
            {
                SwitchToTheNextState(nextState);
            }
        }
    }

    private void SwitchToTheNextState(State nextState)
    {
        currentState = nextState;    
    }

}
