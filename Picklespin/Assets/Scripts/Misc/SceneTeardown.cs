using UnityEngine;

public static class SceneTeardown
{
    public static bool IsUnloading(this GameObject gameObject) => !gameObject.scene.isLoaded;
}
