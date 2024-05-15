using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using System.Collections;

public class EvilEntityDeath : MonoBehaviour
{
    ManaSuckParticlesSpawner manaSuckParticlesSpawner;
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    [SerializeField] private Material deadMaterial;
    private float dissolveProgress; //1 is visible, 0 is not
    private Renderer myRenderer;
    private BoxCollider myCollider;
    private StateManager myStateManager;


    private int howMuchManaIGiveAfterDying = 10;
    private Ammo ammo;

    private void Awake()
    {
        if (aiHealthUiBar == null)
        {
            aiHealthUiBar = gameObject.GetComponent<AiHealthUiBar>();
        }

        myRenderer = gameObject.GetComponentInChildren<Renderer>();
        myCollider = gameObject.GetComponent<BoxCollider>();
        myStateManager = gameObject.GetComponent<StateManager>();
    }

    private void Start()
    {
        manaSuckParticlesSpawner = ManaSuckParticlesSpawner.instance;
        ammo = Ammo.instance;
    }

    public void Die()
    {
        aiHealthUiBar.Detach();
        aiHealthUiBar.FadeOut();

        ammo.GiveManaToPlayer(howMuchManaIGiveAfterDying);
        manaSuckParticlesSpawner.Spawn(transform.position, howMuchManaIGiveAfterDying);

        deathEvent.Invoke(); //custom death behaviour

        CheckAndDisableFire(); //if ai is burning then its dying from burninig, if theres no fire, just dies
    }


    private void CheckAndDisableFire()
    {
        var setOnFireScirpt = gameObject.GetComponent<SetOnFire>();

        if (setOnFireScirpt != null)
        {
            //if ai is dying from fire, not spellhit
            setOnFireScirpt.PanicKill();
        }
        else
        {
            evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
            RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
            evilEntityDeathSoundReference.start();
            DissolvePurple();
        }
    }


    private void DissolvePurple()
    {
        //neds to disable ai here
        myCollider.enabled = false;
        myStateManager.KillMyBrain();

        myRenderer.material = deadMaterial;
        dissolveProgress = 0.7f;
        StartCoroutine(Dissolver());
    }



    private IEnumerator Dissolver()
    {
        while (true)
        {
            dissolveProgress -= Time.deltaTime * 0.7f;
            myRenderer.material.SetFloat("_Progress", dissolveProgress);

            if (dissolveProgress<=0)
            {
                StopAllCoroutines();
                Destroy(gameObject);
            }

            yield return null;
        }
    }

}
