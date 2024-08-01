using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

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

    [HideInInspector] public float speedMultiplier = 1;

    private WaitForSeconds second;
    private float updateRate = 0.1f;

    public bool isCounting = true;

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
        second = new WaitForSeconds(updateRate);
    }
    private void Start()
    {
        newRoundDisplayText = NewRoundDisplayText.instance;
        UpdateText();
        StartCoroutine(Timer());
    }



    private IEnumerator Timer()
    {
        while (true)
        {
            yield return second;

            if (isCounting)
            {

                if (timer > 0)
                {
                    timer -= updateRate * speedMultiplier;
                    roundTimerGUI.value = timer / roundDuration;
                }
                else
                {
                    NextRound();
                }
            }
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
