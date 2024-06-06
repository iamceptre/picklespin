using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitAndLoadMenu : MonoBehaviour
{

    [SerializeField] private float timeToWait = 1;


    private IEnumerator Start()
    {
        DG.Tweening.DOTween.KillAll();
        yield return new WaitForSecondsRealtime(timeToWait);
        DG.Tweening.DOTween.KillAll();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }
}
