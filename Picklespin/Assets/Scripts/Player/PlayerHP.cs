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

    [HideInInspector] public bool isLowHP = false;
    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;
    private float desaturateAmount = 20;
    private float contrastAmount = 10;
    private float exposureAmount = 1;

    private SnapshotManager snapshotManager;
    

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
        if (hp <= 0)
        {
            death.PlayerDeath();
        }
    }

    private void CheckIfPlayerIsLowHP()
    {
        if (hp <= maxHp * 0.1f)
        {
            ApplyLowHPState();
        }
        else
        {
            ReturnFromLowHPState();
        }
    }

    private void ApplyLowHPState()
    {
        if (!isLowHP)
        {
            StartCoroutine(LowHpEffect());
            isLowHP = true;
            snapshotManager.PlayLowHPSnapshot();
        }
    }


    private void ReturnFromLowHPState()
    {
        if (isLowHP)
        {
            StartCoroutine(RestoreHpEffect());
            snapshotManager.StopLowHPSnapshot();
            isLowHP = false;
        }
    }


    public void PlayerHurtSound() //initial signal goes here
    {
        if (!godMode) {
            RuntimeManager.PlayOneShot(hurtSound);
            PlayerHurtVisual();
            CheckIfPlayerIsDead();
            CheckIfPlayerIsLowHP();
        }
    }

    private void PlayerHurtVisual()
    {
        hurtOverlay.enabled = true;
        hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
        hurtOverlay.DOKill();
        hurtOverlay.DOFade(0, 0);
        hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
        {
            hurtOverlay.DOFade(0, 1).OnComplete(() =>
            {
                hurtOverlay.enabled = false;
            });
        });
    }

    public void GiveHPToPlayer(int howMuchHPIGive)
    {
        if (hp < maxHp)
        {

            if (hp + howMuchHPIGive < maxHp)
            {
                //Debug.Log("raz");
                hp += howMuchHPIGive;
                hpBarDisplay.Refresh(true);
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, false); //hp = 0, stamina = 1, mana = 2
            }
            else
            {
                //Debug.Log("dwa");
                hp = maxHp;
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, true);
            }

            hpBarDisplay.Refresh(true);
        }

        //RuntimeManager.PlayOneShot(hpAqquiredSound);
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
        yield break;
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
        yield break;
    }

}
