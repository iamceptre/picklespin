using UnityEngine;
using UnityEngine.AI;
public class AttackPlayer : State
{
    private GameObject player;

    private NavMeshAgent agent;
    [SerializeField] private LoosingPlayer loosingPlayer;
    private AiVision aiVision;

    [SerializeField] private float attackSpeed = 8;

    private void Start()
    {
        aiVision = GetComponentInParent<AiVision>();
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
        agent.speed = attackSpeed;
        loosingPlayer.lostPlayer = false;
        agent.SetDestination(player.transform.position); //make it refresh not that often
    }

}
