using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class WhiteEnemyEye : MonoBehaviour
{

    private const string headshotTag = "Hitbox_Head";
    private const string bodyTag = "NPC_Hitbox";

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
        headshotHitbox.tag = headshotTag;
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
            headshotHitbox.tag = bodyTag;
        });
    }

    // hard reset to the closed state, for pooled reuse (no fades, kills pending tweens)
    public void ResetEye()
    {
        _light.DOKill();
        _rend.enabled = false;
        shutdownFadeOutSprite.SetActive(false);
        _light.enabled = false;
        _light.intensity = 0;
        headshotHitbox.tag = bodyTag;
        headshotHitbox.enabled = false;
    }

}