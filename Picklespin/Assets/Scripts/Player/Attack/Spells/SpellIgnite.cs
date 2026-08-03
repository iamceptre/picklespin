using UnityEngine;

public class SpellIgnite : MonoBehaviour
{

    public void TryIgnite(AiReferences refs)
    {
        if (!refs || !refs.setOnFire) return;
        if (!refs.Health || !refs.Health.IsAlive || refs.Health.hp <= 0) return;

        refs.setOnFire.Ignite();
    }
}
