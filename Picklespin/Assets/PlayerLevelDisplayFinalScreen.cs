using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelDisplayFinalScreen : MonoBehaviour
{
    private TMP_Text _text;
    private PlayerEXP playerExp;
    private Slider _slider;
    [SerializeField] private ExpGatheredDisplayFinalScreen expGathered;

    private int currentlyShownLevel;
    private float previousSliderValue;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _slider = GetComponentInChildren<Slider>();
    }

    void Start()
    {
        playerExp = PlayerEXP.instance;
        currentlyShownLevel = playerExp.PlayerLevelStarting;

        StringBuilder sb = new StringBuilder();
        sb.Append("level: ");
        sb.Append(currentlyShownLevel);
        _text.text = sb.ToString();

        previousSliderValue = expGathered.currentlyAnimatedExp % 1000;
        _slider.value = previousSliderValue;
    }

    void Update()
    {
        float newSliderValue = expGathered.currentlyAnimatedExp % 1000;

        if (newSliderValue < previousSliderValue)
        {
            NewLevel();
        }

        _slider.value = newSliderValue;
        previousSliderValue = newSliderValue;
    }

    private void NewLevel()
    {
        currentlyShownLevel++;
        StringBuilder sb = new StringBuilder();
        sb.Append("level: ");
        sb.Append(currentlyShownLevel);
        _text.text = sb.ToString();
    }

    public void FinishedAnimating()
    {
        currentlyShownLevel = playerExp.playerLevel;
        StringBuilder sb = new StringBuilder();
        sb.Append("level: ");
        sb.Append(currentlyShownLevel);
        _text.text = sb.ToString();
        _slider.value = playerExp.playerExpAmount % 1000;
    }
}