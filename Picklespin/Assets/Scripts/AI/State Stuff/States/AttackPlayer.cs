using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class AttackPlayer : State
{
    private PlayerHP playerHP;
    private HpBarDisplay hpBarDisplay;
    private PublicPlayerTransform playerTransform;

    private NavMeshAgent agent;
    [SerializeField] private LoosingPlayer loosingPlayer;
    private AiVision aiVision;

    [SerializeField] private float attackSpeed = 8;

    private int howMuchDamageIdeal = 10;

    [SerializeField] private StudioEventEmitter attackSoundEmitter;

    private int attackCounter = 0;

    private void Start()
    {
        playerTransform = PublicPlayerTransform.instance;
        playerHP = PlayerHP.instance;
        aiVision = GetComponentInParent<AiVision>();
        agent = GetComponentInParent<NavMeshAgent>();
        hpBarDisplay = HpBarDisplay.instance;
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
        StopAllCoroutines();
        agent.speed = attackSpeed;
        loosingPlayer.lostPlayer = false;
        agent.SetDestination(playerTransform.PlayerTransform.position);
        AttackWhenClose();
    }

    void AttackWhenClose()
    {
        if (Vector3.Distance(transform.position, playerTransform.PlayerTransform.position) < 5)
        {
            attackCounter++;

            if (attackCounter % 2 == 0) {
                if (!playerHP.isLowHP) {
                    playerHP.hp -= howMuchDamageIdeal;
                }
                else
                {
                    playerHP.hp -= (int)(howMuchDamageIdeal * 0.5f);
                }

                attackSoundEmitter.Play();
                hpBarDisplay.Refresh(false);
                playerHP.PlayerHurtSound();
            }
        }
    }

}
