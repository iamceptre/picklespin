using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum HudResource { Health = 0, Stamina = 1, Magicka = 2 }

public class PlayerClassHud : MonoBehaviour
{
    public static PlayerClassHud Instance { get; private set; }

    [Header("Bar roots (the whole bar object, renderer included)")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private GameObject staminaBarRoot;
    [SerializeField, Tooltip("stays on for every class - Umbral runs everything off it, painted black")]
    private GameObject magickaBarRoot;

    [Header("Bar icons - each moves to, and takes the colour of, the bar its resource lives in")]
    [SerializeField, Tooltip("heart")] private Graphic healthIcon;
    [SerializeField, Tooltip("boot")] private Graphic staminaIcon;
    [SerializeField, Tooltip("palm")] private Graphic magickaIcon;
    [SerializeField, Tooltip("offset applied per extra icon when several end up sharing one bar's row - keep it clear of the bar itself")]
    private Vector2 sharedRowOffset = new(-34f, 0f);
    [SerializeField, Tooltip("how long an icon takes to slide to its new row")]
    private float iconMoveDuration = 0.4f;

    private readonly Color healthLightColor = GameColors.HealthBright;
    private readonly Color staminaLightColor = GameColors.StaminaBright;
    private readonly Color magickaLightColor = GameColors.MagickaBright;

    private readonly Color umbralBarColor = GameColors.Dusk;
    private readonly Color umbralBackgroundColor = GameColors.Shadow;
    private readonly Color umbralGhostColor = GameColors.Ghost;
    private readonly Color umbralLightColor = GameColors.UmbralBright;

    [Header("Umbral")]
    [SerializeField, Tooltip("the magicka bar's background graphics - auto-found from each slider's \"Background\" child if left empty")]
    private Graphic[] magickaBarBackgrounds;

    [Header("Other HUD pieces")]
    [SerializeField, Tooltip("hidden for every class but Lightfoot - nobody else has a speed-damage multiplier to read")]
    private GameObject speedIndicatorRoot;
    [SerializeField, Tooltip("hidden for Umbral, who carries a single spell")]
    private GameObject spellInventoryBarRoot;
    [SerializeField, Tooltip("how long a piece takes to fade in when a class turns it on")]
    private float fadeInDuration = 0.4f;

    [Header("Change pulse - a piece breathes for a while after a class reshapes it")]
    [SerializeField, Tooltip("how long a changed piece keeps pulsing")]
    private float pulseDuration = 6f;
    [SerializeField, Tooltip("alpha the bar dips to at the bottom of a pulse")]
    private float pulseMinAlpha = 0.25f;
    [SerializeField, Tooltip("seconds for one full dim-and-back pulse")]
    private float pulsePeriod = 0.8f;

    private readonly List<Graphic> magickaFills = new();
    private readonly List<Color> magickaFillStartColors = new();
    private readonly List<Graphic> magickaGhostFills = new();
    private readonly List<Color> magickaGhostStartColors = new();
    private readonly List<Graphic> magickaBackgrounds = new();
    private readonly List<Color> magickaBackgroundStartColors = new();

    private const int Health = (int)HudResource.Health;
    private const int Stamina = (int)HudResource.Stamina;
    private const int Magicka = (int)HudResource.Magicka;
    private const int IconCount = 3;

    private readonly Graphic[] icons = new Graphic[IconCount];
    private readonly Color[] iconStartColors = new Color[IconCount];
    private readonly Vector2[] iconHomePositions = new Vector2[IconCount];
    private readonly int[] iconRow = new int[IconCount];

    private readonly Color[] resourceColors = new Color[IconCount];
    private readonly bool[] resourceColorKnown = new bool[IconCount];

    private readonly HudPiece[] bars = new HudPiece[IconCount];
    private readonly int[] barSignatures = new int[IconCount];
    private bool signaturesKnown;

    private HudPiece speedIndicator;
    private HudPiece spellInventoryBar;

    private class HudPiece
    {
        private readonly GameObject root;
        private readonly Canvas canvas;
        private readonly CanvasGroup group;
        private readonly Behaviour driver;
        private bool visible;

        public HudPiece(GameObject root)
        {
            this.root = root;
            canvas = Ensure<Canvas>(root);
            group = Ensure<CanvasGroup>(root);

            driver = root.GetComponent<SpeedIndicator>();
            visible = canvas.enabled;
        }

        private static T Ensure<T>(GameObject root) where T : Component =>
            root.TryGetComponent(out T component) ? component : root.AddComponent<T>();

        public bool Set(bool show, float fadeDuration)
        {
            if (show == visible) return false;
            visible = show;

            group.DOKill();

            if (show && !root.activeSelf) root.SetActive(true);
            canvas.enabled = show;
            if (driver) driver.enabled = show;
            if (!show) return true;

            group.alpha = 0f;
            group.DOFade(1f, fadeDuration).SetUpdate(true);
            return true;
        }

        public void Pulse(float duration, float minAlpha, float period, float fadeDuration)
        {
            if (!visible || duration <= 0f || period <= 0f) return;

            group.DOKill();

            int loops = Mathf.Max(2, Mathf.RoundToInt(duration / period) * 2);

            Sequence pulse = DOTween.Sequence().SetTarget(group).SetUpdate(true);
            if (group.alpha < 1f) pulse.Append(group.DOFade(1f, fadeDuration));
            pulse.Append(group.DOFade(minAlpha, period * 0.5f)
                .SetLoops(loops, LoopType.Yoyo)
                .SetEase(Ease.InOutSine));
            pulse.OnComplete(() => group.alpha = 1f);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        icons[Health] = healthIcon;
        icons[Stamina] = staminaIcon;
        icons[Magicka] = magickaIcon;
        for (int i = 0; i < IconCount; i++)
        {
            if (!icons[i]) continue;
            iconStartColors[i] = icons[i].color;
            iconHomePositions[i] = icons[i].rectTransform.anchoredPosition;
            resourceColors[i] = iconStartColors[i];
            resourceColorKnown[i] = true;
        }

        bars[Health] = Build(healthBarRoot);
        bars[Stamina] = Build(staminaBarRoot);
        bars[Magicka] = Build(magickaBarRoot);
        speedIndicator = Build(speedIndicatorRoot);
        spellInventoryBar = Build(spellInventoryBarRoot);

        CacheMagickaFills();
    }

    private static HudPiece Build(GameObject root) => root ? new HudPiece(root) : null;

    private void OnEnable() => PlayerClasses.Changed += Apply;

    private void OnDisable() => PlayerClasses.Changed -= Apply;

    private void Start() => Apply();

    public void Apply()
    {
        bool blackBar = PlayerClasses.Chosen == PlayerClassId.Umbral;

        Show(bars[Health], !PlayerClasses.MagickaIsHealth);
        Show(bars[Stamina], !PlayerClasses.StaminaSharesMagicka);
        Show(bars[Magicka], true);

        TintMagickaBar(blackBar);

        iconRow[Health] = PlayerClasses.MagickaIsHealth ? Magicka : Health;
        iconRow[Stamina] = PlayerClasses.StaminaSharesMagicka ? Magicka : Stamina;
        iconRow[Magicka] = Magicka;

        Color magickaColor = blackBar ? umbralBarColor : iconStartColors[Magicka];
        for (int i = 0; i < IconCount; i++)
        {
            Color rowColor = iconRow[i] == Magicka ? magickaColor : iconStartColors[iconRow[i]];
            Tint(icons[i], rowColor, iconStartColors[i].a);

            resourceColors[i] = rowColor;
            resourceColorKnown[i] = icons[iconRow[i]] || (iconRow[i] == Magicka && blackBar);
        }

        LayoutIcons();

        bool announce = signaturesKnown && PlayerClasses.WasOffered;
        signaturesKnown = true;

        PulseChangedBars(blackBar, announce);

        if (Show(speedIndicator, PlayerClasses.SpeedDamageActive) && announce) Pulse(speedIndicator);
        Show(spellInventoryBar, !PlayerClasses.LockedSpell.HasValue);
    }

    private bool Show(HudPiece piece, bool visible) => piece != null && piece.Set(visible, fadeInDuration);

    private void Pulse(HudPiece piece) =>
        piece?.Pulse(pulseDuration, pulseMinAlpha, pulsePeriod, fadeInDuration);

    private void PulseChangedBars(bool blackBar, bool announce)
    {
        for (int row = 0; row < IconCount; row++)
        {
            int signature = BarSignature(row, blackBar);
            if (signature == barSignatures[row]) continue;

            barSignatures[row] = signature;
            if (announce) Pulse(bars[row]);
        }
    }

    private int BarSignature(int row, bool blackBar)
    {
        int signature = 0;
        for (int i = 0; i < IconCount; i++)
        {
            if (iconRow[i] == row) signature |= 1 << i;
        }
        if (row == Magicka && blackBar) signature |= 1 << IconCount;
        return signature;
    }

    public Color BarLightColor(HudResource bar)
    {

        if (bar == HudResource.Magicka && PlayerClasses.Chosen == PlayerClassId.Umbral) return umbralLightColor;

        return bar switch
        {
            HudResource.Stamina => staminaLightColor,
            HudResource.Magicka => magickaLightColor,
            _ => healthLightColor
        };
    }

    public bool TryGetResourceColor(HudResource resource, out Color color)
    {
        int i = (int)resource;
        if (i < 0 || i >= IconCount || !resourceColorKnown[i])
        {
            color = GameColors.Neutral;
            return false;
        }
        color = resourceColors[i];
        return true;
    }

    private void LayoutIcons()
    {
        for (int row = 0; row < IconCount; row++)
        {
            Vector2 anchor = iconHomePositions[row];
            int placed = 0;

            if (iconRow[row] == row) MoveIcon(row, anchor, placed++);

            for (int i = 0; i < IconCount; i++)
            {
                if (i != row && iconRow[i] == row) MoveIcon(i, anchor, placed++);
            }
        }
    }

    private void MoveIcon(int icon, Vector2 rowAnchor, int placeInRow)
    {
        if (!icons[icon]) return;

        RectTransform rect = icons[icon].rectTransform;
        Vector2 target = rowAnchor + sharedRowOffset * placeInRow;

        rect.DOKill();

        rect.DOAnchorPos(target, iconMoveDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void CacheMagickaFills()
    {
        if (!magickaBarRoot) return;

        foreach (Slider slider in magickaBarRoot.GetComponentsInChildren<Slider>(true))
        {
            if (slider.fillRect && slider.fillRect.TryGetComponent(out Graphic fill))
            {

                bool isGhost = slider.GetComponent<BarEase>();
                (isGhost ? magickaGhostFills : magickaFills).Add(fill);
                (isGhost ? magickaGhostStartColors : magickaFillStartColors).Add(fill.color);
            }

            if (magickaBarBackgrounds != null && magickaBarBackgrounds.Length > 0) continue;
            Transform background = slider.transform.Find("Background");
            if (background && background.TryGetComponent(out Graphic backgroundGraphic))
            {
                Remember(backgroundGraphic);
            }
        }

        if (magickaBarBackgrounds == null) return;
        foreach (Graphic background in magickaBarBackgrounds) Remember(background);
    }

    private void Remember(Graphic background)
    {
        if (!background || magickaBackgrounds.Contains(background)) return;
        magickaBackgrounds.Add(background);
        magickaBackgroundStartColors.Add(background.color);
    }

    private void TintMagickaBar(bool black)
    {
        for (int i = 0; i < magickaFills.Count; i++)
        {
            if (!magickaFills[i]) continue;
            Tint(magickaFills[i], black ? umbralBarColor : magickaFillStartColors[i], magickaFillStartColors[i].a);
        }

        for (int i = 0; i < magickaGhostFills.Count; i++)
        {
            if (!magickaGhostFills[i]) continue;
            Tint(magickaGhostFills[i], black ? umbralGhostColor : magickaGhostStartColors[i], magickaGhostStartColors[i].a);
        }

        for (int i = 0; i < magickaBackgrounds.Count; i++)
        {
            if (!magickaBackgrounds[i]) continue;
            Tint(magickaBackgrounds[i], black ? umbralBackgroundColor : magickaBackgroundStartColors[i], magickaBackgroundStartColors[i].a);
        }
    }

    private static void Tint(Graphic target, Color color, float keepAlpha)
    {
        if (!target) return;
        color.a = keepAlpha;
        target.color = color;
    }
}
