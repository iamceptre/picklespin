using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSelectedScene : MonoBehaviour
{
    public int selectedSceneIndex;

    public void Do()
    {
        SwitchScene();
    }
    
    private void SwitchScene()
    {
        DG.Tweening.DOTween.KillAll();
        Debug.Log("switching scene");
        SceneManager.LoadScene(selectedSceneIndex);
        Time.timeScale = 1.0f;
    }
}
