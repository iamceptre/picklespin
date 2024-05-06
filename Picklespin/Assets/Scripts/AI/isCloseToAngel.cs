using UnityEngine;

public class isCloseToAngel : MonoBehaviour
{

    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private GameObject angelHPGUI;

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
            angelHeal.enabled = false;
        }
    }

    private void Start()
    {
        angelHeal.enabled = false;
    }

}
