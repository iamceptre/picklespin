using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public static class PerformanceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Apply()
    {
        DebugManager.instance.enableRuntimeUI = false;

        InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
        InputSystem.settings.SetInternalFeatureFlag("USE_READ_VALUE_CACHING", true);
        InputSystem.settings.maxEventBytesPerUpdate = 512 * 1024;

        _ = LoopingNoise.Sample(0f);

        DevLog.Info("PerformanceBootstrap: rendering-debugger runtime off, Input System fast paths on");
    }
}
