using UnityEngine;
using FMODUnity;

public class WinGateKeyItem : MonoBehaviour
{
    private RoundSystem roundSystem;
    private InventoryItemsBank inventory;

    [SerializeField] private ItemAfterPickingUp afterPickingUp;

    [SerializeField] private EventReference keyPickUpSound;

    public GameObject toEnable;

 

    private void Start()
    {
        inventory = InventoryItemsBank.instance;
        roundSystem = RoundSystem.instance;
    }

    public void GiveKey()
    {
        inventory.WinGateKey = true;
        toEnable.SetActive(true);
        RuntimeManager.PlayOneShot(keyPickUpSound);
        afterPickingUp.Pickup();
        roundSystem.enabled = false;
    }

}
