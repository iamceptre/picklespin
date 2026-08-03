using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ScreenFlashTint : MonoBehaviour
{

    public static ScreenFlashTint instance { get; private set; }

    private Image _image;

    private float animationTime = 0.25f;

    private void Awake()
    {
        _image = GetComponent<Image>();

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        _image.enabled = false;

    }

    private void Initialize(int index)
    {
        if (index < 0 || index >= GameColors.ScreenFlash.Length)
        {
            DevLog.Warn($"{nameof(ScreenFlashTint)}: no flash colour at index {index}", this);
            return;
        }

        _image.enabled = true;
        _image.DOKill();
        _image.color = GameColors.ScreenFlash[index];
    }

    public void Flash(int index)
    {
         Initialize(index);
        _image.DOFade(0, animationTime).OnComplete(() =>
        {
            _image.enabled = false;
        });
    }

    public void Flash(int index, float customAnimationTime)
    {
        Initialize(index);
        _image.DOFade(0, customAnimationTime).OnComplete(() =>
        {
            _image.enabled = false;
        });
    }
}
