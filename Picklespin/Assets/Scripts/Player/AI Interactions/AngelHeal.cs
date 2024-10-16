using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class AngelHeal : MonoBehaviour
{
    [SerializeField] private HandShakeWhenCannotHeal handShake;
    private Helper_Arrow helperArrow;
    private ScreenFlashTint screenFlashTint;
    AngelHealingMinigame minigame;
    [HideInInspector] public float healSpeedMultiplier = 1; //ITS BOOSTED WHEN IS 0

    [SerializeField] private GameObject hand;

    private Material handOGMaterial;
    [SerializeField] private Material handHighlightMaterial;
    private SkinnedMeshRenderer handRenderer;
    [SerializeField] private Animator handAnimator;

    [SerializeField] private Slider angelHPSlider;
    [SerializeField] private Canvas angelHPCanvas;

    [SerializeField] private HealingParticles healingParticlesScript;

    [SerializeField] private Transform mainCamera;

    [SerializeField] private float range = 5f;
    [SerializeField] private bool isAimingAtAngel = false;

    [HideInInspector] public AngelMind angel;
    private AiHealth aiHealth;

    [SerializeField] private StudioEventEmitter healingBeamEmitter;


    [SerializeField] private ManaLightAnimation manaLightAnimation;

    [SerializeField] private LayerMask layersForRaycast;


    private TipManager tipManager;


    [SerializeField] private CanvasGroup angelHPCanvasGroup;
    [SerializeField] private CanvasGroup minigameCanvasGroup;
    [SerializeField] private float guiFadeTimes = 0.1f;

    private IEnumerator healingRoutine;

    private bool isHealing = false;


    private void Awake()
    {
        handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
        handOGMaterial = handRenderer.material;
    }

    private void Start()
    {
        tipManager = TipManager.instance;
        helperArrow = Helper_Arrow.instance;
        tipManager.Hide(1);
        minigame = AngelHealingMinigame.instance;
        screenFlashTint = ScreenFlashTint.instance;
        minigame.enabled = false;
    }

    void Update()
    {
        Ray ray = new Ray(mainCamera.position, mainCamera.TransformDirection(Vector3.forward * range));

        if (Physics.Raycast(ray, out RaycastHit hit, range, layersForRaycast))
        {
            if (hit.collider.CompareTag("Angel"))
            {
                if (!isAimingAtAngel)
                {
                    SetNewAngel(hit);
                    StartAiming();
                }
            }
        }
        else
        {
            StopAiming();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && !angel.healed && angel != null)
        {
            if (isAimingAtAngel)
            {
                StartHealing();
            }
            else
            {
                handShake.ShakeHand();
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse1) && healSpeedMultiplier == 1)
        {
            CancelHealing();
        }

    }


    private void StartHealing()
    {
        healingBeamEmitter.Play();
        if (healingRoutine != null)
        {
            StopCoroutine(healingRoutine);
        }

        healingRoutine = Healing();
        StartCoroutine(healingRoutine);
    }

    private void SetNewAngel(RaycastHit hit)
    {
        //if (angel != hit.transform.GetComponent<AngelMind>())
        //{
            angel = hit.transform.GetComponent<AngelMind>();
            aiHealth = hit.transform.GetComponent<AiHealth>();
            minigame.AngelChanged();
        //}
    }


    public void CancelHealing()
    {
        if (isHealing)
        {
            isHealing = false;
            handAnimator.SetTrigger("Healing_Beam_Stop");
            helperArrow.ShowArrow();
            StopCoroutine(healingRoutine);
            healingParticlesScript.StopEmitting();
            FadeOutGui();
            minigame.enabled = false;
            healingBeamEmitter.Stop();
            //Debug.Log("CancelHealing");
        }
    }


    private void StartAiming()
    {
        if (!angel.healed)
        {
            //Debug.Log("StartAiming");
            minigame.aiHealth = aiHealth;
            //handRenderer.material = handHighlightMaterial;
            tipManager.Show(1);
            isAimingAtAngel = true;
        }
    }

    public void StopAiming()
    {
        if (isAimingAtAngel && healSpeedMultiplier == 1)
        {
            // Debug.Log("StopAiming");

            tipManager.Hide(1);

            if (handRenderer.material != handOGMaterial)
            {
                handRenderer.material = handOGMaterial;
            }

            isAimingAtAngel = false;

            CancelHealing();
        }

    }



    private IEnumerator Healing()
    {
        //Debug.Log("Healing");
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
        yield break;
    }


    public void Healed()
    {
        if (angel.healed)
        {
            return;
        }

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
        angelHPCanvasGroup.DOFade(0, guiFadeTimes * 1.618f).OnComplete(() =>
        {
            angelHPCanvas.enabled = false;
        });
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