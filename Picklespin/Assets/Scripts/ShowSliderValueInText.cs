using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValueInText : MonoBehaviour
{

    private TMP_Text _text;
    private Slider slider;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        slider = GetComponentInParent<Slider>();
    }

    private void Start()
    {
        RefreshText();
    }

    public void RefreshText()
    {

        _text.text = slider.value.ToString();
    }
}
