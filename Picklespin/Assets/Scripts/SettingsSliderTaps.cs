using UnityEngine;
using UnityEngine.UI;

public class SettingsSliderTaps : MonoBehaviour
{

   [SerializeField] private Slider _slider;

    private float amount = 1;



    public void Increase()
    {
        _slider.value += amount;
    }

    public void Decrease() {
        _slider.value -= amount;
    }

}
