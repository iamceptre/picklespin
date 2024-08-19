using UnityEngine;
using FMODUnity;

public class Pickupable_Potion : MonoBehaviour
{

    //POTION TYPE
    [Header("Potion Type")]
    [SerializeField] private bool imHp;
    [SerializeField] private bool imStamina;
    [SerializeField] private bool imMana;
    private int myPotionTypeIndex = -1;

    //REFERENCES
    [Header("References")]
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    [SerializeField] StudioEventEmitter pickupSoundEmitter;

    //HP
    [Header("HP")]
    private PlayerHP playerHP;
    [SerializeField] private int howMuchHpIGive;

    //STAMINA
    [Header("Stamina")]
    private PlayerMovement playerMovement;
    [SerializeField] private int howMuchStaminaIGive;

    //MANA
    [Header("Mana")]
    private Ammo ammo;
    [SerializeField] private int howMuchManaIGive;


    //GENERAL
    private BarLightsAnimation barLightsAnimation;
    private ScreenFlashTint screenFlashTint;


    private void Awake()
    {
        if (imHp)
        {
            myPotionTypeIndex = 0; // 0 - HP
            return;
        }

        if (imStamina)
        {
            myPotionTypeIndex = 1; // 1 - Stamina
            return;
        }

        if (imMana)
        {
            myPotionTypeIndex = 2; // 2 - Mana
            return;
        }
    }

    private void Start()
    {
        playerHP = PlayerHP.instance;
        playerMovement = PlayerMovement.instance;
        ammo = Ammo.instance;

        barLightsAnimation = BarLightsAnimation.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }


    public void PickupPotion()
    {

        switch (myPotionTypeIndex)
        {

            case 0:
                GiveHPToPlayer();
                break;

            case 1:
                GiveStaminaToPlayer();
                break;

            case 2:
                GiveManaToPlayer();
                break;

            default:
                break;

        }



        itemAfterPickingUp.Pickup();
        pickupSoundEmitter.Play();
        screenFlashTint.Flash(myPotionTypeIndex);
    }

    private void GiveHPToPlayer()
    {
        if (playerHP.hp < playerHP.maxHp)
        {
            playerHP.GiveHPToPlayer(howMuchHpIGive);
        }
    }

    private void GiveStaminaToPlayer()
    {
        playerMovement.GiveStaminaToPlayer(howMuchStaminaIGive);
    }

    private void GiveManaToPlayer()
    {
        if (ammo.ammo < ammo.maxAmmo)
        {
            ammo.GiveManaToPlayer(howMuchManaIGive);
        }
    }

}

