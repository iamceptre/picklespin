using UnityEngine;
using FMODUnity;

public class Pickupable_Stamina : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private BarLightsAnimation barLightsAnimation;
    [SerializeField] private int howMuchStaminaIGive;

    [SerializeField] private EventReference pickupSoundEvent;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
        barLightsAnimation = BarLightsAnimation.instance;
    }

    public void GiveStaminaToPlayer()
    {
        if (playerMovement.stamina < 90) {

            if ((playerMovement.stamina += howMuchStaminaIGive) < 100) //100 is a constant value for now in the playerMovement script
            {
                playerMovement.stamina += howMuchStaminaIGive; // Maybe do a tween later

            }
            else
            {
                playerMovement.stamina = 100;
            }
            barLightsAnimation.PlaySelectedBarAnimation(1); //hp = 0, stamina = 1, mana = 2
            RuntimeManager.PlayOneShot(pickupSoundEvent);
        }
        else
        {
            Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);//kill tweens
        }
    }
}
