using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AwakeFade : MonoBehaviour
{

    private void Awake()
    {
        Image image = GetComponent<Image>();
        image.color = new Color(255, 253, 208, 0);
        image.DOFade(1, 0.2f).SetEase(Ease.InSine);
    }

}
