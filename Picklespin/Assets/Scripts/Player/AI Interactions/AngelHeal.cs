using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class AngelHeal : MonoBehaviour
{

    [SerializeField] AngelHealingMinigame minigame;
    [HideInInspector] public float healboost = 1;

    [SerializeField] private GameObject hand;

    private Material handOGMaterial;
    [SerializeField] private Material handHighlightMaterial;
    private MeshRenderer handRenderer;

    [SerializeField] private ParticleSystem healParticle;
    private ParticleSystem.EmissionModule healEmission;

    [SerializeField] private GameObject healParticleStart;

    [SerializeField] private Slider angelHPSlider;


    [SerializeField] private Transform mainCamera;
   
    [SerializeField] private float range = 5f;
    [SerializeField] private bool isAimingAtAngel=false;

    [HideInInspector] public GameObject currentAngel;
    [HideInInspector] public AngelMind angel;
    private AiHealth aiHealth;

    [SerializeField] private EventReference healingBeamEvent;
    private FMOD.Studio.EventInstance healingBeamInstance;

    private bool canPlayEvent=true;

    [SerializeField] private ManaLightAnimation manaLightAnimation;

    public FloatUpDown floatUpDown;

    [SerializeField] private UnityEvent showTip;
    
    [SerializeField] private UnityEvent hideTip;


    [SerializeField] private CanvasGroup angelHPCanvasGroup;
    [SerializeField] private float guiFadeTimes = 0.1f;


    private void Awake()
    {
        handRenderer = hand.GetComponent<MeshRenderer>();
        handOGMaterial = handRenderer.material;
        healEmission = healParticle.emission;
        minigame.enabled = false;
    }

    private void Start()
    {
        hideTip.Invoke();
    }

    void Update()
    {
        if (!minigame.boosted) {

            Ray ray = new Ray(mainCamera.position, mainCamera.TransformDirection(Vector3.forward * range));

            if (Physics.Raycast(ray, out RaycastHit hit, range) || healboost == 0)
            {
                if (hit.collider.tag == "Angel" && !isAimingAtAngel && !angel.healed)
                {
                    StopAllCoroutines();
                    StartCoroutine(StartAiming());
                }

            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(StopAiming());
            }

            if (Input.GetKey(KeyCode.Mouse1) && isAimingAtAngel && !angel.healed || healboost == 0)
            {
                Healing();
            }
            else //if stop healing mid-through
            {
                CancelHealing();
            }
        }
        else
        {
            Healing();
        }

    }


    private void CancelHealing()
    {
        if (!canPlayEvent) {
            HealingParticleStop();
            FadeOutGui();
            minigame.enabled = false;
            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            healingBeamInstance.release();
            floatUpDown.enabled = false;
            canPlayEvent = true;
        }
    }


    IEnumerator StartAiming()
    {
        //Debug.Log("this should be fired once");
        //tested, fires only once

        angel = currentAngel.GetComponent<AngelMind>();
        aiHealth = currentAngel.GetComponent<AiHealth>();
        minigame.aiHealth = aiHealth;

            handRenderer.material = handHighlightMaterial;
            showTip.Invoke();
            isAimingAtAngel = true;
        
        yield return null;
    }

    IEnumerator StopAiming()
    {
        if (isAimingAtAngel) {
            //Debug.Log("this should be fired once");
            //tested, fires only once

            hideTip.Invoke();
            if (handRenderer.material != handOGMaterial)
            {
                handRenderer.material = handOGMaterial;
            }
            isAimingAtAngel = false;
            yield return null;
        }
    }


    public void Healing()
    {
        if (!angel.healed)
        {
            if (canPlayEvent) //running the event only once
            {
                canPlayEvent = false;
                healingBeamInstance = RuntimeManager.CreateInstance(healingBeamEvent); //create an audio source of beam + load it with healinBeamEvent sound
                healingBeamInstance.start();
                hideTip.Invoke();
                floatUpDown.enabled = true;
                minigame.enabled = true;
                minigame.InitializeMinigame();
                FadeInGui();
            }

            aiHealth.hp += Time.deltaTime * 15 * healboost;
            angelHPSlider.value = aiHealth.hp;
            HealingParticleStart();

            if (aiHealth.hp >= 100)
            {
                Healed();
            }
        }

    }

    private void HealingParticleStart()
    {
        healParticle.transform.LookAt(currentAngel.transform.position);
        if (!healParticle.isEmitting)
        {
            healParticleStart.SetActive(true);
            healParticle.Play();
            healEmission.enabled = true;
        }
    }

    private void HealingParticleStop()
    {
        if (healParticle.isEmitting)
        {
            healEmission.enabled = false;
        }
    }


    private void Healed()
    {
        if (!canPlayEvent)
        {
        HealingParticleStop();
        healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        healingBeamInstance.release();
        handRenderer.material = handOGMaterial;
        floatUpDown.enabled = false;
        angel.AfterHealedAction();
        minigame.enabled = false;
        FadeOutGui();
        canPlayEvent = true;
        angel.healed = true;
        }
    }


    private void FadeOutGui()
    {
        angelHPCanvasGroup.DOKill();
        angelHPCanvasGroup.alpha = 1;
        angelHPCanvasGroup.DOFade(0, guiFadeTimes*1.618f).OnComplete(() =>
        {
            angelHPSlider.gameObject.SetActive(false);
        });
    }

    private void FadeInGui()
    {
        if (!angel.healed) {
            angelHPCanvasGroup.DOKill();
            angelHPCanvasGroup.alpha = 0;
            angelHPSlider.gameObject.SetActive(true);
            angelHPCanvasGroup.DOFade(1, guiFadeTimes);
        }
    }

}