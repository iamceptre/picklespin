using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public static class SilentWarmup
{
    private static readonly List<StudioEventEmitter> found = new();
    private static readonly List<StudioEventEmitter> silenced = new();

    public static void Run(GameObject pooled)
    {
        if (!pooled || pooled.activeSelf) return;

        pooled.GetComponentsInChildren(true, found);
        silenced.Clear();
        for (int i = 0; i < found.Count; i++)
        {
            if (!found[i].enabled) continue;
            found[i].enabled = false;
            silenced.Add(found[i]);
        }

        pooled.SetActive(true);
        pooled.SetActive(false);

        for (int i = 0; i < silenced.Count; i++) silenced[i].enabled = true;
    }
}
