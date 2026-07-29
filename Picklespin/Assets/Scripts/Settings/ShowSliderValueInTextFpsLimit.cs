using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValueInTextFpsLimit : MonoBehaviour
{

    private FPSLimit fpsLimit;
    private StringBuilder sb = new StringBuilder();
    private TMP_Text _text;
    private Slider slider;

    private string fps = " fps";
    private string unlimited = "unlimited";
    private string monitorRefreshRate = "auto (";


    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        slider = GetComponentInParent<Slider>();
    }

    private void Start()
    {
        fpsLimit = FPSLimit.instance;
        RefreshText();
    }

    public void RefreshText()
    {

        if (slider.value == slider.maxValue)
        {
            sb.Clear();
            sb.Append(unlimited);
            _text.text = sb.ToString();
            return;
        }

        if (slider.value <= 29)
        {
            sb.Clear();
            sb.Append(monitorRefreshRate + (fpsLimit.monitorRefreshRate+1) + ")");
            _text.text = sb.ToString();
            return;
        }

        sb.Clear();
        sb.Append(slider.value.ToString());
        sb.Append(fps);
        _text.text = sb.ToString();
    }
}


