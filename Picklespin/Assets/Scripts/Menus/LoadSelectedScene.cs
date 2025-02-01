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

    private FMODResetManager fmodResetManager;
    private AudioSnapshotManager audioSnapshotManager;

    [SerializeField] private FadeOutImageOnEnable fadeInVeins;
    [SerializeField] private GameObject fadeInGroup;

    private bool clickable = true;

    private void Start()
    {
        fmodResetManager = FMODResetManager.instance;
        audioSnapshotManager = AudioSnapshotManager.Instance;
    }



    public void Do()
    {
        if (clickable)
        {
            clickable = false;
            if (fmodResetManager != null)
                fmodResetManager.ResetFMOD(false);

            if (audioSnapshotManager != null)
                audioSnapshotManager.Clear();

            if (loadingBar == null)
            {
                SwitchScene();
            }
            else
            {
                StartCoroutine(LoadNewGameWithBar());
            }

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
        operation.allowSceneActivation = false;

        Time.timeScale = 1.0f;

        if (fadeInVeins != null)
        {
            fadeInGroup.SetActive(true);
            while (operation.progress < 0.9f || !fadeInVeins.fadedIn)
            {
                Loader(operation);
                yield return null;
            }
        }
        else
        {
            while (operation.progress < 0.9f)
            {
                Loader(operation);
                yield return null;
            }
        }

        clickable = true;
        operation.allowSceneActivation = true;
    }

    private void Loader(AsyncOperation operation)
    {
        float progress = Mathf.Clamp01(operation.progress / .9f);
        loadingBar.value = progress;
        loadingText.text = 100 * ((int)progress) + "%";
    }
}
