using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class AngelHeal : MonoBehaviour
{
    AngelHealingMinigame minigame;
    [HideInInspector] public float healSpeedMultiplier = 1; //ITS BOOSTED WHEN IS 0

    [SerializeField] private GameObject hand;

    private Material handOGMaterial;
    [SerializeField] private Material handHighlightMaterial;
    private MeshRenderer handRenderer;

    [SerializeField] private Slider angelHPSlider;
    [SerializeField] private Canvas angelHPCanvas;

    [SerializeField] private HealingParticles healingParticlesScript;

    [SerializeField] private Transform mainCamera;

    [SerializeField] private float range = 5f;
    [SerializeField] private bool isAimingAtAngel = false;

    [HideInInspector] public AngelMind angel;
    private AiHealth aiHealth;

    [SerializeField] private EventReference healingBeamEvent;
    private FMOD.Studio.EventInstance healingBeamInstance;


    [SerializeField] private ManaLightAnimation manaLightAnimation;

    public FloatUpDown floatUpDown;

    [SerializeField] private UnityEvent showTip;

    [SerializeField] private UnityEvent hideTip;


    [SerializeField] private CanvasGroup angelHPCanvasGroup;
    [SerializeField] private float guiFadeTimes = 0.1f;

    private IEnumerator healingRoutine;


    private void Awake()
    {
        handRenderer = hand.GetComponent<MeshRenderer>();
        handOGMaterial = handRenderer.material;
    }

    private void Start()
    {
        hideTip.Invoke();
        minigame = AngelHealingMinigame.instance;
        minigame.enabled = false;
        healingBeamInstance = RuntimeManager.CreateInstance(healingBeamEvent);
    }

    void Update()
    {
        Ray ray = new Ray(mainCamera.position, mainCamera.TransformDirection(Vector3.forward * range));

        if (Physics.Raycast(ray, out RaycastHit hit, range))
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

        if (Input.GetKeyDown(KeyCode.Mouse1) && isAimingAtAngel && !angel.healed)
        {
            StartHealing();
        }

        if (Input.GetKeyUp(KeyCode.Mouse1) && healSpeedMultiplier == 1)
        {
            CancelHealing();
        }

    }


    private void StartHealing()
    {
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
            //Debug.Log("SetNewAngel" + angel + aiHealth.hp);
            minigame.AngelChanged();
        //}
    }


    public void CancelHealing()
    {
        if (floatUpDown.enabled)
        {
            StopCoroutine(healingRoutine);
            healingParticlesScript.StopEmitting();
            FadeOutGui();
            minigame.enabled = false;
            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            floatUpDown.enabled = false;
            //Debug.Log("CancelHealing");
        }
    }


    private void StartAiming()
    {
        if (!angel.healed)
        {
            //Debug.Log("StartAiming");
            minigame.aiHealth = aiHealth;
            handRenderer.material = handHighlightMaterial;
            showTip.Invoke();
            isAimingAtAngel = true;
        }
    }

    public void StopAiming()
    {
        if (isAimingAtAngel && healSpeedMultiplier == 1)
        {
           // Debug.Log("StopAiming");

            hideTip.Invoke();

            if (handRenderer.material != handOGMaterial)
            {
                handRenderer.material = handOGMaterial;
            }

            CancelHealing();
            isAimingAtAngel = false;
        }

    }



    private IEnumerator Healing()
    {
        //Debug.Log("Healing");
        healingBeamInstance.start();
        hideTip.Invoke();
        floatUpDown.enabled = true;
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
        //Debug.Log("Healed");

        StopCoroutine(healingRoutine);

        healingParticlesScript.StopEmitting();
        healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        handRenderer.material = handOGMaterial;
        floatUpDown.enabled = false;
        angel.AfterHealedAction();
        minigame.enabled = false;
        FadeOutGui();
        angel.healed = true;
    }


    private void FadeOutGui()
    {
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
            angelHPCanvasGroup.DOKill();
            angelHPCanvasGroup.alpha = 0;
            angelHPCanvas.enabled = true;
            angelHPCanvasGroup.DOFade(1, guiFadeTimes);
        }
    }

}