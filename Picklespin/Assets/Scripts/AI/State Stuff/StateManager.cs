using System.Collections;
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
            yield return new WaitForSeconds(actualRefreshRate);

            //Debug.Log("AI Refresh, every " + actualRefreshRate + " secs");
            RunStateMachine();
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

    public void KillMyBrain()
    {
        Destroy(this);
    }

}
