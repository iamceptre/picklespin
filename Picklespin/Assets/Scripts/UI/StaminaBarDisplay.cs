using UnityEngine;
using UnityEngine.UI;

public class StaminaBarDisplay : MonoBehaviour
{
    private Slider staminaSlider;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private BarEase barEase;

    private void Awake()
    {
       staminaSlider = GetComponent<Slider>();
    }

    public void RefreshBarDisplay()
    {
        if (staminaSlider.value != playerMovement.stamina) {
            staminaSlider.value = playerMovement.stamina;
        }
    }

}
