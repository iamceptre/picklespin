using UnityEngine;
using FMODUnity;

public class Pickupable_Stamina : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private PlayerMovement playerMovement;
    [SerializeField] private int howMuchStaminaIGive;
    [SerializeField] private EventReference pickupSoundEvent;
    private StaminaBarDisplay staminaBarDisplay;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
        staminaBarDisplay = StaminaBarDisplay.instance;
    }

    public void GiveStaminaToPlayer()
    {
        playerMovement.GiveStaminaToPlayer(howMuchStaminaIGive);
        RuntimeManager.PlayOneShot(pickupSoundEvent);
        itemAfterPickingUp.Pickup();
    }
}
