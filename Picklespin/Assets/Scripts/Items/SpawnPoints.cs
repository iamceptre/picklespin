using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints
{
    private readonly Transform[] points;
    private readonly bool[] taken;
    private readonly List<int> freeBuffer = new();

    private int takenCount;

    public SpawnPoints(Transform[] points)
    {
        this.points = points;
        taken = new bool[points.Length];
    }

    public int FreeCount => points.Length - takenCount;

    public Vector3 PositionOf(int point) => points[point].position;

    public bool TryReserve(out int point, Func<Vector3, bool> isUsable = null)
    {
        freeBuffer.Clear();

        for (int i = 0; i < points.Length; i++)
        {
            if (taken[i] || !points[i]) continue;
            if (isUsable != null && !isUsable(points[i].position)) continue;

            freeBuffer.Add(i);
        }

        if (freeBuffer.Count == 0)
        {
            point = -1;
            return false;
        }

        point = freeBuffer[UnityEngine.Random.Range(0, freeBuffer.Count)];
        Reserve(point);
        return true;
    }

    public void Reserve(int point)
    {
        if (taken[point]) return;

        taken[point] = true;
        takenCount++;
    }

    public void Release(int point)
    {
        if (point < 0 || point >= taken.Length || !taken[point]) return;

        taken[point] = false;
        takenCount--;
    }
}
