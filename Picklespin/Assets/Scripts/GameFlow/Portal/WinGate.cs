using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class WinGate : MonoBehaviour
{
    private Win win;
    private InventoryItemsBank inventory;

    [SerializeField] private UnityEvent showTooltipEvent;
    [SerializeField] private UnityEvent hideTooltipEvent;

    [SerializeField] private StudioEventEmitter portalEnterOneshot;

    [SerializeField] private EventReference snapshotEvent;
    private EventInstance afterPortalEnterSnapshot;

    private PlayerHP playerHP;


    private void Start()
    {
        win = Win.instance;
        inventory = InventoryItemsBank.instance;
        afterPortalEnterSnapshot = RuntimeManager.CreateInstance(snapshotEvent);
        playerHP = PlayerHP.Instance;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (inventory.WinGateKey)
            {
                playerHP.godMode = true;
                StateManager.AllLosePlayer();
                portalEnterOneshot.Play();
                afterPortalEnterSnapshot.start();
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
        afterPortalEnterSnapshot.release();
    }

}
