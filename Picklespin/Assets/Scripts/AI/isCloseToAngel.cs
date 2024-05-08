using System.Collections;
using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private GameObject angelHPGUI;
    [SerializeField] private AngelHealingMinigame minigame;

    private bool enabledGUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AngelScriptAcivationTrigger")
        {
            angelHeal.enabled = true;

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
                StartCoroutine(WaitBeforeDisabling());
            }
            else
            {
                angelHeal.enabled = false;
            }
        }
    }

    private void Start()
    {
        angelHeal.enabled = false;
    }


    private IEnumerator WaitBeforeDisabling()
    {
        while (true)
        {
            if (!minigame.gameObject.activeInHierarchy)
            {
                Invoke("DisableMe", 1);
                StopAllCoroutines();    
            }

            yield return null;
        }
    }


    private void DisableMe()
    {
        angelHeal.enabled = false;
    }



}
