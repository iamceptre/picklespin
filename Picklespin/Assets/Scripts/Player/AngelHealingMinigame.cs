using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using FMODUnity;

//MAKE IT REMEMBER IF ANGEL WAS MISSED

public class AngelHealingMinigame : MonoBehaviour
{

    [SerializeField] private EventReference healBoostSound;
    [SerializeField] private EventReference failedSound;

    [HideInInspector] public AiHealth aiHealth;

    private Slider angelHPslider;
    [SerializeField] private Image sliderFill;
    private Color sliderStartingColor;
    private bool inTurboRange = false;

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private Image scrollTip;
    private float scrollTipStartYPos;
    [SerializeField] private Image turboArea;
    private Color turboAreaColor;

    private bool boosted = false;
    private bool missed = false;
    private bool tooLate = false;

    [SerializeField] AngelHealBoostLight boostLight;

   private void Awake()
    {
        sliderStartingColor = sliderFill.color;
        angelHPslider = GetComponent<Slider>(); 
        turboAreaColor = turboArea.color;
    }

    public void InitializeMinigame()
    {
        if (!tooLate) { // && angel not fucked
            sliderFill.color = sliderStartingColor;
            inTurboRange = false;
            turboArea.enabled = true;
            scrollTip.enabled = true;
            turboArea.color = new Color(255, 255, 255, 1);
            scrollTip.color = new Color(255, 255, 255, 1);
            angelHeal.healboost = 1;
            scrollTipStartYPos = scrollTip.rectTransform.localPosition.y;
            missed = false;
            boosted = false;
            FadeIn();
        }
        else
        {
            turboArea.enabled = false;
            scrollTip.enabled = false;
            enabled = false;    
        }
    }

   private void Update()
    {

        if (angelHPslider.value >= 34 && angelHPslider.value <= 41) 
        {
            EnterRange();
        }
        else
        {
            ExitRange();
        }



        if (Input.mouseScrollDelta.y != 0 || (Input.GetMouseButtonDown(2)))
        {
            if (inTurboRange)
            {
                    Boost();
            }
            else
            {
                    Miss();
            }
        }
    }

    private void EnterRange() //one impulse, not a continous function
    {
        if (!inTurboRange && !missed) {

            turboArea.DOColor(turboAreaColor, 0.2f);
            scrollTip.DOFade(1, 0.2f);
            scrollTip.color = Color.white;
            inTurboRange = true;
        }
    }

    private void ExitRange()
    {
        if (inTurboRange && !boosted)
        {
            tooLate = true;
            FadeOut();
            inTurboRange = false;
        }
    }

    private void Miss()
    {
        if (!missed && !tooLate)
        {
            //angelHeal.healboost = 0.5f;
            DOTween.To(() => aiHealth.hp, x => aiHealth.hp = x, 1, 0.4f).SetEase(Ease.InOutExpo);
            sliderFill.DOColor(Color.red, 0.4f).OnComplete(RevertSliderColor);
            Reddify();
            RuntimeManager.PlayOneShot(failedSound);
            missed = true;
        }
    }

    private void RevertSliderColor()
    {
        //aiHealth.hp = 1;
        sliderFill.DOColor(sliderStartingColor, 0.4f);
    }


    private void FadeOut()
    {
        turboArea.DOKill();
        scrollTip.DOKill();
        turboArea.DOFade(0, 0.2f);
        scrollTip.DOFade(0, 0.4f).OnComplete(DisableScript);
    }

    private void Reddify()
    {
        scrollTip.DOColor(Color.red, 0.3f);
        turboArea.DOColor(Color.red, 0.3f).OnComplete(FadeOut);
    }

    private void FadeIn()
    {
        turboArea.DOKill();
        scrollTip.DOKill();
        turboArea.color = new Color(turboArea.color.r, turboArea.color.g, turboArea.color.b, 0);
        scrollTip.color = new Color(scrollTip.color.r, scrollTip.color.g, scrollTip.color.b, 0);
        turboArea.DOFade(1, 0.1f);
        scrollTip.DOFade(0.62f, 0.2f).OnComplete(TipFloat);
    }
    
    private void TipFloat()
    {
        if (!boosted && !missed && !tooLate)
        {
            scrollTip.rectTransform.DOLocalMoveY(scrollTipStartYPos - 8, 0.4f).OnComplete(FloatDown);
        }
    }

    private void FloatDown()
    {
        if (!boosted && !missed && !tooLate)
        {
            scrollTip.rectTransform.DOLocalMoveY(scrollTipStartYPos, 0.4f).OnComplete(TipFloat);
        }
    }

    private void Boost()
    {
        if (!boosted && !tooLate && !missed) 
        {
            boostLight.LightAnimation();
            FadeOut();
            angelHeal.healboost = 4; //make it smooth with tweening instead
            RuntimeManager.PlayOneShot(healBoostSound);

            boosted = true;
        }
    }

    private void DisableScript()
    {
        enabled = false;
    }
}
