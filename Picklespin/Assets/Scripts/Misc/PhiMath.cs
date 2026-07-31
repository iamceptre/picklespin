using UnityEngine;

// φ is the most irrational number: sequences stepped by it never lock into a
// repeating pattern. A math utility, not a house style - use it only where that
// property does real work, and plain readable numbers everywhere else.
public static class PhiMath
{
    public const float PHI = 1.61803399f;       // φ
    public const float INV_PHI = 0.61803399f;   // 1/φ  = φ−1
    public const float PHI4 = 6.85410197f;      // φ⁴
    public const float GoldenAngleRad = 2.39996323f;

    // Vogel spiral: fills a disc evenly, no clustering, no gaps
    public static Vector2 GoldenSpiralPoint(int index, int count, float radius)
    {
        float r = radius * Mathf.Sqrt((index + 0.5f) / count);
        float angle = index * GoldenAngleRad;
        return new Vector2(r * Mathf.Cos(angle), r * Mathf.Sin(angle));
    }

    // low-discrepancy 0..1 sequence: feels random, never streaks or clumps
    public static float GoldenSequence(int index)
    {
        return (index * INV_PHI) % 1f;
    }

    // the unshifted sequence is deterministic and starts at 0, so index 0 always
    // lands on the same element; a per-run offset decorrelates runs
    public static float GoldenSequence(int index, float offset)
    {
        return (offset + index * INV_PHI) % 1f;
    }
}
