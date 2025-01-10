using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP instance { get; private set; }
    public int hp;
    public int maxHp;
    [HideInInspector] public bool isLowHP = false;
    public bool godMode = false;
    public bool invincible = false;
    [Header("Assets")]
    [SerializeField] private EventReference hurtSound;
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;
    [SerializeField] private EventReference hpAqquiredSound;
    [SerializeField] private PostProcessVolume ppVolume;
    [SerializeField] private EventReference tinnitusEventReference;
    [SerializeField] private PulsatingImage hpBarPulsation;
    [Header("HP Regeneration")]
    [SerializeField, Range(0, 1)] private float regenThresholdPercentage = 0.33f;
    private float hpRegenTreshold;
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

        hpRegenTreshold = maxHp * regenThresholdPercentage;
    }

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;
        death = Death.instance;
        barLightsAnimation = BarLightsAnimation.instance;
        hpBarDisplay = HpBarDisplay.instance;
        hurtOverlay.enabled = false;
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }

    public void SetGodmode(bool isGodmode)
    {
        godMode = isGodmode;
    }

    public void TakeDamage(int damage)
    {
        if (godMode || invincible) return;
        hp = Mathf.Max(hp - damage, 0);
        PlayerHurtVisual();
        CheckIfPlayerIsDead();
        CheckLowHPState();
        hpBarDisplay.Refresh(false);
    }

    public void GiveHPToPlayer(int amount)
    {
        if (hp >= maxHp) return;
        hp = Mathf.Min(hp + amount, maxHp);
        hpBarDisplay.Refresh(true);
        bool gotMaxxed = hp == maxHp;
        CheckLowHPState();
        barLightsAnimation.PlaySelectedBarAnimation(0, amount, gotMaxxed);
    }

    private void PlayerHurtVisual()
    {
        hurtOverlay.enabled = true;
        hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
        hurtOverlay.DOKill();
        hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
            hurtOverlay.DOFade(0, 1f).OnComplete(() => hurtOverlay.enabled = false));
    }

    private void CheckIfPlayerIsDead()
    {
        if (hp <= 0) death.PlayerDeath();
    }

    private void CheckLowHPState()
    {
        if (hp < hpRegenTreshold)
        {
            if (!isLowHP)
            {
                isLowHP = true;
                StartLowHpEffects();
                regenCoroutine = StartCoroutine(RegenerateHP());
            }
        }
        else
        {
            if (isLowHP)
            {
                isLowHP = false;
                StopLowHpEffects();
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                    regenCoroutine = null;
                }
            }
        }
    }

    private void StartLowHpEffects()
    {
        RuntimeManager.PlayOneShot(tinnitusEventReference);
        snapshotManager.LowHp.Play();
        StartCoroutine(LowHpEffect());
        hpBarPulsation.StartPulsating();
    }

    private void StopLowHpEffects()
    {
        snapshotManager.LowHp.Stop();
        StartCoroutine(RestoreHpEffect());
        hpBarPulsation.StopPulsating();
    }

    private IEnumerator LowHpEffect()
    {
        float timer = 0f;
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
            if (hp >= hpRegenTreshold)
            {
                yield break;
            }

            yield return regenInterval;
            hp += regenAmount;
            hpBarDisplay.Refresh(true);
        }
    }
}
