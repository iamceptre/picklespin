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

    [Header("Bar light glow - one colour per bar type, set by hand for each")]
    [SerializeField, Tooltip("FF7C7C")] private Color healthLightColor = new(1f, 0.4862745f, 0.4862745f);
    [SerializeField, Tooltip("ADE78A")] private Color staminaLightColor = new(0.6784314f, 0.9058824f, 0.5411765f);
    [SerializeField, Tooltip("8181FF")] private Color magickaLightColor = new(0.5058824f, 0.5058824f, 1f);

    [Header("Umbral")]
    [SerializeField, Tooltip("the magicka bar's fill and all three icons take this colour")]
    private Color umbralBarColor = new(0.06f, 0.05f, 0.08f, 1f);
    [SerializeField, Tooltip("the empty part of the bar - lighter than the fill, so a black bar still reads at a glance")]
    private Color umbralBackgroundColor = new(0.42f, 0.41f, 0.46f, 1f);
    [SerializeField, Tooltip("the BarEase damage ghost - must differ from the fill, or the trailing shadow reads as the bar itself lagging")]
    private Color umbralGhostColor = new(0.24f, 0.22f, 0.28f, 1f);
    [SerializeField, Tooltip("the glow over the black bar - every resource Umbral spends flashes there")]
    private Color umbralLightColor = new(0.7176471f, 0.6509804f, 0.9098039f);
    [SerializeField, Tooltip("the magicka bar's background graphics - auto-found from each slider's \"Background\" child if left empty")]
    private Graphic[] magickaBarBackgrounds;

    [Header("Other HUD pieces")]
    [SerializeField, Tooltip("hidden for every class but Lightfoot - nobody else has a speed-damage multiplier to read")]
    private GameObject speedIndicatorRoot;
    [SerializeField, Tooltip("hidden for Umbral, who carries a single spell")]
    private GameObject spellInventoryBarRoot;
    [SerializeField, Tooltip("how long a piece takes to fade in when a class turns it on")]
    private float fadeInDuration = 0.4f;

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

    private HudPiece healthBar;
    private HudPiece staminaBar;
    private HudPiece magickaBar;
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

        public void Set(bool show, float fadeDuration)
        {
            if (show == visible) return;
            visible = show;

            group.DOKill();

            if (show && !root.activeSelf) root.SetActive(true);
            canvas.enabled = show;
            if (driver) driver.enabled = show;
            if (!show) return;

            group.alpha = 0f;
            group.DOFade(1f, fadeDuration).SetUpdate(true);
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

        healthBar = Build(healthBarRoot);
        staminaBar = Build(staminaBarRoot);
        magickaBar = Build(magickaBarRoot);
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

        Show(healthBar, !PlayerClasses.MagickaIsHealth);
        Show(staminaBar, !PlayerClasses.StaminaSharesMagicka);
        Show(magickaBar, true);

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

        Show(speedIndicator, PlayerClasses.SpeedDamageActive);
        Show(spellInventoryBar, PlayerClasses.LockedSpellIndex < 0);
    }

    private void Show(HudPiece piece, bool visible) => piece?.Set(visible, fadeInDuration);

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
            color = Color.white;
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
