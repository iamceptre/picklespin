using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;


public class CheatActivatedFeedback : MonoBehaviour
{
    public static CheatActivatedFeedback instance;

    [SerializeField] private EventReference cheatCodeSound;
    [SerializeField] private TMP_Text cheatText;
    [SerializeField] private GameObject cheatPanel;

    private WaitForSeconds waitBeforeHideTime = new WaitForSeconds(2);

    [SerializeField] private Animator handAnimator;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void Do(string cheatName)
    {
        handAnimator.SetTrigger("Thumbs_Up");
        RuntimeManager.PlayOneShot(cheatCodeSound);
        //gui
        cheatText.text = "<b>" + cheatName + "</b> " + "cheat activated!";
        cheatPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(WaitHide());
    }

    private IEnumerator WaitHide()
    {
        yield return waitBeforeHideTime;
        cheatPanel.SetActive(false);
    }
}


