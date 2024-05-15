using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class EvilEntityDeath : MonoBehaviour
{
    ManaSuckParticlesSpawner manaSuckParticlesSpawner;
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    private Dissolver dissolver;

    private int howMuchManaIGiveAfterDying = 10;
    private Ammo ammo;

    private void Awake()
    {
        if (aiHealthUiBar == null)
        {
            aiHealthUiBar = gameObject.GetComponent<AiHealthUiBar>();
        }
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
            //Need to shut down ai here
            dissolver = gameObject.GetComponent<Dissolver>();
            dissolver.StartDissolve();
        }
    }


}
