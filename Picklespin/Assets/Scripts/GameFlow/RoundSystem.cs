using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

// Drives the round timer and fires the per-round UnityEvents wired in the scene
// (enemy waves, pickup spawns, key spawns). Other systems pause it via isCounting,
// or stop it entirely by disabling the component (e.g. once the win key is taken).
public class RoundSystem : MonoBehaviour
{
    public static RoundSystem instance;

    [Header("Timing")]
    [SerializeField] private float roundDuration;
    [Tooltip("timer speed; EnemyCounter raises this while the arena is cleared")]
    public float speedMultiplier = 1;
    public bool isCounting = true;

    [Header("Round Events (one per round)")]
    [SerializeField] private UnityEvent[] RoundEvent;
    [SerializeField] private UnityEvent LastRoundEvent;

    [Header("UI")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Slider roundTimerGUI;
    [SerializeField] private TMP_Text newRoundText;

    public int CurrentRound { get; private set; }

    private const float DimmedOpacity = PhiMath.INV_PHI2;

    private CanvasGroup timerCanvasGroup;
    private NewRoundDisplayText newRoundDisplayText;
    private float timer;
    private bool wasCounting;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        timerCanvasGroup = GetComponent<CanvasGroup>();
        timer = roundDuration;
    }

    private void Start()
    {
        newRoundDisplayText = NewRoundDisplayText.instance;
        timerCanvasGroup.alpha = DimmedOpacity;
        wasCounting = false;
        RefreshRoundLabel();
    }

    private void Update()
    {
        if (isCounting != wasCounting)
        {
            wasCounting = isCounting;
            timerCanvasGroup.alpha = isCounting ? 1f : DimmedOpacity;
        }
        if (!isCounting) return;

        timer -= Time.deltaTime * speedMultiplier;
        roundTimerGUI.value = timer / roundDuration;
        if (timer <= 0f) AdvanceRound();
    }

    private void AdvanceRound()
    {
        speedMultiplier = 1;

        if (CurrentRound < RoundEvent.Length)
        {
            newRoundText.text = $"Round {CurrentRound + 1} begins";
            RoundEvent[CurrentRound].Invoke();
        }
        else
        {
            newRoundText.text = "You reached the end";
            LastRoundEvent.Invoke();
        }

        newRoundDisplayText.Animate();
        CurrentRound++;
        RefreshRoundLabel();
        timer = roundDuration;
    }

    private void RefreshRoundLabel()
    {
        roundText.text = "round " + CurrentRound;
    }
}
