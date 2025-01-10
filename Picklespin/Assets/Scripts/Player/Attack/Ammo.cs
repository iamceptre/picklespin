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


    public void GiveManaToPlayer(int howMuchManaIGive, bool isSilent = false)
    {
        if (ammo >= maxAmmo) return;
        ammo = Mathf.Min(ammo + howMuchManaIGive, maxAmmo);
        bool gotMaxxed = ammo == maxAmmo;
        ammoDisplay.Refresh(true);
        if (isSilent) return;
        barLightsAnimation.PlaySelectedBarAnimation(2, howMuchManaIGive, gotMaxxed);
        //RuntimeManager.PlayOneShot(manaAqquiredSound);
    }

}
