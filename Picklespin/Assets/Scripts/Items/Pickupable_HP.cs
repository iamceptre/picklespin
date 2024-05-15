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
            playerHP.GiveHPToPlayer(howMuchHPIGive);
            itemAfterPickingUp.Pickup();
            //RuntimeManager.PlayOneShot(pickupSoundEvent);
        }
    }

}

