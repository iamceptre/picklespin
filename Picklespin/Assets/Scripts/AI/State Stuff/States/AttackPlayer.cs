using FMODUnity;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
public class AttackPlayer : State
{
    private PlayerHP playerHP;
    private HpBarDisplay hpBarDisplay;
    private PublicPlayerTransform playerTransform;

    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private AIPath aiPath;

    [SerializeField] private AiVision aiVision;
    [SerializeField] private LoosingPlayer loosingPlayer;

    [SerializeField] private float attackSpeed = 8;
    [SerializeField] private int howMuchDamageIdeal = 10;
    [SerializeField] private StudioEventEmitter attackSoundEmitter;

    private int attackCounter = 0;

    public bool canAttack = true;

    private void Start()
    {
        playerTransform = PublicPlayerTransform.instance;
        playerHP = PlayerHP.instance;
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
        aiPath.maxSpeed = attackSpeed;
        loosingPlayer.lostPlayer = false;
        destinationSetter.target = playerTransform.PlayerTransform;

        if (canAttack)
        {
            AttackWhenClose();
        }
    }



    public void SetCanAttack(bool state)
    {
        canAttack = state;
    }

    void AttackWhenClose()
    {
        if (Vector3.Distance(transform.position, playerTransform.PlayerTransform.position) < 5)
        {
            attackCounter++;

            if (attackCounter % 2 != 0) {
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
