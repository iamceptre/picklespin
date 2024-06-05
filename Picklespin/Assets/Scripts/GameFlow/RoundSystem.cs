using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoundSystem : MonoBehaviour
{
    public static RoundSystem instance;

    [SerializeField] private TMP_Text roundText;
    private int roundNumber;
    private float timer;
    [SerializeField] private float roundDuration;
    [SerializeField] private Slider roundTimerGUI;

    [SerializeField] private UnityEvent[] RoundEvent;
    [SerializeField] private UnityEvent LastRoundEvent;

    [SerializeField] private TMP_Text newRoundText;
    private NewRoundDisplayText newRoundDisplayText;

    public float speedMultiplier = 1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        timer = roundDuration;
    }
    private void Start()
    {
        newRoundDisplayText = NewRoundDisplayText.instance;
        UpdateText();
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime * speedMultiplier;
            roundTimerGUI.value = timer / roundDuration;
        }
        else
        {
            NextRound();
        }

    }

    private void NextRound()
    {
        speedMultiplier = 1;

        if (roundNumber<RoundEvent.Length) {
            newRoundText.text = "Round " + (roundNumber + 1) + " begins";
            RoundEvent[roundNumber].Invoke();
        }
        else
        {
            newRoundText.text = "You reached the end";

            LastRoundEvent.Invoke();
        }
            newRoundDisplayText.Animate();
            roundNumber++;
            UpdateText();
            timer = roundDuration;
    }

    void UpdateText()
    {
        roundText.text = "round " + roundNumber;
    }

}
