using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;


public class CheatActivatedFeedback : MonoBehaviour
{
    [SerializeField] private EventReference cheatCodeSound;
    [SerializeField] private TMP_Text cheatText;
    [SerializeField] private GameObject cheatPanel;
    public void Do(string cheatName)
    {
        RuntimeManager.PlayOneShot(cheatCodeSound);
        //gui
        cheatText.text = "<b>" + cheatName + "</b> " + "cheat activated!";
        cheatPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(WaitHide());
    }

    private IEnumerator WaitHide()
    {
        yield return new WaitForSeconds(2);
        cheatPanel.SetActive(false);
    }
}


