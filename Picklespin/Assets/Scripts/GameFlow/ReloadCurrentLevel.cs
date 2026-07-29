using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ReloadCurrentLevel : MonoBehaviour
{
    public void Reload()
    {
        FMODResetManager.instance.ResetFMOD(true);
        StartCoroutine(ReloadAtEndOfFrame());
    }

    // kill tweens at end of frame, after every script had its last update — killing
    // earlier leaves a gap where fresh tweens outlive their scene ("Tween startup failed")
    private IEnumerator ReloadAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _ = DOTween.KillAll();
        System.GC.Collect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
