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
    [SerializeField] private BarEase barEase;

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
            //Debug.Log(slider.value); //reports hp
        }

        FadeIn();

    }


    public void FadeOut()
    {
        StopAllCoroutines();
        bgImage.DOKill();
        fillImage.DOKill();
        barEase.FadeOut();  

        fillImage.DOFade(0, 0.5f);

        bgImage.DOFade(0, 0.5f).OnComplete(() => { 
            fillImage.enabled = false;
            bgImage.enabled = false;
            wholeCanvas.SetActive(false);

            if (slider.value<=0) {
                Destroy(gameObject);
            }
        });
    }

    private void FadeIn()
    {
        if (slider.value > 0)
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndFadeOut());
            wholeCanvas.SetActive(true);
            bgImage.DOKill();
            fillImage.DOKill();
            fillImage.enabled = true;
            bgImage.enabled = true;
            fillImage.DOFade(1, 0f);
            bgImage.DOFade(1, 0f);
        }
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(5);
        FadeOut();
    }

    public void Detach()
    {
        Vector3 myLastPosition = transform.position;
        transform.SetParent(null);
        transform.position = myLastPosition;
    }

}
