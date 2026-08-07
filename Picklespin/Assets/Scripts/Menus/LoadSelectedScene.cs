using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadSelectedScene : MonoBehaviour
{
    // a held-back load reports 0.9 when it is done and waiting for activation
    private const float readyProgress = 0.9f;
    private const float progressToFraction = 1f / readyProgress;

    [SerializeField, Tooltip("build index of the scene this button goes to")]
    private int selectedSceneIndex;

    [Header("Loading bar - without one the scene is loaded outright")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private Canvas loadingBarCanvas;
    [SerializeField] private TMP_Text loadingText;

    [SerializeField, Tooltip("optional - the new scene is held back until this has finished fading in")]
    private FadeOutImageOnEnable fadeInVeins;
    [SerializeField] private GameObject fadeInGroup;

    public void Do()
    {
        if (SceneFlow.IsLeaving) return;

        if (!loadingBar)
        {
            SceneFlow.Load(selectedSceneIndex);
            return;
        }

        StartCoroutine(LoadWithBar());
    }

    private IEnumerator LoadWithBar()
    {
        AsyncOperation operation = SceneFlow.LoadAsync(selectedSceneIndex);
        if (operation == null) yield break;

        if (loadingBarCanvas) loadingBarCanvas.enabled = true;
        if (fadeInGroup) fadeInGroup.SetActive(true);

        while (operation.progress < readyProgress || (fadeInVeins && !fadeInVeins.fadedIn))
        {
            ShowProgress(operation.progress);
            yield return null;
        }

        ShowProgress(readyProgress);
        operation.allowSceneActivation = true;
    }

    private void ShowProgress(float rawProgress)
    {
        float progress = Mathf.Clamp01(rawProgress * progressToFraction);
        loadingBar.value = progress;
        if (loadingText) loadingText.text = (int)(100f * progress) + "%";
    }
}
