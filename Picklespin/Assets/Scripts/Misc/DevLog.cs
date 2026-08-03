using System.Diagnostics;
using UnityEngine;

public static class DevLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(string message, Object context = null) => UnityEngine.Debug.Log(message, context);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(string message, Object context = null) => UnityEngine.Debug.LogWarning(message, context);

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Error(string message, Object context = null) => UnityEngine.Debug.LogError(message, context);
}
