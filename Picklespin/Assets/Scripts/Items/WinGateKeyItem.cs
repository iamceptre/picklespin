using UnityEngine;
using FMODUnity;

public class WinGateKeyItem : MonoBehaviour
{
    private InventoryItemsBank inventory;
    [SerializeField] private ItemAfterPickingUp afterPickingUp;

    [SerializeField] private EventReference keyPickUpSound; 

    private void Start()
    {
        inventory = InventoryItemsBank.instance;
    }

    public void GiveKey()
    {
        inventory.WinGateKey = true;
        RuntimeManager.PlayOneShot(keyPickUpSound);
        afterPickingUp.Pickup();
    }

}
