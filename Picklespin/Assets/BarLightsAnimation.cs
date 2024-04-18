using UnityEngine;

public class BarLightsAnimation : MonoBehaviour
{

    public static BarLightsAnimation instance { get; private set; }

    [SerializeField] ManaLightAnimation manaLightAnimation;
    [SerializeField] ManaLightAnimation staminaLightAnimation;
    [SerializeField] ManaLightAnimation hpLightAnimation;

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

    public void PlaySelectedBarAnimation(int selectedBar) //hp = 0, stamina = 1, mana = 2
    {
        if (selectedBar == 0)
        {
            hpLightAnimation.LightAnimation();
        }

        if (selectedBar == 1)
        {
            staminaLightAnimation.LightAnimation();
        }

        if (selectedBar == 2)
        {
            manaLightAnimation.LightAnimation();
        }

    }

}
