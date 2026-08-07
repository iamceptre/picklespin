using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScreenTint : MonoBehaviour
{
    [System.Serializable]
    private class Entry
    {
        public MenuScreen screen;
        [Range(0f, 1f)] public float opacity = 1f;
        [Tooltip("seconds to reach that darkness - 0 uses the shared fade in duration. Only the portal-fail blackout wants a slow one")]
        public float seconds;
    }

    public static ScreenTint Instance { get; private set; }

    [SerializeField, Tooltip("the shared tint the menus sit on")]
    private Image tint;

    [SerializeField, Tooltip("how dark each page wants the tint. Only pages that want something other than the default need listing")]
    private Entry[] screens = new Entry[0];

    [SerializeField, Range(0f, 1f), Tooltip("default tint for non-listed pages")]
    private float defaultOpacity = 1f;

    [SerializeField, Tooltip("HUD that gets out of the way while any page is up - crosshair, bars, tips. Each fades with the tint and its Canvas is switched off while it is gone. These are shown at full whenever no page is up, so don't list one whose alpha another system owns")]
    private CanvasGroup[] hiddenBehindMenus = new CanvasGroup[0];

    [SerializeField, Tooltip("seconds to darken - runs on unscaled time, so it still plays while the game is paused")]
    private float fadeInDuration = 0.16f;

    [SerializeField, Tooltip("seconds to clear")]
    private float fadeOutDuration = 0.1f;

    private readonly List<MenuScreen> holders = new List<MenuScreen>();
    private readonly Dictionary<MenuScreen, Entry> entries = new Dictionary<MenuScreen, Entry>();
    private CanvasGroup group;
    private Canvas[] hiddenCanvases;
    private bool[] hudCanvasWasOn;
    private float hudAlpha = 1f;
    private bool hudHidden;
    private Tween hudFade;
    private int settledFrame;
    private Tween fade;

    public static void Hold(MenuScreen screen)
    {
        if (Instance) Instance.HoldFor(screen);
    }

    public static void Release(MenuScreen screen)
    {
        if (Instance) Instance.ReleaseFor(screen);
    }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            DevLog.Warn($"{nameof(ScreenTint)}: this scene has more than one, the first one keeps the job", this);
            enabled = false;
            return;
        }
        Instance = this;
        settledFrame = Time.frameCount;

        if (!tint)
        {
            DevLog.Error($"{nameof(ScreenTint)}: no tint image wired in - the menus come up untinted", this);
            enabled = false;
            return;
        }

        if (tint.color.a <= 0f) DevLog.Warn($"{nameof(ScreenTint)}: the tint image is authored fully transparent, so the menus will come up over nothing until its colour is given some alpha", this);

        if (!tint.TryGetComponent(out group)) group = tint.gameObject.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        tint.enabled = false;

        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] != null && screens[i].screen) entries[screens[i].screen] = screens[i];
        }

        hiddenCanvases = new Canvas[hiddenBehindMenus.Length];
        hudCanvasWasOn = new bool[hiddenBehindMenus.Length];
        for (int i = 0; i < hiddenBehindMenus.Length; i++)
        {
            if (hiddenBehindMenus[i]) hiddenBehindMenus[i].TryGetComponent(out hiddenCanvases[i]);
        }

        // the HUD's alpha is put back rather than trusted: a run that ended with a menu up can
        // leave a faded group behind it in the editor. Whether its Canvas is on is left alone -
        // that one belongs to whoever authored it, and to whatever shows it a tip at a time
        ApplyHudAlpha(1f);
    }

    private void OnDestroy()
    {
        fade.Kill();
        hudFade.Kill();
        if (Instance == this) Instance = null;
    }

    private void HoldFor(MenuScreen screen)
    {
        if (!screen) return;

        int at = holders.IndexOf(screen);
        if (at >= 0 && at == holders.Count - 1) return;

        if (at >= 0) holders.RemoveAt(at);
        holders.Add(screen);
        Retarget();
    }

    private void ReleaseFor(MenuScreen screen)
    {
        if (!screen) return;

        int at = holders.IndexOf(screen);
        if (at < 0) return;

        holders.RemoveAt(at);
        Retarget();
    }

    private void Retarget()
    {
        if (!tint) return;

        MenuScreen top = TopHolder();
        Entry wanted = null;
        if (top) entries.TryGetValue(top, out wanted);

        float target = !top ? 0f : wanted != null ? wanted.opacity : defaultOpacity;
        SetHudHidden(holders.Count > 0);

        float current = group.alpha;
        fade.Kill();

        if (target > 0f) tint.enabled = true;

        // the first frame is the scene settling into how it was authored - a menu that starts open
        // starts tinted, rather than fading in over a page that is already there
        if (Time.frameCount == settledFrame || Mathf.Approximately(current, target))
        {
            group.alpha = target;
            if (target <= 0f) tint.enabled = false;
            return;
        }

        bool darkening = target > current;
        float duration = darkening ? fadeInDuration : fadeOutDuration;
        if (darkening && wanted != null && wanted.seconds > 0f) duration = wanted.seconds;

        fade = group.DOFade(target, duration)
                    .SetUpdate(true)
                    .SetLink(gameObject);

        if (target <= 0f) fade.OnComplete(() => tint.enabled = false);
    }

    // the HUD leaves on the tint's way in and comes back on its way out, so it moves with it
    // rather than with any one page - stepping between pages never brings it back for a moment
    private void SetHudHidden(bool hidden)
    {
        if (hudHidden == hidden || hiddenBehindMenus.Length == 0) return;

        hudHidden = hidden;
        hudFade.Kill();

        if (hidden) RememberHudCanvases();
        else RestoreHudCanvases();

        float target = hidden ? 0f : 1f;

        if (Time.frameCount == settledFrame || Mathf.Approximately(hudAlpha, target))
        {
            ApplyHudAlpha(target);
            if (hidden) TurnOffHudCanvases();
            return;
        }

        hudFade = DOTween.To(() => hudAlpha, ApplyHudAlpha, target, hidden ? fadeInDuration : fadeOutDuration)
                         .SetUpdate(true)
                         .SetLink(gameObject);

        if (hidden) hudFade.OnComplete(TurnOffHudCanvases);
    }

    private void ApplyHudAlpha(float alpha)
    {
        hudAlpha = alpha;

        for (int i = 0; i < hiddenBehindMenus.Length; i++)
        {
            if (hiddenBehindMenus[i]) hiddenBehindMenus[i].alpha = alpha;
        }
    }

    // a canvas comes back the way it was found, not switched on: one of these is a tip that was
    // only up for a moment, and it must not be waiting there when the menu closes
    private void RememberHudCanvases()
    {
        for (int i = 0; i < hiddenCanvases.Length; i++)
        {
            if (hiddenCanvases[i]) hudCanvasWasOn[i] = hiddenCanvases[i].enabled;
        }
    }

    private void TurnOffHudCanvases()
    {
        for (int i = 0; i < hiddenCanvases.Length; i++)
        {
            if (hiddenCanvases[i]) hiddenCanvases[i].enabled = false;
        }
    }

    private void RestoreHudCanvases()
    {
        for (int i = 0; i < hiddenCanvases.Length; i++)
        {
            if (hiddenCanvases[i]) hiddenCanvases[i].enabled = hudCanvasWasOn[i];
        }
    }

    private MenuScreen TopHolder()
    {
        for (int i = holders.Count - 1; i >= 0; i--)
        {
            if (holders[i]) return holders[i];

            holders.RemoveAt(i);
        }

        return null;
    }
}
