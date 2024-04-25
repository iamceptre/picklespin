using UnityEngine;
using FMODUnity;

public class Pickupable_Mana : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private Ammo ammo;
    private AmmoDisplay ammoDisplay;
    private BarLightsAnimation barLightsAnimation;

    [SerializeField] private int howMuchManaIGive;

    [SerializeField] EventReference pickupSoundEvent;
    //[SerializeField] EventReference pickupSoundEventFull;


    private void Start()
    {
        ammo = Ammo.instance;
        ammoDisplay = AmmoDisplay.instance;
        barLightsAnimation = BarLightsAnimation.instance;
    }

    public void GiveManaToPlayer()
    {
        if (ammo.ammo < ammo.maxAmmo)
        {
            ammo.ammo += howMuchManaIGive;
            ammo.ammo = Mathf.Clamp(ammo.ammo, 0, ammo.maxAmmo);
            RuntimeManager.PlayOneShot(pickupSoundEvent);
            ammoDisplay.RefreshManaValueSmooth();
            barLightsAnimation.PlaySelectedBarAnimation(2, howMuchManaIGive); //hp = 0, stamina = 1, mana = 2
            itemAfterPickingUp.Pickup();
        }
    }

}

