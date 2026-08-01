using UnityEngine;

public class SpellRocketJump : MonoBehaviour
{
    [SerializeField] private float force = 50f;
    [SerializeField] private float upwardsModifier = 1f;
    [SerializeField, Tooltip("how far the blast reaches - the self-damage falls off across it")]
    private float radius = 5f;
    [SerializeField] private LayerMask affectedLayers;

    private PlayerHP playerHP;

    private static readonly Collider[] overlapResults = new Collider[32];

    private void Start() => playerHP = PlayerHP.Instance;

    public void Apply(Vector3 explosionCenter)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(explosionCenter, radius, overlapResults, affectedLayers, QueryTriggerInteraction.Ignore);
        bool characterControllerFound = false;

        float boostedForce = force * WishUpgrades.RocketJumpForceMultiplier * PlayerClasses.RocketJumpForceMultiplier;

        for (int i = 0; i < hitCount; i++)
        {
            if (characterControllerFound) break;

            var col = overlapResults[i];
            if (col == null) continue;

            var rb = col.attachedRigidbody;
            if (rb && !rb.isKinematic)
            {
                rb.AddExplosionForce(force, explosionCenter, radius, upwardsModifier, ForceMode.Impulse);
                continue;
            }

            var cc = col.GetComponent<CharacterController>();
            if (!cc) continue;

            var playerMove = cc.GetComponent<PlayerMovement>();
            if (!playerMove) continue;

            characterControllerFound = true;

            var distance = Vector3.Distance(playerMove.transform.position, explosionCenter);
            var proximityFactor = Mathf.Clamp01(1f - distance / radius);

            if (proximityFactor < PlayerClasses.RocketJumpMinProximity) break;

            playerMove.AddExplosionJump(boostedForce * 2f, explosionCenter, radius);
            if (WishUpgrades.RocketJumpSelfDamage && playerHP)
            {
                float selfDamage = force * proximityFactor * PlayerClasses.RocketJumpSelfDamageMultiplier;
                playerHP.ModifyHP(Mathf.RoundToInt(selfDamage) * -2);
            }
        }
    }
}
