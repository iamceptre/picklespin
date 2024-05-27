using System.Collections;
using UnityEngine;

public class MaterialFlashWhenHit : MonoBehaviour
{

    [SerializeField] private Renderer rend;

    private float flashElapsed;
    [SerializeField] private float flashDuration = 0.5f;

    private int emissionColor = Shader.PropertyToID("_EmissionColor");


    public IEnumerator Flash()
    {
        Initialize();

        while (flashElapsed > 0)
        {
            float elapsedPercentage = flashElapsed / flashDuration;
            Color currentEmissionColor = new Color(elapsedPercentage * 0.5f, elapsedPercentage * 0.5f, elapsedPercentage * 0.5f);
            rend.material.SetColor(emissionColor, currentEmissionColor);
            flashElapsed -= Time.deltaTime;
            yield return null;
        }
    }

    private void Initialize()
    {
        flashElapsed = flashDuration;
        rend.material.EnableKeyword("_EMISSION");
    }

    public IEnumerator FlashHeadshot()
    {
        Initialize();

        while (flashElapsed > 0)
        {
            float elapsedPercentage = flashElapsed / flashDuration;
            Color currentEmissionColor = new Color(elapsedPercentage * 0.76f, elapsedPercentage * 0.24f, elapsedPercentage * 0.24f);
            rend.material.SetColor(emissionColor, currentEmissionColor);
            flashElapsed -= Time.deltaTime;
            yield return null;
        }
    }


}
