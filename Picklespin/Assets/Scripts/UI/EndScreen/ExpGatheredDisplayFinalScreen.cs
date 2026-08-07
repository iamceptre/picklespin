using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Text;

public class ExpGatheredDisplayFinalScreen : MenuScreenPart
{
    private TMP_Text _text;
    [HideInInspector] public int currentlyAnimatedExp = 0;

    [SerializeField][Tooltip("armed once the count has finished, so the prompt cannot be pressed mid-tally")]
    private Reborn inputPrompt;

    [SerializeField] private PlayerLevelDisplayFinalScreen playerLevelDisplayFinalScreen;

    private Tween myTween;

    [SerializeField] private float animationTime = 6;

    private readonly StringBuilder sb = new();
    private int lastShownExp = -1;

    protected override void Awake()
    {
        _text = GetComponent<TMP_Text>();

        if (inputPrompt) inputPrompt.HoldUntilArmed();
        else DevLog.Error($"{nameof(ExpGatheredDisplayFinalScreen)}: no reborn prompt wired in - the win screen counts up and then offers no way out", this);

        if (!playerLevelDisplayFinalScreen) DevLog.Error($"{nameof(ExpGatheredDisplayFinalScreen)}: no level display wired in", this);

        base.Awake();
    }

    protected override void PageOpened()
    {
        base.PageOpened();

        myTween.Kill();
        myTween = DOTween.To(() => currentlyAnimatedExp, x => currentlyAnimatedExp = x, PlayerEXP.instance.playerExpAmount, animationTime)
                         .SetEase(Ease.InOutSine)
                         .SetUpdate(UpdateType.Normal, true)
                         .SetLink(gameObject)
                         .OnComplete(FinishedAnimating);
    }

    protected override void PageClosed()
    {
        base.PageClosed();

        myTween.Kill();
    }

    private void Update()
    {
        if (InputCompat.AnyKeyDown)
        {
            myTween.Kill();
            FinishedAnimating();
            return;
        }

        if (currentlyAnimatedExp != lastShownExp) UpdateText();
    }

    private void FinishedAnimating()
    {
        currentlyAnimatedExp = PlayerEXP.instance.playerExpAmount;
        UpdateText();
        inputPrompt.Arm();
        playerLevelDisplayFinalScreen.FinishedAnimating();
        enabled = false;
    }

    private void UpdateText()
    {
        lastShownExp = currentlyAnimatedExp;
        sb.Clear();
        sb.Append("you gathered<br>");
        sb.Append(currentlyAnimatedExp);
        sb.Append(" exp");
        _text.text = sb.ToString();
    }
}
