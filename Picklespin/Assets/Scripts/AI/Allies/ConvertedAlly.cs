using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class ConvertedAlly : MonoBehaviour
{
    private static readonly List<ConvertedAlly> active = new();

    private const float TickInterval = 0.2f;

    private const float ChaseSpeed = 10f;
    private const float RotationSpeed = 300f;
    private const float StrikeRange = 6f;
    private const float StrikeInterval = 0.9f;
    private const int StrikeDamage = 22;

    private const float DetectionRadius = 20f;
    private const float CommandArrivalDistance = 3f;

    private const float CommandHoldTime = 12f;

    private const float SlowRadius = 9f;
    private const float SlowFactor = 0.25f;

    private AiReferences refs;
    private AIPath aiPath;
    private AIDestinationSetter destinationSetter;
    private ConvertedAllyGlow glow;
    private Vector3 commandedPoint;
    private bool hasCommandedPoint;
    private float commandExpiryTime;
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

        MakeRoomFor(ally);
        ally.Take(target);
    }

    private static void MakeRoomFor(ConvertedAlly incoming)
    {
        int limit = Mathf.Max(1, SanctusUpgrades.MaxAllies);
        int others = 0;
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i] && active[i] != incoming) others++;
        }

        for (int i = 0; i < active.Count && others >= limit;)
        {
            ConvertedAlly oldest = active[i];
            if (!oldest || oldest == incoming)
            {
                i++;
                continue;
            }

            oldest.Revert();
            others--;
        }
    }

    public static bool IsConverted(AiReferences candidate) =>
        candidate && candidate.TryGetComponent(out ConvertedAlly ally) && ally.IsActive;

    public static float SpeedMultiplierAt(Vector3 position)
    {
        const float radiusSqr = SlowRadius * SlowRadius;

        for (int i = 0; i < active.Count; i++)
        {
            ConvertedAlly ally = active[i];
            if (!ally) continue;
            if ((ally.transform.position - position).sqrMagnitude <= radiusSqr) return SlowFactor;
        }
        return 1f;
    }

    public static void CommandAll(Vector3 point)
    {
        for (int i = 0; i < active.Count; i++)
        {
            active[i].commandedPoint = point;
            active[i].hasCommandedPoint = true;
            active[i].commandExpiryTime = Time.time + CommandHoldTime;
        }
    }

    private void Take(AiReferences owner)
    {
        refs = owner;
        aiPath = owner.aiPath;

        if (!destinationSetter) destinationSetter = GetComponentInChildren<AIDestinationSetter>(true);

        if (owner.stateManager) owner.stateManager.StopAI();
        if (owner.AttackPlayer) owner.AttackPlayer.SetCanAttack(false);
        if (owner.Vision) owner.Vision.ResetVisionState();
        if (destinationSetter) destinationSetter.target = null;
        if (owner.MaterialFlash) owner.MaterialFlash.FlashHeadshot();

        if (!glow) glow = gameObject.AddComponent<ConvertedAllyGlow>();
        glow.Show(owner);
        if (owner.Health) owner.Health.SetAllied(true);
        if (owner.Counter) owner.Counter.StopCounting();

        hasCommandedPoint = false;
        nextStrikeTime = 0f;
        enabled = true;
        if (!active.Contains(this)) active.Add(this);

        CancelInvoke();
        InvokeRepeating(nameof(Think), Random.Range(0f, 0.05f), TickInterval);
    }

    public void Revert()
    {
        CancelInvoke();
        active.Remove(this);
        if (glow) glow.Hide();
        if (refs && refs.Health) refs.Health.SetAllied(false);

        bool stillFighting = refs && refs.isActiveAndEnabled && (!refs.Health || refs.Health.IsAlive);
        if (stillFighting)
        {
            if (refs.Counter) refs.Counter.CountAgain();
            GiveBackItsHead();
        }

        hasCommandedPoint = false;
        refs = null;
        enabled = false;
    }

    private void GiveBackItsHead()
    {
        if (refs.AttackPlayer) refs.AttackPlayer.SetCanAttack(true);
        if (refs.stateManager)
        {
            refs.stateManager.ResetStateManager();
            refs.stateManager.StartAI();
        }
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

        AiReferences prey = NearestHostile();

        if (aiPath) Steer(prey);

        if (prey) TryStrike(prey);
    }

    private void Steer(AiReferences prey)
    {
        aiPath.maxSpeed = ChaseSpeed * WishUpgrades.EnemySpeedMultiplier;
        aiPath.rotationSpeed = RotationSpeed;

        if (hasCommandedPoint)
        {
            aiPath.destination = commandedPoint;

            if (prey
                || Time.time > commandExpiryTime
                || Vector3.Distance(transform.position, commandedPoint) < CommandArrivalDistance)
            {
                hasCommandedPoint = false;
            }
        }
        else if (prey)
        {
            aiPath.destination = prey.transform.position;
        }
        else
        {

            aiPath.destination = transform.position;
        }
    }

    private bool InStrikeRange(AiReferences prey) =>
        prey && Vector3.Distance(transform.position, prey.transform.position) <= StrikeRange;

    private void TryStrike(AiReferences prey)
    {
        if (Time.time < nextStrikeTime) return;
        if (!InStrikeRange(prey)) return;

        nextStrikeTime = Time.time + StrikeInterval;
        if (refs.AttackPlayer) refs.AttackPlayer.PlayAttackOnOther();
        if (prey.MaterialFlash) prey.MaterialFlash.Flash();

        if (prey.Health) prey.Health.TakeQuietDamage(Mathf.RoundToInt(StrikeDamage * SanctusUpgrades.AllyStrikeMultiplier));

        if (prey.AttackPlayer) prey.AttackPlayer.Retaliate(refs);
    }

    private AiReferences NearestHostile()
    {
        AiReferences nearest = null;
        float nearestSqr = DetectionRadius * DetectionRadius;
        Vector3 here = transform.position;

        List<AiReferences> all = AiReferences.AllEnemies;
        for (int i = 0; i < all.Count; i++)
        {
            AiReferences candidate = all[i];
            if (!candidate || candidate == refs) continue;
            if (candidate.IsAngel) continue;
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
