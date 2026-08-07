using System;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
public class MenuScreen : MonoBehaviour
{
    private const string stepSound = "event:/UI/UI_WHISPER";

    private static int pendingStep;
    private static MenuScreen pendingPage;

    [SerializeField, Tooltip("seconds this page takes to appear - unscaled, so it plays the same at timeScale 0")]
    private float fadeInDuration = 0.08f;

    [SerializeField, Tooltip("seconds this page takes to leave")]
    private float fadeOutDuration = 0.05f;

    [SerializeField, Tooltip("Played whenever this page opens - left empty it takes an emitter sitting on the page itself")]
    private StudioEventEmitter openSound;

    private Canvas canvas;
    private CanvasGroup group;
    private Tween fade;
    private bool stateSet;

    public bool IsOpen { get; private set; }

    public MenuScreen OpenedFrom { get; private set; }

    public event Action Opened;

    public event Action Closed;

    public static MenuScreen Of(GameObject page)
    {
        if (!page) return null;
        if (page.TryGetComponent(out MenuScreen screen)) return screen;

        if (!page.TryGetComponent(out Canvas _)) page.AddComponent<Canvas>();
        if (!page.TryGetComponent(out CanvasGroup _)) page.AddComponent<CanvasGroup>();

        return page.AddComponent<MenuScreen>();
    }

    public static MenuScreen Of(Canvas page) => page ? Of(page.gameObject) : null;

    public static void Step(MenuScreen from, MenuScreen to)
    {
        if (!to || from == to) return;

        to.OpenedFrom = from;
        int step = ++pendingStep;
        ClaimTint(to);

        if (from) from.SetOpen(false, () => { if (step == pendingStep) to.OpenWithSound(); });
        else to.OpenWithSound();
    }

    public static void StepBack(MenuScreen page)
    {
        if (!page) return;

        MenuScreen back = page.OpenedFrom;
        page.OpenedFrom = null;
        int step = ++pendingStep;

        if (!back)
        {
            page.Close();
            return;
        }

        ClaimTint(back);
        page.SetOpen(false, () => { if (step == pendingStep) back.OpenWithSound(); });
    }


    public static void CancelPendingSteps()
    {
        pendingStep++;

        if (pendingPage && !pendingPage.IsOpen) ScreenTint.Release(pendingPage);
        pendingPage = null;
    }


    private static void ClaimTint(MenuScreen page)
    {
        pendingPage = page;
        ScreenTint.Hold(page);
    }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        group = GetComponent<CanvasGroup>();
        group.enabled = true;

        if (!openSound) TryGetComponent(out openSound);

        MenuScreenPart[] parts = GetComponentsInChildren<MenuScreenPart>(true);
        for (int i = 0; i < parts.Length; i++) parts[i].Bind(this);
    }

    private void Start()
    {
        if (stateSet) return;

        ApplyImmediate(canvas.enabled && group.alpha > 0f);
    }

    public void Open() => SetOpen(true);

    public void Close() => SetOpen(false);

    public void Toggle() => SetOpen(!IsOpen);


    public void SetOpen(bool open, TweenCallback onDone = null)
    {
        if (open && !gameObject.activeInHierarchy)
            DevLog.Error($"{name}: asked to open while its GameObject is inactive - a page stays active for the whole run and shows itself by its Canvas", this);

        if (open && !stateSet) group.alpha = 0f;

        stateSet = true;

        if (IsOpen == open)
        {
            onDone?.Invoke();
            return;
        }

        IsOpen = open;
        fade.Kill();

        group.interactable = open;
        group.blocksRaycasts = open;

        if (open)
        {
            canvas.enabled = true;
            if (openSound) openSound.Play();
        }

        SetTint(open);
        if (open) Opened?.Invoke();

        fade = group.DOFade(open ? 1f : 0f, open ? fadeInDuration : fadeOutDuration)
                    .SetUpdate(true)
                    .SetLink(gameObject);

        if (open) fade.OnComplete(onDone);
        else fade.OnComplete(() => { HideCanvas(); onDone?.Invoke(); });
    }

    public void ApplyImmediate(bool open)
    {
        stateSet = true;
        IsOpen = open;
        fade.Kill();

        group.alpha = open ? 1f : 0f;
        group.interactable = open;
        group.blocksRaycasts = open;
        canvas.enabled = open;
        SetTint(open);
        if (open) Opened?.Invoke();
    }

    private void SetTint(bool open)
    {
        if (open) ScreenTint.Hold(this);
        else ScreenTint.Release(this);
    }

    private void OpenWithSound()
    {
        if (pendingPage == this) pendingPage = null;

        RuntimeManager.PlayOneShot(stepSound);
        Open();
    }

    private void HideCanvas()
    {
        canvas.enabled = false;
        Closed?.Invoke();
    }

    private void OnEnable()
    {
        if (IsOpen) ScreenTint.Hold(this);
    }

    private void OnDisable()
    {
        fade.Kill();
        ScreenTint.Release(this);
    }
}
