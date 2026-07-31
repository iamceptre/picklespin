using DG.Tweening;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance { get; private set; }

    [Header("Player Health")]
    public int hp;
    public int maxHp;
    public bool isLowHP;
    public bool godMode;
    public bool invincible;

    [Header("UI Elements")]
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;
    private readonly float desaturateAmount = 20;
    private readonly float contrastAmount = 10;
    private readonly float exposureAmount = 1;

    [Header("Audio")]
    [SerializeField] private EventReference tinnitusEventReference;
    private AudioSnapshotManager audioSnapshotManager;

    [Header("Health Regeneration")]
    [SerializeField, Range(0, 1)] private float regenThresholdPercentage = 0.33f;
    [SerializeField] private int regenAmount = 1;
    [SerializeField] private WaitForSeconds regenInterval = new(0.5f);
    private Coroutine regenCoroutine;
    private float drainRemainder;

    [Header("References")]
    private BarLightsAnimation barLightsAnimation;
    private Death death;
    private HpBarDisplay hpBarDisplay;


    // every check below reads whichever pool the class runs on, and maxHp is not
    // constant either - so the low-health threshold is a fraction, never an absolute
    private bool MagickaIsHealth => PlayerClasses.MagickaIsHealth && Ammo.instance;

    // one number for the tinnitus, the desaturation, the regen and the bar's pulse
    public float LowHealthThreshold => regenThresholdPercentage;

    // what the health bar draws - its own pool, with a continuous drain's pending
    // fraction included. HealthFraction below is the class-routed one every rule reads.
    public float DisplayFraction => maxHp > 0 ? Mathf.Clamp01((hp - drainRemainder) / maxHp) : 0f;

    public float HealthFraction => MagickaIsHealth
        ? (Ammo.instance.maxAmmo > 0 ? (float)Ammo.instance.ammo / Ammo.instance.maxAmmo : 0f)
        : (maxHp > 0 ? (float)hp / maxHp : 0f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        audioSnapshotManager = AudioSnapshotManager.Instance;
        death = Death.instance;
        barLightsAnimation = BarLightsAnimation.instance;
        hpBarDisplay = HpBarDisplay.Instance;

        if (hurtOverlay != null)
        {
            hurtOverlay.enabled = false;
        }

        _ = ppVolume.profile.TryGetSettings(out ppColorGrading);
    }

    public void ModifyHP(int amount)
    {
        if (godMode || invincible)
        {
            return;
        }

        if (MagickaIsHealth)
        {
            // the magicka bar owns the feedback too, and GiveManaToPlayer pushes the
            // low-health check back through MagickaChanged
            Ammo.instance.GiveManaToPlayer(amount);
            if (amount < 0)
            {
                HandleDamageEffects();
                if (Ammo.instance.ammo <= 0) death.PlayerDeath();
            }
            return;
        }

        hp = Mathf.Clamp(hp + amount, 0, maxHp);

        if (amount < 0)
        {
            HandleDamageEffects();
            if (hp <= 0)
            {
                death.PlayerDeath();
            }
        }

        hpBarDisplay.Refresh(true);
        barLightsAnimation.PlaySelectedBarAnimation(0, amount, hp == maxHp);
        CheckLowHPState();
    }

    // safe to call every frame: no hurt overlay, floored at minimumHp, and the
    // fractional part is carried between frames so the bar falls at a constant rate
    public void DrainHP(float amount, int minimumHp)
    {
        if (godMode || invincible || amount <= 0)
        {
            return;
        }

        if (hp <= minimumHp)
        {
            if (drainRemainder > 0)
            {
                drainRemainder = 0;
                hpBarDisplay.SetContinuousValue(hp, maxHp);
            }
            return;
        }

        drainRemainder += amount;
        int wholePoints = Mathf.FloorToInt(drainRemainder);

        if (wholePoints > 0)
        {
            drainRemainder -= wholePoints;
            hp = Mathf.Max(minimumHp, hp - wholePoints);
            CheckLowHPState();
        }

        hpBarDisplay.SetContinuousValue(hp - drainRemainder, maxHp);
    }

    public void StopDraining()
    {
        drainRemainder = 0;
        hpBarDisplay.Refresh(false);
    }

    private void HandleDamageEffects()
    {
        if (hurtOverlay != null && hurtOverlays != null && hurtOverlays.Length > 0)
        {
            hurtOverlay.enabled = true;
            hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
            _ = hurtOverlay.DOKill();
            _ = hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
                hurtOverlay.DOFade(0, 1f).OnComplete(() => hurtOverlay.enabled = false));
        }
    }

    private void CheckLowHPState()
    {
        if (HealthFraction < regenThresholdPercentage)
        {
            if (!isLowHP)
            {
                isLowHP = true;
                audioSnapshotManager.EnableSnapshot("LowHP");
                _ = StartCoroutine(LowHpEffect());
                regenCoroutine = StartCoroutine(RegenerateHP());
            }
        }
        else
        {
            if (isLowHP)
            {
                isLowHP = false;
                audioSnapshotManager.DisableSnapshot("LowHP");
                _ = StartCoroutine(RestoreHpEffect());
                StopRegenerationCoroutine();
            }
        }
    }

    // pushed by Ammo: when magicka is the health pool, casting and dashing change it
    // without ever touching PlayerHP
    public void RefreshLowHealthState() => CheckLowHPState();

    public void RestoreToFull()
    {
        ModifyHP(MagickaIsHealth ? Ammo.instance.maxAmmo : maxHp);
    }

    // extra headroom is granted filled, so the bar grows instead of reading as empty
    public void MultiplyMaxHp(float factor)
    {
        int newMax = Mathf.Max(1, Mathf.RoundToInt(maxHp * factor));
        int gained = newMax - maxHp;
        maxHp = newMax;
        hp = Mathf.Clamp(gained > 0 ? hp + gained : hp, 0, maxHp);

        if (hpBarDisplay) hpBarDisplay.Refresh(true);
        CheckLowHPState();
    }

    private void StopRegenerationCoroutine()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    private IEnumerator LowHpEffect()
    {
        float timer = 0f;
        RuntimeManager.PlayOneShot(tinnitusEventReference);
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            ppColorGrading.saturation.value -= Time.deltaTime * desaturateAmount;
            ppColorGrading.contrast.value += Time.deltaTime * contrastAmount;
            ppColorGrading.postExposure.value += Time.deltaTime * exposureAmount;
            yield return null;
        }
    }

    private IEnumerator RestoreHpEffect()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            ppColorGrading.saturation.value += Time.deltaTime * desaturateAmount;
            ppColorGrading.contrast.value -= Time.deltaTime * contrastAmount;
            ppColorGrading.postExposure.value -= Time.deltaTime * exposureAmount;
            yield return null;
        }
    }

    private IEnumerator RegenerateHP()
    {
        while (isLowHP && HealthFraction < 1f)
        {
            if (HealthFraction >= regenThresholdPercentage)
            {
                yield break;
            }

            yield return regenInterval;

            if (MagickaIsHealth)
            {
                Ammo.instance.GiveManaToPlayer(regenAmount, true); // silent: a trickle must not flash the bar every half second
            }
            else
            {
                hp = Mathf.Min(hp + regenAmount, maxHp);
                hpBarDisplay.Refresh(true);
            }
            CheckLowHPState();
        }
    }
}
