using UnityEngine;

public class DisableParticleOnEnable : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private void OnEnable()
    {
        _particleSystem.Clear();
        _particleSystem.Stop();
    }
}
