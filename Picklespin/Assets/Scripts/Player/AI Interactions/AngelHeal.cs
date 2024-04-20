using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using UnityEngine.Events;

public class AngelHeal : MonoBehaviour
{

    private Ammo ammo;
    private int howMuchAmmoAngelGives = 100;

    [SerializeField] AngelHealingMinigame minigame;
    [HideInInspector] public float healboost = 1;

    [SerializeField] private GameObject hand;

    private Material handOGMaterial;
    [SerializeField] private Material handHighlightMaterial;
    private MeshRenderer handRenderer;
    public AmmoDisplay ammoDisplay;


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
    

    private void Awake()
    {
        ammo = GetComponent<Ammo>();
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
        Vector3 direction = Vector3.forward;
        Ray ray = new Ray(mainCamera.position, mainCamera.TransformDirection(direction * range));


        if (Physics.Raycast(ray, out RaycastHit hit, range) || healboost == 0)
        {
            if (hit.collider.tag == "Angel" && !isAimingAtAngel && !angel.healed)
            {
                currentAngel = hit.collider.gameObject;
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
            HealingParticleStop();
            angelHPSlider.gameObject.SetActive(false);
            minigame.enabled = false;
            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            healingBeamInstance.release();
            floatUpDown.enabled = false;
            canPlayEvent = true; //this should always be at the end of this event
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
        if (canPlayEvent) //running the event only once
        {
            healingBeamInstance = RuntimeManager.CreateInstance(healingBeamEvent); //create an audio source of beam + load it with healinBeamEvent sound
            healingBeamInstance.start();
            hideTip.Invoke();
            floatUpDown.enabled = true;
            minigame.enabled = true;
            minigame.InitializeMinigame();
            canPlayEvent = false; //this should always be at the end of this event
        }

        aiHealth.hp += Time.deltaTime * 15 * healboost;
        angelHPSlider.gameObject.SetActive(true);
        angelHPSlider.value = aiHealth.hp;
        HealingParticleStart();

        if (aiHealth.hp >= 100)
        {
            Healed();
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
           // healParticle.Stop();
        }
    }


    private void Healed()
    {
        GiveMana();
        HealingParticleStop();
        angelHPSlider.gameObject.SetActive(false);
        minigame.enabled = false;
        healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        healingBeamInstance.release();
        handRenderer.material = handOGMaterial;
        angel.healed = true;
        floatUpDown.enabled = false;
        angel.StartCoroutine(angel.AfterHealedAction());
        canPlayEvent = true; //this should always be at the end of this event
    }

    private void GiveMana()
    {
        if (ammo.maxAmmo - ammo.ammo <= howMuchAmmoAngelGives)
        {
            ammo.ammo = ammo.maxAmmo;
        }
        else
        {
            ammo.ammo += howMuchAmmoAngelGives;
        }

        ammoDisplay.RefreshManaValueSmooth();
        manaLightAnimation.LightAnimation(howMuchAmmoAngelGives);
    }

}




//Make the whole script activate only when close to an angel for an optimisation reasons
