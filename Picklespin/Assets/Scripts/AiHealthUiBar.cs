using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class AiHealthUiBar : MonoBehaviour
{
    [SerializeField] private AiHealth aiHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image bgImage;

    [SerializeField] private GameObject wholeCanvas;

    private void Awake()
    {
        if (aiHealth == null)
        {
            aiHealth = gameObject.GetComponent<AiHealth>();
        }

        fillImage.enabled = false;
        bgImage.enabled = false;
        wholeCanvas.SetActive(false);
    }



    public void RefreshBar()
    {

        if (aiHealth.hp != slider.value)
        {
            slider.value = aiHealth.hp;
        }

        FadeIn();

    }


    public void FadeOut()
    {

        bgImage.DOFade(0, 0.5f);

        fillImage.DOFade(0, 0.5f).OnComplete(() => { 
            fillImage.enabled = false;
            bgImage.enabled = false;
            wholeCanvas.SetActive(false);
            enabled = false;
        });
    }

    private void FadeIn()
    {
        wholeCanvas.SetActive(true);
        bgImage.DOKill();
        fillImage.DOKill();
        StopAllCoroutines();
        fillImage.enabled = true;
        bgImage.enabled = true;
        fillImage.DOFade(1, 0);
        bgImage.DOFade(1, 0);
        StartCoroutine(WaitAndFadeOut());
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(5);
        FadeOut();
    }

}
