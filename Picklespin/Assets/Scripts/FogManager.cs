using UnityEngine;
using DG.Tweening;
using System.Collections;
using VolumetricFogAndMist;
using System.Runtime.CompilerServices;

public class FogManager : MonoBehaviour
{
    [SerializeField] private VolumetricFog volumetricFog; 
    [SerializeField] private SetDustIntensity dustIntensity;
    [SerializeField] private float[] fogLevels;
    private float currentFogLevel;
    [SerializeField] private Color[] fogColors;
    private Color currentFogColor;
    private Color exposureBoost = new Color(0.8f, 0.8f, 0.8f);


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
        StartCoroutine(VolumetricAlphaTranstitioner());
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
            volumetricFog.skyColor = currentFogColor + exposureBoost;
            //Debug.Log(currentFogColor);
            t += (Time.deltaTime / animationDuration);
            yield return null;
        }
        RenderSettings.fogColor = fogColors[index];
        yield break;
    }

    private IEnumerator VolumetricAlphaTranstitioner()
    {
        float t = 0;

        while (t <= 1)
        {
            volumetricFog.alpha = Mathf.Lerp(volumetricFog.alpha, currentFogLevel * 20, t);
            volumetricFog.skyAlpha = Mathf.Lerp(volumetricFog.skyAlpha, currentFogLevel * 7, t);
            t += (Time.deltaTime / animationDuration);
            yield return null;
        }
        volumetricFog.alpha = currentFogLevel * 20;
        yield break;
    }

}
