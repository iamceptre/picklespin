using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FadeOutImageOnEnable : MonoBehaviour
{
    private Image image;
    [SerializeField] private float animationTime;

    private void Awake()
    {
        image = GetComponent<Image>();
    }
    void Start()
    {
        image.DOFade(0, animationTime).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }


}
