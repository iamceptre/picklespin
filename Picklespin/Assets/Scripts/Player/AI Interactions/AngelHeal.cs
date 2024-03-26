using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;

public class AngelHeal : MonoBehaviour
{

    private Ammo ammo;
    private int howMuchAmmoAngelGives = 100;

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
   // Ray ray;
    [SerializeField] private float range = 5f;
    [SerializeField] private bool isAimingAtAngel=false;

    [HideInInspector] public GameObject currentAngel;
    [HideInInspector] public AngelMind angel;
    private AiHealth aiHealth;

    public EventReference healingBeamEvent;
    private FMOD.Studio.EventInstance healingBeamInstance;

    private bool canPlayEvent=true;

    [SerializeField] private ManaLightAnimation manaLightAnimation;

    public FloatUpDown floatUpDown;
    private void Awake()
    {
        ammo = GetComponent<Ammo>();
        handRenderer = hand.GetComponent<MeshRenderer>();
        handOGMaterial = handRenderer.material;
        healEmission = healParticle.emission;
    }

    void Update()
    {
        Vector3 direction = Vector3.forward;
        Ray ray = new Ray(mainCamera.position, mainCamera.TransformDirection(direction * range));


        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.tag == "Angel" && !isAimingAtAngel)
            {
                currentAngel = hit.collider.gameObject;
                StartCoroutine(StartAiming());
            }

        }
        else
        {
            StartCoroutine(StopAiming());
        }

        if (Input.GetKey(KeyCode.Mouse1) && isAimingAtAngel && !angel.healed)
        {
            Healing();
        }
        else //if stop healing mid-through
        {
            HealingParticleStop();
            angelHPSlider.gameObject.SetActive(false);
            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            healingBeamInstance.release();
            floatUpDown.enabled = false;
            canPlayEvent = true; //this should always be at the end of this event
        }

    }


    IEnumerator StartAiming()
    {
        angel = currentAngel.GetComponent<AngelMind>();
        aiHealth = currentAngel.GetComponent<AiHealth>();
        if (!angel.healed)
        {
            handRenderer.material = handHighlightMaterial;
            isAimingAtAngel = true;
        }
        yield return null;
    }

    IEnumerator StopAiming()
    {
        isAimingAtAngel = false;
        if (handRenderer.material != handOGMaterial)
        {
            handRenderer.material = handOGMaterial;
        }
        yield return null;
    }


    public void Healing()
    {
        if (canPlayEvent) //running the event only once
        {
            healingBeamInstance = RuntimeManager.CreateInstance(healingBeamEvent); //create an audio source of beam + load it with healinBeamEvent sound
            healingBeamInstance.start();
            floatUpDown.enabled = true;
            canPlayEvent = false; //this should always be at the end of this event
        }

        aiHealth.hp += Time.deltaTime*25;
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

        if (ammo.maxAmmo - ammo.ammo <= howMuchAmmoAngelGives)
        {
            ammo.ammo = ammo.maxAmmo;
        }
        else
        {
            ammo.ammo += howMuchAmmoAngelGives;
        }
        //ammoDisplay.RefreshManaValue();
        ammoDisplay.RefreshManaValueSmooth();
        HealingParticleStop();
        angelHPSlider.gameObject.SetActive(false);
        healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        healingBeamInstance.release();
        manaLightAnimation.LightAnimation();
        handRenderer.material = handOGMaterial;
        angel.healed = true;
        floatUpDown.enabled = false;
        canPlayEvent = true; //this should always be at the end of this event
        angel.StartCoroutine(angel.AfterHealedAction());
    }

}




//Make the whole script activate only when close to an angel for an optimisation reasons
