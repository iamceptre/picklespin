using System.Collections;
using UnityEngine;

public class MaterialFlashWhenHit : MonoBehaviour
{
    [SerializeField] private Renderer rend;
    [SerializeField] private float flashDuration = 0.4f;

    private int emissionColor = Shader.PropertyToID("_EmissionColor");

    private WaitForSeconds timeBeforeFadingOutFlash = new WaitForSeconds(0.05f);
    private WaitForSeconds timeBeforeFadingOutHeadshotFlash = new WaitForSeconds(0.3f);

    private Coroutine currentFlashCoroutine;

    private Color whiteFlashColor = new Color(0.4f, 0.4f, 0.4f);
    private Color RedFlashColor = new Color(0.76f, 0.24f, 0.24f);

    private float fadeOutSpeed = 1.5f;

    public void Flash()
    {
        StartFlash(whiteFlashColor, timeBeforeFadingOutFlash);
    }

    public void FlashHeadshot()
    {
        StartFlash(RedFlashColor, timeBeforeFadingOutHeadshotFlash);
    }

    private void StartFlash(Color flashColor, WaitForSeconds waitTime)
    {

        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }

        currentFlashCoroutine = StartCoroutine(FlashRoutine(flashColor, waitTime));
    }

    private IEnumerator FlashRoutine(Color flashColor, WaitForSeconds waitTime)
    {
        rend.material.SetColor(emissionColor, flashColor);

        yield return waitTime;

        float flashElapsed = flashDuration;
        while (flashElapsed > 0)
        {
            float elapsedPercentage = flashElapsed / flashDuration;
            Color currentEmissionColor = flashColor * elapsedPercentage;
            rend.material.SetColor(emissionColor, currentEmissionColor);
            flashElapsed -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        rend.material.SetColor(emissionColor, Color.black);
        currentFlashCoroutine = null;
    }
}