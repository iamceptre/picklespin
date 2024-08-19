using FMODUnity;
using System.Collections;
using UnityEngine;

public class SpawnInEnemy : MonoBehaviour
{
    [SerializeField] private ParticleSystem spawnInParticles;
    [SerializeField] private Renderer toFadeIn;
    [SerializeField] private float timeToSpawnIn;
    [SerializeField] private StateManager stateManager;

    private static readonly Color TransparentWhite = new Color(1, 1, 1, 0);
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private Material material;
    private WaitForSeconds timeToSpawnInYield;

    private void Awake()
    {
        // Cache the material and pre-allocate the wait time
        material = toFadeIn.material;
        timeToSpawnInYield = new WaitForSeconds(timeToSpawnIn);

        // Initialize the material
        InitializeMaterialForFade();
    }

    private IEnumerator Start()
    {
        PlaySpawnParticles();
        yield return timeToSpawnInYield;

        yield return FadeInMaterial();
    }

    private void PlaySpawnParticles()
    {
        var mainModule = spawnInParticles.main;
        mainModule.startLifetime = timeToSpawnIn;
        spawnInParticles.Play();
    }

    private void InitializeMaterialForFade()
    {
        material.SetColor(ColorProperty, TransparentWhite);
        // SetMaterialToFadeMode();
    }

    private IEnumerator FadeInMaterial()
    {
        float elapsedTime = 0f;
        float fadeSpeed = 1f / timeToSpawnIn;

        Color startingColor = TransparentWhite;

        while (elapsedTime < timeToSpawnIn)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime * fadeSpeed);
            Color currentColor = new Color(startingColor.r, startingColor.g, startingColor.b, progress);
            material.SetColor(ColorProperty, currentColor);
            yield return null;
        }

        material.SetColor(ColorProperty, Color.white);
        material.SetInt("_ZWrite", 1);
        stateManager.StartAI();
    }

    private void SetMaterialToFadeMode()
    {
        material.SetFloat("_Mode", 2);
        material.SetInteger("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInteger("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInteger("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void SetMaterialToOpaqueMode()
    {
        material.SetFloat("_Mode", 0);
        //material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInteger("_ZWrite", 1);
        //material.DisableKeyword("_ALPHATEST_ON");
        //material.DisableKeyword("_ALPHABLEND_ON");
        //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
    }
}