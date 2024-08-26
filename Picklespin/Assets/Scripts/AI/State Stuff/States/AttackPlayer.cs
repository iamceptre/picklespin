using FMODUnity;
using Pathfinding;
using UnityEngine;
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
    [SerializeField] private float meleeAttackRange = 4;
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

        if (destinationSetter.target != playerTransform.PlayerTransform)
        {
            destinationSetter.target = playerTransform.PlayerTransform;
        }


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
        if (Vector3.Distance(transform.position, playerTransform.PlayerTransform.position) < meleeAttackRange)
        {
            attackCounter++;

            if (attackCounter % 2 != 0) {

                 attackSoundEmitter.Play();

                if (!playerHP.isLowHP) {
                    playerHP.TakeDamage(howMuchDamageIdeal);
                }
                else
                {
                    playerHP.TakeDamage((int)(howMuchDamageIdeal * 0.5f));
                }

            }
        }
    }

}
