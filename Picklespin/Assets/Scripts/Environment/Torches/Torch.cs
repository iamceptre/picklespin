using FMODUnity;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Torch : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private ParticleSystem[] additionalParticles;

    [SerializeField] private StudioEventEmitter _emitter;
    [SerializeField] private Light _light;
    private float _lightIntensity;
    [SerializeField] private LightFluctuation _fluctuation;


    [SerializeField] private UnityEvent additionalOnEvent;
    [SerializeField] private UnityEvent additionalOffEvent;

    private void Awake()
    {
        _lightIntensity = _light.intensity;
        _fluctuation.enabled = false;
    }



    public void On()
    {
        additionalOnEvent.Invoke();
        _light.DOKill();
        _emitter.Play();
        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = true;
        _particleSystem.Play();
        _light.enabled = true;
        PlayAdditional();
        _light.DOIntensity(_lightIntensity, 0.4f).OnComplete(() =>
        {
            _fluctuation.enabled = true;
        });
    }


    private void PlayAdditional()
    {
        for (int i = 0; i < additionalParticles.Length; i++)
        {
            additionalParticles[i].Play();
        }
    }

    private void StopAdditional()
    {
        for (int i = 0; i < additionalParticles.Length; i++)
        {
            additionalParticles[i].Stop();
        }
    }

    public void Off()
    {
        additionalOffEvent.Invoke();
        _particleSystem.Stop();
        _light.DOKill();
        _emitter.Stop();
        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = false;
        _fluctuation.enabled = false;

        StopAdditional();

        _light.DOIntensity(0, 0.4f).OnComplete(() =>
        {
            _light.enabled = false;
        });

    }

}
