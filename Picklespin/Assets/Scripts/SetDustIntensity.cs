using UnityEngine;

public class SetDustIntensity : MonoBehaviour
{
    [SerializeField] private ParticleSystem sandParticle;
    private ParticleSystem.MainModule sandParticleMain;

    private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule _mainModule;

    private float currentIntensity;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _mainModule = _particleSystem.main;
        currentIntensity = _mainModule.startColor.color.a;

        sandParticleMain = sandParticle.main;
    }

    private void Start()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Windiness", 0);
    }


    public void SetIntensity(float intensity)
    {
        currentIntensity = intensity;
        _mainModule.startColor = new Color(_mainModule.startColor.color.r, _mainModule.startColor.color.g, _mainModule.startColor.color.b, currentIntensity);

        sandParticleMain.startColor = new Color(sandParticleMain.startColor.color.r, sandParticleMain.startColor.color.g, sandParticleMain.startColor.color.b, currentIntensity * 2);

        float windiness = (currentIntensity * 10) - 0.19f;

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Windiness", windiness);
        //Debug.Log("Windiness set to " + windiness);
    }
}
