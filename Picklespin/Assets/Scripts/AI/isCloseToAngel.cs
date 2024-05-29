using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private GameObject angelHPGUI;
    [SerializeField] private AngelHealingMinigame minigame;
    [SerializeField] private GameObject helperArrow;

    private bool enabledGUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AngelScriptAcivationTrigger")
        {
            minigame.boosted = false;
            angelHeal.enabled = true;
            helperArrow.SetActive(false);

            if (!enabledGUI)
            {
                angelHPGUI.SetActive(true);
                enabledGUI = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "AngelScriptAcivationTrigger")
        {
            if (minigame.gameObject.activeInHierarchy)
            {
                WaitBeforeDisabling();
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


    private void WaitBeforeDisabling()
    {

        if (!minigame.gameObject.activeInHierarchy)
        {
            Invoke("DisableMe", 1);
        }
        else
        {
            DisableMe();
        }


    }


    private void DisableMe()
    {
        angelHeal.CancelHealing();
        angelHeal.StopAiming();
        angelHeal.enabled = false;
    }



}
