using FMOD.Studio;
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
   
    //[SerializeField] private StudioEventEmitter portalLoopEmitter;

    [SerializeField] private EventReference snapshotEvent;
    private EventInstance snapshotInstance;


    private void Start()
    {
        win = Win.instance;
        inventory = InventoryItemsBank.instance;
        snapshotInstance = RuntimeManager.CreateInstance(snapshotEvent);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (inventory.WinGateKey)
            {
                portalEnterOneshot.Play();
                //if (portalLoopEmitter) portalLoopEmitter.Stop();
                snapshotInstance.start();
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
    void OnDestroy()
    {
        snapshotInstance.release();
    }

}
