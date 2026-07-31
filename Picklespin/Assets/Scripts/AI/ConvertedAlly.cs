using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

// Added at runtime by Bullet, so AiReferences cannot auto-find it: ResetAll looks it
// up explicitly and calls Revert, or a pooled enemy would come back on your side.
public class ConvertedAlly : MonoBehaviour
{
    private static readonly List<ConvertedAlly> active = new();

    private const float TickInterval = 0.2f;
    private const float ChaseSpeed = 9f;
    private const float RotationSpeed = 300f;
    private const float StrikeRange = 3.5f;
    private const float StrikeInterval = 1f;
    private const int StrikeDamage = 25;
    private const float CommandArrivalDistance = 3f;

    private AiReferences refs;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    private Vector3 commandedPoint;
    private bool hasCommandedPoint;
    private float nextStrikeTime;

    public bool IsActive => enabled && refs;

    public static void Convert(AiReferences target)
    {
        if (!target || !target.isActiveAndEnabled) return;
        if (target.Health && !target.Health.IsAlive) return;

        if (!target.TryGetComponent(out ConvertedAlly ally))
        {
            ally = target.gameObject.AddComponent<ConvertedAlly>();
        }
        ally.Take(target);
    }

    public static bool IsConverted(AiReferences candidate) =>
        candidate && candidate.TryGetComponent(out ConvertedAlly ally) && ally.IsActive;

    public static void CommandAll(Vector3 point)
    {
        for (int i = 0; i < active.Count; i++)
        {
            active[i].commandedPoint = point;
            active[i].hasCommandedPoint = true;
        }
    }

    private void Take(AiReferences owner)
    {
        refs = owner;
        aiPath = owner.aiPath;
        // left alone it overwrites our destination every frame from the old waypoint
        if (!destinationSetter) destinationSetter = GetComponentInChildren<AIDestinationSetter>(true);

        // the FSM and this component would fight over the destination every tick
        if (owner.stateManager) owner.stateManager.StopAI();
        if (owner.AttackPlayer) owner.AttackPlayer.SetCanAttack(false);
        if (owner.Vision) owner.Vision.ResetVisionState();
        if (destinationSetter) destinationSetter.target = null;
        if (owner.MaterialFlash) owner.MaterialFlash.FlashHeadshot();

        hasCommandedPoint = false;
        nextStrikeTime = 0f;
        enabled = true;
        if (!active.Contains(this)) active.Add(this);

        CancelInvoke();
        InvokeRepeating(nameof(Think), Random.Range(0f, 0.05f), TickInterval);
    }

    // everything Take touched is restored by ResetAll's own chain, so this only has
    // to stop thinking and let go
    public void Revert()
    {
        CancelInvoke();
        active.Remove(this);
        hasCommandedPoint = false;
        refs = null;
        enabled = false;
    }

    private void OnDisable()
    {
        CancelInvoke();
        active.Remove(this);
    }

    private void Think()
    {
        if (!refs || (refs.Health && !refs.Health.IsAlive))
        {
            Revert();
            return;
        }

        if (!aiPath) return;
        aiPath.maxSpeed = ChaseSpeed * WishUpgrades.EnemySpeedMultiplier;
        aiPath.rotationSpeed = RotationSpeed;

        AiReferences prey = NearestHostile();

        if (hasCommandedPoint)
        {
            aiPath.destination = commandedPoint;
            if (Vector3.Distance(transform.position, commandedPoint) < CommandArrivalDistance)
            {
                hasCommandedPoint = false;
            }
        }
        else if (prey)
        {
            aiPath.destination = prey.transform.position;
        }

        if (prey) TryStrike(prey);
    }

    private void TryStrike(AiReferences prey)
    {
        if (Time.time < nextStrikeTime) return;
        if (Vector3.Distance(transform.position, prey.transform.position) > StrikeRange) return;

        nextStrikeTime = Time.time + StrikeInterval;
        if (prey.MaterialFlash) prey.MaterialFlash.Flash();
        if (prey.Health) prey.Health.TakeDamage(StrikeDamage, false, false);
    }

    private AiReferences NearestHostile()
    {
        AiReferences nearest = null;
        float nearestSqr = float.MaxValue;
        Vector3 here = transform.position;

        List<AiReferences> all = AiReferences.AllEnemies;
        for (int i = 0; i < all.Count; i++)
        {
            AiReferences candidate = all[i];
            if (!candidate || candidate == refs) continue;
            if (candidate.Health && !candidate.Health.IsAlive) continue;
            if (IsConverted(candidate)) continue;

            float sqr = (candidate.transform.position - here).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = candidate;
            }
        }
        return nearest;
    }
}
