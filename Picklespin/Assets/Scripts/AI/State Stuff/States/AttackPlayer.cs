using FMODUnity;
using Pathfinding;
using UnityEngine;

public class AttackPlayer : State
{
    [Header("References")]
    [SerializeField] AIDestinationSetter destinationSetter;
    [SerializeField] AIPath aiPath;
    [SerializeField] AiVision aiVision;
    [SerializeField] LoosingPlayer loosingPlayer;

    PlayerHP playerHP;
    PublicPlayerTransform playerTransform;

    [Header("Attack Settings")]
    [SerializeField] float attackSpeed = 8f;
    [SerializeField] float rotationSpeed = 300f;
    [SerializeField] int howMuchDamageIdeal = 10;
    [SerializeField] float meleeAttackRange = 4f;
    [SerializeField] StudioEventEmitter attackSoundEmitter;

    int attackCounter;
    public bool canAttack = true;

    void Start()
    {
        playerTransform = PublicPlayerTransform.Instance;
        playerHP = PlayerHP.instance;
    }

    public override State RunCurrentState()
    {
        if (!aiVision.seeingPlayer)
        {
            loosingPlayer.currentTimedown = loosingPlayer.loosingTimedown;
            return loosingPlayer;
        }
        ChasePlayer();
        return this;
    }

    void ChasePlayer()
    {
        aiPath.maxSpeed = attackSpeed;
        aiPath.rotationSpeed = rotationSpeed;
        loosingPlayer.lostPlayer = false;
        if (destinationSetter.target != playerTransform.PlayerTransform)
            destinationSetter.target = playerTransform.PlayerTransform;
        if (canAttack) AttackWhenClose();
    }

    void AttackWhenClose()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.PlayerTransform.position);
        if (dist < meleeAttackRange)
        {
            attackCounter++;
            if (attackCounter % 2 != 0)
            {
                attackSoundEmitter.Play();
                if (!playerHP.isLowHP) playerHP.ModifyHP(-howMuchDamageIdeal);
                else playerHP.ModifyHP((int)(-howMuchDamageIdeal * 0.5f));
            }
        }
    }

    public void SetCanAttack(bool state)
    {
        canAttack = state;
    }

    public void ResetAttackState()
    {
        attackCounter = 0;
        canAttack = true;
    }
}
