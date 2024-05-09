using UnityEngine;
using FMODUnity;

public class Pickupable_Mana : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private Ammo ammo;

    [SerializeField] private int howMuchManaIGive;
    [SerializeField] EventReference pickupSoundEvent;


    private void Start()
    {
        ammo = Ammo.instance;
    }

    public void GiveManaToPlayer()
    {
        if (ammo.ammo < ammo.maxAmmo)
        {
            ammo.GiveManaToPlayer(howMuchManaIGive);
            RuntimeManager.PlayOneShot(pickupSoundEvent);
            itemAfterPickingUp.Pickup();
        }
    }

}

