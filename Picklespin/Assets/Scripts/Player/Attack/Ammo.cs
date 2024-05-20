using FMODUnity;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public static Ammo instance { get; private set; }
    public int ammo;
    public int maxAmmo;

    private BarLightsAnimation barLightsAnimation;
    private AmmoDisplay ammoDisplay;

    [SerializeField] private EventReference manaAqquiredSound;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


    private void Start()
    {
        barLightsAnimation = BarLightsAnimation.instance;
        ammoDisplay = AmmoDisplay.instance;
    }


    public void GiveManaToPlayer(int howMuchManaIGive)
    {

        if (ammo < maxAmmo) {

            if (ammo + howMuchManaIGive <= maxAmmo)
            {
                ammo += howMuchManaIGive;
                barLightsAnimation.PlaySelectedBarAnimation(2, howMuchManaIGive, false); //hp = 0, stamina = 1, mana = 2
            }
            else
            {
                ammo = maxAmmo;
                barLightsAnimation.PlaySelectedBarAnimation(2, howMuchManaIGive, true); //hp = 0, stamina = 1, mana = 2
            }

            ammoDisplay.Refresh(true);
        }

        //RuntimeManager.PlayOneShot(manaAqquiredSound);
    }

}
