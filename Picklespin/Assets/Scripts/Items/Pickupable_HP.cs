using UnityEngine;
using FMODUnity;

public class Pickupable_HP : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private PlayerHP playerHP;
    private HpBarDisplay hpBarDisplay;
    
    private BarLightsAnimation barLightsAnimation;

    [SerializeField] private int howMuchHPIGive;

    [SerializeField] EventReference pickupSoundEvent;
    //[SerializeField] EventReference pickupSoundEventFull;


    private void Start()
    {
        playerHP = PlayerHP.instance;
        hpBarDisplay = HpBarDisplay.instance;
        barLightsAnimation = BarLightsAnimation.instance;
    }

    public void GiveHPToPlayer()
    {
        if (playerHP.hp < playerHP.maxHp)
        {
            playerHP.hp += howMuchHPIGive;
            playerHP.hp = Mathf.Clamp(playerHP.hp, 0, playerHP.maxHp);
            RuntimeManager.PlayOneShot(pickupSoundEvent);
            hpBarDisplay.RefreshHPBarSmooth();
            barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive); //hp = 0, stamina = 1, mana = 2
            itemAfterPickingUp.Pickup();
        }
    }

}

