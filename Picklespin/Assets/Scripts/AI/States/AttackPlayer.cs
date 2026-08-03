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
    [SerializeField, Tooltip("swinging at the player - 2D, it is happening in your face")]
    StudioEventEmitter attackSoundEmitter;
    [SerializeField, Tooltip("swinging at anything but the player - wants a spatialised event, it is happening somewhere in the room. Empty falls back to the one above")]
    StudioEventEmitter attackOtherSoundEmitter;

    [Header("Retaliation")]
    [SerializeField, Tooltip("how long a converted ally stays the target after it lands a hit")]
    float grudgeDuration = 6f;

    int attackCounter;
    public bool canAttack = true;

    AiReferences grudge;
    float grudgeUntilTime;

    public bool HasGrudge => IsLive(grudge);

    AiReferences GrudgeTarget
    {
        get
        {
            if (!IsLive(grudge)) grudge = null;
            return grudge;
        }
    }

    bool IsLive(AiReferences target) =>
        target
        && Time.time <= grudgeUntilTime
        && target.isActiveAndEnabled
        && (!target.Health || target.Health.IsAlive)
        && ConvertedAlly.IsConverted(target);

    public void Retaliate(AiReferences attacker)
    {
        if (!attacker) return;

        grudge = attacker;
        grudgeUntilTime = Time.time + grudgeDuration;
    }

    void Start()
    {
        playerTransform = PublicPlayerTransform.Instance;
        playerHP = PlayerHP.Instance;
    }

    public override State RunCurrentState()
    {

        AiReferences target = GrudgeTarget;
        if (target)
        {
            ChaseAlly(target);
            return this;
        }

        if (!aiVision.seeingPlayer)
        {
            loosingPlayer.StartLoosingState();
            return loosingPlayer;
        }

        ChasePlayer();
        return this;
    }

    void ChasePlayer()
    {
        aiPath.maxSpeed = ChaseSpeed;
        aiPath.rotationSpeed = rotationSpeed;
        if (destinationSetter.target != playerTransform.PlayerTransform)
            destinationSetter.target = playerTransform.PlayerTransform;
        if (canAttack) AttackWhenClose();
    }

    void ChaseAlly(AiReferences target)
    {
        aiPath.maxSpeed = ChaseSpeed;
        aiPath.rotationSpeed = rotationSpeed;
        if (destinationSetter.target != target.transform) destinationSetter.target = target.transform;
        if (canAttack) AttackAllyWhenClose(target);
    }

    float ChaseSpeed =>
        attackSpeed * WishUpgrades.EnemySpeedMultiplier * ConvertedAlly.SpeedMultiplierAt(transform.position);

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

    void AttackAllyWhenClose(AiReferences target)
    {
        if (Vector3.Distance(transform.position, target.transform.position) >= meleeAttackRange) return;

        attackCounter++;
        if (attackCounter % 2 == 0) return;

        PlayAttackOnOther();
        if (target.MaterialFlash) target.MaterialFlash.Flash();
        if (target.Health) target.Health.TakeQuietDamage(howMuchDamageIdeal);
    }

    public void PlayAttackOnOther()
    {
        StudioEventEmitter emitter = attackOtherSoundEmitter ? attackOtherSoundEmitter : attackSoundEmitter;
        if (emitter) emitter.Play();
    }

    public void SetCanAttack(bool state)
    {
        canAttack = state;
    }

    public void ResetAttackState()
    {
        attackCounter = 0;
        canAttack = true;
        grudge = null;
    }
}
