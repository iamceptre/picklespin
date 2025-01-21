using System.Collections;
using UnityEngine;

public class MaterialFlashWhenHit : MonoBehaviour
{
    [SerializeField] Renderer rend;
    [SerializeField] float flashDuration = 0.4f;

   readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
    readonly WaitForSeconds timeBeforeFadingOutFlash = new(0.05f);
    readonly WaitForSeconds timeBeforeFadingOutHeadshotFlash = new(0.3f);

    Coroutine currentFlashCoroutine;
    Color whiteFlashColor = new(0.4f, 0.4f, 0.4f);
    Color redFlashColor = new(0.76f, 0.24f, 0.24f);
   readonly float fadeOutSpeed = 1.5f;

    public void Flash()
    {
        StartFlash(whiteFlashColor, timeBeforeFadingOutFlash);
    }

    public void FlashHeadshot()
    {
        StartFlash(redFlashColor, timeBeforeFadingOutHeadshotFlash);
    }

    void StartFlash(Color flashColor, WaitForSeconds waitTime)
    {
        if (currentFlashCoroutine != null)
            StopCoroutine(currentFlashCoroutine);
        currentFlashCoroutine = StartCoroutine(FlashRoutine(flashColor, waitTime));
    }

    IEnumerator FlashRoutine(Color flashColor, WaitForSeconds waitTime)
    {
        rend.material.SetColor(emissionColor, flashColor);
        yield return waitTime;

        float flashElapsed = flashDuration;
        while (flashElapsed > 0)
        {
            float t = flashElapsed / flashDuration;
            rend.material.SetColor(emissionColor, flashColor * t);
            flashElapsed -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        rend.material.SetColor(emissionColor, Color.black);
        currentFlashCoroutine = null;
    }

    public void ResetFlashState()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
            currentFlashCoroutine = null;
        }
        rend.material.SetColor(emissionColor, Color.black);
    }
}
