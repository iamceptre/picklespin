using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class WinGate : MonoBehaviour
{
    private Win win;
    private InventoryItemsBank inventory;

    [SerializeField] private TipDisplay tipDisplay;

    [SerializeField] private UnityEvent showTooltipEvent;
    [SerializeField] private UnityEvent hideTooltipEvent;

    [SerializeField] private StudioEventEmitter portalEnterOneshot;


    private void Start()
    {
        win = Win.instance;
        inventory = InventoryItemsBank.instance;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (inventory.WinGateKey)
            {
                portalEnterOneshot.Play();
                win.WinFunction();
            }
            else
            {
               showTooltipEvent.Invoke();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!inventory.WinGateKey && other.gameObject.CompareTag("Player"))
        {
            hideTooltipEvent.Invoke();
        }
    }

}
