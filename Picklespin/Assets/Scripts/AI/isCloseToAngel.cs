using System.Collections;
using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private Canvas angelHPGUI;
    [SerializeField] private AngelHealingMinigame minigame;


    private string triggerName = "AngelScriptAcivationTrigger";



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == triggerName)
        {
            minigame.boosted = false;
            angelHeal.enabled = true;
            angelHPGUI.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == triggerName)
        {
            if (angelHeal.healSpeedMultiplier == 0)
            {
                StartCoroutine(waitUntilMinigameStops());
            }
            else
            {
                DisableMe();
            }
        }
    }

    private void Start()
    {
        DisableMe();
    }



    private IEnumerator waitUntilMinigameStops()
    {
        while (angelHeal.healSpeedMultiplier == 0)
        {
            yield return null;
        }
        DisableMe();
        yield break;
    }


    private void DisableMe()
    {
        angelHPGUI.enabled = false;
        angelHeal.StopAiming();
        angelHeal.enabled = false;
    }



}
