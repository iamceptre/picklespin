using UnityEngine;
using DG.Tweening;
using Thinksquirrel.CShake;

public class CameraShakeManagerV2 : MonoBehaviour
{
    public static CameraShakeManagerV2 instance { get; private set; }

    [SerializeField] private CameraShake[] cameraShakes;
    [SerializeField] private Transform handTransform;

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

    public void ShakeSelected(int index)
    {
        StopAll();
        cameraShakes[index].Shake();

    }

    public void ShakeHand(float strenght, float duration, int vibrato)
    {
        handTransform.DOShakePosition(duration, strenght, vibrato, 90, false, true, ShakeRandomnessMode.Harmonic);
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
