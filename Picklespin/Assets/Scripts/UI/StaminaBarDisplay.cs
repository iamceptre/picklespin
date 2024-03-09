using UnityEngine;
using UnityEngine.UI;

public class StaminaBarDisplay : MonoBehaviour
{

    private Slider staminaSlider;
    [SerializeField] private PlayerMovement playerMovement;
    private float velocity;
    private float desiredStaminaDisplay;

    private void Awake()
    {
       staminaSlider = GetComponent<Slider>();
    }

    public void RefreshBarDisplay()
    {
       desiredStaminaDisplay = playerMovement.stamina * 0.01f;
    }

    private void Update()
    {
        if (staminaSlider.value != desiredStaminaDisplay) {
            staminaSlider.value = Mathf.SmoothDamp(staminaSlider.value, desiredStaminaDisplay, ref velocity, 0.1f);
        }
    }

}
