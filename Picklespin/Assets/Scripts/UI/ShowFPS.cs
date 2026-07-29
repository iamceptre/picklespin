using UnityEngine;
using TMPro;

public class ShowFPS : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float refreshInterval = 0.25f;

    private float deltaTime;
    private float nextRefreshTime;
    private int lastDisplayedFps = -1;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        if (Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + refreshInterval;

        int fps = Mathf.CeilToInt(1.0f / deltaTime);
        if (fps != lastDisplayedFps)
        {
            lastDisplayedFps = fps;
            fpsText.text = fps.ToString();
        }
    }
}
