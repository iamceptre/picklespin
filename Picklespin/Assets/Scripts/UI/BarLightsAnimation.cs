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
        if (selectedBar == 0) hpLightAnimation.LightAnimation(howMuchWasGiven, maxxed);
        if (selectedBar == 1) staminaLightAnimation.LightAnimation(howMuchWasGiven, maxxed);
        if (selectedBar == 2) manaLightAnimation.LightAnimation(howMuchWasGiven, maxxed);
    }
}
