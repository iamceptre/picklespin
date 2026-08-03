using UnityEngine;

public class SpellDecal : MonoBehaviour
{
    [SerializeField, Tooltip("which mark this spell leaves - several spells may share one. Mind that Fire is a real fire: it burns the player who stands in it.")]
    private SpellDecalType decalType;
    [SerializeField, Tooltip("surfaces that take a decal at all")]
    private LayerMask decalLayerMask;

    public void TrySpawn(Collision collision)
    {
        if (!SpellDecalManager.Instance) return;
        if (!collision.gameObject.isStatic) return;

        var hitObject = collision.collider.gameObject;
        if (((1 << hitObject.layer) & decalLayerMask) == 0) return;

        var contact = collision.GetContact(0);
        SpellDecalManager.Instance.SpawnDecal(
            contact.point + contact.normal * 0.01f,
            Quaternion.LookRotation(contact.normal),
            decalType,
            hitObject.tag.GetHashCode());
    }
}
