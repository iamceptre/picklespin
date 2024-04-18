using UnityEngine;
using FMODUnity;

public class Pickupable_Mana : MonoBehaviour
{

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

        if (ammo.ammo != ammo.maxAmmo)
        {
            if ((ammo.ammo += howMuchManaIGive) < ammo.maxAmmo)
            {
                ammo.ammo += howMuchManaIGive;
            }
            else
            {
                ammo.ammo = ammo.maxAmmo;
                //RuntimeManager.PlayOneShot(pickupSoundEventFull);
            }
            RuntimeManager.PlayOneShot(pickupSoundEvent);
            ammoDisplay.RefreshManaValueSmooth();
            barLightsAnimation.PlaySelectedBarAnimation(2); //hp = 0, stamina = 1, mana = 2
        }
        else
        {
            Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }


    }
}
