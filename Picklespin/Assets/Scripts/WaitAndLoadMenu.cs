using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitAndLoadMenu : MonoBehaviour
{

    [SerializeField] private float timeToWait = 1;


    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(timeToWait);
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene(0);
        Time.timeScale = 1.0f;

    }
}
