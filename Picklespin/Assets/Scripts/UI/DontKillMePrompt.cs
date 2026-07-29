using System.Collections;
using UnityEngine;

public class DontKillMePrompt : MonoBehaviour
{
    private TipManager tipManager;
    private WaitForSeconds time = new WaitForSeconds(1);

    private void Start()
    {
        tipManager = TipManager.instance;
    }
    public void ShowPrompt()
    {
        StartCoroutine(WaitAndHide());
    }

    private IEnumerator WaitAndHide()
    {
        tipManager.Show(3);
        yield return time;
        tipManager.Hide(3);
    }
}
