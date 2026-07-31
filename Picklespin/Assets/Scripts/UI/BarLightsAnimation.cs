using UnityEngine;

public class BarLightsAnimation : MonoBehaviour
{
    public static BarLightsAnimation instance { get; private set; }
    [SerializeField] ManaLightAnimation manaLightAnimation;
    [SerializeField] ManaLightAnimation staminaLightAnimation;
    [SerializeField] ManaLightAnimation hpLightAnimation;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void PlaySelectedBarAnimation(int selectedBar, float howMuchWasGiven, bool maxxed)
    {
        // selectedBar is HudResource: the light hands it on so its +/- number can be
        // painted in the colour of whichever bar that resource reads on now
        if (selectedBar == 0) hpLightAnimation.LightAnimation(howMuchWasGiven, maxxed, HudResource.Health);
        if (selectedBar == 1) staminaLightAnimation.LightAnimation(howMuchWasGiven, maxxed, HudResource.Stamina);
        if (selectedBar == 2) manaLightAnimation.LightAnimation(howMuchWasGiven, maxxed, HudResource.Magicka);
    }
}
