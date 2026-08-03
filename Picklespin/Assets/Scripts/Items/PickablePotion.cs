using UnityEngine;
using FMODUnity;

public class PickablePotion : MonoBehaviour
{
    private enum PotionType { HP, Stamina, Mana, Umbral }

    [Header("Potion Type")]
    [SerializeField] private PotionType potionType;

    // EXP
    [Header("Exp")]
    [SerializeField] private int howMuchExpIGive = 5;

    // REFERENCES
    [Header("References")]
    [SerializeField] private PickableItem pickableItem;
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
            PotionType.Mana or PotionType.Umbral => () => TryGiveResource(ammo.ammo < ammo.maxAmmo, amount => ammo.GiveManaToPlayer(amount)),
            _ => null
        };

        if (resourceAction != null)
        {
            resourceAction.Invoke();
        }
        else
        {
            DevLog.Error("Invalid potion type");
        }
    }

    private void TryGiveResource(bool hasRoom, System.Action<int> applyEffect)
    {
        if (hasRoom)
        {
            applyEffect(howMuchIGive);

            (string color, string pickedUp) = potionType switch
            {
                PotionType.HP => (GameColors.HealthTag, "Picked up HP potion"),
                PotionType.Stamina => (GameColors.StaminaTag, "Picked up Stamina potion"),
                PotionType.Mana => (GameColors.MagickaTag, "Picked up Mana potion"),
                PotionType.Umbral => (GameColors.UmbralTag, "Picked Up Dark Energy"),
                _ => ("white", "Picked up potion")
            };

            playerExp.GivePlayerExp(howMuchExpIGive, $"<color={color}>{pickedUp}</color>");
            Afterpick();
        }
    }

    private void Afterpick()
    {
        pickableItem.Pickup();
        pickupSoundEmitter.Play();
        screenFlashTint.Flash((int)potionType);
    }
}