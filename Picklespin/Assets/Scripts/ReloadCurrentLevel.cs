using UnityEngine;
using UnityEngine.SceneManagement;
public class ReloadCurrentLevel : MonoBehaviour
{
    public void Reload()
    {
        Scene scene = SceneManager.GetActiveScene();
        FMODResetManager.instance.ResetFMOD(true);
        System.GC.Collect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
