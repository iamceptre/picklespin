using UnityEngine;
using FMODUnity;

public class Pickupable_HP : MonoBehaviour
{
    [SerializeField] private ItemAfterPickingUp itemAfterPickingUp;
    private PlayerHP playerHP;
    private HpBarDisplay hpBarDisplay;
    
    private BarLightsAnimation barLightsAnimation;
    private ScreenFlashTint screenFlashTint;

    [SerializeField] private int howMuchHPIGive;

    [SerializeField] EventReference pickupSoundEvent;


    private void Start()
    {
        playerHP = PlayerHP.instance;
        hpBarDisplay = HpBarDisplay.Instance;
        barLightsAnimation = BarLightsAnimation.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }

    public void GiveHPToPlayer()
    {
        if (playerHP.hp < playerHP.maxHp)
        {
            playerHP.ModifyHP(howMuchHPIGive);
            itemAfterPickingUp.Pickup();
            RuntimeManager.PlayOneShot(pickupSoundEvent);
            screenFlashTint.Flash(0);
        }
    }

}

