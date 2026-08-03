using UnityEngine;

public interface ISpellBehaviour
{
    bool InterceptHit(AiReferences refs, bool keepFlying);
    void OnImpact(Vector3 point);
    void ResetForFlight();
    bool TryRetire();
}
