using UnityEditor;
using UnityEngine;

public class UnusedAssetsFinder : EditorWindow
{
    [MenuItem("Tools/Find Unused Assets")]
    public static void FindUnusedAssets()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string path in assetPaths)
        {
            if (!path.StartsWith("Assets/")) continue;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (AssetDatabase.GetDependencies(path, true).Length == 0)
            {
                UnityEngine.Debug.Log("Unused Asset: " + path, asset);
            }
        }
    }
}
