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

    public void PlaySelectedBarAnimation(int selectedBar, float howMuchWasGiven) //hp = 0, stamina = 1, mana = 2
    {
        if (selectedBar == 0)
        {
            hpLightAnimation.LightAnimation(howMuchWasGiven);
        }

        if (selectedBar == 1)
        {
            staminaLightAnimation.LightAnimation(howMuchWasGiven);
        }

        if (selectedBar == 2)
        {
            manaLightAnimation.LightAnimation(howMuchWasGiven);
        }

    }

}
