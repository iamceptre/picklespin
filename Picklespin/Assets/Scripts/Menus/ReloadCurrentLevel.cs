using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadCurrentLevel : MonoBehaviour
{
    public void ReloadScene()
    {
        DOTween.KillAll();
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        Time.timeScale = 1;
    }
}