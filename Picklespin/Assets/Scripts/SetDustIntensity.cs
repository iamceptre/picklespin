using UnityEngine;

public class SetDustIntensity : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule _mainModule;

    private float currentIntensity;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _mainModule = _particleSystem.main;
        currentIntensity = _mainModule.startColor.color.a;
    }


    public void SetIntensity(float intensity)
    {
        currentIntensity = intensity;
        Color workColor = new Color(_mainModule.startColor.color.r, _mainModule.startColor.color.g, _mainModule.startColor.color.b, currentIntensity);
        _mainModule.startColor = workColor;
    }
}
