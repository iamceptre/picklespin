using System.Collections;
using UnityEngine;

public class SpawnInEnemy : MonoBehaviour
{

    [SerializeField] private ParticleSystem spawnInParticles;
    [SerializeField] private Renderer toFadeIn;
    private Color TransparentWhite = new Color(1, 1, 1, 0);
    private ParticleSystem.MainModule mainModule;
    [SerializeField] private float timeToSpawnIn;
    [SerializeField] private StateManager stateManager;

    private WaitForSeconds timeToSpawnInYield;

    private Color startingColor;

    private int colorProperty = Shader.PropertyToID("_Color");

    private IEnumerator Start()
    {
        toFadeIn.material.color = TransparentWhite;
        mainModule = spawnInParticles.main;
        mainModule.startLifetime = timeToSpawnIn;
        spawnInParticles.Play();
        timeToSpawnInYield = new WaitForSeconds(timeToSpawnIn);
        yield return timeToSpawnInYield;
        stateManager.StartAI();
        StartCoroutine(FadeInMaterial());
        yield break;
    }

    private IEnumerator FadeInMaterial()
    {
        Material mat = toFadeIn.material;
        startingColor = toFadeIn.material.color;
        float progress = 0;

        while (progress<=1)
        {
            progress += Time.deltaTime;
            Color color = new Color(startingColor.r, startingColor.g, startingColor.b, progress);
            mat.SetColor(colorProperty, color);
            yield return null;
        }

        mat.SetFloat("_Mode", 0);
    }

}
