using UnityEngine;

public static class LoopingNoise
{
    public const int Size = 512;
    private const int Mask = Size - 1;

    private static readonly float[] table = Build();

    private static float[] Build()
    {
        float[] samples = new float[Size];
        for (int i = 0; i < Size; i++)
        {
            float angle = i * (2f * Mathf.PI / Size);
            samples[i] = Mathf.PerlinNoise(4f + Mathf.Cos(angle) * 3f, 4f + Mathf.Sin(angle) * 3f) * 2f - 1f;
        }
        return samples;
    }

    public static float Sample(float walk)
    {
        int index = (int)walk;
        float fraction = walk - index;
        float from = table[index & Mask];
        float to = table[(index + 1) & Mask];
        return from + (to - from) * fraction;
    }
}
