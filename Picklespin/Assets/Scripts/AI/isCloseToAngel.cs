using System.Collections;
using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private Canvas angelHPGUI;
    [SerializeField] private AngelHealingMinigame minigame;
    [SerializeField] private GameObject helperArrow;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AngelScriptAcivationTrigger")
        {
            minigame.boosted = false;
            angelHeal.enabled = true;
            helperArrow.SetActive(false);
            angelHPGUI.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "AngelScriptAcivationTrigger")
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
