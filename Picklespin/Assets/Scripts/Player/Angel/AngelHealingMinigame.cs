using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using FMODUnity;
using UnityEngine.InputSystem;

public class AngelHealingMinigame : MonoBehaviour
{
    public static AngelHealingMinigame Instance { get; private set; }

    private enum Phase { Off, Armed, Open, Spent }

    [Header("References")]
    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private AngelHealBoostLight boostLight;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Image turboArea;
    [SerializeField] private Image scrollTip;

    [Header("Sound")]
    [SerializeField] private EventReference healBoostSound;
    [SerializeField] private EventReference failedSound;

    [Header("Turbo Window")]
    [SerializeField, Range(1f, 40f), Tooltip("narrowest the window may roll, in percent of the bar - the area is drawn exactly this wide, so what you see is what counts")]
    private float windowWidthMin = 5f;
    [SerializeField, Range(1f, 40f), Tooltip("widest the window may roll, in percent of the bar")]
    private float windowWidthMax = 8f;
    [SerializeField, Range(0f, 100f), Tooltip("earliest the middle of the window may sit, in percent of the bar")]
    private float windowCenterMin = 7f;
    [SerializeField, Range(0f, 100f), Tooltip("latest the middle of the window may sit, in percent of the bar")]
    private float windowCenterMax = 35f;
    [SerializeField, Range(0f, 50f), Tooltip("how much bar the fill must still have to cross before the window opens - stops the window landing under the health the angel already has")]
    private float minimumRunUp = 5f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference scrollAction;
    [SerializeField] private InputActionReference middleClickAction;

    private const float FullHealth = 100f;
    private const float MissPenaltyHealth = 1f;
    private const float ScrollDeadzone = 0.001f;
    private const float TipRestAlpha = 0.62f;
    private const float TipFloatDistance = 8f;
    private const float TipFloatTime = 0.4f;
    private const float HighlightTime = 0.2f;
    private const float ReddifyTime = 0.3f;
    private const float PenaltyTime = 0.4f;
    private const float BoostTime = 0.7f;
    private const float FadeInTime = 0.1f;
    private const float FadeOutTime = 0.1f;

    private RectTransform turboRect;
    private RectTransform tipRect;
    private Color activeAreaColor;
    private Color restingFillColor;
    private float tipRestY;
    private float areaRestY;
    private float barHalfWidth;
    private float areaWidthPerPercent;
    private float windowCenterX;

    private InputAction scroll;
    private InputAction middleClick;

    private AngelMind angel;
    private AiHealth angelHealth;
    private Phase phase = Phase.Off;
    private float windowOpensAt;
    private float windowClosesAt;
    private float windowRollOffset;
    private int windowRoll;

    private DOGetter<float> healthGetter;
    private DOSetter<float> healthSetter;
    private Tween healthTween;
    private Tween tipFloatTween;
    private Tween fadeDelay;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        turboRect = turboArea.rectTransform;
        tipRect = scrollTip.rectTransform;
        activeAreaColor = turboArea.color;
        restingFillColor = sliderFill.color;
        tipRestY = tipRect.anchoredPosition.y;
        areaRestY = turboRect.anchoredPosition.y;
        windowCenterX = turboRect.anchoredPosition.x;
        windowRollOffset = Random.value;

        RectTransform fillArea = sliderFill.rectTransform.parent as RectTransform;
        barHalfWidth = (fillArea ? fillArea.rect.width : turboRect.rect.width) * 0.5f;
        float areaScale = Mathf.Abs(turboRect.localScale.x);
        areaWidthPerPercent = barHalfWidth * 0.02f / (areaScale > 0.0001f ? areaScale : 1f);

        scroll = scrollAction.action;
        middleClick = middleClickAction.action;

        healthGetter = () => angelHealth.hp;
        healthSetter = value => angelHealth.hp = value;

        HideNow();
        enabled = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Begin(AngelMind target)
    {
        bool isNewAngel = angel != target;
        angel = target;
        angelHealth = target ? target.Health : null;

        KillTweens();

        if (!angel || !angelHealth || angel.BoostSpent)
        {
            GiveUp();
            return;
        }

        if (isNewAngel) RollWindow(angelHealth.hp);

        if (angelHealth.hp > windowClosesAt)
        {
            angel.SpendBoost();
            GiveUp();
            return;
        }

        phase = Phase.Armed;
        enabled = true;
        Show();
    }

    public void Stop()
    {
        KillTweens();
        HideNow();
        if (phase != Phase.Spent) phase = Phase.Off;
        enabled = false;
    }

    private void GiveUp()
    {
        phase = Phase.Spent;
        HideNow();
        enabled = false;
    }

