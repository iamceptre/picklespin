using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using FMODUnity;

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
    public Image scrollTip;
    private float scrollTipStartYPos;
    public Image turboArea;
    private Color turboAreaColor;

    private float turboAreaLeftEdgePosition;
    private float turboAreaRightEdgePosition;
    private float turboAreaWidth = 10;

    public bool boosted = false;
    private bool missed = false;
    private bool tooLate = false;

    [SerializeField] AngelHealBoostLight boostLight;

   private void Awake()
    {
        sliderStartingColor = sliderFill.color;
        angelHPslider = GetComponent<Slider>(); 
        turboAreaColor = turboArea.color;
    }

    private void Start()
    {
        RandomizeTurboAreaPosition();
    }

    public void RandomizeTurboAreaPosition()
    {
        float halfOfWidth = turboAreaWidth * 0.5f;
        float turboAreaRandomizedPosition = Random.Range(8, 35);
        turboAreaLeftEdgePosition = turboAreaRandomizedPosition - halfOfWidth;
        turboAreaRightEdgePosition = turboAreaRandomizedPosition + halfOfWidth;
        float desiredTurboAreaPositionX = Mathf.Lerp(-147, 147, turboAreaRandomizedPosition * 0.01f);
        RectTransform myTransform = turboArea.rectTransform;
        myTransform.localPosition = new Vector2(desiredTurboAreaPositionX, 0);
        scrollTip.rectTransform.localPosition = new Vector2(desiredTurboAreaPositionX, scrollTip.rectTransform.localPosition.y);
    }

    public void InitializeMinigame()
    {
        if (!tooLate) {
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

        if (angelHPslider.value >= turboAreaLeftEdgePosition && angelHPslider.value <= turboAreaRightEdgePosition) 
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
            DOTween.To(() => aiHealth.hp, x => aiHealth.hp = x, 1, 0.4f).SetEase(Ease.OutExpo);
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

        scrollTip.DOFade(0, 0.08f).OnComplete(DisableScript);

        turboArea.DOFade(0, 0.1f).OnComplete(() =>
        {
            turboArea.enabled = false;
            scrollTip.enabled = false;
        });
    }

    private void Reddify()
    {
        scrollTip.DOColor(Color.red, 0.3f);
        turboArea.DOColor(Color.red, 0.3f).OnComplete(FadeOut);
    }

    private void FadeIn()
    {
       // turboArea.DOKill();
        scrollTip.DOKill();
       // turboArea.color = new Color(turboArea.color.r, turboArea.color.g, turboArea.color.b, 0);
       // scrollTip.color = new Color(scrollTip.color.r, scrollTip.color.g, scrollTip.color.b, 0);
       // turboArea.DOFade(1, 0.05f);
        scrollTip.DOFade(0.62f, 0.1f).OnComplete(TipFloat);
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
            //angelHeal.healboost = 4;
            angelHeal.healboost = 0;
            DOTween.To(() => aiHealth.hp, x => aiHealth.hp = x, 100, 0.7f).SetEase(Ease.OutSine);
            RuntimeManager.PlayOneShot(healBoostSound);

            boosted = true;
        }
    }

    private void DisableScript()
    {
        enabled = false;
    }
}
