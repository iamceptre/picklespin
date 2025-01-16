using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP instance { get; private set; }
    public int hp;
    public int maxHp;
    public bool isLowHP;
    public bool godMode;
    public bool invincible;
    //[SerializeField] private EventReference hurtSound;
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;
    //[SerializeField] private EventReference hpAqquiredSound;
    [SerializeField] private PostProcessVolume ppVolume;
    [SerializeField] private EventReference tinnitusEventReference;
    [SerializeField] private PulsatingImage hpBarPulsation;
    [SerializeField, Range(0, 1)] private float regenThresholdPercentage = 0.33f;
    private float hpRegenThreshold;
    [SerializeField] private int regenAmount = 1;
    [SerializeField] private WaitForSeconds regenInterval = new(0.5f);
    private BarLightsAnimation barLightsAnimation;
    private Death death;
    private HpBarDisplay hpBarDisplay;
    private ColorGrading ppColorGrading;
    private readonly float desaturateAmount = 20;
    private readonly float contrastAmount = 10;
    private readonly float exposureAmount = 1;
    private SnapshotManager snapshotManager;
    private Coroutine regenCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        hpRegenThreshold = maxHp * regenThresholdPercentage;
    }

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;
        death = Death.instance;
        barLightsAnimation = BarLightsAnimation.instance;
        hpBarDisplay = HpBarDisplay.Instance;
        if (hurtOverlay != null) hurtOverlay.enabled = false;
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }

    public void ModifyHP(int amount)
    {
        if (godMode || invincible) return;
        hp = Mathf.Clamp(hp + amount, 0, maxHp);
        if (amount < 0)
        {
            if (hurtOverlay != null && hurtOverlays != null && hurtOverlays.Length > 0)
            {
                hurtOverlay.enabled = true;
                hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
                hurtOverlay.DOKill();
                hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
                    hurtOverlay.DOFade(0, 1f).OnComplete(() => hurtOverlay.enabled = false));
            }
            if (hp <= 0) death.PlayerDeath();
        }
        hpBarDisplay.Refresh(true);
        bool gotMaxxed = hp == maxHp;
        barLightsAnimation.PlaySelectedBarAnimation(0, amount, gotMaxxed);
        CheckLowHPState();
    }

    private void CheckLowHPState()
    {
        if (hp < hpRegenThreshold)
        {
            if (!isLowHP)
            {
                isLowHP = true;
                snapshotManager.LowHp.Play();
                StartCoroutine(LowHpEffect());
                hpBarPulsation.StartPulsating();
                regenCoroutine = StartCoroutine(RegenerateHP());
            }
        }
        else
        {
            if (isLowHP)
            {
                isLowHP = false;
                snapshotManager.LowHp.Stop();
                StartCoroutine(RestoreHpEffect());
                hpBarPulsation.StopPulsating();
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                    regenCoroutine = null;
                }
            }
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
            if (hp >= hpRegenThreshold) yield break;
            yield return regenInterval;
            hp += regenAmount;
            hpBarDisplay.Refresh(true);
            CheckLowHPState();
        }
    }
}
