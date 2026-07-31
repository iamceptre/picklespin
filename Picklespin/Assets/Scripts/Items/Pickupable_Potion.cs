using UnityEngine;
using FMODUnity;

public class Pickupable_Potion : MonoBehaviour
{
    private enum PotionType { HP, Stamina, Mana }

    [Header("Potion Type")]
    [SerializeField] private PotionType potionType;

    // EXP
    [Header("Exp")]
    [SerializeField] private int howMuchExpIGive = 5;

    // REFERENCES
    [Header("References")]
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    [SerializeField] private StudioEventEmitter pickupSoundEmitter;

    // GENERAL STAT GAIN
    [Header("Potion Effect")]
    [SerializeField] private int howMuchIGive;

    private PlayerHP playerHP;
    private PlayerMovement playerMovement;
    private Ammo ammo;
    private PlayerEXP playerExp;
    private ScreenFlashTint screenFlashTint;

    private delegate void ResourceAction();

    private void Start()
    {
        playerHP = PlayerHP.Instance;
        playerMovement = PlayerMovement.Instance;
        ammo = Ammo.instance;
        playerExp = PlayerEXP.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }

    public void PickupPotion()
    {
        // room is asked of the pool the resource actually lives in, never of the field
        // it used to live in: a class can keep its health or its breath in the magicka
        // bar, and the raw hp/stamina fields then sit full forever and eat the potion.
        ResourceAction resourceAction = potionType switch
        {
            PotionType.HP => () => TryGiveResource(playerHP.HealthFraction < 1f, amount => playerHP.ModifyHP(amount)),
            PotionType.Stamina => () => TryGiveResource(!playerMovement.StaminaFull, amount => playerMovement.GiveStaminaToPlayer(amount)),
            PotionType.Mana => () => TryGiveResource(ammo.ammo < ammo.maxAmmo, amount => ammo.GiveManaToPlayer(amount)),
            _ => null
        };

        if (resourceAction != null)
        {
            resourceAction.Invoke();
        }
        else
        {
            Debug.LogError("Invalid potion type");
        }
    }

    private void TryGiveResource(bool hasRoom, System.Action<int> applyEffect)
    {
        if (hasRoom)
        {
            applyEffect(howMuchIGive);

            string color = potionType switch
            {
                PotionType.HP => "<color=#E36464>",
                PotionType.Stamina => "<color=#ADE78A>",
                PotionType.Mana => "<color=#7D93F8>",
                _ => "<color=white>" // default color
            };

            string message = $"{color}Picked up {potionType} potion</color>";
            playerExp.GivePlayerExp(howMuchExpIGive, message);
            Afterpick();
        }
    }

    private void Afterpick()
    {
        itemAfterPickingUp.Pickup();
        pickupSoundEmitter.Play();
        screenFlashTint.Flash((int)potionType);
    }
}