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
    [SerializeField] private PulsatingImage hpBarPulsation;

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;
    private readonly float desaturateAmount = 20;
    private readonly float contrastAmount = 10;
    private readonly float exposureAmount = 1;

    [Header("Audio")]
    [SerializeField] private EventReference tinnitusEventReference;
    //[SerializeField] private EventReference hurtSound;
    //[SerializeField] private EventReference hpAqquiredSound];
    private AudioSnapshotManager audioSnapshotManager;

    [Header("Health Regeneration")]
    [SerializeField, Range(0, 1)] private float regenThresholdPercentage = 0.33f;
    private float hpRegenThreshold;
    [SerializeField] private int regenAmount = 1;
    [SerializeField] private WaitForSeconds regenInterval = new(0.5f);
    private Coroutine regenCoroutine;

    [Header("References")]
    private BarLightsAnimation barLightsAnimation;
    private Death death;
    private HpBarDisplay hpBarDisplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        hpRegenThreshold = maxHp * regenThresholdPercentage;
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
        if (hp < hpRegenThreshold)
        {
            if (!isLowHP)
            {
                isLowHP = true;
                audioSnapshotManager.EnableSnapshot("LowHP");
                _ = StartCoroutine(LowHpEffect());
                hpBarPulsation.StartPulsating();
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
                hpBarPulsation.StopPulsating();
                StopRegenerationCoroutine();
            }
        }
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
        while (isLowHP && hp < maxHp)
        {
            if (hp >= hpRegenThreshold)
            {
                yield break;
            }

            yield return regenInterval;
            hp += regenAmount;
            hpBarDisplay.Refresh(true);
            CheckLowHPState();
        }
    }
}
