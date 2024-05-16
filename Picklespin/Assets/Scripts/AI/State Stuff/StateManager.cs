using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StateManager : MonoBehaviour
{

   public State currentState;
   [HideInInspector] public AiVision aiVision;
    //private NavMeshAgent agent;

    [HideInInspector] public float RefreshEveryVarSeconds = 0.2f;

    private float randomTimeOffset;
    private float actualRefreshRate;

    private void Awake()
    {
        aiVision = GetComponent<AiVision>();
        //agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        StartCoroutine(RefreshAI());
        randomTimeOffset = Random.Range(0, 0.05f);
        actualRefreshRate = RefreshEveryVarSeconds + randomTimeOffset;
    }


    private IEnumerator RefreshAI()
    {
        while (true)
        {
            //if(agent.enabled)
            RunStateMachine();
            yield return new WaitForSeconds(actualRefreshRate);
            //Debug.Log("AI Refresh, every " + actualRefreshRate + " secs");
        }
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
