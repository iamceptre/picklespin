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
    [Tooltip("PORTAL_LOOP — plays on enable and has no stop trigger, so escaping has to stop it")]
    [SerializeField] private StudioEventEmitter portalLoopEmitter;


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
                if (portalLoopEmitter) portalLoopEmitter.Stop(); // AllowFadeout lets it tail off
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
