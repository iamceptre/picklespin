using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    private BarLightsAnimation barLightsAnimation;
    public static PlayerHP instance { get; private set; }
    public int hp;
    public int maxHp;
    private Death death;
    private HpBarDisplay hpBarDisplay;
    [Header("Assets")]
    [SerializeField] private EventReference hurtSound;
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;
    [SerializeField] private EventReference hpAqquiredSound;
    public bool godMode = false;
    public bool invincible = false;
    [HideInInspector] public bool isLowHP = false;
    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;
    private float desaturateAmount = 20;
    private float contrastAmount = 10;
    private float exposureAmount = 1;
    private SnapshotManager snapshotManager;
    [SerializeField] private EventReference tinnitusEventReference;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void SetGodmode(bool isGodmode)
    {
        godMode = isGodmode;
    }

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;
        death = Death.instance;
        hurtOverlay.enabled = false;
        barLightsAnimation = BarLightsAnimation.instance;
        hpBarDisplay = HpBarDisplay.instance;
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }

    private void CheckIfPlayerIsDead()
    {
        if (hp <= 0) death.PlayerDeath();
    }

    private void CheckIfPlayerIsLowHP()
    {
        if (hp <= maxHp * 0.15f) ApplyLowHPState();
        else ReturnFromLowHPState();
    }

    private void ApplyLowHPState()
    {
        if (!isLowHP)
        {
            RuntimeManager.PlayOneShot(tinnitusEventReference);
            StartCoroutine(LowHpEffect());
            isLowHP = true;
            snapshotManager.LowHp.Play();
        }
    }

    private void ReturnFromLowHPState()
    {
        if (isLowHP)
        {
            StartCoroutine(RestoreHpEffect());
            snapshotManager.LowHp.Stop();
            isLowHP = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (godMode || invincible) return;
        hp -= damage;
        PlayerHurtVisual();
        CheckIfPlayerIsDead();
        CheckIfPlayerIsLowHP();
        hpBarDisplay.Refresh(false);
    }

    private void PlayerHurtVisual()
    {
        hurtOverlay.enabled = true;
        hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
        hurtOverlay.DOKill();
        hurtOverlay.DOFade(0, 0);
        hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
        {
            hurtOverlay.DOFade(0, 1).OnComplete(() => { hurtOverlay.enabled = false; });
        });
    }

    public void GiveHPToPlayer(int howMuchHPIGive)
    {
        if (hp < maxHp)
        {
            if (hp + howMuchHPIGive < maxHp)
            {
                hp += howMuchHPIGive;
                hpBarDisplay.Refresh(true);
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, false);
            }
            else
            {
                hp = maxHp;
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, true);
            }
            hpBarDisplay.Refresh(true);
        }
        CheckIfPlayerIsLowHP();
    }

    private IEnumerator LowHpEffect()
    {
        float timer = 0;
        while (timer < 1)
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
        float timer = 0;
        while (timer < 1)
        {
            timer += Time.deltaTime;
            ppColorGrading.saturation.value += Time.deltaTime * desaturateAmount;
            ppColorGrading.contrast.value -= Time.deltaTime * contrastAmount;
            ppColorGrading.postExposure.value -= Time.deltaTime * exposureAmount;
            yield return null;
        }
    }
}
