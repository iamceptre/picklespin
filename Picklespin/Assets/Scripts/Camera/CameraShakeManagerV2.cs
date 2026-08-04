using UnityEngine;
using DG.Tweening;
using Thinksquirrel.CShake;

public class CameraShakeManagerV2 : MonoBehaviour
{
    public static CameraShakeManagerV2 instance { get; private set; }

    [SerializeField, Tooltip("the CameraShake the settings are written onto - it needs the camera list, nothing else")]
    private CameraShake shaker;
    [SerializeField] private Transform handTransform;

    private float strength = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        SetStrength(PlayerPrefs.GetFloat(SettingsDefaults.ScreenShakeKey, SettingsDefaults.ScreenShake) * 0.01f);
    }

    public void SetStrength(float normalized) => strength = Mathf.Clamp01(normalized);

    public void Shake(CameraShakeSettings settings)
    {
        if (strength <= 0f || !shaker) return;
        if (settings == null || settings.IsSilent) return;

        shaker.CancelShake();
        shaker.shakeAmount = Vector3.zero;
        shaker.rotationAmount = settings.rotationAmount * (settings.strength * strength);
        shaker.numberOfShakes = settings.numberOfShakes;
        shaker.speed = settings.speed;
        shaker.decay = settings.decay;
        shaker.uiShakeModifier = settings.uiShakeModifier;
        shaker.Shake();
    }

    public void ShakeHand(float strenght, float duration, int vibrato)
    {
        if (strength <= 0) return;
        handTransform.DOShakePosition(duration, strenght * strength, vibrato, 90, false, true, ShakeRandomnessMode.Harmonic);
    }
}
