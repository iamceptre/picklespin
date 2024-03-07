using UnityEngine;
using UnityEngine.UI;

public class StaminaBarDisplay : MonoBehaviour
{

    private Slider staminaSlider;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
       staminaSlider = GetComponent<Slider>();
    }

    public void RefreshBarDisplay()
    {
       staminaSlider.value = playerMovement.stamina * 0.01f;
    }

}
