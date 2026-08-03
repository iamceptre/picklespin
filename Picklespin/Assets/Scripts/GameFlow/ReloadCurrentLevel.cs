using UnityEngine;
using UnityEngine.SceneManagement;
public class ReloadCurrentLevel : MonoBehaviour
{
    public void Reload()
    {
        FMODResetManager.instance.ResetFMOD(true);
        System.GC.Collect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
