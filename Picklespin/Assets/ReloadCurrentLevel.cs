using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadCurrentLevel : MonoBehaviour
{
    public void ReloadScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}