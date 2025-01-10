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
        playerHP = PlayerHP.instance;
        playerMovement = PlayerMovement.instance;
        ammo = Ammo.instance;
        playerExp = PlayerEXP.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }

    public void PickupPotion()
    {
        ResourceAction resourceAction = potionType switch
        {
            PotionType.HP => () => TryGiveResource(playerHP.hp, playerHP.maxHp, amount => playerHP.GiveHPToPlayer(amount)),
            PotionType.Stamina => () => TryGiveResource((int)playerMovement.stamina, 100, amount => playerMovement.GiveStaminaToPlayer(amount)),
            PotionType.Mana => () => TryGiveResource(ammo.ammo, ammo.maxAmmo, amount => ammo.GiveManaToPlayer(amount)),
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

    private void TryGiveResource(int current, int max, System.Action<int> applyEffect)
    {
        if (current < max)
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