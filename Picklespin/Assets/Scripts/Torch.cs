using FMODUnity;
using UnityEngine;
using DG.Tweening;

public class Torch : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    [SerializeField] private StudioEventEmitter _emitter;
    [SerializeField] private Light _light;
    private float _lightIntensity;
    [SerializeField] private LightFluctuation _fluctuation;

    private void Awake()
    {
        _lightIntensity = _light.intensity;
        _fluctuation.enabled = false;
    }



    public void On()
    {
        _emitter.Play();
        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = true;
        _light.enabled = true;
        _light.DOIntensity(_lightIntensity, 0.4f).OnComplete(() =>
        {
            _fluctuation.enabled = true;
        });
    }

    public void Off()
    {
        _emitter.Stop();
        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = false;
        _fluctuation.enabled = false;

        _light.DOIntensity(0, 0.4f).OnComplete(() =>
        {
            _light.enabled = false;
        });

    }

}
