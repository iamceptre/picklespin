using UnityEngine;

public static class GameColors
{
    public static readonly Color Health = new(0.7911f, 0.2544f, 0.275f);
    public static readonly Color Stamina = new(0.3419f, 0.5315f, 0.213f);
    public static readonly Color Magicka = new(0.3425f, 0.4168f, 0.871f);
    public static readonly Color Umbral = new(0.6425f, 0.2611f, 0.8019f);

    public static readonly Color HealthBright = new(0.959f, 0.5574f, 0.5445f);
    public static readonly Color StaminaBright = new(0.4778f, 0.7842f, 0.2474f);
    public static readonly Color MagickaBright = new(0.5981f, 0.6709f, 0.9548f);
    public static readonly Color UmbralBright = new(0.8282f, 0.5621f, 0.9551f);
    public static readonly Color UmbralSurge = new(1f, 0.4207f, 0.9314f);
    public static readonly Color UmbralOverload = new(1f, 0.8534f, 1f);

    public static readonly Color Fireball = new(0.9629f, 0.6581f, 0.2835f);
    public static readonly Color Critical = new(1f, 0.8268f, 0.2071f);
    public static readonly Color Highlight = new(0.9791f, 0.8921f, 0.6276f);
    public static readonly Color Dusk = new(0.2896f, 0.2271f, 0.321f);

    public static readonly Color Shadow = new(0.0518f, 0.0518f, 0.0518f);
    public static readonly Color Dimmed = new(0.3775f, 0.3775f, 0.3775f);
    public static readonly Color Ghost = new(0.5254f, 0.5254f, 0.5254f);
    public static readonly Color Neutral = Color.white;
    public static readonly Color FadedWhite = new(1f, 1f, 1f, 0.5f);
    public static readonly Color ClearWhite = new(1f, 1f, 1f, 0f);
    public static readonly Color NegativeGlow = new(0f, 0f, 0f, 0.38f);

    public static readonly string HealthTag = Tag(HealthBright);
    public static readonly string StaminaTag = Tag(StaminaBright);
    public static readonly string MagickaTag = Tag(MagickaBright);
    public static readonly string UmbralTag = Tag(UmbralBright);

    public static readonly Color[] ScreenFlash =
    {
        Health.WithAlpha(0.05882353f),
        Stamina.WithAlpha(0.05882353f),
        Magicka.WithAlpha(0.05882353f),
        Umbral.WithAlpha(0.05882353f),
        Fireball.WithAlpha(0.14901961f),
        Dusk.WithAlpha(0.2509804f),
        Color.white.WithAlpha(0.7372549f)
    };

    public static string Tag(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);

    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
