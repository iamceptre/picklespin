using UnityEngine.Pool;

public static class PoolPrewarm
{
    public static void Prewarm<T>(this ObjectPool<T> pool, int count) where T : class
    {
        if (pool == null || count <= 0) return;

        var warmed = new T[count];

        for (int i = 0; i < count; i++)
        {
            warmed[i] = pool.Get();
        }

        for (int i = 0; i < count; i++)
        {
            pool.Release(warmed[i]);
        }
    }
}
