using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoundSystem : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    private int roundNumber;
    private float timer;
    [SerializeField] private float roundDuration;
    [SerializeField] private Slider roundTimerGUI;

    [SerializeField] private UnityEvent[] RoundEvent;
    [SerializeField] private UnityEvent LastRoundEvent;

    private void Awake()
    {
        timer = roundDuration;
    }
    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        if (timer < 0)
        {
            NextRound();
        }
        else
        {
            timer -= Time.deltaTime;
            roundTimerGUI.value = timer / roundDuration;
        }

    }

    private void NextRound()
    {
        if (roundNumber<RoundEvent.Length) {
            RoundEvent[roundNumber].Invoke();
        }
        else
        {
            LastRoundEvent.Invoke(); //if you reach the last round, it will repeat forever
        }

            roundNumber++;
            UpdateText();
            timer = roundDuration;
    }

    void UpdateText()
    {
        roundText.text = "round " + roundNumber;
    }

    public void FinalRound()
    {
        roundText.text = "finality";
    }
}
