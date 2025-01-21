using FMODUnity;
using System.Collections;
using UnityEngine;

public class SpawnInEnemy : MonoBehaviour
{
    [SerializeField] ParticleSystem spawnInParticles;
    [SerializeField] Renderer toFadeIn;
    [SerializeField] float timeToSpawnIn;
    [SerializeField] StateManager stateManager;

    static readonly Color TransparentWhite = new Color(1, 1, 1, 0);
    static readonly int ColorProperty = Shader.PropertyToID("_Color");
    Material material;
    WaitForSeconds timeToSpawnInYield;
    Color originalColor = Color.white;

    void Awake()
    {
        material = toFadeIn.material;
        timeToSpawnInYield = new WaitForSeconds(timeToSpawnIn);
        material.SetColor(ColorProperty, TransparentWhite);
    }

    IEnumerator Start()
    {
        PlaySpawnParticles();
        yield return timeToSpawnInYield;
        yield return FadeInMaterial();
        stateManager.StartAI();
    }

    void PlaySpawnParticles()
    {
        var mainModule = spawnInParticles.main;
        mainModule.startLifetime = timeToSpawnIn;
        spawnInParticles.Play();
    }

    IEnumerator FadeInMaterial()
    {
        float elapsedTime = 0f;
        float fadeSpeed = 1f / timeToSpawnIn;

        while (elapsedTime < timeToSpawnIn)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime * fadeSpeed);
            material.SetColor(ColorProperty, new Color(1f, 1f, 1f, progress));
            yield return null;
        }
        material.SetColor(ColorProperty, originalColor);
        material.SetInt("_ZWrite", 1);
    }

    public void ResetSpawnState()
    {
        material.SetColor(ColorProperty, TransparentWhite);
        material.SetInt("_ZWrite", 0);
    }
}
