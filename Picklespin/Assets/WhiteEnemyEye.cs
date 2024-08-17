using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class WhiteEnemyEye : MonoBehaviour
{

    [SerializeField] private Renderer _rend;
    [SerializeField] private Light _light;
    private float lightTargetIntensity;

    [SerializeField] private WhiteEnemyEyeLitAnimation EyeFlashAnimation;

    [SerializeField] private GameObject shutdownFadeOutSprite;

    [SerializeField] private StudioEventEmitter eyeOpenEmitter;
    [SerializeField] private Collider headshotHitbox;

    private void Awake()
    {
        lightTargetIntensity = _light.intensity;
        headshotHitbox.enabled = false;
    }


    public void On()
    {
        headshotHitbox.enabled = true;
        eyeOpenEmitter.Play();
        _rend.enabled = true;
        EyeFlashAnimation.Flash();
        _light.enabled = true;
        _light.intensity = 0;
        _light.DOIntensity(lightTargetIntensity, 1);
    }

    public void Off()
    {
        shutdownFadeOutSprite.SetActive(true);
        _rend.enabled = false;
        _light.DOIntensity(0, 1).OnComplete(() =>
        {
            _light.enabled = false;
            headshotHitbox.enabled = false;
        });
    }


}