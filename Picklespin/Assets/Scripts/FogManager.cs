using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FogManager : MonoBehaviour
{
    [SerializeField] private SetDustIntensity dustIntensity;
    [SerializeField] private float[] fogLevels;
    private float currentFogLevel;
    [SerializeField] private Color[] fogColors;
    private Color currentFogColor;


    [SerializeField] private float animationDuration = 4;

    void Start()
    {
        currentFogLevel = RenderSettings.fogDensity;
        currentFogColor = RenderSettings.fogColor;
    }


    public void SetFogLevel(int index)
    {
        DOTween.To(() => currentFogLevel, x => currentFogLevel = x, fogLevels[index], animationDuration)
    .OnUpdate(() => {
        RenderSettings.fogDensity = currentFogLevel;
    });

        dustIntensity.SetIntensity(currentFogLevel * 7);

    }

    public void SetFogColor(int index)
    {
        StartCoroutine(ColorTranstitioner(index));
    }

    private IEnumerator ColorTranstitioner(int index)
    {
        float t = 0;
        RenderSettings.fogColor = currentFogColor;
        while (t <= 1)
        {
            currentFogColor = Color.Lerp(currentFogColor, fogColors[index], t);
            RenderSettings.fogColor = currentFogColor;
            t += (Time.deltaTime / animationDuration);
            yield return null;
        }
        RenderSettings.fogColor = fogColors[index];
        yield break;
    }

}
