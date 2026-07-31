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
            // the player is in the room now - the spirit has nothing left to lead them to
            if (Helper_Arrow.Instance) Helper_Arrow.Instance.HideSpiritOnly();
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
        angelHeal.StopAiming();
        angelHeal.enabled = false;
        // left the room: if the arrow is still pointing somewhere, the guide comes back
        if (Helper_Arrow.Instance) Helper_Arrow.Instance.RestoreSpirit();
    }



}
