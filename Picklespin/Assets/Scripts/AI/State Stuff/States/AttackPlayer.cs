using UnityEngine;
using UnityEngine.AI;
public class AttackPlayer : State
{
    private GameObject player;

    private NavMeshAgent agent;
    public LoosingPlayer loosingPlayer;
    public AiVision aiVision;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        agent = GetComponentInParent<NavMeshAgent>();
    }


    public override State RunCurrentState()
    {
        if (!aiVision.seeingPlayer)
        {
            //Debug.Log("oddaje dzialanie do Loosing");
            loosingPlayer.currentTimedown = loosingPlayer.loosingTimedown;
            return loosingPlayer;
        }
        else
        {
            RunToPlayer();
            return this;
        }

    }

    void RunToPlayer()
    {
        loosingPlayer.lostPlayer = false;
        agent.SetDestination(player.transform.position); //make it refresh not that often
    }

}
