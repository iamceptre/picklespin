using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance;

    [Header("References")]
    [SerializeField] private Image crosshairImage;

    [Header("Crosshair Settings")]
    [SerializeField] private float fadeDuration = 0.1f;

    private float startingAlpha;

    private int crosshairVisibleCount;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        startingAlpha = crosshairImage.color.a;
    }

    public void ShowCrosshair()
    {
        crosshairVisibleCount++;
        if (crosshairVisibleCount == 1)
        {
            crosshairImage.DOKill();
            crosshairImage.DOFade(1f, fadeDuration);
        }
    }

    public void HideCrosshair()
    {
        crosshairVisibleCount--;
        if (crosshairVisibleCount <= 0)
        {
            crosshairVisibleCount = 0;
            crosshairImage.DOKill();
            crosshairImage.DOFade(startingAlpha, fadeDuration);
        }
    }
}