    private void Update()
    {
        if (!angel || !angelHealth || angel.IsDead || angel.healed)
        {
            Stop();
            return;
        }

        float health = angelHealth.hp;

        if (phase == Phase.Armed && health >= windowOpensAt && health <= windowClosesAt) OpenWindow();

        if (Triggered())
        {
            if (phase == Phase.Open) Boost();
            else Miss();
            return;
        }

        if (health > windowClosesAt) TooLate();
    }

    private bool Triggered() =>
        Mathf.Abs(scroll.ReadValue<float>()) > ScrollDeadzone || middleClick.WasPressedThisFrame();

    private void RollWindow(float fromHealth)
    {
        float width = Random.Range(windowWidthMin, windowWidthMax);
        float half = width * 0.5f;
        float earliest = Mathf.Max(windowCenterMin, fromHealth + minimumRunUp + half);
        float latest = Mathf.Max(earliest, Mathf.Min(windowCenterMax, FullHealth - half));
        float center = Mathf.Lerp(earliest, latest, PhiMath.GoldenSequence(windowRoll++, windowRollOffset));

        windowOpensAt = center - half;
        windowClosesAt = center + half;
        windowCenterX = Mathf.Lerp(-barHalfWidth, barHalfWidth, center * 0.01f);

        turboRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * areaWidthPerPercent);
        turboRect.anchoredPosition = new Vector2(windowCenterX, areaRestY);
        tipRect.anchoredPosition = new Vector2(windowCenterX, tipRestY);
    }

    private void Show()
    {
        ResetTip();
        turboArea.enabled = true;
        scrollTip.enabled = true;
        turboArea.color = Color.white;
        scrollTip.color = new Color(1f, 1f, 1f, 0f);
        scrollTip.DOFade(TipRestAlpha, FadeInTime);
        tipFloatTween = tipRect.DOAnchorPosY(tipRestY - TipFloatDistance, TipFloatTime)
                               .SetEase(Ease.InOutSine)
                               .SetLoops(-1, LoopType.Yoyo);
    }

    private void OpenWindow()
    {
        phase = Phase.Open;
        turboArea.DOKill();
        turboArea.DOColor(activeAreaColor, HighlightTime);
        scrollTip.DOKill();
        scrollTip.color = new Color(1f, 1f, 1f, scrollTip.color.a);
        scrollTip.DOFade(1f, HighlightTime);
    }

    private void Boost()
    {
        Spend();
        boostLight.LightAnimation();
        RuntimeManager.PlayOneShot(healBoostSound);
        angelHeal.BeginBoost();
        healthTween = DOTween.To(healthGetter, healthSetter, FullHealth, BoostTime)
                             .SetEase(Ease.OutSine)
                             .OnComplete(FinishBoost);
        FadeOut();
    }

    private void FinishBoost()
    {
        healthTween = null;
        angelHeal.Healed();
    }

    private void Miss()
    {
        Spend();
        RuntimeManager.PlayOneShot(failedSound);
        healthTween = DOTween.To(healthGetter, healthSetter, MissPenaltyHealth, PenaltyTime)
                             .SetEase(Ease.OutExpo);
        sliderFill.DOKill();
        sliderFill.DOColor(Color.red, PenaltyTime).OnComplete(RevertFillColor);
        scrollTip.DOKill();
        scrollTip.DOColor(Color.red, ReddifyTime);
        turboArea.DOKill();
        turboArea.DOColor(Color.red, ReddifyTime);
        fadeDelay = DOVirtual.DelayedCall(ReddifyTime, FadeOut);
    }

    private void TooLate()
    {
        Spend();
        FadeOut();
    }

    private void Spend()
    {
        phase = Phase.Spent;
        angel.SpendBoost();
        StopTipFloat();
        enabled = false;
    }

    private void RevertFillColor()
    {
        sliderFill.DOColor(restingFillColor, PenaltyTime);
    }

    private void FadeOut()
    {
        fadeDelay = null;
        StopTipFloat();
        scrollTip.DOKill();
        scrollTip.DOFade(0f, FadeOutTime * 0.8f);
        turboArea.DOKill();
        turboArea.DOFade(0f, FadeOutTime).OnComplete(HideNow);
    }

    private void HideNow()
    {
        turboArea.enabled = false;
        scrollTip.enabled = false;
    }

    private void StopTipFloat()
    {
        if (tipFloatTween == null) return;
        tipFloatTween.Kill();
        tipFloatTween = null;
    }

    private void ResetTip()
    {
        StopTipFloat();
        tipRect.anchoredPosition = new Vector2(windowCenterX, tipRestY);
    }

    private void KillTweens()
    {
        ResetTip();
        if (healthTween != null)
        {
            healthTween.Kill();
            healthTween = null;
        }
        if (fadeDelay != null)
        {
            fadeDelay.Kill();
            fadeDelay = null;
        }
        turboArea.DOKill();
        scrollTip.DOKill();
        sliderFill.DOKill();
        sliderFill.color = restingFillColor;
    }
}
