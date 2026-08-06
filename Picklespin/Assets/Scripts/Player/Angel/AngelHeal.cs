using System;
using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class AngelHeal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Slider angelHPSlider;
    [SerializeField] private Canvas angelHPCanvas;
    [SerializeField] private HealingParticles healingParticlesScript;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private StudioEventEmitter healingBeamEmitter;
    [SerializeField] private LayerMask layersForRaycast;
    [SerializeField] private CanvasGroup angelHPCanvasGroup;
    [SerializeField] private CanvasGroup minigameCanvasGroup;

    [Header("Parameters")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float guiFadeTimes = 0.1f;
    [SerializeField, Tooltip("angel health per second the beam alone restores")] private float healPerSecond = 15f;

    [Header("Healing Cost")]
    [SerializeField] private float manaDrainPerSecond = 7f;
    [SerializeField] private float hpDrainPerSecond = 10f;
    [SerializeField] private int minimumHpWhileHealing = 10;

    private const float FullAngelHealth = 100f;

    public bool IsBoosting { get; private set; }
    public bool CanHealNow => isAimingAtAngel && CanHeal();
    public event Action CanHealChanged;

    private AngelMind angel;
    private AngelPointerHelper pointerHelper;
    private ScreenFlashTint screenFlashTint;
    private AngelHealingMinigame minigame;
    private TipManager tipManager;
    private CrosshairManager crosshair;
    private Ammo ammo;
    private PlayerHP playerHP;
    private RoundSystem roundSystem;
    private IEnumerator healingRoutine;
    private AiHealth aiHealth;
    private Transform aimedAt;
    private bool isAimingAtAngel;
    private bool isHealing;
    private bool canHealCached;

    private void Start()
    {
        crosshair = CrosshairManager.Instance;
        tipManager = TipManager.instance;
        pointerHelper = AngelPointerHelper.Instance;
        if (tipManager) tipManager.Hide(1);
        minigame = AngelHealingMinigame.Instance;
        screenFlashTint = ScreenFlashTint.instance;
        ammo = Ammo.instance;
        playerHP = PlayerHP.Instance;
        roundSystem = RoundSystem.instance;
    }

    private void Update()
    {
        if (IsBoosting)
        {
            RefreshCanHeal();
            return;
        }

        Transform lookedAt = null;
        if (Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, range, layersForRaycast)
            && hit.collider.CompareTag("Angel"))
        {
            lookedAt = hit.transform;
        }

        if (lookedAt != aimedAt)
        {
            StopAiming();
            if (lookedAt) StartAiming(lookedAt);
        }

        RefreshCanHeal();
    }

    private void RefreshCanHeal()
    {
        bool canHeal = CanHealNow;
        if (canHeal == canHealCached) return;
        canHealCached = canHeal;
        CanHealChanged?.Invoke();
    }

    private bool CanHeal() => angel && aiHealth && !angel.healed && !angel.IsDead;

    private void StartAiming(Transform target)
    {
        aimedAt = target;
        angel = target.GetComponent<AngelMind>();
        aiHealth = angel ? angel.Health : null;

        if (!CanHeal()) return;

        if (tipManager) tipManager.Show(1);
        crosshair.ShowCrosshair();
        isAimingAtAngel = true;
    }

    public void StopAiming()
    {
        if (IsBoosting) return;

        aimedAt = null;
        if (!isAimingAtAngel) return;

        isAimingAtAngel = false;
        if (tipManager) tipManager.Hide(1);
        crosshair.HideCrosshair();
        CancelHealing();
        RefreshCanHeal();
    }

    public void StartHealing()
    {
        if (isHealing) return;
        healingBeamEmitter.Play();
        StopDrainingPlayer();
        healingRoutine = Healing();
        StartCoroutine(healingRoutine);
    }

    public void CancelHealing()
    {
        if (!isHealing) return;
        isHealing = false;
        IsBoosting = false;
        if (healingRoutine != null)
        {
            StopCoroutine(healingRoutine);
            healingRoutine = null;
        }
        handAnimator.SetTrigger("Healing_Beam_Stop");
        healingParticlesScript.StopEmitting();
        healingBeamEmitter.Stop();
        minigame.Stop();
        StopDrainingPlayer();
        FadeOutGui();
    }

    public void BeginBoost()
    {
        IsBoosting = true;
        StopDrainingPlayer();
    }

    private IEnumerator Healing()
    {
        isHealing = true;
        handAnimator.SetTrigger("Healing_Beam");
        if (pointerHelper) pointerHelper.Pause();
        if (tipManager) tipManager.Hide(1);
        healingParticlesScript.StartEmitting(angel.transform);
        FadeInGui();
        minigame.Begin(angel);

        while (IsBoosting || aiHealth.hp < FullAngelHealth)
        {
            if (!CanHeal())
            {
                CancelHealing();
                yield break;
            }

            if (!IsBoosting)
            {
                aiHealth.hp += Time.deltaTime * healPerSecond;
                DrainPlayer(Time.deltaTime);
            }

            angelHPSlider.value = aiHealth.hp;
            yield return null;
        }

        Healed();
    }

    // mana first, then HP once the player is dry - unless mana *is* the health pool,
    // where there is no second pool and the drain stops at the same floor
    private void DrainPlayer(float deltaTime)
    {
        float cost = deltaTime * PlayerClasses.AngelHealCostMultiplier;

        if (PlayerClasses.MagickaIsHealth)
        {
            if (ammo.ammo > minimumHpWhileHealing) ammo.DrainMana(cost * manaDrainPerSecond);
            return;
        }

        if (ammo.ammo > 0)
        {
            ammo.DrainMana(cost * manaDrainPerSecond);
            return;
        }

        playerHP.DrainHP(cost * hpDrainPerSecond, minimumHpWhileHealing);
    }

    private void StopDrainingPlayer()
    {
        ammo.StopDraining();
        playerHP.StopDraining();
    }

    public void Healed()
    {
        if (!angel || !aiHealth || angel.healed) return;

        angel.healed = true;
        IsBoosting = false;
        aiHealth.hp = FullAngelHealth;
        angelHPSlider.value = FullAngelHealth;

        CancelHealing();
        StopAiming();

        angel.AfterHealedAction();
        screenFlashTint.Flash(5, 4);

        ClassUpgrades.CountAngelHealed();
        if (roundSystem) roundSystem.AngelHealed();
        AskTheAngel();
    }

    // the class and upgrade menus open the wish menu themselves once answered
    private void AskTheAngel()
    {
        if (PlayerClassMenu.Instance && PlayerClassMenu.Instance.CanOffer)
        {
            PlayerClassMenu.Instance.AskForClass();
            if (PlayerClassMenu.Instance.IsAsking) return;
        }
        else if (ClassUpgradeMenu.Instance && ClassUpgradeMenu.Instance.CanOffer
                 && ClassUpgradeMenu.Instance.AskForUpgrade())
        {
            return;
        }

        if (AngelWishMenu.Instance) AngelWishMenu.Instance.AskForWish();
    }

    private void FadeOutGui()
    {
        minigameCanvasGroup.DOKill();
        minigameCanvasGroup.DOFade(0, guiFadeTimes);
        angelHPCanvasGroup.DOKill();
        angelHPCanvasGroup.DOFade(0, guiFadeTimes * 1.5f).OnComplete(() => { angelHPCanvas.enabled = false; });
    }

    private void FadeInGui()
    {
        minigameCanvasGroup.DOKill();
        minigameCanvasGroup.DOFade(1, guiFadeTimes);
        angelHPCanvasGroup.DOKill();
        angelHPCanvasGroup.alpha = 0;
        angelHPCanvas.enabled = true;
        angelHPCanvasGroup.DOFade(1, guiFadeTimes);
    }
}
