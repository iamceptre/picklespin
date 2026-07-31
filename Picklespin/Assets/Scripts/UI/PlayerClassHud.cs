using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// the three player resources, in the order BarLightsAnimation numbers its bars
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

    [Header("Umbral")]
    [SerializeField, Tooltip("the magicka bar's fill and all three icons take this colour")]
    private Color umbralBarColor = new(0.06f, 0.05f, 0.08f, 1f);
    [SerializeField, Tooltip("the empty part of the bar - lighter than the fill, so a black bar still reads at a glance")]
    private Color umbralBackgroundColor = new(0.42f, 0.41f, 0.46f, 1f);
    [SerializeField, Tooltip("the BarEase damage ghost - must differ from the fill, or the trailing shadow reads as the bar itself lagging")]
    private Color umbralGhostColor = new(0.24f, 0.22f, 0.28f, 1f);
    [SerializeField, Tooltip("the magicka bar's background graphics - auto-found from each slider's \"Background\" child if left empty")]
    private Graphic[] magickaBarBackgrounds;

    [Header("Other HUD pieces")]
    [SerializeField, Tooltip("hidden for every class but Lightfoot - nobody else has a speed-damage multiplier to read")]
    private GameObject speedIndicatorRoot;
    [SerializeField, Tooltip("hidden for Umbral, who carries a single spell")]
    private GameObject spellInventoryBarRoot;

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

        CacheMagickaFills();
    }

    private void OnEnable() => PlayerClasses.Changed += Apply;

    // a static event outlives the scene: never leave a destroyed HUD subscribed
    private void OnDisable() => PlayerClasses.Changed -= Apply;

    // PlayerHP and Ammo assign their instances in Awake, and Apply talks to both
    private void Start() => Apply();

    public void Apply()
    {
        bool blackBar = PlayerClasses.Chosen == PlayerClassId.Umbral;

        SetActive(healthBarRoot, !PlayerClasses.MagickaIsHealth);
        SetActive(staminaBarRoot, !PlayerClasses.StaminaSharesMagicka);
        SetActive(magickaBarRoot, true);

        TintMagickaBar(blackBar);

        iconRow[Health] = PlayerClasses.MagickaIsHealth ? Magicka : Health;
        iconRow[Stamina] = PlayerClasses.StaminaSharesMagicka ? Magicka : Stamina;
        iconRow[Magicka] = Magicka;

        Color magickaColor = blackBar ? umbralBarColor : iconStartColors[Magicka];
        for (int i = 0; i < IconCount; i++)
        {
            Color rowColor = iconRow[i] == Magicka ? magickaColor : iconStartColors[iconRow[i]];
            Tint(icons[i], rowColor, iconStartColors[i].a);

            // Umbral's row is black with or without a palm icon to sample
            resourceColors[i] = rowColor;
            resourceColorKnown[i] = icons[iconRow[i]] || (iconRow[i] == Magicka && blackBar);
        }

        LayoutIcons();

        SetActive(speedIndicatorRoot, PlayerClasses.SpeedDamageActive);
        SetActive(spellInventoryBarRoot, PlayerClasses.LockedSpellIndex < 0);
    }

    // false when that row has no icon to sample: the caller keeps its own colour
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

    // a row keeps its own icon and newcomers stack beside it; a bar whose icon left
    // never gains one, so the two cases cannot collide
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
        // unscaled: the class is taken from a menu that runs on unscaled time
        rect.DOAnchorPos(target, iconMoveDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void CacheMagickaFills()
    {
        if (!magickaBarRoot) return;

        foreach (Slider slider in magickaBarRoot.GetComponentsInChildren<Slider>(true))
        {
            if (slider.fillRect && slider.fillRect.TryGetComponent(out Graphic fill))
            {
                // the damage ghost must not take the fill's colour: two black bars on
                // top of each other read as one bar lagging the pool
                bool isGhost = slider.GetComponent<BarEase>();
                (isGhost ? magickaGhostFills : magickaFills).Add(fill);
                (isGhost ? magickaGhostStartColors : magickaFillStartColors).Add(fill.color);
            }

            // "Background" is Unity's stock slider child; only used when nothing was wired
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

    private static void SetActive(GameObject target, bool active)
    {
        if (target && target.activeSelf != active) target.SetActive(active);
    }
}
