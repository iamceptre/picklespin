using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class FadeOutImageOnEnable : MonoBehaviour
{
    private Image image;
    [SerializeField] private float animationTime;

    [SerializeField] private float setWaitTime = 0.5f;
    private WaitForSeconds waitAmount;

    [SerializeField] bool fadeInInstead = false;
    [HideInInspector] public bool fadedIn = false;
    private void Awake()
    {
        waitAmount = new WaitForSeconds(setWaitTime);
        image = GetComponent<Image>();

        if (fadeInInstead)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            return;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }
    private IEnumerator Start()
    {
        yield return waitAmount;

        if (fadeInInstead)
        {
            FadeIn();
            yield break;
        }

        image.DOFade(0, animationTime).SetEase(Ease.OutSine).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

    }

    private void FadeIn()
    {
        image.DOFade(1, animationTime).SetEase(Ease.OutSine).OnComplete(() =>
        {
            fadedIn = true;
        });
    }


}
