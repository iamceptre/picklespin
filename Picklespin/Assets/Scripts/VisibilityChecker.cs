using UnityEngine;

public class VisibilityChecker : MonoBehaviour
{

    [SerializeField] private PerlinFloat perlinFloat;
    [SerializeField] private LightFluctuation lightFluctuation;
    private void OnBecameVisible()
    {
        perlinFloat.enabled = true;
        lightFluctuation.enabled = true;
    }

    private void OnBecameInvisible()
    {
        perlinFloat.enabled = false;
        lightFluctuation.enabled = false;
    }
}
