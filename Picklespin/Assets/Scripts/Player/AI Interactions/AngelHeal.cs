using System.Collections;
using UnityEngine;
using FMODUnity;

public class AngelHeal : MonoBehaviour
{

    private Ammo ammo;
    private int howMuchAmmoAngelGives = 100;

    [SerializeField] private GameObject hand;

    private Material handOGMaterial;
    [SerializeField] private Material handHighlightMaterial;
     private MeshRenderer handRenderer;


    [SerializeField] private Transform mainCamera;
    Ray ray;
    [SerializeField] private float range = 5f;
    [SerializeField] private bool isAimingAtAngel=false;

    private Vector3 handStartPos;

    public GameObject currentAngel;
    public AngelMind angel;

    public EventReference healingBeamEvent;
    private FMOD.Studio.EventInstance healingBeamInstance;

    private bool canPlayEvent=true;

    public FloatUpDown floatUpDown;

    private Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.3F;

    private void Start()
    {
        ammo = GetComponent<Ammo>();
        handStartPos = hand.transform.localPosition;
        handRenderer = hand.GetComponent<MeshRenderer>();
        handOGMaterial = handRenderer.material;
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
        else
        {
            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            healingBeamInstance.release();
            floatUpDown.enabled = false;
            canPlayEvent = true; //this should always be at the end of this event
        }

    }


    IEnumerator StartAiming()
    {
        angel = currentAngel.GetComponent<AngelMind>();
        if (!angel.healed)
        {
            handRenderer.material = handHighlightMaterial;
            isAimingAtAngel = true;
            // StartCoroutine(HandTowardsPlayer());
        }
        yield return null;
    }

    IEnumerator HandTowardsPlayer() //not working for now
    {
        //  hand.transform.localPosition = new Vector3(handStartPos.x, handStartPos.y - 0.1f, handStartPos.z);
        if (hand.transform.localPosition != new Vector3(handStartPos.x, handStartPos.y - 0.1f, handStartPos.z))
        {
            hand.transform.localPosition = Vector3.SmoothDamp(hand.transform.localPosition, new Vector3(handStartPos.x, handStartPos.y - 0.1f, handStartPos.z), ref velocity, smoothTime * Time.deltaTime);
            Debug.Log("Przesuwam reke");
            StartCoroutine (HandTowardsPlayer());
        }
        yield return null;
    }

    IEnumerator StopAiming()
    {
        isAimingAtAngel = false;
        hand.transform.localPosition = handStartPos;
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

        angel.hp += Time.deltaTime*16;

        if (angel.hp >= 100) //when angel is healed
        {

            if (ammo.maxAmmo - ammo.ammo <= howMuchAmmoAngelGives)
            {
                ammo.ammo = ammo.maxAmmo;
            }
            else
            {
                ammo.ammo += howMuchAmmoAngelGives;
            }

            healingBeamInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            healingBeamInstance.release();
            handRenderer.material = handOGMaterial;
            angel.healed = true;
            floatUpDown.enabled = false;
            canPlayEvent = true; //this should always be at the end of this event
        }
    }

}




//Make the whole script activate only when close to an angel for an optimisation reasons
