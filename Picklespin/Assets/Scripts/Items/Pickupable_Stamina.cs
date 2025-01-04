using UnityEngine;
using FMODUnity;

public class Pickupable_Stamina : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private PlayerMovement playerMovement;
    [SerializeField] private int howMuchStaminaIGive;
    [SerializeField] private EventReference pickupSoundEvent;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
    }

    public void GiveStaminaToPlayer()
    {
        playerMovement.GiveStaminaToPlayer(howMuchStaminaIGive, false);
        RuntimeManager.PlayOneShot(pickupSoundEvent);
        itemAfterPickingUp.Pickup();
    }
}
