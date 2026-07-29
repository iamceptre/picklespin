using UnityEngine;
using DG.Tweening;
using Thinksquirrel.CShake;

public class CameraShakeManagerV2 : MonoBehaviour
{
    public static CameraShakeManagerV2 instance { get; private set; }

    [SerializeField] private CameraShake[] cameraShakes;
    [SerializeField] private Transform handTransform;

    private Vector3[] baseShakeAmounts;
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
        baseShakeAmounts = new Vector3[cameraShakes.Length];
        for (int i = 0; i < cameraShakes.Length; i++)
        {
            if (cameraShakes[i] != null)
            {
                baseShakeAmounts[i] = cameraShakes[i].shakeAmount;
            }
        }

        // "ScreenShakeStrenght" (sic) matches the options scene slider's settingName key
        if (PlayerPrefs.HasKey("ScreenShakeStrenght"))
        {
            SetStrength(PlayerPrefs.GetFloat("ScreenShakeStrenght") * 0.01f);
        }
    }

    // 0 = no shakes at all, 1 = authored strength
    public void SetStrength(float normalized)
    {
        strength = Mathf.Clamp01(normalized);
        for (int i = 0; i < cameraShakes.Length; i++)
        {
            if (cameraShakes[i] != null)
            {
                cameraShakes[i].shakeAmount = baseShakeAmounts[i] * strength;
            }
        }
    }

    public void ShakeSelected(int index)
    {
        if (strength <= 0) return;
        StopAll();
        cameraShakes[index].Shake();

    }

    public void ShakeHand(float strenght, float duration, int vibrato)
    {
        if (strength <= 0) return;
        handTransform.DOShakePosition(duration, strenght * strength, vibrato, 90, false, true, ShakeRandomnessMode.Harmonic);
    }

    private void StopAll()
    {
        for (int i = 0; i < cameraShakes.Length; i++)
        {
            if (cameraShakes[i] != null)
            {
                cameraShakes[i].CancelShake();
            }
        }
    }

}
