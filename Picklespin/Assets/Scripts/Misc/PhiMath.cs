using UnityEngine;

// The golden ratio and friends — the game's "natural math" foundation.
// φ is the most irrational number: sequences stepped by φ never lock into a
// repeating pattern and spread as evenly as mathematically possible.
// Nature uses this for sunflower seeds and leaf phyllotaxis; we use it for
// spawn scattering, timing and feel constants.
public static class PhiMath
{
    public const float PHI = 1.61803399f;       // φ
    public const float INV_PHI = 0.61803399f;   // 1/φ  = φ−1
    public const float INV_PHI2 = 0.38196601f;  // 1/φ²
    public const float PHI2 = 2.61803399f;      // φ²
    public const float PHI3 = 4.23606798f;      // φ³
    public const float PHI4 = 6.85410197f;      // φ⁴
    public const float PHI5 = 11.09016994f;     // φ⁵
    public const float GoldenAngleDeg = 137.50776f; // 360°·(1−1/φ)
    public const float GoldenAngleRad = 2.39996323f;

    // Vogel spiral: fills a disc perfectly evenly, no clustering, no gaps
    // (the sunflower seed packing). Great for group spawn offsets.
    public static Vector2 GoldenSpiralPoint(int index, int count, float radius)
    {
        float r = radius * Mathf.Sqrt((index + 0.5f) / count);
        float angle = index * GoldenAngleRad;
        return new Vector2(r * Mathf.Cos(angle), r * Mathf.Sin(angle));
    }

    // low-discrepancy 0..1 sequence (k·1/φ mod 1): feels random but never
    // streaks or clumps the way Random.value does
    public static float GoldenSequence(int index)
    {
        return (index * INV_PHI) % 1f;
    }
}
