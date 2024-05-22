using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSelectedScene : MonoBehaviour
{
    public int selectedSceneIndex;
    [SerializeField] Slider loadingBar;
    [SerializeField] private Canvas loadingBarCanvas;
    [SerializeField] private TMP_Text loadingText;



    public void Do()
    {
        if (loadingBar == null)
        {
            SwitchScene();
        }
        else
        {
            StartCoroutine(LoadNewGameWithBar());
        }

    }

    private void SwitchScene()
    {
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene(selectedSceneIndex);

    }

    private IEnumerator LoadNewGameWithBar()
    {
        DG.Tweening.DOTween.KillAll();
        loadingBarCanvas.enabled = true;
        yield return new WaitForEndOfFrame();
        AsyncOperation operation = SceneManager.LoadSceneAsync(selectedSceneIndex);
        Time.timeScale = 1.0f;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            loadingBar.value = progress;
            loadingText.text = (int)progress * 100 + "%";
            yield return null;
        }

    }
}
