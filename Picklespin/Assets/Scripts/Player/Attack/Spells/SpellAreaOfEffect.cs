using System.Collections.Generic;
using UnityEngine;

public class SpellAreaOfEffect : MonoBehaviour
{
    [SerializeField] private float radius = 5f;
    [SerializeField, Tooltip("Umbral's shell: the splash only bursts while the shared bar is over half")]
    private bool requiresChargedBar;
    [SerializeField] private LayerMask targetLayers;

    private Bullet bullet;

    private static readonly Collider[] overlapResults = new Collider[32];
    private static readonly HashSet<AiReferences> areaHitBuffer = new();

    private void Awake() => bullet = GetComponent<Bullet>();

    public void Burst(AiReferences directHit, Vector3 center)
    {
        if (requiresChargedBar && !PlayerClasses.ChargedBarReady) return;

        int hitCount = Physics.OverlapSphereNonAlloc(center, radius, overlapResults, targetLayers);
        areaHitBuffer.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == null) continue;
            if (!col.transform.TryGetComponent(out AiReferences areaRefs)) continue;
            if (areaRefs == directHit) continue;
            if (!areaHitBuffer.Add(areaRefs)) continue;
            if (areaRefs.Health && !areaRefs.Health.IsAlive) continue;

            bullet.ApplyHit(areaRefs, false);
        }
    }
}
