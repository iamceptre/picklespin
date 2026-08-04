using System.Collections;
using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private Canvas angelHPGUI;


    private string triggerName = "AngelScriptAcivationTrigger";



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == triggerName)
        {
            angelHeal.enabled = true;
            angelHPGUI.enabled = true;
            // the player is in the room now - the spirit has nothing left to lead them to
            if (AngelPointerHelper.Instance) AngelPointerHelper.Instance.Pause();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == triggerName)
        {
            if (angelHeal.IsBoosting)
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
        while (angelHeal.IsBoosting)
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
        if (AngelPointerHelper.Instance) AngelPointerHelper.Instance.Resume();
    }



}
