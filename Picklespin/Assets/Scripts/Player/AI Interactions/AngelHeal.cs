using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class AngelHeal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandShakeWhenCannotHeal handShake;
    [SerializeField] private GameObject hand;
    [SerializeField] private Material handHighlightMaterial;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Slider angelHPSlider;
    [SerializeField] private Canvas angelHPCanvas;
    [SerializeField] private HealingParticles healingParticlesScript;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private StudioEventEmitter healingBeamEmitter;
    [SerializeField] private ManaLightAnimation manaLightAnimation;
    [SerializeField] private LayerMask layersForRaycast;
    [SerializeField] private CanvasGroup angelHPCanvasGroup;
    [SerializeField] private CanvasGroup minigameCanvasGroup;

    [Header("Parameters")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float guiFadeTimes = 0.1f;
    public float healSpeedMultiplier = 1;

    [Header("Input")]
    [SerializeField] private InputActionReference healAction;

    private Helper_Arrow helperArrow;
    private ScreenFlashTint screenFlashTint;
    private AngelHealingMinigame minigame;
    private Material handOGMaterial;
    private SkinnedMeshRenderer handRenderer;
    private TipManager tipManager;
    private CrosshairManager crosshair;
    private IEnumerator healingRoutine;
    private AiHealth aiHealth;
    public AngelMind angel;
    private bool isAimingAtAngel;
    private bool isHealing;

    private void Awake()
    {
        handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
        handOGMaterial = handRenderer.material;
    }

    private void Start()
    {
        crosshair = CrosshairManager.Instance;
        tipManager = TipManager.instance;
        helperArrow = Helper_Arrow.Instance;
        tipManager.Hide(1);
        minigame = AngelHealingMinigame.Instance;
        screenFlashTint = ScreenFlashTint.instance;
        minigame.enabled = false;
    }

    private void OnEnable()
    {
        healAction.action.performed += OnHealPerformed;
        healAction.action.canceled += OnHealCanceled;
        healAction.action.Enable();
    }

    private void OnDisable()
    {
        healAction.action.performed -= OnHealPerformed;
        healAction.action.canceled -= OnHealCanceled;
        healAction.action.Disable();
    }

    private void Update()
    {
        var ray = new Ray(mainCamera.position, mainCamera.forward * range);
        if (Physics.Raycast(ray, out RaycastHit hit, range, layersForRaycast))
        {
            if (hit.collider.CompareTag("Angel"))
            {
                if (!isAimingAtAngel)
                {
                    angel = hit.transform.GetComponent<AngelMind>();
                    aiHealth = hit.transform.GetComponent<AiHealth>();
                    minigame.AngelChanged();
                    StartAiming();
                }
            }
        }
        else StopAiming();
    }

    private void OnHealPerformed(InputAction.CallbackContext ctx)
    {
        if (!angel || angel.healed) return;
        if (isAimingAtAngel) StartHealing(); else handShake.ShakeHand();
    }

    private void OnHealCanceled(InputAction.CallbackContext ctx)
    {
        if (healSpeedMultiplier == 1) CancelHealing();
    }

    private void StartAiming()
    {
        if (!angel.healed)
        {
            minigame.aiHealth = aiHealth;
            tipManager.Show(1);
            crosshair.ShowCrosshair();
            isAimingAtAngel = true;
        }
    }

    public void StopAiming()
    {
        if (isAimingAtAngel && healSpeedMultiplier == 1)
        {
            tipManager.Hide(1);
            crosshair.HideCrosshair();
            if (handRenderer.material != handOGMaterial) handRenderer.material = handOGMaterial;
            isAimingAtAngel = false;
            CancelHealing();
        }
    }

    private void StartHealing()
    {
        healingBeamEmitter.Play();
        if (healingRoutine != null) StopCoroutine(healingRoutine);
        healingRoutine = Healing();
        StartCoroutine(healingRoutine);
    }

    public void CancelHealing()
    {
        if (!isHealing) return;
        isHealing = false;
        handAnimator.SetTrigger("Healing_Beam_Stop");
        helperArrow.ShowArrow();
        StopCoroutine(healingRoutine);
        healingParticlesScript.StopEmitting();
        FadeOutGui();
        minigame.enabled = false;
        healingBeamEmitter.Stop();
    }

    private IEnumerator Healing()
    {
        isHealing = true;
        handAnimator.SetTrigger("Healing_Beam");
        helperArrow.HideArrow();
        tipManager.Hide(1);
        minigame.enabled = true;
        minigame.InitializeMinigame();
        healingParticlesScript.StartEmitting(angel.transform);
        FadeInGui();
        while (aiHealth.hp <= 100)
        {
            aiHealth.hp += Time.deltaTime * 15 * healSpeedMultiplier;
            angelHPSlider.value = aiHealth.hp;
            yield return null;
        }
        Healed();
    }

    public void Healed()
    {
        if (angel.healed) return;
        aiHealth.hp = 100;
        healSpeedMultiplier = 1;
        StopAiming();
        healingParticlesScript.StopEmitting();
        healingBeamEmitter.Stop();
        handRenderer.material = handOGMaterial;
        angel.AfterHealedAction();
        minigame.enabled = false;
        minigameCanvasGroup.DOKill();
        minigameCanvasGroup.alpha = 0;
        FadeOutGui();
        screenFlashTint.Flash(5, 4);
        angel.healed = true;
    }

    private void FadeOutGui()
    {
        minigameCanvasGroup.DOKill();
        minigameCanvasGroup.DOFade(0, guiFadeTimes);
        angelHPCanvasGroup.DOKill();
        angelHPCanvasGroup.alpha = 1;
        angelHPCanvasGroup.DOFade(0, guiFadeTimes * 1.618f).OnComplete(() => { angelHPCanvas.enabled = false; });
    }

    private void FadeInGui()
    {
        if (!angel.healed)
        {
            minigameCanvasGroup.DOKill();
            minigameCanvasGroup.DOFade(1, guiFadeTimes);
            angelHPCanvasGroup.DOKill();
            angelHPCanvasGroup.alpha = 0;
            angelHPCanvas.enabled = true;
            angelHPCanvasGroup.DOFade(1, guiFadeTimes);
        }
    }
}
