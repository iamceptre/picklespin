using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Reborn : MenuScreenPart
{
    private TMP_Text myText;
    private RectTransform myRectTransform;
    private CanvasGroup group;

    private Tween myTween;
    private Tween reveal;

    private readonly float howMuchToSpaceout = 4;

    private readonly float animationTime = 1f;

    private readonly float revealTime = 0.4f;

    [SerializeField] private EventReference rebornEvent;

    [SerializeField][Tooltip("if -1, it restarts the current scene")] private int sceneindex = -1;

    [SerializeField] private UnityEvent OnClickEvent;

    private float authoredSpacing;
    private Vector3 authoredScale;
    private bool clickable;
    private bool held;

    protected override void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myRectTransform = myText.rectTransform;
        authoredSpacing = myText.characterSpacing;
        authoredScale = myRectTransform.localScale;

        if (!TryGetComponent(out group)) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        base.Awake();
    }

    protected override void PageOpened()
    {
        base.PageOpened();

        if (!held) Arm();
    }

    // whoever makes the page wait says so itself, in its own Awake - a prompt that must not be
    // pressable yet is not a checkbox someone can leave ticked on the wrong screen
    public void HoldUntilArmed() => held = true;

    protected override void PageClosed()
    {
        base.PageClosed();

        Disarm();
    }

    public void Arm()
    {
        if (clickable) return;

        clickable = true;
        enabled = true;

        myText.characterSpacing = authoredSpacing;
        myTween.Kill();
        myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, howMuchToSpaceout, animationTime)
                         .SetLoops(-1, LoopType.Yoyo)
                         .SetUpdate(UpdateType.Normal, true)
                         .SetLink(gameObject);

        reveal.Kill();
        reveal = group.DOFade(1f, revealTime).SetUpdate(UpdateType.Normal, true).SetLink(gameObject);
    }

    private void Disarm()
    {
        clickable = false;

        myTween.Kill();
        reveal.Kill();

        myText.characterSpacing = authoredSpacing;
        myRectTransform.localScale = authoredScale;
        group.alpha = 0f;
    }

    private void Update()
    {
        if (!clickable || !InputCompat.GetKeyDown(KeyCode.Return)) return;

        clickable = false;
        OnClickEvent.Invoke();
        RuntimeManager.PlayOneShot(rebornEvent);

        myTween.Kill();
        myTween = myRectTransform.DOScale(authoredScale * 1.6f, 2)
                                 .SetEase(Ease.OutExpo)
                                 .SetUpdate(UpdateType.Normal, true)
                                 .SetLink(gameObject);

        reveal.Kill();
        reveal = group.DOFade(0f, 2)
                      .SetEase(Ease.OutExpo)
                      .SetUpdate(UpdateType.Normal, true)
                      .SetLink(gameObject)
                      .OnComplete(SetScene);
    }

    private void SetScene()
    {
        if (sceneindex == -1) SceneFlow.Reload();
        else SceneFlow.Load(sceneindex);
    }
}
