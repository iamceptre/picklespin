using System;
using System.Collections.Generic;
using UnityEngine;

// One answer to "can the player open the pause menu right now". Anything that owns the screen
// or the clock - a choice menu, the death screen, a scene change - holds the gate while it does.
public static class PauseGate
{
    private static readonly HashSet<UnityEngine.Object> blockers = new();
    private static readonly Predicate<UnityEngine.Object> destroyed = blocker => !blocker;

    public static bool Blocked
    {
        get
        {
            if (SceneFlow.IsLeaving) return true;

            if (blockers.Count > 0) blockers.RemoveWhere(destroyed);
            return blockers.Count > 0;
        }
    }

    public static void Block(UnityEngine.Object source)
    {
        if (source) blockers.Add(source);
    }

    public static void Release(UnityEngine.Object source)
    {
        if (source) blockers.Remove(source);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => blockers.Clear();
}
