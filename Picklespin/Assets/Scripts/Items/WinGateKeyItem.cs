using UnityEngine;
using FMODUnity;

public class WinGateKeyItem : MonoBehaviour
{
    private RoundSystem roundSystem;
    private InventoryItemsBank inventory;

    [SerializeField] private ItemAfterPickingUp afterPickingUp;

    [SerializeField] private EventReference keyPickUpSound;

    [SerializeField] private Canvas canvasToEnable;
    private EscapeTimer escapeTimer;

 

    private void Start()
    {
        inventory = InventoryItemsBank.instance;
        roundSystem = RoundSystem.instance;
        escapeTimer = EscapeTimer.instance;
    }

    public void GiveKey()
    {
        inventory.WinGateKey = true;
        canvasToEnable.enabled = true;
        escapeTimer.enabled = true;
        RuntimeManager.PlayOneShot(keyPickUpSound);
        afterPickingUp.Pickup();
        roundSystem.enabled = false;
    }

}
