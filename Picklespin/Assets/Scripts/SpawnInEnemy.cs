using System.Collections;
using UnityEngine;

public class SpawnInEnemy : MonoBehaviour
{

    [SerializeField] private ParticleSystem spawnInParticles;
    [SerializeField] private Renderer toFadeIn;
    private ParticleSystem.MainModule mainModule;
    [SerializeField] private float timeToSpawnIn;
    [SerializeField] private StateManager stateManager;

    private WaitForSeconds timeToSpawnInYield;

    private Color startingColor;

    private int colorProperty = Shader.PropertyToID("_Color");

    private IEnumerator Start()
    {
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

        while (true)
        {
            progress += Time.deltaTime;
            Color color = new Color(startingColor.r, startingColor.g, startingColor.b, progress);
            mat.SetColor(colorProperty, color);
            if (progress>=1)
            {
                mat.SetFloat("_Mode", 0);
                yield break;
            }
            yield return null;
        }
    }

}
