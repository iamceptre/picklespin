using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

// Drives the round timer and fires the per-round UnityEvents wired in the scene
// (enemy waves, pickup spawns, key spawns). Other systems pause it via isCounting,
// or stop it entirely by disabling the component (e.g. once the win key is taken).
// A spent timer does not advance on its own: it rests at zero until the arena is
// clear - hard mode being the exception that lets waves stack - and the very first
// round also waits for a healed angel and an empty room. Whenever something other
// than the clock is what the round is waiting on, the round label breathes.
public class RoundSystem : MonoBehaviour
{
    public static RoundSystem instance;

    [Header("Timing")]
    [SerializeField] private float roundDuration;
    [Tooltip("timer speed; EnemyCounter raises this while the arena is cleared")]
    public float speedMultiplier = 1;
    public bool isCounting = true;

    [Header("Gating")]
    [SerializeField, Tooltip("hold the next round back while a single enemy is still alive; hard mode ignores this")]
    private bool waitForClearedArena = true;
    [SerializeField, Tooltip("hold the first round back until an angel is healed and the player has left its room")]
    private bool waitForFirstAngel = true;
    [SerializeField, Tooltip("round label pulse speed while the round is held back")]
    private float heldPulseSpeed = 3f;
    [SerializeField, Range(0f, 1f), Tooltip("how far down the round label fades on each pulse")]
    private float heldPulseMinAlpha = 0.25f;

    [Header("Round Events (one per round)")]
    [SerializeField] private UnityEvent[] RoundEvent;
    [SerializeField] private UnityEvent LastRoundEvent;

    [Header("UI")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Slider roundTimerGUI;
    [SerializeField] private TMP_Text newRoundText;

    public int CurrentRound { get; private set; }

    private const float DimmedOpacity = 0.4f;
    private const float HalfPi = Mathf.PI * 0.5f;
    private const float TwoPi = Mathf.PI * 2f;
    // how long both volumes may stay silent before the angel area is assumed left - the
    // exit routes they do not cover are the rocket jump and the grapple
    private const float AngelAreaGrace = 0.5f;

    private CanvasGroup timerCanvasGroup;
    private NewRoundDisplayText newRoundDisplayText;
    private EnemyCounter enemyCounter;
    private AngelSpawner angelSpawner;
    private float timer;
    private float inverseRoundDuration;
    private float pulsePhase;
    private bool isDimmed;
    private bool isHeld;
    private bool hardMode;
    private bool angelHealed;
    private bool reportedInAngelArea;
    private bool reportedInArena;
    private float angelAreaSilence;

    public bool PlayerInAngelArea { get; private set; }

    private bool FirstRoundPending => waitForFirstAngel && CurrentRound == 0;

    // an angel killed before it was healed leaves nothing to heal, and only a round
    // event can summon the next one - so the wait has to give way rather than deadlock
    private bool AnAngelIsWaiting => angelSpawner != null && angelSpawner.AnAngelIsStillWaiting();

    private bool CanAdvance
    {
        get
        {
            if (waitForClearedArena && !hardMode && enemyCounter != null && enemyCounter.EnemyCount > 0) return false;
            if (FirstRoundPending && (PlayerInAngelArea || (!angelHealed && AnAngelIsWaiting))) return false;
            return true;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            enabled = false;
            Destroy(this);
            return;
        }
        instance = this;

        timerCanvasGroup = GetComponent<CanvasGroup>();
        timer = roundDuration;
        inverseRoundDuration = roundDuration > 0f ? 1f / roundDuration : 0f;
        hardMode = HardMode.Enabled;
    }

    private void Start()
    {
        newRoundDisplayText = NewRoundDisplayText.instance;
        enemyCounter = EnemyCounter.instance;
        angelSpawner = AngelSpawner.instance;
        timerCanvasGroup.alpha = DimmedOpacity;
        isDimmed = true;
        RefreshRoundLabel();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        ResolveAngelArea(deltaTime);

        if (isCounting && timer > 0f)
        {
            timer -= deltaTime * speedMultiplier;
            if (timer < 0f) timer = 0f;
            roundTimerGUI.value = timer * inverseRoundDuration;
        }

        SetDimmed(!isCounting);

        bool held = timer <= 0f && !CanAdvance;
        SetHeld(held);

        if (held)
        {
            pulsePhase += deltaTime * heldPulseSpeed;
            if (pulsePhase > TwoPi) pulsePhase -= TwoPi;
            roundText.alpha = heldPulseMinAlpha + (1f - heldPulseMinAlpha) * 0.5f * (1f + Mathf.Sin(pulsePhase));
            return;
        }

        if (isCounting && timer <= 0f) AdvanceRound();
    }

    public void AngelHealed() => angelHealed = true;

    public void ReportPlayerInAngelArea() => reportedInAngelArea = true;

    public void ReportPlayerInArena() => reportedInArena = true;

    private void ResolveAngelArea(float deltaTime)
    {
        bool inside = PlayerInAngelArea;

        if (reportedInAngelArea)
        {
            inside = true;
            angelAreaSilence = 0f;
        }
        else if (reportedInArena)
        {
            inside = false;
        }
        else if (inside)
        {
            // this flag blinds every enemy and voids every spell, so silence fails open
            angelAreaSilence += deltaTime;
            if (angelAreaSilence > AngelAreaGrace) inside = false;
        }

        reportedInAngelArea = false;
        reportedInArena = false;

        SetAngelArea(inside);
    }

    private void SetAngelArea(bool inside)
    {
        if (inside == PlayerInAngelArea) return;
        PlayerInAngelArea = inside;

        if (!inside) isCounting = true;
        else if (!FirstRoundPending) isCounting = false;
    }

    private void SetDimmed(bool dimmed)
    {
        if (dimmed == isDimmed) return;
        isDimmed = dimmed;
        timerCanvasGroup.alpha = dimmed ? DimmedOpacity : 1f;
    }

    private void SetHeld(bool held)
    {
        if (held == isHeld) return;
        isHeld = held;
        pulsePhase = HalfPi;
        if (!held && roundText) roundText.alpha = 1f;
    }

    // disabling this component stops Update, and with it the only thing that clears the
    // angel area - WinGateKeyItem does exactly that when the win key is taken
    private void OnDisable()
    {
        SetHeld(false);
        SetAngelArea(false);
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
